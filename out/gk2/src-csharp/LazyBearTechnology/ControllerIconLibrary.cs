using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using LazyBearTechnology;
using Steamworks;
using UnityEngine;

[CreateAssetMenu(fileName = "ControllerIconLibrary", menuName = "Lazy/ControllerIconLibrary", order = 1)]
public class ControllerIconLibrary : LazySingletonSO<ControllerIconLibrary>
{
	[SerializeField]
	private Color standaloneInactiveColor = new Color(215f, 215f, 215f, 255f);

	[Space]
	public List<ControllerIconData> xBoxControllerIcons = new List<ControllerIconData>();

	public List<ControllerIconData> dualShockControllerIcons = new List<ControllerIconData>();

	public List<ControllerIconData> dualSenseControllerIcons = new List<ControllerIconData>();

	public List<ControllerIconData> joyConControllerIcons = new List<ControllerIconData>();

	protected List<ControllerIconData> standaloneIcons = new List<ControllerIconData>();

	public List<BindingAlias> gameKeyIconAliases;

	protected List<ControllerIconData> currentIcons;

	private static bool forcedViewActive;

	private static ControllerIconViewType forcedView;

	[Header("KeyCode Standalone Options")]
	[Space]
	public bool useReadableKeyCodeReplacements;

	public List<KeyCodeReadableReplacement> readableReplacementsForDisplay = new List<KeyCodeReadableReplacement>();

	private Dictionary<KeyCode, KeyCodeReadableReplacement> keyCodeReadableReplacementsCache = new Dictionary<KeyCode, KeyCodeReadableReplacement>();

	public bool useSplittingForKeyCodeStr;

	private bool swapAB;

	public static bool IsViewForced => forcedViewActive;

	public static ControllerIconViewType ForcedView => forcedView;

	public static event Action<ControllerIconViewType> OnViewChanged;

	public static void SetForcedView(ControllerIconViewType view)
	{
		forcedViewActive = true;
		forcedView = view;
		LazySingletonSO<ControllerIconLibrary>.Instance.ApplyForcedView();
		ControllerIconLibrary.OnViewChanged?.Invoke(view);
		LazyButtonTipsStr.RefreshAll();
	}

	public static void ClearForcedView()
	{
		if (forcedViewActive)
		{
			ResetForcedViewRuntimeState();
			RefreshFromInput();
		}
	}

