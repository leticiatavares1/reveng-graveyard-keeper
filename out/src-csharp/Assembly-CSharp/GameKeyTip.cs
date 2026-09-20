using System.Collections.Generic;
using UnityEngine;

public class GameKeyTip
{
	private static Dictionary<GameKey, GamePadButton> _gamepad_bindings = KeyBindings.gamepad_bindings;

	private static Dictionary<GameKey, KeyCode[]> _keyboard_bindings = KeyBindings.keyboard_bindings;

	private static Dictionary<GameKey, GameKey[]> _mouse_bindings = KeyBindings.mouse_bindings;

	private GameKey _key;

	private string _text;

	private bool _active;

	private bool _gamepad_only;

	private bool _translate;

	private bool _prev_newline;

	private static string _prefix = "";

	public static string Get(GameKey key, string text, bool active = true, bool gamepad_only = false, bool translate = true, bool prev_newline = false)
	{
		return new GameKeyTip(key, text, active, gamepad_only, translate, prev_newline).ToString();
	}

	public GameKeyTip(GameKey key, string text, bool active = true, bool gamepad_only = true, bool translate = true, bool prev_newline = false)
	{
		_key = key;
		_text = text;
		_active = active;
		_gamepad_only = gamepad_only;
		_translate = translate;
		_prev_newline = prev_newline;
	}

	public override string ToString()
	{
		if (_gamepad_only && !LazyInput.gamepad_active)
		{
			return "";
		}
		string icon = GetIcon(_key);
		if (string.IsNullOrEmpty(icon))
		{
			return "";
		}
		icon += (_translate ? GJL.L(_text) : _text);
		if (!_active)
		{
			return "[ffffff55]" + icon + "[-]";
		}
		return icon;
	}

	public static void SetPlatformPrefix(string prefix)
	{
		_prefix = prefix;
	}

	public static string GetPlatformPrefix()
	{
		return _prefix;
	}

	public static string GetIcon(GameKey key)
	{
		string text = "";
		char c = '(';
		char c2 = ')';
		if (LazyInput.gamepad_active)
		{
			switch (key)
			{
			case GameKey.Move:
				return "(LS) ";
			case GameKey.MapCursor:
				return "(RS) ";
			case GameKey.AnyQuickslot:
				return "(" + _prefix + "DPD)";
			}
			text = ((!_gamepad_bindings.ContainsKey(key)) ? "" : _gamepad_bindings[key].ToString());
		}
		else
		{
			c = '[';
			c2 = ']';
			switch (key)
			{
			case GameKey.Move:
				return "WASD";
			case GameKey.AnyQuickslot:
				return "1-4";
			}
			if (_keyboard_bindings.ContainsKey(key))
			{
				KeyCode[] array = _keyboard_bindings[key];
				text = ((array.Length == 0) ? "" : array[0].ToString().Replace("Alpha", ""));
			}
			if (_mouse_bindings.ContainsKey(key))
			{
				return c + GJL.L(key.ToString()) + c2 + " ";
			}
		}
		if (!string.IsNullOrEmpty(text))
		{
			return c + _prefix + text + c2 + " ";
		}
		return "";
	}

	public static GameKeyTip Back(string text, bool active = true, bool gamepad_only = true, bool translate = true)
	{
		return new GameKeyTip(GameKey.Back, text, active, gamepad_only, translate);
	}

	public static GameKeyTip Back(bool active = true, bool gamepad_only = true, bool translate = true)
	{
		return new GameKeyTip(GameKey.Back, "back", active, gamepad_only, translate);
	}

	public static GameKeyTip Close(bool active = true, bool gamepad_only = true, bool translate = true)
	{
		return new GameKeyTip(GameKey.Back, "close", active, gamepad_only, translate);
	}

	public static GameKeyTip Select(string text, bool active = true, bool gamepad_only = true, bool translate = true)
	{
		return new GameKeyTip(GameKey.Select, text, active, gamepad_only, translate);
	}

	public static GameKeyTip Select(bool active = true, bool gamepad_only = true, bool translate = true)
	{
		return new GameKeyTip(GameKey.Select, "select", active, gamepad_only, translate);
	}

	public static GameKeyTip Option1(string text, bool active = true, bool gamepad_only = true, bool translate = true)
	{
		return new GameKeyTip(GameKey.Option1, text, active, gamepad_only, translate);
	}

	public static GameKeyTip Option2(string text, bool active = true, bool gamepad_only = true, bool translate = true)
	{
		return new GameKeyTip(GameKey.Option2, text, active, gamepad_only, translate);
	}

	public static GameKeyTip LeftStick(bool active = true, bool gamepad_only = true, bool translate = true)
	{
		return new GameKeyTip(GameKey.Move, "move_tip", active, gamepad_only, translate);
	}

	public static GameKeyTip RightStick(bool active = true, bool gamepad_only = true, bool translate = true, string text = "cursor_tip")
	{
		return new GameKeyTip(GameKey.MapCursor, text, active, gamepad_only, translate);
	}
}
