using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEngine;

public class OrganEnhancerGUI : BaseGUI, CraftInterface
{
	public const string TOTAL_WHITE_SKULLS_WGO_RES = "total_white_skulls";

	public const string TOTAL_RED_SKULLS_WGO_RES = "total_red_skulls";

	public const int MAX_ORGAN_ENHANCING_CUP = 3;

	public UIWidget frame;

	[SerializeField]
	private BaseItemCellGUI _base_item_cell;

	[SerializeField]
	private GamepadSelectableButton _choose_craft_button;

	private InventoryWidget.ItemFilterDelegate _filter;

	private BaseItemCellGUI _current_item_gui;

	private MultiInventory _multi_inventory;

	private WorldGameObject _craftery_wgo;

	public override void Init()
	{
		_choose_craft_button.Init();
		_base_item_cell.SetCallbacks(OnItemOver, null, OnItemSelect);
		_choose_craft_button.SetCallbacks(OnChooseButtonPress, OnCraftButtonOver);
		base.Init();
	}

	public void Open(WorldGameObject craftery_wgo)
	{
		_craftery_wgo = craftery_wgo;
		_base_item_cell.DrawEmpty();
		_base_item_cell.InitInputBehaviour();
		MultiInventory.PlayerMultiInventory player_mi = MultiInventory.PlayerMultiInventory.DontChange;
		WorldGameObject worldGameObject;
		if (GlobalCraftControlGUI.is_global_control_active)
		{
			if (!WorldZone.GetZoneOfObject(craftery_wgo).IsPlayerInZone())
			{
				player_mi = MultiInventory.PlayerMultiInventory.ExcludePlayer;
			}
			worldGameObject = _craftery_wgo;
		}
		else
		{
			worldGameObject = MainGame.me.player;
		}
		_multi_inventory = worldGameObject.GetMultiInventory(null, "", player_mi, include_toolbelt: false, sortWGOS: true);
		base.Open();
		if (BaseGUI.for_gamepad)
		{
			base.button_tips.Activate();
			base.gamepad_controller.Enable();
			base.gamepad_controller.ReinitItems(focus_on_first_active: true);
		}
	}

	public void Hide(bool play_hide_sound = true, bool return_item_back = false)
	{
		if (return_item_back)
		{
			ReturnItemToInventory();
		}
		base.Hide(play_hide_sound);
	}

	public bool CanCraft(CraftDefinition craft, List<string> multiquality_ids = null, int amount = 1, List<Item> override_needs = null)
	{
		if (_multi_inventory.IsEnoughItems(craft.needs))
		{
			return craft.condition.EvaluateBoolean(_craftery_wgo, MainGame.me.player);
		}
		return false;
	}

	public bool OnCraft(CraftDefinition craft, Item try_use_particular_item = null, List<string> multiquality_ids = null, int amount = 1, List<Item> override_needs = null, WorldGameObject other_obj_override = null)
	{
		_craftery_wgo.components.craft.Craft(craft, null, null, craft.needs, ignore_crafts_list: true);
		_current_item_gui = null;
		GUIElements.me.craft.Hide(play_hide_sound: false);
		if (GlobalCraftControlGUI.is_global_control_active)
		{
			GUIElements.me.global_craft_control_gui.Open();
		}
		Hide();
		return true;
	}

	private void OnCraftButtonOver()
	{
		if (BaseGUI.for_gamepad)
		{
			base.button_tips.Print(GameKeyTip.Select(!(_current_item_gui == null) && !_current_item_gui.item.IsEmpty()), GameKeyTip.Close());
		}
	}

	public new void OnRightClick()
	{
		base.OnRightClick();
	}

	public void OnChooseButtonPress()
	{
		if (!(_current_item_gui == null) && !_current_item_gui.item.IsEmpty())
		{
			WorldGameObject craftery_wgo = _craftery_wgo;
			Item item = _current_item_gui.item;
			Hide();
			GUIElements.me.craft.OpenAsOrganEnhancer(craftery_wgo, item, ReturnItemToInventory);
		}
	}

	public static int GetCraftIndex(WorldGameObject wgo, string skulls_res)
	{
		if (wgo != null)
		{
			int num = wgo.GetParamInt(skulls_res);
			if (num < 0)
			{
				num = 0;
			}
			if (num == 3)
			{
				return 3;
			}
			return ++num;
		}
		return 0;
	}

	public static Item TryGetModifiedVersionOfItem(Item item)
	{
		if (item.definition.stack_count > 1)
		{
			Item item2 = new Item(item);
			item2.SetItemID(item.id + "_mod");
			return item2;
		}
		return item;
	}

	public static Item GetModifiedItemForCraftOutput(Item item, bool is_white_skull_change)
	{
		string text = item.id.Split('_')[0];
		if (!text.Contains(":"))
		{
			text = item.id + ":" + item.id;
		}
		int num = item.GetWhiteSkullsValue();
		int num2 = item.GetRedSkullsValue();
		if (is_white_skull_change)
		{
			if (num < 3)
			{
				num++;
			}
		}
		else if (num2 < 3)
		{
			num2++;
		}
		text += $"_{num2}_{num}";
		return new Item(text);
	}

