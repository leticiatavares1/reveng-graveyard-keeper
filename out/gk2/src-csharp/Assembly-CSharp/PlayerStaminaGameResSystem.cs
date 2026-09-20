using LazyBearTechnology;
using UnityEngine;

public class PlayerStaminaGameResSystem : GK2GameResSystem
{
	public PlayerStaminaGameResSystem(string gameResAtomName, GameRes gameRes)
		: base(gameResAtomName, gameRes)
	{
	}

	public bool CanChangeStamina(float value)
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
		onValueChanged?.Invoke(num);
		onValueDeltaChanged?.Invoke(num - res);
	}

	public override void Add(float value, bool silent = false)
	{
		Set(GK2GameResSystem.PlayerData.GetRes(gameResAtomName) + value, silent);
	}

	public static PlayerStaminaGameResSystem GetSystem()
	{
		return GK2GameResSystem.PlayerData.GetResSystem("stamina") as PlayerStaminaGameResSystem;
	}
}
