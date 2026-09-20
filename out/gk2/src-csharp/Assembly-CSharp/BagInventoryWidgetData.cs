using System;

public class BagInventoryWidgetData : InventoryWidgetData
{
	public Inventory ParentInventory { get; set; }

	public BagInventoryWidgetData(Inventory inventory, InventoryHeaderWidgetData inventoryHeaderWidgetData, Action<UIItemCell> onItemCellOver, Action<UIItemCell> onItemCellOut, Action<UIItemCell> onItemCellPress, Action<UIItemCell> onItemCellPress2, Action<UIItemCell> onItemCellDown, Func<Item, bool> itemsAvailableCondition = null, Func<Item, bool> customItemsNotShowCondition = null, ItemRelatedWidgetState widgetState = ItemRelatedWidgetState.Default, Action<InventoryWidget> onWidgetPressed = null)
		: base(inventory, inventoryHeaderWidgetData, onItemCellOver, onItemCellOut, onItemCellPress, onItemCellPress2, onItemCellDown, itemsAvailableCondition, customItemsNotShowCondition, widgetState, onWidgetPressed)
	{
	}
}
