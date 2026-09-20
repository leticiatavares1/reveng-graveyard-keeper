using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using Steamworks;
using UnityEngine;

public static class PlatformSpecific
{
	public enum Platform
	{
		PС,
		XBox,
		PS4,
		Switch
	}

	public delegate void OnCompleteReadSaveSlotsDelegate(List<SaveSlotData> slots);

	public delegate void OnSaveCompleteDelegate(SaveSlotData slot);

	public delegate void OnGameLoadedDelegate(GameSave save_data);

	public delegate void OnCompleteDelegate();

	private enum DiskOperationType
	{
		Other,
		Save,
		Load
	}

	public const Platform PLATFORM = Platform.PС;

	public const bool GAMEPAD_SUPPORTED = true;

	public const bool HAS_EXIT_BUTTON_IN_MAIN_MENU = true;

	public const bool HAS_PROFILES_BUTTON_IN_MAIN_MENU = false;

	public const bool ALLOW_GAMEPAD_COMBINATION_FOR_FPS_COUNTER = true;

	public const float DEFAULT_FIXED_DELTA_TIME = 1f / 60f;

	private const bool EMULATE_CONSOLE_SAFE_ZONES = false;

	public const bool FX_NOISE_AND_GRAIN = true;

	public const int MAX_SHADOWS_CREATED_PER_FRAME = 4;

	public const int MAX_SHADOWS_PER_OBJECT = 4;

	public const bool PRELOAD_WOPS_ASYNC = true;

	public const bool PRELOAD_VISUAL_SCRIPTS_ASYNC = true;

	public const int SIMULTANEOUS_ASYNC_RESOURCE_LOADING_THREADS = 4;

	public const bool HALF_RESOLUTION_MODE = false;

	private static List<SaveSlotData> _slots = new List<SaveSlotData>();

	private static GameEvents.GameStatus _cur_status = GameEvents.GameStatus.Undefined;

	private static FullScreenMode _full_screen_mode = FullScreenMode.FullScreenWindow;

