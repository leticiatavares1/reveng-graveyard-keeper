using LazyBearTechnology;

public class PlayerEnergyGameResSystem : GK2GameResSystem
{
	private PerkSystemData PerkSystemData => MainGame.Instance.GameSave.perkSystemData;

	public PlayerEnergyGameResSystem(string gameResAtomName, GameRes gameRes)
		: base(gameResAtomName, gameRes)
	{
	}

	public override void Add(float value, bool silent = false)
	{
		if (value < 0f && PerkSystemData.HasPerk("lack_of_sleep_debuff"))
		{
			GK2GameResSystem.PlayerData.AddRes("insanity", (0f - value) / 2f);
		}
		Set(GK2GameResSystem.PlayerData.GetRes(gameResAtomName) + value, silent);
	}

	public static PlayerEnergyGameResSystem GetSystem()
	{
		return GK2GameResSystem.PlayerData.GetResSystem("energy") as PlayerEnergyGameResSystem;
	}
}
