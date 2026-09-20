using LazyBearTechnology;

public class UISurveyResultWindowData : LazyWidgetDataBase
{
	public ItemDef ItemDef { get; private set; }

	public UISurveyResultWindowData(ItemDef itemDef)
	{
		ItemDef = itemDef;
	}
}
