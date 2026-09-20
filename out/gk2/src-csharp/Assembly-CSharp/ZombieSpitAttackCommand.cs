using LazyBearTechnology;
using UnityEngine;
using UnityEngine.Events;

public class ZombieSpitAttackCommand : ZombieAttackCommand
{
	private const float SPIT_EMIT_FALLBACK_DELAY = 0.75f;

	private float attackRange;

	private bool isAttackAnimPlaying;

	private bool spitEmitted;

	private float attackAnimStartTime;

	private bool debugLogs;

	public ZombieSpitAttackCommand(float attackRange)
		: base(CommandType.ZombieSpitAttack)
	{
		this.attackRange = attackRange;
	}

	public override void OnStart()
	{
		base.OnStart();
		SetFacingDirection(DirectionToTarget);
		AnimationEventReceiver animationEventReceiver = base.Wgo.MainWgoPart?.AnimationComponent?.AnimationEventReceiver;
		if (animationEventReceiver == null)
		{
			agent.StopCommandExecution(reportAlsoAsCompletion: true);
			return;
		}
		if (animationEventReceiver.onEvent4 == null)
		{
			animationEventReceiver.onEvent4 = new UnityEvent();
		}
		if (animationEventReceiver.onEvent7 == null)
		{
			animationEventReceiver.onEvent7 = new UnityEvent();
		}
		ClearAnimationEventListeners();
	}

	public override void OnUpdate(float deltaTime)
	{
		base.OnUpdate(deltaTime);
		if (base.TargetEntity == null || base.TargetEntity.CombatEntityHpComponent.Hp <= 0)
		{
			StopAttackAndCommand();
			return;
		}
		if ((base.Wgo.Data.Position - base.TargetEntity.CombatEntityPosition).XZ().magnitude > attackRange + 0.06666668f)
		{
			StopAttackAndCommand();
			return;
		}
		Transform transform = (agent.AttackComponent.weapon ? agent.AttackComponent.weapon.transform : agent.transform);
		if (!FightingWgoTarget.IsBarricadeOrTower(base.TargetEntity) && !AgentAI.TryLineCastByRecast(transform.position, base.TargetEntity.CombatEntityPosition + Vector3.up * 0.5f))
		{
			StopAttackAndCommand();
		}
		else if (isAttackAnimPlaying)
		{
			if (!spitEmitted && Time.time - attackAnimStartTime >= 0.75f)
			{
				EmitSpit();
			}
		}
		else if (customStopCondition != null && customStopCondition())
		{
			agent.StopCommandExecution(reportAlsoAsCompletion: true);
		}
		else if (!ShouldPause(deltaTime))
		{
			DoAttack();
		}
	}

	public override void OnFinish()
	{
		base.OnFinish();
		ClearAnimationEventListeners();
		isAttackAnimPlaying = false;
		spitEmitted = false;
		base.Wgo.MainWgoPart?.AnimationComponent?.SetState(AnimationState.Idle);
	}

	public ZombieSpitAttackCommand WithDebugLogs(bool enabled)
	{
		debugLogs = enabled;
		return this;
	}

	private void DoAttack()
	{
		if (!agent.AttackComponent.weapon)
		{
			agent.StopCommandExecution(reportAlsoAsCompletion: true);
			return;
		}
		isAttackAnimPlaying = true;
		spitEmitted = false;
		attackAnimStartTime = Time.time;
		agent.AttackComponent.PerformAttack(useCustomDirection: false, default(Vector3), useAnimationFromWeapon: true, delegate
		{
			if (isAttackAnimPlaying)
			{
				isAttackAnimPlaying = false;
				agent.StopCommandExecution(reportAlsoAsCompletion: true);
			}
		}, activateWeapon: false);
		LazyAudio.PlayAtGameObject("spitter_attack", base.Wgo.transform, SpatialType.sound3D);
		AnimationEventReceiver animationEventReceiver = base.Wgo.MainWgoPart.AnimationComponent.AnimationEventReceiver;
		animationEventReceiver.onEvent4.AddListener(EmitSpit);
		animationEventReceiver.onEvent7.AddListener(HandleAnimationFinish);
	}

	private void EmitSpit()
	{
		if (spitEmitted)
		{
			return;
		}
		spitEmitted = true;
		(base.Wgo.MainWgoPart?.AnimationComponent?.AnimationEventReceiver)?.onEvent4?.RemoveListener(EmitSpit);
		if (agent.AttackComponent.weapon == null)
		{
			agent.StopCommandExecution(reportAlsoAsCompletion: true);
		}
		else if (base.TargetEntity != null && base.TargetEntity.CombatEntityHpComponent.Hp > 0)
		{
			float num = 0.8333335f;
			if (FightingWgoTarget.IsBarricadeOrTower(base.TargetEntity))
			{
				num *= 0.5f;
			}
			Vector3 normalized = (base.TargetEntity.CombatEntityPosition + Vector3.up * num - agent.AttackComponent.weapon.transform.position).normalized;
			if (!(normalized.sqrMagnitude <= 0f))
			{
				agent.AttackComponent.ActivateWeapon(normalized);
			}
		}
	}

	private void HandleAnimationFinish()
	{
		if (isAttackAnimPlaying)
		{
			base.Wgo.MainWgoPart.AnimationComponent.AnimationEventReceiver.onEvent7.RemoveListener(HandleAnimationFinish);
			agent.AttackComponent.OnAttackAnimFinished(base.Wgo.MainWgoPart.AnimationComponent);
		}
	}

	private void StopAttackAndCommand()
	{
		if (isAttackAnimPlaying)
		{
			isAttackAnimPlaying = false;
			spitEmitted = false;
			agent.AttackComponent.CancelAttack();
		}
		ClearAnimationEventListeners();
		agent.StopCommandExecution(reportAlsoAsCompletion: true);
	}

	private void ClearAnimationEventListeners()
	{
		AnimationEventReceiver animationEventReceiver = base.Wgo.MainWgoPart?.AnimationComponent?.AnimationEventReceiver;
		if (!(animationEventReceiver == null))
		{
			animationEventReceiver.onEvent4?.RemoveListener(EmitSpit);
			animationEventReceiver.onEvent7?.RemoveListener(HandleAnimationFinish);
		}
	}
}
