using System;
using Pathfinding;
using UnityEngine;

public class RichAI_Custom : RichAI
{
	[Header("Custom Stuck Detection")]
	[Tooltip("Time in seconds after which the agent is considered stuck if it hasn't reached the next steering target.")]
	public float stuckThreshold = 2f;

	[Tooltip("Cooldown after a forced repath to avoid frequent recalculations.")]
	public float repathCooldownDuration = 1f;

	[Tooltip("Max elevation change (meters) allowed when snapping to the seeker recast graph. Larger jumps are treated as a different floor (e.g. cobblestones under a bridge) so the agent stays on the current floor instead of falling through navmesh holes.")]
	public float maxVerticalSnapDelta = 1.25f;

	private float snapQueryElevation;

	private Func<GraphNode, bool> currentFloorNodeFilter;

	public bool GroundSnapEnabled { get; set; } = true;


	public bool RequiredRepath { get; private set; }

	public bool IsMovementPaused { get; private set; }

	public void SetMovementPaused(bool paused)
	{
		IsMovementPaused = paused;
	}

	protected override void OnUpdate(float dt)
	{
		if (!IsMovementPaused)
		{
			base.OnUpdate(dt);
		}
	}

	public override void SearchPath()
	{
		base.SearchPath();
		RequiredRepath = false;
	}

	protected override Vector3 ClampToNavmesh(Vector3 position, out bool positionChanged)
	{
		if (!GroundSnapEnabled)
		{
			return base.ClampToNavmesh(position, out positionChanged);
		}
		position = base.ClampToNavmesh(position, out positionChanged);
		float y = position.y;
		NNInfo nearestOnCurrentFloor = GetNearestOnCurrentFloor(position);
		if (nearestOnCurrentFloor.node == null)
		{
			return position;
		}
		Vector2 vector = movementPlane.ToPlane(nearestOnCurrentFloor.position - position);
		float sqrMagnitude = vector.sqrMagnitude;
		if (sqrMagnitude > 1.0000001E-06f)
		{
			velocity2D -= vector * Vector2.Dot(vector, velocity2D) / sqrMagnitude;
			position += movementPlane.ToWorld(vector);
			positionChanged = true;
		}
		float a = (position.y = nearestOnCurrentFloor.position.y);
		if (!positionChanged)
		{
			positionChanged = !a.EqualsTo(y, 0.001f);
		}
		return position;
	}

	protected override Vector3 ClampPositionToGraph(Vector3 newPosition)
	{
		return SnapToSeekerGraph(newPosition);
	}

	public Vector3 SnapToSeekerGraph(Vector3 worldPos)
	{
		if (!GroundSnapEnabled)
		{
			return worldPos;
		}
		NNInfo nearestOnCurrentFloor = GetNearestOnCurrentFloor(worldPos);
		if (nearestOnCurrentFloor.node == null)
		{
			return worldPos;
		}
		movementPlane.ToPlane(worldPos, out var elevation);
		return movementPlane.ToWorld(movementPlane.ToPlane(nearestOnCurrentFloor.position), elevation);
	}

	private NNInfo GetNearestOnCurrentFloor(Vector3 worldPos)
	{
		AstarPath active = AstarPath.active;
		if ((UnityEngine.Object)(object)active == null)
		{
			return NNInfo.Empty;
		}
		movementPlane.ToPlane(worldPos, out snapQueryElevation);
		if (currentFloorNodeFilter == null)
		{
			currentFloorNodeFilter = IsNodeOnCurrentFloor;
		}
		NearestNodeConstraint walkable = NearestNodeConstraint.Walkable;
		if ((bool)(UnityEngine.Object)(object)seeker)
		{
			walkable.graphMask = seeker.graphMask;
			walkable.tags = seeker.traversableTags;
		}
		walkable.distanceMetric = DistanceMetric.ClosestAsSeenFromAbove();
		walkable.filter = currentFloorNodeFilter;
		NNInfo nearest = active.GetNearest(worldPos, walkable);
		if (nearest.node == null)
		{
			return NNInfo.Empty;
		}
		movementPlane.ToPlane(nearest.position, out var elevation);
		if (Mathf.Abs(elevation - snapQueryElevation) > maxVerticalSnapDelta)
		{
			return NNInfo.Empty;
		}
		return nearest;
	}

	private bool IsNodeOnCurrentFloor(GraphNode node)
	{
		movementPlane.ToPlane((Vector3)node.position, out var elevation);
		return Mathf.Abs(elevation - snapQueryElevation) <= maxVerticalSnapDelta;
	}

	public Vector3 ApplySeekerGraphSnap(Vector3 worldPos)
	{
		worldPos = SnapToSeekerGraph(worldPos);
		prevPosition2 = prevPosition1;
		prevPosition1 = simulatedPosition;
		simulatedPosition = worldPos;
		return worldPos;
	}

	protected override Vector3 UpdateTarget(RichFunnel fn)
	{
		nextCorners.Clear();
		bool requiresRepath;
		Vector3 result = fn.Update(simulatedPosition, nextCorners, 2, out lastCorner, out requiresRepath);
		if (!RequiredRepath && requiresRepath && !waitingForPathCalculation)
		{
			RequiredRepath = true;
		}
		return result;
	}
}
