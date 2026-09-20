public class HpBarPlayerWidgetData : HpBarSimpleWidgetData
{
	public new float CustomWidth { get; private set; } = -1f;


	public new float CustomHeight { get; private set; } = -1f;


	public HpBarPlayerWidgetData(HPComponent hpComponent, float customWidth = -1f, float customHeight = -1f)
		: base(hpComponent)
	{
		CustomWidth = customWidth;
		CustomHeight = customHeight;
	}
}
