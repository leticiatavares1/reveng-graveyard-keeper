using System.Collections.Generic;
using UnityEngine;

public class ChestGUI : BaseGUI
{
	public InventoryPanelGUI player_panel;

	public InventoryPanelGUI chest_panel;

	private MultiInventory _player_inventory;

	private MultiInventory _chest_inventory;

	private InventoryPanelGUI _last_selected_panel;

	private WorldGameObject _chest_obj;

	public override void Init()
	{
		player_panel.Init();
		player_panel.SetCallbacks(OnItemOver, null, OnItemSelect, OnCustomItemOver);
		chest_panel.Init();
		chest_panel.SetCallbacks(OnItemOver, null, OnItemSelect, OnCustomItemOver);
		base.Init();
	}

	public void Open(WorldGameObject chest_obj)
	{
		if (chest_obj == null)
		{
			Debug.LogError("Cannot open chest gui for null obj");
			return;
		}
		base.Open();
		_chest_obj = chest_obj;
		_player_inventory = MainGame.me.player.GetMultiInventory(new List<WorldGameObject> { _chest_obj }, "", MultiInventory.PlayerMultiInventory.DontChange, include_toolbelt: false, sortWGOS: true, include_bags: true);
		_chest_inventory = _chest_obj.GetMultiInventoryOfWGOWithoutWorldZone(duplicate_bags: true);
		player_panel.Open(_player_inventory, 1);
		chest_panel.Open(_chest_inventory, 2);
		player_panel.SetGrayToNotMainWidgets(do_not_gray_bags: true);
		if (BaseGUI.for_gamepad)
		{
			base.gamepad_controller.ReinitItems(focus_on_first_active: false);
			if (_last_selected_panel == null || _last_selected_panel.selected_item == null)
			{
				base.gamepad_controller.FocusOnFirstActive();
			}
			else
			{
				BaseItemCellGUI itemCellGuiForItem = _last_selected_panel.GetItemCellGuiForItem(_last_selected_panel.selected_item);
				base.gamepad_controller.SetFocusedItem((itemCellGuiForItem == null) ? null : itemCellGuiForItem.gamepad_item);
			}
		}
		MainGame.SetPausedMode(is_paused: true);
	}

	private void FullRedrawPanels(int possible_index_for_gamepad = -1)
	{
		_player_inventory = MainGame.me.player.GetMultiInventory(new List<WorldGameObject> { _chest_obj }, "", MultiInventory.PlayerMultiInventory.DontChange, include_toolbelt: false, sortWGOS: true, include_bags: true);
		_chest_inventory = _chest_obj.GetMultiInventoryOfWGOWithoutWorldZone(duplicate_bags: true);
		player_panel.FullRedraw(_player_inventory, 1);
		chest_panel.FullRedraw(_chest_inventory, 2);
		if (BaseGUI.for_gamepad)
		{
			base.gamepad_controller.ReinitItems(focus_on_first_active: false);
			if (possible_index_for_gamepad != -1)
			{
				base.gamepad_controller.SetFocusedItem(possible_index_for_gamepad);
				return;
			}
			if (_last_selected_panel == null || _last_selected_panel.selected_item == null)
			{
				base.gamepad_controller.FocusOnFirstActive();
				return;
			}
			BaseItemCellGUI itemCellGuiForItem = _last_selected_panel.GetItemCellGuiForItem(_last_selected_panel.selected_item);
			base.gamepad_controller.SetFocusedItem((itemCellGuiForItem == null) ? null : itemCellGuiForItem.gamepad_item);
		}
	}

	public void OnCustomItemOver()
	{
		if (BaseGUI.for_gamepad)
		{
			base.button_tips.PrintClose();
			_last_selected_panel = null;
		}
	}

	public void OnItemOver()
	{
		if (!BaseGUI.for_gamepad)
		{
			return;
		}
		InventoryPanelGUI inventoryPanelGUI = (_last_selected_panel = InventoryPanelGUI.last);
		Item inventory_data = inventoryPanelGUI.selected_widget.inventory_data;
		if (inventoryPanelGUI == null || (inventoryPanelGUI.selected_widget_is_not_main && (inventory_data == null || !inventory_data.is_bag)))
		{
			base.button_tips.PrintClose();
			return;
		}
		Item from_bag = null;
		if (inventory_data != null && inventory_data.is_bag)
		{
			from_bag = inventory_data;
		}
		bool flag = inventoryPanelGUI == player_panel;
		bool selected_item_is_empty = inventoryPanelGUI.selected_item_is_empty;
		if (((!selected_item_is_empty) ? GetMaxMoveCount(inventoryPanelGUI.selected_item, inventoryPanelGUI == player_panel, from_bag) : 0) <= 1)
		{
			base.button_tips.Print(GameKeyTip.Select(flag ? "put" : "take", !selected_item_is_empty), GameKeyTip.Close());
		}
		else
		{
			base.button_tips.Print(GameKeyTip.Select(flag ? "put" : "take", !selected_item_is_empty), GameKeyTip.Option1(GJL.L(flag ? "put" : "take") + " 1", !selected_item_is_empty), GameKeyTip.Close());
		}
	}

