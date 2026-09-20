using UnityEngine;

public class SoulExtractorGUI : BaseGUI
{
	private const int BODY_ITEMS_GROUP = 1;

	public const string MIN_DAMAGE_ID = "min_damage_chance";

	public const string MAX_DAMAGE_ID = "max_damage_chance";

	[SerializeField]
	private BodyPanelGUI _body_panel_gui;

	[SerializeField]
	private InventoryWidget _inventory_widget;

	[SerializeField]
	private GameObject _no_body_label;

	[SerializeField]
	private UIButton _remove_body_button;

	[SerializeField]
	private UIButton _extract_soul_button;

	[SerializeField]
	private CraftItemGUI _body_item_gui;

	[SerializeField]
	private UniversalObjectInfoGUI _universal_object_info;

	[SerializeField]
	private SoulExtractorInfoWidget soul_info_widget;

	[SerializeField]
	[Space]
	private GamepadNavigationItem _soul_extractor_button_nav_item;

	[SerializeField]
	private GamepadNavigationItem _exhume_body_button_nav_item;

	private WorldGameObject _soul_extractor_obj;

	private BaseItemCellGUI _last_body_item;

	private Inventory _parts_inventory = new Inventory(Item.empty);

	private Item _body;

	public override void Init()
	{
		base.Init();
		_inventory_widget.Init();
		_inventory_widget.interaction_enabled = false;
		_body_panel_gui.skull_bar.on_enable_skulls_frame += OnSkullsOver;
		_body_panel_gui.skull_bar.on_disable_skulls_frame += OnSkullsOut;
		_body_item_gui.gamepad_navigation_item.SetCallbacks(delegate
		{
			_body_item_gui.selection_frame.Activate();
			base.button_tips.Print(GameKeyTip.Select(), GameKeyTip.Close());
			Sounds.OnGUIHover();
		}, delegate
		{
			_body_item_gui.selection_frame.Deactivate();
		}, DropBody);
		_soul_extractor_button_nav_item.SetCallbacks(delegate
		{
			base.button_tips.Print(GameKeyTip.Select(), GameKeyTip.Close());
			Sounds.OnGUIHover();
		}, null, StartExtractionCraft);
		_body_panel_gui.skull_bar.InitGamepadItem();
	}

	public void Open(WorldGameObject craft_obj)
	{
		base.Open();
		_universal_object_info.Draw(craft_obj.GetUniversalObjectInfo());
		_soul_extractor_obj = craft_obj;
		_parts_inventory = new Inventory(Item.empty);
		_parts_inventory.data.inventory_size = 1;
		_body = craft_obj.GetBodyFromInventory();
		_body_panel_gui.Draw(_body);
		_extract_soul_button.isEnabled = false;
		soul_info_widget.SetActive(active: false);
		_exhume_body_button_nav_item.SetCallbacks(delegate
		{
			base.button_tips.Print(GameKeyTip.Select(_body != null), GameKeyTip.Close());
			Sounds.OnGUIHover();
		}, null, DropBody);
		if (_body == null)
		{
			DrawEmpty();
			return;
		}
		if (BaseGUI.for_gamepad)
		{
			base.gamepad_controller.ReinitItems(focus_on_first_active: false);
			base.gamepad_controller.Enable();
			base.gamepad_controller.SetFocusedItem(_exhume_body_button_nav_item, animate_auto_scroll: false);
			_soul_extractor_button_nav_item.active = false;
		}
		foreach (Item item in _body.inventory)
		{
			if (item.definition.type == ItemDefinition.ItemType.SoulBodyPart)
			{
				if (TryAddSoulPartToInventory(item))
				{
					if (BaseGUI.for_gamepad)
					{
						_soul_extractor_button_nav_item.active = true;
						base.gamepad_controller.SetFocusedItem(_soul_extractor_button_nav_item, animate_auto_scroll: false);
					}
					_extract_soul_button.isEnabled = true;
					soul_info_widget.SetActive(active: true);
					soul_info_widget.SetData(GetDamageChance(craft_obj, is_min_chance: true), GetDamageChance(craft_obj, is_min_chance: false));
					soul_info_widget.Redraw();
				}
				break;
			}
			if (item.definition.id == "sin_shard_body_part")
			{
				TryAddSoulPartToInventory(item);
			}
		}
		DrawBody();
	}

