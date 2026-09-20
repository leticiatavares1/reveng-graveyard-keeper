using LazyBearTechnology;

public class CargoLiftInteractionHandler : WGOInteractionHandlerBase
{
	public override bool Interact(PlayerController interactor)
	{
		if (base.Interact(interactor))
		{
			return true;
		}
		UIPorterStationWindow window = LazyUI.GetWindow<UIPorterStationWindow>();
		UIPorterStationWindowData data = new UIPorterStationWindowData(assignedWgo.Data);
		window.Open(data);
		return true;
	}

	public override bool HasInteraction(PlayerController interactor)
	{
		return true;
	}

	protected override InteractionInfos FormInteractionInfo()
	{
		InteractionInfos interactionInfos = base.FormInteractionInfo();
		if (!interactionInfos.IsEmpty)
		{
			return interactionInfos;
		}
		InteractionInfos interactionInfos2 = new InteractionInfos();
		interactionInfos2.Add(new InteractionInfo(LocalizeHintWithActionIcon("hint_open", GameKey.Interaction)));
		if (HasInteraction2(MainGame.PlayerController))
		{
			interactionInfos2.Add(new InteractionInfo(LocalizeHintWithActionIcon("hint_put", GameKey.Action)));
		}
		return interactionInfos2;
	}
}
