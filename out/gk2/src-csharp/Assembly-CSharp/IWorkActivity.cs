public interface IWorkActivity
{
	bool IsEnoughDurability(Item tool);

	int GetActionDamage(Item item);

	bool CanStartActivity();

	void OnStartActivity();

	bool IsEnoughMastery();

	bool CanUseTool(Item tool);

	void UseTool(Item tool, int deltaTick);

	bool IsEnoughEnergy(Item tool, float energyPerTick);

	void ConsumeEnergy(Item tool, float energyPerTick);

	float GetEnergyCostPerTick(Item tool);

	bool CanChangeInsanity(Item tool, float insanityPerTick);

	void ChangeInsanity(Item tool, float insanityPerTick);

	float GetInsanityCostPerTick(Item tool);
}
