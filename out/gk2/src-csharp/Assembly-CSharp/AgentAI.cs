using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LazyBearTechnology;
using Pathfinding;
using UnityEngine;

[Serializable]
public abstract class AgentAI : ScriptableObject
{
	private static class TargetSelectionBuffers
	{
		public static readonly List<Vector3> LinecastDestinations = new List<Vector3>();

		public static readonly List<bool> LinecastVisibility = new List<bool>();

		public static readonly List<Vector3> SingleLinecastDestinations = new List<Vector3>(1);

		public static readonly List<bool> SingleLinecastVisibility = new List<bool>(1);

		public static readonly List<ICombatEntity> Targets = new List<ICombatEntity>();

		public static readonly List<float> TargetDistances = new List<float>();

		public static readonly List<int> TargetPriorities = new List<int>();
	}

	protected const float Y_LINECAST_OFFSET = 0.5f;

	protected const float DOCKING_DISTANCE = 0.06666668f;

	private const float OVERFLOW_DOCK_RING_PADDING_FALLBACK = 0.4f;

	public float aggroDistance = 10f;

	public float deAggroDistance = 15f;

	public FightingGameController FightingGameController => LazySingleton<FightingGameController>.Instance;

	public abstract MobCommand GetCommand(FightingAgent agent, Func<IEnumerable<ICombatEntity>> targets);

	public static bool TryLineCastByRecast(Vector3 start, Vector3 end)
	{
		TargetSelectionBuffers.SingleLinecastDestinations.Clear();
		TargetSelectionBuffers.SingleLinecastDestinations.Add(end);
		TargetSelectionBuffers.SingleLinecastVisibility.Clear();
		SpecialPhysicsCastUtils.GetLinecastVisibility(start, TargetSelectionBuffers.SingleLinecastDestinations, 256, TargetSelectionBuffers.SingleLinecastVisibility);
		if (TargetSelectionBuffers.SingleLinecastVisibility.Count > 0)
		{
			return TargetSelectionBuffers.SingleLinecastVisibility[0];
		}
		return false;
	}

	[CanBeNull]
	protected static ICombatEntity GetClosestTarget(Vector3 position, Func<IEnumerable<ICombatEntity>> targets, LazyConsts.Fighting.TeamType teamType, float maxDistance = float.PositiveInfinity, bool targetMustBeDirectlyVisible = false)
	{
		if (targets == null)
		{
			return null;
		}
		return GetClosestTarget(position, targets(), teamType, maxDistance, targetMustBeDirectlyVisible);
	}

	[CanBeNull]
	protected static ICombatEntity GetClosestTarget(Vector3 position, IEnumerable<ICombatEntity> targets, LazyConsts.Fighting.TeamType teamType, float maxDistance = float.PositiveInfinity, bool targetMustBeDirectlyVisible = false)
	{
		TargetSelectionBuffers.Targets.Clear();
		TargetSelectionBuffers.TargetDistances.Clear();
		TargetSelectionBuffers.TargetPriorities.Clear();
		foreach (ICombatEntity target in targets)
		{
			if (target.CombatEntityHpComponent.Hp != 0)
			{
				float combatEntityDistance = target.GetCombatEntityDistance(position, teamType);
				if (combatEntityDistance < maxDistance)
				{
					TargetSelectionBuffers.Targets.Add(target);
					TargetSelectionBuffers.TargetDistances.Add(combatEntityDistance);
					TargetSelectionBuffers.TargetPriorities.Add(target.AttackPriority);
				}
			}
		}
		if (TargetSelectionBuffers.Targets.Count == 0)
		{
			return null;
		}
		if (targetMustBeDirectlyVisible)
		{
			TargetSelectionBuffers.LinecastDestinations.Clear();
			for (int i = 0; i < TargetSelectionBuffers.Targets.Count; i++)
			{
				TargetSelectionBuffers.LinecastDestinations.Add(TargetSelectionBuffers.Targets[i].CombatEntityPosition + Vector3.up * 0.5f);
			}
			TargetSelectionBuffers.LinecastVisibility.Clear();
			SpecialPhysicsCastUtils.GetLinecastVisibility(position + Vector3.up * 0.5f, TargetSelectionBuffers.LinecastDestinations, 256, TargetSelectionBuffers.LinecastVisibility);
		}
		ICombatEntity result = null;
		float num = float.PositiveInfinity;
		int num2 = -1;
		for (int j = 0; j < TargetSelectionBuffers.Targets.Count; j++)
		{
			if (!targetMustBeDirectlyVisible || (j < TargetSelectionBuffers.LinecastVisibility.Count && TargetSelectionBuffers.LinecastVisibility[j]))
			{
				int num3 = TargetSelectionBuffers.TargetPriorities[j];
				float num4 = TargetSelectionBuffers.TargetDistances[j];
				if (num3 > num2 || (num3 == num2 && num4 < num))
				{
					result = TargetSelectionBuffers.Targets[j];
					num = num4;
					num2 = num3;
				}
			}
		}
		return result;
	}

