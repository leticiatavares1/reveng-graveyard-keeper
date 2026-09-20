using System;
using LazyBearTechnology;

public class ZombieEquipmentInventoryWidgetData : InventoryWidgetDataBase
{
	private ZombieWgoData zombie;

	private MultiInventory currentMultiInventory;

	private Item itemToReplace;

	public ZombieWgoData ZombieWgoData => zombie;

	public ZombieEquipmentInventoryWidgetData()
		: base(Inventory.GetDefault(), null, null, null, null, null)
	{
	}

	public ZombieEquipmentInventoryWidgetData(Inventory inventory, Action<UIItemCell> onItemCellOver, Action<UIItemCell> onItemCellOut, Action<UIItemCell> onItemCellPress, Action<UIItemCell> onItemCellPress2, Action<UIItemCell> onItemCellDown, Func<Item, bool> itemsAvailableCondition = null, Func<Item, bool> customItemsNotShowCondition = null, ItemRelatedWidgetState widgetState = ItemRelatedWidgetState.Default)
		: base(inventory, onItemCellOver, onItemCellOut, onItemCellPress, onItemCellPress2, onItemCellDown, itemsAvailableCondition, customItemsNotShowCondition, widgetState)
	{
	}

	public ZombieEquipmentInventoryWidgetData(ZombieWgoData wgoData, Inventory inventory, Action<UIItemCell> onItemCellOver, Action<UIItemCell> onItemCellOut, bool isBlocked = false)
		: this(inventory, onItemCellOver, onItemCellOut, null, null, null)
	{
		zombie = wgoData;
	}

	public void OnArmorCellPressed(UIItemCell itemCell)
	{
		if (itemCell.DisplayingItem == null)
		{
			UIMultiInventoryWindow window = LazyUI.GetWindow<UIMultiInventoryWindow>();
			UIMultiInventoryWindowData uIMultiInventoryWindowData = new UIMultiInventoryWindowData(MainGame.PlayerData, InsertItem, IsArmor, addCurrentPlayerWorldZone: false);
			window.Open(uIMultiInventoryWindowData);
			currentMultiInventory = uIMultiInventoryWindowData.MultiInventory;
		}
		else
		{
			itemToReplace = itemCell.DisplayingItem;
			UIMultiInventoryWindow window2 = LazyUI.GetWindow<UIMultiInventoryWindow>();
			UIMultiInventoryWindowData uIMultiInventoryWindowData2 = new UIMultiInventoryWindowData(MainGame.PlayerData, ReplaceItem, IsArmor, addCurrentPlayerWorldZone: false);
			window2.Open(uIMultiInventoryWindowData2);
			currentMultiInventory = uIMultiInventoryWindowData2.MultiInventory;
		}
	}

	public void OnArmorCellPressed2(UIItemCell itemCell)
	{
		if (itemCell.DisplayingItem != null)
		{
			Item displayingItem = itemCell.DisplayingItem;
			Item item = new Item(displayingItem.id, displayingItem.Count);
			if (zombie != null)
			{
				zombie.equippedArmor.SetGuid(SGuid.Empty);
			}
			base.Inventory.RemoveItemFromInventoryByUID(itemCell.DisplayingItem);
			new MultiInventory(MainGame.PlayerData).AddItem(item);
			TryUpdateZombie();
		}
	}

	public void OnInstrumentCellPressed(UIItemCell itemCell)
	{
		if (itemCell.DisplayingItem == null)
		{
			UIMultiInventoryWindow window = LazyUI.GetWindow<UIMultiInventoryWindow>();
			UIMultiInventoryWindowData uIMultiInventoryWindowData = new UIMultiInventoryWindowData(MainGame.PlayerData, InsertItem, IsInstrumentOrWeapon, addCurrentPlayerWorldZone: false);
			window.Open(uIMultiInventoryWindowData);
			currentMultiInventory = uIMultiInventoryWindowData.MultiInventory;
		}
		else
		{
			itemToReplace = itemCell.DisplayingItem;
			UIMultiInventoryWindow window2 = LazyUI.GetWindow<UIMultiInventoryWindow>();
			UIMultiInventoryWindowData uIMultiInventoryWindowData2 = new UIMultiInventoryWindowData(MainGame.PlayerData, ReplaceItem, IsInstrumentOrWeapon, addCurrentPlayerWorldZone: false);
			window2.Open(uIMultiInventoryWindowData2);
			currentMultiInventory = uIMultiInventoryWindowData2.MultiInventory;
		}
	}

	public void OnInstrumentCellPressed2(UIItemCell itemCell)
	{
		if (itemCell.DisplayingItem != null)
		{
			Item displayingItem = itemCell.DisplayingItem;
			Item item = new Item(displayingItem.id, displayingItem.Count);
			if (zombie != null)
			{
				zombie.equippedHand.SetGuid(SGuid.Empty);
			}
			base.Inventory.RemoveItemFromInventoryByUID(itemCell.DisplayingItem);
			new MultiInventory(MainGame.PlayerData).AddItem(item);
			TryUpdateZombie();
		}
	}

