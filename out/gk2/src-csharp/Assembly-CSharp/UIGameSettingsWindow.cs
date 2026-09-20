using System;
using System.Collections.Generic;
using LazyBearTechnology;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

public class UIGameSettingsWindow : LazyWindow<LazyWidgetDataBase>
{
	private static readonly GameKey[] decreaseKeys = new GameKey[2]
	{
		GameKey.DecSlider,
		GameKey.Left
	};

	private static readonly GameKey[] increaseKeys = new GameKey[2]
	{
		GameKey.IncSlider,
		GameKey.Right
	};

	private const string VSYNC_OFF = "ui_off";

	private const string VSYNC_ON = "ui_on";

	private const string FPS_UNLIMITED = "ui_fps_unlimited";

	private const string VOICE_OVER = "ui_voiceover_mode";

	private const string MUMBLING = "ui_mumbling_mode";

	private const string FULLSCREEN = "ui_fullscreen";

	private const string WINDOWED = "ui_windowed";

	private const string CURSOR_HARDWARE = "ui_cursor_hardware";

	private const string CURSOR_SOFTWARE = "ui_cursor_software";

	private const string GRAPHICS_LOWEST = "ui_graphics_lowest";

	private const string GRAPHICS_LOW = "ui_graphics_low";

	private const string GRAPHICS_MEDIUM = "ui_graphics_medium";

	private const string GRAPHICS_HIGH = "ui_graphics_high";

	private const float VolumeSliderStep = 5f;

	[SerializeField]
	private UISwitchButton resolutionSwitch;

	[SerializeField]
	private UISwitchButton fullscreenButton;

	[SerializeField]
	private UISwitchButton vSyncButton;

	[SerializeField]
	private UISwitchButton fpsLockButton;

	[SerializeField]
	private UISwitchButton cursorBtn;

	[SerializeField]
	private UISwitchButton graphicsTierButton;

	[SerializeField]
	private UISwitchButton voiceOverButton;

	[SerializeField]
	private UISwitchButton languageButton;

	[SerializeField]
	private LocalizedLabel languageLocalizedLabel;

	[SerializeField]
	private TextStyleComponent languageStyleComponent;

	[SerializeField]
	private UIDialogWindowButton lazyButton;

	[SerializeField]
	private UISlider masterVolumeSlider;

	[SerializeField]
	private UISlider musicVolumeSlider;

	[SerializeField]
	private UISlider sfxVolumeSlider;

	[SerializeField]
	private UISlider speechVolumeSlider;

	[SerializeField]
	private TextStyleComponent tipsStyleComponent;

	public Action onClosed;

	private UIDialogWindowData.ButtonData btnData;

	private string[] screenModes = new string[2] { "ui_fullscreen", "ui_windowed" };

	private string[] vsyncModes = new string[2] { "ui_off", "ui_on" };

	private string[] voiceOverModes = new string[2] { "ui_voiceover_mode", "ui_mumbling_mode" };

	private string[] graphicsTierModes = new string[4] { "ui_graphics_lowest", "ui_graphics_low", "ui_graphics_medium", "ui_graphics_high" };

	private GameSettings GameSettings => GameSettings.Instance;

