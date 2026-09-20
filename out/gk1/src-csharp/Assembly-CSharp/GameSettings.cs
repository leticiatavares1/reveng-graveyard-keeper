using System;
using UnityEngine;

[Serializable]
public class GameSettings
{
	public enum CursorMode
	{
		Default,
		Hardware,
		Software
	}

	public enum ScreenMode
	{
		Borderless,
		FullScreen,
		Windowed
	}

	private static GameSettings _me;

	public int volume_master = 100;

	public int volume_music = 100;

	public int volume_sfx = 100;

	public int volume_speech = 100;

	public int screen_mode;

	public CursorMode cursor_mode = CursorMode.Software;

	public string language = "";

	private static string _cur_lng = "";

	public int res_x;

	public int res_y;

	public int vsync_mode;

	public static ResolutionConfig current_resolution = null;

	public string keys_binding_json = "";

	public bool is_stranger_sins_popup_window_shown;

	public bool is_refugees_popup_window_shown;

	public bool is_souls_popup_window_shown;

	public static GameSettings me
	{
		get
		{
			if (_me == null)
			{
				_me = PlatformSpecific.LoadGameSettings();
				KeyBindings.FromJSON(_me.keys_binding_json);
			}
			return _me;
		}
	}

	public static void Init()
	{
		me.ApplyVolume();
		if (MainGame.me.grain_fx_component != null)
		{
			MainGame.me.grain_fx_component.enabled = true;
		}
	}

	public void ApplyVolume()
	{
		SmartAudioEngine.me.SetChannelVolume("master", (float)volume_master / 100f);
		SmartAudioEngine.me.SetChannelVolume("music", (float)volume_music / 100f);
		SmartAudioEngine.me.SetChannelVolume("sfx", (float)volume_sfx / 100f);
		SmartAudioEngine.me.SetChannelVolume("speech", (float)volume_speech / 100f, 9f);
		Save();
	}

	public static void Save()
	{
		me.keys_binding_json = KeyBindings.ToJSON();
		PlatformSpecific.SaveGameSettings(me);
	}

	public void ApplyScreenMode()
	{
		Debug.Log("ApplyScreenMode()");
		if (res_x == 0 || res_y == 0)
		{
			res_x = 1920;
			res_y = 1080;
		}
		ApplyCustomScreenParameters(screen_mode, cursor_mode, res_x, res_y, vsync_mode);
	}

	public void ApplyCustomScreenParameters(int screen_mode, CursorMode cursor_mode, int res_x, int res_y, int vsync, bool retrying = false)
	{
		ScreenMode screenMode = (ScreenMode)screen_mode;
		Debug.Log("ApplyCustomScreenParameters, screen_mode = " + screenMode.ToString() + $", cursor = {cursor_mode}, res = {res_x}x{res_y}");
		PlatformSpecific.ApplyFullScreenMode((ScreenMode)screen_mode, res_x, res_y, vsync);
		if (cursor_mode == CursorMode.Default)
		{
			PlatformSpecific.SetCursor(null, Vector2.zero);
		}
		else
		{
			PlatformSpecific.SetCursor(Resources.Load<Texture2D>("mouse_cursor"), new Vector2(2f, -2f), cursor_mode == CursorMode.Software);
		}
		if (MainGame.me != null)
		{
			MainGame.me.OnScreenSizeChanged(res_x, res_y);
		}
		current_resolution = ResolutionConfig.GetResolutionConfigOrNull(res_x, res_y);
		if (current_resolution == null || !current_resolution.IsHardwareSupported())
		{
			if (retrying)
			{
				Debug.LogError("Coudln't apply FullHD... Don't know what to do.");
				current_resolution = new ResolutionConfig(res_x, res_y);
			}
			else
			{
				Debug.LogError($"Couldn't find a suitable configuration for resolution {res_x}x{res_y}, trying FullHD...");
				ApplyCustomScreenParameters(screen_mode, cursor_mode, 1920, 1080, vsync, retrying: true);
			}
		}
		else
		{
			Debug.Log("Applied resolution: " + current_resolution);
		}
	}

	public void ApplyLanguageChange()
	{
		if (string.IsNullOrEmpty(language))
		{
			language = GJL.GetCurrentLocaleCode();
		}
		if (_cur_lng == language)
		{
			return;
		}
		_cur_lng = language;
		GJL.LoadLanguageResource(language);
		GUIElements.UpdateLanguageChangeForAllBaseGUI();
		foreach (ItemDefinition items_datum in GameBalance.me.items_data)
		{
			items_datum.ResetLanguageCache();
		}
		foreach (TechDefinition techs_datum in GameBalance.me.techs_data)
		{
			techs_datum.ResetLanguageCache();
		}
		LabelSizeCalculator.ApplyLanguageChange();
	}

	public static string GetCurrentLanguage()
	{
		return _cur_lng;
	}
}
