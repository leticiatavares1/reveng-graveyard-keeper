using System;
using System.Collections.Generic;
using LazyBearTechnology;
using LinqTools;
using UnityEngine;

[CreateAssetMenu(menuName = "GK2/Fighting/AIs/EnemyLeglessZombieJump")]
public class EnemyLeglessZombieJumpAI : EnemyWithDoorAI
{
	private struct LandingReservation
	{
		public Vector3 Position;

		public float ExpiresAt;
	}

	[Header("Jump Limits")]
	public int maxJumpCount = 1;

	[Header("Jump")]
	[Tooltip("Minimum distance to target required to start jump. Negative value disables this check.")]
	public float minJumpDistance = -1f;

	public float lookingForJumpSearchRadius = 2.5f;

	public float jumpCooldown = 3f;

	public float jumpDuration = 0.35f;

	public float jumpArcHeight = 0.8f;

	[Header("Landing Search")]
	[Tooltip("Distance from the target to offset the landing search center. Helps the agent land further away.")]
	public float landingOffsetFromTarget = 1f;

	public float landingLookDistance = 1.5f;

	public float landingCapsuleRadius = 0.35f;

	public float landingCapsuleHeight = 1.4f;

	[Header("Landing Reservation")]
	public float reservedLandingRadius = 0.6f;

	public float reservedLandingLifetime = 1.2f;

	[Tooltip("Predefined jump height curves in normalized time [0..1]. One random curve is selected for each jump.")]
	public List<AnimationCurve> jumpHeightCurves = new List<AnimationCurve>();

	private readonly Dictionary<SGuid, float> jumpCooldownByAgent = new Dictionary<SGuid, float>();

	private readonly Dictionary<SGuid, int> jumpCountByAgent = new Dictionary<SGuid, int>();

	private readonly Dictionary<SGuid, LandingReservation> landingReservationsByAgent = new Dictionary<SGuid, LandingReservation>();

	private readonly List<SGuid> reservationKeysToRemove = new List<SGuid>();

	public override MobCommand GetCommand(FightingAgent agent, Func<IEnumerable<ICombatEntity>> targets)
	{
		if (TryCreateJumpCommand(agent, targets, out var command))
		{
			return command;
		}
		MobCommand command2 = base.GetCommand(agent, targets);
		if (command2 is MobCommandGoTo mobCommandGoTo)
		{
			mobCommandGoTo.WithCustomStopCondition(() => ShouldInterruptGoToForJump(agent, targets), retargetDeltaTime, UnityEngine.Random.Range(0f, retargetDeltaTime));
		}
		return command2;
	}

	protected override EnemyDecisionContext BuildContext(FightingAgent agent, Func<IEnumerable<ICombatEntity>> targets)
	{
		FightingCapturePoint capturePoint = null;
		FightingLine fightingLine = (agent.ParentController ? agent.ParentController.FightingLine : null);
		if (fightingLine != null)
		{
			capturePoint = fightingLine.FindNearestEnemySectorBy(LazyConsts.Fighting.TeamType.WildZombie)?.point;
		}
		if (!capturePoint && LazySingleton<FightingGameController>.Instance != null && LazySingleton<FightingGameController>.Instance.CurrentLevel != null)
		{
			capturePoint = LazySingleton<FightingGameController>.Instance.CurrentLevel.BaseCapturePoint;
		}
		if (capturePoint != null && (agent.Wgo.Data.Position - capturePoint.transform.position).XZ().magnitude < capturePoint.Radius)
		{
			Func<IEnumerable<ICombatEntity>> targets2 = () => from t in targets()
				where (t.CombatEntityPosition - capturePoint.transform.position).XZ().magnitude < capturePoint.Radius
				select t;
			return new EnemyDecisionContext(this, agent, WrapPotentialTargets(agent, targets2), aggroDistance);
		}
		return base.BuildContext(agent, WrapPotentialTargets(agent, targets));
	}

