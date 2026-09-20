using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

[CreateAssetMenu(menuName = "GK2/Fighting/AIs/EnemySpit")]
public class EnemySpitAI : EnemyDefaultAI
{
	[Serializable]
	private sealed class SpitEngagementDecisionStep : IEnemyDecisionStep, IAgentDecisionStep<EnemyDecisionContext>
	{
		public MobCommand TryCreateCommand(EnemyDecisionContext context)
		{
			if (!(context.Owner is EnemySpitAI enemySpitAI))
			{
				return null;
			}
			return enemySpitAI.TryGetSpitEngagementCommand(context);
		}
	}

	private const float TINY_EPSILON = 0.0001f;

	[Header("Debug")]
	[SerializeField]
	private bool debugSpitAI;

	private readonly Dictionary<SGuid, SGuid> lockedTargetByAgent = new Dictionary<SGuid, SGuid>();

	protected override List<IEnemyDecisionStep> DefaultDecisionSteps => new List<IEnemyDecisionStep>
	{
		new SpitEngagementDecisionStep(),
		new CapturePointDecisionStep()
	};

	public override MobCommand GetCommand(FightingAgent agent, Func<IEnumerable<ICombatEntity>> targets)
	{
		return base.GetCommand(agent, targets);
	}

	[CanBeNull]
	private MobCommand TryGetSpitEngagementCommand(EnemyDecisionContext context)
	{
		if (!context.HasPlayerTargets)
		{
			ClearLockedTarget(context.Agent);
			return null;
		}
		float attackRange = GetAttackRange(context.Agent);
		ICombatEntity combatEntity = ResolveEngagementTarget(context, attackRange);
		if (combatEntity == null)
		{
			ClearLockedTarget(context.Agent);
			return null;
		}
		if (IsTargetAttackable(context.Agent, combatEntity, attackRange))
		{
			LockTarget(context.Agent, combatEntity);
			return CreateSpitAttackCommand(context, combatEntity, attackRange);
		}
		ClearLockedTarget(context.Agent);
		return CreateApproachCommand(context, combatEntity, attackRange);
	}

	[CanBeNull]
	private MobCommand CreateSpitAttackCommand(EnemyDecisionContext context, ICombatEntity entity, float attackRange)
	{
		FightingAgent agent = context.Agent;
		Func<bool> condition = () => !IsTargetAttackable(agent, entity, GetAttackRange(agent));
		return new ZombieSpitAttackCommand(attackRange).WithDebugLogs(debugSpitAI).WithCustomStopCondition(condition).ToTarget(entity);
	}

	[CanBeNull]
	private MobCommand CreateApproachCommand(EnemyDecisionContext context, ICombatEntity entity, float attackRange)
	{
		FightingAgent agent = context.Agent;
		if (!IsTargetAliveAndValid(agent, entity))
		{
			return null;
		}
		Func<bool> func = CreateApproachStopCondition(context, entity);
		Wgo wgo = entity as Wgo;
		ReleaseTakenDockPointIfDifferent(agent, wgo);
		DockPointData dockPoint = null;
		float num;
		if ((bool)wgo && TryGetDockPoint(agent, wgo, out dockPoint, DockPointData.Availability.OnlyNotOccupied, DockPointData.Filter.All))
		{
			num = DistToPos(agent, dockPoint.GetPosFrom(wgo.Data.Position));
			if (IsWithinAttackRange(agent, entity, attackRange))
			{
				if (!HasDirectVisionToTarget(agent, entity))
				{
					return CreateGoToCommand(agent, entity, dockPoint, 0.06666668f, func);
				}
				return null;
			}
			if ((num - 0.06666668f).More(0f, 0.0001f))
			{
				return new MobCommandGoTo(new CombatEntityDestinationModifier(dockPoint)).WithCustomTargetDestinationOffset(0.06666668f).WithCustomStopCondition(func, retargetDeltaTime, UnityEngine.Random.Range(0f, retargetDeltaTime)).WithCustomActionOnDestReached(delegate
				{
					agent.SetFacingDirection(dockPoint.Direction.ConvertToVector2XZ(), instant: true);
					TryAnchorAtTakenDockPoint(agent);
				})
					.ToTarget(wgo);
			}
		}
		num = DistToPos(agent, entity.CombatEntityPosition);
		float num2 = ((dockPoint != null) ? 0.06666668f : attackRange);
		if (dockPoint == null && wgo != null && TryGetOverflowDockRing(agent, wgo, out var _, out var approachOffsetFromCenter))
		{
			num2 = approachOffsetFromCenter;
		}
		if (!HasDirectVisionToTarget(agent, entity) && IsWithinAttackRange(agent, entity, attackRange))
		{
			num2 = 0.06666668f;
		}
		if ((num - num2).More(0f, 0.0001f))
		{
			return CreateGoToCommand(agent, entity, dockPoint, num2, func);
		}
		return null;
	}

