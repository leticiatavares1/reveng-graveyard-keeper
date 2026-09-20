using System;
using System.Collections.Generic;
using UnityEngine;

public class ControlsGUI : MonoBehaviour
{
	public UILabel control_move;

	public UILabel control_interact;

	public UILabel control_work;

	public UILabel control_atk;

	public UILabel control_dash;

	public UILabel control_gmenu;

	public UILabel control_pause;

	public UILabel control_qslot;

	public UILabel control_tab;

	public UILabel control_map;

	public UIGrid grid;

	public int grid_height_for_keyboard = 15;

	public int grid_height_for_gamepad = 20;

	public int grid_pos_y_for_keyboard = -134;

	public int grid_pos_y_for_gamepad = -147;

	[NonSerialized]
	public bool just_opened;

	private List<ControlKeyLineGUI> redefine_key_item = new List<ControlKeyLineGUI>();

	public void OnEnable()
	{
		PlatformDependentElement[] componentsInChildren = GetComponentsInChildren<PlatformDependentElement>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Init(LazyInput.gamepad_active);
		}
		if (grid != null)
		{
			grid.cellHeight = (LazyInput.gamepad_active ? grid_height_for_gamepad : grid_height_for_keyboard);
			grid.transform.localPosition = new Vector3(grid.transform.localPosition.x, LazyInput.gamepad_active ? grid_pos_y_for_gamepad : grid_pos_y_for_keyboard);
			control_move.text = GameKeyTip.GetIcon(GameKey.Move);
			control_interact.text = GameKeyTip.GetIcon(GameKey.Interaction);
			control_work.text = GameKeyTip.GetIcon(GameKey.Work);
			control_atk.text = GameKeyTip.GetIcon(GameKey.Attack);
			control_dash.text = GameKeyTip.GetIcon(GameKey.Dash);
			if (LazyInput.gamepad_active)
			{
				UILabel uILabel = control_dash;
				uILabel.text = uILabel.text + " / " + GameKeyTip.GetIcon(GameKey.Dash2);
			}
			control_gmenu.text = GameKeyTip.GetIcon(GameKey.GameGUI);
			control_tab.text = "I, T, N";
			control_map.text = GameKeyTip.GetIcon(GameKey.Map);
			control_pause.text = GameKeyTip.GetIcon(GameKey.IngameMenu);
			control_qslot.text = GameKeyTip.GetIcon(GameKey.AnyQuickslot);
			grid.Reposition();
			grid.repositionNow = true;
		}
		redefine_key_item.Clear();
		ControlKeyLineGUI[] componentsInChildren2 = base.gameObject.GetComponentsInChildren<ControlKeyLineGUI>(includeInactive: true);
		foreach (ControlKeyLineGUI controlKeyLineGUI in componentsInChildren2)
		{
			controlKeyLineGUI.Redraw();
			controlKeyLineGUI.changed = false;
			redefine_key_item.Add(controlKeyLineGUI);
		}
	}

	public void OnDisable()
	{
		just_opened = false;
		bool flag = false;
		foreach (ControlKeyLineGUI item in redefine_key_item)
		{
			flag |= item.changed;
		}
		if (flag)
		{
			GameSettings.Save();
		}
	}

	public void ResetKeyBindings()
	{
		KeyBindings.Reset();
		foreach (ControlKeyLineGUI item in redefine_key_item)
		{
			item.Redraw();
		}
	}
}
