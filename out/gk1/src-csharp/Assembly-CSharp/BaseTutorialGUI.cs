using System.Collections.Generic;
using UnityEngine;

public class BaseTutorialGUI : BaseGUI
{
	protected TutorialItemGUI[] items;

	protected UITable ui_table;

	protected SimpleUITable simple_ui_table;

	protected string back_btn_text = "back";

	[Tooltip("gui elemets which should be hidden when any window is opened above this window")]
	public List<GameObject> hideable_controls;

	protected List<bool> controlls_states = new List<bool>();

	public override void Init()
	{
		items = GetComponentsInChildren<TutorialItemGUI>(includeInactive: true);
		TutorialItemGUI[] array = items;
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
		TutorialItemGUI tutorialItemGUI = null;
		TutorialItemGUI tutorialItemGUI2 = null;
		TutorialItemGUI[] array = items;
		foreach (TutorialItemGUI tutorialItemGUI3 in array)
		{
			tutorialItemGUI3.Show();
			if (tutorialItemGUI3.gameObject.activeSelf)
			{
				if (tutorialItemGUI == null)
				{
					tutorialItemGUI = tutorialItemGUI3;
				}
				tutorialItemGUI2 = tutorialItemGUI3;
			}
		}
		if (tutorialItemGUI != null && tutorialItemGUI2 != null)
		{
			tutorialItemGUI.gamepad_item.SetCustomDirectionItem(tutorialItemGUI2.gamepad_item, Direction.Up);
			tutorialItemGUI2.gamepad_item.SetCustomDirectionItem(tutorialItemGUI.gamepad_item, Direction.Down);
		}
		array = items;
		foreach (TutorialItemGUI tutorialItemGUI4 in array)
		{
			if (!(tutorialItemGUI4.gamepad_frame == null))
			{
				tutorialItemGUI4.gamepad_frame.Deactivate();
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
		TutorialItemGUI[] array = items;
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
