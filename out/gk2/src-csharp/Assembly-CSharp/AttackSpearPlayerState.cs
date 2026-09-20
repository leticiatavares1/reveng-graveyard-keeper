using LazyBearTechnology;
using UnityEngine;

public class AttackSpearPlayerState : SSMState
{
	private bool isAttacking;

	private StaminaSystem staminaSystem;

	public override bool IsActive => isAttacking;

	public AttackSpearPlayerState(PlayerController playerController)
		: base(playerController)
	{
	}

	public override void OnEnter()
	{
		staminaSystem = MainGame.PlayerData.staminaSystem;
		isAttacking = true;
		playerController.SetControlTakenType(TakenControlType.ByAttack, isEnabled: false);
		playerController.AttackComponent.PerformAttack(useCustomDirection: false, default(Vector3), useAnimationFromWeapon: true, delegate
		{
			isAttacking = false;
		});
	}

	public override void Update()
	{
		if (LazyInput.GetKeyDown(GameKey.Attack) && isAttacking && staminaSystem.CanPerformAttack())
		{
			playerController.AttackComponent.PerformAttack(useCustomDirection: false, default(Vector3), useAnimationFromWeapon: true, delegate
			{
				isAttacking = false;
			});
		}
	}

	public override void OnExit()
	{
		playerController.AttackComponent.OnAttackAnimFinished(playerController.View.PlayerAnimation);
		playerController.SetControlTakenType(TakenControlType.ByAttack, isEnabled: true);
	}
}