	public override void Init()
	{
		base.Init();
		ResolutionConfig resolutionConfig = GameSettings.resolutionConfig;
		if (resolutionConfig == null)
		{
			IntVector2 resolutionIntVector = GameSettings.GetResolutionIntVector2();
			resolutionConfig = new ResolutionConfig(resolutionIntVector.x, resolutionIntVector.y);
		}
		string[] resolutionsStringArray = ResolutionConfig.GetResolutionsStringArray();
		resolutionSwitch.Initialize(delegate(int index)
		{
			ResolutionConfig resolutionConfigByIndex = ResolutionConfig.GetResolutionConfigByIndex(index);
			GameSettings.resolutionConfig = resolutionConfigByIndex;
			GameSettings.ApplyGraphicSettings();
			((RectTransform)base.transform).RefreshContentFitter();
		}, resolutionsStringArray, Mathf.Max(0, ResolutionConfig.FindResolutionConfigIndex(resolutionConfig)), "", decreaseKeys, increaseKeys);
		fullscreenButton.Initialize(delegate(int value)
		{
			GameSettings.screenMode = (ScreenMode)value;
			GameSettings.ApplyGraphicSettings();
		}, GetLocalizedArray(screenModes), (int)GameSettings.screenMode, "", decreaseKeys, increaseKeys);
		voiceOverButton.Initialize(delegate(int value)
		{
			GameSettings.voiceOverMode = (VoiceOverMode)value;
			VoiceOverSettings.IsEnabled = value == 0;
			SaveSystem.SaveGameSettings();
			VoiceOverModePreview.Play((VoiceOverMode)value, this);
		}, GetLocalizedArray(voiceOverModes), (int)GameSettings.voiceOverMode, "", decreaseKeys, increaseKeys);
		if (DevUtils.IsDemoBitsummitActive)
		{
			voiceOverButton.gameObject.SetActive(value: false);
		}
		vSyncButton.Initialize(delegate(int value)
		{
			GameSettings.vsyncMode = (VSyncMode)value;
			GameSettings.ApplyGraphicSettings();
		}, GetLocalizedArray(vsyncModes), (int)GameSettings.vsyncMode, "", decreaseKeys, increaseKeys);
		fpsLockButton.Initialize(delegate(int value)
		{
			GameSettings.targetFrameRate = (TargetFrameRate)value;
			GameSettings.ApplyGraphicSettings();
		}, GetTargetFrameRateLabels(), (int)GameSettings.targetFrameRate, "", decreaseKeys, increaseKeys);
		cursorBtn.Initialize(delegate(int value)
		{
			GameSettings.cursorMode = (GameCursorMode)value;
			CursorController.UpdateCursorState();
			SaveSystem.SaveGameSettings();
		}, GetCursorModeLabels(), (int)GameSettings.cursorMode, "", decreaseKeys, increaseKeys);
		if (graphicsTierButton != null)
		{
			graphicsTierButton.Initialize(delegate(int value)
			{
				GameSettings.graphicsTier = FromGraphicsTierSwitchIndex(value);
				GameSettings.ApplyGraphicsTier();
			}, GetLocalizedArray(graphicsTierModes), ToGraphicsTierSwitchIndex(GameSettings.graphicsTier), "", decreaseKeys, increaseKeys, loopNavigation: false);
		}
		masterVolumeSlider.Initialize(delegate(float volume)
		{
			GameSettings.masterVolume = volume;
			GameSettings.ApplyAudioSettings();
		}, GameSettings.masterVolume, 5f);
		musicVolumeSlider.Initialize(delegate(float volume)
		{
			GameSettings.musicVolume = volume;
			GameSettings.ApplyAudioSettings();
		}, GameSettings.musicVolume, 5f);
		sfxVolumeSlider.Initialize(delegate(float volume)
		{
			GameSettings.sfxVolume = volume;
			GameSettings.ApplyAudioSettings();
		}, GameSettings.sfxVolume, 5f);
		speechVolumeSlider.Initialize(delegate(float volume)
		{
			GameSettings.speechVolume = volume;
			GameSettings.ApplyAudioSettings();
		}, GameSettings.speechVolume, 5f);
		InitLanguageButton();
	}

