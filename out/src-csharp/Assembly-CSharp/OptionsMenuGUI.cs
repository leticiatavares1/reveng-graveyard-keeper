using System;
using System.Collections.Generic;
using LinqTools;
using UnityEngine;

public class OptionsMenuGUI : BaseMenuGUI
{
	private string[] _resolutions = new string[1] { "1920x1080" };

	private string[] _screen_modes = new string[3] { "Borderless", "Fullscreen", "Windowed" };

	private string[] _cursor_modes = new string[3] { "Default", "Hardware", "Software" };

	private string[] _mixer_light_modes = new string[2] { "disabled", "enabled" };

	private string[] _vsync_mode_modes = new string[2] { "vsync_off", "vsync_on" };

	public MenuItemGUI slider_master;

	public MenuItemGUI slider_music;

	public MenuItemGUI slider_sfx;

	public MenuItemGUI language_switcher;

	public MenuItemGUI resolution_switcher;

	public MenuItemGUI screen_mode_switcher;

	public MenuItemGUI cursor_mode_switcher;

	public MenuItemGUI mixer_light_switcher;

	public MenuItemGUI vsync_mode_switcher;

	public MenuItemGUI slider_speech;

	private int _screen_mode_index;

	private int _cursor_mode_index;

	private int _screen_res_index;

	private int _cur_screen_res_index;

	private int _vsync_mode_index;

	private const int REVERT_SETTINGS_TIME = 10;

	private float _revert_time_start;

	private bool _revert_dialog_closed_with_button;

	private GJTimer _revert_timer;

	public override void Open()
	{
		base.Open();
		string[] _temp_languages = new string[GJL.LANGUAGES.Length];
		List<string> list = GJL.AVAILABLE_LOCALES.ToList();
		int num = 0;
		for (int i = 0; i < _temp_languages.Length; i++)
		{
			string text = GJL.LANGUAGES[i];
			if (text == GameSettings.me.language)
			{
				num = i;
			}
			int num2 = list.IndexOf(text);
			if (num2 == -1)
			{
				Debug.LogError("Can't find a language name for: " + text);
			}
			else
			{
				text = GJL.AVAILABLE_LOCALE_NAMES[num2];
			}
			_temp_languages[i] = text;
		}
		GameSettings me = GameSettings.me;
		slider_master.SetupSlider(me.volume_master, 0, 100, delegate(int volume)
		{
			GameSettings.me.volume_master = volume;
			GameSettings.me.ApplyVolume();
		}, 10, 11);
		slider_music.SetupSlider(me.volume_music, 0, 100, delegate(int volume)
		{
			GameSettings.me.volume_music = volume;
			GameSettings.me.ApplyVolume();
		}, 10, 11);
		slider_sfx.SetupSlider(me.volume_sfx, 0, 100, delegate(int volume)
		{
			GameSettings.me.volume_sfx = volume;
			GameSettings.me.ApplyVolume();
		}, 10, 11);
		slider_speech.SetupSlider(me.volume_speech, 0, 100, delegate(int volume)
		{
			GameSettings.me.volume_speech = volume;
			GameSettings.me.ApplyVolume();
		}, 10, 11);
		Debug.Log("cur_lng_index = " + num);
		language_switcher.SetupOptions(num, _temp_languages.Length - 1, _temp_languages[num], delegate(int current_index, UILabel label)
		{
			label.text = _temp_languages[current_index];
			GameSettings.me.language = GJL.LANGUAGES[current_index];
			GameSettings.me.ApplyLanguageChange();
			GameSettings.Save();
		});
		_screen_mode_index = me.screen_mode;
		_cursor_mode_index = (int)me.cursor_mode;
		_vsync_mode_index = me.vsync_mode;
		if (resolution_switcher != null)
		{
			_resolutions = ResolutionConfig.GetResolutionsStringArray();
			_screen_res_index = 0;
			string resolutionName = GameSettings.current_resolution.GetResolutionName();
			Debug.Log("Current resolution name = " + resolutionName + ", " + GameSettings.current_resolution);
			int num3 = -1;
			string[] resolutions = _resolutions;
			foreach (string obj in resolutions)
			{
				num3++;
				if (obj == resolutionName)
				{
					Debug.Log("Found resolution index = " + num3);
					_screen_res_index = num3;
					break;
				}
			}
			resolution_switcher.SetupOptions(_screen_res_index, _resolutions.Length - 1, _resolutions[_screen_res_index], delegate(int index, UILabel label)
			{
				label.text = _resolutions[index];
				_screen_res_index = index;
			}, call_onchanged_on_init: true);
			_cur_screen_res_index = _screen_res_index;
		}
		if (screen_mode_switcher != null)
		{
			screen_mode_switcher.SetupOptions(_screen_mode_index, _screen_modes.Length - 1, _screen_modes[_screen_mode_index], delegate(int index, UILabel label)
			{
				label.text = GJL.L(_screen_modes[index]);
				_screen_mode_index = index;
			}, call_onchanged_on_init: true);
		}
		if (vsync_mode_switcher != null)
		{
			vsync_mode_switcher.SetupOptions(_vsync_mode_index, _vsync_mode_modes.Length - 1, _vsync_mode_modes[_vsync_mode_index], delegate(int index, UILabel label)
			{
				label.text = GJL.L(_vsync_mode_modes[index]);
				_vsync_mode_index = index;
			}, call_onchanged_on_init: true);
		}
		if (cursor_mode_switcher != null)
		{
			cursor_mode_switcher.SetupOptions(_cursor_mode_index, _cursor_modes.Length - 1, _cursor_modes[_cursor_mode_index], delegate(int index, UILabel label)
			{
				label.text = GJL.L(_cursor_modes[index]);
				_cursor_mode_index = index;
			}, call_onchanged_on_init: true);
		}
		if (mixer_light_switcher != null)
		{
			mixer_light_switcher.gameObject.SetActive(value: false);
		}
		if (BaseGUI.for_gamepad)
		{
			slider_master.OnOver();
		}
	}

