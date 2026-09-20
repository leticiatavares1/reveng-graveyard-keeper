using System;
using LazyBearTechnology;
using Pathfinding;
using UnityEngine;

public class CombatEntityDestinationModifier : GoToDestinationModifier
{
	private DockPointData targetDockPoint;

	private WgoData targetObj;

	private DockPointData customDockPoint;

	private bool wasDockPointValid;

	public override bool IsValid
	{
		get
		{
			if (wasDockPointValid && targetDockPoint == null)
			{
				return targetObj == null;
			}
			return true;
		}
	}

	public override bool ShouldAnchorOnArrival => targetDockPoint != null;

	public WgoData TargeObj => targetObj;

	public DockPointData TargetDockPoint => targetDockPoint;

	public CombatEntityDestinationModifier(DockPointData dockPointData = null)
	{
		customDockPoint = dockPointData;
		wasDockPointValid = customDockPoint != null;
	}

	public override void Init(FightingAgent agent)
	{
		base.Init(agent);
		UpdateTargetData();
	}

	private void UpdateTargetData()
	{
		if (base.Agent.MobCommand.TargetEntity != null)
		{
			targetObj = MainGame.WorldData.GetWgoData(base.Agent.MobCommand.TargetEntity.CombatEntityUID);
			if (customDockPoint != null)
			{
				targetDockPoint = customDockPoint;
				return;
			}
			RecastGraph recastGraph = LazySingleton<FightingGameController>.Instance?.RecastGraph;
			targetDockPoint = targetObj?.MainWgoPartData.GetNearestDockPoint(targetObj, base.Wgo.Data.Position, DockPointData.Availability.OnlyNotOccupied, DockPointData.Filter.All, (recastGraph == null) ? null : ((Func<DockPointData, Vector3, bool>)((DockPointData data, Vector3 parentPos) => data.IsOnRecast(parentPos, recastGraph))));
		}
		else
		{
			targetObj = null;
			targetDockPoint = null;
		}
	}

	public override bool TryGetTargetPositionForPathfinding(out ICombatEntity combatEntity, out Vector3 position)
	{
		combatEntity = null;
		if (targetDockPoint != null && targetObj != null && base.Agent.MobCommand.TargetEntity != null)
		{
			position = targetObj.GetDockPointDataWorldPosition(targetDockPoint);
			return true;
		}
		if (base.Agent.MobCommand.TargetEntity != null)
		{
			position = base.Agent.MobCommand.TargetEntity.CombatEntityPosition;
			combatEntity = base.Agent.MobCommand.TargetEntity;
			return true;
		}
		position = base.Agent.MobCommand.Position;
		return true;
	}

	public override Vector3 GetCurrentTargetPosition()
	{
		if (targetDockPoint != null && targetObj != null)
		{
			return targetObj.GetDockPointDataWorldPosition(targetDockPoint);
		}
		return base.Agent.MobCommand.Position;
	}

	public override void OnReachedDestination()
	{
		if (base.Wgo.TeamType != 0)
		{
			base.Agent.MobCommand.TargetEntity?.OnOtherCombatTargetReachedToMe(base.Wgo);
		}
	}

	public override GoToDestinationModifierDebugInfo GetDebugInfo()
	{
		GoToDestinationModifierDebugInfo debugInfo = base.GetDebugInfo();
		debugInfo.TargetWgoId = targetObj?.id;
		debugInfo.TargetUniqueId = ((targetObj != null) ? targetObj.UniqueId.ToString() : null);
		debugInfo.TargetWgo = GoToDestinationModifierDebugInfo.ResolveTargetWgoView(base.Agent?.MobCommand?.TargetEntity, targetObj);
		debugInfo.ActiveDockPoint = targetDockPoint;
		debugInfo.CustomDockPoint = customDockPoint;
		debugInfo.ActiveDockPointView = GoToDestinationModifierDebugInfo.ResolveDockPointView(debugInfo.TargetWgo, targetDockPoint);
		debugInfo.CustomDockPointView = GoToDestinationModifierDebugInfo.ResolveDockPointView(debugInfo.TargetWgo, customDockPoint);
		debugInfo.WasDockPointValid = wasDockPointValid;
		return debugInfo;
	}
}
