using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class UIConveyorWineChestWindow : UIBaseChestWindow
{
	[SerializeField]
	private UIConveyorChestSlot chestSlot;

	[SerializeField]
	private List<GameObject> whenSelectionEnabledObjects;

	private Action<UIItemCell> previousChestOnPress;

	private Action<UIItemCell> previousChestOnPress2;

	private bool isItemSelectionModActive;

	private UIConveyorChestSlot selectedSlot;

	public bool IsItemSelectionModActive => isItemSelectionModActive;

	public override void Redraw()
	{
		base.Redraw();
		RedrawChestSlot();
	}

	public override void Close()
	{
		base.Close();
		data.SecondMultiInventoryData.SelectedWidgetData.Inventory.OnItemsAdd -= RedrawChestSlot;
		data.SecondMultiInventoryData.SelectedWidgetData.Inventory.OnItemsRemove -= RedrawChestSlot;
	}

	protected override void SetData(UIBaseChestWindowData data)
	{
		base.SetData(data);
		data.SecondMultiInventoryData.SelectedWidgetData.Inventory.OnItemsAdd += RedrawChestSlot;
		data.SecondMultiInventoryData.SelectedWidgetData.Inventory.OnItemsRemove += RedrawChestSlot;
	}

	protected override void UpdateGamepadDependentStuff()
	{
		UpdateGamepadDependentStuffBasic();
		if (LazyInput.IsGamepadActive)
		{
			GamepadNavigationItem focusedItem = base.GamepadNavigationController.FocusedItem;
			if (isItemSelectionModActive)
			{
				base.GamepadNavigationController.ReinitItems(focusOnFirstActive: true);
			}
			else if (focusedItem != null)
			{
				base.GamepadNavigationController.ReinitItems(focusOnFirstActive: false);
				base.GamepadNavigationController.SetFocusedItem(focusedItem);
			}
			else
			{
				base.GamepadNavigationController.ReinitItems(focusOnFirstActive: true);
			}
		}
	}

	protected override void PrintTips(GamepadNavigationItem gamepadNavigationItem)
	{
		List<LazyGameKeyTip> list = new List<LazyGameKeyTip>();
		if (gamepadNavigationItem != null && gamepadNavigationItem.TryGetComponent<UIItemCell>(out var component))
		{
			if (component.GetComponentInParentExcludeCurrent<UIConveyorChestSlot>(includeInactive: true) != null)
			{
				if (!isItemSelectionModActive)
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
			}
			else
			{
				if (component.DisplayingItem != null && !component.DisplayingItem.IsEmpty && component.OnItemCellPress != null)
				{
					list.Add(LazyGameKeyTip.Select());
				}
				if (!isItemSelectionModActive)
				{
					if (component.DisplayingItem != null && !component.DisplayingItem.IsEmpty && component.IsInteractable && component.OnItemCellPress2 != null)
					{
						list.Add(new LazyGameKeyTip(GameKey.ItemMove, "tip_item_action"));
					}
					TryAddMoveAllSimilarItemsTip(list);
				}
			}
		}
		list.Add(LazyGameKeyTip.Back());
		lazyButtonTips.Print(list);
	}

	protected override void InitCloseButton(LazyButton button)
	{
		button.onClick.AddListener(CloseOrDeactivateItemSettingMode);
	}

	protected override bool OnPressedBack()
	{
		CloseOrDeactivateItemSettingMode();
		return true;
	}

	protected override void OnAllToChestPressed()
	{
		if (!isItemSelectionModActive)
		{
			data.OnMoveAllSimilarItemFromPlayerToChest?.Invoke();
		}
	}

	private void RedrawChestSlot(List<Item> items = null)
	{
		ConveyorWgoData conveyorWgoData = data.Chest as ConveyorWgoData;
		UIConveyorChestSlot uIConveyorChestSlot = GetChestSlot();
		if (conveyorWgoData == null || uIConveyorChestSlot == null)
		{
			return;
		}
		if (conveyorWgoData.ConveyorComponent is ConveyorChestComponent conveyorChestComponent)
		{
			ConveyorChestSlotData conveyorChestSlotData = conveyorChestComponent.SlotsData.Find((ConveyorChestSlotData x) => x.slotPosDirection == Direction.Right);
			if (conveyorChestSlotData == null)
			{
				uIConveyorChestSlot.gameObject.SetActive(value: false);
				return;
			}
			uIConveyorChestSlot.gameObject.SetActive(value: true);
			uIConveyorChestSlot.Draw(conveyorChestSlotData, conveyorChestComponent, SetChestItemSettingMode, RemoveItemFromConveyorSlot);
		}
		else if (conveyorWgoData.ConveyorComponent is ConveyorChestOutComponent conveyorChestOutComponent)
		{
			conveyorChestOutComponent.UpdateSlotsData();
			ConveyorChestSlotData conveyorChestSlotData2 = conveyorChestOutComponent.SlotsData.Find((ConveyorChestSlotData x) => x.slotPosDirection == Direction.Right);
			if (conveyorChestSlotData2 == null)
			{
				uIConveyorChestSlot.gameObject.SetActive(value: false);
				return;
			}
			uIConveyorChestSlot.gameObject.SetActive(value: true);
			uIConveyorChestSlot.Draw(conveyorChestSlotData2, conveyorChestOutComponent, SetChestItemSettingMode, RemoveItemFromConveyorSlot);
		}
	}

	private UIConveyorChestSlot GetChestSlot()
	{
		if (chestSlot == null)
		{
			chestSlot = GetComponentInChildren<UIConveyorChestSlot>(includeInactive: true);
		}
		return chestSlot;
	}

	private void SetItemSlot(UIItemCell cell, ConveyorChestSlotData slotData)
	{
		if (data.Chest is ConveyorWgoData { ConveyorComponent: var conveyorComponent } && (conveyorComponent is ConveyorChestComponent || conveyorComponent is ConveyorChestOutComponent))
		{
			slotData.SetItemSlotId(cell.DisplayingItem.id);
			SetChestItemSettingMode(isActive: false);
			RedrawChestSlot();
		}
	}

	private void RemoveItemFromConveyorSlot(UIConveyorChestSlot conveyorChestSlot)
	{
		if (data.Chest is ConveyorWgoData { ConveyorComponent: var conveyorComponent } && (conveyorComponent is ConveyorChestComponent || conveyorComponent is ConveyorChestOutComponent))
		{
			conveyorChestSlot.SlotData.SetItemSlotId(string.Empty);
			RedrawChestSlot();
		}
	}

	private void SetChestItemSettingMode(UIConveyorChestSlot conveyorChestSlot)
	{
		if (!isItemSelectionModActive)
		{
			selectedSlot = conveyorChestSlot;
			SetChestItemSettingMode(isActive: true, conveyorChestSlot.SlotData);
			conveyorChestSlot.SetState(UIConveyorChestSlot.State.OutDuringSelection);
		}
	}

	private void SetChestItemSettingMode(bool isActive, ConveyorChestSlotData slotData = null)
	{
		if (isActive && isItemSelectionModActive)
		{
			return;
		}
		isItemSelectionModActive = isActive;
		foreach (GameObject whenSelectionEnabledObject in whenSelectionEnabledObjects)
		{
			whenSelectionEnabledObject.SetActive(isActive);
		}
		if (isActive)
		{
			previousChestOnPress = data.SecondMultiInventoryData.inventoriesData[0].OnItemCellPress;
			previousChestOnPress2 = data.SecondMultiInventoryData.inventoriesData[0].OnItemCellPress2;
			foreach (InventoryWidgetDataBase inventoriesDatum in data.SecondMultiInventoryData.inventoriesData)
			{
				inventoriesDatum.SetCustomOnCellPressedAction(delegate(UIItemCell x)
				{
					SetItemSlot(x, slotData);
				});
				inventoriesDatum.SetCustomOnCellPressed2Action(delegate
				{
				});
			}
			leftMultiInventoryWidget.Redraw();
			rightMultiInventoryWidget.Redraw();
			foreach (InventoryWidget drawnInventory in leftMultiInventoryWidget.DrawnInventories)
			{
				foreach (UIItemCell cell in drawnInventory.Cells)
				{
					cell.GamepadNavigationItem.Active = false;
				}
			}
		}
		else
		{
			foreach (InventoryWidgetDataBase inventoriesDatum2 in data.SecondMultiInventoryData.inventoriesData)
			{
				inventoriesDatum2.SetCustomOnCellPressedAction(previousChestOnPress);
				inventoriesDatum2.SetCustomOnCellPressed2Action(previousChestOnPress2);
			}
			leftMultiInventoryWidget.Redraw();
			rightMultiInventoryWidget.Redraw();
			foreach (InventoryWidget drawnInventory2 in leftMultiInventoryWidget.DrawnInventories)
			{
				foreach (UIItemCell cell2 in drawnInventory2.Cells)
				{
					cell2.GamepadNavigationItem.Active = true;
				}
			}
		}
		RedrawChestSlot();
		UpdateGamepadDependentStuff();
	}

	private void CloseOrDeactivateItemSettingMode()
	{
		if (isItemSelectionModActive)
		{
			if (selectedSlot != null)
			{
				selectedSlot.SetState(selectedSlot.PreviousState);
			}
			SetChestItemSettingMode(isActive: false);
		}
		else
		{
			Close();
		}
	}
}
