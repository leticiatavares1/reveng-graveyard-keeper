using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LazyBearTechnology;
using LinqTools;
using UnityEngine;

[CreateAssetMenu(menuName = "GK2/Fighting/AIs/EnemyDefault")]
public class EnemyDefaultAI : AgentAI
{
	[Serializable]
	protected sealed class LineEngagementDecisionStep : IEnemyDecisionStep, IAgentDecisionStep<EnemyDecisionContext>
	{
		public MobCommand TryCreateCommand(EnemyDecisionContext context)
		{
			if (context.Owner == null)
			{
				return null;
			}
			return context.Owner.TryGetLineEngagementCommand(context);
		}
	}

	[Serializable]
	protected sealed class CapturePointDecisionStep : IEnemyDecisionStep, IAgentDecisionStep<EnemyDecisionContext>
	{
		public MobCommand TryCreateCommand(EnemyDecisionContext context)
		{
			if (context.Owner == null)
			{
				return null;
			}
			return context.Owner.TryGetCapturePointCommand(context);
		}
	}

	[Serializable]
	protected sealed class PrimaryTargetDecisionStep : IEnemyDecisionStep, IAgentDecisionStep<EnemyDecisionContext>
	{
		public MobCommand TryCreateCommand(EnemyDecisionContext context)
		{
			if (context.Owner == null)
			{
				return null;
			}
			return context.Owner.TryEngagePrimaryTarget(context);
		}
	}

	public readonly struct EnemyDecisionContext
	{
		private readonly EnemyDefaultAI owner;

		private readonly FightingAgent agent;

		private readonly Func<IEnumerable<ICombatEntity>> playerTargets;

		private readonly float aggroDistance;

		private readonly ICombatEntity closestAggroTarget;

		private readonly ICombatEntity closestTarget;

		private readonly ICombatEntity nearestTargetOnLine;

		private readonly ICombatEntity nearestTargetOnLineDirectVisible;

		private readonly FightingCapturePoint nearestCapturePoint;

		private readonly bool isAgentOnNearestCapturePoint;

		private readonly bool isRangedAttacker;

		public EnemyDefaultAI Owner => owner;

		public FightingAgent Agent => agent;

		public Func<IEnumerable<ICombatEntity>> PlayerTargets => playerTargets;

		public float AggroDistance => aggroDistance;

		public bool HasPlayerTargets => (playerTargets?.Invoke()?.Any()).GetValueOrDefault();

		public ICombatEntity ClosestAggroTarget => closestAggroTarget;

		public ICombatEntity ClosestTarget => closestTarget;

		public ICombatEntity NearestTargetOnLine => nearestTargetOnLine;

		public ICombatEntity NearestTargetOnLineDirectVisible => nearestTargetOnLineDirectVisible;

		public FightingCapturePoint NearestCapturePoint => nearestCapturePoint;

		public bool IsAgentOnNearestCapturePoint => isAgentOnNearestCapturePoint;

		public bool IsRangedAttacker => isRangedAttacker;

		public EnemyDecisionContext(EnemyDefaultAI owner, FightingAgent agent, Func<IEnumerable<ICombatEntity>> potentialTargets, float aggroDistance)
		{
			this.owner = owner;
			this.agent = agent;
			this.aggroDistance = aggroDistance;
			isRangedAttacker = agent.AttackComponent?.IsRangedWeapon ?? false;
			playerTargets = potentialTargets;
			closestAggroTarget = AgentAI.GetClosestTarget(agent.Wgo.Data.Position, playerTargets, agent.Wgo.TeamType, aggroDistance, isRangedAttacker);
			closestTarget = AgentAI.GetClosestTarget(agent.Wgo.Data.Position, playerTargets, agent.Wgo.TeamType);
			FightingLine fightingLine = (agent.ParentController ? agent.ParentController.FightingLine : null);
			nearestTargetOnLine = owner.FindFrontmostTargetOnLine(agent, fightingLine, requireDirectVisibility: false);
			nearestTargetOnLineDirectVisible = owner.FindFrontmostTargetOnLine(agent, fightingLine, requireDirectVisibility: true);
			FightingCapturePoint fightingCapturePoint = fightingLine?.FindNearestEnemySectorBy(LazyConsts.Fighting.TeamType.WildZombie)?.point;
			if (!fightingCapturePoint)
			{
				fightingCapturePoint = LazySingleton<FightingGameController>.Instance.CurrentLevel?.BaseCapturePoint;
			}
			nearestCapturePoint = fightingCapturePoint;
			isAgentOnNearestCapturePoint = (bool)fightingCapturePoint && owner.IsOnCapturePoint(agent.Wgo.Data.Position, fightingCapturePoint);
		}

		public EnemyDecisionContext(EnemyDefaultAI owner, FightingAgent agent, Func<IEnumerable<ICombatEntity>> potentialTargets, float aggroDistance, ICombatEntity nearestTargetOnLine)
		{
			this.owner = owner;
			this.agent = agent;
			this.aggroDistance = aggroDistance;
			playerTargets = potentialTargets;
			isRangedAttacker = agent.AttackComponent?.IsRangedWeapon ?? false;
			closestAggroTarget = AgentAI.GetClosestTarget(agent.Wgo.Data.Position, playerTargets, agent.Wgo.TeamType, aggroDistance, isRangedAttacker);
			closestTarget = AgentAI.GetClosestTarget(agent.Wgo.Data.Position, playerTargets, agent.Wgo.TeamType);
			FightingLine fightingLine = (agent.ParentController ? agent.ParentController.FightingLine : null);
			this.nearestTargetOnLine = ((nearestTargetOnLine != null && owner.IsTargetValid(nearestTargetOnLine, agent)) ? nearestTargetOnLine : null);
			nearestTargetOnLineDirectVisible = (fightingLine ? owner.FindFrontmostTargetOnLine(agent, fightingLine, requireDirectVisibility: true) : null);
			FightingCapturePoint fightingCapturePoint = fightingLine?.FindNearestEnemySectorBy(LazyConsts.Fighting.TeamType.WildZombie)?.point;
			if (!fightingCapturePoint)
			{
				fightingCapturePoint = LazySingleton<FightingGameController>.Instance.CurrentLevel?.BaseCapturePoint;
			}
			nearestCapturePoint = fightingCapturePoint;
			isAgentOnNearestCapturePoint = (bool)fightingCapturePoint && owner.IsOnCapturePoint(agent.Wgo.Data.Position, fightingCapturePoint);
		}
	}

