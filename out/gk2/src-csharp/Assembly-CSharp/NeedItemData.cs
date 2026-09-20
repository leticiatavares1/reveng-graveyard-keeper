using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NeedItemData : IAutoParsable
{
	public string id;

	public LazyExpression count = new LazyExpression();

	public ItemGroup groupType;

	[NonSerialized]
	private ItemDef itemDef;

	public ItemDef ItemDef
	{
		get
		{
			if (groupType != 0)
			{
				Debug.LogError($"Need Item is a group! Can not get {typeof(ItemDef)}");
				return null;
			}
			if (itemDef == null)
			{
				itemDef = GameBalance.Me.GetDataOrNull<ItemDef>(id);
			}
			return itemDef;
		}
	}

	public string Id => id;

	public bool IsEmpty
	{
		get
		{
			if (!string.IsNullOrEmpty(id) && count != null && count.HasExpression)
			{
				return !count.HasPureValue;
			}
			return true;
		}
	}

	public bool IsGroup => groupType != ItemGroup.None;

	public int GetCount(WgoData wgoData = null)
	{
		if (count == null || !count.HasExpression)
		{
			return 1;
		}
		if (wgoData == null)
		{
			return count.EvaluateInt();
		}
		return count.EvaluateInt(wgoData);
	}

	public bool TryGetGroupItemDefs(out List<ItemDef> groupItemDefs)
	{
		groupItemDefs = null;
		return groupType switch
		{
			ItemGroup.Common => GameBalance.Me.groupItemsCache.TryGetValue(id, out groupItemDefs), 
			ItemGroup.Star => GameBalance.Me.starGroupItemsCache.TryGetValue(id, out groupItemDefs), 
			_ => false, 
		};
	}

	public NeedItemData()
	{
	}

	public NeedItemData(string id, int count)
	{
		Reinitialize(id, count);
	}

	public NeedItemData(string id, LazyExpression count)
	{
		Reinitialize(id, count);
	}

	public static float GetQualitySum(List<NeedItemData> needItems)
	{
		float num = 0f;
		int num2 = 0;
		foreach (NeedItemData needItem in needItems)
		{
			if (needItem.groupType == ItemGroup.None && needItem.ItemDef.qualityType == ItemDef.QualityType.Star)
			{
				num += (float)needItem.ItemDef.quality;
				num2++;
			}
		}
		if (num2 > 0)
		{
			num /= (float)num2;
		}
		return num;
	}

	public static int GetCraftStartTicksBonusValue(List<NeedItemData> needItems)
	{
		int num = 0;
		foreach (NeedItemData needItem in needItems)
		{
			if (needItem.groupType == ItemGroup.None && needItem.ItemDef.qualityType == ItemDef.QualityType.Star)
			{
				num += needItem.ItemDef.quality - 1;
			}
		}
		return num;
	}

	public bool Equals(NeedItemData other)
	{
		if (id == other.id && (count?.GetRawExpressionString() ?? string.Empty) == (other.count?.GetRawExpressionString() ?? string.Empty))
		{
			return groupType == other.groupType;
		}
		return false;
	}

	public void Reinitialize(string id, int count)
	{
		Reinitialize(id, new LazyExpression(count.ToString()));
	}

	public void Reinitialize(string id, LazyExpression count)
	{
		this.id = id;
		this.count = count ?? new LazyExpression();
		if (GameBalance.Me.starGroupItemsCache.ContainsKey(id))
		{
			groupType = ItemGroup.Star;
		}
		else if (GameBalance.Me.groupItemsCache.ContainsKey(id))
		{
			groupType = ItemGroup.Common;
		}
	}

	public override string ToString()
	{
		return $"{id}={count}";
	}
}