	private bool TryCreateJumpCommand(FightingAgent agent, Func<IEnumerable<ICombatEntity>> targets, out MobCommand command)
	{
		command = null;
		if (agent == null || targets == null)
		{
			return false;
		}
		SGuid uniqueId = agent.Wgo.Data.UniqueId;
		jumpCountByAgent.TryGetValue(uniqueId, out var value);
		if (value >= maxJumpCount)
		{
			return false;
		}
		if (IsJumpOnCooldown(agent))
		{
			return false;
		}
		CleanupExpiredLandingReservations();
		if (!TryGetNearestLandableJumpTarget(agent, targets, out var nearestTarget, out var landingPosition))
		{
			StartJumpCooldown(agent);
			return false;
		}
		ReserveLandingPosition(agent, landingPosition);
		StartJumpCooldown(agent);
		jumpCountByAgent[uniqueId] = value + 1;
		command = new LeglessZombieJumpCommand(landingPosition, jumpDuration, jumpArcHeight, GetRandomHeightCurve())
		{
			TargetEntity = nearestTarget,
			customPosition = landingPosition
		};
		return true;
	}

	private bool TryGetNearestLandableJumpTarget(FightingAgent agent, Func<IEnumerable<ICombatEntity>> targets, out ICombatEntity nearestTarget, out Vector3 landingPosition)
	{
		nearestTarget = null;
		landingPosition = default(Vector3);
		IEnumerable<ICombatEntity> enumerable = targets();
		if (enumerable == null)
		{
			return false;
		}
		float num = float.PositiveInfinity;
		Vector3 position = agent.Wgo.Data.Position;
		foreach (ICombatEntity item in enumerable)
		{
			if (item != null && item.CombatEntityHpComponent.Hp > 0 && IsTargetValid(item, agent) && HasNonZeroMovementSpeed(item))
			{
				float combatEntityDistance = item.GetCombatEntityDistance(position, agent.Wgo.TeamType);
				if ((!(minJumpDistance >= 0f) || !(combatEntityDistance < minJumpDistance)) && !(combatEntityDistance > lookingForJumpSearchRadius) && !(combatEntityDistance >= num) && TryFindLandingPosition(agent, item, out var landingPosition2))
				{
					num = combatEntityDistance;
					nearestTarget = item;
					landingPosition = landingPosition2;
				}
			}
		}
		return nearestTarget != null;
	}

	private bool HasNonZeroMovementSpeed(ICombatEntity target)
	{
		if (target is UnityEngine.Object @object && @object is Wgo wgo)
		{
			FighterDef data = GameBalance.Me.GetData<FighterDef>(wgo.Id);
			if (data != null)
			{
				return data.mvtSpeed > 0f;
			}
			return false;
		}
		if (target is UnityEngine.Object object2 && object2 is PlayerPhysicalBody)
		{
			return true;
		}
		return false;
	}

	private bool TryFindLandingPosition(FightingAgent agent, ICombatEntity target, out Vector3 landingPosition)
	{
		landingPosition = default(Vector3);
		Vector3 combatEntityPosition = target.CombatEntityPosition;
		Vector2 vector = (combatEntityPosition - agent.Wgo.Data.Position).XZ2().normalized;
		if (vector == Vector2.zero)
		{
			vector = agent.Wgo.Data.direction.Value;
		}
		Vector3 vector2 = combatEntityPosition + new Vector3(vector.x, 0f, vector.y) * landingOffsetFromTarget;
		if (!SpecialPhysicsCastUtils.GetLandPositionByCapsule(vector2, vector, landingLookDistance, landingCapsuleRadius, landingCapsuleHeight, out var foundDropPos, 45f))
		{
			return false;
		}
		if (!IsValidLandingPosition(agent, vector2, foundDropPos))
		{
			return false;
		}
		landingPosition = foundDropPos;
		return true;
	}

