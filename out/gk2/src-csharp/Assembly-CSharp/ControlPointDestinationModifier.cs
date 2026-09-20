using Pathfinding;
using UnityEngine;

public class ControlPointDestinationModifier : GoToDestinationModifier
{
	private const float DISTANCE_TO_STOP_AS_STUCKED = 0.05f;

	private FightingCapturePoint targetControlPoint;

	private Vector3 targetPosition;

	public override bool IsStucked => (targetPosition - base.Wgo.Data.Position).XZ().magnitude < 0.05f;

	public override bool ShouldAnchorOnArrival => true;

	public ControlPointDestinationModifier(FightingCapturePoint capturePoint)
	{
		targetControlPoint = capturePoint;
	}

	public override Vector3 GetCurrentTargetPosition()
	{
		return targetPosition;
	}

	public override bool TryGetTargetPositionForPathfinding(out ICombatEntity combatEntity, out Vector3 position)
	{
		combatEntity = null;
		if (targetControlPoint.TryReserveSlot(base.Wgo.Data.UniqueId, out var position2, out var _))
		{
			position = position2;
		}
		else
		{
			position = GetAvailablePositionFromRecast();
		}
		targetPosition = position;
		return true;
	}

	public override void OnFinish()
	{
		if ((bool)targetControlPoint)
		{
			targetControlPoint.ReleaseSlot(base.Wgo.Data.UniqueId);
		}
		base.OnFinish();
	}

	private Vector3 GetAvailablePositionFromRecast()
	{
		Vector3 position = targetControlPoint.transform.position;
		float radius = targetControlPoint.Radius;
		RecastGraph recastGraph = AstarPath.active.graphs[12] as RecastGraph;
		NearestNodeConstraint walkable = NearestNodeConstraint.Walkable;
		walkable.graphMask = GraphMask.FromGraph(recastGraph);
		for (int i = 0; i < 24; i++)
		{
			Vector2 vector = Random.insideUnitCircle * radius;
			Vector3 position2 = new Vector3(position.x + vector.x, position.y, position.z + vector.y);
			GraphNode graphNode = recastGraph.PointOnNavmesh(position2, walkable);
			if (graphNode != null)
			{
				Vector3 result = (Vector3)graphNode.position;
				float num = result.x - position.x;
				float num2 = result.z - position.z;
				if (num * num + num2 * num2 <= radius * radius)
				{
					return result;
				}
			}
		}
		return AstarPath.active.GetNearest(position, walkable).position;
	}

	public override GoToDestinationModifierDebugInfo GetDebugInfo()
	{
		GoToDestinationModifierDebugInfo debugInfo = base.GetDebugInfo();
		debugInfo.CapturePointName = (targetControlPoint ? targetControlPoint.name : null);
		debugInfo.ReservedSlotPosition = targetPosition;
		return debugInfo;
	}
}
