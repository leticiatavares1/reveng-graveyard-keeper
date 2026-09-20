using DLCRefugees;
using UnityEngine;

public class InGameMenuGUI : BaseMenuGUI
{
	public UILabel label_save_and_exit;

	private GamepadNavigationItem _stored_focus;

	public override void Open()
	{
		base.Open();
		MainGame.SetPausedMode(is_paused: true);
		SmartAudioEngine.me.SetDullMusicMode();
		PlatformSpecific.SetGameStatus(GameEvents.GameStatus.InMenu);
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

	public void OnPressedContinue()
	{
		OnClosePressed();
		_stored_focus = null;
	}

	public void OnPressedRestart()
	{
		Hide();
		MainGame.me.RestartDemoBuild();
	}

	public void OnPressedOptions()
	{
		if (BaseGUI.for_gamepad)
		{
			_stored_focus = base.gamepad_controller.focused_item;
		}
		base.gameObject.Deactivate();
		GUIElements.me.options.Open();
	}

	public void OnPressedSaveAndExit()
	{
		_stored_focus = null;
		SetControllsActive(active: false);
		OnClosePressed();
		GUIElements.me.dialog.OpenYesNo(GJL.L("exit_menu_confirm_txt") + "\n\n" + GJL.L("exit_menu_confirm"), delegate
		{
			LoadingGUI.Show(delegate
			{
				ReturnToMainMenu();
			});
		}, null, delegate
		{
			SetControllsActive(active: true);
		});
	}

	public override void OnAboveWindowClosed()
	{
		Open();
	}

	public override void OnClosePressed()
	{
		_stored_focus = null;
		base.OnClosePressed();
		SmartAudioEngine.me.SetDullMusicMode(dull_mode: false);
	}

	public override void Hide(bool play_sound = true)
	{
		MainGame.SetPausedMode(is_paused: false);
		base.Hide(play_sound);
		PlatformSpecific.SetGameStatus(GameEvents.GameStatus.InGame);
	}

	protected override bool OnPressedBack()
	{
		OnClosePressed();
		return true;
	}

	public void ReturnToMainMenu()
	{
		_stored_focus = null;
		Debug.Log("ReturnToMainMenu");
		GUIElements.me.saves.StopPlayingGame();
		GUIElements.me.relation.Hide();
		GUIElements.me.main_menu.Open();
		LoadingGUI.Hide();
		RefugeesCampEngine.instance.DeInit();
	}

	public void OpenControlsHelpWindow()
	{
		if (BaseGUI.for_gamepad)
		{
			_stored_focus = base.gamepad_controller.focused_item;
		}
		base.gameObject.Deactivate();
		GUIElements.me.tutorial.Open("controls");
		MainGame.SetPausedMode(is_paused: true);
	}

	public void OpenTutorialsWindow()
	{
		if (BaseGUI.for_gamepad)
		{
			_stored_focus = base.gamepad_controller.focused_item;
		}
		base.gameObject.Deactivate();
		GUIElements.me.tutorial_windows_gui.Open();
	}
}
