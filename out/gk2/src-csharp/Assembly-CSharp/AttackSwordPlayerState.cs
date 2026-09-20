using LazyBearTechnology;
using UnityEngine;

public class AttackSwordPlayerState : SSMState
{
	private const string ATTACK_TRIGGER = "attack";

	private bool isAttacking;

	private float dashEndTime;

	private float dashCooldownEndTime;

	private Vector2 dashDirection;

	private StaminaSystem staminaSystem;

	public override bool IsActive => isAttacking;

	public AttackSwordPlayerState(PlayerController playerController)
		: base(playerController)
	{
	}

	public override void OnEnter()
	{
		staminaSystem = MainGame.PlayerData.staminaSystem;
		isAttacking = true;
		playerController.View.PlayerAnimation.SetLayerWeight(AnimationComponent.Layers.SwordAttack, 1f);
		playerController.View.PlayerAnimation.SetLayerWeight(AnimationComponent.Layers.SwordAttackHitbox, 1f);
		playerController.SetControlTakenType(TakenControlType.ByAttack, isEnabled: false);
		dashDirection = LazyInput.GetDirection();
		if (dashDirection.magnitude < 0.1f)
		{
			dashDirection = playerController.PlayerData.Direction;
		}
		dashEndTime = Time.time + playerController.PhysicalBody.PhysicsConfig.attackDashDuration;
		dashCooldownEndTime = Time.time + playerController.PhysicalBody.PhysicsConfig.attackDashCooldown;
		playerController.AttackComponent.PerformAttackByTrigger(useCustomDirection: false, default(Vector3), "attack", delegate
		{
			isAttacking = false;
		});
	}

	public override void Update()
	{
		if (LazyInput.GetKeyDown(GameKey.Attack) && isAttacking && staminaSystem.CanPerformAttack())
		{
			playerController.AttackComponent.PerformAttackByTrigger(useCustomDirection: false, default(Vector3), "attack", delegate
			{
				isAttacking = false;
			});
		}
	}

	public override void FixedUpdate()
	{
		if (Time.time < dashEndTime)
		{
			Vector3 force = dashDirection.XZ().normalized * playerController.PhysicalBody.PhysicsConfig.attackDashForce;
			playerController.PhysicalBody.Rb.AddForce(force, ForceMode.Impulse);
		}
	}

	public override void OnExit()
	{
		playerController.AttackComponent.OnAttackAnimFinished(playerController.View.PlayerAnimation);
		playerController.View.PlayerAnimation.SetLayerWeight(AnimationComponent.Layers.SwordAttack, 0f);
		playerController.View.PlayerAnimation.SetLayerWeight(AnimationComponent.Layers.SwordAttackHitbox, 0f);
		playerController.SetControlTakenType(TakenControlType.ByAttack, isEnabled: true);
	}
}
