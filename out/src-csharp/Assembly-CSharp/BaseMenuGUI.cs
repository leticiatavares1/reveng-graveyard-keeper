using System.Collections.Generic;
using UnityEngine;

public class BaseMenuGUI : BaseGUI
{
	protected MenuItemGUI[] items;

	protected UITable ui_table;

	protected SimpleUITable simple_ui_table;

	protected string back_btn_text = "back";

	[Tooltip("gui elemets which should be hidden when any window is opened above this window")]
	public List<GameObject> hideable_controls;

	protected List<bool> controlls_states = new List<bool>();

	public override void Init()
	{
		items = GetComponentsInChildren<MenuItemGUI>(includeInactive: true);
		MenuItemGUI[] array = items;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Init(this);
		}
		ui_table = GetComponentInChildren<UITable>(includeInactive: true);
		simple_ui_table = GetComponentInChildren<SimpleUITable>(includeInactive: true);
		base.Init();
	}

	public override void Open()
	{
		base.Open();
		MenuItemGUI menuItemGUI = null;
		MenuItemGUI menuItemGUI2 = null;
		MenuItemGUI[] array = items;
		foreach (MenuItemGUI menuItemGUI3 in array)
		{
			menuItemGUI3.Show();
			if (menuItemGUI3.gameObject.activeSelf)
			{
				if (menuItemGUI == null)
				{
					menuItemGUI = menuItemGUI3;
				}
				menuItemGUI2 = menuItemGUI3;
			}
		}
		if (menuItemGUI != null && menuItemGUI2 != null)
		{
			menuItemGUI.gamepad_item.SetCustomDirectionItem(menuItemGUI2.gamepad_item, Direction.Up);
			menuItemGUI2.gamepad_item.SetCustomDirectionItem(menuItemGUI.gamepad_item, Direction.Down);
		}
		array = items;
		foreach (MenuItemGUI menuItemGUI4 in array)
		{
			if (!(menuItemGUI4.gamepad_frame == null))
			{
				menuItemGUI4.gamepad_frame.Deactivate();
			}
		}
		Reposition();
		UpdatePixelPerfect();
		if (base.is_shown && BaseGUI.for_gamepad)
		{
			base.gamepad_controller.ReinitItems(focus_on_first_active: true);
		}
	}

	public override void Hide(bool play_sound = true)
	{
		MenuItemGUI[] array = items;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].OnOut(animate: false);
		}
		Reposition();
		base.Hide(play_sound);
	}

	protected void Reposition()
	{
		if (simple_ui_table != null)
		{
			simple_ui_table.Reposition();
		}
		if (ui_table != null)
		{
			ui_table.Reposition();
		}
	}

	public virtual void UpdatTip(bool select_active)
	{
		if (BaseGUI.for_gamepad)
		{
			base.button_tips.Print(GameKeyTip.Select(), GameKeyTip.Back(back_btn_text));
		}
	}

	public void SetControllsActive(bool active)
	{
		hideable_controls.RemoveUnityNulls();
		if (active)
		{
			if (controlls_states.Count != 0)
			{
				for (int i = 0; i < hideable_controls.Count; i++)
				{
					hideable_controls[i].SetActive(controlls_states[i]);
				}
			}
			return;
		}
		controlls_states.Clear();
		foreach (GameObject hideable_control in hideable_controls)
		{
			controlls_states.Add(hideable_control.activeSelf);
			hideable_control.Deactivate();
		}
	}

	public override void OnAboveWindowClosed()
	{
		InitPlatformDependentStuff();
		if (base.is_shown && BaseGUI.for_gamepad)
		{
			base.gamepad_controller.ReinitItems(focus_on_first_active: false);
			base.gamepad_controller.RestoreFocus();
		}
	}
}
