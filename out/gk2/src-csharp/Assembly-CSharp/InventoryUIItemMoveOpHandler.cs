using System;
using LazyBearTechnology;
using UnityEngine;

public class InventoryUIItemMoveOpHandler
{
	private Func<Inventory> inv1;

	private Func<Inventory> inv2;

	private Item ignoreBagWhenAdd;

	private bool silent;

	public bool Silent
	{
		get
		{
			return silent;
		}
		set
		{
			silent = value;
		}
	}

	public InventoryUIItemMoveOpHandler(Func<Inventory> inv1, Func<Inventory> inv2)
	{
		this.inv1 = inv1;
		this.inv2 = inv2;
	}

	public InventoryUIItemMoveOpHandler(Func<Inventory> inv1, Func<Inventory> inv2, Item ignoreBagWhenAdd)
	{
		this.inv1 = inv1;
		this.inv2 = inv2;
		this.ignoreBagWhenAdd = ignoreBagWhenAdd;
	}

	public void OnInventory1ItemPress1(UIItemCell itemCell)
	{
		if (itemCell.DisplayingItem == null)
		{
			return;
		}
		if (itemCell.DisplayingItem.Count <= 1)
		{
			OnInventory1ItemPress2(itemCell);
		}
		else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
		{
			if (TryMoveItem(itemCell, itemCell.DisplayingItem.Count, inv1(), inv2()))
			{
				LazyAudio.PlayAndForget("item_put");
			}
		}
		else if (!TryMoveHalfStackOnCtrlClick(itemCell, inv1(), inv2()))
		{
			OpenItemCountWindow(itemCell, inv1(), inv2());
		}
	}

	public void OnInventory1ItemPress2(UIItemCell itemCell)
	{
		if (TryMoveItem(itemCell, 1, inv1(), inv2()))
		{
			LazyAudio.PlayAndForget("item_put");
		}
	}

	public void OnInventory2ItemPress1(UIItemCell itemCell)
	{
		if (itemCell.DisplayingItem == null)
		{
			return;
		}
		if (itemCell.DisplayingItem.Count <= 1)
		{
			OnInventory2ItemPress2(itemCell);
		}
		else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
		{
			if (TryMoveItem(itemCell, itemCell.DisplayingItem.Count, inv2(), inv1()))
			{
				LazyAudio.PlayAndForget("item_put");
			}
		}
		else if (!TryMoveHalfStackOnCtrlClick(itemCell, inv2(), inv1()))
		{
			OpenItemCountWindow(itemCell, inv2(), inv1());
		}
	}

	public void OnInventory2ItemPress2(UIItemCell itemCell)
	{
		if (TryMoveItem(itemCell, 1, inv2(), inv1()))
		{
			LazyAudio.PlayAndForget("item_put");
		}
	}

	private bool TryMoveHalfStackOnCtrlClick(UIItemCell itemCell, Inventory from, Inventory to)
	{
		if (LazyInput.IsGamepadActive)
		{
			return false;
		}
		if (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl))
		{
			return false;
		}
		if (TryMoveItem(itemCell, itemCell.DisplayingItem.Count / 2, from, to))
		{
			LazyAudio.PlayAndForget("item_put");
		}
		return true;
	}

	private void OpenItemCountWindow(UIItemCell itemCell, Inventory from, Inventory to)
	{
		if (itemCell.DisplayingItem == null)
		{
			return;
		}
		int totalCountInInventory = from.Data.GetTotalCountInInventory(itemCell.DisplayingItem.id, itemCell.DisplayingItem.IsBag ? null : ignoreBagWhenAdd, ignoreAllBags: true);
		int num = to.Data.CanAddItemCountToInventory(itemCell.DisplayingItem.Definition, totalCountInInventory, considerEmptySlots: true, itemCell.DisplayingItem.IsBag ? null : ignoreBagWhenAdd);
		if (num > 0)
		{
			LazyAudio.PlayAndForget("item_put");
			UIItemCountWindowData uIItemCountWindowData = new UIItemCountWindowData();
			uIItemCountWindowData.Item = new Item(itemCell.DisplayingItem.id);
			uIItemCountWindowData.Min = 1;
			uIItemCountWindowData.Max = num;
			uIItemCountWindowData.OnConfirm = delegate(int count)
			{
				TryMoveItem(itemCell, count, from, to);
			};
			uIItemCountWindowData.IsForVendor = false;
			uIItemCountWindowData.OkBtnData = new UIDialogWindowData.ButtonData(null, LLBase.L("btn_ok"), null, replaceForGamepad: true, GameKey.Select);
			uIItemCountWindowData.BackBtnData = new UIDialogWindowData.ButtonData(null, LLBase.L("btn_cancel"), null, replaceForGamepad: true, GameKey.Back);
			LazyUI.GetWindow<UIItemCountWindow>().Open(uIItemCountWindowData);
		}
		else if (!silent && to == MainGame.PlayerData.Inventory)
		{
			LazySingleton<UINotificator>.Instance.HandleInventoryFull();
		}
	}

	private bool TryMoveItem(UIItemCell itemCell, int count, Inventory from, Inventory to)
	{
		if (itemCell.DisplayingItem == null)
		{
			return false;
		}
		if (silent)
		{
			UINotificator.isSilent = true;
		}
		if (itemCell.DisplayingItem.IsBag)
		{
			if (to.AddItemToInventory(Item.Copy(itemCell.DisplayingItem)))
			{
				from.RemoveItemFromInventoryByUID(itemCell.DisplayingItem, count);
			}
		}
		else if (to.AddItemToInventory(new Item(itemCell.DisplayingItem.id, count), ignoreBagWhenAdd, ignoreAllBags: true))
		{
			from.RemoveItemById(itemCell.DisplayingItem.id, count, ignoreBagWhenAdd, from.TryFindSourceBagForItem(itemCell.DisplayingItem), ignoreAllBags: true);
		}
		if (silent)
		{
			UINotificator.isSilent = false;
		}
		if (MainGame.PlayerData != null && MainGame.PlayerData.HasInteractingItem)
		{
			MainGame.PlayerData.UpdateInteractingItem();
		}
		return true;
	}
}
