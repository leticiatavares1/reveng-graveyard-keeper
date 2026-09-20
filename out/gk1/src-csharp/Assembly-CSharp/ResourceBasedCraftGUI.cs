using System;
using System.Collections.Generic;
using UnityEngine;

public class ResourceBasedCraftGUI : BaseCraftGUI
{
	private enum State
	{
		Start,
		CraftOrChoose,
		ChoosingItem
	}

	public static CraftDefinition last_previewed_craft;

	[HideInInspector]
	public UITable craft_list_table;

	public GameObject close_btn;

	public GameObject back_for_inactive_stuff;

	public GameObject plus;

	public BaseItemCellGUI main_ingredient;

	public List<BaseItemCellGUI> ingredients;

	public UI2DSprite decoration;

	[NonSerialized]
	public GamepadSelectableButton craft_button;

	private CraftDefinition _current_craft_definition;

	private State _state;

	private Transform _main_ingredient_container;

	public UILabel header;

	public UILabel label_craft_btn;

	public UILabel label_resourse_hint;

	protected Item _selected_item;

	private ItemDefinition _specific_item;

	private List<string> _allowed_ids = new List<string>();

	private List<string> _multiquality_ids;

	public const string SIENCE_FONT_TOKEN = "(sci)";

	public override void Init()
	{
		craft_button = GetComponentInChildren<GamepadSelectableButton>(includeInactive: true);
		craft_button.Init();
		craft_list_table = GetComponentInChildren<UITable>(includeInactive: true);
		main_ingredient.SetCallbacks(OnItemOver, null, OnChooseItem);
		_main_ingredient_container = main_ingredient.transform.parent;
		base.Init();
	}

	public void Open(WorldGameObject craftery_wgo, CraftDefinition.CraftType allowed_type = CraftDefinition.CraftType.ResourcesBasedCraft)
	{
		if (base.is_shown || base.isActiveAndEnabled)
		{
			return;
		}
		CommonOpen(craftery_wgo, allowed_type);
		header.text = GJL.L(craftery_wgo.obj_id);
		_current_craft_definition = null;
		DrawText();
		_allowed_ids.Clear();
		foreach (CraftDefinition craft in crafts)
		{
			if (craft.needs == null || craft.needs.Count == 0)
			{
				Debug.LogError("Craft " + craft.id + " has no needs!");
			}
			else
			{
				_allowed_ids.Add(craft.needs[0].id);
			}
		}
		string craft_preset = craftery_wgo.obj_def.craft_preset;
		if (UseCustomDecorations())
		{
			if (string.IsNullOrEmpty(craft_preset))
			{
				decoration.sprite2D = null;
			}
			else
			{
				decoration.sprite2D = EasySpritesCollection.GetSprite(craft_preset);
			}
		}
		if (ingredients.Count > 0)
		{
			ingredients.Add(ingredients[0].Copy());
			ingredients.Add(ingredients[0].Copy());
		}
		main_ingredient.InitInputBehaviour();
		foreach (BaseItemCellGUI ingredient in ingredients)
		{
			ingredient.InitInputBehaviour();
			ingredient.SetCallbacks(OnIngredientOver, null, null);
		}
		GUIElements.me.resource_picker.ClearResultDelegate();
		GUIElements.me.resource_picker.Hide();
		_current_craft_definition = null;
		_state = State.Start;
		_current_craft_definition = null;
		Redraw();
		if ((object)craftery_wgo != null && craftery_wgo.components?.craft?.last_craft_id != null && craftery_wgo.obj_def != null && !craftery_wgo.obj_def.dont_restore_last_craft)
		{
			CraftDefinition dataOrNull = GameBalance.me.GetDataOrNull<CraftDefinition>(craftery_wgo.components.craft.last_craft_id);
			if (dataOrNull != null && dataOrNull.needs.Count > 0)
			{
				OnResourcePickerClosed(dataOrNull.needs[0]);
			}
		}
		if (BaseGUI.for_gamepad)
		{
			base.button_tips.Activate();
		}
		craft_list_table.Reposition();
	}

	private void DrawText()
	{
		string text = craftery_wgo.obj_def.inventory_preset;
		if (!string.IsNullOrEmpty(text))
		{
			text = "_" + text;
		}
		label_craft_btn.text = GJL.L("btn_craft" + text);
		label_resourse_hint.text = GJL.L("craft_pick_res_hint" + text);
		if (_selected_item != null && _current_craft_definition != null && _current_craft_definition.sub_type == CraftDefinition.CraftSubType.SurveySciencePoints)
		{
			label_craft_btn.text = GJL.L("btn_science_decompose");
			label_resourse_hint.text = GJL.L("science_decompose", "+(sci)" + _current_craft_definition.output_to_wgo[0].value);
		}
		else if (_current_craft_definition != null)
		{
			label_resourse_hint.text = "";
		}
	}

