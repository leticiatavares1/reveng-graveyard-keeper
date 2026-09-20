using LazyBearTechnology;

public class TeleportMilestoneInteractionHandler : WGOInteractionHandlerBase
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
		if (base.HasInteraction(interactor))
		{
			return true;
		}
		if (MainGame.PlayerController.PlayerData.GetResInt("milestones_activated") <= 0)
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
		return new InteractionInfos(new InteractionInfo(LocalizeHintWithActionIcon("ui_use", GameKey.Interaction)));
	}
}