	protected override bool OnPressedBack()
	{
		Hide(play_hide_sound: true, return_item_back: true);
		if (GUIElements.me.craft.is_shown && BaseGUI.for_gamepad)
		{
			GUIElements.me.craft.gamepad_controller.Enable();
			GUIElements.me.craft.gamepad_controller.ReinitItems(focus_on_first_active: true);
		}
		return true;
	}

	public override void OnClosePressed()
	{
		Hide(play_hide_sound: true, return_item_back: true);
	}

	private void OnItemOver(BaseItemCellGUI item_gui)
	{
		_current_item_gui = item_gui;
		if (BaseGUI.for_gamepad)
		{
			base.button_tips.Print(GameKeyTip.Select(item_gui.item.IsEmpty() ? "select" : "change"), GameKeyTip.Close());
		}
	}

	private void OnItemSelect(BaseItemCellGUI item_gui)
	{
		if (_current_item_gui != null && !_current_item_gui.item.IsEmpty())
		{
			ReturnItemToInventory();
		}
		_current_item_gui = item_gui;
		WorldGameObject obj = MainGame.me.player;
		if (GlobalCraftControlGUI.is_global_control_active && _craftery_wgo != null)
		{
			obj = _craftery_wgo;
		}
		GUIElements.me.resource_picker.Open(obj, delegate(Item item, InventoryWidget widget)
		{
			if (item == null || item.IsEmpty())
			{
				return InventoryWidget.ItemFilterResult.Hide;
			}
			return (item.definition.type == ItemDefinition.ItemType.BodyUniversalPart) ? ((!(item.id != "skull") || !(item.id != "bone") || !(item.id != "surgeon_mistake") || item.id.EndsWith("_dark") || !(item.id != "tr_bellas_ring") || (item.GetRedSkullsValue() >= 3 && item.GetWhiteSkullsValue() >= 3)) ? InventoryWidget.ItemFilterResult.Inactive : InventoryWidget.ItemFilterResult.Active) : InventoryWidget.ItemFilterResult.Hide;
		}, OnResourcePickerClosed, force_ignore_toolbelt: true);
	}

	private void OnResourcePickerClosed(Item item)
	{
		if (_current_item_gui == null)
		{
			return;
		}
		if (BaseGUI.for_gamepad)
		{
			base.button_tips.Activate();
			base.gamepad_controller.Enable();
			base.gamepad_controller.ReinitItems(focus_on_first_active: true);
		}
		if (item != null && !item.IsEmpty())
		{
			if (item.value > 1)
			{
				item = new Item(item)
				{
					value = 1
				};
			}
			_multi_inventory.RemoveItem(item, 1);
			_current_item_gui.DrawItem(item);
		}
		SetValuesToCrafteryWGO(item);
		_choose_craft_button.SetEnabled(enabled: true);
		if (BaseGUI.for_gamepad)
		{
			base.gamepad_controller.Enable(GamepadNavigationController.OpenMethod.GetAll);
			base.gamepad_controller.SetFocusedItem(_choose_craft_button.navigation_item);
		}
	}

	private void ReturnItemToInventory()
	{
		if (_current_item_gui == null)
		{
			return;
		}
		List<Item> cant_insert = new List<Item>();
		if (!GlobalCraftControlGUI.is_global_control_active)
		{
			MainGame.me.player.TryPutToInventory(new List<Item> { _current_item_gui.item }, out cant_insert);
		}
		else if (WorldZone.GetZoneOfObject(_craftery_wgo).IsPlayerInZone())
		{
			MainGame.me.player.TryPutToInventory(new List<Item> { _current_item_gui.item }, out cant_insert);
		}
		else
		{
			_craftery_wgo.PutToAllPossibleInventories(new List<Item> { _current_item_gui.item }, out cant_insert);
		}
		if (!cant_insert.IsNullOrEmpty())
		{
			_craftery_wgo.PutToAllPossibleInventories(cant_insert, out var cant_insert2);
			if (!cant_insert2.IsNullOrEmpty())
			{
				_craftery_wgo.DropItems(cant_insert2);
			}
		}
		_current_item_gui.DrawEmpty();
	}

	private void SetValuesToCrafteryWGO(Item organ_item)
	{
		if (organ_item != null && !organ_item.IsEmpty() && _craftery_wgo != null && organ_item.definition.type == ItemDefinition.ItemType.BodyUniversalPart)
		{
			_craftery_wgo.SetParam("total_white_skulls", organ_item.GetWhiteSkullsValue());
			_craftery_wgo.SetParam("total_red_skulls", organ_item.GetRedSkullsValue());
		}
	}

	private void ResetValuesFromCrafteryWGO()
	{
		if (_craftery_wgo != null)
		{
			_craftery_wgo.SetParam("total_white_skulls", 0f);
			_craftery_wgo.SetParam("total_red_skulls", 0f);
		}
	}
}
