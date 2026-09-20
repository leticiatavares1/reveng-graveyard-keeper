using System;
using DarkTonic.MasterAudio;
using Steamworks;
using UnityEngine;

public class MainMenuGUI : BaseMenuGUI
{
	private NewsGUI _news_gui;

	public SimpleUITable buttons_table;

	public MenuItemGUI mm_profiles;

	public MenuItemGUI mm_exit;

	public UILabel version_txt;

	public GameObject[] logos_dlc;

	private GamepadNavigationItem _last_focused_item;

	public GameObject pc2PreorderBanner;

	public GameObject pc2AvailableBanner;

	[SerializeField]
	private MainMenuLogoController _main_menu_logo_controller;

	private float shift;

	public override void Init()
	{
		_news_gui = GetComponentInChildren<NewsGUI>();
		if (_news_gui != null)
		{
			_news_gui.Init();
		}
		ShiftMenuElementsForSmallResolution();
		base.Init();
	}

	public new void Open(bool switch_music = true)
	{
		Debug.Log("Open main menu, switch_music = " + switch_music);
		GameSettings.me.ApplyVolume();
		MainGame.game_started = false;
		version_txt.text = $"ver. {LazyConsts.VERSION:0.000#}".Replace(",", ".");
		UtilityStuff.ProcessVersionLabel(version_txt);
		base.Open();
		LazyInput.on_input_changed += OnInputSourceChanged;
		MainGame.me.world_root.gameObject.SetActive(value: false);
		ShowDLCIcons();
		mm_profiles.gameObject.SetActive(value: false);
		mm_exit.gameObject.SetActive(value: true);
		buttons_table.Reposition();
		if (BaseGUI.for_gamepad)
		{
			if (_last_focused_item == null)
			{
				base.gamepad_controller.FocusOnFirstActive();
			}
			else
			{
				base.gamepad_controller.SetFocusedItem(_last_focused_item);
			}
		}
		if (switch_music)
		{
			MasterAudio.StopAllPlaylists();
			MasterAudio.TriggerPlaylistClip("menu", "menu");
		}
		if (_news_gui != null)
		{
			_news_gui.Open();
		}
		TitleScreen.Show();
		PlatformSpecific.SetGameStatus(GameEvents.GameStatus.InMenu);
		GUIElements.me.CloseAllInGameWindows();
		pc2AvailableBanner.gameObject.SetActive(value: false);
		pc2PreorderBanner.gameObject.SetActive(value: false);
		if (DateTime.Now > new DateTime(2023, 7, 23))
		{
			pc2AvailableBanner.gameObject.SetActive(value: true);
		}
		else
		{
			pc2PreorderBanner.gameObject.SetActive(value: true);
		}
	}

	public static void ShowDLCIcons(GameObject[] dlc_icons)
	{
		int num = 0;
		for (int i = 0; i < dlc_icons.Length; i++)
		{
			dlc_icons[i].SetActive(value: false);
		}
		if (DLCEngine.IsDLCAvailable(DLCEngine.DLCVersion.BreakingDead))
		{
			num = 1;
			if (DLCEngine.IsDLCAvailable(DLCEngine.DLCVersion.Stories))
			{
				num = 4;
				if (DLCEngine.IsDLCAvailable(DLCEngine.DLCVersion.Refugees))
				{
					num = 7;
				}
			}
			else if (DLCEngine.IsDLCAvailable(DLCEngine.DLCVersion.Refugees))
			{
				num = 5;
			}
		}
		else if (DLCEngine.IsDLCAvailable(DLCEngine.DLCVersion.Stories))
		{
			num = 2;
			if (DLCEngine.IsDLCAvailable(DLCEngine.DLCVersion.Refugees))
			{
				num = 6;
			}
		}
		else if (DLCEngine.IsDLCAvailable(DLCEngine.DLCVersion.Refugees))
		{
			num = 3;
		}
		dlc_icons[num].SetActive(value: true);
	}

	public void ShowDLCIcons()
	{
		_main_menu_logo_controller.ShowLogos();
	}

