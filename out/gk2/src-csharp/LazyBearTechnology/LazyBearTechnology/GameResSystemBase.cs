using System;

namespace LazyBearTechnology;

public abstract class GameResSystemBase
{
	public Action<float> onValueChanged;

	protected readonly GameRes resForChanges;

	protected readonly string gameResAtomName;

	protected GameResSystemBase(string gameResAtomName, GameRes gameRes)
	{
		this.gameResAtomName = gameResAtomName;
		resForChanges = gameRes;
	}

	public virtual void Set(float value, bool silent = false)
	{
		resForChanges.SetWithoutSystemsCheck(gameResAtomName, value);
		if (!silent)
		{
			onValueChanged?.Invoke(resForChanges.Get(gameResAtomName));
		}
	}

	public virtual void Add(float value, bool silent = false)
	{
		resForChanges.AddWithoutSystemsCheck(gameResAtomName, value);
		if (!silent)
		{
			onValueChanged?.Invoke(resForChanges.Get(gameResAtomName));
		}
	}

	public virtual float Get()
	{
		return resForChanges.GetWithoutSystemsCheck(gameResAtomName);
	}
}