	private static void ResetForcedViewRuntimeState()
	{
		forcedViewActive = false;
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void ResetForcedViewOnSubsystemRegistration()
	{
		ResetForcedViewRuntimeState();
	}

	public static void RefreshFromInput()
	{
		switch (Platform.Type)
		{
		case PlatformType.PС:
			if (!forcedViewActive)
			{
				LazySingletonSO<ControllerIconLibrary>.Instance.UpdateInputDeviceForPC();
			}
			break;
		case PlatformType.PlayStation:
			if (!forcedViewActive)
			{
				LazySingletonSO<ControllerIconLibrary>.Instance.UpdateInputDeviceForPlaystation();
			}
			break;
		case PlatformType.Switch:
		case PlatformType.Switch2:
			if (!forcedViewActive)
			{
				LazySingletonSO<ControllerIconLibrary>.Instance.currentIcons = LazySingletonSO<ControllerIconLibrary>.Instance.joyConControllerIcons;
			}
			break;
		}
		LazyButtonTipsStr.RefreshAll();
	}

	public static string GetViewFileName(ControllerIconViewType view)
	{
		return view switch
		{
			ControllerIconViewType.Keyboard => "keyboard", 
			ControllerIconViewType.Xbox => "xbox", 
			ControllerIconViewType.DualShock => "dualshock", 
			ControllerIconViewType.DualSense => "dualsense", 
			ControllerIconViewType.JoyCon => "joycon", 
			_ => view.ToString().ToLowerInvariant(), 
		};
	}

	private void ApplyForcedView()
	{
		currentIcons = forcedView switch
		{
			ControllerIconViewType.Keyboard => standaloneIcons, 
			ControllerIconViewType.Xbox => xBoxControllerIcons, 
			ControllerIconViewType.DualShock => dualShockControllerIcons, 
			ControllerIconViewType.DualSense => dualSenseControllerIcons, 
			ControllerIconViewType.JoyCon => joyConControllerIcons, 
			_ => standaloneIcons, 
		};
	}

	public static void UpdateStandaloneIcons()
	{
		LazySingletonSO<ControllerIconLibrary>.Instance.GenerateStandaloneIcons();
		LazySingletonSO<ControllerIconLibrary>.Instance.currentIcons = LazySingletonSO<ControllerIconLibrary>.Instance.standaloneIcons;
	}

	public static void UpdateStandaloneIcons(List<KeyBinding> keyBindings)
	{
		LazySingletonSO<ControllerIconLibrary>.Instance.UpdateStandaloneIconsForBindings(keyBindings);
	}

	public static string GetIconId(GameKey key, GameKeyIconType gameKeyIconType = null, bool trailingSpace = true)
	{
		return LazySingletonSO<ControllerIconLibrary>.Instance.GetTextIcon(key, gameKeyIconType, trailingSpace);
	}

	public virtual void Init()
	{
		ResetForcedViewRuntimeState();
		InitCacheData();
		switch (Platform.Type)
		{
		case PlatformType.PС:
			GenerateStandaloneIcons();
			UpdateInputDeviceForPC();
			LazyInput.OnInputChanged += UpdateInputDeviceForPC;
			break;
		case PlatformType.XBox:
			currentIcons = xBoxControllerIcons;
			break;
		case PlatformType.PlayStation:
			UpdateInputDeviceForPlaystation();
			LazyInput.OnInputChanged += UpdateInputDeviceForPlaystation;
			break;
		case PlatformType.Switch:
		case PlatformType.Switch2:
			currentIcons = joyConControllerIcons;
			break;
		default:
			currentIcons = xBoxControllerIcons;
			break;
		}
	}

	private void InitCacheData()
	{
		foreach (KeyCodeReadableReplacement item in readableReplacementsForDisplay)
		{
			if (!keyCodeReadableReplacementsCache.TryAdd(item.keyCode, item))
			{
				Debug.LogError($"Error: KeyCode [{item.keyCode}] is already exist, skipping [{item.replacementDisplay}]");
			}
		}
	}

	private string GetIcon(GameKey key, GameKeyIconType gameKeyIconType)
	{
		if (swapAB)
		{
			GamepadBinding gamepadBinding = LazyInput.GameBindings.gamepadBindings.Find((GamepadBinding b) => b.gameKey.value == key.value);
			if (gamepadBinding != null)
			{
				if (gamepadBinding.gamepadButton.value == GamepadButton.A.value)
				{
					return GetIconTyped(GameKey.Back, gameKeyIconType);
				}
				if (gamepadBinding.gamepadButton.value == GamepadButton.B.value)
				{
					return GetIconTyped(GameKey.Select, gameKeyIconType);
				}
			}
		}
		return GetIconTyped(key, gameKeyIconType);
	}

	private string GetIconTyped(GameKey key, GameKeyIconType iconType)
	{
		if ((object)iconType == null)
		{
			iconType = GameKeyIconType.Default;
		}
		GameKey gameKey = key;
		if (currentIcons != standaloneIcons)
		{
			for (int i = 0; i < gameKeyIconAliases.Count; i++)
			{
				BindingAlias bindingAlias = gameKeyIconAliases.Find((BindingAlias a) => a.gameKey1.value == key.value);
				if (bindingAlias != null)
				{
					gameKey = bindingAlias.gameKey2;
					break;
				}
			}
		}
		foreach (ControllerIconData currentIcon in currentIcons)
		{
			if (!(currentIcon.gameKey == gameKey))
			{
				continue;
			}
			for (int j = 0; j < currentIcon.typedIcons.Length; j++)
			{
				if (currentIcon.typedIcons[j].iconType == iconType)
				{
					return currentIcon.typedIcons[j].iconId;
				}
			}
		}
		Debug.LogError("Cannot find icon for GameKey [" + Enumeration.GetNameOfStaticField<GameKey>(gameKey.value) + "], icon type [" + Enumeration.GetNameOfStaticField<GameKeyIconType>(iconType.value) + "]");
		return string.Empty;
	}

	private string GetTextIcon(GameKey key, GameKeyIconType gameKeyIconType, bool trailingSpace = true)
	{
		string icon = GetIcon(key, gameKeyIconType);
		if (icon.StartsWith("["))
		{
			return icon + (trailingSpace ? " " : "");
		}
		return "<sprite name=\"" + icon + "\">" + (trailingSpace ? " " : "");
	}

	protected void GenerateStandaloneIcons()
	{
		standaloneIcons.Clear();
		foreach (KeyBinding keyBinding in LazyInput.GameBindings.keyBindings)
		{
			string keycodeString = GetKeycodeString(keyBinding.keyCode);
			standaloneIcons.Add(new ControllerIconData(keyBinding.gameKey, new ControllerIconDataTyped[2]
			{
				new ControllerIconDataTyped(GameKeyIconType.Default, "[" + keycodeString + "]"),
				new ControllerIconDataTyped(GameKeyIconType.Inactive, "[<color=#" + ColorUtility.ToHtmlStringRGB(standaloneInactiveColor) + ">" + keycodeString + "</color>]")
			}));
		}
	}

	protected void UpdateStandaloneIconsForBindings(List<KeyBinding> keyBindings)
	{
		foreach (ControllerIconData standaloneIcon in standaloneIcons)
		{
			foreach (KeyBinding keyBinding in keyBindings)
			{
				if (standaloneIcon.gameKey == keyBinding.gameKey)
				{
					string keycodeString = GetKeycodeString(keyBinding.keyCode);
					standaloneIcon.typedIcons[0] = new ControllerIconDataTyped(GameKeyIconType.Default, "[" + keycodeString + "]");
					standaloneIcon.typedIcons[1] = new ControllerIconDataTyped(GameKeyIconType.Inactive, "[<color=#" + ColorUtility.ToHtmlStringRGB(standaloneInactiveColor) + ">" + keycodeString + "</color>]");
				}
			}
		}
	}

	protected virtual void UpdateInputDeviceForPC()
	{
		if (forcedViewActive)
		{
			ApplyForcedView();
		}
		else if (IsRunningOnSteamDeck())
		{
			currentIcons = xBoxControllerIcons;
		}
		else if (LazyInput.IsGamepadActive)
		{
			if ((object)LazyInput.CurrentGamepadType == null)
			{
				currentIcons = xBoxControllerIcons;
				return;
			}
			int value = LazyInput.CurrentGamepadType.value;
			if (value == GamepadType.Sony_DualShock.value)
			{
				currentIcons = dualShockControllerIcons;
			}
			else if (value == GamepadType.Sony_DualSense.value)
			{
				currentIcons = dualSenseControllerIcons;
			}
			else if (value == GamepadType.Switch_Handheld.value || value == GamepadType.Switch_Pro.value || value == GamepadType.Switch_JoyCon_Left.value || value == GamepadType.Switch_JoyCon_Right.value || value == GamepadType.Switch_JoyCon_Dual.value)
			{
				currentIcons = joyConControllerIcons;
			}
			else
			{
				currentIcons = xBoxControllerIcons;
			}
		}
		else
		{
			currentIcons = standaloneIcons;
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

	protected virtual void UpdateInputDeviceForPlaystation()
	{
		int num = LazyInput.CurrentGamepadType?.value ?? (-1);
		if (num == GamepadType.Sony_DualSense.value)
		{
			currentIcons = dualSenseControllerIcons;
		}
		else if (num == GamepadType.Sony_DualShock.value)
		{
			currentIcons = dualShockControllerIcons;
		}
		else
		{
			currentIcons = GetDefaultPlaystationIcons();
		}
	}

	private List<ControllerIconData> GetDefaultPlaystationIcons()
	{
		return dualShockControllerIcons;
	}

	public string GetKeycodeString(KeyCode keyCode)
	{
		string text = keyCode.ToString();
		text = text.Replace("Alpha", "");
		if (useReadableKeyCodeReplacements && keyCodeReadableReplacementsCache.TryGetValue(keyCode, out var value))
		{
			return value.replacementDisplay;
		}
		if (text == "Escape")
		{
			return "Esc";
		}
		if (useSplittingForKeyCodeStr)
		{
			return Regex.Replace(text, "([A-Z][a-z]*|\\d+)", " $1").Trim();
		}
		return text;
	}
}