	public void OnCollarCellPressed(UIItemCell itemCell)
	{
		if (zombie != null && !zombie.Collar.IsEmpty)
		{
			UIMultiInventoryWindow window = LazyUI.GetWindow<UIMultiInventoryWindow>();
			UIMultiInventoryWindowData uIMultiInventoryWindowData = new UIMultiInventoryWindowData(MainGame.PlayerData, UpgradeCollar, zombie.CanUpgradeCollarTo, addCurrentPlayerWorldZone: false, "zombie_collar");
			window.Open(uIMultiInventoryWindowData);
			currentMultiInventory = uIMultiInventoryWindowData.MultiInventory;
		}
	}

	private void UpgradeCollar(UIItemCell itemCell)
	{
		Item displayingItem = itemCell.DisplayingItem;
		if (zombie != null && zombie.CanUpgradeCollarTo(displayingItem))
		{
			Item collar = zombie.Collar;
			if (base.Inventory.TryAddItemToInventory(new Item(displayingItem.id), out var addedItems))
			{
				zombie.equippedCollar.SetGuid(addedItems[0].UniqueId);
				base.Inventory.RemoveItemFromInventoryByUID(collar, 1);
				currentMultiInventory.RemoveItemFromInventoryByUID(displayingItem, 1);
				base.Inventory.ForceTriggerOnItemsAddEventWithoutItems();
			}
		}
		LazyUI.GetWindow<UIMultiInventoryWindow>().Close();
	}

	public void ReplaceItem(UIItemCell itemCell)
	{
		Item item = new Item(itemToReplace.id, itemToReplace.Count);
		Item item2 = new Item(itemCell.DisplayingItem.id, itemCell.DisplayingItem.Count);
		base.Inventory.RemoveItemFromInventoryByUID(itemToReplace);
		currentMultiInventory.RemoveItemFromInventoryByUID(itemCell.DisplayingItem);
		currentMultiInventory.AddItem(item);
		if (base.Inventory.TryAddItemToInventory(item2, out var addedItems) && zombie != null)
		{
			Item item3 = addedItems[0];
			if (IsArmor(item3))
			{
				zombie.equippedArmor.SetGuid(item3.UniqueId);
				base.Inventory.ForceTriggerOnItemsAddEventWithoutItems();
			}
			else if (IsInstrumentOrWeapon(item3))
			{
				zombie.equippedHand.SetGuid(item3.UniqueId);
				base.Inventory.ForceTriggerOnItemsAddEventWithoutItems();
			}
		}
		itemToReplace = null;
		LazyUI.GetWindow<UIMultiInventoryWindow>().Close();
		TryUpdateZombie();
	}

	private void InsertItem(UIItemCell itemCell)
	{
		if (base.Inventory.TryAddItemToInventory(itemCell.DisplayingItem, out var addedItems) && zombie != null)
		{
			Item item = addedItems[0];
			if (IsArmor(item))
			{
				zombie.equippedArmor.SetGuid(item.UniqueId);
				base.Inventory.ForceTriggerOnItemsAddEventWithoutItems();
			}
			else if (IsInstrumentOrWeapon(item))
			{
				zombie.equippedHand.SetGuid(item.UniqueId);
				base.Inventory.ForceTriggerOnItemsAddEventWithoutItems();
			}
		}
		currentMultiInventory.RemoveItemFromInventoryByUID(itemCell.DisplayingItem);
		LazyUI.GetWindow<UIMultiInventoryWindow>().Close();
		TryUpdateZombie();
	}

	private void TryUpdateZombie()
	{
		if (zombie != null)
		{
			switch (zombie.ZombieType)
			{
			case ZombieType.Crafter:
				zombie.CrafterOnToolChanged();
				break;
			case ZombieType.ConveyorCrafter:
				zombie.ConveyorCrafterOnToolChanged();
				break;
			case ZombieType.Worker:
				zombie.CrafterOnToolChanged();
				break;
			case ZombieType.Gardener:
				zombie.GardenerOnToolChanged();
				break;
			case ZombieType.Fighter:
				zombie.FighterOnEquipmentChange();
				break;
			default:
				zombie.FighterOnEquipmentChange();
				break;
			case ZombieType.Free:
			case ZombieType.Caretaker:
			case ZombieType.Porter:
				break;
			}
		}
	}

	private bool IsInstrumentOrWeapon(Item item)
	{
		if (item != null && (item.Definition.isTool || item.Definition.isWeapon))
		{
			return item.Definition.type != ItemType.Sword;
		}
		return false;
	}

	private bool IsArmor(Item item)
	{
		if (item != null)
		{
			return item.Definition.type == ItemType.BodyArmor;
		}
		return false;
	}
}