	public override void Hide(bool play_hide_sound = true)
	{
		ClearItemsGrid();
		_inventory_widget.Hide();
		if (GlobalCraftControlGUI.is_global_control_active)
		{
			GUIElements.me.global_craft_control_gui.Open();
		}
		base.Hide(play_hide_sound);
	}

	public void DropBody()
	{
		for (int i = 0; i < _soul_extractor_obj.data.inventory.Count; i++)
		{
			Item item = _soul_extractor_obj.data.inventory[i];
			if (item.definition.type == ItemDefinition.ItemType.Body)
			{
				_soul_extractor_obj.GiveItemToPlayersHands(item);
				Hide(play_hide_sound: false);
				break;
			}
		}
	}

	public void StartExtractionCraft()
	{
		Item item = _parts_inventory.data.inventory[0];
		CraftDefinition craftDefinition = GetCraftDefinition(item);
		if (craftDefinition != null)
		{
			_soul_extractor_obj.components.craft.Craft(craftDefinition, new Item(item));
			if (_soul_extractor_obj.is_current_craft_gratitude)
			{
				_soul_extractor_obj.SetParam("craft_started_from_gc", 1f);
			}
			Hide();
		}
		else
		{
			Debug.LogError("Not found extraction craft for item " + item.id);
		}
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

	private void DrawEmpty()
	{
		ClearItemsGrid();
		_remove_body_button.isEnabled = false;
		_extract_soul_button.isEnabled = false;
		base.button_tips.Print(GameKeyTip.Close());
		_no_body_label.SetActive(value: true);
	}

	private void DrawBody()
	{
		_inventory_widget.Open(_parts_inventory, BaseGUI.for_gamepad, 1);
		_no_body_label.SetActive(value: false);
		_remove_body_button.isEnabled = !GlobalCraftControlGUI.is_global_control_active;
	}

	private bool TryAddSoulPartToInventory(Item item)
	{
		item.ReplaceItemIfNeeded();
		_parts_inventory.data.inventory.Add(item);
		return true;
	}

	private void ClearItemsGrid()
	{
		_parts_inventory.data.inventory.Clear();
		_inventory_widget.Redraw();
	}

	private CraftDefinition GetCraftDefinition(Item soul_item)
	{
		return GameBalance.me.GetDataOrNull<CraftDefinition>(_soul_extractor_obj.obj_id + ":" + soul_item.id);
	}

	private static void RemoveBodyPartFromBody(Item body, Item item)
	{
		foreach (Item item2 in body.inventory)
		{
			if (item2.id == item.id)
			{
				body.RemoveItem(item, 1);
				break;
			}
			foreach (Item item3 in item2.inventory)
			{
				if (item3.id == item.id)
				{
					item2.RemoveItem(item, 1);
					return;
				}
			}
		}
	}

	private float GetDamageChance(WorldGameObject craftery_wgo, bool is_min_chance)
	{
		string obj_id = craftery_wgo.obj_id;
		switch (obj_id)
		{
		case "soul_extractor":
			if (!is_min_chance)
			{
				return 0.3f;
			}
			return 0.1f;
		case "soul_extractor_2":
			if (!is_min_chance)
			{
				return 0.15f;
			}
			return 0.05f;
		case "soul_extractor_3":
			return 0f;
		default:
			Debug.LogError("Not expected value \"" + obj_id + "\"");
			return 0f;
		}
	}

	public void OnSkullsOver()
	{
		if (BaseGUI.for_gamepad)
		{
			base.button_tips.Print(GameKeyTip.Close(), GameKeyTip.RightStick(active: true, gamepad_only: true, translate: true, "move_tip"));
		}
	}

	public void OnSkullsOut()
	{
		_ = BaseGUI.for_gamepad;
	}
}