	private MobCommand CreateGoToCommand(FightingAgent agent, ICombatEntity entity, DockPointData dockPoint, float destinationOffset, Func<bool> goToStopCondition)
	{
		return new MobCommandGoTo(new CombatEntityDestinationModifier(dockPoint)).WithCustomTargetDestinationOffset(destinationOffset).WithCustomStopCondition(goToStopCondition, retargetDeltaTime, UnityEngine.Random.Range(0f, retargetDeltaTime)).WithCustomActionOnDestReached(delegate
		{
			TryAnchorAtTakenDockPoint(agent);
		})
			.ToTarget(entity);
	}

	private Func<bool> CreateApproachStopCondition(EnemyDecisionContext context, ICombatEntity chosenEntity)
	{
		return delegate
		{
			FightingAgent agent = context.Agent;
			if (!IsTargetAliveAndValid(agent, chosenEntity))
			{
				return true;
			}
			float attackRange = GetAttackRange(agent);
			if (IsTargetAttackable(agent, chosenEntity, attackRange))
			{
				return true;
			}
			ICombatEntity combatEntity = FindClosestApproachTarget(context);
			return (combatEntity != null && combatEntity.CombatEntityUID != chosenEntity.CombatEntityUID) || ShouldRetargetDockAssignment(agent, chosenEntity);
		};
	}

	[CanBeNull]
	private ICombatEntity ResolveEngagementTarget(EnemyDecisionContext context, float attackRange)
	{
		FightingAgent agent = context.Agent;
		SGuid uniqueId = agent.Wgo.Data.UniqueId;
		if (lockedTargetByAgent.TryGetValue(uniqueId, out var value) && !SGuid.IsNullOrEmpty(value))
		{
			ICombatEntity combatEntity = FindTargetById(context.PlayerTargets, value);
			if (IsTargetAttackable(agent, combatEntity, attackRange))
			{
				return combatEntity;
			}
			lockedTargetByAgent.Remove(uniqueId);
		}
		ICombatEntity combatEntity2 = FindClosestAttackableTarget(context, attackRange);
		if (combatEntity2 != null)
		{
			return combatEntity2;
		}
		return FindClosestApproachTarget(context);
	}

	[CanBeNull]
	private ICombatEntity FindClosestAttackableTarget(EnemyDecisionContext context, float attackRange)
	{
		FightingAgent agent = context.Agent;
		IEnumerable<ICombatEntity> enumerable = context.PlayerTargets?.Invoke();
		if (enumerable == null)
		{
			return null;
		}
		ICombatEntity result = null;
		float num = float.MaxValue;
		int num2 = -1;
		foreach (ICombatEntity item in enumerable)
		{
			if (IsTargetAttackable(agent, item, attackRange))
			{
				float num3 = DistToPos(agent, item.CombatEntityPosition);
				int attackPriority = item.AttackPriority;
				if (attackPriority > num2 || (attackPriority == num2 && num3 < num))
				{
					result = item;
					num = num3;
					num2 = attackPriority;
				}
			}
		}
		return result;
	}

