using UnityEngine;

namespace LazyBearTechnology;

public class LazyGameKeyTip
{
	private const string INACTIVE_TIP_COLOR = "#d7d7d7";

	private GameKey key;

	private string text;

	private bool active;

	private bool gamepadOnly;

	private bool translate;

	private static string uiSelectLocale = "Select";

	private static string uiBackLocale = "Back";

	private static bool selectAndBackLocalesInitialized;

	public static void InitSelectAndBackLocales(string selectLocale, string backLocale)
	{
		if (!selectAndBackLocalesInitialized)
		{
			selectAndBackLocalesInitialized = true;
			uiSelectLocale = selectLocale;
			uiBackLocale = backLocale;
		}
	}

	public static string Get(GameKey key, string text, bool active = true, bool gamepadOnly = true, bool translate = true)
	{
		return new LazyGameKeyTip(key, text, active, gamepadOnly, translate).ToString();
	}

	public LazyGameKeyTip(GameKey key, string text, bool active = true, bool gamepadOnly = true, bool translate = true)
	{
		this.key = key;
		this.text = text;
		this.active = active;
		this.translate = translate;
		this.gamepadOnly = gamepadOnly;
	}

	public override string ToString()
	{
		if (gamepadOnly && !LazyInput.IsGamepadActive)
		{
			return string.Empty;
		}
		string icon = GetIcon(key, active ? GameKeyIconType.Default : GameKeyIconType.Inactive);
		if (string.IsNullOrEmpty(icon))
		{
			return string.Empty;
		}
		string text = (translate ? LLBase.L(this.text) : this.text);
		return icon + (active ? (text ?? "") : ("<color=#d7d7d7>" + text + "</color>"));
	}

	public string GetIcon(GameKey key, GameKeyIconType gameKeyIconType)
	{
		return ControllerIconLibrary.GetIconId(key, gameKeyIconType, trailingSpace: false);
	}

	public static LazyGameKeyTip Select(bool active = true, bool gamepadOnly = true, bool translate = true)
	{
		if (!selectAndBackLocalesInitialized)
		{
			Debug.LogWarning("Locales for Select & Back requires initialization. Please call once InitSelectAndBackLocales");
		}
		return new LazyGameKeyTip(GameKey.Select, uiSelectLocale, active, gamepadOnly, translate);
	}

	public static LazyGameKeyTip Back(bool active = true, bool gamepadOnly = true, bool translate = true)
	{
		if (!selectAndBackLocalesInitialized)
		{
			Debug.LogWarning("Locales for Select & Back requires initialization. Please call once InitSelectAndBackLocales");
		}
		return new LazyGameKeyTip(GameKey.Back, uiBackLocale, active, gamepadOnly, translate);
	}
}
