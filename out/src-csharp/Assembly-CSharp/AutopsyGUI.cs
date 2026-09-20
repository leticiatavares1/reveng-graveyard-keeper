using LinqTools;
using UnityEngine;

public class AutopsyGUI : BaseGUI
{
	private const int BODY_ITEMS_GROUP = 1;

	private const int REMOVE_BODY_GROUP = 2;

	public GameObject no_body_label;

	private BodyPartGUI[] _body_parts;

	private InventoryWidget _inventory_widget;

	private CraftItemGUI _body_item_gui;

	public UIButton remove_body_button;

	private WorldGameObject _autopti_obj;

	private BodyPartGUI _current_part;

	private Item _current_part_inventory;

	private Inventory _parts_inventory = new Inventory(Item.empty);

	private BaseItemCellGUI _last_body_item;

	private UniversalObjectInfoGUI _universal_info;

	private Item _body;

	public BodyPanelGUI body_panel;

	public override void Init()
	{
		_body_parts = GetComponentsInChildren<BodyPartGUI>(includeInactive: true);
		BodyPartGUI[] body_parts = _body_parts;
		for (int i = 0; i < body_parts.Length; i++)
		{
			body_parts[i].Init();
		}
		_universal_info = GetComponentInChildren<UniversalObjectInfoGUI>(includeInactive: true);
		_inventory_widget = GetComponentInChildren<InventoryWidget>(includeInactive: true);
		_inventory_widget.Init();
		_inventory_widget.SetCallbacks(OnBodyItemOver, null, OnBodyItemPress);
		_body_item_gui = GetComponentInChildren<CraftItemGUI>(includeInactive: true);
		_body_item_gui.gamepad_navigation_item.SetCallbacks(delegate
		{
			_body_item_gui.selection_frame.Activate();
			base.button_tips.Print(GameKeyTip.Select(), GameKeyTip.Close());
			Sounds.OnGUIHover();
		}, delegate
		{
			_body_item_gui.selection_frame.Deactivate();
		}, DropBody);
		body_panel.button_item.SetCallbacks(delegate
		{
			_body_item_gui.selection_frame.Activate();
			base.button_tips.Print(GameKeyTip.Select(), GameKeyTip.Close());
			Sounds.OnGUIHover();
		}, delegate
		{
			_body_item_gui.selection_frame.Deactivate();
		}, DropBody);
		body_panel.skull_bar.on_enable_skulls_frame += OnSkullsOver;
		body_panel.skull_bar.on_disable_skulls_frame += OnSkullsOut;
		base.Init();
	}

	public void Open(WorldGameObject craft_obj)
	{
		base.Open();
		_body_item_gui.gamepad_navigation_item.UnFocus();
		_autopti_obj = craft_obj;
		_body = craft_obj.GetBodyFromInventory();
		_parts_inventory = new Inventory(Item.empty);
		_universal_info.Draw(craft_obj.GetUniversalObjectInfo());
		body_panel.Draw(_body);
		if (_body == null)
		{
			Debug.Log("AutopsyGUI: no body in inventory, drawing empty");
			DrawEmpty();
			return;
		}
		if (CanInsertInsideBody())
		{
			AddInsertionButtonToPartsInventory();
		}
		foreach (Item item in _body.inventory)
		{
			if (item.definition.type == ItemDefinition.ItemType.BodyUniversalPart)
			{
				TryAddBodyPartToPartsInventory(item);
			}
			foreach (Item item2 in item.inventory)
			{
				if (item2.id.Contains('\n'))
				{
					Debug.LogError("Item " + item2.id + " contains \\n in id. This should never happen.");
					item2.SetItemID(item2.id.Trim('\n'));
				}
				if (item2.definition.type == ItemDefinition.ItemType.BodyUniversalPart)
				{
					TryAddBodyPartToPartsInventory(item2);
				}
			}
		}
		DrawBody();
	}

	private void AddInsertionButtonToPartsInventory()
	{
		_parts_inventory.data.inventory.Add(new Item("insertion_button_pseudoitem"));
	}

	private bool CanInsertInsideBody()
	{
		return true;
	}

	private void TryAddBodyPartToPartsInventory(Item item)
	{
		if (GetExtractCraftDefinition(item) == null && item.id != "surgeon_mistake")
		{
			item = new Item("unknown_body_part");
		}
		item.ReplaceItemIfNeeded();
		if (!(item.id == "sin_shard_body_part") || DLCEngine.IsDLCAvailable(DLCEngine.DLCVersion.Souls))
		{
			_parts_inventory.data.inventory.Add(item);
		}
	}

	private void DrawEmpty()
	{
		ClearItemsGrid();
		BodyPartGUI[] body_parts = _body_parts;
		for (int i = 0; i < body_parts.Length; i++)
		{
			body_parts[i].Deactivate();
		}
		no_body_label.SetActive(value: true);
		base.button_tips.PrintClose();
		remove_body_button.isEnabled = false;
		_inventory_widget.interaction_enabled = false;
	}

