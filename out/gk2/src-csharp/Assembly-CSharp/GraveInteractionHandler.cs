using LazyBearTechnology;

public class GraveInteractionHandler : WGOInteractionHandlerBase
{
	private CraftComponent assignedCraftComponent;

	public override bool Interact(PlayerController interactor)
	{
		if (base.Interact(interactor))
		{
			return true;
		}
		LazyUI.GetWindow<UIGraveWindow>().Open(new UIGraveWindowData(assignedWgo));
		return true;
	}

	public override bool HasInteraction(PlayerController interactor)
	{
		if (base.HasInteraction(interactor))
		{
			return true;
		}
		if (assignedWgo.Data.CraftComponent.IsStarted)
		{
			return false;
		}
		return true;
	}

	public override bool HasInteraction2(PlayerController interactor)
	{
		if (base.HasInteraction2(interactor))
		{
			return true;
		}
		if (!assignedWgo.Data.CraftComponent.IsStarted)
		{
			return false;
		}
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
		return new InteractionInfos(new InteractionInfo(TryGetCustomInteractionStr(out hint) ? hint : LocalizeHintWithActionIcon("hint_grave", GameKey.Interaction)));
	}
}
