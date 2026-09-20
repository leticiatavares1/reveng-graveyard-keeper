using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class UIHotBarWidget : LazyWidget<UIHotBarWidgetData>
{
	[SerializeField]
	private List<UIHotBarItemCell> equippedItemsKeyboard = new List<UIHotBarItemCell>();

	[SerializeField]
	private List<UIHotBarItemCell> equippedItemsGamepad = new List<UIHotBarItemCell>();

	[SerializeField]
	private GameObject keyboardItemsParent;

	[SerializeField]
	private GameObject gamepadItemsParent;

	public List<UIHotBarItemCell> HotBarItems
	{
		get
		{
			if (!LazyInput.IsGamepadActive)
			{
				return equippedItemsKeyboard;
			}
			return equippedItemsGamepad;
		}
	}

	public override void Init()
	{
		base.Init();
		MainGame.OnGameStarted = (Action)Delegate.Combine(MainGame.OnGameStarted, new Action(OnGameStarted));
	}

	protected override void SetData(UIHotBarWidgetData data)
	{
		base.SetData(data);
		data.PlayerData.OnPinnedItemsChanged += Redraw;
		data.PlayerData.Inventory.OnItemsAdd += RedrawHotbarItems;
		data.PlayerData.Inventory.OnItemsRemove += RedrawHotbarItems;
		LazyInput.OnInputChanged += Redraw;
		SubscribeToFightState();
	}

	public override void Redraw()
	{
		if (data != null)
		{
			base.Redraw();
			RedrawHotbarItems();
		}
	}

	public void SetDataOutside(UIHotBarWidgetData outsideData)
	{
		SetData(outsideData);
	}

	private void OnGameStarted()
	{
		Redraw();
	}

	public void RedrawHotbarItems(List<Item> items = null)
	{
		keyboardItemsParent.SetActive(!LazyInput.IsGamepadActive);
		gamepadItemsParent.SetActive(LazyInput.IsGamepadActive);
		bool flag = data.PinnableItem != null;
		for (int i = 0; i < HotBarItems.Count; i++)
		{
			HotBarItems[i].UIItemCell.ClearCallbacks();
			if (string.IsNullOrEmpty(data.PlayerData.pinnedItems[i]))
			{
				if (!flag)
				{
					HotBarItems[i].UIItemCell.SetWidgetState(ItemRelatedWidgetState.Default);
					HotBarItems[i].UIItemCell.DrawEmpty();
				}
				else
				{
					HotBarItems[i].UIItemCell.DrawEmptyInteractable();
				}
			}
			else
			{
				int totalCountInInventory = data.PlayerData.inventory.Data.GetTotalCountInInventory(data.PlayerData.pinnedItems[i]);
				Item item = new Item(data.PlayerData.pinnedItems[i], totalCountInInventory);
				if (!flag && (totalCountInInventory == 0 || !CanBeUsedNow(item)))
				{
					HotBarItems[i].DrawNonInteractable(item);
				}
				else
				{
					HotBarItems[i].Draw(item, data.IsUsable, TryUseHotBarItem);
				}
			}
			if (flag)
			{
				int index = i;
				UIItemCell uIItemCell = HotBarItems[i].UIItemCell;
				uIItemCell.OnItemCellPress = (Action<UIItemCell>)Delegate.Combine(uIItemCell.OnItemCellPress, (Action<UIItemCell>)delegate
				{
					TryPinItemToHotBar(index);
				});
			}
			HotBarItems[i].Init(i);
		}
		UpdateButtonsText();
	}

	public void UpdateButtonsText()
	{
		foreach (UIHotBarItemCell item in equippedItemsKeyboard)
		{
			item.UpdateBtnText();
		}
	}

	public override void Hide()
	{
		base.Hide();
		foreach (UIHotBarItemCell item in equippedItemsKeyboard)
		{
			item.UIItemCell.ClearCallbacks();
		}
		foreach (UIHotBarItemCell item2 in equippedItemsGamepad)
		{
			item2.UIItemCell.ClearCallbacks();
		}
		if (data != null)
		{
			data.PlayerData.OnPinnedItemsChanged -= Redraw;
			data.PlayerData.Inventory.OnItemsAdd -= RedrawHotbarItems;
			data.PlayerData.Inventory.OnItemsRemove -= RedrawHotbarItems;
		}
		UnsubscribeFromFightState();
	}

	private void TryUseHotBarItem(UIItemCell uiItemCell)
	{
		MainGame.PlayerData.TryUseHotBarItem(uiItemCell.DisplayingItem.id);
	}

	private bool CanBeUsedNow(Item item)
	{
		if (item.IsFertilizer || item.IsSeed)
		{
			return true;
		}
		return item.Definition.CanBeUsed;
	}

	private void SubscribeToFightState()
	{
		if (!(LazySingleton<FightingGameController>.Instance == null))
		{
			LazySingleton<FightingGameController>.Instance.OnFightStateChanged -= HandleFightStateChanged;
			LazySingleton<FightingGameController>.Instance.OnFightStateChanged += HandleFightStateChanged;
		}
	}

	private void UnsubscribeFromFightState()
	{
		if (!(LazySingleton<FightingGameController>.Instance == null))
		{
			LazySingleton<FightingGameController>.Instance.OnFightStateChanged -= HandleFightStateChanged;
		}
	}

	private void HandleFightStateChanged(FightState fightState)
	{
		RedrawHotbarItems();
	}

	private void TryPinItemToHotBar(int index)
	{
		HotBarItems[index].Set(data.PinnableItem);
		LazyUI.GetWindow<UIHotBarSelectionWindow>().Close();
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		Draw(new UIHotBarWidgetData(MainGame.Instance.GameSave, isUsable: true));
	}
}
