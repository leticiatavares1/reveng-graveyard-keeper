public abstract class PlayerActivity : IWorkActivity
{
	protected PlayerData playerData;

	protected WgoData wgoData;

	public WgoData WgoData => wgoData;

	public abstract bool IsEnoughDurability(Item tool);

	public abstract int GetActionDamage(Item item);

	public abstract bool CanStartActivity();

	public abstract void OnStartActivity();

	public abstract bool IsEnoughMastery();

	public abstract bool CanUseTool(Item tool);

	public abstract void UseTool(Item tool, int deltaTick);

	public abstract bool IsEnoughEnergy(Item tool, float energyPerTick);

	public abstract void ConsumeEnergy(Item tool, float energyPerTick);

	public abstract float GetEnergyCostPerTick(Item tool);

	public abstract bool CanChangeInsanity(Item tool, float insanityPerTick);

	public abstract void ChangeInsanity(Item tool, float insanityPerTick);

	public abstract float GetInsanityCostPerTick(Item tool);
}
