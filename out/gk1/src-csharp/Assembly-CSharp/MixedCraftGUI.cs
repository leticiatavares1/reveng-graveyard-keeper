using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEngine;

public class MixedCraftGUI : BaseCraftGUI
{
	public UIWidget frame;

	private Dictionary<string, MixedCraftPresetGUI> _presets = new Dictionary<string, MixedCraftPresetGUI>();

	private GamepadSelectableButton _craft_button;

	private MixedCraftPresetGUI _current_preset;

	private bool _allow_empty;

	private InventoryWidget.ItemFilterDelegate _filter;

	private BaseItemCellGUI _current_item_gui;

	private int _current_item_gui_n = -1;

	private MultiInventory _multi_inventory;

	public override void Init()
	{
		_craft_button = GetComponentInChildren<GamepadSelectableButton>(includeInactive: true);
		_craft_button.Init();
		MixedCraftPresetGUI[] componentsInChildren = GetComponentsInChildren<MixedCraftPresetGUI>(includeInactive: true);
		foreach (MixedCraftPresetGUI mixedCraftPresetGUI in componentsInChildren)
		{
			_presets.Add(mixedCraftPresetGUI.name, mixedCraftPresetGUI);
		}
		foreach (MixedCraftPresetGUI value in _presets.Values)
		{
			value.Init(this, OnItemOver, null, OnItemSelect);
		}
		base.Init();
	}

	public void Open(WorldGameObject craftery_wgo, string preset_name, bool allow_empty, InventoryWidget.ItemFilterDelegate filter)
	{
		if (!_presets.ContainsKey(preset_name))
		{
			Debug.LogError("No MixCraftGUI preset for preset_name = " + preset_name);
			return;
		}
		CommonOpen(craftery_wgo, CraftDefinition.CraftType.MixedCraft);
		GUIElements.me.resource_picker.ClearResultDelegate();
		GUIElements.me.resource_picker.Hide();
		_current_preset = _presets[preset_name];
		_allow_empty = allow_empty;
		_filter = filter;
		MultiInventory.PlayerMultiInventory player_mi = MultiInventory.PlayerMultiInventory.DontChange;
		if (GlobalCraftControlGUI.is_global_control_active && !WorldZone.GetZoneOfObject(craftery_wgo).IsPlayerInZone())
		{
			player_mi = MultiInventory.PlayerMultiInventory.ExcludePlayer;
		}
		_multi_inventory = MainGame.me.player.GetMultiInventory(null, "", player_mi, include_toolbelt: false, sortWGOS: true);
		foreach (MixedCraftPresetGUI value in _presets.Values)
		{
			if (value != _current_preset)
			{
				value.Hide();
			}
		}
		_current_preset.Open(BaseGUI.for_gamepad);
		_craft_button.SetCallbacks(OnCraftPressed, OnCraftButtonOver);
		frame.width = _current_preset.ui_widget.width;
		frame.height = _current_preset.ui_widget.height;
		if (BaseGUI.for_gamepad)
		{
			base.gamepad_controller.ReinitItems(focus_on_first_active: true);
		}
		_craft_button.SetEnabled(enabled: false);
	}

	public void OpenAsAlchemy(WorldGameObject craftery_wgo, string preset_name)
	{
		Open(craftery_wgo, preset_name, allow_empty: false, AlchemyItemPickerFilter);
	}

	private InventoryWidget.ItemFilterResult AlchemyItemPickerFilter(Item item, InventoryWidget widget)
	{
		if (item == null || item.definition == null)
		{
			return InventoryWidget.ItemFilterResult.Hide;
		}
		if (item.definition.alch_type == ItemDefinition.AlchemyType.Universal || item.definition.alch_type == (ItemDefinition.AlchemyType)(_current_item_gui_n + 1))
		{
			foreach (Item selectedItem in _current_preset.GetSelectedItems())
			{
				if (selectedItem?.id == item?.id)
				{
					return InventoryWidget.ItemFilterResult.Inactive;
				}
			}
			return InventoryWidget.ItemFilterResult.Active;
		}
		return InventoryWidget.ItemFilterResult.Inactive;
	}

	private bool IsCraftAllowed()
	{
		int num = 0;
		List<Item> selectedItems = _current_preset.GetSelectedItems();
		foreach (Item item in selectedItems)
		{
			if (item.IsEmpty())
			{
				num++;
			}
		}
		if (num == selectedItems.Count || (!_allow_empty && num > 0))
		{
			return false;
		}
		return true;
	}

