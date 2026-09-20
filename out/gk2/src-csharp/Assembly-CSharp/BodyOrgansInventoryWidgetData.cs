using System;

public class BodyOrgansInventoryWidgetData : InventoryWidgetDataBase
{
	public WgoData InteractionWgo { get; private set; }

	public ZombieWgoData ZombieWgoData { get; private set; }

	public bool IsActive { get; private set; }

	public BodyOrgansInventoryWidgetData()
		: this(Inventory.GetDefault(), null, null, null, null, null)
	{
	}

	public BodyOrgansInventoryWidgetData(Inventory inventory, Action<UIItemCell> onItemCellOver, Action<UIItemCell> onItemCellOut, Action<UIItemCell> onItemCellPress, Action<UIItemCell> onItemCellPress2, Action<UIItemCell> onItemCellDown, Func<Item, bool> itemsAvailableCondition = null, Func<Item, bool> customItemsNotShowCondition = null, ItemRelatedWidgetState widgetState = ItemRelatedWidgetState.Default)
		: base(inventory, onItemCellOver, onItemCellOut, onItemCellPress, onItemCellPress2, onItemCellDown, itemsAvailableCondition, customItemsNotShowCondition, widgetState)
	{
	}

	public BodyOrgansInventoryWidgetData(bool isActive, WgoData wgoData, ZombieWgoData zombieWgoData, Inventory bodyItemInventory, Action<UIItemCell> onItemCellOver, Action<UIItemCell> onItemCellOut, Action<UIItemCell> onItemCellPress = null)
		: this(bodyItemInventory, onItemCellOver, onItemCellOut, onItemCellPress, null, null)
	{
		InteractionWgo = wgoData;
		ZombieWgoData = zombieWgoData;
		IsActive = isActive;
	}

	public bool HasAllMainOrgans()
	{
		for (int i = 0; i < LazyConsts.MAIN_ORGANS_TYPES.Count; i++)
		{
			if (!base.Inventory.Data.HasItemsByItemType(LazyConsts.MAIN_ORGANS_TYPES[i]))
			{
				return false;
			}
			if (base.Inventory.Data.GetItemByType(LazyConsts.MAIN_ORGANS_TYPES[i]).Definition.isOrganMistake)
			{
				return false;
			}
		}
		return true;
	}
}
