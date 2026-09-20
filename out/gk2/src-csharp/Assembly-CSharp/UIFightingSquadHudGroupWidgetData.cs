using LazyBearTechnology;

public class UIFightingSquadHudGroupWidgetData : LazyWidgetDataBase
{
	public FightingLevel FightingLevel { get; private set; }

	public UIFightingSquadHudGroupWidgetData(FightingLevel fightingLevel)
	{
		FightingLevel = fightingLevel;
	}
}
