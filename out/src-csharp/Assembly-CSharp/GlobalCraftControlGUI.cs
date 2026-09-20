using System;
using System.Collections.Generic;
using UnityEngine;

public class GlobalCraftControlGUI : BaseGUI
{
	public static bool is_global_control_active;

	public CraftControlItem craft_control_item_prefab;

	public UIScrollView scroll_view;

	public UITable list_grid;

	public UITable table_grid;

	public UniversalObjectInfoGUI object_info;

	[NonSerialized]
	public List<CraftControlItem> list_items = new List<CraftControlItem>();

	public UITableOrGrid tabs_table;

	public CraftTabGUI craft_tab_prefab;

	public static GlobalCraftControlGUI current_instance;

	public UIWidget header_height_widget;

	public GameObject tabs_go;

	public GameObject no_totem_go;

	public UILabel LB;

	public UILabel RB;

	public UILabel no_crafts;

	public CraftControlItem last_selected;

	private List<string> _tabs_ids = new List<string>();

	private List<CraftTabGUI> _tabs = new List<CraftTabGUI>();

	private string _zone_group;

	private string _cur_tab_id;

	private WorldGameObject builder;

	private List<WorldZone> _zones = new List<WorldZone>();

	public List<WorldZone> zones
	{
		get
		{
			_zones.Clear();
			for (int i = 0; i < GameBalance.me.world_zones_data.Count; i++)
			{
				WorldZone zoneByID = WorldZone.GetZoneByID(GameBalance.me.world_zones_data[i].id, null_is_error: false);
				if (!(zoneByID == null) && !string.IsNullOrEmpty(zoneByID.definition.zone_group) && zoneByID.definition.zone_group == _zone_group && !zoneByID.IsDisabled())
				{
					_zones.Add(zoneByID);
				}
			}
			return _zones;
		}
	}

	public override void Init()
	{
		craft_control_item_prefab.SetActive(active: false);
		craft_tab_prefab.SetActive(active: false);
		tabs_go.SetActive(value: false);
		base.Init();
	}

	public override void Update()
	{
		base.Update();
		if (!(builder == null))
		{
			UniversalObjectInfo universalObjectInfo = builder.GetUniversalObjectInfo();
			universalObjectInfo.header = GJL.L(builder.obj_id);
			universalObjectInfo.descr = "";
			if (builder.HasSoulsTotemInZone() && is_global_control_active && !universalObjectInfo.right_items.ContainsKey("gratitude_as_item"))
			{
				universalObjectInfo.right_items.Add("gratitude_as_item", (int)MainGame.me.player.gratitude_points);
			}
			object_info.Draw(universalObjectInfo);
		}
	}

	public void Open(string zone_group)
	{
		if (!base.is_shown && !base.isActiveAndEnabled)
		{
			current_instance = this;
			base.Open();
			is_global_control_active = true;
			_zone_group = zone_group;
			tabs_table.DestroyChildren(new CraftTabGUI[1] { craft_tab_prefab });
			RedrawTabs();
			if (!string.IsNullOrEmpty(_cur_tab_id) && _tabs_ids.Contains(_cur_tab_id))
			{
				SwitchTab(_cur_tab_id);
			}
			else if (_tabs_ids.Count > 0)
			{
				SwitchTab(_tabs_ids[0]);
			}
			else
			{
				Debug.Log("#GLOBAL CONTROL# No tabs for group:[" + zone_group + "]");
			}
			header_height_widget.height = ((_tabs_ids.Count > 0) ? 82 : 50);
			if (LB != null)
			{
				LB.text = GameKeyTip.GetIcon(GameKey.PrevTab);
			}
			if (RB != null)
			{
				RB.text = GameKeyTip.GetIcon(GameKey.NextTab);
			}
		}
	}

	public override void Open()
	{
		base.Open();
		base.gamepad_controller.ReinitItems(focus_on_first_active: false);
		if (last_selected != null)
		{
			base.gamepad_controller.SetFocusedItem(last_selected._gamepad_navigation_item);
		}
		else
		{
			base.gamepad_controller.SetFocusedItem(list_items[0]._gamepad_navigation_item);
		}
	}

	private void ResetScroll()
	{
		scroll_view.Scroll(0f);
		scroll_view.currentMomentum = Vector3.zero;
		UpdateAllAnchors();
		scroll_view.UpdatePosition();
	}

	private void RedrawTabs()
	{
		_tabs_ids.Clear();
		_tabs.Clear();
		List<WorldZone> list = zones;
		for (int i = 0; i < list.Count; i++)
		{
			if (!list[i].IsDisabled() && MainGame.me.save.IsWorldZoneKnown(list[i].id))
			{
				_tabs_ids.Add(list[i].id);
			}
		}
		tabs_go.SetActive(_tabs_ids.Count > 1);
		table_grid.Reposition();
		table_grid.repositionNow = true;
		for (int j = 0; j < _tabs_ids.Count; j++)
		{
			string tab_id = _tabs_ids[j];
			builder = list[j].GetZoneWGOs().Find((WorldGameObject w) => w.obj_def.interaction_type == ObjectDefinition.InteractionType.Builder);
			if (builder == null)
			{
				Debug.Log("#GLOBAL CONTROL# Null builder for zone:[" + list[j].id + "]");
				continue;
			}
			CraftTabGUI craftTabGUI = craft_tab_prefab.Copy();
			_tabs.Add(craftTabGUI);
			craftTabGUI.Draw(builder, tab_id, SwitchTab);
		}
	}

