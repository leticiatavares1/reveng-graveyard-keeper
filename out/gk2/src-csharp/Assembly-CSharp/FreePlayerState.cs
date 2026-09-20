using LazyBearTechnology;

public class FreePlayerState : SSMState
{
	private PlayerInputHandler playerInputHandler;

	public override bool IsActive => playerController.IsControlsEnabled;

	public FreePlayerState(PlayerController playerController)
		: base(playerController)
	{
		playerInputHandler = playerController.PlayerInputHandler;
	}

	public override void Update()
	{
		playerInputHandler.UpdateInput();
	}

	public override void FixedUpdate()
	{
		playerController.PhysicalBody.MoveByDirection(LazyInput.GetDirection());
	}
}
