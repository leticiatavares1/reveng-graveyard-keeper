using System;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

[CreateAssetMenu(menuName = "GK2/Fighting/AIs/AllyDefault")]
public class AllyDefaultAI : AgentAI
{
	[Serializable]
	private sealed class CombatDecisionStep : IAllyDecisionStep, IAgentDecisionStep<AllyDecisionContext>
	{
		public MobCommand TryCreateCommand(AllyDecisionContext context)
		{
			if (context.Owner == null)
			{
				return null;
			}
			return context.Owner.TryGetCombatCommand(context);
		}
	}

	[Serializable]
	private sealed class CapturePointDecisionStep : IAllyDecisionStep, IAgentDecisionStep<AllyDecisionContext>
	{
		public MobCommand TryCreateCommand(AllyDecisionContext context)
		{
			if (context.Owner == null)
			{
				return null;
			}
			return context.Owner.TryGetCapturePointCommand(context);
		}
	}

	[Serializable]
	private sealed class FlagFollowDecisionStep : IAllyDecisionStep, IAgentDecisionStep<AllyDecisionContext>
	{
		public MobCommand TryCreateCommand(AllyDecisionContext context)
		{
			if (context.Owner == null)
			{
				return null;
			}
			return context.Owner.TryGetFlagFollowCommand(context);
		}
	}

	[Serializable]
	private sealed class FlagDockingDecisionStep : IAllyDecisionStep, IAgentDecisionStep<AllyDecisionContext>
	{
		public MobCommand TryCreateCommand(AllyDecisionContext context)
		{
			if (context.Owner == null)
			{
				return null;
			}
			return context.Owner.TryGetFlagDockingCommand(context);
		}
	}

	public readonly struct AllyDecisionContext
	{
		private readonly AllyDefaultAI owner;

		private readonly FightingAgent agent;

		private readonly AgentsGroupFlagController flagController;

		private readonly List<ICombatEntity> enemyTargets;

		private readonly Func<IEnumerable<ICombatEntity>> potentialTargets;

		private readonly float attackRange;

		private readonly float followDistance;

		private readonly float aggroDistance;

		private readonly float deAggroDistance;

		private readonly bool isRangedAttacker;

		public AllyDefaultAI Owner => owner;

		public FightingAgent Agent => agent;

		public bool HasValidFlag
		{
			get
			{
				if ((bool)flagController)
				{
					return flagController.FlagWgo;
				}
				return false;
			}
		}

		public AgentsGroupFlagController FlagController => flagController;

		public IReadOnlyList<ICombatEntity> EnemyTargets => enemyTargets;

		public Func<IEnumerable<ICombatEntity>> PotentialTargets => potentialTargets;

		public float AttackRange => attackRange;

		public float FollowDistance => followDistance;

		public float AggroDistance => aggroDistance;

		public float DeAggroDistance => deAggroDistance;

		public bool HasEnemyTargets => enemyTargets.Count > 0;

		public bool IsRangedAttacker => isRangedAttacker;

		public AllyDecisionContext(AllyDefaultAI owner, FightingAgent agent, Func<IEnumerable<ICombatEntity>> potentialTargets, AgentsGroupFlagController flagController, float followDistance, float defaultAttackDistance, float aggroDistance, float deAggroDistance)
		{
			this.owner = owner;
			this.agent = agent;
			this.flagController = flagController;
			this.followDistance = followDistance;
			this.aggroDistance = aggroDistance;
			this.deAggroDistance = deAggroDistance;
			this.potentialTargets = potentialTargets;
			isRangedAttacker = agent.AttackComponent.IsRangedWeapon;
			attackRange = (agent.AttackComponent.weapon ? ((float)agent.FighterDef.atkRange.EvaluateInt(agent.Wgo)) : defaultAttackDistance);
			enemyTargets = new List<ICombatEntity>();
			if (potentialTargets == null)
			{
				return;
			}
			foreach (ICombatEntity item in potentialTargets())
			{
				if (item != null && item.TeamType != agent.Wgo.TeamType)
				{
					enemyTargets.Add(item);
				}
			}
		}
	}

