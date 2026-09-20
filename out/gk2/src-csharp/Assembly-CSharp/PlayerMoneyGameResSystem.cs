using System;
using LazyBearTechnology;

public class PlayerMoneyGameResSystem : GK2GameResSystem
{
	public Action<float> onValueChangedDiff;

	public PlayerMoneyGameResSystem(string gameResAtomName, GameRes gameRes)
		: base(gameResAtomName, gameRes)
	{
	}

	public static PlayerMoneyGameResSystem GetSystem()
	{
		return GK2GameResSystem.PlayerData.GetResSystem("money") as PlayerMoneyGameResSystem;
	}

	public override void Set(float value, bool silent = false)
	{
		float num = resForChanges.Get(gameResAtomName);
		base.Set(value, silent);
		onValueChangedDiff?.Invoke(value - num);
	}

	public override void Add(float value, bool silent = false)
	{
		base.Add(value, silent);
		onValueChangedDiff?.Invoke(value);
	}
}
