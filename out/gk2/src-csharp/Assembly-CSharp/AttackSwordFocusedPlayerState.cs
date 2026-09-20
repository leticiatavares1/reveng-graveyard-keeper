using LazyBearTechnology;
using UnityEngine;

public class AttackSwordFocusedPlayerState : SSMState, IStaminaConsumer
{
	private const string ATTACK_TRIGGER = "attack";

	private const string MOVEMENT_DIR = "MovementDirection";

	private static readonly int WalkSpeed = Animator.StringToHash("walk_speed");

	private static readonly int AttackFocus = Animator.StringToHash("attack_focus");

	private float movementSpeedMultiplier = 0.5f;

	private float walkAnimationSpeed = 0.5f;

	private bool isInFocus;

	private Animator animator;

	private Direction focusedDirection;

	private StaminaSystem staminaSystem;

	public override bool IsActive => isInFocus;

	public Direction FocusedDirection => focusedDirection;

	public AttackSwordFocusedPlayerState(PlayerController playerController)
		: base(playerController)
	{
	}

	public override void OnEnter()
	{
		staminaSystem = MainGame.PlayerData.staminaSystem;
		isInFocus = true;
		focusedDirection = playerController.PlayerData.Direction.ConvertFromVector2();
		animator = playerController.View.PlayerAnimation.Animator;
		playerController.View.PlayerAnimation.SetLayerWeight(AnimationComponent.Layers.SwordAttack, 1f);
		playerController.View.PlayerAnimation.SetLayerWeight(AnimationComponent.Layers.SwordAttackHitbox, 1f);
		playerController.View.PlayerAnimation.SetLayerWeight(AnimationComponent.Layers.StanceWalk, 1f);
		animator.SetBool(AttackFocus, value: true);
		animator.SetFloat(WalkSpeed, walkAnimationSpeed);
		playerController.PlayerData.SetDirectionLock(isEnabled: false);
		playerController.PhysicalBody.SetDirectionLock(isEnabled: false);
		playerController.PhysicalBody.SpeedMultiplier = movementSpeedMultiplier;
		Debug.Log("Entering AttackSwordFocusedPlayerState");
	}

	public override void Update()
	{
		if (!playerController.IsControlsEnabled)
		{
			isInFocus = false;
		}
		float value = Mathf.Atan2(LazyInput.GetDirection().y, LazyInput.GetDirection().x) * 57.29578f;
		animator.SetFloat("MovementDirection", value);
		if (LazyInput.GetKeyDown(GameKey.Attack) && isInFocus)
		{
			TryDoAttack();
		}
		float x = playerController.MovableDirection.x;
		float @float = animator.GetFloat(WalkSpeed);
		if (!Mathf.Sign(x).EqualsTo(Mathf.Sign(@float)) && x != 0f)
		{
			float value2 = Mathf.Abs(walkAnimationSpeed) * Mathf.Sign(x);
			animator.SetFloat(WalkSpeed, value2);
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

	private bool TryDoAttack()
	{
		if (!staminaSystem.CanPerformAttack())
		{
			return false;
		}
		playerController.AttackComponent.PerformAttackByTrigger(useCustomDirection: false, default(Vector3), "attack");
		return true;
	}

	public override void OnExit()
	{
		playerController.View.PlayerAnimation.SetLayerWeight(AnimationComponent.Layers.SwordAttack, 0f);
		playerController.View.PlayerAnimation.SetLayerWeight(AnimationComponent.Layers.SwordAttackHitbox, 0f);
		playerController.View.PlayerAnimation.SetLayerWeight(AnimationComponent.Layers.StanceWalk, 0f);
		Animator obj = playerController.View.PlayerAnimation.Animator;
		obj.SetBool(AttackFocus, value: false);
		obj.SetFloat(WalkSpeed, 1f);
		playerController.PlayerData.SetDirectionLock(isEnabled: true);
		playerController.PhysicalBody.SetDirectionLock(isEnabled: true);
		playerController.PhysicalBody.SpeedMultiplier = 1f;
		Debug.Log("Exiting AttackSwordFocusedPlayerState");
	}
}