	public void OnCraftPressed()
	{
		if (!IsCraftAllowed())
		{
			return;
		}
		bool do_override_needs;
		CraftDefinition craftDefinition = GetCraftDefinition(for_empty: false, out do_override_needs);
		List<Item> list = null;
		bool flag = craftDefinition != null;
		if (!flag)
		{
			craftDefinition = GetCraftDefinition(for_empty: true, out do_override_needs);
		}
		if (craftDefinition == null)
		{
			Debug.LogError("no mixed craft definition for this situation", base.gameObject);
			return;
		}
		list = ((flag || do_override_needs) ? new List<Item>() : null);
		_current_preset?.ClearItems();
		WorldGameObject other_obj_override = null;
		if (GlobalCraftControlGUI.is_global_control_active && craftery_wgo != null)
		{
			WorldZone myWorldZone = craftery_wgo.GetMyWorldZone();
			if (myWorldZone != null && !myWorldZone.IsPlayerInZone())
			{
				other_obj_override = craftery_wgo;
			}
		}
		OnCraft(craftDefinition, null, null, 1, list, other_obj_override);
		if (IsASubWindow())
		{
			GUIElements.me.craft.Hide(play_hide_sound: false);
		}
	}

	private string GetCraftDefinitionId(bool for_empty, out bool do_override_needs)
	{
		string text = "mix:" + craftery_wgo.obj_id;
		do_override_needs = false;
		List<Item> selectedItems = _current_preset.GetSelectedItems();
		string text2 = string.Empty;
		string text3 = string.Empty;
		if (for_empty)
		{
			List<CraftDefinition> list = new List<CraftDefinition>();
			int count = selectedItems.Count;
			foreach (CraftDefinition craft_datum in GameBalance.me.craft_data)
			{
				if (craft_datum.craft_type != CraftDefinition.CraftType.MixedCraft || craft_datum.needs.Count != count)
				{
					continue;
				}
				for (int i = 0; i < count; i++)
				{
					if (craft_datum.needs[i]?.id == selectedItems[i]?.id)
					{
						list.Add(craft_datum);
						break;
					}
				}
			}
			if (list.Count == 0)
			{
				string text4 = string.Empty;
				foreach (Item item in selectedItems)
				{
					if (!string.IsNullOrEmpty(text4))
					{
						text4 += ", ";
					}
					text4 = text4 + "\"" + item.id + "\"";
				}
				Debug.LogWarning("Not found any proper alchemy ingridient for set {" + text4 + "}");
			}
			else
			{
				int index = Random.Range(0, list.Count);
				CraftDefinition craftDefinition = list[index];
				Debug.Log("Selected alchemy goo craft: " + craftDefinition.id);
				int num = 0;
				for (num = 0; num < selectedItems.Count && !(selectedItems[num].id == craftDefinition.needs[num].id); num++)
				{
					if (num == selectedItems.Count - 1)
					{
						Debug.LogError("Wrong shit happen while trying to find propper alchemy craft! Call Bulat, it's his shitcode!");
					}
				}
				for (int j = 0; j < craftDefinition.needs.Count; j++)
				{
					if (j != num)
					{
						if (string.IsNullOrEmpty(text2))
						{
							text2 = craftDefinition.needs[j]?.id;
							text2 = ItemDefinition.GetGooFromAlchemyIngridient(text2);
						}
						else if (string.IsNullOrEmpty(text3))
						{
							text3 = craftDefinition.needs[j]?.id;
							text3 = ItemDefinition.GetGooFromAlchemyIngridient(text3);
							break;
						}
					}
				}
			}
		}
		bool flag = string.IsNullOrEmpty(text2);
		bool flag2 = string.IsNullOrEmpty(text3);
		foreach (Item item2 in selectedItems)
		{
			if (for_empty && !flag)
			{
				flag = true;
				do_override_needs = true;
				text = text + ":" + text2;
			}
			else if (for_empty && !flag2)
			{
				flag2 = true;
				text = text + ":" + text3;
			}
			else
			{
				text = text + ":" + (for_empty ? "_" : (item2.IsEmpty() ? "" : item2.id));
			}
		}
		for (int k = selectedItems.Count; k < 4; k++)
		{
			text += ":";
		}
		return text;
	}

