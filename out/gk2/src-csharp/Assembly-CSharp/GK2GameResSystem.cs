using System;
using LazyBearTechnology;
using UnityEngine;

public class GK2GameResSystem : GameResSystemBase
{
	public Action<float> onValueDeltaChanged;

	private const float EPSILON = 0.001f;

	protected static PlayerData PlayerData => MainGame.PlayerData;

	private GameResSystemDef Def => GameBalance.Me.GetData<GameResSystemDef>(gameResAtomName);

	public float Max => Def.max.EvaluateFloat();

	public float Min => Def.min.EvaluateFloat();

	public GK2GameResSystem(string gameResAtomName, GameRes gameRes)
		: base(gameResAtomName, gameRes)
	{
	}

	public bool IsEnoughValue(float value)
	{
		if ((resForChanges.Get(gameResAtomName) - value).EqualsOrMore(0f, 0.001f))
		{
			return true;
		}
		return false;
	}

	public bool HasMax()
	{
		return PlayerData.GetRes(gameResAtomName).EqualsTo(Max);
	}

	public bool CanAddValue(float value)
	{
		float num = resForChanges.Get(gameResAtomName);
		if (num + value > Max || num < Min)
		{
			Debug.Log(string.Format("[{0}] typed:[{1}]: Cannot add more {2} [current maximum = {3}].", "GameResSystemBase", gameResAtomName, gameResAtomName, Max));
			return false;
		}
		return true;
	}

	public override void Add(float value, bool silent = false)
	{
		float num = resForChanges.Get(gameResAtomName);
		float num2 = num;
		num += value;
		num = Mathf.Clamp(num, Min, Max);
		resForChanges.SetWithoutSystemsCheck(gameResAtomName, num);
		if (!silent)
		{
			onValueChanged?.Invoke(num);
			onValueDeltaChanged?.Invoke(num - num2);
		}
		int valueDelta = (int)num - (int)num2;
		EvaluateGameResExpressions(valueDelta);
	}

	public override void Set(float value, bool silent = false)
	{
		float num = resForChanges.Get(gameResAtomName);
		float num2 = Mathf.Clamp(value, Min, Max);
		resForChanges.SetWithoutSystemsCheck(gameResAtomName, num2);
		if (!silent)
		{
			onValueChanged?.Invoke(num2);
			onValueDeltaChanged?.Invoke(num2 - num);
		}
		int valueDelta = (int)num2 - (int)num;
		EvaluateGameResExpressions(valueDelta);
	}

	protected void EvaluateGameResExpressions(int valueDelta)
	{
		if (valueDelta > 0)
		{
			foreach (LazyExpression item in Def.expressionsOnIncrease)
			{
				item.EvaluateValueDelta(valueDelta);
			}
		}
		if (valueDelta >= 0)
		{
			return;
		}
		foreach (LazyExpression item2 in Def.expressionsOnDecrease)
		{
			item2.EvaluateValueDelta(Mathf.Abs(valueDelta));
		}
	}

	public static GK2GameResSystem GetSystem(string type)
	{
		return PlayerData.GetResSystem(type) as GK2GameResSystem;
	}
}
