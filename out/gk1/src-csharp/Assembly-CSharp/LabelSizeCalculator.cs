using System.Collections.Generic;
using UnityEngine;

public class LabelSizeCalculator : MonoBehaviour
{
	private static Dictionary<UIFont, LabelSizeCalculator> _calculators = new Dictionary<UIFont, LabelSizeCalculator>();

	[HideInInspector]
	public UILabel label;

	private UILabel _last_proceed_label;

	private void Start()
	{
		label = GetComponent<UILabel>();
		UIFont bitmapFont = label.bitmapFont;
		if (bitmapFont == null)
		{
			Debug.LogError("No font", this);
		}
		else if (!_calculators.ContainsKey(bitmapFont))
		{
			_calculators.Add(bitmapFont, this);
		}
		else
		{
			Debug.LogError("Label size calculator for " + bitmapFont?.ToString() + " already exists", this);
		}
	}

	private Vector2 Proceed(UILabel target_label, string text)
	{
		if (_last_proceed_label != target_label)
		{
			_last_proceed_label = target_label;
			label.width = target_label.width;
			label.height = target_label.height;
			label.fontSize = target_label.fontSize;
			label.alignment = target_label.alignment;
			label.overflowMethod = target_label.overflowMethod;
			label.overflowWidth = target_label.overflowWidth;
			label.useFloatSpacing = target_label.useFloatSpacing;
			label.spacingX = target_label.spacingX;
			label.spacingY = target_label.spacingY;
		}
		GJL.EnsureLabelHasCorrectFont(label);
		label.text = text;
		label.ProcessText();
		return label.localSize;
	}

	public static Vector2 Calc(UILabel target_label, string text)
	{
		if (_calculators.ContainsKey(target_label.bitmapFont))
		{
			return _calculators[target_label.bitmapFont].Proceed(target_label, text);
		}
		Debug.LogError("No label size calculator for " + target_label.bitmapFont, target_label);
		return Vector2.zero;
	}

	public static void ApplyLanguageChange()
	{
		foreach (KeyValuePair<UIFont, LabelSizeCalculator> calculator in _calculators)
		{
			GJL.EnsureLabelHasCorrectFont(calculator.Value.label);
		}
	}
}
