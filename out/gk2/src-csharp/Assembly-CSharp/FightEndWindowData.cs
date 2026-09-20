using LazyBearTechnology;

public class FightEndWindowData : LazyWidgetDataBase
{
	public FightDef FightDefinition { get; private set; }

	public FightEndWindowData(FightDef fightDefinition)
	{
		FightDefinition = fightDefinition;
	}
}