	public override void Redraw()
	{
		base.Redraw();
		Redraw(null);
		DrawText();
		if (object_info != null)
		{
			object_info.Draw(craftery_wgo.GetUniversalObjectInfo());
		}
	}

	protected void Redraw(ItemDefinition specific_item)
	{
		_specific_item = specific_item;
		switch (_state)
		{
		case State.Start:
			main_ingredient.DrawEmpty();
			foreach (BaseItemCellGUI ingredient in ingredients)
			{
				ingredient.Deactivate();
			}
			break;
		}
		back_for_inactive_stuff.SetActive(value: false);
		craft_list_table.Reposition();
		craft_list_table.repositionNow = true;
		craft_button.SetCallbacks(OnCraftButtonPressed, OnCraftButtonOver);
		List<Item> list = new List<Item>();
		if (_current_craft_definition != null)
		{
			foreach (Item need in _current_craft_definition.needs)
			{
				list.Add(new Item(need));
			}
			foreach (Item item in _current_craft_definition.needs_from_wgo)
			{
				if (craftery_wgo.obj_def.additional_header_items.Contains(item.id))
				{
					list.Add(new Item(item));
				}
			}
		}
		if (GlobalCraftControlGUI.is_global_control_active && _current_craft_definition != null)
		{
			float num = _current_craft_definition.gratitude_points_craft_cost?.EvaluateFloat() ?? 0f;
			if (num > 0f)
			{
				list.Add(new Item("gratitude_as_item", (int)num));
			}
		}
		if (specific_item != null)
		{
			list[0].id = specific_item.id;
		}
		if (list.Count > 0 && list[0].definition == null)
		{
			_current_craft_definition = null;
			list.Clear();
		}
		Vector3 localPosition = _main_ingredient_container.localPosition;
		localPosition.x = ((_state != 0 && list.Count > 1) ? (-80) : 0);
		_main_ingredient_container.localPosition = localPosition;
		if (_current_craft_definition == null)
		{
			plus.Deactivate();
			foreach (BaseItemCellGUI ingredient2 in ingredients)
			{
				ingredient2.Deactivate();
			}
			craft_list_table.Reposition();
			craft_button.SetEnabled(enabled: false);
			if (BaseGUI.for_gamepad && _state != State.ChoosingItem)
			{
				base.gamepad_controller.Enable(GamepadNavigationController.OpenMethod.GetAll);
				base.gamepad_controller.SetFocusedItem(main_ingredient.gamepad_item);
			}
		}
		else
		{
			main_ingredient.DrawIngredient(list[0], base.multi_inventory, deactivate_colliders: false, init_tooltip: true);
			list.RemoveAt(0);
			plus.SetActive(list.Count > 0);
			foreach (BaseItemCellGUI ingredient3 in ingredients)
			{
				if (list.Count == 0)
				{
					ingredient3.Deactivate();
					continue;
				}
				ingredient3.DrawIngredient(list[0], base.multi_inventory, deactivate_colliders: false, init_tooltip: true, "", craftery_wgo.data);
				list.RemoveAt(0);
			}
			craft_list_table.Reposition();
			craft_button.SetEnabled(CanCraft());
			if (BaseGUI.for_gamepad && _state != State.ChoosingItem)
			{
				base.gamepad_controller.Enable(GamepadNavigationController.OpenMethod.GetAll);
				base.gamepad_controller.SetFocusedItem(CanCraft() ? craft_button.navigation_item : main_ingredient.gamepad_item);
			}
		}
		DrawText();
	}

	private void OnIngredientOver(BaseItemCellGUI item_gui)
	{
		if (BaseGUI.for_gamepad)
		{
			base.button_tips.Print(GameKeyTip.Select(active: false), GameKeyTip.Close());
		}
		else
		{
			item_gui.SetVisualyOveredState(overed: false, BaseGUI.for_gamepad);
		}
	}

	private void OnItemOver()
	{
		if (BaseGUI.for_gamepad)
		{
			base.button_tips.Print(GameKeyTip.Select(), GameKeyTip.Close());
		}
	}

	public void OnCraftButtonPressed()
	{
		PrayCraftGUI component = GetComponent<PrayCraftGUI>();
		if (component != null)
		{
			if (GUIElements.me.pray_craft?.pray_craft != null)
			{
				component.OnPrayButtonPressed();
			}
		}
		else
		{
			OnCraft();
		}
	}