	public void OnPressedPlay()
	{
		GUIElements.me.saves.Open();
	}

	public void OnPressedContinue()
	{
		GUIElements.me.saves.Open();
	}

	public void OnPressedProfile()
	{
		PlatformSpecific.OnProfileSelect();
	}

	public void OnPressedOptions()
	{
		GUIElements.me.main_menu.Hide();
		GUIElements.me.options.Open();
	}

	public void OnPressedLiveStreaming()
	{
		GUIElements.me.main_menu.Hide();
		GUIElements.me.live_streaming.Open();
	}

	public void OnPressedCredits()
	{
		GUIElements.me.main_menu.Hide();
		GUIElements.me.credits.Open();
		GUIElements.me.hud.Hide();
	}

	public void OnBackFromCredits()
	{
		GUIElements.me.main_menu.Open(switch_music: false);
		GUIElements.me.credits.Hide();
	}

	public void OnPressedExit()
	{
		Application.Quit();
	}

	public override void Hide(bool play_sound = true)
	{
		if (_news_gui != null)
		{
			_news_gui.Hide();
		}
		LazyInput.on_input_changed -= OnInputSourceChanged;
		if (play_sound && BaseGUI.for_gamepad)
		{
			_last_focused_item = base.gamepad_controller.focused_item;
		}
		base.Hide(play_sound);
	}

	protected override void OnInputSourceChanged()
	{
		if (base.is_shown_and_top)
		{
			UpdateSourceType(force: true);
			InitPlatformDependentStuff();
			if (base.is_shown && BaseGUI.for_gamepad)
			{
				LazyInput.ClearAllKeysDown();
				LazyInput.WaitForReleaseNavigationKeys();
				base.gamepad_controller.ReinitItems(focus_on_first_active: true);
			}
		}
	}

	public override void UpdatTip(bool select_active)
	{
	}

	public void OnLeaveFeedbackButtonPressed()
	{
		Application.OpenURL("https://steamcommunity.com/app/599140/discussions/1/");
	}

	public void OnStrangerSinsBannerPressed()
	{
		PlatformSpecific.OpenStoreLink(DLCEngine.DLCVersion.Stories);
	}

	public void OnRefugeesBannerPressed()
	{
		PlatformSpecific.OpenStoreLink(DLCEngine.DLCVersion.Refugees);
	}

	public void OnSoulsBannerPressed()
	{
		PlatformSpecific.OpenStoreLink(DLCEngine.DLCVersion.Souls);
	}

	public void OnPC2BannerPressed()
	{
		string text = "https://store.steampowered.com/app/1161590/Punch_Club_2_Fast_Forward/";
		if (SteamManager.Initialized)
		{
			Debug.Log("SteamManager is initialized");
			SteamFriends.ActivateGameOverlayToWebPage(text);
		}
		else
		{
			Debug.Log("SteamManager isn't initialized");
			Application.OpenURL(text);
		}
	}

	public void ShiftMenuElementsForSmallResolution()
	{
		float num = (float)Screen.width / (float)Screen.height;
		float y = 0f;
		if (num > 1.5f)
		{
			if (Screen.width <= 1366)
			{
				y = -55f - shift;
				shift = -55f;
			}
			else if (Screen.width <= 1600 && DLCEngine.DLCAvailableCount() == 4)
			{
				y = -50f - shift;
				shift = -50f;
			}
			else if (Screen.width <= 1920 && DLCEngine.DLCAvailableCount() == 4)
			{
				y = -50f - shift;
				shift = -50f;
			}
			else
			{
				y = 0f - shift;
				shift = 0f;
			}
		}
		Vector3 vector = new Vector3(0f, y, 0f);
		buttons_table.gameObject.transform.localPosition += vector;
		version_txt.gameObject.transform.localPosition += vector;
		for (int i = 0; i < logos_dlc.Length; i++)
		{
			logos_dlc[i].gameObject.transform.localPosition += vector;
		}
		_main_menu_logo_controller.gameObject.transform.localPosition += vector;
	}
}
