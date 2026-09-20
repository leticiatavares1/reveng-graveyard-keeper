using LazyBearTechnology;

public class StarQualityWidgetData : LazyWidgetDataBase
{
	public CraftParamsData CraftParams { get; private set; }

	public StarQualityWidgetData(CraftParamsData craftParams)
	{
		CraftParams = craftParams;
	}
}
