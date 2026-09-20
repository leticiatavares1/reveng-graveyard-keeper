using System.Collections.Generic;
using UnityEngine;

public class LocalizedLabel : MonoBehaviour
{
	public enum TextColor
	{
		Default,
		Tutorial,
		SpeechBubble,
		Task
	}

	public string token = "";

	public TextColor text_color;

	private string _lng_token = "";

	private UILabel _label;

	private bool _initialized;

	[Space(15f)]
	public string prefix = "";

	public GamePadButton gamepad_button_prefix;

	[Space(15f)]
	public List<GameKey> replace_params_with_keys = new List<GameKey>();

	private int _remembered_line_spacing;

	[Space(10f)]
	public bool modify_line_spacing_for_gamepad;

	public int gamepad_line_spacing = -2;

	public static string ColorizeTags(string s, TextColor color)
	{
		string text = "FFFFFF";
		switch (color)
		{
		case TextColor.Tutorial:
		case TextColor.Task:
			text = "FFBD00";
			break;
		case TextColor.SpeechBubble:
			text = "bb5c1c";
			break;
		}
		if (s.Contains("<") && s.Contains(">"))
		{
			s = s.Replace("<", "[c][" + text + "]").Replace(">", "[-][/c]");
		}
		return s;
	}

	public void Localize()
	{
		if (!_initialized)
		{
			_label = GetComponent<UILabel>();
			_remembered_line_spacing = _label.spacingY;
			_initialized = true;
			_lng_token = token;
			if (string.IsNullOrEmpty(_lng_token))
			{
				_lng_token = _label.text;
			}
		}
		if (string.IsNullOrEmpty(_lng_token))
		{
			Debug.LogError("LocalizedLabel token is empty", base.gameObject);
			return;
		}
		string text = prefix + PlatformSpecific.GetGamepadButtonSymbol(gamepad_button_prefix) + ColorizeTags(GJL.L(_lng_token), text_color);
		int num = 0;
		foreach (GameKey replace_params_with_key in replace_params_with_keys)
		{
			num++;
			text = text.Replace("%" + num, GameKeyTip.GetIcon(replace_params_with_key));
		}
		_label.text = text;
		if (modify_line_spacing_for_gamepad)
		{
			_label.spacingY = (LazyInput.gamepad_active ? gamepad_line_spacing : _remembered_line_spacing);
		}
		GJL.EnsureLabelHasCorrectFont(_label);
	}
}
