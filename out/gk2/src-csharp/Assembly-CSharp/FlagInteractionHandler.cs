using LazyBearTechnology;

public class FlagInteractionHandler : WGOInteractionHandlerBase
{
	private string ItemId => assignedWgo.Data.id + "_item";

	public override bool HasInteraction(PlayerController interactor)
	{
		if (AgentsGroupFlagController.IsInteractionLocked(assignedWgo))
		{
			return false;
		}
		if (!interactor.PlayerData.Inventory.CanAddItemToInventory(ItemId, 1))
		{
			return false;
		}
		return true;
	}

	public override bool Interact(PlayerController interactor)
	{
		if (base.Interact(interactor))
		{
			return true;
		}
		AgentsGroupFlagController componentInChildren = assignedWgo.GetComponentInChildren<AgentsGroupFlagController>();
		if ((bool)componentInChildren)
		{
			componentInChildren.Init();
			assignedWgo.UpdateFlag(ChunkingIgnoreType.Fighting, newValue: true);
			LazySingleton<FightingGameController>.Instance.AllDynamicObjectsInZone.Add(assignedWgo);
			LazySingleton<FightingGameController>.Instance.customFlagControllers.Add(componentInChildren);
		}
		interactor.AttachTheFlag(assignedWgo);
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
		interactionInfos2.Add(new InteractionInfo(LocalizeHintWithActionIcon("hint_take", GameKey.Interaction)));
		interactionInfos2.Add(new InteractionInfo("", "icon-arrow_flag-take"));
		return interactionInfos2;
	}
}