	private const float EPSILON = 0.4f;

	private const float ARCHER_KITE_MIN_MOVE = 0.75f;

	private const float ARCHER_KITE_MIN_GAIN = 0.35f;

	private const float ARCHER_KITE_SLOT_RADIUS = 0.9f;

	private const float ARCHER_KITE_MAX_SNAP = 0.5f;

	private static readonly float[] ArcherKiteYawOffsetsDeg = new float[5] { 0f, 45f, -45f, 90f, -90f };

	public float attackDistance = 0.1f;

	[Range(0.4f, 20f)]
	public float followDistance = 2f;

	[Range(0f, 10f)]
	public float retargetDeltaTime = 1f;

	public bool pikemenHoldFrontline = true;

	[Range(0f, 20f)]
	public float archerKiteDistance = 3f;

	[SerializeReference]
	private List<IAllyDecisionStep> customDecisionSteps = new List<IAllyDecisionStep>();

	private List<IAllyDecisionStep> defaultDecisionSteps;

	private float DistToEnemy(FightingAgent agent, ICombatEntity enemy)
	{
		return (agent.Wgo.Data.Position - enemy.CombatEntityPosition).XZ().magnitude;
	}

	private bool TryDistToFlag(FightingAgent agent, out float dist)
	{
		dist = 0f;
		if (!agent.FlagController || !agent.FlagController.FlagWgo)
		{
			return false;
		}
		dist = (agent.FlagController.FlagWgo.Data.Position - agent.Wgo.Data.Position).XZ().magnitude;
		return true;
	}

	private bool IsAgentInsideRangeFlag(FightingAgent agent)
	{
		if (!TryDistToFlag(agent, out var dist))
		{
			return false;
		}
		return dist < followDistance;
	}

	private bool HasEnemyInAttackRange(AllyDecisionContext context)
	{
		for (int i = 0; i < context.EnemyTargets.Count; i++)
		{
			if (DistToEnemy(context.Agent, context.EnemyTargets[i]) < context.AttackRange)
			{
				return true;
			}
		}
		return false;
	}

	public override MobCommand GetCommand(FightingAgent agent, Func<IEnumerable<ICombatEntity>> targets)
	{
		AgentsGroupFlagController flagController = agent.FlagController;
		if (!flagController)
		{
			Debug.LogError("FlagController is not set, but should", this);
			return null;
		}
		AllyDecisionContext context = new AllyDecisionContext(this, agent, WrapPotentialTargets(agent, targets), flagController, followDistance, attackDistance, aggroDistance, deAggroDistance);
		IReadOnlyList<IAllyDecisionStep> activeDecisionSteps = GetActiveDecisionSteps();
		for (int i = 0; i < activeDecisionSteps.Count; i++)
		{
			IAllyDecisionStep allyDecisionStep = activeDecisionSteps[i];
			if (allyDecisionStep != null)
			{
				MobCommand mobCommand = allyDecisionStep.TryCreateCommand(context);
				if (mobCommand != null)
				{
					return mobCommand;
				}
			}
		}
		return null;
	}

