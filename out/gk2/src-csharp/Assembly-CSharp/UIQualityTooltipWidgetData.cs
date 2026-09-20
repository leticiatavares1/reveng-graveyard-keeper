using LazyBearTechnology;

public class UIQualityTooltipWidgetData : LazyWidgetDataBase
{
	public string IconName { get; private set; }

	public WgoData WgoData { get; private set; }

	public string ValueText { get; private set; }

	public bool HasValueOverride { get; private set; }

	public UIQualityTooltipWidgetData(string iconName, WgoData wgoData)
	{
		IconName = iconName;
		WgoData = wgoData;
	}

	public UIQualityTooltipWidgetData(string iconName, string valueText)
	{
		IconName = iconName;
		ValueText = valueText;
		HasValueOverride = true;
	}
}
