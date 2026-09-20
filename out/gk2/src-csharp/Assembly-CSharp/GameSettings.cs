using System;
using System.Collections.Generic;
using LazyBearTechnology;
using Steamworks;
using UnityEngine;
using UnityEngine.Rendering;

public class GameSettings : ISerializableData
{
	[Serializable]
	public class KeyBindingForSave
	{
		public int gameKeyValue;

		public KeyCode keyCode;
	}

	private static GameSettings instance;

	public float masterVolume = 100f;

	public float musicVolume = 80f;

	public float sfxVolume = 80f;

	public float speechVolume = 80f;

	public string language = "";

	public ResolutionConfig resolutionConfig;

	public ScreenMode screenMode;

	public VSyncMode vsyncMode = VSyncMode.Enabled;

	public TargetFrameRate targetFrameRate = TargetFrameRate.Fps60;

	public VoiceOverMode voiceOverMode;

	public GameCursorMode cursorMode;

	public GraphicsTier graphicsTier;

	public bool gpuGraphicsDefaultApplied;

	public bool steamDeckGraphicsDefaultApplied;

	public List<int> shownDlcStartupPopUps = new List<int>();

	public List<KeyBindingForSave> defaultKeyboardKeybindings = new List<KeyBindingForSave>();

	public List<KeyBindingForSave> keyboardKeybindings = new List<KeyBindingForSave>();

	[NonSerialized]
	private bool savedGameBindingsApplied;

	[NonSerialized]
	private int graphicSettingsAppliedFrame = -1;

	public static GameSettings Instance
	{
		get
		{
			if (instance == null)
			{
				instance = LoadAndApplyPlatformDefaults();
				ApplyPlatformSpecificRenderSettings();
			}
			return instance;
		}
	}

	public bool GraphicSettingsAppliedThisFrame => graphicSettingsAppliedFrame == Time.frameCount;

	public static event Action<IntVector2> OnResolutionChanged;

	public static event Action OnScreenSettingsApplied;

	public static event Action OnLanguageChanged;

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterSerialize()
	{
	}

	public void ApplySettings()
	{
		ApplySteamDeckDefaultGraphicsIfNeeded();
		ApplyGraphicSettings(applySave: false, applyEditorGameView: false);
		ApplyGraphicsTier(applySave: false);
		ApplyLanguageSettings(applySave: false);
		ApplyAudioSettings();
		TryInitDefaultBindings();
		ApplySavedGameBindings();
		SaveSystem.SaveGameSettings();
	}

	public void ApplyAudioSettings()
	{
		if (LazyAudio.IsInitialized)
		{
			LazyAudio.SetChannelVolume("master", masterVolume / 100f);
			LazyAudio.SetChannelVolume("music", musicVolume / 100f);
			LazyAudio.SetChannelVolume("sfx", sfxVolume / 100f);
			LazyAudio.SetChannelVolume("speech", speechVolume / 100f);
			SaveSystem.SaveGameSettings();
		}
	}

	public void ApplyGraphicSettings(bool applySave = true, bool applyEditorGameView = true)
	{
		if (!GraphicSettingsAppliedThisFrame)
		{
			graphicSettingsAppliedFrame = Time.frameCount;
			ApplyResolutionSettings(applySave: false, applyEditorGameView);
			ApplyScreenSettings();
			if (applySave)
			{
				SaveSystem.SaveGameSettings();
			}
		}
	}

	public void ApplyGraphicsTier(bool applySave = true)
	{
		PlatformFeatures.ReapplyAll();
		if (applySave)
		{
			SaveSystem.SaveGameSettings();
		}
	}

	public void ApplyResolutionSettings(bool applySave = false, bool applyEditorGameView = true)
	{
		if (resolutionConfig == null)
		{
			resolutionConfig = ResolutionConfig.GetOptimalResolution();
		}
		if (!resolutionConfig.IsValid)
		{
			resolutionConfig = ResolutionConfig.GetOptimalResolution();
		}
		ResolutionConfig.SetResolution(resolutionConfig);
		if (ResolutionConfig.currentResolution != null)
		{
			resolutionConfig = ResolutionConfig.currentResolution.Copy();
		}
		if (applySave)
		{
			SaveSystem.SaveGameSettings();
		}
		ResolutionConfig.LogCurrentResolutionConfig();
	}

