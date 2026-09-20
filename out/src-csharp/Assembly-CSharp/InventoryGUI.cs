using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryGUI : BaseGameGUI
{
	private enum EquipmentState
	{
		None,
		StartedFromInventory,
		StartedFromToolbar
	}

	private enum InventoryGUIState
	{
		Standart,
		BagIsOpen
	}

	[Serializable]
	public struct ToolbeltItemDescriptionGUI
	{
		public BaseItemCellGUI item;

		public ItemDefinition.EquipmentType type;
	}

	public UILabel character_info_label;

	public Transform head_pos_tf;

	private InventoryPanelGUI _inventory_panel;

	private MultiInventory _inventory;

	private ToolbarGUI _toolbar;

	private EquipmentState _equipment_state;

	private Item _equipping_item;

	private BaseItemCellGUI _context_menu_target_item;

	public PerkBuffItemGUI perk_buff_item_prefab;

	public GameObject perk_buff_separator_prefab;

	public GameObject go_no_buffs;

	public GameObject go_no_perks;

	public GameObject go_hdr_buffs;

	public GameObject go_hdr_perks;

	public UITable table_perks_buffs;

	public InventoryWidget toolbelt_widget;

	public InventoryWidget bag_inventory_widget;

	private const int SIBLING_INDEX_BUFFS = 0;

	private const int SIBLING_INDEX_PERKS = 500;

	private InventoryGUIState _inventory_state;

	private Item _current_open_bag;

	private InventoryPanelGUI _last_selected_panel;

	public InventoryPanelGUI bag_panel;

	private MultiInventory _bag_inventory;

	public List<ToolbeltItemDescriptionGUI> toolbelt_items = new List<ToolbeltItemDescriptionGUI>();

	public Item selected_item
	{
		get
		{
			if (_inventory_state == InventoryGUIState.BagIsOpen)
			{
				InventoryPanelGUI last = InventoryPanelGUI.last;
				if (last != null)
				{
					return last.selected_item;
				}
			}
			return _inventory_panel.selected_item;
		}
	}

	public Vector3 head_pos => MainGame.me.world_cam.ScreenToWorldPoint(MainGame.me.gui_cam.WorldToScreenPoint(head_pos_tf.position));

	public override void Init()
	{
		_inventory_panel = GetComponentInChildren<InventoryPanelGUI>();
		_inventory_panel.Init();
		_inventory_panel.SetCallbacks(OnItemOver, OnItemOut, OnItemPressed, OnItemOver);
		bag_panel.Init();
		bag_panel.SetCallbacks(OnItemOverInBag, OnItemOutInBag, OnItemPressedInBag);
		bag_panel.Hide();
		_toolbar = GetComponentInChildren<ToolbarGUI>(includeInactive: true);
		_toolbar.Init();
		toolbelt_widget.Init();
		toolbelt_widget.SetCallbacks(OnToolbeltItemOver, OnToolbeltItemOut, OnItemGUIPressed);
		perk_buff_item_prefab.SetActive(active: false);
		perk_buff_separator_prefab.SetActive(value: false);
		go_no_buffs.SetActive(value: false);
		go_no_perks.SetActive(value: false);
		go_hdr_buffs.transform.SetSiblingIndex(0);
		go_no_buffs.transform.SetSiblingIndex(1);
		go_hdr_perks.transform.SetSiblingIndex(500);
		go_no_perks.transform.SetSiblingIndex(501);
		int num = 0;
		BaseItemCellGUI[] cells = _toolbar.keyboard.cells;
		foreach (BaseItemCellGUI obj in cells)
		{
			int index = num++;
			obj.interaction_enabled = true;
			obj.SetCallbacks((GJCommons.VoidDelegate)null, (GJCommons.VoidDelegate)null, (GJCommons.VoidDelegate)delegate
			{
				OnToolbarClicked(index);
			});
		}
		base.Init();
	}

	public override void OpenFromGameGUI()
	{
		Open();
	}

	public override void Open()
	{
		base.Open();
		_inventory_state = InventoryGUIState.Standart;
		_current_open_bag = null;
		_inventory = MainGame.me.player.GetMultiInventory(null, "", MultiInventory.PlayerMultiInventory.DontChange, include_toolbelt: false, sortWGOS: true, include_bags: true);
		_inventory_panel.Open(_inventory, 0, 0, clear_name: false, -1, is_debt_show: true);
		IniGamepadAndTooltips();
		RedrawPlayerInfoAndToolbelt();
		_toolbar.SetActive(!BaseGUI.for_gamepad);
		RedrawBuffsAndPerks();
		if (BaseGUI.for_gamepad)
		{
			return;
		}
		_toolbar.Redraw();
		int num = 0;
		BaseItemCellGUI[] keyboard_cells = GUIElements.me.equip_to_toolbar.keyboard_cells;
		foreach (BaseItemCellGUI obj in keyboard_cells)
		{
			int index = num++;
			obj.interaction_enabled = true;
			obj.SetCallbacks((GJCommons.VoidDelegate)null, (GJCommons.VoidDelegate)null, (GJCommons.VoidDelegate)delegate
			{
				OnToolbarClicked(index);
			});
		}
	}

	public void RedrawBuffsAndPerks()
	{
		PerkBuffItemGUI[] componentsInChildren = GetComponentsInChildren<PerkBuffItemGUI>();
		foreach (PerkBuffItemGUI obj in componentsInChildren)
		{
			obj.transform.SetParent(null, worldPositionStays: true);
			UnityEngine.Object.Destroy(obj.gameObject);
		}
		SeparatorGUI[] componentsInChildren2 = GetComponentsInChildren<SeparatorGUI>();
		foreach (SeparatorGUI obj2 in componentsInChildren2)
		{
			obj2.transform.SetParent(null, worldPositionStays: true);
			UnityEngine.Object.Destroy(obj2.gameObject);
		}
		int num = 0;
		int num2 = 1;
		foreach (PlayerBuff buff in MainGame.me.save.buffs)
		{
			if (!buff.definition.is_hidden)
			{
				if (num > 0)
				{
					perk_buff_separator_prefab.Copy().transform.SetSiblingIndex(num2++);
				}
				PerkBuffItemGUI perkBuffItemGUI = perk_buff_item_prefab.Copy();
				perkBuffItemGUI.transform.SetSiblingIndex(num2++);
				perkBuffItemGUI.Draw(buff);
				num++;
			}
		}
		go_no_buffs.SetActive(num == 0);
		num = 0;
		num2 = 501;
		foreach (string unlocked_perk in MainGame.me.save.unlocked_perks)
		{
			PerkDefinition data = GameBalance.me.GetData<PerkDefinition>(unlocked_perk);
			if (data.show)
			{
				if (num > 0)
				{
					perk_buff_separator_prefab.Copy().transform.SetSiblingIndex(num2++);
				}
				PerkBuffItemGUI perkBuffItemGUI2 = perk_buff_item_prefab.Copy();
				perkBuffItemGUI2.transform.SetSiblingIndex(num2++);
				perkBuffItemGUI2.Draw(data);
				num++;
			}
		}
		go_no_perks.SetActive(num == 0);
		table_perks_buffs.Reposition();
		table_perks_buffs.repositionNow = true;
	}

	public override void OnAboveWindowClosed()
	{
		Redraw();
		InitPlatformDependentStuff();
		IniGamepadAndTooltips();
		_equipping_item = null;
		_equipment_state = EquipmentState.None;
		if (!BaseGUI.for_gamepad)
		{
			_toolbar.Redraw();
		}
	}

	private void IniGamepadAndTooltips()
	{
		if (BaseGUI.for_gamepad)
		{
			_inventory_panel.InitGamepad(base.gamepad_controller);
		}
		TooltipBubbleGUI.ChangeAvaibility(available: true);
	}

	public override void CloseFromGameGUI()
	{
		if (_inventory_state == InventoryGUIState.BagIsOpen)
		{
			CloseBag();
		}
		_inventory_panel.Hide();
		base.CloseFromGameGUI();
		TooltipBubbleGUI.ChangeAvaibility(available: false);
	}

	public void OnItemOver()
	{
		if (BaseGUI.for_gamepad)
		{
			UpdateButtonTips();
		}
	}

	public void OnItemOut()
	{
		if (!BaseGUI.for_gamepad && !(_context_menu_target_item == null))
		{
			_context_menu_target_item.SetVisualyOveredState(overed: true, by_gamepad: false);
		}
	}

	public void OnItemPressed()
	{
		if (_inventory_panel.selected_widget_is_not_main)
		{
			if (_inventory_state == InventoryGUIState.BagIsOpen && (InventoryPanelGUI.last != _inventory_panel || _inventory_panel.selected_widget.inventory_data == _current_open_bag))
			{
				return;
			}
			if (_inventory_state == InventoryGUIState.Standart)
			{
				Item inventory_data = _inventory_panel.selected_widget.inventory_data;
				if (inventory_data == null || !inventory_data.is_bag)
				{
					return;
				}
			}
		}
		OnItemPressedItem(selected_item);
	}

	private void OnToolbeltItemOver(BaseItemCellGUI item)
	{
		if (BaseGUI.for_gamepad)
		{
			UpdateButtonTips();
		}
	}

	private void OnToolbeltItemOut(BaseItemCellGUI item)
	{
	}

	private void OnItemGUIPressed(BaseItemCellGUI item)
	{
		if (_inventory_panel.selected_widget_is_not_main)
		{
			if (_inventory_state == InventoryGUIState.BagIsOpen && (InventoryPanelGUI.last != _inventory_panel || _inventory_panel.selected_widget.inventory_data == _current_open_bag))
			{
				return;
			}
			if (_inventory_state == InventoryGUIState.Standart && item.GetComponent<ToolbeltItemGUI>() == null)
			{
				Item item2 = _inventory_panel.selected_widget?.inventory_data;
				if (item2 == null || !item2.is_bag)
				{
					return;
				}
			}
		}
		OnItemPressedItem(item.item);
	}

	private bool ItemIsEquipped(Item item)
	{
		if (!item.is_equipped)
		{
			return MainGame.me.player.data.secondary_inventory.Contains(item);
		}
		return true;
	}

	private void OnItemPressedItem(Item item)
	{
		if (item == null || item.IsEmpty())
		{
			return;
		}
		if (_inventory_state == InventoryGUIState.BagIsOpen)
		{
			if (BaseGUI.for_gamepad)
			{
				if (item.is_bag)
				{
					if (_current_open_bag != item)
					{
						CloseBag();
						GJTimer.AddTimer(0.01f, delegate
						{
							OpenBag(item);
						});
					}
					else
					{
						CloseBag();
					}
					return;
				}
				bool num = InventoryPanelGUI.last == _inventory_panel;
				Item from_not_open_bag2 = null;
				if (num && _inventory_panel.selected_widget_is_not_main)
				{
					from_not_open_bag2 = _inventory_panel.selected_widget.inventory_data;
				}
				if (LazyInput.GetKeyDown(GameKey.Action))
				{
					MoveItemToBag(item, 1, from_bag: false, from_not_open_bag2);
					return;
				}
				Debug.Log("#BAG# Gamepad: Moving [" + item.id + "] items from inventory to bag.");
				int maxMoveCount = GetMaxMoveCount(item, to_bag: true, count_in_bags: false, from_not_open_bag2);
				if (item.definition.stack_count == 1 || maxMoveCount == 1)
				{
					MoveItemToBag(item, 1, from_bag: false, from_not_open_bag2);
					return;
				}
				if (item.definition.stack_count > 1 && LazyInput.GetKeyDown(GameKey.MoveAllStack))
				{
					MoveItemToBag(item, maxMoveCount, from_bag: false, from_not_open_bag2);
					return;
				}
				base.button_tips.Deactivate();
				GUIElements.me.item_count.Open(item.id, 1, maxMoveCount, delegate(int chosen_count)
				{
					MoveItemToBag(item, chosen_count, from_bag: false, from_not_open_bag2);
				});
				GUIElements.me.item_count.SetOnHide(delegate
				{
					if (BaseGUI.for_gamepad)
					{
						base.button_tips.Activate();
						base.gamepad_controller.Enable(GamepadNavigationController.OpenMethod.GetAll);
						base.gamepad_controller.SetFocusedItem(_inventory_panel.selected_item_gui.gamepad_item);
					}
				});
				return;
			}
			if (item.is_bag)
			{
				Debug.Log("#BAG# Mouse: Open/close Bag \"" + item.id + "\"");
				if (_current_open_bag != item)
				{
					CloseBag();
					GJTimer.AddTimer(0.01f, delegate
					{
						OpenBag(item);
					});
				}
				else
				{
					CloseBag();
				}
				return;
			}
			bool num2 = InventoryPanelGUI.last == _inventory_panel;
			Item from_not_open_bag = null;
			if (num2 && _inventory_panel.selected_widget_is_not_main)
			{
				from_not_open_bag = _inventory_panel.selected_widget.inventory_data;
			}
			if (LazyInput.GetKeyDown(GameKey.RightClick))
			{
				Debug.Log("#BAG# Mouse: Moving 1 [" + item.id + "] item from Inventory to bag.");
				MoveItemToBag(item, 1, from_bag: false, from_not_open_bag);
				return;
			}
			Debug.Log("#BAG# Mouse: Moving [" + item.id + "] items from inventory to bag.");
			int maxMoveCount2 = GetMaxMoveCount(item, to_bag: true, count_in_bags: false, from_not_open_bag);
			if (item.definition.stack_count == 1 || maxMoveCount2 == 1)
			{
				MoveItemToBag(item, 1, from_bag: false, from_not_open_bag);
				return;
			}
			if (item.definition.stack_count > 1 && LazyInput.GetKeyDown(GameKey.MoveAllStack))
			{
				MoveItemToBag(item, maxMoveCount2, from_bag: false, from_not_open_bag);
				return;
			}
			base.button_tips.Deactivate();
			GUIElements.me.item_count.Open(item.id, 1, maxMoveCount2, delegate(int chosen_count)
			{
				MoveItemToBag(item, chosen_count, from_bag: false, from_not_open_bag);
			});
			return;
		}
		if (BaseGUI.for_gamepad || !LazyInput.GetKeyDown(GameKey.RightClick))
		{
			if (item.is_bag)
			{
				Debug.Log("#BAG# Opening Bag \"" + item.id + "\"");
				OpenBag(item);
			}
			else
			{
				OnItemEquip(item, from_context_menu: false);
			}
			return;
		}
		bool num3 = MainGame.me.player.data.secondary_inventory.Contains(item);
		BubbleWidgetDataOptions bubbleWidgetDataOptions = new BubbleWidgetDataOptions();
		bool flag = item.definition.can_be_used;
		if (flag && item.GetGrayedCooldownPercent() > 0)
		{
			flag = false;
		}
		if (!num3)
		{
			if (item.is_bag)
			{
				if (_inventory_state == InventoryGUIState.Standart)
				{
					bubbleWidgetDataOptions.AddOption("open", delegate
					{
						OpenBag(item);
					});
				}
				else if (_current_open_bag == item)
				{
					bubbleWidgetDataOptions.AddOption("close", CloseBag);
				}
				else
				{
					bubbleWidgetDataOptions.AddOption("open", delegate
					{
						CloseBag();
						GJTimer.AddTimer(0.01f, delegate
						{
							OpenBag(item);
						});
					});
				}
			}
			else
			{
				bubbleWidgetDataOptions.AddOption("use", UseItem, flag);
			}
		}
		if (CanBeEquipped(item))
		{
			bubbleWidgetDataOptions.AddOption(ItemIsEquipped(item) ? "unequip" : "equip", delegate
			{
				OnItemEquip(item, from_context_menu: true);
			});
		}
		else
		{
			bubbleWidgetDataOptions.AddOption("equip", null, enabled: false);
		}
		if (!item.definition.player_cant_throw_out)
		{
			bubbleWidgetDataOptions.AddOption("destroy", OnDestroyItem);
		}
		_context_menu_target_item = _inventory_panel.selected_item_gui;
		ContextMenuBubbleGUI.Show(bubbleWidgetDataOptions, Input.mousePosition, delegate
		{
			if (_context_menu_target_item != null)
			{
				_context_menu_target_item.SetVisualyOveredState(overed: false, by_gamepad: false);
			}
			_context_menu_target_item = null;
		});
	}

	public void OnToolbarClicked(int index)
	{
		switch (_equipment_state)
		{
		case EquipmentState.None:
		{
			string text = MainGame.me.save.equipped_items[index];
			if (!string.IsNullOrEmpty(text))
			{
				StartEquipment(new Item(text, 1), from_inventory: false);
			}
			break;
		}
		case EquipmentState.StartedFromInventory:
		case EquipmentState.StartedFromToolbar:
			if (_equipping_item != null)
			{
				EquipCurrentItem(index);
				GUIElements.me.equip_to_toolbar.Hide();
			}
			break;
		}
	}

	public void OnEquipmentBackClicked()
	{
		if (_equipment_state != 0)
		{
			if (_equipment_state == EquipmentState.StartedFromToolbar)
			{
				MainGame.me.save.UnEquip(_equipping_item.id);
			}
			GUIElements.me.equip_to_toolbar.Hide();
		}
	}

	private void OnItemEquip(Item item, bool from_context_menu)
	{
		if (!MainGame.me.player.data.secondary_inventory.Contains(item))
		{
			Item inventory_data = _inventory_panel.selected_widget.inventory_data;
			if (_inventory_panel.selected_widget_is_not_main && (inventory_data == null || !inventory_data.is_bag))
			{
				return;
			}
		}
		if (item == null || item.IsEmpty() || item.durability_state == Item.DurabilityState.Broken)
		{
			return;
		}
		if (item.definition.can_be_used && !item.definition.cooldown.has_expression)
		{
			StartEquipment(item, from_inventory: true);
		}
		else
		{
			if (item.definition.equipment_type == ItemDefinition.EquipmentType.None)
			{
				return;
			}
			bool num = ItemIsEquipped(item);
			GamepadNavigationItem gamepadNavigationItem = ((!BaseGUI.for_gamepad) ? null : base.gamepad_controller?.focused_item);
			Sounds.OnToolEquip(!num);
			if (num)
			{
				MainGame.me.player.UnEquipItem(item);
			}
			else
			{
				Item inventory_data2 = _inventory_panel.selected_widget.inventory_data;
				Item try_from_bag = null;
				if (inventory_data2 != null && inventory_data2.is_bag)
				{
					try_from_bag = inventory_data2;
				}
				MainGame.me.player.EquipItem(item, -1, try_from_bag);
			}
			Redraw();
			if (BaseGUI.for_gamepad)
			{
				UpdateButtonTips();
			}
			if (IsToolbeltItemFocused())
			{
				if (gamepadNavigationItem != null)
				{
					base.gamepad_controller.SetFocusedItem(gamepadNavigationItem);
				}
			}
			else
			{
				_inventory_panel?.selected_item_gui?.OnOver(BaseGUI.for_gamepad);
			}
		}
	}

	private void StartEquipment(Item item, bool from_inventory)
	{
		_equipping_item = item;
		_equipment_state = (from_inventory ? EquipmentState.StartedFromInventory : EquipmentState.StartedFromToolbar);
		Sounds.PlaySound("gui_item_pickup");
		GUIElements.me.equip_to_toolbar.Open(item, from_inventory);
	}

	private void EquipCurrentItem(int toolbar_index = -1, Item try_from_bag = null)
	{
		if (_equipping_item != null)
		{
			MainGame.me.player.EquipItem(_equipping_item, toolbar_index, try_from_bag);
			Redraw();
			_equipping_item = null;
			_equipment_state = EquipmentState.None;
		}
	}

	protected override bool OnPressedSelect()
	{
		if (_inventory_state == InventoryGUIState.BagIsOpen)
		{
			bool to_bag = InventoryPanelGUI.last == _inventory_panel;
			Item from_not_open_bag = null;
			if (to_bag && _inventory_panel.selected_widget_is_not_main)
			{
				from_not_open_bag = _inventory_panel.selected_widget.inventory_data;
				if (!from_not_open_bag.is_bag)
				{
					return false;
				}
				if (from_not_open_bag == _current_open_bag)
				{
					return false;
				}
			}
			Item item = selected_item;
			if (item == null)
			{
				return false;
			}
			if (item.IsEmpty())
			{
				return false;
			}
			if (item.is_bag)
			{
				if (_inventory_state == InventoryGUIState.BagIsOpen)
				{
					if (_current_open_bag == item)
					{
						CloseBag();
					}
					else
					{
						CloseBag();
						GJTimer.AddTimer(0.01f, delegate
						{
							OpenBag(item);
						});
					}
				}
				else
				{
					OpenBag(item);
				}
				return true;
			}
			if (!item.CanBeInsertedInBag(_current_open_bag))
			{
				return false;
			}
			int maxMoveCount = GetMaxMoveCount(item, to_bag, count_in_bags: false, from_not_open_bag);
			if (item.definition.stack_count == 1 || maxMoveCount == 1)
			{
				MoveItemToBag(item, 1, !to_bag, from_not_open_bag);
				return true;
			}
			if (item.definition.stack_count > 1 && LazyInput.GetKeyDown(GameKey.MoveAllStack))
			{
				MoveItemToBag(item, maxMoveCount, !to_bag, from_not_open_bag);
				return true;
			}
			base.button_tips.Deactivate();
			GUIElements.me.item_count.Open(item.id, 1, maxMoveCount, delegate(int chosen_count)
			{
				MoveItemToBag(item, chosen_count, !to_bag, from_not_open_bag);
			});
			GUIElements.me.item_count.SetOnHide(delegate
			{
				if (BaseGUI.for_gamepad)
				{
					base.button_tips.Activate();
					base.gamepad_controller.Enable(GamepadNavigationController.OpenMethod.GetAll);
					if (to_bag)
					{
						if (_inventory_panel.selected_item_is_empty)
						{
							base.gamepad_controller.ReinitItems(focus_on_first_active: false);
						}
						else
						{
							base.gamepad_controller.SetFocusedItem(_inventory_panel.selected_item_gui.gamepad_item);
						}
					}
					else if (bag_panel.selected_item_is_empty)
					{
						base.gamepad_controller.ReinitItems(focus_on_first_active: false);
					}
					else
					{
						base.gamepad_controller.SetFocusedItem(bag_panel.selected_item_gui.gamepad_item);
					}
				}
			});
			return true;
		}
		if (BaseGUI.for_gamepad && InventoryPanelGUI.last != _inventory_panel)
		{
			return false;
		}
		if (_inventory_panel.selected_widget_is_not_main && !_inventory_panel.selected_widget.inventory_data.is_bag)
		{
			return false;
		}
		UseItem();
		return true;
	}

	protected override bool OnPressedBack()
	{
		if (_inventory_state == InventoryGUIState.BagIsOpen)
		{
			CloseBag();
		}
		else
		{
			GUIElements.me.game_gui.Hide();
		}
		return true;
	}

	private bool IsToolbeltItemFocused()
	{
		Item toolbelt_focused_item;
		return IsToolbeltItemFocused(out toolbelt_focused_item);
	}

	private bool IsToolbeltItemFocused(out Item toolbelt_focused_item)
	{
		bool flag = base.gamepad_controller?.focused_item?.GetComponent<ToolbeltItemGUI>() != null;
		toolbelt_focused_item = ((!flag) ? null : base.gamepad_controller.focused_item.GetComponent<BaseItemCellGUI>()?.item);
		return flag;
	}

	protected override bool OnPressedOption1()
	{
		if (_inventory_state == InventoryGUIState.Standart)
		{
			if (IsToolbeltItemFocused(out var toolbelt_focused_item))
			{
				GamepadNavigationItem focused_item = base.gamepad_controller.focused_item;
				OnItemPressedItem(toolbelt_focused_item);
				base.gamepad_controller.SetFocusedItem(focused_item);
				focused_item.UnFocus();
				focused_item.Focus();
			}
			else
			{
				OnItemPressed();
			}
		}
		else
		{
			bool flag = InventoryPanelGUI.last == _inventory_panel;
			Item item = null;
			if (flag && _inventory_panel.selected_widget_is_not_main)
			{
				item = _inventory_panel.selected_widget.inventory_data;
				if (!item.is_bag)
				{
					return false;
				}
				if (item == _current_open_bag)
				{
					return false;
				}
			}
			Item item2 = selected_item;
			if (!item2.CanBeInsertedInBag(_current_open_bag))
			{
				return false;
			}
			GamepadNavigationItem focused_item2 = base.gamepad_controller.focused_item;
			if (focused_item2 != null && item2 != null && !item2.IsEmpty() && !item2.is_bag)
			{
				if (IsToolbeltItemFocused(out var _))
				{
					Debug.LogError("TODO: Toolbelt Item Pressed Option 1");
				}
				else if (flag)
				{
					Debug.Log("#BAG# Gamepad: moving 1 [" + item2.id + "] item from Inventory to Bag", focused_item2);
					MoveItemToBag(item2, 1, from_bag: false, item);
				}
				else
				{
					Debug.Log("#BAG# Gamepad: moving 1 [" + item2.id + "] item from Bag to Inventory", focused_item2);
					MoveItemToBag(item2, 1, from_bag: true);
				}
			}
		}
		return true;
	}

	protected override bool OnPressedOption2()
	{
		if (_inventory_state == InventoryGUIState.BagIsOpen)
		{
			return false;
		}
		OnDestroyItem();
		return true;
	}

	private void OnDestroyItem()
	{
		Item item = null;
		if (_inventory_panel.selected_widget_is_not_main)
		{
			item = _inventory_panel.selected_widget.inventory_data;
			if (item == null || !item.is_bag)
			{
				return;
			}
		}
		if (selected_item != null && !selected_item.IsEmpty() && !selected_item.definition.player_cant_throw_out)
		{
			if (BaseGUI.for_gamepad)
			{
				base.button_tips.Deactivate();
				TooltipBubbleGUI.ChangeAvaibility(available: false);
			}
			string text = selected_item.definition.GetItemName();
			if (selected_item.value > 1)
			{
				text = text + " (" + selected_item.value + ")";
			}
			GUIElements.me.dialog.OpenYesNo(GJL.L("destroy_question", text), DestroyItem);
		}
	}

	private void DestroyItem()
	{
		if (_inventory_panel.selected_widget == null)
		{
			return;
		}
		Item item = null;
		if (_inventory_panel.selected_widget_is_not_main)
		{
			item = _inventory_panel.selected_widget.inventory_data;
			if (item == null || !item.is_bag)
			{
				return;
			}
		}
		if (selected_item != null && !selected_item.IsEmpty() && !selected_item.definition.player_cant_throw_out)
		{
			_inventory_panel.selected_widget.inventory_data.RemoveItem(selected_item);
			Redraw();
			if (BaseGUI.for_gamepad)
			{
				_inventory_panel.selected_item_gui.OnOver(by_gamepad: true);
			}
		}
	}

	private void UseItem()
	{
		Item item2 = null;
		if (_inventory_panel.selected_widget_is_not_main)
		{
			item2 = _inventory_panel.selected_widget.inventory_data;
			if (item2 == null || !item2.is_bag)
			{
				return;
			}
		}
		Item item = selected_item;
		if (item == null || item.IsEmpty())
		{
			return;
		}
		if (item.is_bag)
		{
			Debug.Log("#BAG# Item \"" + item.id + "\" is used!");
			if (_inventory_state == InventoryGUIState.BagIsOpen)
			{
				if (_current_open_bag == item)
				{
					CloseBag();
					return;
				}
				CloseBag();
				GJTimer.AddTimer(0.01f, delegate
				{
					OpenBag(item);
				});
			}
			else
			{
				OpenBag(item);
			}
		}
		else if (item.definition.can_be_used)
		{
			if (item.definition.close_inv_on_use)
			{
				GUIElements.me.game_gui.Hide();
				MainGame.me.player.UseItemFromInventory(item, null, item2);
			}
			else
			{
				MainGame.me.player.UseItemFromInventory(item, head_pos, item2);
				Redraw();
				_inventory_panel.selected_item_gui.OnOver(BaseGUI.for_gamepad);
			}
		}
	}

	private void Redraw()
	{
		_inventory_panel.Redraw();
		UpdateButtonTips();
		RedrawPlayerInfoAndToolbelt();
		if (_inventory_state == InventoryGUIState.BagIsOpen)
		{
			UpdateBagInventoryWidget();
		}
		UpdateFiltering();
	}

	private void UpdateFiltering()
	{
		if (_inventory_state == InventoryGUIState.Standart)
		{
			_inventory_panel.FilterItems((Item item, InventoryWidget widget) => InventoryWidget.ItemFilterResult.Active);
		}
		else
		{
			if (_inventory_state != InventoryGUIState.BagIsOpen)
			{
				return;
			}
			_inventory_panel.FilterItems(delegate(Item item, InventoryWidget widget)
			{
				if (widget != null && widget.inventory_data != null)
				{
					if (!widget.IsMain() && !widget.inventory_data.is_bag)
					{
						return InventoryWidget.ItemFilterResult.Inactive;
					}
					if (widget.inventory_data == _current_open_bag)
					{
						return InventoryWidget.ItemFilterResult.Inactive;
					}
				}
				if (item == null || item.IsEmpty())
				{
					return InventoryWidget.ItemFilterResult.Inactive;
				}
				if (item == _current_open_bag)
				{
					return InventoryWidget.ItemFilterResult.Active;
				}
				return (!item.CanBeInsertedInBag(_current_open_bag)) ? InventoryWidget.ItemFilterResult.Inactive : InventoryWidget.ItemFilterResult.Active;
			});
		}
	}

	private void RedrawToolbelt()
	{
		if (toolbelt_items == null)
		{
			return;
		}
		foreach (ToolbeltItemDescriptionGUI toolbelt_item in toolbelt_items)
		{
			toolbelt_item.item.DrawItem(MainGame.me.player.GetItemFromToolbelt(toolbelt_item.type));
		}
		toolbelt_widget.UpdateItemsCallbacksAndStuff(1);
	}

	private void RedrawPlayerInfoAndToolbelt()
	{
		RedrawToolbelt();
	}

	private bool CanBeEquipped(Item item)
	{
		if (item == null || item.IsEmpty())
		{
			return false;
		}
		ItemDefinition definition = item.definition;
		if (definition.IsWeapon() || definition.IsEquipment())
		{
			return item.durability_state != Item.DurabilityState.Broken;
		}
		return false;
	}

	private void UpdateButtonTips()
	{
		if (!BaseGUI.for_gamepad)
		{
			return;
		}
		base.button_tips.Activate();
		if (_inventory_state == InventoryGUIState.BagIsOpen && !IsToolbeltItemFocused())
		{
			if (selected_item == null)
			{
				base.button_tips.PrintClose();
				return;
			}
			InventoryPanelGUI inventoryPanelGUI = (_last_selected_panel = InventoryPanelGUI.last);
			bool flag = inventoryPanelGUI == _inventory_panel;
			if ((inventoryPanelGUI == null || inventoryPanelGUI.selected_widget_is_not_main) && ((flag && inventoryPanelGUI.selected_widget.inventory_data == null) || !inventoryPanelGUI.selected_widget.inventory_data.is_bag))
			{
				base.button_tips.PrintClose();
				return;
			}
			bool selected_item_is_empty = inventoryPanelGUI.selected_item_is_empty;
			if (inventoryPanelGUI.selected_item.is_bag)
			{
				bool flag2 = inventoryPanelGUI.selected_item == _current_open_bag;
				base.button_tips.Print(GameKeyTip.Select(flag2 ? "close" : "open"), GameKeyTip.Close());
				return;
			}
			int num = 0;
			if (!selected_item_is_empty)
			{
				Item from_not_open_bag = null;
				if (flag && _inventory_panel.selected_widget_is_not_main)
				{
					from_not_open_bag = _inventory_panel.selected_widget.inventory_data;
				}
				num = GetMaxMoveCount(inventoryPanelGUI.selected_item, flag, count_in_bags: false, from_not_open_bag);
			}
			if (num <= 1)
			{
				base.button_tips.Print(GameKeyTip.Select(flag ? "put" : "take", !selected_item_is_empty && num > 0), GameKeyTip.Close());
			}
			else
			{
				base.button_tips.Print(GameKeyTip.Select(flag ? "put" : "take", !selected_item_is_empty), GameKeyTip.Option1(GJL.L(flag ? "put" : "take") + " 1", !selected_item_is_empty), GameKeyTip.Close());
			}
			return;
		}
		if (selected_item == null)
		{
			base.button_tips.PrintClose();
			return;
		}
		Item toolbelt_focused_item = null;
		if (selected_item.is_bag)
		{
			base.button_tips.Print(GameKeyTip.Select("open"), GameKeyTip.Close());
			return;
		}
		bool num2 = IsToolbeltItemFocused(out toolbelt_focused_item);
		if (toolbelt_focused_item == null)
		{
			toolbelt_focused_item = selected_item;
		}
		bool flag3 = _inventory_panel.selected_widget.inventory_data?.is_bag ?? false;
		if (!num2 && _inventory_panel.selected_widget_is_not_main && !flag3)
		{
			base.button_tips.PrintClose();
			return;
		}
		bool flag4 = toolbelt_focused_item?.IsEmpty() ?? true;
		bool flag5 = !flag4 && toolbelt_focused_item.definition.can_be_used && toolbelt_focused_item.durability_state != Item.DurabilityState.Broken;
		bool flag6 = !flag4 && (CanBeEquipped(toolbelt_focused_item) || flag5);
		if (flag5 && toolbelt_focused_item.GetGrayedCooldownPercent() > 0)
		{
			flag5 = false;
		}
		base.button_tips.Print("\n", GameKeyTip.Select("use", !flag4 && flag5), GameKeyTip.Option1((flag4 || !flag6 || ItemIsEquipped(toolbelt_focused_item)) ? "unequip" : "equip", !flag4 && flag6), GameKeyTip.Option2("destroy", !flag4 && !toolbelt_focused_item.definition.player_cant_throw_out));
	}

	public void OpenBag(Item bag_item)
	{
		if (bag_item == null || bag_item.IsEmpty() || !bag_item.is_bag)
		{
			CloseBag();
			return;
		}
		_current_open_bag = bag_item;
		_inventory_state = InventoryGUIState.BagIsOpen;
		_bag_inventory = new MultiInventory(new Inventory(_current_open_bag, _current_open_bag.id));
		bag_panel.Open(_bag_inventory, 0, 1, clear_name: false, _current_open_bag.definition.bag_size_x);
		bag_panel.SetCallbacks(OnItemOverInBag, OnItemOutInBag, OnItemPressedInBag);
		Sounds.PlaySound("bag_open");
		UpdateFiltering();
		UpdateBagInventoryWidget();
		base.gamepad_controller.ReinitItems(focus_on_first_active: false);
		UpdateButtonTips();
	}

	public void CloseBag()
	{
		_current_open_bag = null;
		_inventory_state = InventoryGUIState.Standart;
		bag_panel.Hide();
		Sounds.PlaySound("bag_close");
		bool focus_on_first_active = false;
		base.gamepad_controller.ReinitItems(focus_on_first_active);
		UpdateFiltering();
		UpdateButtonTips();
	}

	private void UpdateBagInventoryWidget()
	{
		if (_inventory_state == InventoryGUIState.Standart)
		{
			CloseBag();
		}
		else
		{
			bag_panel.Redraw();
		}
	}

	public void OnItemOverInBag()
	{
		if (BaseGUI.for_gamepad)
		{
			UpdateButtonTips();
		}
	}

	public void OnItemOutInBag()
	{
		if (!BaseGUI.for_gamepad && !(_context_menu_target_item == null))
		{
			_context_menu_target_item.SetVisualyOveredState(overed: true, by_gamepad: false);
		}
	}

	public void OnItemPressedInBag()
	{
		Item item = selected_item;
		if (item == null || item.IsEmpty())
		{
			return;
		}
		if ((BaseGUI.for_gamepad && LazyInput.GetKeyDown(GameKey.Action)) || LazyInput.GetKeyDown(GameKey.RightClick))
		{
			Debug.Log("#BAG# Moving 1 item \"" + item.id + "\" from Bag to Inventory");
			MoveItemToBag(item, 1, from_bag: true);
			return;
		}
		Debug.Log("#BAG# Moving items \"" + item.id + "\" from Bag to Inventory");
		int maxMoveCount = GetMaxMoveCount(item, to_bag: false);
		if (item.definition.stack_count == 1 || maxMoveCount == 1)
		{
			MoveItemToBag(item, 1, from_bag: true);
			return;
		}
		if (item.definition.stack_count > 1 && LazyInput.GetKeyDown(GameKey.MoveAllStack))
		{
			MoveItemToBag(item, maxMoveCount, from_bag: true);
			return;
		}
		base.button_tips.Deactivate();
		GUIElements.me.item_count.Open(item.id, 1, maxMoveCount, delegate(int chosen_count)
		{
			MoveItemToBag(item, chosen_count, from_bag: true);
		});
		GUIElements.me.item_count.SetOnHide(delegate
		{
			if (BaseGUI.for_gamepad)
			{
				base.button_tips.Activate();
				base.gamepad_controller.Enable(GamepadNavigationController.OpenMethod.GetAll);
				base.gamepad_controller.SetFocusedItem(bag_panel.selected_item_gui.gamepad_item);
			}
		});
	}

	private int GetMaxMoveCount(Item item, bool to_bag, bool count_in_bags = false, Item from_not_open_bag = null)
	{
		if (from_not_open_bag != null && from_not_open_bag == _current_open_bag)
		{
			return 0;
		}
		if (to_bag && !item.CanBeInsertedInBag(_current_open_bag))
		{
			return 0;
		}
		MultiInventory multiInventory = (to_bag ? _inventory : _bag_inventory);
		MultiInventory multiInventory2 = (to_bag ? _bag_inventory : _inventory);
		if (from_not_open_bag != null)
		{
			multiInventory = new MultiInventory(new Inventory(from_not_open_bag));
		}
		int totalCount = multiInventory.GetTotalCount(item.id, MultiInventory.DestinationType.OnlyFirst, count_in_bags);
		int b = multiInventory2.CanAddCount(item.id);
		return Mathf.Min(totalCount, b);
	}

	private void MoveItemToBag(Item item, int count, bool from_bag = false, Item from_not_open_bag = null)
	{
		if (_inventory_state != InventoryGUIState.BagIsOpen || _bag_inventory == null || item == null || item.IsEmpty())
		{
			return;
		}
		Sounds.PlaySound("item_put");
		MultiInventory multiInventory = (from_bag ? _bag_inventory : _inventory);
		MultiInventory another_inventory = (from_bag ? _inventory : _bag_inventory);
		InventoryPanelGUI inventoryPanelGUI = (from_bag ? bag_panel : _inventory_panel);
		int value = inventoryPanelGUI.selected_item.value;
		if (!from_bag && from_not_open_bag != null)
		{
			multiInventory = new MultiInventory(new Inventory(from_not_open_bag));
		}
		if (multiInventory.MoveItemTo(another_inventory, item, count, use_only_first_from_inventory: true, allow_bag: false))
		{
			Redraw();
			bag_panel.Redraw();
			inventoryPanelGUI.UpdateSelection();
			if (count >= value)
			{
				TooltipsManager.Redraw();
			}
		}
	}
}
