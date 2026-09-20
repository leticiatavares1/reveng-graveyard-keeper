using System;
using System.Collections.Generic;
using LazyBearTechnology;

public class UIBoostElementData : LazyWidgetDataBase
{
	public CraftElement CraftElement { get; private set; }

	public Action<List<NeedItemData>> OnPress { get; private set; }

	public Func<List<NeedItemData>, bool> CanCraft { get; private set; }

	public Action OnOver { get; private set; }

	public Action OnOut { get; private set; }

	public List<UICraftItemCellData> CraftItemCellsData { get; private set; }

	public MultiInventory MultiInventory { get; private set; }

	public string Name { get; private set; }

	public string Description { get; private set; }

	public WorldZoneData WorldZoneData { get; private set; }

	public WgoData WgoData { get; private set; }

	public UIBoostElementData(CraftElement craftElement, MultiInventory multiInventory, Action<CraftElement, List<NeedItemData>> onPress, Func<CraftElement, List<NeedItemData>, bool> canCraft, Action onOver, Action onOut, WorldZoneData worldZoneData, WgoData wgoData = null)
	{
		UIBoostElementData uIBoostElementData = this;
		CraftElement = craftElement;
		MultiInventory = multiInventory;
		WorldZoneData = worldZoneData;
		WgoData = wgoData;
		OnPress = OnPressAction;
		CanCraft = CanCraftFunc;
		OnOver = onOver;
		OnOut = onOut;
		FillCraftItemCellsData();
		Name = craftElement.Def.id;
		Description = craftElement.Definition.GetBoostRunesAsString();
		bool CanCraftFunc(List<NeedItemData> needItems)
		{
			return canCraft(uIBoostElementData.CraftElement, needItems);
		}
		void OnPressAction(List<NeedItemData> needItems)
		{
			onPress?.Invoke(uIBoostElementData.CraftElement, needItems);
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
		if (CraftElement.Def.needItems == null)
		{
			return;
		}
		foreach (NeedItemData needItem in CraftElement.Def.needItems)
		{
			CraftItemCellsData.Add(new UICraftItemCellData(needItem, MultiInventory, null, 0f, "", WgoData));
		}
	}
}
