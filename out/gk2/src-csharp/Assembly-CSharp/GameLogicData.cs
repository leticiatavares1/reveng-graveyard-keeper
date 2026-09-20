using System;
using LazyBearTechnology;
using UnityEngine;

[Serializable]
public class GameLogicData : ObjectLinkedToDefinition<GameLogicDef>
{
	[SerializeField]
	public float execTime = -1f;

	[SerializeField]
	public int execDay = 1;

	public int lastExecDay;

	private float lastFireTimeOfDay = -1f;

	private int lastFireDay = -1;

	public GameLogicData()
	{
	}

	public GameLogicData(string id)
		: base(id)
	{
	}

	public virtual void Init()
	{
		if (execTime < 0f)
		{
			EnvironmentData environmentData = MainGame.Instance.GameSave.environmentData;
			execTime = base.Definition.startTime;
			execDay = 1 + (int)execTime;
			execTime %= 1f;
			if (execDay == 1 && execTime < environmentData.TimeOfDay)
			{
				execDay++;
			}
			SkipOverduePeriods(environmentData);
		}
	}

	private void SkipOverduePeriods(EnvironmentData envData)
	{
		float periodTime = base.Definition.periodTime;
		if (!(periodTime <= 0f))
		{
			float num = (float)envData.Day + envData.TimeOfDay;
			float num2 = (float)execDay + execTime;
			if (!(num < num2))
			{
				float num3 = MathF.Floor((num - num2) / periodTime) + 1f;
				num2 = MathF.Round(num2 + num3 * periodTime, 3);
				execDay = (int)num2;
				execTime = num2 - (float)execDay;
			}
		}
	}

	protected virtual void UpdateTimer()
	{
		execTime += base.Definition.periodTime;
		execTime = MathF.Round(execTime, 3);
		execDay += (int)execTime;
		execTime %= 1f;
	}

	protected bool TryConsumeFireSlot()
	{
		EnvironmentData environmentData = MainGame.Instance.GameSave.environmentData;
		if ((environmentData.TimeOfDay - lastFireTimeOfDay).EqualsTo(0f) && environmentData.Day == lastFireDay)
		{
			return false;
		}
		lastFireTimeOfDay = environmentData.TimeOfDay;
		lastFireDay = environmentData.Day;
		return true;
	}

	public virtual void TryExecute()
	{
		if (!TryConsumeFireSlot())
		{
			return;
		}
		UpdateTimer();
		if (!base.Definition.condition.EvaluateChance())
		{
			return;
		}
		Debug.Log("[GameLogicData]: [" + id + "] executing");
		foreach (LazyExpression execExpression in base.Definition.execExpressions)
		{
			execExpression.Evaluate();
		}
		if (!string.IsNullOrEmpty(base.Definition.execFlowscriptName))
		{
			GameScriptUtility.RunGlobalScript(base.Definition.execFlowscriptName);
		}
	}
}
