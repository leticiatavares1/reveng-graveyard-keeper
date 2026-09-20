using LazyBearTechnology;

public class UISquadHudElementWidgetData : LazyWidgetDataBase
{
	public WgoData Fighter { get; private set; }

	public UISquadHudElementWidgetData(WgoData fighter)
	{
		Fighter = fighter;
	}
}