	public void OnItemSelect()
	{
		InventoryPanelGUI last = InventoryPanelGUI.last;
		Item inventory_data = last.selected_widget.inventory_data;
		if ((last.selected_widget_is_not_main && (inventory_data == null || !inventory_data.is_bag)) || last.selected_item_is_empty)
		{
			return;
		}
		BaseItemCellGUI item_gui = last.selected_item_gui;
		Item item = last.selected_item;
		bool to_chest = last == player_panel;
		Item from_bag = null;
		if (inventory_data != null && inventory_data.is_bag)
		{
			from_bag = inventory_data;
		}
		int maxMoveCount = GetMaxMoveCount(last.selected_item, last == player_panel, from_bag);
		if (item.definition.stack_count > 1 && LazyInput.GetKeyDown(GameKey.MoveAllStack))
		{
			MoveItem(item, maxMoveCount, to_chest, from_bag);
			return;
		}
		if (item.definition.stack_count == 1 || LazyInput.GetKeyDown(GameKey.RightClick) || maxMoveCount == 1)
		{
			MoveItem(item, 1, to_chest, from_bag);
			return;
		}
		base.button_tips.Deactivate();
		GUIElements.me.item_count.Open(item.id, 1, maxMoveCount, delegate(int chosen_count)
		{
			MoveItem(item, chosen_count, to_chest, from_bag, after_item_count_gui: true);
		});
		GUIElements.me.item_count.SetOnHide(delegate
		{
			if (BaseGUI.for_gamepad)
			{
				base.button_tips.Activate();
				base.gamepad_controller.Enable(GamepadNavigationController.OpenMethod.GetAll);
				base.gamepad_controller.SetFocusedItem(item_gui.gamepad_item);
			}
		});
	}

	private int GetMaxMoveCount(Item item, bool to_chest, Item from_bag = null, bool count_in_bags = false)
	{
		MultiInventory multiInventory = (to_chest ? _player_inventory : _chest_inventory);
		MultiInventory multiInventory2 = (to_chest ? _chest_inventory : _player_inventory);
		if (from_bag != null)
		{
			multiInventory = new MultiInventory(new Inventory(from_bag));
		}
		int totalCount = multiInventory.GetTotalCount(item.id, MultiInventory.DestinationType.OnlyFirst, count_in_bags);
		int b = multiInventory2.CanAddCount(item.id, count_bags: true);
		return Mathf.Min(totalCount, b);
	}

	private void MoveItem(int count)
	{
		InventoryPanelGUI last = InventoryPanelGUI.last;
		Item inventory_data = last.selected_widget.inventory_data;
		if (!(last == null) && (!last.selected_widget_is_not_main || (inventory_data != null && inventory_data.is_bag)) && !last.selected_item_is_empty)
		{
			Item from_bag = null;
			bool to_chest = last == player_panel;
			if (inventory_data != null && inventory_data.is_bag)
			{
				from_bag = inventory_data;
			}
			MoveItem(last.selected_item, count, to_chest, from_bag);
		}
	}

	private void MoveItem(Item item, int count, bool to_chest, Item from_bag = null, bool after_item_count_gui = false)
	{
		if (item == null || item.IsEmpty())
		{
			return;
		}
		Sounds.PlaySound("item_put");
		MultiInventory multiInventory = (to_chest ? _player_inventory : _chest_inventory);
		MultiInventory another_inventory = (to_chest ? _chest_inventory : _player_inventory);
		InventoryPanelGUI inventoryPanelGUI = (to_chest ? player_panel : chest_panel);
		int value = inventoryPanelGUI.selected_item.value;
		if (from_bag != null)
		{
			int itemsCount = from_bag.GetItemsCount(item.id);
			if (itemsCount > 0)
			{
				MultiInventory multiInventory2 = new MultiInventory(new Inventory(from_bag));
				int num = ((itemsCount > count) ? count : itemsCount);
				if (multiInventory2.MoveItemTo(another_inventory, item, num, use_only_first_from_inventory: true))
				{
					count -= num;
				}
			}
		}
		int num2 = -1;
		if (item.is_bag && BaseGUI.for_gamepad)
		{
			BaseItemCellGUI itemCellGuiForItem = _last_selected_panel.GetItemCellGuiForItem(_last_selected_panel.selected_item);
			if (itemCellGuiForItem != null)
			{
				num2 = base.gamepad_controller.GetFocusedItemIndex(itemCellGuiForItem.gamepad_item);
			}
			if (num2 != -1 && !to_chest)
			{
				num2 += item.inventory_size;
			}
		}
		if (count > 0 && !multiInventory.MoveItemTo(another_inventory, item, count, use_only_first_from_inventory: true))
		{
			return;
		}
		if (item.is_bag)
		{
			FullRedrawPanels(num2);
			return;
		}
		player_panel.Redraw();
		chest_panel.Redraw();
		if (count >= value)
		{
			TooltipsManager.Redraw();
		}
		if (!to_chest)
		{
			MainGame.me.player.TryEquipPickupedDrop(item, check_last_item: false);
		}
		inventoryPanelGUI.UpdateSelection(after_item_count_gui);
	}

	public override void Hide(bool play_hide_sound = true)
	{
		player_panel.Hide();
		chest_panel.Hide();
		base.Hide(play_hide_sound);
		MainGame.SetPausedMode(is_paused: false);
	}

	public override void Update()
	{
		if (base.is_shown_and_top && Input.GetKeyDown(KeyCode.Tab))
		{
			OnPressedBack();
		}
		base.Update();
	}

	protected override bool OnPressedOption1()
	{
		MoveItem(1);
		return true;
	}

	protected override bool OnPressedBack()
	{
		OnClosePressed();
		return true;
	}
}
