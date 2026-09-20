using LazyBearTechnology;
using TMPro;

public class UITooltipTextWidgetData : LazyWidgetDataBase
{
	public string Text { get; private set; }

	public TextAlignmentOptions TextAlignmentOptions { get; private set; }

	public TextStyle TextStyle { get; private set; }

	public UITooltipTextWidgetData(string text, TextAlignmentOptions textAlignmentOptions, TextStyle textStyle)
	{
		Text = text;
		TextAlignmentOptions = textAlignmentOptions;
		TextStyle = textStyle;
	}
}
