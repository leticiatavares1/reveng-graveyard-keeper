using UnityEngine;

public class TutorialWindowsGUI : BaseTutorialGUI
{
	private GamepadNavigationItem _stored_focus;

	public override void Open()
	{
		base.Open();
		if (BaseGUI.for_gamepad)
		{
			base.gamepad_controller.ReinitItems(focus_on_first_active: false);
			base.gamepad_controller.RestoreFocus();
			if (_stored_focus != null)
			{
				base.gamepad_controller.SetFocusedItem(_stored_focus);
			}
			_stored_focus = null;
		}
	}

	public override void OnAboveWindowClosed()
	{
		Open();
	}

	protected override bool OnPressedBack()
	{
		OnClosePressed();
		return true;
	}

	public void OnPressedTutorialWindowOpen()
	{
		if (BaseGUI.for_gamepad)
		{
			_stored_focus = base.gamepad_controller.focused_item;
		}
		base.gameObject.Deactivate();
	}

	public void OnBackBtnClicked()
	{
		base.OnClosePressed();
	}

	public void RemoveNotAvailableTutorialItem(TutorialItemGUI tutorial_item)
	{
		TutorialItemGUI[] array = items;
		items = new TutorialItemGUI[items.Length - 1];
		int num = 0;
		for (int i = 0; i < array.Length; i++)
		{
			if (!(array[i] == tutorial_item))
			{
				items[num] = array[i];
				num++;
			}
		}
		Object.Destroy(tutorial_item.gameObject);
	}
}
