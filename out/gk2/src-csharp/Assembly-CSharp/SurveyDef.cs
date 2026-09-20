using System;
using System.Collections.Generic;
using LazyBearTechnology;

[Serializable]
public class SurveyDef : CraftDefBase
{
	[AutoParse("surveyed_at_start")]
	public bool surveyedAtStart;

	[AutoParse("on_craft_end_expressions")]
	public List<LazyExpression> onCraftEndExpressions = new List<LazyExpression>();

	[AutoParse("tech_r")]
	public int techRed;

	[AutoParse("tech_g")]
	public int techGreen;

	[AutoParse("tech_b")]
	public int techBlue;

	public bool isOneTimeCraft;

	public bool isScienceFuelCraft;

	private OutputPreview outputPreview;

	public NeedItemData SurveyedItem
	{
		get
		{
			if (needItems.Count <= 0)
			{
				return null;
			}
			return needItems[0];
		}
	}

	public bool IsSurveyForItem(ItemDef itemDef)
	{
		if (itemDef == null || SurveyedItem == null)
		{
			return false;
		}
		switch (SurveyedItem.groupType)
		{
		case ItemGroup.None:
			return SurveyedItem.id == itemDef.id;
		case ItemGroup.Common:
			return itemDef.itemGroupIds.Contains(SurveyedItem.id);
		case ItemGroup.Star:
			if (itemDef.qualityType == ItemDef.QualityType.Star)
			{
				return itemDef.id.Split(':')[0] == SurveyedItem.id;
			}
			return false;
		default:
			return false;
		}
	}

	public List<ItemDef> GetSurveyedItemDefs()
	{
		List<ItemDef> list = new List<ItemDef>();
		if (SurveyedItem == null)
		{
			return list;
		}
		switch (SurveyedItem.groupType)
		{
		case ItemGroup.None:
		{
			ItemDef dataOrNull = GameBalance.Me.GetDataOrNull<ItemDef>(SurveyedItem.id);
			if (dataOrNull != null)
			{
				list.Add(dataOrNull);
			}
			break;
		}
		case ItemGroup.Common:
		{
			if (GameBalance.Me.groupItemsCache.TryGetValue(SurveyedItem.id, out var value2))
			{
				list.AddRange(value2);
			}
			break;
		}
		case ItemGroup.Star:
		{
			if (GameBalance.Me.starGroupItemsCache.TryGetValue(SurveyedItem.id, out var value))
			{
				list.AddRange(value);
			}
			break;
		}
		}
		return list;
	}

	public override OutputPreview GetOutputPreview(WgoData wgoData = null)
	{
		if (outputPreview == null)
		{
			string text = id.Replace("surv:", string.Empty);
			ItemDef itemDef = GameBalance.Me.GetDataOrNull<ItemDef>(text);
			if (itemDef == null)
			{
				List<ItemDef> surveyedItemDefs = GetSurveyedItemDefs();
				itemDef = ((surveyedItemDefs.Count > 0) ? surveyedItemDefs[0] : null);
			}
			string customIconId = ((itemDef == null) ? ("i_" + text) : itemDef.iconId);
			outputPreview = new OutputPreview(id, "", isStarOutput: false, 1, -1, customIconId);
		}
		return outputPreview;
	}
}