	public static void ReadSaveSlots(OnCompleteReadSaveSlotsDelegate on_complete)
	{
		ShowDiskAccessIndicator(DiskOperationType.Load, show: true);
		_slots = new List<SaveSlotData>();
		string[] files = Directory.GetFiles(GetSaveFolder(), "*.info", SearchOption.TopDirectoryOnly);
		foreach (string text in files)
		{
			try
			{
				SaveSlotData saveSlotData = SaveSlotData.FromJSON(File.ReadAllText(text));
				if (saveSlotData != null)
				{
					saveSlotData.filename_no_extension = Path.GetFileNameWithoutExtension(text);
					_slots.Add(saveSlotData);
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("Error reading savegame information, file: " + text + "\n" + ex);
			}
		}
		ShowDiskAccessIndicator(DiskOperationType.Load, show: false);
		on_complete(_slots);
	}

	public static void LoadGame(SaveSlotData slot, OnGameLoadedDelegate on_lodaded)
	{
		ShowDiskAccessIndicator(DiskOperationType.Load, show: true);
		Debug.Log("Load game, slot = " + slot);
		DelayByOneFrame(delegate
		{
			string text = GetSaveFolder() + slot.filename_no_extension + ".dat";
			if (!File.Exists(text))
			{
				Debug.LogError("Save file not found: " + text);
				ShowDiskAccessIndicator(DiskOperationType.Load, show: false);
				on_lodaded(null);
			}
			else
			{
				GameSave gameSave = null;
				try
				{
					gameSave = ((!slot.IsBinaryFormat()) ? GameSave.FromJSON(File.ReadAllText(text)) : GameSave.FromBinary(File.ReadAllBytes(text)));
				}
				catch (Exception ex)
				{
					Debug.LogError("Error reading save file " + text + "\n" + ex);
					ShowDiskAccessIndicator(DiskOperationType.Load, show: false);
					LoadingGUI.HideImmediate();
					LoadingGUI.ShowBlackBackground(vis: false);
					GUIElements.me.dialog.OpenOK(GJL.L("LoadErrorPrompt header") + "\n\n" + GJL.L("LoadErrorPrompt body"), delegate
					{
						GUIElements.me.main_menu.Open();
					});
					return;
				}
				slot.linked_save = gameSave;
				MainGame.me.save_slot = slot;
				ShowDiskAccessIndicator(DiskOperationType.Load, show: false);
				on_lodaded(gameSave);
			}
		});
	}

	public static void DeleteSlot(SaveSlotData slot, OnCompleteDelegate on_complete)
	{
		Debug.Log("DeleteSlot, slot = " + slot);
		string text = GetSaveFolder() + slot.filename_no_extension;
		File.Delete(text + ".dat");
		File.Delete(text + ".info");
		on_complete();
	}

	public static void SaveGame(SaveSlotData slot, GameSave save, OnSaveCompleteDelegate on_complete)
	{
		Debug.Log("SaveGame, slot = " + slot?.ToString() + ", filename = " + ((slot == null) ? "null" : slot.filename_no_extension));
		if (slot != null)
		{
			ShowDiskAccessIndicator(DiskOperationType.Save, show: true);
			slot.PrepareForSave();
		}
		DelayByOneFrame(delegate
		{
			if (slot == null)
			{
				slot = new SaveSlotData
				{
					filename_no_extension = GetNewSlotFilename(),
					linked_save = save
				};
				Debug.Log("Created a new save filename = " + slot.filename_no_extension);
			}
			SaveSlotInfoAndData(slot, save, delegate(SaveSlotData s)
			{
				ShowDiskAccessIndicator(DiskOperationType.Save, show: false);
				on_complete(s);
			});
		});
	}

	private static void ShowDiskAccessIndicator(DiskOperationType type, bool show)
	{
		if (type == DiskOperationType.Save)
		{
			GUIElements.me.ShowSavingStatus(show);
		}
	}

	public static string GetSaveFolder()
	{
		return Application.persistentDataPath + "/";
	}

	public static void Init()
	{
		GameKeyTip.SetPlatformPrefix("");
		Debug.Log("SteamManager.Initialized = " + SteamManager.Initialized);
	}

	private static string GetNewSlotFilename()
	{
		int num = 0;
		while (num++ <= 1000)
		{
			string text = num.ToString();
			bool flag = false;
			foreach (SaveSlotData slot in _slots)
			{
				if (slot.filename_no_extension == text)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				return text;
			}
		}
		Debug.LogError("GetNewSlotFilename: too many iterations");
		return null;
	}

	private static void SaveSlotInfoAndData(SaveSlotData slot, GameSave save, OnSaveCompleteDelegate on_complete)
	{
		if (slot == null)
		{
			Debug.LogError("SaveSlotData: Can't save to a null slot");
			on_complete(null);
			return;
		}
		File.WriteAllText(GetSaveFolder() + slot.filename_no_extension + ".info", slot.ToJSON());
		SaveGameDataToSlot(slot, save, delegate
		{
			on_complete(slot);
		});
	}

	private static void SaveGameDataToSlot(SaveSlotData slot, GameSave save, OnCompleteDelegate on_complete)
	{
		if (slot == null)
		{
			Debug.LogError("SaveGameDataToSlot: Can't save to a null slot");
			on_complete();
			return;
		}
		if (save == null)
		{
			Debug.LogError("SaveGameDataToSlot: Can't save a null game data");
			on_complete();
			return;
		}
		save.PrepareForSave();
		string text = GetSaveFolder() + slot.filename_no_extension + ".dat";
		GC.Collect();
		Resources.UnloadUnusedAssets();
		Debug.Log("Serializing save...");
		byte[] array = save.ToBinary();
		Debug.Log("Serialized length: " + array.Length);
		try
		{
			File.WriteAllBytes(text + ".new", array);
			if (File.Exists(text + ".backup.2"))
			{
				File.Delete(text + ".backup.2");
			}
			if (File.Exists(text + ".backup.1"))
			{
				File.Move(text + ".backup.1", text + ".backup.2");
			}
			if (File.Exists(text))
			{
				File.Move(text, text + ".backup.1");
			}
			File.Move(text + ".new", text);
		}
		catch (Exception ex)
		{
			Debug.LogError("Error saving file: " + ex);
			GUIElements.me.dialog.OpenOK("Error saving file!\n\n" + ex);
		}
		on_complete();
	}

	public static void OnProfileSelect()
	{
	}

	public static void SaveGameSettings(GameSettings data)
	{
		string value = JsonUtility.ToJson(data);
		PlayerPrefs.SetString("settings", value);
	}

	public static GameSettings LoadGameSettings()
	{
		Debug.Log("LoadGameSettings");
		string text = "";
		if (PlayerPrefs.HasKey("settings"))
		{
			text = PlayerPrefs.GetString("settings");
		}
		GameSettings gameSettings = (string.IsNullOrEmpty(text) ? new GameSettings() : JsonUtility.FromJson<GameSettings>(text));
		if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)) && (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt)) && (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl)))
		{
			Debug.Log("Resetting video settings...");
			gameSettings.screen_mode = 0;
			gameSettings.res_x = 1920;
			gameSettings.res_y = 1080;
			gameSettings.cursor_mode = GameSettings.CursorMode.Software;
		}
		return gameSettings;
	}

	public static string GetInteractionButtonHint(bool for_gamepad)
	{
		if (!for_gamepad)
		{
			return "(E)";
		}
		return GetGamepadButtonSymbol(GamePadButton.A, "");
	}

	private static void DelayByOneFrame(Action action)
	{
		if (action != null)
		{
			GJTimer.AddTimer(0.001f, delegate
			{
				action();
			});
		}
	}

	public static void SetCursor(Texture2D tx, Vector2 pos, bool force_software = false)
	{
		Cursor.SetCursor(tx, pos, force_software ? CursorMode.ForceSoftware : CursorMode.Auto);
	}

	public static void FixResolutionAfterStart()
	{
	}

	public static void FixScreenModeAfterStart()
	{
	}

	public static void ApplyFullScreenMode(GameSettings.ScreenMode mode, int scr_w, int scr_h, int vsync)
	{
		switch (mode)
		{
		case GameSettings.ScreenMode.Windowed:
			_full_screen_mode = FullScreenMode.Windowed;
			break;
		case GameSettings.ScreenMode.FullScreen:
			_full_screen_mode = FullScreenMode.ExclusiveFullScreen;
			break;
		case GameSettings.ScreenMode.Borderless:
			_full_screen_mode = FullScreenMode.FullScreenWindow;
			break;
		default:
			throw new Exception("Unsupported screen mode: " + mode);
		}
		Debug.Log($"ApplyFullScreenMode {scr_w}x{scr_h}, {_full_screen_mode}, vsync = {vsync}");
		int preferred_refresh_rate = 0;
		if (_full_screen_mode != 0 || vsync == 0)
		{
			preferred_refresh_rate = 60;
		}
		if (true)
		{
			Debug.Log("SetResolution #1");
			Screen.SetResolution(scr_w, scr_h, _full_screen_mode, preferred_refresh_rate);
			ResolutionHelper.OnResolutionChanged(scr_w, scr_h);
			GJTimer.AddTimer(0f, delegate
			{
				Debug.Log("SetResolution #2");
				Screen.SetResolution(scr_w, scr_h, _full_screen_mode, preferred_refresh_rate);
			});
			switch (vsync)
			{
			case 0:
				QualitySettings.vSyncCount = 0;
				Application.targetFrameRate = 60;
				break;
			case 1:
				QualitySettings.vSyncCount = 1;
				Application.targetFrameRate = -1;
				break;
			default:
				Debug.LogError($"Unsupported vsync mode = {vsync}");
				break;
			}
		}
	}

	public static void ApplyScreenSafeZones()
	{
	}

	public static string GetGamepadButtonSymbol(GamePadButton b, string suffix = " ")
	{
		switch (b)
		{
		case GamePadButton.None:
			return string.Empty;
		case GamePadButton.A:
			return "(" + GameKeyTip.GetPlatformPrefix() + "A)" + suffix;
		case GamePadButton.B:
			return "(" + GameKeyTip.GetPlatformPrefix() + "B)" + suffix;
		case GamePadButton.X:
			return "(" + GameKeyTip.GetPlatformPrefix() + "X)" + suffix;
		case GamePadButton.Y:
			return "(" + GameKeyTip.GetPlatformPrefix() + "Y)" + suffix;
		default:
			Debug.LogWarning("No symbol for a gamepad button = " + b);
			return string.Empty;
		}
	}

	public static void SetGameStatus(GameEvents.GameStatus status)
	{
		if (_cur_status != status)
		{
			_cur_status = status;
			Debug.Log("SetGameStatus: " + status);
		}
	}

	public static void SetGameMetric(GameEvents.GameMetric metric, float value)
	{
		Debug.Log("SetGameMetric " + metric.ToString() + " = " + value);
	}

	public static void OnAchievementComplete(AchievementDefinition ach)
	{
		Debug.Log("OnAchievementComplete: <color=green>" + ach.id + "</color>");
		if (!SteamManager.Initialized)
		{
			Debug.LogWarning("No steam - no achievement");
			return;
		}
		SteamUserStats.SetAchievement(ach.id);
		SteamUserStats.StoreStats();
		SteamAPI.RunCallbacks();
	}

	public static void SetDefaultCultureInfo()
	{
		Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
		CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
	}

	public static void OpenStoreLink(DLCEngine.DLCVersion version)
	{
		string text = string.Empty;
		switch (version)
		{
		case DLCEngine.DLCVersion.Stories:
			text = "https://store.steampowered.com/app/1163770/Graveyard_Keeper__Stranger_Sins/";
			break;
		case DLCEngine.DLCVersion.Refugees:
			text = "https://store.steampowered.com/app/1430990/Graveyard_Keeper__Game_Of_Crone/";
			break;
		case DLCEngine.DLCVersion.Souls:
			text = "https://store.steampowered.com/app/1788370/Graveyard_Keeper__Better_Save_Soul/";
			break;
		}
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
}
