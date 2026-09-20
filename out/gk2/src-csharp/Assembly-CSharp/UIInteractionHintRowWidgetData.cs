using LazyBearTechnology;

public class UIInteractionHintRowWidgetData : LazyWidgetDataBase
{
	public InteractionInfo InteractionInfo { get; private set; }

	public UIInteractionHintRowWidgetData(InteractionInfo interactionInfo)
	{
		InteractionInfo = interactionInfo;
	}
}
