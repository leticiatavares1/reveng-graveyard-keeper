using System;

public class InventoryWidgetData : InventoryWidgetDataBase
{
	public Action<InventoryWidget> OnWidgetPressed { get; set; }

	public InventoryHeaderWidgetData InventoryHeaderWidgetData { get; set; }

	public InventoryWidgetData(Inventory inventory, InventoryHeaderWidgetData inventoryHeaderWidgetData, Action<UIItemCell> onItemCellOver, Action<UIItemCell> onItemCellOut, Action<UIItemCell> onItemCellPress, Action<UIItemCell> onItemCellPress2, Action<UIItemCell> onItemCellDown, Func<Item, bool> itemsAvailableCondition = null, Func<Item, bool> customItemsNotShowCondition = null, ItemRelatedWidgetState widgetState = ItemRelatedWidgetState.Default, Action<InventoryWidget> onWidgetPressed = null)
		: base(inventory, onItemCellOver, onItemCellOut, onItemCellPress, onItemCellPress2, onItemCellDown, itemsAvailableCondition, customItemsNotShowCondition, widgetState)
	{
		InventoryHeaderWidgetData = inventoryHeaderWidgetData;
		OnWidgetPressed = onWidgetPressed;
	}
}
