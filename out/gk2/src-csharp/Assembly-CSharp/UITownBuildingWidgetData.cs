using System;
using System.Collections.Generic;
using LazyBearTechnology;

public class UITownBuildingWidgetData : LazyWidgetDataBase
{
	public TownBuildingDef TownBuildingDef { get; private set; }

	public Action<List<NeedItemData>> OnPress { get; private set; }

	public Action OnOver { get; private set; }

	public Action OnOut { get; private set; }

	public List<UICraftItemCellData> CraftItemCellsData { get; private set; }

	public MultiInventory MultiInventory { get; private set; }

	public string Name { get; private set; }

	public string Description { get; private set; }

	public WorldZoneData WorldZoneData { get; private set; }

	public UITownBuildingWidgetData(TownBuildingDef townBuildingDef, MultiInventory multiInventory, Action<TownBuildingDef, List<NeedItemData>> onPress, Action onOver, Action onOut, WorldZoneData worldZoneData)
	{
		UITownBuildingWidgetData uITownBuildingWidgetData = this;
		TownBuildingDef = townBuildingDef;
		MultiInventory = multiInventory;
		WorldZoneData = worldZoneData;
		OnPress = OnPressAction;
		OnOver = onOver;
		OnOut = onOut;
		FillCraftItemCellsData();
		Name = LLBase.L(TownBuildingDef.id);
		void OnPressAction(List<NeedItemData> needItems)
		{
			onPress?.Invoke(uITownBuildingWidgetData.TownBuildingDef, needItems);
		}
	}

	public List<NeedItemData> GetCurrentNeedItems()
	{
		List<NeedItemData> list = new List<NeedItemData>();
		for (int i = 0; i < CraftItemCellsData.Count; i++)
		{
			list.Add(CraftItemCellsData[i].currentItem);
		}
		return list;
	}

	private void FillCraftItemCellsData()
	{
		if (CraftItemCellsData != null)
		{
			CraftItemCellsData.Clear();
		}
		else
		{
			CraftItemCellsData = new List<UICraftItemCellData>();
		}
		foreach (NeedItemData needItem in TownBuildingDef.needItems)
		{
			CraftItemCellsData.Add(new UICraftItemCellData(needItem, MultiInventory, null));
		}
	}
}