	public void OnPressedLog()
	{
		Debug.Log("Option menu log");
	}

	public void OnBackBtnClicked()
	{
		OnClosePressed();
	}

	protected override bool OnPressedBack()
	{
		OnClosePressed();
		return true;
	}

	public override void OnClosePressed()
	{
		base.OnClosePressed();
		if (GameSettings.me.screen_mode != _screen_mode_index || GameSettings.me.cursor_mode != (GameSettings.CursorMode)_cursor_mode_index || _cur_screen_res_index != _screen_res_index || GameSettings.me.vsync_mode != _vsync_mode_index)
		{
			ShowVideoOptionsConfirmationWindow(delegate
			{
				if (!MainGame.game_started)
				{
					GUIElements.me.main_menu.Open(switch_music: false);
				}
			});
		}
		else if (!MainGame.game_started)
		{
			GUIElements.me.main_menu.Open(switch_music: false);
		}
	}

	private void ShowVideoOptionsConfirmationWindow(Action on_closed)
	{
		ResolutionConfig res = ResolutionConfig.GetResolutionByIndex(_screen_res_index);
		GameSettings.me.ApplyCustomScreenParameters(_screen_mode_index, (GameSettings.CursorMode)_cursor_mode_index, res.x, res.y, _vsync_mode_index);
		_revert_time_start = Time.time;
		_revert_dialog_closed_with_button = false;
		Action on_dialog_closed = delegate
		{
			Debug.Log("Video settings confirmation: on_dialog_closed");
			_revert_dialog_closed_with_button = true;
			if (_revert_timer != null)
			{
				_revert_timer.Stop();
			}
			_revert_timer = null;
			on_closed.TryInvoke();
		};
		if (GUIElements.me.ingame_menu.is_shown)
		{
			GUIElements.me.ingame_menu.Hide(play_sound: false);
		}
		GUIElements.me.dialog.Open(GJL.L("video_changed"), GJL.L("apply"), delegate
		{
			GameSettings.me.screen_mode = _screen_mode_index;
			GameSettings.me.cursor_mode = (GameSettings.CursorMode)_cursor_mode_index;
			GameSettings.me.res_x = res.x;
			GameSettings.me.res_y = res.y;
			GameSettings.me.vsync_mode = _vsync_mode_index;
			GameSettings.Save();
			SmartAudioEngine.me.SetDullMusicMode(dull_mode: false);
			GUIElements.me.main_menu.ShiftMenuElementsForSmallResolution();
		}, GJL.L("revert"), delegate
		{
			GameSettings.me.ApplyScreenMode();
			Open();
		}, on_dialog_closed.TryInvoke);
		UpdateTimerInVideoSettingsDialog(10f);
		_revert_timer = GJTimer.AddConditionalChecker(() => _revert_dialog_closed_with_button, delegate
		{
			float num = 10f - (Time.time - _revert_time_start);
			UpdateTimerInVideoSettingsDialog(num);
			if (num <= 0f)
			{
				GameSettings.me.ApplyScreenMode();
				GUIElements.me.dialog.OnClosePressed();
				Open();
				on_dialog_closed();
			}
		}, delegate
		{
		});
	}

	private static void UpdateTimerInVideoSettingsDialog(float time_left)
	{
		string s = GJL.L("video_changed", "<" + Mathf.CeilToInt(time_left) + ">");
		s = LocalizedLabel.ColorizeTags(s, LocalizedLabel.TextColor.Tutorial);
		GUIElements.me.dialog.label_1.text = s;
	}
}
