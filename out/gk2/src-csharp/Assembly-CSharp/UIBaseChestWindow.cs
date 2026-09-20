using System;
using System.Collections.Generic;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class UIBaseChestWindow : LazyWindow<UIBaseChestWindowData>
{
	[SerializeField]
	protected MultiInventoryWidget leftMultiInventoryWidget;

	[SerializeField]
	protected MultiInventoryWidget rightMultiInventoryWidget;

	[SerializeField]
	private TextMeshProUGUI headerLeft;

	[SerializeField]
	private TextMeshProUGUI headerRight;

	[SerializeField]
	private UIWorkerIcon playerIcon;

	[SerializeField]
	private MoneyWidget moneyWidget;

	public override void Open(UIBaseChestWindowData data)
	{
		base.Open(data);
		playerIcon.ShowWithoutTalent(MainGame.PlayerController, MainGame.PlayerController.View.PlayerAnimation.SkinPreset);
		headerLeft.text = LLBase.L("ui_player");
		headerRight.text = LLBase.L("ui_storage");
	}

	public override void Redraw()
	{
		leftMultiInventoryWidget.Draw(data.FirstMultiInventoryData);
		rightMultiInventoryWidget.Draw(data.SecondMultiInventoryData);
		moneyWidget.Draw(data.MoneyWidgetData);
		leftMultiInventoryWidget.OnAnyInventoryRedraw = UpdateGamepadDependentStuff;
		rightMultiInventoryWidget.OnAnyInventoryRedraw = UpdateGamepadDependentStuff;
		leftMultiInventoryWidget.OnMoveAllSimilarBtnInteractableChanged = RefreshMoveAllSimilarGamepadTips;
		leftMultiInventoryWidget.SelectFirstWidget();
		rightMultiInventoryWidget.SelectFirstWidget();
		((RectTransform)base.transform).RefreshContentFitter();
		if (LazyInput.IsGamepadActive)
		{
			base.GamepadNavigationController.ReinitItems(focusOnFirstActive: true);
			UpdateGamepadDependentStuff();
		}
	}

	public override void Hide()
	{
		if (leftMultiInventoryWidget != null)
		{
			leftMultiInventoryWidget.OnMoveAllSimilarBtnInteractableChanged = null;
		}
		base.Hide();
		leftMultiInventoryWidget.Hide();
		rightMultiInventoryWidget.Hide();
	}

	private bool OnItemPressed2()
	{
		if (LazyInput.IsGamepadActive)
		{
			GamepadNavigationItem focusedItem = base.GamepadNavigationController.FocusedItem;
			if (focusedItem != null && focusedItem.TryGetComponent<UIItemCell>(out var component) && component.DisplayingItem != null && !component.DisplayingItem.IsEmpty)
			{
				component.OnGamepadPress2();
				return true;
			}
		}
		return false;
	}

	protected virtual void OnAllToChestPressed()
	{
		data.OnMoveAllSimilarItemFromPlayerToChest?.Invoke();
	}

	protected override Dictionary<GameKey, Func<bool>> GetGameKeyDelegates()
	{
		Dictionary<GameKey, Func<bool>> gameKeyDelegates = base.GetGameKeyDelegates();
		gameKeyDelegates.Add(GameKey.ItemMove, OnItemPressed2);
		gameKeyDelegates.Add(GameKey.MoveAllItemsFromPlayer, delegate
		{
			OnAllToChestPressed();
			return true;
		});
		return gameKeyDelegates;
	}

	protected void UpdateGamepadDependentStuffBasic()
	{
		base.UpdateGamepadDependentStuff();
	}

	protected override void UpdateGamepadDependentStuff()
	{
		base.UpdateGamepadDependentStuff();
		if (!LazyInput.IsGamepadActive)
		{
			return;
		}
		GamepadNavigationItem focusedItem = base.GamepadNavigationController.FocusedItem;
		base.GamepadNavigationController.ReinitItems(focusOnFirstActive: false);
		base.GamepadNavigationController.navigationGroupSources.Clear();
		int count = leftMultiInventoryWidget.DrawnInventories.Count;
		int count2 = rightMultiInventoryWidget.DrawnInventories.Count;
		List<GamepadNavigationController.NavigationGroupTarget> list = new List<GamepadNavigationController.NavigationGroupTarget>();
		List<GamepadNavigationController.NavigationGroupTarget> list2 = new List<GamepadNavigationController.NavigationGroupTarget>();
		for (int i = 0; i < count2; i++)
		{
			list.Add(new GamepadNavigationController.NavigationGroupTarget(i + 1000, GUIDirection.Right));
		}
		for (int j = 0; j < count; j++)
		{
			list2.Add(new GamepadNavigationController.NavigationGroupTarget(j, GUIDirection.Left));
		}
		for (int k = 0; k < count; k++)
		{
			InventoryWidget inventoryWidget = leftMultiInventoryWidget.DrawnInventories[k];
			List<GamepadNavigationController.NavigationGroupTarget> list3 = new List<GamepadNavigationController.NavigationGroupTarget>();
			list3.AddRange(list);
			list3.Add(new GamepadNavigationController.NavigationGroupTarget(k - 1, GUIDirection.Up));
			list3.Add(new GamepadNavigationController.NavigationGroupTarget(k + 1, GUIDirection.Down));
			base.GamepadNavigationController.navigationGroupSources.Add(new GamepadNavigationController.NavigationGroupSource(k, list3));
			for (int l = 0; l < inventoryWidget.Cells.Count; l++)
			{
				inventoryWidget.Cells[l].GamepadNavigationItem.group = k;
			}
		}
		for (int m = 1000; m < count2 + 1000; m++)
		{
			InventoryWidget inventoryWidget2 = rightMultiInventoryWidget.DrawnInventories[m - 1000];
			List<GamepadNavigationController.NavigationGroupTarget> list4 = new List<GamepadNavigationController.NavigationGroupTarget>();
			list4.AddRange(list2);
			list4.Add(new GamepadNavigationController.NavigationGroupTarget(m - 1, GUIDirection.Up));
			list4.Add(new GamepadNavigationController.NavigationGroupTarget(m + 1, GUIDirection.Down));
			base.GamepadNavigationController.navigationGroupSources.Add(new GamepadNavigationController.NavigationGroupSource(m, list4));
			for (int n = 0; n < inventoryWidget2.Cells.Count; n++)
			{
				inventoryWidget2.Cells[n].GamepadNavigationItem.group = m;
			}
		}
		if (focusedItem != null)
		{
			base.GamepadNavigationController.SetFocusedItem(focusedItem);
		}
	}

	protected override void PrintTips()
	{
		PrintTips(base.GamepadNavigationController.FocusedItem);
	}

	protected override void PrintTips(GamepadNavigationItem gamepadNavigationItem)
	{
		List<LazyGameKeyTip> list = new List<LazyGameKeyTip>();
		if (gamepadNavigationItem != null && gamepadNavigationItem.TryGetComponent<UIItemCell>(out var component))
		{
			if (component.DisplayingItem != null && !component.DisplayingItem.IsEmpty && component.OnItemCellPress != null)
			{
				list.Add(LazyGameKeyTip.Select());
			}
			if (component.DisplayingItem != null && !component.DisplayingItem.IsEmpty && component.IsInteractable && component.OnItemCellPress2 != null)
			{
				list.Add(new LazyGameKeyTip(GameKey.ItemMove, "tip_item_action"));
			}
		}
		if ((bool)closeButton)
		{
			list.Add(LazyGameKeyTip.Back());
		}
		TryAddMoveAllSimilarItemsTip(list);
		lazyButtonTips.Print(list);
	}

	protected void TryAddMoveAllSimilarItemsTip(List<LazyGameKeyTip> tips)
	{
		if (leftMultiInventoryWidget.IsMoveAllSimilarBtnInteractable)
		{
			tips.Add(new LazyGameKeyTip(GameKey.MoveAllItemsFromPlayer, "tip_move_similar_items"));
		}
	}

	private void RefreshMoveAllSimilarGamepadTips()
	{
		if (LazyInput.IsGamepadActive)
		{
			PrintTips(base.GamepadNavigationController.FocusedItem);
		}
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		WgoData wgoData = new WgoData("chest_home", MainGame.PlayerController.MovablePosition, MainGame.PlayerData.currentGameSceneId);
		wgoData.Inventory.AddItemToInventory(new Item("faith", 5));
		wgoData.Inventory.AddItemToInventory(new Item("stick", 15));
		wgoData.Inventory.AddItemToInventory(new Item("bag_universal"));
		LazyUI.GetWindow<UIChestWindow>().Open(new UIBaseChestWindowData(MainGame.PlayerData.inventory, null, wgoData));
	}
}
