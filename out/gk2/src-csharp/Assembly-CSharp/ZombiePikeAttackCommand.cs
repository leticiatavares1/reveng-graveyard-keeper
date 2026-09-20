using UnityEngine;

public class ZombiePikeAttackCommand : ZombieAttackCommand
{
	private float attackRange = 1f;

	private bool isAttackAnimPlaying;

	public ZombiePikeAttackCommand()
		: base(CommandType.ZombiePikeAttack)
	{
	}

	public override void OnStart()
	{
		base.OnStart();
		attackRange = agent.FighterDef.atkRange.EvaluateInt(agent.Wgo);
		SetFacingDirection((base.Position - base.Wgo.Data.Position).XZ2());
	}

	public override void OnUpdate(float deltaTime)
	{
		base.OnUpdate(deltaTime);
		if (base.TargetEntity == null || base.TargetEntity.CombatEntityHpComponent.Hp <= 0)
		{
			agent.StopCommandExecution(reportAlsoAsCompletion: true);
		}
		else if (!isAttackAnimPlaying)
		{
			if (customStopCondition != null && customStopCondition())
			{
				agent.StopCommandExecution(reportAlsoAsCompletion: true);
			}
			else if ((base.Wgo.Data.Position - base.Position).XZ2().magnitude > attackRange + 0.06666668f)
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
		base.Wgo.MainWgoPart?.AnimationComponent?.SetState(AnimationState.Idle);
	}

	private void DoAttack()
	{
		isAttackAnimPlaying = true;
		SetFacingDirection((base.Position - base.Wgo.Data.Position).XZ2());
		agent.AttackComponent.PerformAttack(useCustomDirection: false, default(Vector3), useAnimationFromWeapon: true, delegate
		{
			isAttackAnimPlaying = false;
			agent.StopCommandExecution(reportAlsoAsCompletion: true);
		});
	}
}
