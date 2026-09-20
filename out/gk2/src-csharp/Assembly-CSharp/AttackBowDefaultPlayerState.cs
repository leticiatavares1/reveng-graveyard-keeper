using LazyBearTechnology;
using UnityEngine;

public class AttackBowDefaultPlayerState : SSMState
{
	private static readonly int AttackFocus = Animator.StringToHash("attack_focus");

	private static readonly int BowShoot = Animator.StringToHash("bow_shoot");

	private static readonly int BowPrepare = Animator.StringToHash("bow_preparing");

	private bool isHoldingKey;

	private bool wasShoot;

	private Animator animator;

	private StaminaSystem staminaSystem;

	public override bool IsActive => isHoldingKey;

	public AttackBowDefaultPlayerState(PlayerController playerController)
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
		animator.SetBool(BowPrepare, value: true);
		Debug.Log("Entering AttackBowDefaultPlayerState");
	}

	public override void Update()
	{
		if (!LazyInput.GetKey(GameKey.Attack) && !animator.GetBool(BowShoot))
		{
			if (!wasShoot)
			{
				wasShoot = true;
				animator.SetBool(BowShoot, value: true);
				animator.SetBool(BowPrepare, value: false);
				playerController.View.PlayerAnimation.CancelBowAimLoop();
			}
			else
			{
				isHoldingKey = false;
			}
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
		playerController.View.PlayerAnimation.CancelBowAimLoop();
		wasShoot = false;
		animator.SetBool(AttackFocus, value: false);
		animator.SetBool(BowPrepare, value: false);
		animator.SetBool(BowShoot, value: false);
		Debug.Log("Exiting AttackBowDefaultPlayerState");
	}
}
