public class WorkInteractionHandler : WGOInteractionHandlerBase
{
	public override bool HasInteraction2(PlayerController interactor)
	{
		if (MainGame.PlayerData != null && MainGame.PlayerData.HasMultipleOverheadItems)
		{
			return false;
		}
		base.HasInteraction2(interactor);
		return true;
	}

	protected override InteractionInfos FormInteractionInfo()
	{
		InteractionInfos interactionInfos = base.FormInteractionInfo();
		if (!interactionInfos.IsEmpty)
		{
			return interactionInfos;
		}
		return new InteractionInfos(GetInteractionInfoByUsingTool());
	}
}
