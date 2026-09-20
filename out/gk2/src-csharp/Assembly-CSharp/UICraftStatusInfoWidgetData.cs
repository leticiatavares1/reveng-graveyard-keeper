using LazyBearTechnology;
using TMPro;
using UnityEngine;

public class UICraftStatusInfoWidgetData : LazyWidgetDataBase
{
	public string Text { get; private set; }

	public TextAlignmentOptions TextAlignmentOptions { get; private set; }

	public TextStyle TextStyle { get; private set; }

	public Sprite StatusIcon { get; private set; }

	public UICraftStatusInfoWidgetData(string text, TextAlignmentOptions textAlignmentOptions, TextStyle textStyle, Sprite statusIcon)
	{
		Text = text;
		TextAlignmentOptions = textAlignmentOptions;
		TextStyle = textStyle;
		StatusIcon = statusIcon;
	}
}
