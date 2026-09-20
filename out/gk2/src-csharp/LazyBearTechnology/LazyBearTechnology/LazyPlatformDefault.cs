using System;
using Steamworks;
using UnityEngine;

namespace LazyBearTechnology;

public class LazyPlatformDefault : ILazyPlatform
{
	private Callback<GamepadTextInputDismissed_t> gamepadTextInputDismissed;

	private Action<string> gamepadTextInputCallback;

	private bool steamStatsDirty;

	public event Action<PlayerInfo> OnUserDataChanged;

	public event Action<string> OnDifferentUserFound;

	public event Action OnNoUsersFound;

	public event Action OnAllControllersDisabled;

	public void Init()
	{
		if (SteamManager.Initialized)
		{
			gamepadTextInputDismissed = Callback<GamepadTextInputDismissed_t>.Create(OnGamepadTextInputRecieved);
		}
	}

	public LazyPlatform GetPlatformId()
	{
		return LazyPlatform.PC;
	}

	public void Update()
	{
		FlushSteamStatsIfDirty();
	}

	public void AddUser(bool tryAddSilently = true)
	{
	}

	public string GetUniqueUserId()
	{
		if (SteamManager.Initialized)
		{
			return SteamFriends.GetPersonaName() + " " + SteamUser.GetSteamID().ToString();
		}
		return null;
	}

	public string GetPlatformName()
	{
		if (SteamManager.Initialized)
		{
			return "Steam";
		}
		return "PC";
	}

	public bool IsGuest()
	{
		return false;
	}

	public void UnlockAchievement(string achievementId)
	{
		if (IsAchievementSystemActive())
		{
			SteamUserStats.SetAchievement(achievementId);
			MarkSteamStatsDirty();
			FlushSteamStatsIfDirty();
		}
	}

	public void SetAchievementProgress(string id, int currentValue, int fullValue)
	{
		if (string.IsNullOrEmpty(id) || currentValue <= 0 || !IsAchievementSystemActive())
		{
			return;
		}
		try
		{
			if ((!SteamUserStats.GetStat(id, out int pData) || pData != currentValue) && SteamUserStats.SetStat(id, currentValue))
			{
				MarkSteamStatsDirty();
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning($"#achievement# SetAchievementProgress failed id:[{id}] value:[{currentValue}/{fullValue}] {ex}");
		}
	}

	public bool ClearAchievementById(string id)
	{
		return SteamUserStats.ClearAchievement(id);
	}

	public bool ClearAllAchievements()
	{
		return SteamUserStats.ResetAllStats(bAchievementsToo: true);
	}

	private void MarkSteamStatsDirty()
	{
		steamStatsDirty = true;
	}

	private void FlushSteamStatsIfDirty()
	{
		if (steamStatsDirty && SteamManager.Initialized)
		{
			SteamUserStats.StoreStats();
			steamStatsDirty = false;
		}
	}

	private bool IsAchievementSystemActive()
	{
		try
		{
			return SteamManager.Initialized;
		}
		catch (Exception)
		{
			return false;
		}
	}

	public bool PrefsHasKey(string key)
	{
		return PlayerPrefs.HasKey(key);
	}

	public string PrefsGetString(string key, string defaultValue = "")
	{
		return PlayerPrefs.GetString(key, defaultValue);
	}

	public int PrefsGetInt(string key, int defaultValue = 0)
	{
		return PlayerPrefs.GetInt(key, defaultValue);
	}

	public void PrefsSetString(string key, string value)
	{
		PlayerPrefs.SetString(key, value);
	}

	public void PrefsSave()
	{
		PlayerPrefs.Save();
	}

	public bool IsDLCAvailable(DLCInfo dlcInfo)
	{
		if (!dlcInfo.useFileCheck)
		{
			if (dlcInfo.steamAppId == 0)
			{
				return false;
			}
			if (SteamManager.Initialized)
			{
				return SteamApps.BIsDlcInstalled(new AppId_t(dlcInfo.steamAppId));
			}
		}
		return LazyAPI.LazyFile.Exists(Application.dataPath + "/" + dlcInfo.dlcFilePath);
	}

	public void OpenProductInStore(StoreProductInfo storeProductInfo)
	{
		Application.OpenURL(storeProductInfo.steamUrl);
	}

	public void ShowKeyboard(Action<string> callback, int textMaxLength, string headerText)
	{
		if (SteamManager.Initialized)
		{
			if (SteamUtils.ShowGamepadTextInput(EGamepadTextInputMode.k_EGamepadTextInputModeNormal, EGamepadTextInputLineMode.k_EGamepadTextInputLineModeSingleLine, headerText, (uint)textMaxLength, string.Empty))
			{
				gamepadTextInputCallback = callback;
				SteamAPI.RunCallbacks();
			}
			else
			{
				callback?.Invoke(string.Empty);
			}
		}
	}

	private void OnGamepadTextInputRecieved(GamepadTextInputDismissed_t callback)
	{
		if (!SteamManager.Initialized || !callback.m_bSubmitted)
		{
			gamepadTextInputCallback?.Invoke(string.Empty);
			return;
		}
		uint enteredGamepadTextLength = SteamUtils.GetEnteredGamepadTextLength();
		SteamUtils.GetEnteredGamepadTextInput(out var pchText, enteredGamepadTextLength);
		gamepadTextInputCallback?.Invoke(pchText);
	}
}
