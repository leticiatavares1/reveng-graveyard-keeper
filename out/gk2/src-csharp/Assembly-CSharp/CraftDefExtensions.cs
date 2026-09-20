using System.Collections.Generic;

public static class CraftDefExtensions
{
	public static bool IsOneTimeCraft(this CraftDefBase craftDef)
	{
		if (!(craftDef is CraftDef { isOneTimeCraft: var isOneTimeCraft }))
		{
			if (!(craftDef is SurveyDef { isOneTimeCraft: var isOneTimeCraft2 }))
			{
				return false;
			}
			return isOneTimeCraft2;
		}
		return isOneTimeCraft;
	}

	public static bool ShouldSkipDuplicateOneTimeCraft(bool incomingIsStarted, bool existingIsStarted)
	{
		return !incomingIsStarted || existingIsStarted;
	}

	public static bool CanActuallyStartCraft(this CraftDef craftDef, WgoData wgoData)
	{
		CraftParamsData paramsData = new CraftParamsData(craftDef.id, wgoData);
		if (!craftDef.isStarCraft && wgoData.Worker?.GetMasteryLevelForTalentBranch(wgoData.Definition.talent, craftDef) < craftDef.talentLock)
		{
			return false;
		}
		if (craftDef.needItems == null || craftDef.needItems.Count == 0)
		{
			return craftDef.CanActuallyStartCraftWithNeeds(new List<NeedItemData>(), paramsData, wgoData);
		}
		MultiInventory craftableMultiInventory = wgoData.GetCraftableMultiInventory();
		List<List<NeedItemData>> list = ExpandNeedItemCombinations(craftDef.needItems, craftableMultiInventory, wgoData);
		if (list.Count == 0)
		{
			return false;
		}
		foreach (List<NeedItemData> item in list)
		{
			if (craftDef.CanActuallyStartCraftWithNeeds(item, paramsData, wgoData))
			{
				return true;
			}
		}
		return false;
	}

	private static List<List<NeedItemData>> ExpandNeedItemCombinations(List<NeedItemData> needItems, MultiInventory multiInventory, WgoData wgoData)
	{
		List<List<NeedItemData>> list = new List<List<NeedItemData>>();
		foreach (NeedItemData needItem in needItems)
		{
			List<NeedItemData> needItemVariants = GetNeedItemVariants(needItem, multiInventory, wgoData);
			if (needItemVariants.Count == 0)
			{
				return new List<List<NeedItemData>>();
			}
			if (list.Count == 0)
			{
				foreach (NeedItemData item in needItemVariants)
				{
					list.Add(new List<NeedItemData>
					{
						new NeedItemData(item.id, item.count)
					});
				}
				continue;
			}
			List<List<NeedItemData>> list2 = new List<List<NeedItemData>>();
			foreach (List<NeedItemData> item2 in list)
			{
				foreach (NeedItemData item3 in needItemVariants)
				{
					List<NeedItemData> list3 = new List<NeedItemData>(item2);
					list3.Add(new NeedItemData(item3.id, item3.count));
					list2.Add(list3);
				}
			}
			list = list2;
		}
		return list;
	}

	private static List<NeedItemData> GetNeedItemVariants(NeedItemData needItemData, MultiInventory multiInventory, WgoData wgoData)
	{
		if (!needItemData.IsGroup)
		{
			return new List<NeedItemData>
			{
				new NeedItemData(needItemData.id, needItemData.count)
			};
		}
		if (!needItemData.TryGetGroupItemDefs(out var groupItemDefs) || groupItemDefs == null || groupItemDefs.Count == 0)
		{
			return new List<NeedItemData>();
		}
		List<NeedItemData> list = new List<NeedItemData>();
		List<NeedItemData> list2 = new List<NeedItemData>();
		foreach (ItemDef item in groupItemDefs)
		{
			NeedItemData needItemData2 = new NeedItemData(item.id, needItemData.count);
			list.Add(needItemData2);
			if (multiInventory != null && multiInventory.HasItemQuantity(needItemData2.Id, needItemData2.GetCount(wgoData)))
			{
				list2.Add(needItemData2);
			}
		}
		if (list2.Count > 0)
		{
			return list2;
		}
		return new List<NeedItemData> { list[0] };
	}

	private static bool CanActuallyStartCraftWithNeeds(this CraftDef craftDef, List<NeedItemData> needItemDatas, CraftParamsData paramsData, WgoData wgoData)
	{
		paramsData.RecalculateParams(needItemDatas, wgoData.Worker);
		CraftElement craftElement = new CraftElement(craftDef.id, 1, needItemDatas, paramsData);
		if (wgoData.CraftComponent.GetStartCraftStatus(craftElement) == CraftStatus.OK)
		{
			return true;
		}
		return false;
	}

	public static bool CanActuallyStartInstantCraft(this CraftDef craftDef, List<NeedItemData> needItemDatas, WgoData wgoData)
	{
		CraftParamsData craftParamsData = new CraftParamsData(craftDef.id, wgoData);
		craftParamsData.RecalculateParams(needItemDatas, wgoData.Worker);
		CraftElement craftElement = new CraftElement(craftDef.id, 1, needItemDatas, craftParamsData);
		craftElement.DoBeforeStartCalculations(wgoData);
		if (wgoData.CraftComponent.GetStartCraftStatus(craftElement) == CraftStatus.OK)
		{
			return craftElement.CanFinishCraft(wgoData) == CraftStatus.OK;
		}
		return false;
	}
}
