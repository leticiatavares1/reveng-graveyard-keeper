using LazyBearTechnology;
using UnityEngine;

public class PlantingPlayerState : SSMState
{
	public override bool CanEnter => playerController.PlayerData?.interactingItem != null;

	public override bool IsActive => playerController.PlayerData?.interactingItem != null;

	public PlantingPlayerState(PlayerController playerController)
		: base(playerController)
	{
	}

	public override void Update()
	{
		playerController.PlayerInputHandler.UpdateInputInteractionOnly();
		if (LazyInput.GetKeyDown(GameKey.Action) || LazyInput.GetKeyDown(GameKey.InGameMenu) || Input.GetKeyDown(KeyCode.Tab))
		{
			playerController.PlayerData.RemoveInteractingItem();
		}
	}

	public override void FixedUpdate()
	{
		if (MainGame.PlayerController.IsControlsEnabled)
		{
			playerController.PhysicalBody.MoveByDirection(LazyInput.GetDirection());
		}
	}

	public override void OnExit()
	{
		playerController.PlayerData.RemoveInteractingItem();
		playerController.PlayerInteractionComponent.ResetInteractionState();
	}
}
