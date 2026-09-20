using LazyBearTechnology;
using UnityEngine;

public class AttackBowAutoPlayerState : SSMState
{
	private static readonly int BowShoot = Animator.StringToHash("bow_shoot");

	private static readonly int BowPrepare = Animator.StringToHash("bow_preparing");

	private static readonly int AttackFocus = Animator.StringToHash("attack_focus");

	private static readonly int WalkSpeed = Animator.StringToHash("walk_speed");

	private float movementSpeedMultiplier = 0.5f;

	private float walkAnimationSpeed = 0.5f;

	private bool isHoldingKey;

	private bool wasShoot;

	private Animator animator;

	private StaminaSystem staminaSystem;

	public override bool IsActive => isHoldingKey;

	public AttackBowAutoPlayerState(PlayerController playerController)
		: base(playerController)
	{
	}

	public override void OnEnter()
	{
		staminaSystem = MainGame.PlayerData.staminaSystem;
		wasShoot = false;
		isHoldingKey = true;
		playerController.View.PlayerAnimation.SetLayerWeight(AnimationComponent.Layers.BowAttack, 1f);
		animator = playerController.View.PlayerAnimation.Animator;
		animator.SetBool(AttackFocus, value: true);
		animator.SetBool(BowPrepare, value: true);
		animator.SetFloat(WalkSpeed, walkAnimationSpeed);
		playerController.PlayerData.SetDirectionLock(isEnabled: false);
		playerController.PhysicalBody.SetDirectionLock(isEnabled: false);
		playerController.PhysicalBody.SpeedMultiplier = movementSpeedMultiplier;
		Debug.Log("Entering AttackBowDefaultPlayerState");
	}

	public override void Update()
	{
		float x = playerController.MovableDirection.x;
		float @float = animator.GetFloat(WalkSpeed);
		if (!Mathf.Sign(x).EqualsTo(Mathf.Sign(@float)) && x != 0f)
		{
			float value = Mathf.Abs(walkAnimationSpeed) * Mathf.Sign(x);
			animator.SetFloat(WalkSpeed, value);
		}
		if (!animator.GetBool(BowPrepare) && staminaSystem.CanPerformAttack())
		{
			animator.SetBool(BowPrepare, value: true);
		}
		if (!animator.GetBool(BowShoot) && animator.GetBool(BowPrepare))
		{
			animator.SetBool(BowShoot, value: true);
		}
		if (!LazyInput.GetKey(GameKey.Attack))
		{
			isHoldingKey = false;
		}
	}

	public override void FixedUpdate()
	{
		playerController.PhysicalBody.MoveByDirection(LazyInput.GetDirection());
	}

	public override void OnExit()
	{
		wasShoot = false;
		playerController.AttackComponent.OnAttackAnimFinished(playerController.View.PlayerAnimation);
		playerController.View.PlayerAnimation.SetLayerWeight(AnimationComponent.Layers.SwordAttack, 0f);
		animator.SetBool(AttackFocus, value: false);
		animator.SetBool(BowPrepare, value: false);
		animator.SetBool(BowShoot, value: false);
		animator.SetFloat(WalkSpeed, 1f);
		playerController.PlayerData.SetDirectionLock(isEnabled: true);
		playerController.PhysicalBody.SetDirectionLock(isEnabled: true);
		playerController.PhysicalBody.SpeedMultiplier = 1f;
		Debug.Log("Exiting AttackBowDefaultPlayerState");
	}
}
