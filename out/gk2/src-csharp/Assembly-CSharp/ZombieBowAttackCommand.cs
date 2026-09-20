using LazyBearTechnology;
using UnityEngine;
using UnityEngine.Events;

public class ZombieBowAttackCommand : ZombieAttackCommand
{
	private float attackRange;

	private bool isAttackAnimPlaying;

	public ZombieBowAttackCommand()
		: base(CommandType.ZombieBowAttack)
	{
	}

	public override void OnStart()
	{
		base.OnStart();
		attackRange = agent.FighterDef.atkRange.EvaluateInt(agent.Wgo);
		SetDirectionToTarget();
		if (base.Wgo.MainWgoPart.AnimationComponent.AnimationEventReceiver.onEvent4 == null)
		{
			base.Wgo.MainWgoPart.AnimationComponent.AnimationEventReceiver.onEvent4 = new UnityEvent();
		}
		if (base.Wgo.MainWgoPart.AnimationComponent.AnimationEventReceiver.onEvent7 == null)
		{
			base.Wgo.MainWgoPart.AnimationComponent.AnimationEventReceiver.onEvent7 = new UnityEvent();
		}
	}

	public override void OnUpdate(float deltaTime)
	{
		base.OnUpdate(deltaTime);
		if (base.TargetEntity != null && (bool)agent.AttackComponent.weapon && !AgentAI.TryLineCastByRecast(agent.AttackComponent.weapon.transform.position, base.TargetEntity.CombatEntityPosition))
		{
			agent.StopCommandExecution(reportAlsoAsCompletion: true);
			if (isAttackAnimPlaying)
			{
				isAttackAnimPlaying = false;
				agent.AttackComponent.CancelAttack();
			}
		}
		else if (base.TargetEntity == null || base.TargetEntity.CombatEntityHpComponent.Hp <= 0)
		{
			if (isAttackAnimPlaying)
			{
				isAttackAnimPlaying = false;
				agent.AttackComponent.CancelAttack();
			}
			agent.StopCommandExecution(reportAlsoAsCompletion: true);
		}
		else if (!isAttackAnimPlaying)
		{
			if (customStopCondition != null && customStopCondition())
			{
				agent.StopCommandExecution(reportAlsoAsCompletion: true);
			}
			else if ((base.Wgo.Data.Position - base.Position).XZ2().magnitude > attackRange)
			{
				agent.StopCommandExecution(reportAlsoAsCompletion: true);
			}
			else if (!ShouldPause(deltaTime) && !isAttackAnimPlaying)
			{
				DoAttack();
			}
		}
	}

	public override void OnFinish()
	{
		base.OnFinish();
		base.Wgo.MainWgoPart?.AnimationComponent?.CancelBowAimLoop();
		isAttackAnimPlaying = false;
		base.Wgo.MainWgoPart?.AnimationComponent?.SetState(AnimationState.Idle);
		base.Wgo.MainWgoPart?.AnimationComponent?.AnimationEventReceiver?.onEvent4?.RemoveAllListeners();
		base.Wgo.MainWgoPart?.AnimationComponent?.AnimationEventReceiver?.onEvent7?.RemoveAllListeners();
	}

	private Vector3 GetAimPosition(Vector3 targetPosition)
	{
		return targetPosition + Vector3.up * 1.666667f / 2f;
	}

	private void DoAttack()
	{
		isAttackAnimPlaying = true;
		agent.AttackComponent.PerformAttack(useCustomDirection: false, default(Vector3), useAnimationFromWeapon: true, delegate
		{
			if (isAttackAnimPlaying)
			{
				isAttackAnimPlaying = false;
				agent.StopCommandExecution(reportAlsoAsCompletion: true);
			}
		}, activateWeapon: false);
		base.Wgo.MainWgoPart.AnimationComponent.AnimationEventReceiver.onEvent4.AddListener(EmitArrow);
		base.Wgo.MainWgoPart.AnimationComponent.AnimationEventReceiver.onEvent7.AddListener(HandleAnimationFinish);
	}

	private void HandleAnimationFinish()
	{
		agent.AttackComponent.OnAttackAnimFinished(base.Wgo.MainWgoPart.AnimationComponent);
		base.Wgo.MainWgoPart.AnimationComponent.AnimationEventReceiver.onEvent7.RemoveAllListeners();
	}

	private void EmitArrow()
	{
		base.Wgo.MainWgoPart.AnimationComponent.AnimationEventReceiver.onEvent4.RemoveAllListeners();
		BowWeapon bowWeapon = agent.AttackComponent.weapon as BowWeapon;
		if (bowWeapon == null)
		{
			agent.StopCommandExecution();
		}
		else if (base.TargetEntity != null && base.TargetEntity.CombatEntityHpComponent.Hp > 0)
		{
			Vector3 normalized = (GetAimPosition(base.TargetEntity.CombatEntityPosition) - bowWeapon.transform.position).normalized;
			LazyAudio.PlayAtGameObject("bow_aim_shot", agent.transform, SpatialType.sound3D);
			agent.AttackComponent.ActivateWeapon(normalized);
		}
	}
}
