using System;
using LazyBearTechnology;

public class StaminaBarPlayerWidgetData : LazyWidgetDataBase
{
	public Action onNotEnoughStamina;

	private StaminaSystem staminaSystem;

	private bool subscribedToDataChanges;

	public PlayerStaminaGameResSystem StaminaResSystem { get; private set; }

	public StaminaBarPlayerWidgetData()
	{
		StaminaResSystem = PlayerStaminaGameResSystem.GetSystem();
		staminaSystem = MainGame.PlayerData.staminaSystem;
	}

	public void SubscribeToDataChanges()
	{
		if (!subscribedToDataChanges)
		{
			staminaSystem.OnNotEnoughStamina += onNotEnoughStamina;
			subscribedToDataChanges = true;
		}
	}

	public void UnsubscribeFromDataChanges()
	{
		if (subscribedToDataChanges)
		{
			staminaSystem.OnNotEnoughStamina -= onNotEnoughStamina;
			subscribedToDataChanges = false;
		}
	}
}