	private const float EPSILON = 0.4f;

	private const float TINY_EPSILON = 0.0001f;

	public float attackDistance = 1f;

	[Range(0f, 10f)]
	public float retargetDeltaTime = 1f;

	public int zombieMeleeDamage = 1;

	public LazyConsts.Fighting.EntityType ignoreEntityTypes;

	[SerializeReference]
	private List<IEnemyDecisionStep> customDecisionSteps = new List<IEnemyDecisionStep>();

	private List<IEnemyDecisionStep> defaultDecisionSteps;

	protected virtual List<IEnemyDecisionStep> DefaultDecisionSteps => new List<IEnemyDecisionStep>
	{
		new LineEngagementDecisionStep(),
		new CapturePointDecisionStep(),
		new PrimaryTargetDecisionStep()
	};

	[CanBeNull]
	public override MobCommand GetCommand(FightingAgent agent, Func<IEnumerable<ICombatEntity>> targets)
	{
		EnemyDecisionContext context = BuildContext(agent, targets);
		IReadOnlyList<IEnemyDecisionStep> activeDecisionSteps = GetActiveDecisionSteps();
		for (int i = 0; i < activeDecisionSteps.Count; i++)
		{
			IEnemyDecisionStep enemyDecisionStep = activeDecisionSteps[i];
			if (enemyDecisionStep != null)
			{
				MobCommand mobCommand = enemyDecisionStep.TryCreateCommand(context);
				if (mobCommand != null)
				{
					return mobCommand;
				}
			}
		}
		return null;
	}

	protected virtual EnemyDecisionContext BuildContext(FightingAgent agent, Func<IEnumerable<ICombatEntity>> targets)
	{
		return new EnemyDecisionContext(this, agent, WrapPotentialTargets(agent, targets), aggroDistance);
	}

	protected override bool IsTargetValid(ICombatEntity target, FightingAgent agent)
	{
		if (target == null)
		{
			return false;
		}
		if (base.IsTargetValid(target, agent))
		{
			return (target.EntityType & ignoreEntityTypes) == 0;
		}
		return false;
	}

	protected float DistToPos(FightingAgent agent, Vector3 pos)
	{
		return (agent.Wgo.Data.Position - pos).XZ().magnitude;
	}

	protected bool IsOnCapturePoint(Vector3 pos, FightingCapturePoint point)
	{
		return (pos - point.transform.position).XZ().magnitude < point.Radius;
	}

