using LazyBearTechnology;
using UnityEngine;

public class PlayerInsanityGameResSystem : GK2GameResSystem
{
	private PerkSystemData PerkSystemData => MainGame.Instance.GameSave.perkSystemData;

	public PlayerInsanityGameResSystem(string gameResAtomName, GameRes gameRes)
		: base(gameResAtomName, gameRes)
	{
	}

	public bool CanChangeInsanity(float value)
	{
		if (value > 0f)
		{
			return CanAddValue(value);
		}
		return IsEnoughValue(0f - value);
	}

	public override void Set(float value, bool silent = false)
	{
		float res = GK2GameResSystem.PlayerData.GetRes(gameResAtomName);
		float num = Mathf.Clamp(value, 0f, base.Max);
		GK2GameResSystem.PlayerData.SetResWithoutSystemsCheck(gameResAtomName, num);
		float max = PlayerEnergyGameResSystem.GetSystem().Max;
		if (GK2GameResSystem.PlayerData.GetRes("energy") > max)
		{
			GK2GameResSystem.PlayerData.SetRes("energy", max);
		}
		if (!silent)
		{
			onValueChanged?.Invoke(num);
			onValueDeltaChanged?.Invoke(num - res);
		}
		int valueDelta = (int)num - (int)res;
		EvaluateGameResExpressions(valueDelta);
	}

	public override void Add(float value, bool silent = false)
	{
		if (value < 0f && PerkSystemData.HasPerk("lack_of_sleep_debuff"))
		{
			GK2GameResSystem.PlayerData.AddResWithoutSystemsCheck(gameResAtomName, (0f - value) / 2f);
		}
		Set(GK2GameResSystem.PlayerData.GetRes(gameResAtomName) + value, silent);
	}

	public static PlayerInsanityGameResSystem GetSystem()
	{
		return GK2GameResSystem.PlayerData.GetResSystem("insanity") as PlayerInsanityGameResSystem;
	}
}
