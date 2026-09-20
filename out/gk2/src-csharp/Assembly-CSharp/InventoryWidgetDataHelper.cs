using System;
using System.Collections.Generic;

public static class InventoryWidgetDataHelper
{
	public static List<InventoryWidgetDataBase> GetWidgetsDataForInventory(Inventory inventory, Action<UIItemCell> onItemCellOver, Action<UIItemCell> onItemCellOut, Action<UIItemCell> onItemCellPress, Action<UIItemCell> onItemCellPress2, Action<UIItemCell> onItemCellDown = null, Func<Item, bool> itemsAvailableCondition = null, bool addBags = true, bool disableHeaderForFirstWidget = false, ItemRelatedWidgetState customState = ItemRelatedWidgetState.Default, ItemRelatedWidgetState customStateBags = ItemRelatedWidgetState.Default)
	{
		List<InventoryWidgetDataBase> list = new List<InventoryWidgetDataBase>();
		InventoryWidgetData item = new InventoryWidgetData(inventory, disableHeaderForFirstWidget ? new InventoryHeaderWidgetData() : new InventoryHeaderWidgetData(inventory), onItemCellOver, onItemCellOut, onItemCellPress, onItemCellPress2, onItemCellDown, itemsAvailableCondition, null, customState);
		list.Add(item);
		if (addBags)
		{
			foreach (Item item2 in inventory.Data.Inventory)
			{
				if (item2.IsBag)
				{
					Inventory inventoryFromBag = Inventory.GetInventoryFromBag(item2, inventory);
					BagInventoryWidgetData bagInventoryWidgetData = new BagInventoryWidgetData(inventoryFromBag, new InventoryHeaderWidgetData(inventoryFromBag, "comm-header_2-type_icon-simple_bag", inventoryFromBag.Data.id), onItemCellOver, onItemCellOut, onItemCellPress, onItemCellPress2, onItemCellDown, itemsAvailableCondition, null, customStateBags);
					bagInventoryWidgetData.ParentInventory = inventory;
					list.Add(bagInventoryWidgetData);
				}
			}
		}
		return list;
	}

	public static List<InventoryWidgetDataBase> GetWidgetsDataForMultiInventory(MultiInventory multiInventory, Action<UIItemCell> onItemCellOver, Action<UIItemCell> onItemCellOut, Action<UIItemCell> onItemCellPress, Action<UIItemCell> onItemCellPress2, Action<UIItemCell> onItemCellDown, Func<Item, bool> itemsAvailableCondition = null, bool addBags = true, bool disableHeaderForFirstWidget = false, ItemRelatedWidgetState customState = ItemRelatedWidgetState.Default, ItemRelatedWidgetState customStateBags = ItemRelatedWidgetState.Default)
	{
		List<InventoryWidgetDataBase> list = new List<InventoryWidgetDataBase>();
		foreach (Inventory inventory in multiInventory.inventoryList)
		{
			list.AddRange(GetWidgetsDataForInventory(inventory, onItemCellOver, onItemCellOut, onItemCellPress, onItemCellPress2, onItemCellDown, itemsAvailableCondition, addBags, disableHeaderForFirstWidget, customState, customStateBags));
			disableHeaderForFirstWidget = false;
		}
		return list;
	}
}
