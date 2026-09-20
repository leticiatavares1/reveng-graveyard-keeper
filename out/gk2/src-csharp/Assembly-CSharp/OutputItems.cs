using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class OutputItems : IAutoParsable
{
	public List<ChanceOutputItem> chanceOutputItems = new List<ChanceOutputItem>();

	public List<GroupChanceOutputItem> groupChanceOutputItems = new List<GroupChanceOutputItem>();

	public bool HasOutputItems
	{
		get
		{
			if (chanceOutputItems.Count <= 0)
			{
				return groupChanceOutputItems.Count > 0;
			}
			return true;
		}
	}

	public bool HasFirstChanceItemWithMinValueExpression
	{
		get
		{
			if (chanceOutputItems.Count > 0)
			{
				return chanceOutputItems[0].HasMinValueExpression;
			}
			return false;
		}
	}

	public void Add(ChanceOutputItem chanceOutputItem, GameBalance gameBalance)
	{
		if (gameBalance == null)
		{
			Debug.LogError("Error adding chance output item. Current parsable Game balance is null.");
			return;
		}
		chanceOutputItem.isStarGroup = gameBalance.starGroupItemsCache.ContainsKey(chanceOutputItem.id);
		AddChanceOutputItem(chanceOutputItem);
	}

	public void AddChanceOutputItem(ChanceOutputItem chanceOutputItem)
	{
		if (string.IsNullOrEmpty(chanceOutputItem.outputGroupId))
		{
			chanceOutputItems.Add(chanceOutputItem);
			return;
		}
		int num = groupChanceOutputItems.FindIndex((GroupChanceOutputItem x) => x.outputGroupId == chanceOutputItem.outputGroupId);
		if (num == -1)
		{
			GroupChanceOutputItem groupChanceOutputItem = new GroupChanceOutputItem();
			groupChanceOutputItem.outputGroupId = chanceOutputItem.outputGroupId;
			groupChanceOutputItem.chanceItems.Add(chanceOutputItem);
			groupChanceOutputItems.Add(groupChanceOutputItem);
		}
		else
		{
			groupChanceOutputItems[num].chanceItems.Add(chanceOutputItem);
		}
	}

	public List<ItemCount> MakePreOutput(ICraftable craftable, float resultForQualityRoll = 0f)
	{
		WgoData wgoData = craftable as WgoData;
		List<ItemCount> list = new List<ItemCount>();
		foreach (ChanceOutputItem chanceOutputItem in chanceOutputItems)
		{
			list.AddRange(chanceOutputItem.MakePreOutput(wgoData, resultForQualityRoll));
		}
		foreach (GroupChanceOutputItem groupChanceOutputItem in groupChanceOutputItems)
		{
			List<ItemCount> list2 = groupChanceOutputItem.MakePreOutput(wgoData, resultForQualityRoll);
			if (list2 != null)
			{
				list.AddRange(list2);
			}
		}
		return list;
	}

	public static List<Item> MakeOutput(List<ItemCount> preOutputItems)
	{
		List<Item> list = new List<Item>();
		foreach (ItemCount preOutputItem in preOutputItems)
		{
			list.Add(new Item(preOutputItem.itemId, preOutputItem.count));
		}
		return list;
	}

	public bool HasOutput()
	{
		if (chanceOutputItems.Count == 0)
		{
			return groupChanceOutputItems.Count != 0;
		}
		return true;
	}

	public OutputPreview GetOutputPreview(string craftId, WgoData wgoData = null)
	{
		if (chanceOutputItems.Count > 0)
		{
			return TryFormOutputPreview(craftId, chanceOutputItems[0], wgoData);
		}
		if (groupChanceOutputItems.Count > 0)
		{
			return TryFormOutputPreview(craftId, groupChanceOutputItems[0].chanceItems[0], wgoData);
		}
		return null;
	}

	public override string ToString()
	{
		string text = string.Empty;
		if (chanceOutputItems.Count > 0)
		{
			for (int i = 0; i < chanceOutputItems.Count; i++)
			{
				if (i > 0)
				{
					text += ";";
				}
				string text2 = (chanceOutputItems[i].count.HasExpression ? chanceOutputItems[i].count.ToString() : chanceOutputItems[i].minValue.ToString());
				text = text + chanceOutputItems[i].id + "=" + text2;
			}
		}
		else if (groupChanceOutputItems.Count > 0)
		{
			for (int j = 0; j < chanceOutputItems.Count; j++)
			{
				if (j > 0)
				{
					text += ";";
				}
				string text3 = (groupChanceOutputItems[j].chanceItems[0].count.HasExpression ? groupChanceOutputItems[j].chanceItems[0].count.ToString() : groupChanceOutputItems[j].chanceItems[0].minValue.ToString());
				text = text + groupChanceOutputItems[j].chanceItems[0].id + "=" + text3;
			}
		}
		return text;
	}

	private OutputPreview TryFormOutputPreview(string craftId, ChanceOutputItem chanceOutputItem, WgoData wgoData = null)
	{
		OutputPreview outputPreview = null;
		int count = 0;
		if (chanceOutputItem.count.HasExpression)
		{
			count = ((wgoData != null) ? chanceOutputItem.count.EvaluateInt(wgoData) : chanceOutputItem.count.EvaluateInt());
		}
		else if (chanceOutputItem.minValue.HasExpression)
		{
			count = ((wgoData != null) ? chanceOutputItem.minValue.EvaluateInt(wgoData) : chanceOutputItem.minValue.EvaluateInt());
		}
		if (chanceOutputItem.isStarGroup)
		{
			return new OutputPreview(craftId, chanceOutputItem.id, isStarOutput: true, count, 0);
		}
		return new OutputPreview(craftId, chanceOutputItem.id, isStarOutput: false, count);
	}
}