	protected bool TryGetDockPoint(FightingAgent agent, Wgo wgo, out DockPointData dockPoint, DockPointData.Availability availability = DockPointData.Availability.OnlyNotOccupied, DockPointData.Filter filter = DockPointData.Filter.OnlyNotZombie, bool getRandomInsteadOfNearest = false, Func<DockPointData, Vector3, bool> additionalCheck = null)
	{
		dockPoint = null;
		if (!wgo.HasAnyDockPoint)
		{
			return false;
		}
		if (!SGuid.IsNullOrEmpty(agent.Wgo.Data.takenDockPointsParentSGuid) && agent.Wgo.Data.takenDockPointsParentSGuid == wgo.Data.UniqueId)
		{
			dockPoint = wgo.Data.MainWgoPartData.GetOccupiedDockPointBy(agent.Wgo.Data.UniqueId);
		}
		if (dockPoint == null)
		{
			dockPoint = ((!getRandomInsteadOfNearest) ? wgo.Data.MainWgoPartData.GetNearestDockPoint(wgo.Data, agent.Wgo.Data.Position, availability, filter, OutOfGraphCheck) : wgo.Data.MainWgoPartData.GetDockPoints(availability, filter).GetRandom());
		}
		return dockPoint != null;
		bool OutOfGraphCheck(DockPointData data, Vector3 parentPos)
		{
			if (additionalCheck != null && !additionalCheck(data, parentPos))
			{
				return false;
			}
			return data.IsOnRecast(parentPos, FightingGameController.RecastGraph);
		}
	}

	protected bool HasAnyAvailableDockPointOnRecast(WgoData wgoData, DockPointData.Filter filter)
	{
		if (wgoData?.MainWgoPartData == null)
		{
			return false;
		}
		RecastGraph recastGraph = FightingGameController.RecastGraph;
		if (recastGraph == null)
		{
			return false;
		}
		Vector3 position = wgoData.Position;
		foreach (DockPointData dockPoint in wgoData.MainWgoPartData.GetDockPoints(DockPointData.Availability.OnlyNotOccupied, filter))
		{
			if (dockPoint.IsOnRecast(position, recastGraph))
			{
				return true;
			}
		}
		return false;
	}

	protected bool TryGetOverflowDockRing(FightingAgent agent, Wgo wgoTarget, out DockPointData nearestZombieDock, out float approachOffsetFromCenter)
	{
		nearestZombieDock = null;
		approachOffsetFromCenter = 0f;
		if (agent?.Wgo?.Data == null || wgoTarget?.Data?.MainWgoPartData == null)
		{
			return false;
		}
		nearestZombieDock = wgoTarget.Data.MainWgoPartData.GetNearestDockPoint(wgoTarget.Data, agent.Wgo.Data.Position, DockPointData.Availability.All, DockPointData.Filter.OnlyZombie);
		if (nearestZombieDock == null)
		{
			return false;
		}
		float magnitude = nearestZombieDock.BakedData.Position.XZ().magnitude;
		float num = ((agent.Settings != null) ? (agent.Settings.aiPathRadius * 2f) : 0.4f);
		approachOffsetFromCenter = magnitude + num;
		return true;
	}

	protected void ReleaseTakenDockPointIfDifferent(FightingAgent agent, Wgo targetWgo)
	{
		if (agent?.Wgo?.Data != null && !SGuid.IsNullOrEmpty(agent.Wgo.Data.takenDockPointsParentSGuid) && (!(targetWgo != null) || !(agent.Wgo.Data.takenDockPointsParentSGuid == targetWgo.Data.UniqueId)))
		{
			WgoData wgoData = MainGame.Instance?.GameSave?.WorldData?.GetWgoData(agent.Wgo.Data.takenDockPointsParentSGuid);
			wgoData?.MainWgoPartData?.TryFreeDockPoint(wgoData.UniqueId, agent.Wgo.Data.UniqueId);
			agent.Wgo.Data.takenDockPointsParentSGuid = SGuid.Empty;
			agent.IsAnchoredAtDockPoint = false;
		}
	}

	protected void TryClaimDockPoint(FightingAgent agent, Wgo targetWgo, DockPointData dockPoint)
	{
		if (agent?.Wgo?.Data == null || targetWgo?.Data == null || dockPoint == null)
		{
			return;
		}
		SGuid combatEntityUID = agent.Wgo.CombatEntityUID;
		if (!dockPoint.IsOccupied || dockPoint.IsOccupiedBy(combatEntityUID))
		{
			if (!dockPoint.IsOccupiedBy(combatEntityUID))
			{
				dockPoint.Occupy(combatEntityUID);
			}
			agent.Wgo.Data.takenDockPointsParentSGuid = targetWgo.Data.UniqueId;
		}
	}

	protected IEnumerable<ICombatEntity> FilterTargets(IEnumerable<ICombatEntity> targets, FightingAgent agent)
	{
		foreach (ICombatEntity target in targets)
		{
			if (IsTargetValid(target, agent))
			{
				yield return target;
			}
		}
	}

	[CanBeNull]
	protected virtual Func<IEnumerable<ICombatEntity>> WrapPotentialTargets(FightingAgent agent, [CanBeNull] Func<IEnumerable<ICombatEntity>> targets)
	{
		if (targets == null)
		{
			return null;
		}
		return delegate
		{
			IEnumerable<ICombatEntity> enumerable = targets();
			return (enumerable != null) ? FilterTargets(enumerable, agent) : Array.Empty<ICombatEntity>();
		};
	}

	protected virtual bool IsTargetValid(ICombatEntity target, FightingAgent agent)
	{
		if (target == null)
		{
			return false;
		}
		return true;
	}
}
