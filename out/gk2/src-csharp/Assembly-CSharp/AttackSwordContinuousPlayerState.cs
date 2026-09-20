using System;
using LazyBearTechnology;
using UnityEngine;

public class AttackSwordContinuousPlayerState : SSMState
{
	private const string ATTACK_TRIGGER = "attack";

	private static readonly int WalkSpeed = Animator.StringToHash("walk_speed");

	private static readonly int AttackFocus = Animator.StringToHash("attack_focus");

	private static readonly int AttackSpeed = Animator.StringToHash("attack_speed");

	private float movementSpeedMultiplier = 0.5f;

	private float walkAnimationSpeed = 0.5f;

	private bool isAttacking;

	private bool isAttackAnimPlaying;

	private Animator animator;

	private StaminaSystem staminaSystem;

	public override bool IsActive => isAttacking;

	public AttackSwordContinuousPlayerState(PlayerController playerController)
		: base(playerController)
	{
	}

	public override void OnEnter()
	{
		staminaSystem = MainGame.PlayerData.staminaSystem;
		isAttacking = true;
		animator = playerController.View.PlayerAnimation.Animator;
		playerController.View.PlayerAnimation.SetLayerWeight(AnimationComponent.Layers.SwordAttack, 1f);
		playerController.View.PlayerAnimation.SetLayerWeight(AnimationComponent.Layers.SwordAttackHitbox, 1f);
		animator.SetFloat(WalkSpeed, walkAnimationSpeed);
		playerController.PlayerData.SetDirectionLock(isEnabled: false);
		playerController.PhysicalBody.SetDirectionLock(isEnabled: false);
		playerController.PhysicalBody.SpeedMultiplier = movementSpeedMultiplier;
		isAttackAnimPlaying = true;
		AttackComponent attackComponent = playerController.AttackComponent;
		Action onAnimationFinished = OnAttackFinished;
		attackComponent.PerformAttackByTrigger(useCustomDirection: false, default(Vector3), "attack", onAnimationFinished);
		Debug.Log("Entering ContinuousPlayerState");
	}

	public override void Update()
	{
		if (!LazyInput.GetKey(GameKey.Attack) && !isAttackAnimPlaying)
		{
			isAttacking = false;
		}
		else if (!isAttackAnimPlaying && staminaSystem.CanPerformAttack())
		{
			playerController.PlayerData.SetDirectionLock(isEnabled: false);
			playerController.PhysicalBody.SetDirectionLock(isEnabled: false);
			isAttackAnimPlaying = true;
			AttackComponent attackComponent = playerController.AttackComponent;
			Action onAnimationFinished = OnAttackFinished;
			attackComponent.PerformAttackByTrigger(useCustomDirection: false, default(Vector3), "attack", onAnimationFinished);
		}
	}

	public override void FixedUpdate()
	{
		playerController.PhysicalBody.MoveByDirection(LazyInput.GetDirection());
	}

	private void OnAttackFinished()
	{
		isAttackAnimPlaying = false;
	}

	public override void OnExit()
	{
		playerController.AttackComponent.OnAttackAnimFinished(playerController.View.PlayerAnimation);
		playerController.View.PlayerAnimation.SetLayerWeight(AnimationComponent.Layers.SwordAttack, 0f);
		playerController.View.PlayerAnimation.SetLayerWeight(AnimationComponent.Layers.SwordAttackHitbox, 0f);
		playerController.View.PlayerAnimation.Animator.SetFloat(WalkSpeed, 1f);
		Debug.Log("Exiting ContinuousPlayerState");
		playerController.PlayerData.SetDirectionLock(isEnabled: true);
		playerController.PhysicalBody.SetDirectionLock(isEnabled: true);
		playerController.PhysicalBody.SpeedMultiplier = 1f;
	}
}
