using System;

public class BodyPocketInventoryWidgetData : InventoryWidgetDataBase
{
	public WgoData InteractionWgo { get; private set; }

	public ZombieWgoData ZombieWgoData { get; private set; }

	public bool IsActive { get; private set; }

	public bool FirstEmptyIsInteractable { get; set; }

	public BodyPocketInventoryWidgetData()
		: this(Inventory.GetDefault(), null, null, null, null, null)
	{
	}

	public BodyPocketInventoryWidgetData(Inventory inventory, Action<UIItemCell> onItemCellOver, Action<UIItemCell> onItemCellOut, Action<UIItemCell> onItemCellPress, Action<UIItemCell> onItemCellPress2, Action<UIItemCell> onItemCellDown, Func<Item, bool> itemsAvailableCondition = null, Func<Item, bool> customItemsNotShowCondition = null, ItemRelatedWidgetState widgetState = ItemRelatedWidgetState.Default)
		: base(inventory, onItemCellOver, onItemCellOut, onItemCellPress, onItemCellPress2, onItemCellDown, itemsAvailableCondition, customItemsNotShowCondition, widgetState)
	{
	}

	public BodyPocketInventoryWidgetData(bool isActive, WgoData wgoData, ZombieWgoData zombieWgoData, Inventory bodyItemInventory, Action<UIItemCell> onItemCellOver, Action<UIItemCell> onItemCellOut, Action<UIItemCell> onItemCellPress = null)
		: this(bodyItemInventory, onItemCellOver, onItemCellOut, onItemCellPress, null, null)
	{
		InteractionWgo = wgoData;
		ZombieWgoData = zombieWgoData;
		IsActive = isActive;
	}
}
