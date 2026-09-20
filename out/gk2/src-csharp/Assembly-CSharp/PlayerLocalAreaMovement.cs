using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class PlayerLocalAreaMovement : MonoBehaviour
{
	public enum MovementStatus
	{
		NotMoving,
		CalcPath,
		MovingToTarget
	}

	private const float PLAYER_AI_MOVEMENT_SPEED = 3.3f;

	private MovementType movementType;

	private Vector3 endPosition;

	private Vector3 startPosition;

	private List<Vector3> currentPath;

	private int currentWaypointIndex;

	private Seeker seeker;

	private PlayerPhysicalBody playerPhysics;

	private MovementStatus movementStatus;

	private Direction arrivalFacingDirection;

	private int pathRequestVersion;

	public MovementType MovementType => movementType;

	public MovementStatus Status => movementStatus;

	public Seeker Seeker => seeker;

	public bool IsMovementStarted
	{
		get
		{
			if (movementStatus != MovementStatus.CalcPath)
			{
				return movementStatus == MovementStatus.MovingToTarget;
			}
			return true;
		}
	}

	private void Awake()
	{
		seeker = GetComponent<Seeker>();
		seeker.graphMask = new GraphMask(8u);
	}

	public void Init(PlayerPhysicalBody playerPhysics)
	{
		this.playerPhysics = playerPhysics;
	}

	public void StartMovement(Vector3 endPosition, Direction arrivalFacingDirection = Direction.None)
	{
		int requestVersion = ++pathRequestVersion;
		this.arrivalFacingDirection = arrivalFacingDirection;
		seeker.graphMask = new GraphMask(8u);
		movementStatus = MovementStatus.CalcPath;
		startPosition = base.transform.position;
		this.endPosition = endPosition;
		RescanPlayerGraph();
		if (!IsReachable(endPosition))
		{
			Debug.LogError("Can not calculate path");
			StopMovement(force: true);
		}
		else
		{
			seeker.StartPath(startPosition, endPosition, delegate(Path path)
			{
				OnPathFound(path, requestVersion);
			});
		}
	}

	public void StopMovement(bool force = false)
	{
		if (movementType == MovementType.Recast || force)
		{
			pathRequestVersion++;
			movementStatus = MovementStatus.NotMoving;
			playerPhysics.SetNonKinematicFlag(PlayerDynamicType.ByMovementComponent, isDynamic: true);
			currentPath = null;
			currentWaypointIndex = 0;
			movementType = MovementType.None;
			arrivalFacingDirection = Direction.None;
			playerPhysics.PlayerView.PlayerAnimation.SetState(AnimationState.Idle);
		}
	}

	public void CustomUpdate(float delta)
	{
		if (currentPath == null)
		{
			return;
		}
		Vector3 vector = playerPhysics.transform.position;
		float num = delta * 3.3f;
		Vector3 normalized;
		while (true)
		{
			if (currentWaypointIndex >= currentPath.Count)
			{
				ApplyArrivalFacing();
				StopMovement();
				return;
			}
			Vector3 vector2 = currentPath[currentWaypointIndex];
			normalized = (vector2 - vector).normalized;
			float magnitude = (vector - vector2).magnitude;
			if (!(num > magnitude))
			{
				break;
			}
			num -= magnitude;
			vector = vector2;
			currentWaypointIndex++;
			if (currentWaypointIndex == currentPath.Count)
			{
				playerPhysics.SetPosition(vector);
			}
		}
		vector += num * normalized;
		playerPhysics.MoveByPosition(vector, normalized.XZ2());
	}

	public bool IsReachable(Vector3 position)
	{
		GridGraph obj = (GridGraph)AstarPath.active.data.graphs[3];
		NNInfo nearest = obj.GetNearest(base.transform.position, NNConstraint.Walkable);
		NNInfo nearest2 = obj.GetNearest(position);
		if (PlayerColliderTester.IsPositionReachable(nearest2.position))
		{
			if (!PathUtilities.IsPathPossible(nearest.node, nearest2.node))
			{
				return (base.transform.position - position).sqrMagnitude < 0.01f;
			}
			return true;
		}
		return false;
	}

	private void ApplyArrivalFacing()
	{
		if (arrivalFacingDirection != 0)
		{
			playerPhysics.SetFacingDirection(arrivalFacingDirection.ConvertToVector2XZ());
		}
	}

	private void OnPathFound(Path p, int requestVersion)
	{
		if (requestVersion == pathRequestVersion && movementStatus == MovementStatus.CalcPath)
		{
			if (p.CompleteState != PathCompleteState.Complete || p.path.Count == 0)
			{
				Debug.LogError("Can not calculate path");
				StopMovement(force: true);
				return;
			}
			movementStatus = MovementStatus.MovingToTarget;
			playerPhysics.SetNonKinematicFlag(PlayerDynamicType.ByMovementComponent, isDynamic: false);
			movementType = MovementType.Recast;
			currentPath = p.vectorPath;
			List<Vector3> list = currentPath;
			list[list.Count - 1] = endPosition;
			currentWaypointIndex = 0;
		}
	}

	public void RescanPlayerGraph()
	{
		GridGraph obj = (GridGraph)AstarPath.active.data.graphs[3];
		int num = Mathf.RoundToInt(44f);
		obj.center = VisualConsts.GetRoundedPosXYZ(base.transform.position, 2, 10);
		obj.collision.diameter = 2.8f;
		obj.SetDimensions(num, num, 0.1f);
		obj.Scan();
	}
}