	private void SwitchTab(string tab_id)
	{
		ClearList();
		_cur_tab_id = tab_id;
		foreach (CraftTabGUI tab in _tabs)
		{
			tab.SetSelectedState(tab_id == tab.tab_id);
		}
		List<WorldGameObject> list = new List<WorldGameObject>();
		List<WorldZone> list2 = zones;
		WorldZone worldZone = null;
		for (int i = 0; i < list2.Count; i++)
		{
			if (!(list2[i].id == tab_id))
			{
				continue;
			}
			worldZone = list2[i];
			List<WorldGameObject> zoneWGOs = list2[i].GetZoneWGOs();
			for (int j = 0; j < zoneWGOs.Count; j++)
			{
				if ((zoneWGOs[j].obj_def.interaction_type == ObjectDefinition.InteractionType.Craft && zoneWGOs[j].obj_def.global_craft_control_access != ObjectDefinition.GlobalControlAccess.Ignore) || zoneWGOs[j].obj_def.global_craft_control_access == ObjectDefinition.GlobalControlAccess.ForceAdd)
				{
					list.Add(zoneWGOs[j]);
				}
			}
		}
		if (worldZone != null)
		{
			builder = worldZone.GetZoneWGOs().Find((WorldGameObject w) => w.obj_def.interaction_type == ObjectDefinition.InteractionType.Builder);
		}
		for (int k = 0; k < list.Count; k++)
		{
			CraftControlItem craftControlItem = craft_control_item_prefab.Copy();
			GJL.EnsureChildLabelsHasCorrectFont(craftControlItem.gameObject, do_cache: false);
			craftControlItem.Draw(list[k], builder.HasSoulsTotemInZone());
			list_items.Add(craftControlItem);
		}
		no_crafts.gameObject.SetActive(list.Count <= 0);
		scroll_view.transform.localPosition = new Vector3(scroll_view.transform.localPosition.x, 0f, 0f);
		UpdateAllAnchors();
		scroll_view.ResetPosition();
		Sounds.OnGUITabClick();
		if (worldZone != null)
		{
			if (builder == null)
			{
				return;
			}
			UniversalObjectInfo universalObjectInfo = builder.GetUniversalObjectInfo();
			universalObjectInfo.header = GJL.L(builder.obj_id);
			universalObjectInfo.descr = "";
			bool flag = builder.HasSoulsTotemInZone();
			if (flag && is_global_control_active && !universalObjectInfo.right_items.ContainsKey("gratitude_as_item"))
			{
				universalObjectInfo.right_items.Add("gratitude_as_item", (int)MainGame.me.player.gratitude_points);
			}
			object_info.Draw(universalObjectInfo);
			no_totem_go.SetActive(!flag);
		}
		else
		{
			Debug.Log("#GLOBAL CONTROL# Null builder for zone:[" + tab_id + "]");
		}
		UpdateAllAnchors();
		list_grid.Reposition();
		list_grid.repositionNow = true;
		Sounds.OnGUITabClick();
		if (BaseGUI.for_gamepad)
		{
			base.button_tips.PrintClose();
			base.gamepad_controller.ReinitItems(focus_on_first_active: false);
			if (list_items.Count > 0)
			{
				base.gamepad_controller.SetFocusedItem(list_items[0]._gamepad_navigation_item);
			}
		}
		else
		{
			ResetScroll();
		}
	}

	public ButtonTipsStr GetButtonTips()
	{
		return base.button_tips;
	}

	public new void UpdateAllAnchors()
	{
		BroadcastMessage("UpdateAnchors", SendMessageOptions.DontRequireReceiver);
		GetComponentInParent<UIPanel>().BroadcastMessage("UpdateAnchors", SendMessageOptions.DontRequireReceiver);
	}

	private void ClearList()
	{
		foreach (CraftControlItem list_item in list_items)
		{
			list_item.gameObject.SetActive(value: false);
			UnityEngine.Object.Destroy(list_item.gameObject);
		}
		list_items.Clear();
		list_grid.Reposition();
	}

	public override void OnClosePressed()
	{
		GUIElements.me.game_gui.Open();
		is_global_control_active = false;
		base.OnClosePressed();
	}

	protected override bool OnPressedBack()
	{
		OnClosePressed();
		return base.OnPressedBack();
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
		if (_tabs_ids.Count < 2)
		{
			return false;
		}
		int num = _tabs_ids.IndexOf(_cur_tab_id);
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
		if (_tabs_ids.Count < 2)
		{
			return false;
		}
		int num = _tabs_ids.IndexOf(_cur_tab_id);
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
}