	private void OnCraftButtonOver()
	{
		if (BaseGUI.for_gamepad)
		{
			base.button_tips.Print(GameKeyTip.Select(CanCraft()), GameKeyTip.Close());
		}
	}

	private void OnChooseItem()
	{
		ChooseItem();
		Redraw(_specific_item);
		base.button_tips.Deactivate();
		close_btn.Deactivate();
	}

	private void OnCraft()
	{
		if (!CanCraft())
		{
			return;
		}
		WorldGameObject other_obj_override = null;
		if (GlobalCraftControlGUI.is_global_control_active && craftery_wgo != null)
		{
			WorldZone myWorldZone = craftery_wgo.GetMyWorldZone();
			if (myWorldZone != null && !myWorldZone.IsPlayerInZone())
			{
				other_obj_override = craftery_wgo;
			}
		}
		OnCraft(_current_craft_definition, _selected_item, _multiquality_ids, 1, null, other_obj_override);
	}

	protected virtual bool CanCraft()
	{
		_multiquality_ids = null;
		if (_specific_item != null)
		{
			_multiquality_ids = new List<string>();
			foreach (Item need in _current_craft_definition.needs)
			{
				_multiquality_ids.Add(need.id);
			}
			_multiquality_ids[0] = _specific_item.id;
		}
		return CanCraft(_current_craft_definition, _multiquality_ids);
	}

	protected override bool OnPressedBack()
	{
		OnClosePressed();
		return true;
	}

	protected virtual void ChooseItem()
	{
		WorldGameObject player = MainGame.me.player;
		if (GlobalCraftControlGUI.is_global_control_active && craftery_wgo != null && !WorldZone.GetZoneOfObject(craftery_wgo).IsPlayerInZone())
		{
			player = craftery_wgo;
		}
		GUIElements.me.resource_picker.Open(player, (Item item, InventoryWidget widget) => (!IsItemAllowed(item)) ? InventoryWidget.ItemFilterResult.Inactive : InventoryWidget.ItemFilterResult.Active, OnResourcePickerClosed);
		_state = State.ChoosingItem;
	}

	private bool IsItemAllowed(Item item)
	{
		if (item == null)
		{
			return false;
		}
		if (item.id.Contains(":"))
		{
			string nameWithoutQualitySuffix = item.definition.GetNameWithoutQualitySuffix();
			if (!_allowed_ids.Contains(nameWithoutQualitySuffix))
			{
				return _allowed_ids.Contains(item.id);
			}
			return true;
		}
		return _allowed_ids.Contains(item.id);
	}

	protected virtual void OnResourcePickerClosed(Item item)
	{
		string text = ((item != null) ? item.id : "");
		if (item != null)
		{
			_selected_item = item;
			bool flag = false;
			if (text.Contains(":"))
			{
				string nameWithoutQualitySuffix = item.definition.GetNameWithoutQualitySuffix();
				if (_allowed_ids.Contains(nameWithoutQualitySuffix))
				{
					flag = true;
					text = nameWithoutQualitySuffix;
				}
			}
			if (_allowed_ids.Contains(text))
			{
				foreach (CraftDefinition craft in crafts)
				{
					if (craft.needs[0].id == text)
					{
						_current_craft_definition = craft;
						break;
					}
				}
			}
			_state = ((_current_craft_definition != null) ? State.CraftOrChoose : State.Start);
			Redraw(flag ? item.definition : null);
		}
		if (BaseGUI.for_gamepad)
		{
			base.button_tips.Activate();
			base.gamepad_controller.Enable();
			base.gamepad_controller.ReinitItems(focus_on_first_active: true);
		}
		else
		{
			close_btn.Activate();
		}
		if (GetComponent<PrayCraftGUI>() != null)
		{
			GetComponent<PrayCraftGUI>().OnResourcePickerClosed(item);
		}
		DrawText();
	}

	public override void Update()
	{
		if (!(craft_list_table == null))
		{
			craft_list_table.Reposition();
			if (!BaseGUI.for_gamepad && LazyInput.GetKeyDown(GameKey.Interaction))
			{
				OnCraftButtonPressed();
			}
			base.Update();
		}
	}

	public override void Hide(bool play_hide_sound = true)
	{
		if (!(GetComponent<PrayCraftGUI>() != null) || GetComponent<PrayCraftGUI>().CanClose())
		{
			while (ingredients.Count > 1)
			{
				ingredients[1].DestroyGO();
				ingredients.RemoveAt(1);
			}
			base.Hide(play_hide_sound);
		}
	}

	protected bool UseCustomDecorations()
	{
		return GetComponent<PrayCraftGUI>() == null;
	}
}