	private void DrawBody()
	{
		_last_body_item = null;
		_inventory_widget.Open(_parts_inventory, BaseGUI.for_gamepad, 1);
		_inventory_widget.interaction_enabled = true;
		no_body_label.SetActive(value: false);
		if (BaseGUI.for_gamepad)
		{
			base.gamepad_controller.ReinitItems(focus_on_first_active: false);
			base.gamepad_controller.FocusOnFirstActive(1);
		}
		remove_body_button.isEnabled = !GlobalCraftControlGUI.is_global_control_active;
	}

	private void OnBodyItemOver(BaseItemCellGUI item_gui)
	{
		if (item_gui.item.id == "insertion_button_pseudoitem")
		{
			_last_body_item = item_gui;
			if (BaseGUI.for_gamepad)
			{
				base.button_tips.Print(GameKeyTip.Select("insertion_button_pseudoitem"), GameKeyTip.Close());
			}
			return;
		}
		CraftDefinition extractCraftDefinition = GetExtractCraftDefinition(item_gui.item);
		_last_body_item = item_gui;
		if (BaseGUI.for_gamepad)
		{
			base.button_tips.Print(GameKeyTip.Select("extract", extractCraftDefinition != null), GameKeyTip.Close());
		}
	}

	private void OnBodyItemPress(BaseItemCellGUI item_gui)
	{
		if (item_gui.item.id == "insertion_button_pseudoitem")
		{
			Debug.Log("Not done yet.");
			WorldGameObject obj = MainGame.me.player;
			if (GlobalCraftControlGUI.is_global_control_active && _autopti_obj != null)
			{
				obj = _autopti_obj;
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
				text = text.Replace("_dark", "");
				foreach (Item item in _parts_inventory.data.inventory)
				{
					if (item != null && !item.IsEmpty() && item.id.StartsWith(text))
					{
						return InventoryWidget.ItemFilterResult.Inactive;
					}
				}
				return (GetInsertCraftDefinition(item) == null) ? InventoryWidget.ItemFilterResult.Inactive : InventoryWidget.ItemFilterResult.Active;
			}, OnItemForInsertionPicked);
			return;
		}
		CraftDefinition craft_definition = GetExtractCraftDefinition(item_gui.item);
		if (craft_definition != null)
		{
			GUIElements.me.dialog.OpenYesNo(GJL.L("extract_question", item_gui.item.definition.GetItemName()), delegate
			{
				RemoveBodyPartFromBody(_body, item_gui.item);
				_autopti_obj.components.craft.CraftAsPlayer(craft_definition, item_gui.item);
				Hide();
			}, delegate
			{
			});
		}
	}

	public void OnItemForInsertionPicked(Item item)
	{
		if (item != null && !item.IsEmpty())
		{
			CraftDefinition insertCraftDefinition = GetInsertCraftDefinition(item);
			if (insertCraftDefinition == null)
			{
				Debug.LogError("Not found insertion CraftDefinition for item \"" + item.id + "\"");
				return;
			}
			_autopti_obj.components.craft.CraftAsPlayer(insertCraftDefinition, new Item(item.id, 1));
			Hide();
		}
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

	public override void OnAboveWindowClosed()
	{
		if (BaseGUI.for_gamepad && !(_last_body_item == null))
		{
			base.gamepad_controller.Enable(GamepadNavigationController.OpenMethod.GetAll);
			base.gamepad_controller.SetFocusedItem(_last_body_item.gamepad_item);
		}
	}

	private CraftDefinition GetExtractCraftDefinition(Item item)
	{
		if (item.IsEmpty())
		{
			return null;
		}
		string text = item.id;
		if (text.Contains(":"))
		{
			text = text.Split(':')[0];
		}
		CraftDefinition dataOrNull = GameBalance.me.GetDataOrNull<CraftDefinition>("ex:" + _autopti_obj.obj_id + ":" + text);
		if (dataOrNull != null && !MainGame.me.save.IsCraftVisible(dataOrNull))
		{
			return null;
		}
		return dataOrNull;
	}

	public CraftDefinition GetInsertCraftDefinition(Item item)
	{
		if (item == null || item.IsEmpty())
		{
			return null;
		}
		string text = item.id;
		if (text.Contains(":"))
		{
			text = text.Split(':')[0];
		}
		CraftDefinition dataOrNull = GameBalance.me.GetDataOrNull<CraftDefinition>("insert:" + _autopti_obj.obj_id + ":" + text);
		if (dataOrNull != null && !MainGame.me.save.IsCraftVisible(dataOrNull))
		{
			return null;
		}
		return dataOrNull;
	}

	public void DropBody()
	{
		for (int i = 0; i < _autopti_obj.data.inventory.Count; i++)
		{
			Item item = _autopti_obj.data.inventory[i];
			if (item.definition.type == ItemDefinition.ItemType.Body)
			{
				_autopti_obj.GiveItemToPlayersHands(item);
				Hide(play_hide_sound: false);
				break;
			}
		}
	}

	private void ClearItemsGrid()
	{
		_parts_inventory.data.inventory.Clear();
		_inventory_widget.Redraw();
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

	public override void Hide(bool play_hide_sound = true)
	{
		if (GlobalCraftControlGUI.is_global_control_active)
		{
			GUIElements.me.global_craft_control_gui.Open();
		}
		ClearItemsGrid();
		base.Hide(play_hide_sound);
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
