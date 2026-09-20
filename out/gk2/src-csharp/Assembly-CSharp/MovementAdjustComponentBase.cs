using Pathfinding;
using UnityEngine;

public class MovementAdjustComponentBase : MonoBehaviour
{
	private const float HIT_POINT_TO_CAST = 1f;

	private const float HIT_MAX_DISTANCE = 10f;

	public virtual void Init(IMovable movable)
	{
	}

	public virtual void DeInit()
	{
	}

	public virtual void SetAdjustmentActive(bool active)
	{
	}

	public void UpdatePos(Vector3 newPos)
	{
		RecastGraph sceneRecastGraph = MainGame.PlayerController.SceneRecastGraph;
		if (sceneRecastGraph != null)
		{
			NearestNodeConstraint walkable = NearestNodeConstraint.Walkable;
			walkable.distanceMetric = DistanceMetric.ClosestAsSeenFromAbove();
			NNInfo nearest = sceneRecastGraph.GetNearest(newPos, walkable);
			if ((nearest.position.XZ() - newPos.XZ()).magnitude < 0.01f)
			{
				base.transform.position = new Vector3(newPos.x, nearest.position.y, newPos.z);
				return;
			}
		}
		base.transform.position = RaycastUtils.TrySnapToTheGround(newPos, 1f, 10f);
	}

	protected virtual void UpdatePosIfMoving(Vector3 newPos)
	{
	}
}
