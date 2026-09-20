using LazyBearTechnology;
using UnityEngine;

public class PlayerHappinessGameResSystem : GK2GameResSystem
{
	public PlayerHappinessGameResSystem(string gameResAtomName, GameRes gameRes)
		: base(gameResAtomName, gameRes)
	{
	}

	public override void Add(float value, bool silent = false)
	{
		float num = resForChanges.Get(gameResAtomName);
		float num2 = num;
		if (MainGame.Instance.GameSave.townSystem.Quality > 0)
		{
			if (value >= 0f)
			{
				if (!(num >= (float)MainGame.Instance.GameSave.townSystem.Quality))
				{
					num += value;
					num = Mathf.Clamp(num, base.Min, MainGame.Instance.GameSave.townSystem.Quality);
					resForChanges.SetWithoutSystemsCheck(gameResAtomName, num);
				}
			}
			else if (num >= (float)MainGame.Instance.GameSave.townSystem.Quality)
			{
				num += value;
				num = Mathf.Clamp(num, base.Min, num2);
				resForChanges.SetWithoutSystemsCheck(gameResAtomName, num);
			}
			else
			{
				num += value;
				num = Mathf.Clamp(num, base.Min, MainGame.Instance.GameSave.townSystem.Quality);
				resForChanges.SetWithoutSystemsCheck(gameResAtomName, num);
			}
		}
		else
		{
			num += value;
			num = Mathf.Clamp(num, base.Min, base.Max);
			resForChanges.SetWithoutSystemsCheck(gameResAtomName, num);
		}
		if (!silent)
		{
			onValueChanged?.Invoke(num);
			if (!Mathf.Approximately(num, num2))
			{
				onValueDeltaChanged?.Invoke(num - num2);
			}
		}
		int valueDelta = (int)num - (int)num2;
		EvaluateGameResExpressions(valueDelta);
	}
}
