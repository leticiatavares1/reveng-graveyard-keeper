using LazyBearTechnology;

public class UIModulesLimitsWidgetData : LazyWidgetDataBase
{
	public int ModulesLimit;

	public int ModulesCount;

	public UIModulesLimitsWidgetData()
	{
	}

	public UIModulesLimitsWidgetData(int modulesCount, int modulesLimit)
	{
		ModulesCount = modulesCount;
		ModulesLimit = modulesLimit;
	}

	public UIModulesLimitsWidgetData(BuildingDef buildingDef)
	{
		ModulesCount = buildingDef.currentLimitExpression.EvaluateInt();
		ModulesLimit = buildingDef.limitMax;
	}
}