	private IReadOnlyList<IAllyDecisionStep> GetActiveDecisionSteps()
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
			defaultDecisionSteps = new List<IAllyDecisionStep>
			{
				new CombatDecisionStep(),
				new CapturePointDecisionStep(),
				new FlagDockingDecisionStep(),
				new FlagFollowDecisionStep()
			};
		}
		return defaultDecisionSteps;
	}

	private MobCommand TryGetCombatCommand(AllyDecisionContext context)
	{
		if (context.Agent.IsUnderMainHeroPush)
		{
			return null;
		}
		FightingCapturePoint nearestAllyCapturePoint = GetNearestAllyCapturePoint(context.Agent);
		ICombatEntity combatEntity = ((nearestAllyCapturePoint != null) ? GetClosestEnemyOnCapturePoint(nearestAllyCapturePoint, context.Agent) : null);
		bool flag = combatEntity != null && DistToEnemy(context.Agent, combatEntity) <= context.AggroDistance;
		ICombatEntity combatEntity2 = (flag ? combatEntity : null);
		if (combatEntity2 == null)
		{
			if (!context.HasEnemyTargets)
			{
				return null;
			}
			combatEntity2 = AgentAI.GetClosestTarget(context.Agent.Wgo.Data.Position, context.EnemyTargets, context.Agent.Wgo.TeamType, context.AggroDistance, context.IsRangedAttacker);
		}
		if (combatEntity2 == null)
		{
			return null;
		}
		if (context.FlagController.IsSetAtPoint)
		{
			if (!context.Agent.IsAnchoredAtDockPoint)
			{
				return null;
			}
			return TryAttackEnemy(context, combatEntity2, flag);
		}
		if (!flag)
		{
			MobCommand mobCommand = TryKeepAgentCloseToFlag(context, combatEntity2);
			if (mobCommand != null)
			{
				return mobCommand;
			}
			MobCommand mobCommand2 = TryKiteAwayFromEnemy(context, combatEntity2, flag);
			if (mobCommand2 != null)
			{
				return mobCommand2;
			}
		}
		MobCommand mobCommand3 = TryChaseEnemy(context, combatEntity2, flag);
		if (mobCommand3 != null)
		{
			return mobCommand3;
		}
		return TryAttackEnemy(context, combatEntity2, flag);
	}

	private MobCommand TryKeepAgentCloseToFlag(AllyDecisionContext context, ICombatEntity enemy = null)
	{
		if (context.Agent.IsUnderMainHeroPush)
		{
			return null;
		}
		if (!context.HasValidFlag)
		{
			return null;
		}
		bool flag = IsPikeman(context.Agent);
		if (enemy != null && pikemenHoldFrontline && flag)
		{
			if (DistToEnemy(context.Agent, enemy) <= context.AttackRange)
			{
				return null;
			}
			Vector3 flagEdgePositionTowards = GetFlagEdgePositionTowards(context, enemy.CombatEntityPosition);
			if (DistToPos(context.Agent, flagEdgePositionTowards) <= 0.4f)
			{
				return null;
			}
			return new MobCommandGoTo(new CombatEntityDestinationModifier()).ToPosition(flagEdgePositionTowards);
		}
		if (IsAgentInsideRangeFlag(context.Agent))
		{
			return null;
		}
		return new MobCommandGoTo(new CombatEntityDestinationModifier()).WithCustomTargetDestinationOffset(context.FollowDistance).ToTarget(context.FlagController.FlagWgo);
	}

	private Vector3 GetFlagEdgePositionTowards(AllyDecisionContext context, Vector3 towardsPosition)
	{
		Vector3 position = context.FlagController.FlagWgo.Data.Position;
		Vector2 vector = (towardsPosition - position).XZ2();
		if (vector.sqrMagnitude <= 0.0001f)
		{
			return position;
		}
		return position + vector.normalized.XZ() * context.FollowDistance;
	}

	private bool IsPikeman(FightingAgent agent)
	{
		return agent.AttackCommandType == MobCommand.CommandType.ZombiePikeAttack;
	}

	private bool IsArcher(FightingAgent agent)
	{
		return agent.AttackCommandType == MobCommand.CommandType.ZombieBowAttack;
	}

	private MobCommand TryKiteAwayFromEnemy(AllyDecisionContext context, ICombatEntity enemy, bool defendCapturePoint)
	{
		if (context.Agent.IsUnderMainHeroPush)
		{
			return null;
		}
		if (archerKiteDistance <= 0f || !IsArcher(context.Agent))
		{
			return null;
		}
		float num = DistToEnemy(context.Agent, enemy);
		if (num >= archerKiteDistance)
		{
			return null;
		}
		Vector3 position = context.Agent.Wgo.Data.Position;
		Vector2 vector = (position - enemy.CombatEntityPosition).XZ2();
		if (vector.sqrMagnitude <= 0.0001f)
		{
			return null;
		}
		Vector2 normalized = vector.normalized;
		float num2 = archerKiteDistance - num;
		Vector3 combatEntityPosition = enemy.CombatEntityPosition;
		Vector3 vector2 = default(Vector3);
		float num3 = float.NegativeInfinity;
		bool flag = false;
		for (int i = 0; i < ArcherKiteYawOffsetsDeg.Length; i++)
		{
			Vector2 vector3 = RotateXZ(normalized, ArcherKiteYawOffsetsDeg[i]);
			Vector3 candidate = ClampToFlagFollowRadius(context, position + vector3.XZ() * num2);
			if (!TryProjectKitePointOntoGraph(context.Agent, candidate, out var onGraph, out var node))
			{
				continue;
			}
			candidate = onGraph;
			float num4 = DistToPos(context.Agent, candidate);
			if (num4 < 0.75f)
			{
				continue;
			}
			float magnitude = (candidate - combatEntityPosition).XZ().magnitude;
			if (!(magnitude < num + 0.35f) && !IsArcherKiteSlotOccupied(context, candidate) && IsKitePointReachable(context.Agent, node))
			{
				float num5 = magnitude * 10f - num4;
				if (!(num5 <= num3))
				{
					num3 = num5;
					vector2 = candidate;
					flag = true;
				}
			}
		}
		if (!flag)
		{
			return null;
		}
		Vector3 retreatPosition = vector2;
		return new MobCommandGoTo(new CombatEntityDestinationModifier()).WithCustomStopCondition(() => DistToEnemy(context.Agent, enemy) >= archerKiteDistance || DistToPos(context.Agent, retreatPosition) <= 0.75f || ShouldStopFlagBoundCombat(context, defendCapturePoint)).ToPosition(retreatPosition);
	}

	private static Vector3 ClampToFlagFollowRadius(AllyDecisionContext context, Vector3 position)
	{
		if (!context.HasValidFlag)
		{
			return position;
		}
		Vector3 position2 = context.FlagController.FlagWgo.Data.Position;
		Vector2 vector = (position - position2).XZ2();
		if (vector.magnitude <= context.FollowDistance)
		{
			return position;
		}
		return position2 + vector.normalized.XZ() * context.FollowDistance;
	}

	private static Vector2 RotateXZ(Vector2 v, float degrees)
	{
		float f = degrees * (MathF.PI / 180f);
		float num = Mathf.Cos(f);
		float num2 = Mathf.Sin(f);
		return new Vector2(v.x * num - v.y * num2, v.x * num2 + v.y * num);
	}

	private bool TryProjectKitePointOntoGraph(FightingAgent agent, Vector3 candidate, out Vector3 onGraph, out GraphNode node)
	{
		onGraph = default(Vector3);
		node = null;
		RecastGraph recastGraph = base.FightingGameController?.RecastGraph;
		if (recastGraph == null || (UnityEngine.Object)(object)AstarPath.active == null)
		{
			return false;
		}
		NearestNodeConstraint walkable = NearestNodeConstraint.Walkable;
		walkable.graphMask = GraphMask.FromGraph(recastGraph);
		walkable.distanceMetric = DistanceMetric.ClosestAsSeenFromAbove();
		NNInfo nearest = recastGraph.GetNearest(candidate, walkable);
		if (nearest.node == null || !nearest.node.Walkable)
		{
			return false;
		}
		if ((nearest.position - candidate).XZ().sqrMagnitude > 0.25f)
		{
			return false;
		}
		onGraph = nearest.position;
		node = nearest.node;
		if ((bool)agent.FlagController && (bool)agent.FlagController.FlagWgo)
		{
			Vector3 position = agent.FlagController.FlagWgo.Data.Position;
			Vector2 vector = (onGraph - position).XZ2();
			if (vector.magnitude > followDistance)
			{
				Vector3 vector2 = position + vector.normalized.XZ() * followDistance;
				NNInfo nearest2 = recastGraph.GetNearest(vector2, walkable);
				if (nearest2.node == null || !nearest2.node.Walkable)
				{
					return false;
				}
				if ((nearest2.position - vector2).XZ().sqrMagnitude > 0.25f)
				{
					return false;
				}
				onGraph = nearest2.position;
				node = nearest2.node;
			}
		}
		return true;
	}

	private bool IsKitePointReachable(FightingAgent agent, GraphNode destNode)
	{
		if (destNode == null || (UnityEngine.Object)(object)AstarPath.active == null)
		{
			return false;
		}
		RecastGraph recastGraph = base.FightingGameController?.RecastGraph;
		if (recastGraph == null)
		{
			return false;
		}
		NearestNodeConstraint walkable = NearestNodeConstraint.Walkable;
		walkable.graphMask = GraphMask.FromGraph(recastGraph);
		walkable.distanceMetric = DistanceMetric.ClosestAsSeenFromAbove();
		NNInfo nearest = recastGraph.GetNearest(agent.Wgo.Data.Position, walkable);
		if (nearest.node == null || !nearest.node.Walkable)
		{
			return false;
		}
		return PathUtilities.IsPathPossible(nearest.node, destNode);
	}

	private bool IsArcherKiteSlotOccupied(AllyDecisionContext context, Vector3 candidate)
	{
		IReadOnlyList<FightingAgent> readOnlyList = context.FlagController?.AgentsController?.Agents;
		if (readOnlyList == null)
		{
			return false;
		}
		SGuid uniqueId = context.Agent.Wgo.Data.UniqueId;
		float num = 0.80999994f;
		float num2 = 0.45562494f;
		for (int i = 0; i < readOnlyList.Count; i++)
		{
			FightingAgent fightingAgent = readOnlyList[i];
			if (!(fightingAgent == null) && !(fightingAgent.Wgo == null) && !(fightingAgent.Wgo.Data.UniqueId == uniqueId) && IsArcher(fightingAgent))
			{
				if (fightingAgent.MobCommand is MobCommandGoTo { DestinationModifier: not null } mobCommandGoTo && (mobCommandGoTo.DestinationModifier.GetCurrentTargetPosition() - candidate).XZ().sqrMagnitude < num)
				{
					return true;
				}
				if ((fightingAgent.Wgo.Data.Position - candidate).XZ().sqrMagnitude < num2)
				{
					return true;
				}
			}
		}
		return false;
	}

	private MobCommand TryChaseEnemy(AllyDecisionContext context, ICombatEntity enemy, bool defendCapturePoint = false)
	{
		if (DistToEnemy(context.Agent, enemy) <= context.AttackRange)
		{
			return null;
		}
		if (!defendCapturePoint && !IsAgentInsideRangeFlag(context.Agent))
		{
			return null;
		}
		if (!defendCapturePoint && IsAgentInsideEpsilonZoneFlag(context.Agent) && IsEnemyOutwardBeyondFlagRim(context, enemy))
		{
			return null;
		}
		GoToDestinationModifier goToDestinationModifier = context.Agent.GoToDestinationModifier ?? new CombatEntityDestinationModifier();
		float offset = ((goToDestinationModifier.CustomDestinationOffset > 0f) ? goToDestinationModifier.CustomDestinationOffset : context.AttackRange);
		return new MobCommandGoTo(goToDestinationModifier).WithCustomTargetDestinationOffset(offset).WithCustomStopCondition(() => ShouldStopFlagBoundCombat(context, defendCapturePoint)).ToTarget(enemy);
	}

	private bool IsEnemyOutwardBeyondFlagRim(AllyDecisionContext context, ICombatEntity enemy)
	{
		if (!context.HasValidFlag)
		{
			return true;
		}
		Vector3 position = context.FlagController.FlagWgo.Data.Position;
		Vector2 vector = (context.Agent.Wgo.Data.Position - position).XZ2();
		Vector2 vector2 = (enemy.CombatEntityPosition - position).XZ2();
		if (vector2.magnitude < context.FollowDistance - 0.4f)
		{
			return false;
		}
		if (vector.sqrMagnitude <= 0.0001f || vector2.sqrMagnitude <= 0.0001f)
		{
			return true;
		}
		return Vector2.Dot(vector.normalized, vector2.normalized) > 0.25f;
	}

	private MobCommand TryAttackEnemy(AllyDecisionContext context, ICombatEntity enemy, bool defendCapturePoint = false)
	{
		if (DistToEnemy(context.Agent, enemy) >= context.AttackRange)
		{
			return null;
		}
		return context.Agent.AttackCommandType switch
		{
			MobCommand.CommandType.ZombieMeleeAttack => new ZombieMeleeAttackCommand(context.AttackRange).WithCustomStopCondition(() => ShouldStopFlagBoundCombat(context, defendCapturePoint)).ToTarget(enemy), 
			MobCommand.CommandType.ZombieBowAttack => new ZombieBowAttackCommand().WithCustomStopCondition(() => ShouldStopFlagBoundCombat(context, defendCapturePoint)).ToTarget(enemy), 
			MobCommand.CommandType.ZombiePikeAttack => new ZombiePikeAttackCommand().WithCustomStopCondition(() => ShouldStopFlagBoundCombat(context, defendCapturePoint)).ToTarget(enemy), 
			_ => null, 
		};
	}

	private bool ShouldStopFlagBoundCombat(AllyDecisionContext context, bool defendCapturePoint = false)
	{
		if (!context.HasValidFlag)
		{
			return true;
		}
		if (defendCapturePoint)
		{
			FightingCapturePoint nearestAllyCapturePoint = GetNearestAllyCapturePoint(context.Agent);
			ICombatEntity combatEntity = ((nearestAllyCapturePoint != null) ? GetClosestEnemyOnCapturePoint(nearestAllyCapturePoint, context.Agent) : null);
			if (combatEntity == null)
			{
				return true;
			}
			return DistToEnemy(context.Agent, combatEntity) > context.DeAggroDistance;
		}
		if (!context.FlagController.IsSetAtPoint)
		{
			return !IsAgentInsideRangeFlag(context.Agent);
		}
		return false;
	}

	private MobCommand TryGetFlagFollowCommand(AllyDecisionContext context)
	{
		if (context.Agent.IsUnderMainHeroPush)
		{
			return null;
		}
		if (!context.HasValidFlag)
		{
			return null;
		}
		if (context.FlagController.IsSetAtPoint)
		{
			return null;
		}
		if (HasEnemyInAttackRange(context))
		{
			return null;
		}
		if (!TryDistToFlag(context.Agent, out var dist) || dist <= context.FollowDistance)
		{
			return null;
		}
		return new MobCommandGoTo(new CombatEntityDestinationModifier()).WithCustomTargetDestinationOffset(context.FollowDistance).ToTarget(context.FlagController.FlagWgo);
	}

	private MobCommand TryGetFlagDockingCommand(AllyDecisionContext context)
	{
		if (!context.FlagController || !context.FlagController.IsSetAtPoint)
		{
			return null;
		}
		if (context.Agent.IsUnderMainHeroPush)
		{
			return null;
		}
		if (context.Agent.IsAnchoredAtDockPoint)
		{
			return null;
		}
		return DoDockToFlagPlacedObjectLogic(context);
	}

	private MobCommand DoDockToFlagPlacedObjectLogic(AllyDecisionContext context)
	{
		FightingAgent agent = context.Agent;
		Wgo wgoViewGlobal = GameScene.GetWgoViewGlobal(agent.FlagController.AttachedSGuid);
		DockPointTag agentTagToLookFor = ((agent.Settings.useDockPointPrioritizationByWeapon && (bool)agent.Weapon) ? agent.FighterDef.TargetFilterDockPointTag(agent.Wgo.Data) : DockPointTag.None);
		bool hasTagInFlagTargetObj = false;
		if (agentTagToLookFor != 0)
		{
			foreach (DockPointData dockPoint2 in wgoViewGlobal.Data.MainWgoPartData.GetDockPoints(DockPointData.Availability.OnlyNotOccupied, DockPointData.Filter.OnlyNotZombie))
			{
				if (dockPoint2.BakedData.DockPointTag == agentTagToLookFor)
				{
					hasTagInFlagTargetObj = true;
					break;
				}
			}
		}
		if ((bool)wgoViewGlobal && TryGetDockPoint(agent, wgoViewGlobal, out var dockPoint, DockPointData.Availability.OnlyNotOccupied, DockPointData.Filter.OnlyNotZombie, getRandomInsteadOfNearest: false, (DockPointData data, Vector3 position) => AllyDockPointCheck(hasTagInFlagTargetObj ? agentTagToLookFor : DockPointTag.None, data, position)))
		{
			if (SGuid.IsNullOrEmpty(agent.Wgo.Data.takenDockPointsParentSGuid))
			{
				dockPoint.Occupy(agent.Wgo.Data.UniqueId);
				agent.Wgo.Data.takenDockPointsParentSGuid = wgoViewGlobal.Data.UniqueId;
				return new MobCommandGoTo(new CombatEntityDestinationModifier(dockPoint)).WithCustomTargetDestinationOffset(0.06666668f).WithCustomActionOnDestReached(delegate
				{
					OnAgentDocked(agent, dockPoint);
				}).ToTarget(wgoViewGlobal);
			}
			float magnitude = (wgoViewGlobal.Data.GetDockPointDataWorldPosition(dockPoint) - agent.Wgo.Data.Position).XZ().magnitude;
			if (magnitude < 0.06666668f || (magnitude - 0.06666668f).EqualsTo(0f, 0.0001f))
			{
				OnAgentDocked(agent, dockPoint);
				return null;
			}
			return new MobCommandGoTo(new CombatEntityDestinationModifier(dockPoint)).WithCustomTargetDestinationOffset(0.06666668f).WithCustomActionOnDestReached(delegate
			{
				OnAgentDocked(agent, dockPoint);
			}).ToTarget(wgoViewGlobal);
		}
		return null;
	}

	private static void OnAgentDocked(FightingAgent agent, DockPointData dockPoint)
	{
		agent.SetFacingDirection(dockPoint.Direction.ConvertToVector2XZ(), instant: true);
		agent.IsAnchoredAtDockPoint = true;
		agent.SetNavmeshCutActive(active: true);
	}

	private bool AllyDockPointCheck(DockPointTag tag, DockPointData data, Vector3 parentPos)
	{
		if (tag != 0 && data.BakedData.DockPointTag != tag)
		{
			return false;
		}
		return true;
	}

	private bool IsAgentInsideEpsilonZoneFlag(FightingAgent agent)
	{
		if (!TryDistToFlag(agent, out var dist))
		{
			return false;
		}
		if (dist < followDistance)
		{
			return dist > followDistance - 0.4f;
		}
		return false;
	}

	private float DistToPos(FightingAgent agent, Vector3 pos)
	{
		return (agent.Wgo.Data.Position - pos).XZ().magnitude;
	}

	private bool IsOnCapturePoint(Vector3 pos, FightingCapturePoint point)
	{
		if ((bool)point)
		{
			return (pos - point.transform.position).XZ().magnitude < point.Radius;
		}
		return false;
	}

	private FightingCapturePoint GetNearestAllyCapturePoint(FightingAgent agent)
	{
		FightingCapturePoint fightingCapturePoint = agent.FlagController?.CapturePoint;
		if ((bool)fightingCapturePoint)
		{
			return fightingCapturePoint;
		}
		return (agent.ParentController?.FightingLine)?.FindNearestEnemySectorBy(LazyConsts.Fighting.TeamType.Player)?.point;
	}

	private MobCommand TryGetCapturePointCommand(AllyDecisionContext context)
	{
		if (context.Agent.IsUnderMainHeroPush)
		{
			return null;
		}
		if (context.FlagController.IsSetAtPoint)
		{
			return null;
		}
		FightingCapturePoint capturePoint = GetNearestAllyCapturePoint(context.Agent);
		if (!capturePoint || capturePoint.LockedForCapture)
		{
			return null;
		}
		if (HasEnemyOnCapturePoint(capturePoint))
		{
			return null;
		}
		if (!ShouldAllyContestCapturePoint(capturePoint))
		{
			return null;
		}
		FightingAgent agent = context.Agent;
		bool flag = IsOnCapturePoint(agent.Wgo.Data.Position, capturePoint);
		if (agent.MobCommand is MobCommandGoTo { DestinationModifier: AllyCapturePointDestinationModifier destinationModifier })
		{
			if (!(DistToPos(agent, destinationModifier.GetCurrentTargetPosition()) < 0.1f))
			{
				return null;
			}
			flag = true;
		}
		if (!flag)
		{
			return new MobCommandGoTo(new AllyCapturePointDestinationModifier(capturePoint)).WithCustomStopCondition(() => ShouldStopAllyCaptureGoTo(context, capturePoint), retargetDeltaTime, UnityEngine.Random.Range(0f, retargetDeltaTime)).ToPosition(capturePoint.transform.position);
		}
		return new MobCommandFlagCapture(capturePoint).WithCustomStopCondition(() => ShouldStopAllyFlagCapture(context, capturePoint));
	}

	private bool ShouldStopAllyCaptureGoTo(AllyDecisionContext context, FightingCapturePoint capturePoint)
	{
		if (capturePoint == null)
		{
			return true;
		}
		if (IsAllyCaptureContestComplete(capturePoint))
		{
			return true;
		}
		return HasEnemyOnCapturePoint(capturePoint);
	}

	private bool ShouldStopAllyFlagCapture(AllyDecisionContext context, FightingCapturePoint capturePoint)
	{
		if (capturePoint == null)
		{
			return true;
		}
		if (IsAllyCaptureContestComplete(capturePoint))
		{
			return true;
		}
		if (!IsOnCapturePoint(context.Agent.Wgo.Data.Position, capturePoint))
		{
			return true;
		}
		return HasEnemyOnCapturePoint(capturePoint);
	}

	private static bool ShouldAllyContestCapturePoint(FightingCapturePoint capturePoint)
	{
		if (!capturePoint)
		{
			return false;
		}
		if (capturePoint.OwnedByTeam != 0)
		{
			return true;
		}
		return !capturePoint.CurrentProgress.EqualsOrMore(1f);
	}

	private static bool IsAllyCaptureContestComplete(FightingCapturePoint capturePoint)
	{
		if (capturePoint != null && capturePoint.OwnedByTeam == LazyConsts.Fighting.TeamType.Player)
		{
			return capturePoint.CurrentProgress.EqualsOrMore(1f);
		}
		return false;
	}

	private static bool HasEnemyOnCapturePoint(FightingCapturePoint capturePoint)
	{
		if (!capturePoint)
		{
			return false;
		}
		for (int i = 0; i < capturePoint.enemies.Count; i++)
		{
			ICombatEntity combatEntity = capturePoint.enemies[i];
			if (combatEntity != null && combatEntity.IsActiveCombatant)
			{
				return true;
			}
		}
		return false;
	}

	private static ICombatEntity GetClosestEnemyOnCapturePoint(FightingCapturePoint capturePoint, FightingAgent agent)
	{
		if (!capturePoint || capturePoint.enemies.Count == 0)
		{
			return null;
		}
		Vector3 position = agent.Wgo.Data.Position;
		ICombatEntity result = null;
		float num = float.MaxValue;
		for (int i = 0; i < capturePoint.enemies.Count; i++)
		{
			ICombatEntity combatEntity = capturePoint.enemies[i];
			if (combatEntity != null && combatEntity.IsActiveCombatant)
			{
				float sqrMagnitude = (position - combatEntity.CombatEntityPosition).XZ().sqrMagnitude;
				if (!(sqrMagnitude >= num))
				{
					num = sqrMagnitude;
					result = combatEntity;
				}
			}
		}
		return result;
	}
}