	private bool IsAgentInsideEpsilonCatchPoint(FightingAgent agent, FightingCapturePoint point)
	{
		float num = DistToPos(agent, point.transform.position);
		if (num < point.Radius)
		{
			return num > point.Radius - 0.4f;
		}
		return false;
	}

	private IReadOnlyList<IEnemyDecisionStep> GetActiveDecisionSteps()
	{
		if (customDecisionSteps != null)
		{
			for (int num = customDecisionSteps.Count - 1; num >= 0; num--)
			{
				if (customDecisionSteps[num] == null)
				{
					customDecisionSteps.RemoveAt(num);
				}
			}
			if (customDecisionSteps.Count > 0)
			{
				return customDecisionSteps;
			}
		}
		if (defaultDecisionSteps == null)
		{
			defaultDecisionSteps = DefaultDecisionSteps;
		}
		return defaultDecisionSteps;
	}

	private MobCommand TryGetLineEngagementCommand(EnemyDecisionContext context)
	{
		if (context.ClosestAggroTarget != null)
		{
			return null;
		}
		ICombatEntity combatEntity = ((!context.IsRangedAttacker) ? context.NearestTargetOnLine : context.NearestTargetOnLineDirectVisible);
		if (combatEntity == null)
		{
			return null;
		}
		if (context.IsAgentOnNearestCapturePoint)
		{
			return null;
		}
		FightingCapturePoint nearestCapturePoint = context.NearestCapturePoint;
		return DoGoAndAttackLogic(context.Agent, combatEntity, CreateLineTargetStopCondition(context, combatEntity, nearestCapturePoint));
	}

	private MobCommand TryGetCapturePointCommand(EnemyDecisionContext context)
	{
		return DoCapturingPointLogic(context);
	}

	protected virtual MobCommand TryEngagePrimaryTarget(EnemyDecisionContext context)
	{
		if (!context.HasPlayerTargets)
		{
			return null;
		}
		ICombatEntity combatEntity = context.ClosestAggroTarget ?? (context.IsAgentOnNearestCapturePoint ? null : context.ClosestTarget);
		if (combatEntity == null)
		{
			return null;
		}
		return DoGoAndAttackLogic(context.Agent, combatEntity, CreatePrimaryTargetStopCondition(context, combatEntity));
	}

	private Func<bool> CreateLineTargetStopCondition(EnemyDecisionContext context, ICombatEntity targetOnLine, FightingCapturePoint capturePoint)
	{
		return delegate
		{
			if (!IsTargetValid(targetOnLine, context.Agent))
			{
				return true;
			}
			if ((bool)capturePoint)
			{
				if (DistToPos(context.Agent, capturePoint.transform.position) < DistToPos(context.Agent, targetOnLine.CombatEntityPosition))
				{
					return true;
				}
				if (context.Agent.MobCommand is MobCommandGoTo { DestinationModifier: ControlPointDestinationModifier destinationModifier })
				{
					if (DistToPos(context.Agent, destinationModifier.GetCurrentTargetPosition()) < 0.1f)
					{
						return true;
					}
				}
				else if (IsOnCapturePoint(context.Agent.Wgo.Data.Position, capturePoint))
				{
					return true;
				}
			}
			FightingLine fightingLine = (context.Agent.ParentController ? context.Agent.ParentController.FightingLine : null);
			ICombatEntity combatEntity = FindFrontmostTargetOnLine(context.Agent, fightingLine, context.IsRangedAttacker);
			if (combatEntity != null && combatEntity != targetOnLine)
			{
				return true;
			}
			ICombatEntity closestTarget = AgentAI.GetClosestTarget(context.Agent.Wgo.Data.Position, context.PlayerTargets, context.Agent.Wgo.TeamType, context.AggroDistance, context.IsRangedAttacker);
			if (closestTarget != null && closestTarget != targetOnLine)
			{
				return true;
			}
			return ShouldRetargetDockAssignment(context.Agent, targetOnLine) ? true : false;
		};
	}

	protected Func<bool> CreatePrimaryTargetStopCondition(EnemyDecisionContext context, ICombatEntity chosenEntity)
	{
		return delegate
		{
			if (!IsTargetValid(chosenEntity, context.Agent))
			{
				return true;
			}
			if (context.Agent.MobCommand is MobCommandGoTo { DestinationModifier: ControlPointDestinationModifier destinationModifier })
			{
				if (DistToPos(context.Agent, destinationModifier.GetCurrentTargetPosition()) < 0.1f)
				{
					return true;
				}
			}
			else if (IsOnCapturePoint(context.Agent.Wgo.Data.Position, context.NearestCapturePoint))
			{
				return true;
			}
			if (AgentAI.GetClosestTarget(context.Agent.Wgo.Data.Position, context.PlayerTargets, context.Agent.Wgo.TeamType, context.AggroDistance, context.IsRangedAttacker) != chosenEntity)
			{
				return true;
			}
			return ShouldRetargetDockAssignment(context.Agent, chosenEntity) ? true : false;
		};
	}

