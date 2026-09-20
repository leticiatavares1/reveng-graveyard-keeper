using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ChanceOutputItem
{
	public string id;

	public string outputGroupId;

	public LazyExpression count = new LazyExpression();

	public LazyExpression minValue = new LazyExpression();

	public LazyExpression maxValue = new LazyExpression();

	public LazyExpression chance = new LazyExpression();

	public bool isStarGroup;

	public bool HasMinValueExpression => minValue.HasExpression;

	public List<ItemCount> MakePreOutput(WgoData wgoData, float resultForQualityRoll = 0f)
	{
		if (!chance.HasExpression && !HasMinValueExpression)
		{
			return MakePreOutputSingle(wgoData, resultForQualityRoll);
		}
		if (!chance.HasExpression && HasMinValueExpression)
		{
			return MakePreOutputRandomAmount(wgoData, resultForQualityRoll);
		}
		return MakePreOutputChance(wgoData, resultForQualityRoll);
	}

	public List<ItemCount> MakePreOutputAsGroupItem(WgoData wgoData, float resultForQualityRoll = 0f)
	{
		if (!HasMinValueExpression)
		{
			return MakePreOutputSingle(wgoData, resultForQualityRoll);
		}
		return MakePreOutputRandomAmount(wgoData, resultForQualityRoll);
	}

	private List<ItemCount> MakePreOutputSingle(WgoData wgoData, float resultForQualityRoll = 0f)
	{
		return MakePreOutputItemsListFromCount(count.EvaluateInt(wgoData), resultForQualityRoll, IsGardenGrowingCraft(wgoData));
	}

	private List<ItemCount> MakePreOutputRandomAmount(WgoData wgoData, float resultForQualityRoll = 0f)
	{
		return MakePreOutputItemsListFromCount(UnityEngine.Random.Range(minValue.EvaluateInt(wgoData), maxValue.EvaluateInt(wgoData) + 1), resultForQualityRoll, IsGardenGrowingCraft(wgoData));
	}

	private List<ItemCount> MakePreOutputChance(WgoData wgoData, float resultForQualityRoll = 0f)
	{
		if (UnityEngine.Random.Range(0f, 1f) <= chance.EvaluateFloat(wgoData))
		{
			if (!HasMinValueExpression)
			{
				return MakePreOutputSingle(wgoData, resultForQualityRoll);
			}
			return MakePreOutputRandomAmount(wgoData, resultForQualityRoll);
		}
		return new List<ItemCount>();
	}

	private static bool IsGardenGrowingCraft(WgoData wgoData)
	{
		if (wgoData == null)
		{
			return false;
		}
		return wgoData.CraftComponent?.CurrentCraftElement?.ParamsData.craftParamsType == CraftParamsData.CraftParamsType.GardenGrowing;
	}

	private List<ItemCount> MakePreOutputItemsListFromCount(int amount, float resultForQualityRoll = 0f, bool forceCommonOutputRule = false)
	{
		List<ItemCount> outputItems = new List<ItemCount>();
		if (isStarGroup && !forceCommonOutputRule)
		{
			if (resultForQualityRoll == 0f)
			{
				return outputItems;
			}
			int num = (int)resultForQualityRoll;
			Debug.Log(string.Format("Made Common craft output as star item [{0}] with quality [{1}]", id + ":" + num, num));
			for (int i = 0; i < amount; i++)
			{
				AddOutputItemToList(outputItems, num);
			}
		}
		else
		{
			Debug.Log("Made craft output as common item [" + id + "]");
			FillPreOutputFromAmount(ref outputItems, id, amount, GameBalance.Me.GetData<ItemDef>(id).stackCount);
		}
		return outputItems;
	}

	private void AddOutputItemToList(List<ItemCount> outputItems, int resultQuality)
	{
		ItemDef itemDef = GameBalance.Me.GetData<ItemDef>(id + ":" + resultQuality);
		int num = outputItems.FindIndex((ItemCount x) => x.itemId == itemDef.id && x.count < itemDef.stackCount);
		if (num != -1)
		{
			outputItems[num].count++;
		}
		else
		{
			outputItems.Add(new ItemCount(itemDef.id, 1));
		}
	}

	private void FillPreOutputFromAmount(ref List<ItemCount> outputItems, string itemId, int amount, int stackCount)
	{
		while (amount > 0)
		{
			if (amount >= stackCount)
			{
				outputItems.Add(new ItemCount(itemId, stackCount));
				amount -= stackCount;
			}
			else
			{
				outputItems.Add(new ItemCount(itemId, amount));
				amount = -1;
			}
		}
	}
}