	private CraftDefinition GetCraftDefinition(bool for_empty, out bool do_override_needs)
	{
		string craftDefinitionId = GetCraftDefinitionId(for_empty, out do_override_needs);
		foreach (CraftDefinition craft in crafts)
		{
			if (craft.id == craftDefinitionId)
			{
				return craft;
			}
		}
		return null;
	}

	private void ReturnItemsToInventory()
	{
		if (!(_current_preset != null))
		{
			return;
		}
		List<Item> cant_insert = new List<Item>();
		MainGame.me.player.TryPutToInventory(_current_preset.GetSelectedItems(), out cant_insert);
		if (!cant_insert.IsNullOrEmpty())
		{
			craftery_wgo.PutToAllPossibleInventories(cant_insert, out var cant_insert2);
			if (!cant_insert2.IsNullOrEmpty())
			{
				craftery_wgo.DropItems(cant_insert2);
			}
		}
		_current_preset.ClearItems();
	}

	private void OnItemOver(BaseItemCellGUI item_gui)
	{
		_current_item_gui = item_gui;
		if (BaseGUI.for_gamepad)
		{
			UpdateGamepadTips(item_gui.item.IsEmpty() ? "select" : "change", active: true);
		}
	}

	private void OnItemSelect(BaseItemCellGUI item_gui)
	{
		_current_item_gui = item_gui;
		_current_item_gui_n = -1;
		for (int i = 0; i < _current_preset.items.Length; i++)
		{
			if (_current_preset.items[i] == _current_item_gui)
			{
				_current_item_gui_n = i;
				break;
			}
		}
		Debug.Log("OnItemSelect, n = " + _current_item_gui_n, item_gui);
		WorldGameObject player = MainGame.me.player;
		if (GlobalCraftControlGUI.is_global_control_active && craftery_wgo != null)
		{
			player = craftery_wgo;
		}
		GUIElements.me.resource_picker.Open(player, _filter, OnResourcePickerClosed);
	}

	private void OnCraftButtonOver()
	{
		_current_item_gui = null;
		if (BaseGUI.for_gamepad)
		{
			UpdateGamepadTips("select", IsCraftAllowed());
		}
	}

	private void OnResourcePickerClosed(Item item)
	{
		if (_current_item_gui == null)
		{
			return;
		}
		Item item2 = Item.empty;
		if (item != null)
		{
			item2 = new Item(item)
			{
				value = 1,
				equipped_as = ItemDefinition.EquipmentType.None
			};
			if (!_current_item_gui.item.IsEmpty())
			{
				_multi_inventory.AddItem(_current_item_gui.item);
			}
		}
		if (!item2.IsEmpty())
		{
			_multi_inventory.RemoveItem(item2);
			_current_item_gui.DrawItem(item2);
		}
		_craft_button.SetEnabled(IsCraftAllowed());
		if (BaseGUI.for_gamepad)
		{
			base.gamepad_controller.Enable(GamepadNavigationController.OpenMethod.GetAll);
			base.gamepad_controller.SetFocusedItem(IsCraftAllowed() ? _craft_button.navigation_item : _current_item_gui.gamepad_item);
		}
	}

	private void UpdateGamepadTips(string select_text, bool active)
	{
		base.button_tips.Print(GameKeyTip.Select(select_text, active), GameKeyTip.Close());
	}

	public override void Hide(bool play_hide_sound = true)
	{
		ReturnItemsToInventory();
		_current_item_gui = null;
		if (_current_preset != null)
		{
			_current_preset.Hide();
		}
		base.Hide(play_hide_sound);
	}

	private bool IsASubWindow()
	{
		return GUIElements.me.craft.is_shown;
	}

	protected override bool OnPressedBack()
	{
		OnClosePressed();
		return true;
	}

	public override void OnClosePressed()
	{
		if (IsASubWindow())
		{
			GUIElements.me.craft.Hide(play_hide_sound: false);
		}
		base.OnClosePressed();
	}

	protected override bool OnPressedPrevTab()
	{
		if (IsASubWindow())
		{
			return GUIElements.me.craft.PressPrevTab();
		}
		return base.OnPressedPrevTab();
	}

	protected override bool OnPressedNextTab()
	{
		if (IsASubWindow())
		{
			return GUIElements.me.craft.PressNextTab();
		}
		return base.OnPressedPrevTab();
	}
}
