using System.Collections.Generic;
using UnityEngine;

public class TimeMachineGUI : BaseGUI
{
	private const string CUTSCENE_NUM_KEY = "cutscene_dlc_num";

	[SerializeField]
	private TimeMachineItemGUI _time_machine_item;

	private List<TimeMachineItemGUI> _items;

	private bool _is_data_initialized;

	private const string _TIME_MACHINE_CUSTOM_TAG = "tavern_time_machine";

	private const string _TIME_MACHINE_ACT_ANIM = "activate_long";

	private const string _TIME_MACHINE_DEACT_ANIM = "deactivate_long";

	public List<TimeMachineItemGUI> items => _items;

	public override void Open()
	{
		base.Open();
		if (!_is_data_initialized)
		{
			InitData();
		}
		List<CutscenesDLCDefinition> cutscenes_data = GameBalance.me.cutscenes_data;
		int paramInt = MainGame.me.player.data.GetParamInt("cutscene_dlc_num");
		string value = "dlc_cutscene_" + paramInt;
		for (int i = 0; i < cutscenes_data.Count; i++)
		{
			_items[i].CheckEnabled(is_enabled: false);
		}
		if (paramInt != 0)
		{
			bool flag = false;
			for (int j = 0; j < cutscenes_data.Count; j++)
			{
				if (cutscenes_data[j].id.Equals(value))
				{
					flag = true;
					_items[j].CheckEnabled(is_enabled: true);
				}
				else
				{
					_items[j].CheckEnabled(!flag);
				}
			}
		}
		if (BaseGUI.for_gamepad)
		{
			SetCustomDirectionForFirstAndLast();
			base.gamepad_controller.ReinitItems(focus_on_first_active: true);
		}
	}

	public override void Hide(bool play_hide_sound = true)
	{
		base.Hide(play_hide_sound);
		if (_items == null)
		{
			return;
		}
		foreach (TimeMachineItemGUI item in _items)
		{
			item.OnMouseOuted();
		}
	}

	private void InitData()
	{
		_items = new List<TimeMachineItemGUI>();
		_items.Add(_time_machine_item);
		List<CutscenesDLCDefinition> cutscenes_data = GameBalance.me.cutscenes_data;
		for (int i = 1; i < cutscenes_data.Count; i++)
		{
			TimeMachineItemGUI timeMachineItemGUI = _time_machine_item.Copy();
			_items.Add(timeMachineItemGUI);
			_ = cutscenes_data[i];
			timeMachineItemGUI.Initialize(cutscenes_data[i].id, cutscenes_data[i].GetIconName(), cutscenes_data[i].flow_script, OnCutscenePicked);
		}
		_time_machine_item.Initialize(cutscenes_data[0].id, cutscenes_data[0].GetIconName(), cutscenes_data[0].flow_script, OnCutscenePicked);
		_is_data_initialized = true;
	}

	private void OnCutscenePicked(string flowscript)
	{
		Debug.LogWarning("OnCutscenePicked: " + flowscript);
		Hide();
		WorldGameObject tavern_machine_wgo = WorldMap.GetWorldGameObjectByCustomTag("tavern_time_machine");
		if (tavern_machine_wgo != null)
		{
			GS.SetPlayerEnable(player_enabled: false, affect_cinematic: false);
			tavern_machine_wgo.TriggerSmartAnimation("activate_long");
			ChunkedGameObject ch_obj = tavern_machine_wgo.GetComponent<ChunkedGameObject>();
			ch_obj.always_active = true;
			GJTimer.AddTimer(2.5f, delegate
			{
				CutsceneManager.ExecuteCutscene(flowscript, delegate
				{
					ch_obj.always_active = false;
					tavern_machine_wgo.TriggerSmartAnimation("deactivate_long");
				});
			});
		}
		else
		{
			CutsceneManager.ExecuteCutscene(flowscript);
		}
	}

	protected override bool OnPressedBack()
	{
		GUIElements.me.time_machine_gui.Hide();
		return true;
	}

	public override void OnClosePressed()
	{
		Hide();
	}

	private void SetCustomDirectionForFirstAndLast()
	{
		TimeMachineItemGUI timeMachineItemGUI = null;
		TimeMachineItemGUI timeMachineItemGUI2 = null;
		foreach (TimeMachineItemGUI item in items)
		{
			if (timeMachineItemGUI == null)
			{
				timeMachineItemGUI = item;
			}
			timeMachineItemGUI2 = item;
		}
		if (timeMachineItemGUI != null && timeMachineItemGUI2 != null)
		{
			timeMachineItemGUI.gamepad_navigation_item.SetCustomDirectionItem(timeMachineItemGUI2.gamepad_navigation_item, Direction.Up);
			timeMachineItemGUI2.gamepad_navigation_item.SetCustomDirectionItem(timeMachineItemGUI.gamepad_navigation_item, Direction.Down);
		}
	}
}
