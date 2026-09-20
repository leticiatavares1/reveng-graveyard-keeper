using System;

[Serializable]
public class StaminaSystem
{
	public float regenerationDelayTimer;

	public bool isRegenerationDelayed;

	private float regenerationDelay = -1f;

	private float regeneration = -1f;

	private float regenerationStance = -1f;

	private float RegenerationDelay
	{
		get
		{
			if (regenerationDelay <= 0f)
			{
				regenerationDelay = GameBalance.Me.GetData<ConstDef>("stamina_regeneration_delay").FloatValue;
			}
			return regenerationDelay;
		}
	}

	private float Regeneration
	{
		get
		{
			if (regeneration <= 0f)
			{
				regeneration = GameBalance.Me.GetData<ConstDef>("stamina_regeneration").FloatValue;
			}
			return regeneration;
		}
	}

	private float RegenerationStance
	{
		get
		{
			if (regenerationStance <= 0f)
			{
				regenerationStance = GameBalance.Me.GetData<ConstDef>("stamina_regeneration_stance").FloatValue;
			}
			return regenerationStance;
		}
	}

	public event Action OnNotEnoughStamina;

	public bool CanPerformAttack()
	{
		Item itemByGroupId = MainGame.PlayerData.toolBeltInventory.GetItemByGroupId("weapon");
		if (itemByGroupId.IsEmpty)
		{
			return false;
		}
		if (!PlayerStaminaGameResSystem.GetSystem().CanChangeStamina(-itemByGroupId.Definition.staminaCost.EvaluateInt()))
		{
			this.OnNotEnoughStamina?.Invoke();
			return false;
		}
		return true;
	}

	public void ConsumeStamina()
	{
		Item itemByGroupId = MainGame.PlayerData.toolBeltInventory.GetItemByGroupId("weapon");
		PlayerStaminaGameResSystem.GetSystem().Add(-itemByGroupId.Definition.staminaCost.EvaluateInt());
		isRegenerationDelayed = true;
		regenerationDelayTimer = 0f;
	}

	public void UpdateStaminaLogic(float deltaTime)
	{
		if (isRegenerationDelayed)
		{
			regenerationDelayTimer += deltaTime;
			if (regenerationDelayTimer < RegenerationDelay)
			{
				return;
			}
			regenerationDelayTimer = 0f;
			isRegenerationDelayed = false;
		}
		if (!PlayerStaminaGameResSystem.GetSystem().HasMax())
		{
			float num = 0f;
			num = ((!(MainGame.PlayerController.Ssm.CurState is IStaminaConsumer)) ? Regeneration : RegenerationStance);
			PlayerStaminaGameResSystem.GetSystem().Add(num * deltaTime);
		}
	}

	public void SetMax()
	{
		PlayerStaminaGameResSystem system = PlayerStaminaGameResSystem.GetSystem();
		system.Set(system.Max);
	}
}
