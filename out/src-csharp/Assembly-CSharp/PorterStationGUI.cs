using System.Collections.Generic;
using UnityEngine;

public class PorterStationGUI : BaseGameGUI
{
	public BodyPanelGUI body_panel;

	public InventoryWidget inventory_widget;

	public UniversalObjectInfoGUI universal_info;

	public UIScrollView scroll_view;

	private PorterStation _station;

	private WorldGameObject _wgo;

	public override void Init()
	{
		base.Init();
		body_panel.skull_bar.on_enable_skulls_frame += OnSkullsOver;
		body_panel.skull_bar.on_disable_skulls_frame += OnSkullsOut;
	}

	public void Open(WorldGameObject porter_station_wgo)
	{
		base.Open();
		universal_info.Draw(porter_station_wgo.GetUniversalObjectInfo());
		_station = porter_station_wgo.porter_station;
		_wgo = porter_station_wgo;
		Item item = new Item
		{
			inventory_size = 999
		};
		WorldZone myWorldZone = porter_station_wgo.GetMyWorldZone();
		if (myWorldZone == null)
		{
			Debug.LogError("World zone is null for porter station", porter_station_wgo);
			return;
		}
		List<string> list = new List<string>();
		foreach (TransportPathsDefinition transport_path in GameBalance.me.transport_paths)
		{
			if (!(transport_path.source_zone_id != myWorldZone.id) && !(transport_path.station_wgo_id != porter_station_wgo.obj_id) && !(transport_path.destination_zone_id != porter_station_wgo.porter_station?.destination?.id))
			{
				list = transport_path.transport_items;
			}
		}
		foreach (string item2 in list)
		{
			item.inventory.Add(new Item(item2, 1));
			if (GameBalance.me.GetData<ItemDefinition>(item2).is_big)
			{
				item.inventory.Add(new Item());
			}
		}
		foreach (Item item3 in item.inventory)
		{
			item3.value = ((!_station.blacklist.Contains(item3.id)) ? 1 : 2);
		}
		item.inventory_size = item.inventory.Count;
		Inventory inventory = new Inventory(item, "transfer_items");
		inventory_widget.Open(inventory, BaseGUI.for_gamepad, 0, 0, dont_show_empty_rows: true);
		BaseItemCellGUI[] componentsInChildren = inventory_widget.gameObject.GetComponentsInChildren<BaseItemCellGUI>();
		foreach (BaseItemCellGUI baseItemCellGUI in componentsInChildren)
		{
			baseItemCellGUI.SetCallbacks(delegate
			{
				base.button_tips.Print(GameKeyTip.Select(), GameKeyTip.Close());
			}, null, OnItemClicked);
			RedrawItemSelectionState(baseItemCellGUI);
			if (baseItemCellGUI?.container?.container == null || !baseItemCellGUI.container.container.activeInHierarchy)
			{
				GamepadNavigationItem component = baseItemCellGUI.GetComponent<GamepadNavigationItem>();
				if (component != null)
				{
					Object.Destroy(component);
				}
			}
		}
		scroll_view.ResetPosition();
		body_panel.DrawWorker(porter_station_wgo.linked_worker);
		if (!porter_station_wgo.has_linked_worker)
		{
			body_panel.spr_body.enabled = false;
		}
		if (_station.HasLinkedWorker() && _station.state == PorterStation.PorterState.Waiting)
		{
			if (BaseGUI.for_gamepad)
			{
				body_panel.btn_remove_body.GetComponent<GamepadNavigationItem>().SetCallbacks(null, null, TakeWorker);
			}
		}
		else
		{
			body_panel.btn_remove_body.SetActive(value: false);
		}
		MainGame.SetPausedMode(is_paused: true);
		if (BaseGUI.for_gamepad)
		{
			base.gamepad_controller.ReinitItems(focus_on_first_active: true);
		}
	}

	private void OnItemClicked(BaseItemCellGUI itm)
	{
		if (++itm.item.value > 2)
		{
			itm.item.value = 1;
		}
		RedrawItemSelectionState(itm);
	}

	private void RedrawItemSelectionState(BaseItemCellGUI itm)
	{
		itm.container.counter.text = ((itm.item.value == 1) ? "(check)" : "");
	}

	public override void Hide(bool play_hide_sound = true)
	{
		inventory_widget.ClearItems();
		MainGame.SetPausedMode(is_paused: false);
		base.Hide(play_hide_sound);
	}

	protected override bool OnPressedBack()
	{
		OnClosePressed();
		return true;
	}

	public override void OnClosePressed()
	{
		if (_station == null)
		{
			Debug.LogError("PorterStationGUI.OnClosePressed error: _station is null!");
		}
		else
		{
			List<string> list = new List<string>();
			if (inventory_widget?.inventory_data?.inventory == null)
			{
				Debug.LogError("PorterStationGUI.OnClosePressed error: inventory is null!");
			}
			else
			{
				foreach (Item item in inventory_widget.inventory_data.inventory)
				{
					if (item.value != 1)
					{
						list.Add(item.id);
					}
				}
			}
			_station.blacklist = list;
		}
		Hide();
	}

	public void TakeWorker()
	{
		if (!_wgo.has_linked_worker)
		{
			Debug.LogError("TakeWorker error: WGO has not linked worker!");
			return;
		}
		WorldGameObject linked_worker = _wgo.linked_worker;
		if (linked_worker == null)
		{
			Debug.LogError("TakeWorker error: worker_wgo is null!");
			return;
		}
		WorldMap.RemoveZombieWorkerToStock(linked_worker);
		Item overheadItem = linked_worker.worker.GetOverheadItem();
		if (MainGame.me.player_char.has_overhead)
		{
			MainGame.me.player_char.DropOverheadItem();
		}
		MainGame.me.player_char.SetOverheadItem(overheadItem);
		OnClosePressed();
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