	private bool ShouldInterruptGoToForJump(FightingAgent agent, Func<IEnumerable<ICombatEntity>> targets)
	{
		if (agent == null || targets == null)
		{
			return false;
		}
		if (IsJumpOnCooldown(agent))
		{
			return false;
		}
		SGuid uniqueId = agent.Wgo.Data.UniqueId;
		jumpCountByAgent.TryGetValue(uniqueId, out var value);
		if (value >= maxJumpCount)
		{
			return false;
		}
		CleanupExpiredLandingReservations();
		ICombatEntity nearestTarget;
		Vector3 landingPosition;
		return TryGetNearestLandableJumpTarget(agent, targets, out nearestTarget, out landingPosition);
	}

	private bool IsValidLandingPosition(FightingAgent agent, Vector3 searchCenter, Vector3 candidate)
	{
		if ((candidate - searchCenter).XZ().magnitude > landingLookDistance + 0.1f)
		{
			return false;
		}
		if ((candidate - agent.Wgo.Data.Position).XZ().magnitude < 0.2f)
		{
			return false;
		}
		if (IsLandingReservedByAnotherAgent(agent, candidate))
		{
			return false;
		}
		Vector3 halfExtents = new Vector3(agent.Settings.aiPathRadius, 0.9f, agent.Settings.aiPathRadius);
		return !Physics.CheckBox(candidate + Vector3.up * halfExtents.y, halfExtents, Quaternion.identity, 16843009, QueryTriggerInteraction.Ignore);
	}

	private bool IsJumpOnCooldown(FightingAgent agent)
	{
		SGuid uniqueId = agent.Wgo.Data.UniqueId;
		if (!jumpCooldownByAgent.TryGetValue(uniqueId, out var value))
		{
			return false;
		}
		if (Time.time >= value)
		{
			jumpCooldownByAgent.Remove(uniqueId);
			return false;
		}
		return true;
	}

	private void StartJumpCooldown(FightingAgent agent)
	{
		jumpCooldownByAgent[agent.Wgo.Data.UniqueId] = Time.time + jumpCooldown;
	}

	private AnimationCurve GetRandomHeightCurve()
	{
		if (jumpHeightCurves == null || jumpHeightCurves.Count == 0)
		{
			return null;
		}
		for (int num = jumpHeightCurves.Count - 1; num >= 0; num--)
		{
			if (jumpHeightCurves[num] == null)
			{
				jumpHeightCurves.RemoveAt(num);
			}
		}
		if (jumpHeightCurves.Count == 0)
		{
			return null;
		}
		int index = UnityEngine.Random.Range(0, jumpHeightCurves.Count);
		return jumpHeightCurves[index];
	}

	private bool IsLandingReservedByAnotherAgent(FightingAgent agent, Vector3 candidate)
	{
		float num = Mathf.Max(reservedLandingRadius, landingCapsuleRadius);
		float num2 = num * num;
		SGuid uniqueId = agent.Wgo.Data.UniqueId;
		foreach (KeyValuePair<SGuid, LandingReservation> item in landingReservationsByAgent)
		{
			if (!(item.Key == uniqueId) && (item.Value.Position - candidate).XZ().sqrMagnitude <= num2)
			{
				return true;
			}
		}
		return false;
	}

	private void ReserveLandingPosition(FightingAgent agent, Vector3 position)
	{
		float num = Mathf.Max(0.1f, reservedLandingLifetime);
		landingReservationsByAgent[agent.Wgo.Data.UniqueId] = new LandingReservation
		{
			Position = position,
			ExpiresAt = Time.time + num
		};
	}

	private void CleanupExpiredLandingReservations()
	{
		if (landingReservationsByAgent.Count == 0)
		{
			return;
		}
		reservationKeysToRemove.Clear();
		foreach (KeyValuePair<SGuid, LandingReservation> item in landingReservationsByAgent)
		{
			if (Time.time >= item.Value.ExpiresAt)
			{
				reservationKeysToRemove.Add(item.Key);
			}
		}
		for (int i = 0; i < reservationKeysToRemove.Count; i++)
		{
			landingReservationsByAgent.Remove(reservationKeysToRemove[i]);
		}
	}
}
