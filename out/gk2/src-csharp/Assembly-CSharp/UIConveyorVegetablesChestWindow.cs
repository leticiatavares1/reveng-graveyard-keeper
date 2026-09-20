using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class UIConveyorVegetablesChestWindow : UIBaseChestWindow
{
	private const string GardenBagsStorage3Id = "garden_bags_storage_3";

	[SerializeField]
	private UIConveyorChestSlot slot1;

	[SerializeField]
	private UIConveyorChestSlot slot2;

	[SerializeField]
	private UIConveyorChestSlot slot3;

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
		if (!(data.Chest is ConveyorWgoData { ConveyorComponent: ConveyorChestOutComponent conveyorComponent } conveyorWgoData))
		{
			return;
		}
		conveyorComponent.UpdateSlotsData();
		List<UIConveyorChestSlot> chestSlots = GetChestSlots();
		int num = ((conveyorWgoData.id == "garden_bags_storage_3") ? 3 : 2);
		for (int i = 0; i < chestSlots.Count; i++)
		{
			UIConveyorChestSlot uIConveyorChestSlot = chestSlots[i];
			if (uIConveyorChestSlot == null)
			{
				continue;
			}
			if (i >= num)
			{
				uIConveyorChestSlot.gameObject.SetActive(value: false);
				continue;
			}
			int slotIndex = ((uIConveyorChestSlot.SlotIndex > 0) ? uIConveyorChestSlot.SlotIndex : (i + 1));
			ConveyorChestSlotData conveyorChestSlotData = conveyorComponent.SlotsData.Find((ConveyorChestSlotData x) => x.slotIndex == slotIndex);
			if (conveyorChestSlotData == null)
			{
				uIConveyorChestSlot.gameObject.SetActive(value: false);
				continue;
			}
			uIConveyorChestSlot.gameObject.SetActive(value: true);
			uIConveyorChestSlot.Draw(conveyorChestSlotData, conveyorComponent, SetChestItemSettingMode, RemoveItemFromConveyorSlot);
		}
	}

	private List<UIConveyorChestSlot> GetChestSlots()
	{
		List<UIConveyorChestSlot> list = new List<UIConveyorChestSlot>();
		if (slot1 != null)
		{
			list.Add(slot1);
		}
		if (slot2 != null)
		{
			list.Add(slot2);
		}
		if (slot3 != null)
		{
			list.Add(slot3);
		}
		if (list.Count == 0)
		{
			list.AddRange(GetComponentsInChildren<UIConveyorChestSlot>(includeInactive: true));
			list.Sort(CompareChestSlots);
		}
		return list;
	}

	private static int CompareChestSlots(UIConveyorChestSlot a, UIConveyorChestSlot b)
	{
		if (a.SlotIndex > 0 && b.SlotIndex > 0)
		{
			return a.SlotIndex.CompareTo(b.SlotIndex);
		}
		if (a.SlotIndex > 0)
		{
			return -1;
		}
		if (b.SlotIndex > 0)
		{
			return 1;
		}
		return a.transform.position.x.CompareTo(b.transform.position.x);
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
		if (data.Chest is ConveyorWgoData conveyorWgoData && conveyorWgoData.ConveyorComponent is ConveyorChestOutComponent)
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
