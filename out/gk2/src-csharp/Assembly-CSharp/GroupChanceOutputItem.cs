using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GroupChanceOutputItem
{
	public string outputGroupId;

	public List<ChanceOutputItem> chanceItems = new List<ChanceOutputItem>();

	public List<ItemCount> MakePreOutput(WgoData wgoData, float resultForQualityRoll = 0f)
	{
		float num = UnityEngine.Random.Range(0f, GetChanceSum(wgoData));
		float num2 = 0f;
		List<ItemCount> result = null;
		foreach (ChanceOutputItem chanceItem in chanceItems)
		{
			num2 += chanceItem.chance.EvaluateFloat(wgoData);
			if (num2 >= num)
			{
				result = chanceItem.MakePreOutputAsGroupItem(wgoData, resultForQualityRoll);
				break;
			}
		}
		return result;
	}

	private float GetChanceSum(WgoData wgoData)
	{
		float num = 0f;
		foreach (ChanceOutputItem chanceItem in chanceItems)
		{
			num += chanceItem.chance.EvaluateFloat(wgoData);
		}
		return num;
	}
}
