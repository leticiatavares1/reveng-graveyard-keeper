using UnityEngine;

[ExecuteInEditMode]
public class UITextStyles : MonoBehaviour
{
	public enum TextStyle
	{
		None = -1,
		Usual,
		InteractionHint,
		DialogText,
		HintTitle,
		TinyDescription,
		QualityHint
	}

	private static UITextStyles _instance;

	public UILabel[] style_labels;

	public static UITextStyles me => _instance ?? (_instance = Object.FindObjectOfType<UITextStyles>());

	public static void Set(UILabel some_label, TextStyle style)
	{
		if (style != TextStyle.None)
		{
			UILabel uILabel = me.style_labels[(int)style];
			some_label.bitmapFont = uILabel.bitmapFont;
			some_label.fontSize = uILabel.fontSize;
			some_label.color = uILabel.color;
			some_label.effectStyle = uILabel.effectStyle;
			some_label.effectColor = uILabel.effectColor;
			some_label.spacingY = uILabel.spacingY;
			some_label.effectDistance = uILabel.effectDistance;
			if (uILabel.overflowMethod != UILabel.Overflow.ResizeFreely)
			{
				some_label.width = uILabel.width;
			}
		}
	}
}
