using System;
using System.Collections.Generic;
using UnityEngine;

public class SoulHealerGUI : BaseGUI
{
	[SerializeField]
	private UniversalObjectInfoGUI _universal_info;

	[SerializeField]
	private SoulHealingWidget _soul_healing_widget;

	[SerializeField]
	[Space]
	private List<SinItem> _sin_items = new List<SinItem>();

	private WorldGameObject _wgo;

	private BaseItemCellGUI _current_gui_item;

	private SinItem _current_sin_item;

	private float sins_heal_rate;

	public SoulHealingWidget soul_healing_widget => _soul_healing_widget;

	public override void Init()
	{
		_soul_healing_widget.Init();
		_soul_healing_widget.SetActive(active: true);
		SoulHealingWidget soulHealingWidget = _soul_healing_widget;
		soulHealingWidget.ON_GUI_ITEM_CHANGE = (Action)Delegate.Combine(soulHealingWidget.ON_GUI_ITEM_CHANGE, new Action(Redraw));
		SoulHealingWidget soulHealingWidget2 = _soul_healing_widget;
		soulHealingWidget2.ON_FOCUS_GAMEPAD_ITEM = (Action<GamepadNavigationItem>)Delegate.Combine(soulHealingWidget2.ON_FOCUS_GAMEPAD_ITEM, new Action<GamepadNavigationItem>(FocusGamepadItem));
		base.Init();
	}

	public void Open(WorldGameObject obj)
	{
		base.Open();
		_wgo = obj;
		for (int i = 0; i < _sin_items.Count; i++)
		{
			if (!_sin_items[i].IsSinUnlocked(_wgo))
			{
				_sin_items[i].gameObject.SetActive(value: false);
				continue;
			}
			_sin_items[i].SetSinValuesInSkulls();
			_sin_items[i].item_cell_gui.SetCallbacks(OnItemSinOver, null, OnItemSinPress);
			_sin_items[i].item_cell_gui.InitInputBehaviour();
			_sin_items[i].item_cell_gui.InitTooltips();
			_sin_items[i].item_cell_gui.DrawItem(obj.data.GetItemByIndex(i));
		}
		DrawLabelsAndCalculateHealRate();
		_soul_healing_widget.sins_heal_rate = sins_heal_rate;
		_soul_healing_widget.Draw(obj, _sin_items);
		_universal_info.Draw(obj.GetUniversalObjectInfo());
		if (BaseGUI.for_gamepad)
		{
			base.button_tips.Activate();
			base.gamepad_controller.Enable();
			base.gamepad_controller.ReinitItems(focus_on_first_active: true);
		}
	}

	public override void Hide(bool play_hide_sound = true)
	{
		_wgo = null;
		_current_gui_item = null;
		_current_sin_item = null;
		if (GlobalCraftControlGUI.is_global_control_active)
		{
			GUIElements.me.global_craft_control_gui.Open();
		}
		base.Hide(play_hide_sound);
	}

	protected override bool OnPressedBack()
	{
		OnClosePressed();
		return true;
	}

	public override void OnClosePressed()
	{
		if (GlobalCraftControlGUI.is_global_control_active)
		{
			GUIElements.me.global_craft_control_gui.Open();
		}
		base.OnClosePressed();
	}

	private void Redraw()
	{
		DrawLabelsAndCalculateHealRate();
		_soul_healing_widget.sins_heal_rate = sins_heal_rate;
		_soul_healing_widget.Redraw();
	}

	private void DrawLabelsAndCalculateHealRate()
	{
		sins_heal_rate = 0f;
		Item inserted_item = _soul_healing_widget.inserted_item;
		bool flag = inserted_item != null && !inserted_item.IsEmpty();
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		for (int i = 0; i < _sin_items.Count; i++)
		{
			if (!_sin_items[i].IsSinUnlocked(_wgo))
			{
				continue;
			}
			SetParamAccordingSinPart(_sin_items[i], 0f);
			_sin_items[i].SetSinValuesInSkulls();
			Item item = _sin_items[i].item_cell_gui.item;
			if (item != null && !item.IsEmpty())
			{
				num = item.GetWhiteSkullsValue();
				num2 = item.GetRedSkullsValue();
				if (num < 0)
				{
					num = 0;
				}
				if (num2 < 0)
				{
					num2 = 0;
				}
			}
			if (flag)
			{
				Item sinItemByTypeFromItem = SinItem.GetSinItemByTypeFromItem(_sin_items[i].item_type, inserted_item);
				if (sinItemByTypeFromItem != null && !sinItemByTypeFromItem.IsEmpty())
				{
					_sin_items[i].SetSinValuesInSkulls(is_sin_item_set: true, sinItemByTypeFromItem.GetRedSkullsValue(), sinItemByTypeFromItem.GetWhiteSkullsValue(), num2, num);
					num3++;
					SetParamAccordingSinPart(_sin_items[i], 1f);
				}
				else
				{
					_sin_items[i].SetSinValuesInSkulls(is_sin_item_set: false, 0, 0, num2, num);
				}
			}
			else
			{
				_sin_items[i].SetSinValuesInSkulls(is_sin_item_set: false, 0, 0, num2, num);
			}
			num = 0;
			num2 = 0;
			sins_heal_rate += _sin_items[i].GetHealRate();
		}
		if (num3 > 0)
		{
			sins_heal_rate /= num3;
		}
	}

