using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class UIConveyorChestWindow : UIBaseChestWindow
{
	[SerializeField]
	private List<UIConveyorChestSlot> chestSlots = new List<UIConveyorChestSlot>();

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
		RedrawChestSlots();
	}

	public override void Close()
	{
		base.Close();
		data.SecondMultiInventoryData.SelectedWidgetData.Inventory.OnItemsAdd -= RedrawChestSlots;
		data.SecondMultiInventoryData.SelectedWidgetData.Inventory.OnItemsRemove -= RedrawChestSlots;
	}

	protected override void SetData(UIBaseChestWindowData data)
	{
		base.SetData(data);
		data.SecondMultiInventoryData.SelectedWidgetData.Inventory.OnItemsAdd += RedrawChestSlots;
		data.SecondMultiInventoryData.SelectedWidgetData.Inventory.OnItemsRemove += RedrawChestSlots;
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

	private void RedrawChestSlots(List<Item> items = null)
	{
		if (!(data.Chest is ConveyorWgoData conveyorWgoData))
		{
			return;
		}
		if (conveyorWgoData.ConveyorComponent is ConveyorChestComponent conveyorChestComponent)
		{
			{
				foreach (UIConveyorChestSlot chestSlot in chestSlots)
				{
					ConveyorChestSlotData conveyorChestSlotData = conveyorChestComponent.SlotsData.Find((ConveyorChestSlotData x) => x.slotPosDirection == chestSlot.ChestPosDirection);
					if (conveyorChestSlotData != null)
					{
						chestSlot.gameObject.SetActive(value: true);
						chestSlot.Draw(conveyorChestSlotData, conveyorChestComponent, SetChestItemSettingMode, RemoveItemFromConveyorSlot);
					}
				}
				return;
			}
		}
		if (!(conveyorWgoData.ConveyorComponent is ConveyorChestOutComponent conveyorChestOutComponent))
		{
			return;
		}
		int count = conveyorChestOutComponent.SlotsData.Count;
		for (int i = 0; i < chestSlots.Count; i++)
		{
			if (i >= count)
			{
				chestSlots[i].gameObject.SetActive(value: false);
				continue;
			}
			chestSlots[i].gameObject.SetActive(value: true);
			chestSlots[i].Draw(conveyorChestOutComponent.SlotsData[i], conveyorChestOutComponent, SetChestItemSettingMode, RemoveItemFromConveyorSlot);
		}
	}

	private void SetItemSlot(UIItemCell cell, ConveyorChestSlotData slotData)
	{
		if (data.Chest is ConveyorWgoData { ConveyorComponent: var conveyorComponent } && (conveyorComponent is ConveyorChestComponent || conveyorComponent is ConveyorChestOutComponent))
		{
			slotData.SetItemSlotId(cell.DisplayingItem.id);
			SetChestItemSettingMode(isActive: false);
			RedrawChestSlots();
		}
	}

	private void RemoveItemFromConveyorSlot(UIConveyorChestSlot conveyorChestSlot)
	{
		if (data.Chest is ConveyorWgoData conveyorWgoData && conveyorWgoData.ConveyorComponent is ConveyorChestComponent)
		{
			conveyorChestSlot.SlotData.SetItemSlotId(string.Empty);
			RedrawChestSlots();
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
		RedrawChestSlots();
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
