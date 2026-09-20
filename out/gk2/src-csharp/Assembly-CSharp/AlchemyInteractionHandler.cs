using LazyBearTechnology;

public class AlchemyInteractionHandler : WGOInteractionHandlerBase
{
	public override bool Interact(PlayerController interactor)
	{
		if (base.Interact(interactor))
		{
			return true;
		}
		if (!assignedWgo.Data.CraftComponent.IsStarted)
		{
			UIAlchemyWindowData data = new UIAlchemyWindowData(assignedWgo);
			LazyUI.GetWindow<UIAlchemyWindow>().Open(data);
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
			return new InteractionInfos(GetInteractionInfoByUsingTool());
		}
		string hint;
		return new InteractionInfos(new InteractionInfo(TryGetCustomInteractionStr(out hint) ? hint : LocalizeHintWithActionIcon("hint_alchemy", GameKey.Interaction)));
	}
}
