using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class PlayerInventoryUIItemOpHandler
{
	private PlayerData playerData;

	private MultiInventoryWidgetData playerMultiInventoryWidgetData;

	private bool isBagShown;

	private BagInventoryWidgetData bagInventoryWidgetData;

	private Action<Item> onBagHide;

	private Action<Item> onBagShow;

	private Item shownBag;

	public bool IsBagShown => isBagShown;

	public BagInventoryWidgetData BagInventoryWidgetData => bagInventoryWidgetData;

	public Action<Item> OnBagHide
	{
		get
		{
			return onBagHide;
		}
		set
		{
			onBagHide = value;
		}
	}

	public Action<Item> OnBagShow
	{
		get
		{
			return onBagShow;
		}
		set
		{
			onBagShow = value;
		}
	}

	public PlayerInventoryUIItemOpHandler(PlayerData playerData)
	{
		this.playerData = playerData;
	}

	public PlayerInventoryUIItemOpHandler(PlayerData playerData, MultiInventoryWidgetData playerMultiInventoryWidgetData)
	{
		this.playerData = playerData;
		this.playerMultiInventoryWidgetData = playerMultiInventoryWidgetData;
	}

	public void OnPlayerInvItemPressed(UIItemCell cell)
	{
		if (isBagShown)
		{
			if (cell.DisplayingItem.IsBag)
			{
				if (IsShownBag(cell.DisplayingItem))
				{
					HideBag();
				}
				else
				{
					ShowBag(cell);
				}
				return;
			}
			InventoryUIItemMoveOpHandler inventoryUIItemMoveOpHandler = new InventoryUIItemMoveOpHandler(() => playerMultiInventoryWidgetData.SelectedWidgetData.Inventory, () => bagInventoryWidgetData.Inventory, shownBag);
			inventoryUIItemMoveOpHandler.Silent = true;
			inventoryUIItemMoveOpHandler.OnInventory1ItemPress1(cell);
		}
		else if (cell.DisplayingItem.IsBag)
		{
			ShowBag(cell);
		}
		else
		{
			UIContextMenuWindowData uIContextMenuWindowData = new UIContextMenuWindowData();
			uIContextMenuWindowData.Position = (LazyInput.IsGamepadActive ? cell.transform.position : Input.mousePosition);
			uIContextMenuWindowData.Options = new List<UIContextMenuWindowWidgetData>();
			FillContextPressData(cell, cell.DisplayingItem, uIContextMenuWindowData);
			if (uIContextMenuWindowData.Options.Count > 0)
			{
				uIContextMenuWindowData.Options[0].callback?.Invoke();
			}
		}
	}

	public void OnPlayerInvItemPressed2(UIItemCell cell)
	{
		Item displayingItem = cell.DisplayingItem;
		if (isBagShown)
		{
			if (!displayingItem.IsBag)
			{
				InventoryUIItemMoveOpHandler inventoryUIItemMoveOpHandler = new InventoryUIItemMoveOpHandler(() => playerMultiInventoryWidgetData.SelectedWidgetData.Inventory, () => bagInventoryWidgetData.Inventory, shownBag);
				inventoryUIItemMoveOpHandler.Silent = true;
				inventoryUIItemMoveOpHandler.OnInventory1ItemPress2(cell);
			}
			return;
		}
		UIContextMenuWindowData uIContextMenuWindowData = new UIContextMenuWindowData();
		uIContextMenuWindowData.Position = (LazyInput.IsGamepadActive ? cell.transform.position : Input.mousePosition);
		uIContextMenuWindowData.Options = new List<UIContextMenuWindowWidgetData>();
		FillContextPressData(cell, displayingItem, uIContextMenuWindowData);
		uIContextMenuWindowData.Options.Add(new UIContextMenuWindowWidgetData(LLBase.L("ui_destroy"), delegate
		{
			TryDestroyItem(cell);
			LazyUI.GetWindow<UIContextMenuWindow>().Close();
		}, !displayingItem.Definition.CanNotBeDestroyed));
		LazyAudio.PlayAndForget("gui_click");
		LazyUI.GetWindow<UIContextMenuWindow>().Open(uIContextMenuWindowData);
	}

	public void OnPlayerInventoryPressedDown(UIItemCell cell)
	{
	}

	private void FillContextPressData(UIItemCell cell, Item item, UIContextMenuWindowData contextMenuWindowData)
	{
		bool num = item.Definition.CanItemBeEquipped();
		bool canBeUsed = item.Definition.CanBeUsed;
		bool enabled = LazySingleton<FightingGameController>.Instance.CurrentFightState == FightState.Disabled;
		if (!num)
		{
			if (item.IsBag)
			{
				contextMenuWindowData.Options.Add(new UIContextMenuWindowWidgetData(LLBase.L("ui_open"), delegate
				{
					ShowBag(cell);
					LazyUI.GetWindow<UIContextMenuWindow>().Close();
				}));
			}
			else if (item.IsSeed)
			{
				contextMenuWindowData.Options.Add(new UIContextMenuWindowWidgetData(LLBase.L("ui_plant"), delegate
				{
					TrySetInteractingItem(cell);
					LazyUI.GetWindow<UIContextMenuWindow>().Close();
				}, enabled));
			}
			else if (item.IsFertilizer)
			{
				contextMenuWindowData.Options.Add(new UIContextMenuWindowWidgetData(LLBase.L("ui_fertilize"), delegate
				{
					TrySetInteractingItem(cell);
					LazyUI.GetWindow<UIContextMenuWindow>().Close();
				}, enabled));
			}
			else
			{
				contextMenuWindowData.Options.Add(new UIContextMenuWindowWidgetData(LLBase.L("ui_use"), delegate
				{
					TryUseItem(cell);
					LazyUI.GetWindow<UIContextMenuWindow>().Close();
				}, canBeUsed));
			}
			if (item.Definition.CanBePinnedToHotBar)
			{
				contextMenuWindowData.Options.Add(new UIContextMenuWindowWidgetData(LLBase.L("ui_pin_hot_bar"), delegate
				{
					TryPinItemToHotBar(cell);
					LazyUI.GetWindow<UIContextMenuWindow>().Close();
				}));
			}
		}
		else
		{
			contextMenuWindowData.Options.Add(new UIContextMenuWindowWidgetData(LLBase.L("ui_equip"), delegate
			{
				TryEquipItem(cell);
				LazyUI.GetWindow<UIContextMenuWindow>().Close();
			}));
		}
	}

	public bool PlayerItemsAvailabilityCondition(Item item)
	{
		if (item == null)
		{
			return true;
		}
		if (isBagShown)
		{
			if (item.IsBag)
			{
				return true;
			}
			return item.Definition.CanBeInsertedInBag(bagInventoryWidgetData.Inventory.Data.Definition);
		}
		return true;
	}

	public bool ToolBeltItemsAvailabilityCondition(Item item)
	{
		if (item == null)
		{
			return true;
		}
		if (isBagShown)
		{
			return false;
		}
		if (LazySingleton<FightingGameController>.Instance.CurrentFightState != 0)
		{
			return !item.Definition.IsFightingEquipment();
		}
		return true;
	}

	public void TryUnEquipItem(UIItemCell cell)
	{
		if (cell.DisplayingItem != null && !cell.DisplayingItem.IsEmpty)
		{
			UINotificator.isSilent = true;
			Inventory toolBeltInventory = MainGame.PlayerData.toolBeltInventory;
			Inventory inventory = MainGame.PlayerData.inventory;
			Item displayingItem = cell.DisplayingItem;
			if (inventory.CanAddItemToInventory(displayingItem))
			{
				toolBeltInventory.RemoveItemFromInventoryByUID(displayingItem);
				inventory.AddItemToInventory(displayingItem);
				LazyAudio.PlayAndForget("unequip_tool");
			}
			UINotificator.isSilent = false;
		}
	}

	public void TryEquipItem(UIItemCell cell)
	{
		if (cell.DisplayingItem != null && cell.DisplayingItem.Definition.CanItemBeEquipped())
		{
			UINotificator.isSilent = true;
			Inventory toolBeltInventory = playerData.toolBeltInventory;
			Inventory inventory = playerData.Inventory;
			Item itemByType = toolBeltInventory.GetItemByType(cell.DisplayingItem.Definition.type);
			Item displayingItem = cell.DisplayingItem;
			inventory.RemoveItemFromInventoryByUID(cell.DisplayingItem);
			if (itemByType.IsEmpty)
			{
				toolBeltInventory.AddItemToInventory(displayingItem);
			}
			else
			{
				toolBeltInventory.RemoveItemFromInventoryByUID(itemByType);
				toolBeltInventory.AddItemToInventory(displayingItem);
				inventory.AddItemToInventory(itemByType);
			}
			SyncFightEquipment(displayingItem.Definition);
			LazyAudio.PlayAndForget("equip_tool");
			UINotificator.isSilent = false;
		}
	}

	private static void SyncFightEquipment(ItemDef equippedDef)
	{
		if (LazySingleton<FightingGameController>.Instance.CurrentFightState != 0)
		{
			switch (equippedDef.type)
			{
			case ItemType.BodyArmor:
				MainGame.PlayerController.SetArmorView(isActive: true);
				break;
			case ItemType.Sword:
			case ItemType.Bow:
				MainGame.PlayerController.AttackComponent.EquipWeapon(equippedDef);
				break;
			}
		}
	}

	private void TrySetInteractingItem(UIItemCell cell)
	{
		if (cell.DisplayingItem != null && !cell.DisplayingItem.IsEmpty && (cell.DisplayingItem.IsSeed || cell.DisplayingItem.IsFertilizer))
		{
			playerData.SetInteractingItem(cell.DisplayingItem);
			LazyAudio.PlayAndForget("equip_tool");
			LazyUI.GetWindow<CharacterWindow>().Close();
		}
	}

	private void TryUseItem(UIItemCell cell)
	{
		if (cell.DisplayingItem != null && !cell.DisplayingItem.IsEmpty && cell.DisplayingItem.Definition.CanBeUsed)
		{
			playerData.UseItem(cell.DisplayingItem);
		}
	}

	private void TryDestroyItem(UIItemCell cell)
	{
		if (cell.DisplayingItem != null && !cell.DisplayingItem.Definition.CanNotBeDestroyed)
		{
			playerData.Inventory.RemoveItemFromInventoryByUID(cell.DisplayingItem);
		}
	}

	private bool TryPinItemToHotBar(UIItemCell cell)
	{
		if (cell.DisplayingItem == null || cell.DisplayingItem.IsEmpty || !cell.DisplayingItem.Definition.CanBePinnedToHotBar)
		{
			return false;
		}
		LazyUI.GetWindow<UIHotBarSelectionWindow>().Open(new UIHotBarSelectionWindowData(MainGame.Instance.GameSave, cell.DisplayingItem));
		return true;
	}

	public bool IsShownBag(Item item)
	{
		if (isBagShown && item != null && shownBag != null)
		{
			return item.UniqueId == shownBag.UniqueId;
		}
		return false;
	}

	private void ShowBag(UIItemCell cell)
	{
		isBagShown = true;
		shownBag = cell.DisplayingItem;
		Inventory bagInventory = ((playerMultiInventoryWidgetData != null) ? playerMultiInventoryWidgetData.FindBagInventory(cell.DisplayingItem) : null);
		if (bagInventory == null)
		{
			bagInventory = Inventory.GetInventoryFromBag(cell.DisplayingItem, playerData.Inventory);
		}
		else
		{
			bagInventory.ParentInventory = playerData.Inventory;
		}
		InventoryUIItemMoveOpHandler inventoryUIItemMoveOpHandler = new InventoryUIItemMoveOpHandler(() => playerMultiInventoryWidgetData.SelectedWidgetData.Inventory, () => bagInventory, cell.DisplayingItem);
		inventoryUIItemMoveOpHandler.Silent = true;
		bagInventoryWidgetData = new BagInventoryWidgetData(bagInventory, new InventoryHeaderWidgetData(bagInventory, "comm-header_2-type_icon-simple_bag", bagInventory.Data.id), null, null, inventoryUIItemMoveOpHandler.OnInventory2ItemPress1, inventoryUIItemMoveOpHandler.OnInventory2ItemPress2, null);
		bagInventoryWidgetData.ParentInventory = playerData.Inventory;
		onBagShow?.Invoke(shownBag);
	}

	public void HideBag()
	{
		if (isBagShown)
		{
			isBagShown = false;
			onBagHide?.Invoke(shownBag);
		}
	}
}
