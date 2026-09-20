using UnityEngine;

public class BubbleWidgetText : BubbleWidget<BubbleWidgetTextData>
{
	[HideInInspector]
	[SerializeField]
	private UILabel _label;

	public override void Init()
	{
		_label = GetComponent<UILabel>();
		base.Init();
	}

	public override void Draw(BubbleWidgetTextData data)
	{
		if (!initialized)
		{
			Init();
		}
		base.data = data;
		_label.alignment = data.alignment;
		if (data.style == UITextStyles.TextStyle.None)
		{
			_label.bitmapFont = data.font;
			_label.fontSize = data.font.defaultSize;
			_label.spacingY = data.line_spacing;
		}
		else
		{
			UITextStyles.Set(_label, data.style);
		}
		GJL.EnsureLabelHasCorrectFont(_label, do_cache: false);
		_label.text = data.text;
	}
}
