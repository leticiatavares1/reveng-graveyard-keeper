using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CraftGUI : BaseCraftGUI, CraftGUIInterface, CraftInterface
{
	public Action ON_CRAFT_GUI_HIDE;

	[HideInInspector]
	public CraftItemGUI craft_item_prefab;

	[HideInInspector]
	public UIScrollView scroll_view;

	public UITable craft_list_grid;

	[NonSerialized]
	public List<CraftItemGUI> list_items = new List<CraftItemGUI>();

	private Vector3 _panel_start_pos;

	private List<Item> _bodies = new List<Item>();

	private static ObjectCraftDefinition _remove_building_craft;

	private CraftItemGUI _exanded_item;

	public UITableOrGrid tabs_table;

	public CraftTabGUI craft_tab_prefab;

	private List<string> _tabs_ids = new List<string>();

	private List<CraftTabGUI> _tabs = new List<CraftTabGUI>();

	public UIWidget header_height_widget;

	public GameObject tabs_go;

	public bool has_amount_buttons;

	public GameObject mixed_craft_go;

	public CraftInterface custom_craft_interface;

	public UILabel LB;

	public UILabel RB;

	private const bool OLD_ALCHEMY_CRAFT = false;

	public CraftQueueGUI queue;

	public UILabel gamepad_hint_queue_big;

	private bool _gamepad_in_queue_area;

	private bool _gamepad_was_in_queue_area;

	private bool hide_after_build_gui;

	public static bool in_redraw_mode;

	private static CraftGUI _current_instance;

	public override void Init()
	{
		craft_item_prefab = GetComponentInChildren<CraftItemGUI>(includeInactive: true);
		craft_item_prefab.Init();
		if (craft_list_grid == null)
		{
			craft_list_grid = GetComponentInChildren<UITable>();
		}
		scroll_view = GetComponentInChildren<UIScrollView>(includeInactive: true);
		_panel_start_pos = scroll_view.transform.localPosition;
		if (tabs_go != null)
		{
			craft_tab_prefab.SetActive(active: false);
			tabs_go.SetActive(value: false);
		}
		queue?.Init(this);
		base.Init();
	}

	public void OpenAsBuild(WorldGameObject build_desk, CraftsInventory crafts_inventory)
	{
		hide_after_build_gui = true;
		custom_craft_interface = null;
		has_amount_buttons = false;
		Open(build_desk, crafts_inventory);
	}

	public void OpenCraftList(WorldGameObject craftery_wgo)
	{
		custom_craft_interface = null;
		has_amount_buttons = true;
		Open(craftery_wgo, null);
	}

	public void OpenAsRatCell(WorldGameObject rat_cell_wgo, Item rat_item)
	{
		CraftsInventory craftsInventory = new CraftsInventory
		{
			is_building = false,
			craft_type = CraftDefinition.CraftType.RatBuff
		};
		foreach (CraftDefinition craft_datum in GameBalance.me.craft_data)
		{
			if (craft_datum.craft_type != CraftDefinition.CraftType.RatBuff)
			{
				continue;
			}
			bool flag = true;
			if (craft_datum.item_needs.Count > 0)
			{
				flag = rat_item.IsEnoughItems(craft_datum.item_needs);
				foreach (Item item_need in craft_datum.item_needs)
				{
					if (!rat_item.HasItemInInventory(item_need.id))
					{
						flag = false;
						break;
					}
				}
			}
			if (!flag)
			{
				continue;
			}
			bool flag2 = true;
			if (craft_datum.item_output.Count > 0)
			{
				foreach (Item item in craft_datum.item_output)
				{
					if (rat_item.HasItemInInventory(item.id))
					{
						flag2 = false;
						break;
					}
				}
			}
			if (flag2)
			{
				craftsInventory.AddCraft(craft_datum.id);
			}
		}
		craftery_wgo = rat_cell_wgo;
		Open(craftery_wgo, craftsInventory);
	}

	public void OpenAsGrave(WorldGameObject grave_wgo, Item grave_part, ItemDefinition.ItemType grave_part_type)
	{
		if (grave_part == null)
		{
			grave_part = Item.empty;
		}
		string text = "";
		switch (grave_part_type)
		{
		case ItemDefinition.ItemType.GraveStone:
			text = "cross";
			break;
		case ItemDefinition.ItemType.GraveFence:
			text = "fence";
			break;
		default:
			Debug.LogError("Unsupported grave part type: " + grave_part_type);
			return;
		}
		CraftsInventory craftsInventory = new CraftsInventory
		{
			additional_crafts = new List<ObjectCraftDefinition>()
		};
		CraftDefinition fixCraftAndPrice = grave_part.GetFixCraftAndPrice(out var fix_price);
		ObjectCraftDefinition item = new ObjectCraftDefinition
		{
			id = "fix_grave_craft_" + text,
			icon = "i_b_hammer",
			needs = fix_price.inventory,
			enabled = (fix_price.inventory.Count > 0),
			energy = fixCraftAndPrice.energy
		};
		craftsInventory.additional_crafts.Add(item);
		fixCraftAndPrice = GameBalance.me.GetRemoveCraftForItem(grave_wgo.obj_id, grave_part.id);
		craftsInventory.additional_crafts.Add(new ObjectCraftDefinition
		{
			id = fixCraftAndPrice.id,
			icon = "i_b_remove",
			needs = fixCraftAndPrice.needs,
			craft_time = fixCraftAndPrice.craft_time,
			custom_name = "_remove_"
		});
		craftery_wgo = grave_wgo;
		custom_craft_interface = GUIElements.me.grave;
		Open(grave_wgo, craftsInventory);
		UniversalObjectInfo universalObjectInfo = grave_wgo.GetUniversalObjectInfo();
		universalObjectInfo.header = (grave_part.IsEmpty() ? GJL.L("no_grave_" + text) : GJL.L(grave_part.id));
		universalObjectInfo.descr = "";
		if (!grave_part.IsEmpty())
		{
			universalObjectInfo.descr = string.Format("+(wr){0}, " + grave_part.GetDurabilityHint(), grave_part.definition.GetQualityString(grave_part));
			universalObjectInfo.icon = "i_" + grave_part.id;
			universalObjectInfo.icon_color = "739ECCFF";
		}
		object_info.Draw(universalObjectInfo);
	}

	public void OpenAsOrganEnhancer(WorldGameObject craftery_wgo, Item item, Action on_gui_hide = null)
	{
		Hide();
		int craftIndex = OrganEnhancerGUI.GetCraftIndex(craftery_wgo, "total_white_skulls");
		int craftIndex2 = OrganEnhancerGUI.GetCraftIndex(craftery_wgo, "total_red_skulls");
		Item modifiedItemForCraftOutput = OrganEnhancerGUI.GetModifiedItemForCraftOutput(item, is_white_skull_change: true);
		Item modifiedItemForCraftOutput2 = OrganEnhancerGUI.GetModifiedItemForCraftOutput(item, is_white_skull_change: false);
		CraftDefinition dataOrNull = GameBalance.me.GetDataOrNull<CraftDefinition>($"organ_enhance_w_{craftIndex}");
		CraftDefinition craftDefinition = new CraftDefinition();
		craftDefinition.id = dataOrNull.id;
		craftDefinition.craft_in = new List<string> { "soul_workbench" };
		craftDefinition.sanity = new SmartExpression();
		craftDefinition.disable_multi_craft = dataOrNull.disable_multi_craft;
		craftDefinition.icon = "i_" + item.id.Split(':')[0] + "_w_upgrd";
		craftDefinition.condition = dataOrNull.condition;
		craftDefinition.needs = dataOrNull.needs;
		craftDefinition.energy = dataOrNull.energy;
		craftDefinition.craft_time = dataOrNull.craft_time;
		craftDefinition.enqueue_type = dataOrNull.enqueue_type;
		craftDefinition.output = new List<Item> { modifiedItemForCraftOutput };
		dataOrNull = craftDefinition;
		foreach (Item item2 in dataOrNull.output)
		{
			item2.min_value = new SmartExpression();
			item2.max_value = new SmartExpression();
			item2.self_chance = new SmartExpression
			{
				default_value = 1f
			};
		}
		CraftDefinition dataOrNull2 = GameBalance.me.GetDataOrNull<CraftDefinition>($"organ_enhance_r_{craftIndex2}");
		craftDefinition = new CraftDefinition();
		craftDefinition.id = dataOrNull2.id;
		craftDefinition.craft_in = new List<string> { "soul_workbench" };
		craftDefinition.sanity = new SmartExpression();
		craftDefinition.disable_multi_craft = dataOrNull2.disable_multi_craft;
		craftDefinition.icon = "i_" + item.id.Split(':')[0] + "_r_upgrd";
		craftDefinition.condition = dataOrNull2.condition;
		craftDefinition.needs = dataOrNull2.needs;
		craftDefinition.energy = dataOrNull2.energy;
		craftDefinition.craft_time = dataOrNull2.craft_time;
		craftDefinition.enqueue_type = dataOrNull2.enqueue_type;
		craftDefinition.output = new List<Item> { modifiedItemForCraftOutput2 };
		dataOrNull2 = craftDefinition;
		foreach (Item item3 in dataOrNull2.output)
		{
			item3.min_value = new SmartExpression();
			item3.max_value = new SmartExpression();
			item3.self_chance = new SmartExpression
			{
				default_value = 1f
			};
		}
		List<CraftDefinition> list = new List<CraftDefinition>();
		if (dataOrNull.condition.EvaluateBoolean(craftery_wgo, MainGame.me.player))
		{
			list.Add(dataOrNull);
		}
		if (dataOrNull2.condition.EvaluateBoolean(craftery_wgo, MainGame.me.player))
		{
			list.Add(dataOrNull2);
		}
		craftery_wgo.components.craft.crafts = list;
		custom_craft_interface = GUIElements.me.organ_enhancer_gui;
		Open(craftery_wgo, null);
		ON_CRAFT_GUI_HIDE = on_gui_hide;
	}

	private void Open(WorldGameObject craftery_wgo, CraftsInventory crafts_inventory, string add_custom_tab = null)
	{
		if (base.is_shown || base.isActiveAndEnabled)
		{
			return;
		}
		_current_instance = this;
		CraftItemGUI.last_item_pressed = null;
		base.crafts_inventory = crafts_inventory;
		in_redraw_mode = false;
		_gamepad_was_in_queue_area = (_gamepad_in_queue_area = false);
		if (mixed_craft_go != null)
		{
			mixed_craft_go.SetActive(value: false);
		}
		CraftComponent craftComponent = craftery_wgo?.components?.craft;
		if (craftComponent != null && craftComponent.is_crafting && craftComponent.current_craft != null && !craftComponent.current_craft.CanEnqueue())
		{
			NeedToCancelCraftsDialog(craftComponent, delegate
			{
				ProceedOpen(craftery_wgo, crafts_inventory, add_custom_tab);
			});
		}
		else
		{
			ProceedOpen(craftery_wgo, crafts_inventory, add_custom_tab);
		}
	}

	private void ProceedOpen(WorldGameObject craftery_wgo, CraftsInventory crafts_inventory, string add_custom_tab = null)
	{
		CommonOpen(craftery_wgo, crafts_inventory?.craft_type ?? CraftDefinition.CraftType.None);
		scroll_view.gameObject.SetActive(value: true);
		Debug.Log("Open craft window, crafts: " + crafts.Count);
		bool flag = false;
		_tabs_ids.Clear();
		foreach (CraftDefinition craft in crafts)
		{
			CraftItemGUI craftItemGUI = ExistingCraftItemWithSimilarCraft(craft);
			if (craftItemGUI != null)
			{
				craftItemGUI.AddOneMoreCraftDefinition(craft);
				continue;
			}
			craftItemGUI = craft_item_prefab.Copy();
			GJL.EnsureChildLabelsHasCorrectFont(craftItemGUI.gameObject, do_cache: false);
			craftItemGUI.Draw(craft);
			list_items.Add(craftItemGUI);
			if (!string.IsNullOrEmpty(craft.tab_id))
			{
				if (!_tabs_ids.Contains(craft.tab_id))
				{
					_tabs_ids.Add(craft.tab_id);
				}
			}
			else
			{
				flag = true;
			}
		}
		if (_tabs_ids.Count > 0 && flag)
		{
			_tabs_ids.Insert(0, "");
		}
		if (!string.IsNullOrEmpty(add_custom_tab))
		{
			_tabs_ids.Insert(0, "?" + add_custom_tab);
			if (_tabs_ids.Count == 1 && crafts.Count > 0)
			{
				_tabs_ids.Add("");
			}
		}
		RedrawCraftTabs();
		if (craftery_wgo.obj_def.interaction_type == ObjectDefinition.InteractionType.Builder)
		{
			AddRemoveBuildingItem();
		}
		UpdateAllAnchors();
		craft_list_grid.Reposition();
		craft_list_grid.repositionNow = true;
		if (BaseGUI.for_gamepad)
		{
			base.button_tips.PrintClose();
			CraftItemGUI lastCraftedItem = GetLastCraftedItem();
			if (lastCraftedItem == null)
			{
				ResetScroll();
			}
			base.gamepad_controller.ReinitItems(focus_on_first_active: false);
			base.gamepad_controller.SetFocusedItem((lastCraftedItem == null) ? null : lastCraftedItem.gamepad_navigation_item, animate_auto_scroll: false);
		}
		else
		{
			ResetScroll();
		}
		SetCustomDirectionForFirstAndLast();
		if (_tabs_ids.Count > 0)
		{
			bool flag2 = false;
			foreach (string tabs_id in _tabs_ids)
			{
				if (tabs_id == craftery_wgo.last_opened_tab)
				{
					flag2 = true;
					SwitchTab(tabs_id);
					break;
				}
			}
			if (!flag2)
			{
				SwitchTab(_tabs_ids[0]);
			}
		}
		scroll_view.ResetPosition();
		if (LB != null)
		{
			LB.text = GameKeyTip.GetIcon(GameKey.PrevTab);
		}
		if (RB != null)
		{
			RB.text = GameKeyTip.GetIcon(GameKey.NextTab);
		}
		queue?.Draw(craftery_wgo?.components?.craft);
	}

	private void RedrawCraftTabs()
	{
		if (tabs_go == null)
		{
			return;
		}
		tabs_go.SetActive(_tabs_ids.Count > 0);
		tabs_table.DestroyChildren(new CraftTabGUI[1] { craft_tab_prefab });
		header_height_widget.height = ((_tabs_ids.Count > 0) ? 82 : 50);
		_tabs.Clear();
		foreach (string tabs_id in _tabs_ids)
		{
			CraftTabGUI craftTabGUI = craft_tab_prefab.Copy();
			_tabs.Add(craftTabGUI);
			craftTabGUI.Draw(craftery_wgo, tabs_id, SwitchTab);
		}
		tabs_table.Reposition();
	}

	private void SwitchTab(string tab_id)
	{
		if (_exanded_item != null)
		{
			CollapseItem();
		}
		craftery_wgo.last_opened_tab = tab_id;
		foreach (CraftItemGUI list_item in list_items)
		{
			list_item.SetActive(list_item.craft_definition.tab_id == tab_id || list_item.craft_definition.id == "_remove_");
			if (list_item.craft_definition.id == "_remove_")
			{
				SetCustomDirectionForFirstAndLast(tab_id);
			}
		}
		bool flag = false;
		foreach (CraftTabGUI tab in _tabs)
		{
			tab.SetSelectedState(tab_id == tab.tab_id);
			if (tab.tab_id.StartsWith("?"))
			{
				flag = true;
			}
		}
		if (flag)
		{
			if (tab_id.StartsWith("?"))
			{
				scroll_view.gameObject.SetActive(value: false);
				mixed_craft_go.SetActive(value: true);
				GUIElements.me.mixed_craft_tabbed.OpenAsAlchemy(craftery_wgo, craftery_wgo.obj_def.craft_preset);
				if (BaseGUI.for_gamepad)
				{
					base.button_tips.Deactivate();
				}
			}
			else
			{
				bool activeInHierarchy = scroll_view.gameObject.activeInHierarchy;
				scroll_view.gameObject.SetActive(value: true);
				mixed_craft_go.SetActive(value: false);
				GUIElements.me.mixed_craft_tabbed.Hide(play_hide_sound: false);
				if (BaseGUI.for_gamepad)
				{
					base.button_tips.Activate();
				}
				if (!activeInHierarchy && BaseGUI.for_gamepad)
				{
					base.gamepad_controller.Enable();
				}
			}
		}
		craft_list_grid.Reposition();
		scroll_view.transform.localPosition = new Vector3(scroll_view.transform.localPosition.x, 0f, 0f);
		UpdateAllAnchors();
		scroll_view.ResetPosition();
		Sounds.OnGUITabClick();
		if (BaseGUI.for_gamepad)
		{
			base.gamepad_controller.ReinitItems(focus_on_first_active: true);
		}
	}

	public void OpenAsAlchemy(WorldGameObject craftery_wgo)
	{
		custom_craft_interface = null;
		CraftsInventory craftsInventory = new CraftsInventory
		{
			is_building = false,
			craft_type = CraftDefinition.CraftType.MixedCraft
		};
		foreach (string completed_one_time_craft in MainGame.me.save.completed_one_time_crafts)
		{
			CraftDefinition dataOrNull = GameBalance.me.GetDataOrNull<CraftDefinition>(completed_one_time_craft);
			if (dataOrNull != null && dataOrNull.craft_in.Contains(craftery_wgo.obj_id))
			{
				craftsInventory.AddCraft(completed_one_time_craft);
			}
		}
		has_amount_buttons = true;
		Open(craftery_wgo, craftsInventory, "alch");
	}

	private CraftItemGUI ExistingCraftItemWithSimilarCraft(CraftDefinition craft_definition)
	{
		if (craft_definition.output.Count == 0)
		{
			return null;
		}
		string id = craft_definition.output[0].id;
		ItemDefinition dataOrNull = GameBalance.me.GetDataOrNull<ItemDefinition>(id);
		if (dataOrNull != null && dataOrNull.quality_type != ItemDefinition.QualityType.Stars)
		{
			return null;
		}
		int count = craft_definition.needs.Count;
		bool flag = craft_definition.IsMultiqualityOutput();
		foreach (CraftItemGUI list_item in list_items)
		{
			CraftDefinition current_craft = list_item.current_craft;
			if (current_craft.needs.Count == count && current_craft.IsMultiqualityOutput() == flag && current_craft.output.Count != 0 && !(current_craft.output[0].id != id))
			{
				return list_item;
			}
		}
		return null;
	}

	private void AddRemoveBuildingItem()
	{
		if (_remove_building_craft == null)
		{
			_remove_building_craft = new ObjectCraftDefinition
			{
				id = "_remove_",
				icon = "i_b_remove"
			};
		}
		CraftItemGUI craftItemGUI = craft_item_prefab.Copy();
		GJL.EnsureChildLabelsHasCorrectFont(craftItemGUI.gameObject, do_cache: false);
		craftItemGUI.Draw(_remove_building_craft);
		list_items.Add(craftItemGUI);
	}

	public override bool OnCraft(CraftDefinition craft, Item try_use_particular_item = null, List<string> multiquality_ids = null, int amount = 1, List<Item> override_needs = null, WorldGameObject other_obj_override = null)
	{
		other_obj_override = null;
		if (GlobalCraftControlGUI.is_global_control_active && craftery_wgo != null)
		{
			WorldZone myWorldZone = craftery_wgo.GetMyWorldZone();
			if (myWorldZone != null && !myWorldZone.IsPlayerInZone())
			{
				other_obj_override = craftery_wgo;
			}
		}
		return base.OnCraft(craft, try_use_particular_item, multiquality_ids, amount, null, other_obj_override);
	}

	private CraftItemGUI GetLastCraftedItem()
	{
		return null;
	}

	private void ResetScroll()
	{
		scroll_view.Scroll(0f);
		scroll_view.currentMomentum = Vector3.zero;
		UpdateAllAnchors();
		scroll_view.UpdatePosition();
	}

	public override void Hide(bool play_hide_sound = true)
	{
		if (DOTween.IsTweening(scroll_view.transform))
		{
			scroll_view.transform.DOKill();
		}
		scroll_view.StopScrolling();
		ClearList();
		base.Hide(play_hide_sound);
		if (GUIElements.me.mixed_craft_tabbed != null && GUIElements.me.mixed_craft_tabbed.is_shown)
		{
			GUIElements.me.mixed_craft_tabbed.Hide(play_hide_sound: false);
		}
		if (hide_after_build_gui)
		{
			MainGame.paused = false;
			hide_after_build_gui = false;
		}
		ON_CRAFT_GUI_HIDE?.Invoke();
		ON_CRAFT_GUI_HIDE = null;
	}

	public override void OnAboveWindowClosed()
	{
		if (BaseGUI.for_gamepad)
		{
			if (CraftItemGUI.current_overed != null)
			{
				base.button_tips.Activate();
				CraftItemGUI.current_overed.OnAboveWindowClosed();
			}
			else
			{
				base.gamepad_controller.Enable(GamepadNavigationController.OpenMethod.GetAllAndFocus);
			}
		}
	}

	public void ExpandItem(CraftItemGUI craft_item_gui)
	{
		Debug.Log("ExpandItem", craft_item_gui);
		craft_item_gui.full_detailed_view = true;
		craft_item_gui.Redraw();
		GamepadNavigationItem[] componentsInChildren = craft_item_gui.GetComponentsInChildren<GamepadNavigationItem>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].active = true;
		}
		_exanded_item = craft_item_gui;
		if (BaseGUI.for_gamepad)
		{
			foreach (CraftItemGUI list_item in list_items)
			{
				list_item.gamepad_navigation_item.active = false;
				list_item.GetComponent<UIWidget>().alpha = ((list_item == craft_item_gui) ? 1f : 0.5f);
			}
			base.gamepad_controller.Enable(GamepadNavigationController.OpenMethod.GetAll);
			base.gamepad_controller.SetFocusedItem(_exanded_item.ingredients_table.GetComponentsInChildren<BaseItemCellGUI>()[0].GetComponent<GamepadNavigationItem>());
			_exanded_item.selection_frame.gameObject.SetActive(value: true);
		}
		else
		{
			_exanded_item.selection_frame.gameObject.SetActive(value: false);
		}
		UpdateAllAnchors();
		craft_list_grid.Reposition();
	}

	protected override bool OnPressedBack()
	{
		if (_gamepad_in_queue_area)
		{
			_gamepad_in_queue_area = false;
			RedrawQueueAreaFocus();
			return true;
		}
		if (_exanded_item != null)
		{
			CollapseItem();
			return true;
		}
		OnClosePressed();
		return true;
	}

	public void ExitFromQueueArea()
	{
		_gamepad_in_queue_area = false;
		RedrawQueueAreaFocus();
	}

	protected override bool OnPressedOption1()
	{
		if (queue == null || queue.IsEmpty())
		{
			return false;
		}
		_gamepad_in_queue_area = !_gamepad_in_queue_area;
		RedrawQueueAreaFocus();
		return true;
	}

	protected void RedrawQueueAreaFocus()
	{
		if (_gamepad_in_queue_area != _gamepad_was_in_queue_area)
		{
			in_redraw_mode = true;
			if (_gamepad_was_in_queue_area)
			{
				CraftQueueItemGUI.current_over?.ForceRemoveSelectionFrame();
			}
			_gamepad_was_in_queue_area = _gamepad_in_queue_area;
			base.gamepad_controller.RestoreSelection(_gamepad_in_queue_area ? 5 : 0);
			in_redraw_mode = false;
			queue.gamepad_hint.gameObject.SetActive(BaseGUI.for_gamepad && !_gamepad_in_queue_area);
			gamepad_hint_queue_big.text = (_gamepad_was_in_queue_area ? GameKeyTip.Get(GameKey.Back, "Back") : "");
		}
	}

	public void CollapseItem(CraftItemGUI specific_item = null)
	{
		CraftItemGUI craftItemGUI = ((specific_item == null) ? _exanded_item : specific_item);
		Debug.Log("CollapseItem", craftItemGUI);
		if (craftItemGUI == null)
		{
			Debug.LogError("Can't collapse item, _exanded_item is null");
			return;
		}
		craftItemGUI.full_detailed_view = false;
		craftItemGUI.Redraw();
		if (BaseGUI.for_gamepad)
		{
			GamepadNavigationItem[] componentsInChildren = craftItemGUI.GetComponentsInChildren<GamepadNavigationItem>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].active = false;
			}
			foreach (CraftItemGUI list_item in list_items)
			{
				list_item.gamepad_navigation_item.active = true;
				list_item.GetComponent<UIWidget>().alpha = 1f;
			}
			base.gamepad_controller.Enable(GamepadNavigationController.OpenMethod.GetAll);
			base.gamepad_controller.SetFocusedItem(craftItemGUI.gamepad_navigation_item);
		}
		_exanded_item = null;
		UpdateAllAnchors();
		craft_list_grid.Reposition();
	}

	public override void OnClosePressed()
	{
		if (GlobalCraftControlGUI.is_global_control_active)
		{
			GUIElements.me.global_craft_control_gui.Open();
		}
		base.OnClosePressed();
	}

	private void ClearList()
	{
		foreach (CraftItemGUI list_item in list_items)
		{
			list_item.gameObject.SetActive(value: false);
			UnityEngine.Object.Destroy(list_item.gameObject);
		}
		list_items.Clear();
		craft_list_grid.Reposition();
	}

	private new void LateUpdate()
	{
		if (!(scroll_view == null) && scroll_view.RestrictWithinBounds(instant: false) && DOTween.IsTweening(scroll_view.transform))
		{
			scroll_view.transform.DOKill();
		}
	}

	public List<CraftItemGUI> GetItemsList()
	{
		return list_items;
	}

	public new void OnRightClick()
	{
		base.OnRightClick();
	}

	protected override bool CanCloseWithRightClick()
	{
		return true;
	}

	protected override bool OnPressedNextTab()
	{
		if (_gamepad_in_queue_area)
		{
			return false;
		}
		if (_tabs_ids.Count < 2)
		{
			return false;
		}
		if (craftery_wgo == null)
		{
			Debug.LogError("Craftery wgo is null");
			return false;
		}
		int num = _tabs_ids.IndexOf(craftery_wgo.last_opened_tab);
		if (num == -1)
		{
			num = 0;
		}
		if (++num >= _tabs_ids.Count)
		{
			num = 0;
		}
		SwitchTab(_tabs_ids[num]);
		return true;
	}

	protected override bool OnPressedPrevTab()
	{
		if (_gamepad_in_queue_area)
		{
			return false;
		}
		if (_tabs_ids.Count < 2)
		{
			return false;
		}
		if (craftery_wgo == null)
		{
			Debug.LogError("Craftery wgo is null");
			return false;
		}
		int num = _tabs_ids.IndexOf(craftery_wgo.last_opened_tab);
		if (num == -1)
		{
			num = 0;
		}
		if (--num < 0)
		{
			num = _tabs_ids.Count - 1;
		}
		SwitchTab(_tabs_ids[num]);
		return true;
	}

	public bool PressPrevTab()
	{
		return OnPressedPrevTab();
	}

	public bool PressNextTab()
	{
		return OnPressedNextTab();
	}

	protected override bool OnPressedLeft()
	{
		if (_gamepad_in_queue_area)
		{
			CraftQueueItemGUI.current_over?.OnDecreasePressed();
			return true;
		}
		GamepadNavigationItem focused_item = base.gamepad_controller.focused_item;
		CraftItemGUI craftItemGUI = ((focused_item == null) ? null : focused_item.GetComponent<CraftItemGUI>());
		if (craftItemGUI != null && craftItemGUI.amount_buttons != null && craftItemGUI.amount_buttons.activeSelf)
		{
			craftItemGUI.OnAmountMinus();
			return true;
		}
		return base.OnPressedLeft();
	}

	protected override bool OnPressedRight()
	{
		if (_gamepad_in_queue_area)
		{
			CraftQueueItemGUI.current_over?.OnIncreasePressed();
			return true;
		}
		GamepadNavigationItem focused_item = base.gamepad_controller.focused_item;
		CraftItemGUI craftItemGUI = ((focused_item == null) ? null : focused_item.GetComponent<CraftItemGUI>());
		if (craftItemGUI != null && craftItemGUI.amount_buttons != null && craftItemGUI.amount_buttons.activeSelf)
		{
			craftItemGUI.OnAmountPlus();
			return true;
		}
		return base.OnPressedRight();
	}

	protected override bool OnPressedDown()
	{
		GamepadNavigationItem focused_item = base.gamepad_controller.focused_item;
		CraftItemGUI craftItemGUI = ((focused_item == null) ? null : focused_item.GetComponentInParent<CraftItemGUI>());
		if (craftItemGUI != null && craftItemGUI.ProcessIngredientStep(-1))
		{
			return true;
		}
		return base.OnPressedDown();
	}

	protected override bool OnPressedUp()
	{
		GamepadNavigationItem focused_item = base.gamepad_controller.focused_item;
		CraftItemGUI craftItemGUI = ((focused_item == null) ? null : focused_item.GetComponentInParent<CraftItemGUI>());
		if (craftItemGUI != null && craftItemGUI.ProcessIngredientStep(1))
		{
			return true;
		}
		return base.OnPressedUp();
	}

	protected override bool OnPressedOption2()
	{
		if (_gamepad_in_queue_area)
		{
			CraftQueueItemGUI.current_over?.OnInfinityButtonPressed();
			return true;
		}
		GamepadNavigationItem focused_item = base.gamepad_controller.focused_item;
		CraftItemGUI craftItemGUI = ((focused_item == null) ? null : focused_item.GetComponent<CraftItemGUI>());
		if (craftItemGUI != null && !craftItemGUI.current_craft.IsMultiqualityOutput())
		{
			GamepadNavigationItem focused = base.gamepad_controller.focused_item;
			GUIElements.me.tech_dialog.OpenAsItemsList(GJL.L("craft_recipe_hint", GJL.L(craftItemGUI.current_craft.GetNameNonLocalized())), craftItemGUI.current_craft.needs, delegate
			{
				Debug.Log("Back to craft gui");
				if (BaseGUI.for_gamepad)
				{
					base.gamepad_controller.Enable();
					base.gamepad_controller.ReinitItems(focus_on_first_active: false);
					base.gamepad_controller.SetFocusedItem(focused);
					base.gamepad_controller.RememberFocused(focused);
				}
			});
		}
		return base.OnPressedOption2();
	}

	public static void NeedToCancelCraftsDialog(CraftComponent craft, Action on_confirmed)
	{
		GUIElements.me.dialog.Open("need_cancel_craft", "OK", delegate
		{
			craft.craft_queue.Clear();
			craft.Cancel();
			on_confirmed();
			RestoreFocusAfterDialogWindow();
		}, "Cancel");
	}

	public static void RestoreFocusAfterDialogWindow()
	{
		if (BaseGUI.for_gamepad && !(_current_instance == null) && _current_instance.is_shown)
		{
			_current_instance.gamepad_controller.Enable();
			_current_instance.gamepad_controller.ReinitItems(focus_on_first_active: true);
			if (CraftItemGUI.last_item_pressed != null)
			{
				_current_instance.gamepad_controller.SetFocusedItem(CraftItemGUI.last_item_pressed.GetComponent<GamepadNavigationItem>());
			}
		}
	}

	protected void SetCustomDirectionForFirstAndLast(string only_tab = null)
	{
		if (_tabs_ids.Count == 0)
		{
			CraftItemGUI craftItemGUI = null;
			CraftItemGUI craftItemGUI2 = null;
			foreach (CraftItemGUI list_item in list_items)
			{
				if (craftItemGUI == null)
				{
					craftItemGUI = list_item;
				}
				craftItemGUI2 = list_item;
			}
			if (craftItemGUI != null && craftItemGUI2 != null)
			{
				craftItemGUI.gamepad_navigation_item.SetCustomDirectionItem(craftItemGUI2.gamepad_navigation_item, Direction.Up);
				craftItemGUI2.gamepad_navigation_item.SetCustomDirectionItem(craftItemGUI.gamepad_navigation_item, Direction.Down);
			}
			return;
		}
		if (!string.IsNullOrEmpty(only_tab) && _tabs_ids.Contains(only_tab))
		{
			CraftItemGUI craftItemGUI3 = null;
			CraftItemGUI craftItemGUI4 = null;
			CraftItemGUI craftItemGUI5 = null;
			foreach (CraftItemGUI list_item2 in list_items)
			{
				if (list_item2.craft_definition.tab_id == only_tab || (_current_instance.craftery_wgo.obj_def.interaction_type == ObjectDefinition.InteractionType.Builder && list_item2.craft_definition.id == "_remove_"))
				{
					if (craftItemGUI3 == null)
					{
						craftItemGUI3 = list_item2;
					}
					if (craftItemGUI4 != null)
					{
						craftItemGUI5 = craftItemGUI4;
					}
					craftItemGUI4 = list_item2;
				}
			}
			if (craftItemGUI3 != null && craftItemGUI4 != null)
			{
				craftItemGUI3.gamepad_navigation_item.SetCustomDirectionItem(craftItemGUI4.gamepad_navigation_item, Direction.Up);
				craftItemGUI4.gamepad_navigation_item.SetCustomDirectionItem(craftItemGUI3.gamepad_navigation_item, Direction.Down);
				if (craftItemGUI5 != null)
				{
					craftItemGUI5.gamepad_navigation_item.SetCustomDirectionItem(craftItemGUI4.gamepad_navigation_item, Direction.Down);
				}
			}
			return;
		}
		foreach (string tabs_id in _tabs_ids)
		{
			CraftItemGUI craftItemGUI6 = null;
			CraftItemGUI craftItemGUI7 = null;
			foreach (CraftItemGUI list_item3 in list_items)
			{
				if (list_item3.craft_definition.tab_id == tabs_id)
				{
					if (craftItemGUI6 == null)
					{
						craftItemGUI6 = list_item3;
					}
					craftItemGUI7 = list_item3;
				}
			}
			if (craftItemGUI6 != null && craftItemGUI7 != null)
			{
				craftItemGUI6.gamepad_navigation_item.SetCustomDirectionItem(craftItemGUI7.gamepad_navigation_item, Direction.Up);
				craftItemGUI7.gamepad_navigation_item.SetCustomDirectionItem(craftItemGUI6.gamepad_navigation_item, Direction.Down);
			}
		}
	}
}
