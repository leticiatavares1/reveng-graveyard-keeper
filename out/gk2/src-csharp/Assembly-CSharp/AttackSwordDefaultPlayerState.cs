using System;
using LazyBearTechnology;
using UnityEngine;

public class AttackSwordDefaultPlayerState : SSMState
{
	private const string ATTACK_TRIGGER = "attack";

	private static readonly int AttackFocus = Animator.StringToHash("attack_focus");

	private bool isAttacking;

	private Animator animator;

	private StaminaSystem staminaSystem;

	public override bool IsActive => isAttacking;

	public AttackSwordDefaultPlayerState(PlayerController playerController)
		: base(playerController)
	{
	}

	public override void OnEnter()
	{
		staminaSystem = MainGame.PlayerData.staminaSystem;
		animator = playerController.View.PlayerAnimation.Animator;
		playerController.View.PlayerAnimation.SetLayerWeight(AnimationComponent.Layers.SwordAttack, 1f);
		playerController.View.PlayerAnimation.SetLayerWeight(AnimationComponent.Layers.SwordAttackHitbox, 1f);
		isAttacking = TryDoAttack();
		Debug.Log("Entering AttackSwordDefaultPlayerState");
	}

	public override void Update()
	{
		if (LazyInput.GetKeyDown(GameKey.Attack) && isAttacking)
		{
			TryDoAttack();
		}
	}

	public override void FixedUpdate()
	{
		if (playerController.IsControlsEnabled)
		{
			playerController.PhysicalBody.MoveByDirection(LazyInput.GetDirection());
		}
	}

	private bool TryDoAttack()
	{
		if (!staminaSystem.CanPerformAttack())
		{
			isAttacking = false;
			return false;
		}
		AttackComponent attackComponent = playerController.AttackComponent;
		Action onAnimationFinished = OnAttackFinished;
		attackComponent.PerformAttackByTrigger(useCustomDirection: false, default(Vector3), "attack", onAnimationFinished);
		return true;
	}

	private void OnAttackFinished()
	{
		isAttacking = false;
	}

	public override void OnExit()
	{
		playerController.View.PlayerAnimation.SetLayerWeight(AnimationComponent.Layers.SwordAttack, 0f);
		playerController.View.PlayerAnimation.SetLayerWeight(AnimationComponent.Layers.SwordAttackHitbox, 0f);
		animator.SetBool(AttackFocus, value: false);
		Debug.Log("Exiting AttackSwordDefaultPlayerState");
	}
}