	private void OnItemSinPress(BaseItemCellGUI item_gui)
	{
		_current_gui_item = item_gui;
		if (item_gui.TryGetComponent<SinItem>(out var component))
		{
			_current_sin_item = component;
			if (item_gui.item != null && !item_gui.item.IsEmpty())
			{
				_wgo.data.RemoveItemByIndex(_sin_items.FindIndex((SinItem x) => x == _current_sin_item));
				List<Item> cant_insert = new List<Item>();
				if (!GlobalCraftControlGUI.is_global_control_active)
				{
					MainGame.me.player.TryPutToInventory(new List<Item> { item_gui.item }, out cant_insert);
				}
				else
				{
					_wgo.PutToAllPossibleInventories(new List<Item> { item_gui.item }, out cant_insert);
				}
				if (cant_insert.Count > 0)
				{
					_wgo.DropItems(cant_insert);
				}
				SetParamIsOrganInserted(0f);
				_current_gui_item.DrawEmpty();
				Redraw();
				if (BaseGUI.for_gamepad)
				{
					base.gamepad_controller.Enable(GamepadNavigationController.OpenMethod.GetAll);
					base.gamepad_controller.SetFocusedItem(_current_gui_item.gamepad_item);
				}
				return;
			}
			string organ_item_id = SinItem.GetOrganIdBySin(component);
			if (string.IsNullOrEmpty(organ_item_id))
			{
				return;
			}
			WorldGameObject obj = MainGame.me.player;
			if (GlobalCraftControlGUI.is_global_control_active && _wgo != null)
			{
				obj = _wgo;
			}
			GUIElements.me.resource_picker.Open(obj, delegate(Item item, InventoryWidget widget)
			{
				if (item == null || item.IsEmpty())
				{
					return InventoryWidget.ItemFilterResult.Hide;
				}
				if (item.definition.type != ItemDefinition.ItemType.BodyUniversalPart)
				{
					return InventoryWidget.ItemFilterResult.Inactive;
				}
				string text = item.id;
				if (text.Contains(":"))
				{
					text = text.Split(':')[0];
				}
				return (!(text == organ_item_id)) ? InventoryWidget.ItemFilterResult.Inactive : InventoryWidget.ItemFilterResult.Active;
			}, OnItemSinForInsertionPicked);
		}
		else
		{
			Debug.LogError("SinItem component not found", this);
		}
	}

	private void OnItemSinOver(BaseItemCellGUI item_gui)
	{
		if (BaseGUI.for_gamepad)
		{
			base.button_tips.Print(GameKeyTip.Select(item_gui.item.IsEmpty() ? "select" : "change"), GameKeyTip.Close());
		}
	}

	private void OnItemSinForInsertionPicked(Item selected_item)
	{
		if (selected_item == null || selected_item.IsEmpty())
		{
			if (BaseGUI.for_gamepad)
			{
				base.gamepad_controller.Enable(GamepadNavigationController.OpenMethod.GetAll);
				base.gamepad_controller.SetFocusedItem(_current_gui_item.gamepad_item);
			}
			return;
		}
		Item item = new Item(selected_item)
		{
			value = 1,
			equipped_as = ItemDefinition.EquipmentType.None
		};
		_current_gui_item.DrawItem(item);
		MultiInventory.PlayerMultiInventory player_mi = MultiInventory.PlayerMultiInventory.DontChange;
		if (GlobalCraftControlGUI.is_global_control_active && !WorldZone.GetZoneOfObject(_wgo).IsPlayerInZone())
		{
			player_mi = MultiInventory.PlayerMultiInventory.ExcludePlayer;
		}
		MainGame.me.player.GetMultiInventory(null, "", player_mi, include_toolbelt: false, sortWGOS: true).RemoveItem(item);
		_wgo.data.AddItemByIndex(item, _sin_items.FindIndex((SinItem x) => x == _current_sin_item));
		SetParamIsOrganInserted(1f);
		Redraw();
		if (BaseGUI.for_gamepad)
		{
			base.gamepad_controller.Enable(GamepadNavigationController.OpenMethod.GetAll);
			base.gamepad_controller.SetFocusedItem(_current_gui_item.gamepad_item);
		}
	}

	private void SetParamIsOrganInserted(float value)
	{
		_wgo.SetParam("is_" + SinItem.GetOrganIdBySin(_current_sin_item) + "_inserted", value);
	}

	private void SetParamAccordingSinPart(SinItem sin_item, float value)
	{
		_wgo.SetParam(sin_item.item_type.ToString().ToLower(), value);
	}

	private void FocusGamepadItem(GamepadNavigationItem gamepad_item)
	{
		base.gamepad_controller.Enable(GamepadNavigationController.OpenMethod.GetAll);
		base.gamepad_controller.SetFocusedItem(gamepad_item);
	}
}