	protected bool ShouldRetargetDockAssignment(FightingAgent agent, ICombatEntity chosenEntity)
	{
		WgoData wgoData = (chosenEntity as Wgo)?.Data;
		if (wgoData == null && agent.MobCommand is MobCommandGoTo { DestinationModifier: CombatEntityDestinationModifier destinationModifier })
		{
			wgoData = destinationModifier.TargeObj;
		}
		if (wgoData?.MainWgoPartData == null)
		{
			return false;
		}
		SGuid combatEntityUID = agent.Wgo.CombatEntityUID;
		if (wgoData.MainWgoPartData.GetOccupiedDockPointBy(combatEntityUID) != null)
		{
			return false;
		}
		if (agent.MobCommand is ZombieMeleeAttackCommand { CustomDockPoint: not null } zombieMeleeAttackCommand && !zombieMeleeAttackCommand.CustomDockPoint.IsOccupied)
		{
			return false;
		}
		if (agent.MobCommand is MobCommandGoTo { DestinationModifier: CombatEntityDestinationModifier { TargetDockPoint: not null } destinationModifier2 })
		{
			if (destinationModifier2.TargetDockPoint.IsOccupied)
			{
				return !destinationModifier2.TargetDockPoint.IsOccupiedBy(combatEntityUID);
			}
			return false;
		}
		return HasAnyAvailableDockPointOnRecast(wgoData, DockPointData.Filter.All);
	}

	private MobCommand DoGoAndAttackLogic(FightingAgent agent, ICombatEntity entity, Func<bool> stopCondition)
	{
		if (!IsTargetValid(entity, agent))
		{
			return null;
		}
		Wgo wgo = entity as Wgo;
		ReleaseTakenDockPointIfDifferent(agent, wgo);
		float num = agent.FighterDef?.atkRange.EvaluateFloat() ?? attackDistance;
		int damage = agent.FighterDef?.atkDamage.EvaluateInt(agent.Wgo) ?? agent.Weapon?.ItemDef?.damage.EvaluateInt(agent.Wgo) ?? zombieMeleeDamage;
		float num2 = -1f;
		DockPointData dockPoint = null;
		if ((bool)wgo && TryGetDockPoint(agent, wgo, out dockPoint, DockPointData.Availability.OnlyNotOccupied, DockPointData.Filter.All))
		{
			num2 = DistToPos(agent, dockPoint.GetPosFrom(wgo.Data.Position));
			if ((num2 - 0.06666668f).More(0f, 0.0001f))
			{
				return new MobCommandGoTo(new CombatEntityDestinationModifier(dockPoint)).WithCustomTargetDestinationOffset(0.06666668f).WithCustomStopCondition(stopCondition, retargetDeltaTime, UnityEngine.Random.Range(0f, retargetDeltaTime)).WithCustomActionOnDestReached(delegate
				{
					agent.SetFacingDirection(dockPoint.Direction.ConvertToVector2XZ(), instant: true);
					TryAnchorAtTakenDockPoint(agent);
				})
					.ToTarget(wgo);
			}
		}
		DockPointData nearestZombieDock = null;
		float num3 = num;
		if (dockPoint != null)
		{
			num3 = 0.06666668f;
		}
		else
		{
			num2 = DistToPos(agent, entity.CombatEntityPosition);
			if (wgo != null && TryGetOverflowDockRing(agent, wgo, out nearestZombieDock, out var approachOffsetFromCenter))
			{
				num3 = approachOffsetFromCenter;
			}
		}
		float num4 = Mathf.Max(num, num3);
		if ((num2 - num3).More(0f, 0.0001f))
		{
			return new MobCommandGoTo(new CombatEntityDestinationModifier(dockPoint)).WithCustomTargetDestinationOffset(num3).WithCustomStopCondition(stopCondition, retargetDeltaTime, UnityEngine.Random.Range(0f, retargetDeltaTime)).WithCustomActionOnDestReached(delegate
			{
				TryAnchorAtTakenDockPoint(agent);
			})
				.ToTarget(entity);
		}
		if ((num2 - num4).Less(0f, 0.0001f))
		{
			if (dockPoint != null)
			{
				TryClaimDockPoint(agent, wgo, dockPoint);
			}
			return new ZombieMeleeAttackCommand((dockPoint != null) ? num : Mathf.Max(num, num3)).WithCustomDockPoint(dockPoint ?? nearestZombieDock).WithDamage(damage).WithCustomStopCondition(stopCondition)
				.ToTarget(entity);
		}
		return null;
	}