	public static void RefreshLanguageSwitcherIfOpen()
	{
		UIGameSettingsWindow[] array = UnityEngine.Object.FindObjectsByType<UIGameSettingsWindow>(FindObjectsInactive.Include, FindObjectsSortMode.None);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != null)
			{
				array[i].InitLanguageButton();
			}
		}
	}

	private void InitLanguageButton()
	{
		if (languageLocalizedLabel != null)
		{
			languageLocalizedLabel.IgnoreLocalize = true;
		}
		string[] availableLanguageNamesRange = LLBase.GetAvailableLanguageNamesRange();
		languageButton.Initialize(delegate(int value)
		{
			GameSettings.language = LLBase.GetAvailableLanguageInfoByIndex(value).id;
			GameSettings.ApplyLanguageSettings();
			GUIElements.Instance.UpdateLocalizedLabels();
			fullscreenButton.ReinitLabels(GetLocalizedArray(screenModes));
			vSyncButton.ReinitLabels(GetLocalizedArray(vsyncModes));
			fpsLockButton.ReinitLabels(GetTargetFrameRateLabels());
			voiceOverButton.ReinitLabels(GetLocalizedArray(voiceOverModes));
			cursorBtn.ReinitLabels(GetCursorModeLabels());
			if (graphicsTierButton != null)
			{
				graphicsTierButton.ReinitLabels(GetLocalizedArray(graphicsTierModes));
			}
			languageButton.ReinitLabels(LLBase.GetAvailableLanguageNamesRange());
			languageStyleComponent.ApplyStyle();
			btnData = new UIDialogWindowData.ButtonData(Close, LLBase.L("btn_ok"), null, replaceForGamepad: true, GameKey.Select);
			lazyButton.Draw(btnData);
			if (MainGame.Instance.gameState == MainGame.GameState.InGame)
			{
				GUIElements.Instance.WorldZoneWidget.Draw(new WorldZoneWidgetData());
			}
			TextStyleComponent[] componentsInChildren = GUIElements.Instance.GetComponentsInChildren<TextStyleComponent>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].ApplyStyle();
			}
			PrintTips();
		}, availableLanguageNamesRange, (!string.IsNullOrEmpty(GameSettings.language)) ? LLBase.GetIndexByLanguage(GameSettings.language) : 0, "", decreaseKeys, increaseKeys);
		languageButton.IsInteractable = availableLanguageNamesRange.Length > 1;
	}

	private void RefreshScreenModeSwitch(bool syncFromHardware)
	{
		if (syncFromHardware && !GameSettings.GraphicSettingsAppliedThisFrame)
		{
			GameSettings.SyncScreenModeFromHardware();
		}
		int screenMode = (int)GameSettings.screenMode;
		if (fullscreenButton.CurrentFieldIndex != screenMode)
		{
			fullscreenButton.UpdateField(screenMode, fireCallback: false);
		}
	}

	private void LateUpdate()
	{
		if (base.IsShown)
		{
			RefreshScreenModeSwitch(syncFromHardware: false);
		}
	}

	protected override void UpdateGamepadDependentStuff()
	{
		base.UpdateGamepadDependentStuff();
		((RectTransform)base.transform).RefreshContentFitter();
	}

	protected override void PrintTips()
	{
		if (LazyInput.IsGamepadActive)
		{
			lazyButtonTips.Print(LazyGameKeyTip.Back(), new LazyGameKeyTip(GameKey.DpadLeft, "-"), new LazyGameKeyTip(GameKey.DpadRight, "+"));
			if (tipsStyleComponent != null)
			{
				tipsStyleComponent.ApplyStyle();
			}
		}
		else
		{
			lazyButtonTips.Clear();
		}
	}

	protected override bool OnPressedBack()
	{
		Close();
		return true;
	}

	protected override Dictionary<GameKey, Func<bool>> GetGameKeyDelegates()
	{
		Dictionary<GameKey, Func<bool>> gameKeyDelegates = base.GetGameKeyDelegates();
		gameKeyDelegates.Add(GameKey.Select, () => false);
		gameKeyDelegates.TryAdd(GameKey.DpadLeft, () => false);
		gameKeyDelegates.TryAdd(GameKey.DpadRight, () => false);
		gameKeyDelegates.TryAdd(GameKey.Left, () => false);
		gameKeyDelegates.TryAdd(GameKey.Right, () => false);
		return gameKeyDelegates;
	}

	public override void Open(LazyWidgetDataBase data)
	{
		base.Open(data);
		InitLanguageButton();
		RefreshScreenModeSwitch(syncFromHardware: true);
		if (SteamManager.Initialized && SteamUtils.IsSteamRunningOnSteamDeck())
		{
			fullscreenButton.gameObject.SetActive(value: false);
			vSyncButton.gameObject.SetActive(value: false);
			fpsLockButton.gameObject.SetActive(value: false);
			cursorBtn.gameObject.SetActive(value: false);
			resolutionSwitch.gameObject.SetActive(value: false);
		}
		btnData = new UIDialogWindowData.ButtonData(Close, LLBase.L("btn_ok"), null, replaceForGamepad: true, GameKey.Select);
		lazyButton.Draw(btnData);
		((RectTransform)base.transform).RefreshContentFitter();
		VoiceOverModePreview.Warmup();
		PrintTips();
	}

	private static int ToGraphicsTierSwitchIndex(GraphicsTier tier)
	{
		return tier switch
		{
			GraphicsTier.Lowest => 0, 
			GraphicsTier.Low => 1, 
			GraphicsTier.Medium => 2, 
			_ => 3, 
		};
	}

	private static GraphicsTier FromGraphicsTierSwitchIndex(int index)
	{
		return index switch
		{
			0 => GraphicsTier.Lowest, 
			1 => GraphicsTier.Low, 
			2 => GraphicsTier.Medium, 
			_ => GraphicsTier.High, 
		};
	}

	private string[] GetLocalizedArray(string[] array)
	{
		string[] array2 = new string[array.Length];
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i] = LLBase.L(array[i]);
		}
		return array2;
	}

	private string[] GetTargetFrameRateLabels()
	{
		return new string[4]
		{
			"30",
			"60",
			"120",
			LLBase.L("ui_fps_unlimited")
		};
	}

	private string[] GetCursorModeLabels()
	{
		string text = LLBase.L("ui_cursor_software");
		return new string[3]
		{
			LLBase.L("ui_cursor_hardware"),
			text ?? "",
			text + " 150%"
		};
	}

	public override void Close()
	{
		base.Close();
		onClosed?.Invoke();
		onClosed = null;
	}

	protected override void TestDraw()
	{
	}
}
