public class BubbleWidgetDarkHeader : BubbleWidget<BubbleWidgetDarkHeaderData>
{
	public UILabel hdr_label;

	public override void Init()
	{
		base.Init();
	}

	public override void Draw(BubbleWidgetDarkHeaderData data)
	{
		if (!initialized)
		{
			Init();
		}
		base.data = data;
		hdr_label.text = data.text;
	}
}
