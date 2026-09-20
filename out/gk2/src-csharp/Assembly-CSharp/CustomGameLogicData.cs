using System;
using UnityEngine;

[Serializable]
public class CustomGameLogicData : GameLogicData
{
	private bool destroyAfterExecute;

	private float periodTime;

	private Action executionLogic;

	private EnvironmentData envData;

	public CustomGameLogicData()
	{
	}

	public CustomGameLogicData(string id, float periodTime, Action executionLogic)
	{
		base.id = id;
		this.periodTime = periodTime;
		this.executionLogic = executionLogic;
	}

	public override void Init()
	{
		envData = MainGame.Instance.GameSave.environmentData;
		float timeOfDay = envData.TimeOfDay;
		float x = timeOfDay + periodTime;
		x = MathF.Round(x, 3);
		if (execTime < 0f)
		{
			execDay = (int)x;
			execTime = x - (float)execDay;
			if (execDay == 0 && execTime < timeOfDay)
			{
				execDay++;
			}
		}
	}

	protected override void UpdateTimer()
	{
		float num = (float)envData.Day + envData.TimeOfDay;
		execTime = num + periodTime;
		execTime = MathF.Round(execTime, 3);
		execDay += (int)execTime;
		execTime %= 1f;
	}

	public override void TryExecute()
	{
		if (TryConsumeFireSlot())
		{
			UpdateTimer();
			Debug.Log("[CustomGameLogicData]: [" + id + "] executing");
			executionLogic?.Invoke();
		}
	}
}
