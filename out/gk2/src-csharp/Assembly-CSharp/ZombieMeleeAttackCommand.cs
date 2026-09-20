using LazyBearTechnology;
using UnityEngine;

public class ZombieMeleeAttackCommand : ZombieAttackCommand
{
	private const float HitOffset = 0.6f;

	private const float HitHeight = 0.5f;

	private readonly float attackRange;

	private DockPointData customDockPoint;

	private int damage = 1;

	private bool hasDamage = true;

	private bool isAttackAnimPlaying;

	public DockPointData CustomDockPoint => customDockPoint;

	public override Vector2 DirectionToTarget
	{
		get
		{
			if (customDockPoint == null)
			{
				return base.DirectionToTarget;
			}
			return customDockPoint.Direction.ConvertToVector2XZ().normalized;
		}
	}

	public ZombieMeleeAttackCommand(float attackRange)
		: base(CommandType.ZombieMeleeAttack)
	{
		this.attackRange = attackRange;
	}

	public override void OnStart()
	{
		base.OnStart();
		SetFacingDirection(DirectionToTarget);
	}

	public override void OnUpdate(float deltaTime)
	{
		base.OnUpdate(deltaTime);
		hasDamage = true;
		if (!(base.TargetEntity is Object @object) || @object == null || base.TargetEntity.CombatEntityHpComponent.Hp <= 0)
		{
			agent.StopCommandExecution(reportAlsoAsCompletion: true);
			return;
		}
		if (customStopCondition != null && customStopCondition())
		{
			agent.StopCommandExecution(reportAlsoAsCompletion: true);
			return;
		}
		Vector3 pos = (base.TargetEntity as Wgo)?.Data.Position ?? base.Position;
		Vector3 vector = customDockPoint?.GetPosFrom(pos) ?? base.Position;
		if ((base.Wgo.Data.Position - vector).XZ2().magnitude > attackRange + 0.06666668f)
		{
			if (isAttackAnimPlaying)
			{
				hasDamage = false;
			}
			else
			{
				agent.StopCommandExecution(reportAlsoAsCompletion: true);
			}
		}
		else if (!isAttackAnimPlaying && !ShouldPause(deltaTime) && !isAttackAnimPlaying)
		{
			DoAttack();
		}
	}

	public override void OnFinish()
	{
		base.OnFinish();
		base.Wgo.MainWgoPart?.AnimationComponent?.SetState(AnimationState.Idle);
	}

	public void HandleAttackHit()
	{
		if (!(base.TargetEntity is Object @object) || @object == null)
		{
			agent.StopCommandExecution(reportAlsoAsCompletion: true);
			return;
		}
		if (base.TargetEntity.CombatEntityHpComponent.Hp <= 0)
		{
			agent.StopCommandExecution(reportAlsoAsCompletion: true);
			return;
		}
		if (hasDamage && FightingWgoTarget.IsBarricadeOrTower(base.TargetEntity))
		{
			LazyAudio.PlayAtGameObject("zombie_hit_wood", agent.Wgo.transform, SpatialType.sound3D);
		}
		damage = (hasDamage ? Mathf.Clamp(damage - base.TargetEntity.ArmorValue, 0, int.MaxValue) : 0);
		if (damage != 0)
		{
			if (base.TargetEntity is PlayerPhysicalBody)
			{
				LazyAudio.PlayAtGameObject("zombie_hit_player", agent.Wgo.transform, SpatialType.sound3D);
			}
			if (base.TargetEntity.IsActiveCombatant)
			{
				base.TargetEntity.CombatEntityHpComponent.ApplyDamage(damage);
			}
			Vector3 attackHitPosition = GetAttackHitPosition();
			Vector3 vector = agent.Wgo.Data.direction.Value.XZ();
			DamageEffectComponent.TryPlayEffect(base.TargetEntity.CombatEntityUID, attackHitPosition, vector);
			AttackContext ctx = new AttackContext(agent.Wgo, agent.Wgo.TeamType, agent.FighterDef, agent.Weapon?.ItemDef, agent.Wgo.CombatEntityPosition, vector, attackHitPosition, -1, isReturnDamage: false, agent.AttackComponent);
			base.TargetEntity.OnOtherCombatTargetHitMe(ctx);
		}
	}

	private Vector3 GetAttackHitPosition()
	{
		Vector3 combatEntityPosition = agent.Wgo.CombatEntityPosition;
		Vector3 vector = (GetAttackHitTargetPoint() - combatEntityPosition).XZ();
		float magnitude = vector.magnitude;
		if (magnitude < 0.0001f)
		{
			return combatEntityPosition + Vector3.up * 0.5f;
		}
		float num = ((customDockPoint != null) ? magnitude : Mathf.Min(0.6f, magnitude));
		return combatEntityPosition + vector / magnitude * num + Vector3.up * 0.5f;
	}

	private Vector3 GetAttackHitTargetPoint()
	{
		if (customDockPoint != null && base.TargetEntity is Wgo wgo)
		{
			return wgo.Data.GetDockPointDataWorldPosition(customDockPoint) + customDockPoint.Direction.ConvertToVector3() * 0.6f;
		}
		return base.TargetEntity.CombatEntityPosition;
	}

	private void DoAttack()
	{
		isAttackAnimPlaying = true;
		agent.AttackComponent.PerformAttackMelee(delegate
		{
			isAttackAnimPlaying = false;
			agent.StopCommandExecution(reportAlsoAsCompletion: true);
		});
	}

	public ZombieMeleeAttackCommand WithCustomDockPoint(DockPointData dockPoint)
	{
		customDockPoint = dockPoint;
		return this;
	}

	public ZombieMeleeAttackCommand WithDamage(int damage)
	{
		this.damage = damage;
		return this;
	}
}