	[CanBeNull]
	private ICombatEntity FindClosestApproachTarget(EnemyDecisionContext context)
	{
		FightingAgent agent = context.Agent;
		if (IsTargetAliveAndValid(agent, context.ClosestAggroTarget))
		{
			return context.ClosestAggroTarget;
		}
		if (context.IsAgentOnNearestCapturePoint)
		{
			return null;
		}
		ICombatEntity closestTarget = context.ClosestTarget;
		if (!IsTargetAliveAndValid(agent, closestTarget))
		{
			return null;
		}
		if (!(DistToPos(agent, closestTarget.CombatEntityPosition) <= deAggroDistance))
		{
			return null;
		}
		return closestTarget;
	}

	private bool IsTargetAttackable(FightingAgent agent, ICombatEntity entity, float attackRange)
	{
		if (!IsTargetAliveAndValid(agent, entity))
		{
			return false;
		}
		if (!IsWithinAttackRange(agent, entity, attackRange))
		{
			return false;
		}
		if (IsBarricadeTarget(entity))
		{
			return true;
		}
		return HasDirectVisionToTarget(agent, entity);
	}

	private static bool IsBarricadeTarget(ICombatEntity entity)
	{
		return FightingWgoTarget.IsBarricadeOrTower(entity);
	}

	private bool IsWithinAttackRange(FightingAgent agent, ICombatEntity entity, float attackRange)
	{
		return DistToPos(agent, entity.CombatEntityPosition) <= attackRange + 0.06666668f;
	}

	private bool IsTargetAliveAndValid(FightingAgent agent, ICombatEntity entity)
	{
		if (!IsTargetValid(entity, agent))
		{
			return false;
		}
		if (entity.CombatEntityHpComponent != null)
		{
			return entity.CombatEntityHpComponent.Hp > 0;
		}
		return false;
	}

	private static float GetAttackRange(FightingAgent agent)
	{
		if (agent?.FighterDef?.atkRange == null)
		{
			return 0f;
		}
		return agent.FighterDef.atkRange.EvaluateFloat(agent.Wgo);
	}

	[CanBeNull]
	private static ICombatEntity FindTargetById(Func<IEnumerable<ICombatEntity>> targets, SGuid targetId)
	{
		if (targets == null || SGuid.IsNullOrEmpty(targetId))
		{
			return null;
		}
		IEnumerable<ICombatEntity> enumerable = targets();
		if (enumerable == null)
		{
			return null;
		}
		foreach (ICombatEntity item in enumerable)
		{
			if (item != null && item.CombatEntityUID == targetId)
			{
				return item;
			}
		}
		return null;
	}

	private void LockTarget(FightingAgent agent, ICombatEntity entity)
	{
		if (!(agent?.Wgo == null) && entity != null)
		{
			lockedTargetByAgent[agent.Wgo.Data.UniqueId] = entity.CombatEntityUID;
		}
	}

	private void ClearLockedTarget(FightingAgent agent)
	{
		if (!(agent?.Wgo == null))
		{
			lockedTargetByAgent.Remove(agent.Wgo.Data.UniqueId);
		}
	}

	private static bool HasDirectVisionToTarget(FightingAgent agent, ICombatEntity entity)
	{
		return AgentAI.TryLineCastByRecast(GetLinecastOrigin(agent), entity.CombatEntityPosition + Vector3.up * 0.5f);
	}

	private static Vector3 GetLinecastOrigin(FightingAgent agent)
	{
		if ((bool)agent.AttackComponent?.weapon)
		{
			return agent.AttackComponent.weapon.transform.position;
		}
		return agent.Wgo.Data.Position + Vector3.up * 0.5f;
	}

	private static void TryAnchorAtTakenDockPoint(FightingAgent agent)
	{
		if (!SGuid.IsNullOrEmpty(agent?.Wgo?.Data?.takenDockPointsParentSGuid))
		{
			agent.IsAnchoredAtDockPoint = true;
		}
	}
}
