using LazyBearTechnology;

public class UICraftsTabSeparatorWidget : LazyWidget<UICraftsTabSeparatorWidgetData>
{
	[LazyUITest]
	protected override void TestDraw()
	{
		Draw(new UICraftsTabSeparatorWidgetData());
	}
}
