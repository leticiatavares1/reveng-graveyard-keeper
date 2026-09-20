using LazyBearTechnology;
using UnityEngine;

public class AttackBowFocusedPlayerState : SSMState, IStaminaConsumer
{
	private const string MOVEMENT_DIR = "MovementDirection";

	private static readonly int BowPrepare = Animator.StringToHash("bow_preparing");

	private static readonly int AttackFocus = Animator.StringToHash("attack_focus");

	private static readonly int BowShoot = Animator.StringToHash("bow_shoot");

	private static readonly int WalkSpeed = Animator.StringToHash("walk_speed");

	private float movementSpeedMultiplier = 0.5f;

	private float attackAnimationSpeed = 1.7f;

	private float walkAnimationSpeed = 0.5f;

	private bool isInFocus;

	private bool isAttackBtnHold;

	private Animator animator;

	private Direction focusedDirection;

	private StaminaSystem staminaSystem;

	public override bool IsActive => isInFocus;

	public Direction FocusedDirection => focusedDirection;

	public AttackBowFocusedPlayerState(PlayerController playerController)
		: base(playerController)
	{
	}

	public override void OnEnter()
	{
		staminaSystem = MainGame.PlayerData.staminaSystem;
		isInFocus = true;
		focusedDirection = playerController.PlayerData.Direction.ConvertFromVector2();
		animator = playerController.View.PlayerAnimation.Animator;
		playerController.View.PlayerAnimation.SetLayerWeight(AnimationComponent.Layers.BowAttack, 1f);
		playerController.View.PlayerAnimation.SetLayerWeight(AnimationComponent.Layers.StanceWalk, 1f);
		animator.SetFloat(WalkSpeed, walkAnimationSpeed);
		animator.SetBool(BowPrepare, value: true);
		animator.SetBool(AttackFocus, value: true);
		playerController.PlayerData.SetDirectionLock(isEnabled: false);
		playerController.PhysicalBody.SetDirectionLock(isEnabled: false);
		playerController.PhysicalBody.SpeedMultiplier = movementSpeedMultiplier;
		Debug.Log("Entering AttackBowFocusedPlayerState");
	}

	public override void Update()
	{
		if (!playerController.IsControlsEnabled)
		{
			isInFocus = false;
		}
		float value = Mathf.Atan2(LazyInput.GetDirection().y, LazyInput.GetDirection().x) * 57.29578f;
		animator.SetFloat("MovementDirection", value);
		float x = playerController.MovableDirection.x;
		float @float = animator.GetFloat(WalkSpeed);
		if (!Mathf.Sign(x).EqualsTo(Mathf.Sign(@float)) && x != 0f)
		{
			float value2 = Mathf.Abs(walkAnimationSpeed) * Mathf.Sign(x);
			animator.SetFloat(WalkSpeed, value2);
		}
		if (LazyInput.GetKeyDown(GameKey.Attack))
		{
			isAttackBtnHold = true;
		}
		if (!LazyInput.GetKey(GameKey.Attack))
		{
			if (animator.GetBool(BowPrepare) && !animator.GetBool(BowShoot) && isAttackBtnHold && staminaSystem.CanPerformAttack())
			{
				playerController.View.PlayerAnimation.CancelBowAimLoop();
				animator.SetBool(BowShoot, value: true);
			}
			isAttackBtnHold = false;
		}
		if (!LazyInput.GetKey(GameKey.AttackFocus))
		{
			isInFocus = false;
		}
		if (playerController.IsControlsEnabled)
		{
			playerController.PlayerInputHandler.UpdateHotBarInteraction();
		}
	}

	public override void FixedUpdate()
	{
		if (playerController.IsControlsEnabled)
		{
			playerController.PhysicalBody.MoveByDirection(LazyInput.GetDirection());
		}
	}

	public override void OnExit()
	{
		playerController.View.PlayerAnimation.SetLayerWeight(AnimationComponent.Layers.BowAttack, 0f);
		playerController.View.PlayerAnimation.SetLayerWeight(AnimationComponent.Layers.StanceWalk, 0f);
		playerController.View.PlayerAnimation.CancelBowAimLoop();
		animator.SetBool(AttackFocus, value: false);
		animator.SetBool(BowPrepare, value: false);
		animator.SetBool(BowShoot, value: false);
		animator.SetFloat(WalkSpeed, 1f);
		playerController.PlayerData.SetDirectionLock(isEnabled: true);
		playerController.PhysicalBody.SetDirectionLock(isEnabled: true);
		playerController.PhysicalBody.SpeedMultiplier = 1f;
		Debug.Log("Exiting AttackBowFocusedPlayerState");
	}
}
