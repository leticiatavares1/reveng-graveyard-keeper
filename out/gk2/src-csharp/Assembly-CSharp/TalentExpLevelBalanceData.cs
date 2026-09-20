using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TalentExpLevelBalanceData
{
	public string id;

	public List<int> expLevels = new List<int>();

	public TalentExpLevelBalanceData()
	{
	}

	public TalentExpLevelBalanceData(string id)
	{
		this.id = id;
		expLevels = new List<int>();
	}

	public int GetExpForLevel(int level)
	{
		if (!HasLevelCorrectValue(level))
		{
			return 0;
		}
		return expLevels[level - 1];
	}

	public int GetExpForNextLevel(int curLevel)
	{
		if (!HasLevelCorrectValue(curLevel))
		{
			return 0;
		}
		return expLevels[curLevel];
	}

	private bool HasLevelCorrectValue(int level)
	{
		if (level < 0 || level >= expLevels.Count)
		{
			Debug.LogError($"Talent [{id}]: level [{level}] is out of range");
			return false;
		}
		return true;
	}
}
