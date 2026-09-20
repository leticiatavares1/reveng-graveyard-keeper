using LazyBearTechnology;

public class UIMixInfoWidgetData : LazyWidgetDataBase
{
	public AlchemyMixDef MixDef { get; set; }

	public UIMixInfoWidgetData(AlchemyMixDef mixDef)
	{
		MixDef = mixDef;
	}
}