	[CanBeNull]
	protected ICombatEntity FindFrontmostTargetOnLine(FightingAgent agent, [CanBeNull] FightingLine fightingLine, bool requireDirectVisibility, Func<ICombatEntity, bool> additionalFilter = null)
	{
		if (fightingLine == null || LazySingleton<FightingGameController>.Instance?.TargetsDatabase == null)
		{
			return null;
		}
		int num = int.MinValue;
		ICombatEntity result = null;
		float num2 = float.MaxValue;
		foreach (TargetInfo allTarget in LazySingleton<FightingGameController>.Instance.TargetsDatabase.AllTargets)
		{
			if (allTarget.LineId == fightingLine.lineIdx && allTarget.Team == LazyConsts.Fighting.TeamType.Player && IsTargetValid(allTarget.entity, agent) && (additionalFilter == null || additionalFilter(allTarget.entity)) && (!requireDirectVisibility || AgentAI.TryLineCastByRecast(agent.Wgo.Data.Position, allTarget.entity.CombatEntityPosition)) && allTarget.SectorId >= num)
			{
				float combatEntityDistance = allTarget.entity.GetCombatEntityDistance(agent.Wgo.Data.Position, agent.Wgo.TeamType);
				if (allTarget.SectorId > num)
				{
					num = allTarget.SectorId;
					result = allTarget.entity;
					num2 = combatEntityDistance;
				}
				else if (combatEntityDistance < num2)
				{
					result = allTarget.entity;
					num2 = combatEntityDistance;
				}
			}
		}
		return result;
	}

	private static void TryAnchorAtTakenDockPoint(FightingAgent agent)
	{
		if (!SGuid.IsNullOrEmpty(agent?.Wgo?.Data?.takenDockPointsParentSGuid))
		{
			agent.IsAnchoredAtDockPoint = true;
		}
	}

	private MobCommand DoCapturingPointLogic(EnemyDecisionContext context)
	{
		if (context.ClosestAggroTarget != null)
		{
			return null;
		}
		FightingCapturePoint capturePoint = context.NearestCapturePoint;
		if (!capturePoint)
		{
			return null;
		}
		Func<IEnumerable<ICombatEntity>> playerTargets = context.PlayerTargets;
		IEnumerable<ICombatEntity> targets = playerTargets?.Invoke();
		FightingAgent agent = context.Agent;
		if (capturePoint.OwnedByTeam != agent.Wgo.TeamType)
		{
			bool flag = IsOnCapturePoint(agent.Wgo.Data.Position, capturePoint);
			if (agent.MobCommand is MobCommandGoTo { DestinationModifier: ControlPointDestinationModifier destinationModifier })
			{
				if (!(DistToPos(agent, destinationModifier.GetCurrentTargetPosition()) < 0.1f))
				{
					return null;
				}
				flag = true;
			}
			if (!flag)
			{
				return new MobCommandGoTo(new ControlPointDestinationModifier(capturePoint)).WithCustomStopCondition(delegate
				{
					if (AgentAI.GetClosestTarget(agent.Wgo.Data.Position, playerTargets, agent.Wgo.TeamType, aggroDistance, context.IsRangedAttacker) != null)
					{
						return true;
					}
					return capturePoint.OwnedByTeam == LazyConsts.Fighting.TeamType.WildZombie;
				}, retargetDeltaTime, UnityEngine.Random.Range(0f, retargetDeltaTime)).ToPosition(capturePoint.gameObject.transform.position);
			}
			ICombatEntity entityInsideSector = AgentAI.GetClosestTarget(capturePoint.transform.position, targets, context.Agent.Wgo.TeamType, aggroDistance);
			if (entityInsideSector != null && IsAgentInsideEpsilonCatchPoint(agent, capturePoint))
			{
				return DoGoAndAttackLogic(agent, entityInsideSector, () => !IsOnCapturePoint(entityInsideSector.CombatEntityPosition, capturePoint) || AgentAI.GetClosestTarget(capturePoint.transform.position, targets, context.Agent.Wgo.TeamType, aggroDistance) != entityInsideSector);
			}
		}
		return null;
	}
}