	public void NotifyResolutionChanged()
	{
		Action<IntVector2> onResolutionChanged = GameSettings.OnResolutionChanged;
		if (onResolutionChanged == null)
		{
			return;
		}
		IntVector2 resolutionIntVector = GetResolutionIntVector2();
		Delegate[] invocationList = onResolutionChanged.GetInvocationList();
		for (int i = 0; i < invocationList.Length; i++)
		{
			try
			{
				((Action<IntVector2>)invocationList[i])(resolutionIntVector);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	public void ApplyScreenSettings()
	{
		if (ResolutionConfig.currentResolution == null)
		{
			Debug.Log("currentResolution is null");
			return;
		}
		FullScreenMode fullScreenMode = FullScreenMode.Windowed;
		switch (this.screenMode)
		{
		case ScreenMode.Windowed:
			fullScreenMode = FullScreenMode.Windowed;
			break;
		case ScreenMode.FullScreen:
			fullScreenMode = FullScreenMode.FullScreenWindow;
			break;
		default:
		{
			ScreenMode screenMode = this.screenMode;
			throw new Exception("Unsupported screen mode: " + screenMode);
		}
		}
		int unityTargetFrameRate = GetUnityTargetFrameRate();
		Debug.Log($"ApplyScreenMode Resolution: {ResolutionConfig.currentResolution.AppliedWidth}x{ResolutionConfig.currentResolution.AppliedHeight} PixelSize: " + $"{ResolutionConfig.PixelSize}, FullScreenMode: {fullScreenMode}, VSync: {vsyncMode != VSyncMode.Disabled}, TargetFPS: {unityTargetFrameRate}");
		Screen.SetResolution(ResolutionConfig.currentResolution.AppliedWidth, ResolutionConfig.currentResolution.AppliedHeight, fullScreenMode, new RefreshRate
		{
			numerator = 0u
		});
		switch (vsyncMode)
		{
		case VSyncMode.Disabled:
			QualitySettings.vSyncCount = 0;
			Application.targetFrameRate = unityTargetFrameRate;
			break;
		case VSyncMode.Enabled:
			QualitySettings.vSyncCount = 1;
			Application.targetFrameRate = unityTargetFrameRate;
			break;
		default:
			Debug.LogError($"Unsupported vsync mode = {vsyncMode}");
			break;
		}
		NotifyResolutionChanged();
		GameSettings.OnScreenSettingsApplied?.Invoke();
	}

	public bool SyncScreenModeFromHardware(bool applySave = true)
	{
		if (!TryGetScreenModeFromFullScreenMode(Screen.fullScreenMode, out var screenMode))
		{
			return false;
		}
		if (this.screenMode == screenMode)
		{
			return false;
		}
		this.screenMode = screenMode;
		if (applySave)
		{
			SaveSystem.SaveGameSettings();
		}
		return true;
	}

	private static bool TryGetScreenModeFromFullScreenMode(FullScreenMode fullScreenMode, out ScreenMode screenMode)
	{
		switch (fullScreenMode)
		{
		case FullScreenMode.ExclusiveFullScreen:
		case FullScreenMode.FullScreenWindow:
			screenMode = ScreenMode.FullScreen;
			return true;
		case FullScreenMode.MaximizedWindow:
		case FullScreenMode.Windowed:
			screenMode = ScreenMode.Windowed;
			return true;
		default:
			screenMode = ScreenMode.Windowed;
			return false;
		}
	}

	public void ApplyLanguageSettings(bool applySave = true)
	{
		if (string.IsNullOrEmpty(language))
		{
			language = LL.GetCurrentLocaleCode();
		}
		LLBase.LoadLanguageResource(language);
		if (applySave)
		{
			SaveSystem.SaveGameSettings();
		}
		VoiceOverModLoader.Refresh(language);
		VoiceOverSettings.LanguageId = (VoiceOverModLoader.IsActive ? VoiceOverModLoader.ActiveLanguage : "en");
		GameSettings.OnLanguageChanged?.Invoke();
	}

	public void ApplyDefaultGameBindings()
	{
		foreach (KeyBindingForSave defaultKeyboardKeybinding in defaultKeyboardKeybindings)
		{
			foreach (KeyBinding keyBinding in LazyInput.GameBindings.keyBindings)
			{
				if (keyBinding.gameKey.value == defaultKeyboardKeybinding.gameKeyValue)
				{
					keyBinding.keyCode = defaultKeyboardKeybinding.keyCode;
					break;
				}
			}
		}
		ControllerIconLibrary.UpdateStandaloneIcons();
		SaveCurrentGameBindings();
	}

	public void SaveCurrentGameBindings()
	{
		if (LazyInput.IsInitialized)
		{
			FillKeyBindingsForSave(keyboardKeybindings);
			SaveSystem.SaveGameSettings();
		}
	}

	public bool IsDlcStartupPopUpShown(DLCVersion dlcVersion)
	{
		EnsureShownDlcStartupPopUpsInitialized();
		return shownDlcStartupPopUps.Contains((int)dlcVersion);
	}

	public void MarkDlcStartupPopUpShown(DLCVersion dlcVersion)
	{
		EnsureShownDlcStartupPopUpsInitialized();
		if (!shownDlcStartupPopUps.Contains((int)dlcVersion))
		{
			shownDlcStartupPopUps.Add((int)dlcVersion);
			SaveSystem.SaveGameSettings();
		}
	}

	private void EnsureShownDlcStartupPopUpsInitialized()
	{
		if (shownDlcStartupPopUps == null)
		{
			shownDlcStartupPopUps = new List<int>();
		}
	}

	private void ApplySavedGameBindings()
	{
		if (!LazyInput.IsInitialized || savedGameBindingsApplied)
		{
			return;
		}
		if (keyboardKeybindings.Count == 0)
		{
			FillKeyBindingsForSave(keyboardKeybindings);
			savedGameBindingsApplied = true;
			return;
		}
		foreach (KeyBindingForSave keyboardKeybinding in keyboardKeybindings)
		{
			foreach (KeyBinding keyBinding in LazyInput.GameBindings.keyBindings)
			{
				if (keyBinding.gameKey.value == keyboardKeybinding.gameKeyValue)
				{
					keyBinding.keyCode = keyboardKeybinding.keyCode;
					break;
				}
			}
		}
		if (keyboardKeybindings.Count != LazyInput.GameBindings.keyBindings.Count)
		{
			FillKeyBindingsForSave(keyboardKeybindings);
		}
		ControllerIconLibrary.UpdateStandaloneIcons();
		savedGameBindingsApplied = true;
	}

	private void TryInitDefaultBindings()
	{
		if (LazyInput.IsInitialized && !savedGameBindingsApplied)
		{
			if (defaultKeyboardKeybindings.Count == 0)
			{
				FillKeyBindingsForSave(defaultKeyboardKeybindings);
			}
			else if (!IsSameBindingsAsCurrentSource(defaultKeyboardKeybindings))
			{
				MigrateSavedBindingsToCurrentDefaults();
				FillKeyBindingsForSave(defaultKeyboardKeybindings);
			}
		}
	}

	private void MigrateSavedBindingsToCurrentDefaults()
	{
		List<KeyBindingForSave> list = new List<KeyBindingForSave>();
		foreach (KeyBinding keyBinding in LazyInput.GameBindings.keyBindings)
		{
			KeyBindingForSave keyBindingForSave = FindBindingForSave(keyboardKeybindings, keyBinding.gameKey.value);
			KeyBindingForSave keyBindingForSave2 = FindBindingForSave(defaultKeyboardKeybindings, keyBinding.gameKey.value);
			bool flag = keyBindingForSave != null && keyBindingForSave2 != null && keyBindingForSave.keyCode != keyBindingForSave2.keyCode;
			KeyBindingForSave keyBindingForSave3 = new KeyBindingForSave();
			keyBindingForSave3.gameKeyValue = keyBinding.gameKey.value;
			keyBindingForSave3.keyCode = (flag ? keyBindingForSave.keyCode : keyBinding.keyCode);
			list.Add(keyBindingForSave3);
		}
		keyboardKeybindings = list;
	}

	private bool IsSameBindingsAsCurrentSource(List<KeyBindingForSave> keyBindingsForSave)
	{
		if (keyBindingsForSave.Count != LazyInput.GameBindings.keyBindings.Count)
		{
			return false;
		}
		foreach (KeyBinding keyBinding in LazyInput.GameBindings.keyBindings)
		{
			KeyBindingForSave keyBindingForSave = FindBindingForSave(keyBindingsForSave, keyBinding.gameKey.value);
			if (keyBindingForSave == null || keyBindingForSave.keyCode != keyBinding.keyCode)
			{
				return false;
			}
		}
		return true;
	}

	private KeyBindingForSave FindBindingForSave(List<KeyBindingForSave> keyBindingsForSave, int gameKeyValue)
	{
		foreach (KeyBindingForSave item in keyBindingsForSave)
		{
			if (item.gameKeyValue == gameKeyValue)
			{
				return item;
			}
		}
		return null;
	}

	private void FillKeyBindingsForSave(List<KeyBindingForSave> keyBindingsForSave)
	{
		keyBindingsForSave.Clear();
		foreach (KeyBinding keyBinding in LazyInput.GameBindings.keyBindings)
		{
			KeyBindingForSave keyBindingForSave = new KeyBindingForSave();
			keyBindingForSave.gameKeyValue = keyBinding.gameKey.value;
			keyBindingForSave.keyCode = keyBinding.keyCode;
			keyBindingsForSave.Add(keyBindingForSave);
		}
	}

	private static GameSettings LoadAndApplyPlatformDefaults()
	{
		GameSettings gameSettings = SaveSystem.LoadGameSettings();
		gameSettings.ApplyGpuDetectedDefaultGraphicsIfNeeded();
		gameSettings.ApplySteamDeckDefaultGraphicsIfNeeded();
		return gameSettings;
	}

	private void ApplyGpuDetectedDefaultGraphicsIfNeeded()
	{
		if (gpuGraphicsDefaultApplied)
		{
			Debug.Log($"[GameSettings] Graphics tier already initialized (tier {this.graphicsTier}); skipping GPU-based default detection.");
			return;
		}
		gpuGraphicsDefaultApplied = true;
		if (IsRunningOnSteamDeck())
		{
			Debug.Log("[GameSettings] Running on Steam Deck; skipping GPU-based default detection in favor of Steam Deck defaults.");
			return;
		}
		GraphicsTier graphicsTier = this.graphicsTier;
		this.graphicsTier = GpuGraphicsTierDetector.DetectDefaultGraphicsTier();
		Debug.Log($"[GameSettings] No graphics tier stored yet: default {graphicsTier} -> {this.graphicsTier} (GPU-based).");
	}

	private void ApplySteamDeckDefaultGraphicsIfNeeded()
	{
		if (!steamDeckGraphicsDefaultApplied && IsRunningOnSteamDeck())
		{
			steamDeckGraphicsDefaultApplied = true;
			if (graphicsTier == GraphicsTier.High)
			{
				graphicsTier = GraphicsTier.Medium;
			}
		}
	}

	private static bool IsRunningOnSteamDeck()
	{
		if (SteamManager.Initialized)
		{
			return SteamUtils.IsSteamRunningOnSteamDeck();
		}
		return false;
	}

	private static void ApplyPlatformSpecificRenderSettings()
	{
		RenderSettings.ambientMode = AmbientMode.Flat;
	}

	public static bool IsHBAOEnabled()
	{
		return PlatformFeatures.IsHBAOEnabled();
	}

	public int GetUnityTargetFrameRate()
	{
		return targetFrameRate switch
		{
			TargetFrameRate.Fps30 => 30, 
			TargetFrameRate.Fps60 => 60, 
			TargetFrameRate.Fps120 => 120, 
			TargetFrameRate.Unlimited => -1, 
			_ => 60, 
		};
	}

	public IntVector2 GetResolutionIntVector2()
	{
		if (resolutionConfig != null)
		{
			return new IntVector2(resolutionConfig.AppliedWidth, resolutionConfig.AppliedHeight);
		}
		if (ResolutionConfig.currentResolution != null)
		{
			return new IntVector2(ResolutionConfig.currentResolution.AppliedWidth, ResolutionConfig.currentResolution.AppliedHeight);
		}
		return new IntVector2(Screen.width, Screen.height);
	}
}
