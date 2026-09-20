using UnityEngine;

public class BubbleWidgetTextData : BubbleWidgetData
{
	public enum Font
	{
		Header,
		MicroFont,
		SmallFont,
		SmallFontBold,
		TinyFont
	}

	public string text;

	public UIFont font;

	public NGUIText.Alignment alignment = NGUIText.Alignment.Center;

	public int line_spacing;

	public UITextStyles.TextStyle style;

	public int max_width = -1;

	public BubbleWidgetTextData(string text, Font font, NGUIText.Alignment alignment = NGUIText.Alignment.Center, int line_spacing = -4, int max_width = -1)
	{
		this.text = text;
		this.font = Resources.Load<UIFont>("ngui_fonts/" + font switch
		{
			Font.Header => "header", 
			Font.MicroFont => "micro_font", 
			Font.SmallFontBold => "small_font_bold", 
			Font.TinyFont => "tiny_font", 
			_ => "small_font", 
		});
		this.alignment = alignment;
		this.line_spacing = line_spacing;
		style = UITextStyles.TextStyle.None;
		this.max_width = max_width;
	}

	public BubbleWidgetTextData(string text, UITextStyles.TextStyle style = UITextStyles.TextStyle.Usual, NGUIText.Alignment alignment = NGUIText.Alignment.Center, int max_width = -1)
	{
		this.text = text;
		this.style = style;
		this.alignment = alignment;
		this.max_width = max_width;
	}

	public override bool IsEmpty()
	{
		return string.IsNullOrEmpty(text);
	}

	public override void TrySetAlign(NGUIText.Alignment alignment)
	{
		this.alignment = alignment;
	}
}
