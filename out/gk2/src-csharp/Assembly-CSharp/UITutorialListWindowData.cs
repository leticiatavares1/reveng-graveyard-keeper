using LazyBearTechnology;

public class UITutorialListWindowData : LazyWidgetDataBase
{
	public UITutorialListOpenSource OpenSource { get; }

	public UITutorialListWindowData(UITutorialListOpenSource openSource)
	{
		OpenSource = openSource;
	}
}
