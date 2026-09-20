using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LazyBearTechnology;

[RequireComponent(typeof(LayoutElement))]
[RequireComponent(typeof(ContentSizeFitter))]
[RequireComponent(typeof(TextMeshProUGUI))]
public class LabelSizeCalculator : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI label;

	[SerializeField]
	private RectTransform rectTransform;

	[SerializeField]
	private LayoutElement layout;

	private static LabelSizeCalculator instance;

	private void Awake()
	{
		instance = this;
	}

	private Vector2 CalcRenderedTextValues(TextMeshProUGUI refLabel, string text, float maxWidth)
	{
		layout.preferredWidth = maxWidth;
		label.text = text;
		label.font = refLabel.font;
		label.fontSize = refLabel.fontSize;
		label.isRightToLeftText = refLabel.isRightToLeftText;
		LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
		label.ForceMeshUpdate();
		Vector2 renderedValues = label.GetRenderedValues();
		if (!(renderedValues.x > maxWidth))
		{
			return renderedValues;
		}
		return new Vector2(maxWidth, renderedValues.y);
	}

	public static float CalculateFitWidth(TextMeshProUGUI label, string text, float maxWidth)
	{
		return CalculateFitVector(label, text, maxWidth).x;
	}

	public static Vector2 CalculateFitVector(TextMeshProUGUI label, string text, float maxWidth)
	{
		return instance.CalcRenderedTextValues(label, text, maxWidth);
	}
}
