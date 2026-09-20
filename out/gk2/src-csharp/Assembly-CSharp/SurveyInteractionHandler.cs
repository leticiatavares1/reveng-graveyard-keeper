using LazyBearTechnology;

public class SurveyInteractionHandler : WGOInteractionHandlerBase
{
	public override bool Interact(PlayerController interactor)
	{
		if (base.Interact(interactor))
		{
			return true;
		}
		if (!assignedWgo.Data.CraftComponent.IsStarted)
		{
			assignedWgo.Data.TrySetWorker(interactor);
			UIResourceBasedCraftWindow window = LazyUI.GetWindow<UIResourceBasedCraftWindow>();
			UIResourceBasedCraftWindowData data = new UIResourceBasedCraftWindowData(assignedWgo.Data);
			window.Open(data);
		}
		return true;
	}

	public override bool HasInteraction(PlayerController interactor)
	{
		base.HasInteraction(interactor);
		return true;
	}

	protected override InteractionInfos FormInteractionInfo()
	{
		InteractionInfos interactionInfos = base.FormInteractionInfo();
		if (!interactionInfos.IsEmpty)
		{
			return interactionInfos;
		}
		if (assignedWgo.Data.CraftComponent.IsStarted)
		{
			return new InteractionInfos(new InteractionInfo(base.WorkHint));
		}
		string hint;
		return new InteractionInfos(new InteractionInfo(TryGetCustomInteractionStr(out hint) ? hint : LocalizeHintWithActionIcon("hint_survey", GameKey.Interaction)));
	}
}
