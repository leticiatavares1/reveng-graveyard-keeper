using LazyBearTechnology;

public class ScriptInteractionHandler : WGOInteractionHandlerBase
{
	public override bool Interact(PlayerController interactor)
	{
		if (base.Interact(interactor))
		{
			return true;
		}
		assignedWgo.FireEvent("interaction");
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
		string hint;
		return new InteractionInfos(new InteractionInfo(TryGetCustomInteractionStr(out hint) ? hint : LocalizeHintWithActionIcon("hint_interact", GameKey.Interaction)));
	}
}
