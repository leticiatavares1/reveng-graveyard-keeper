using LazyBearTechnology;

public class WellUpgradeInteractionHandler : WorkInteractionHandler
{
	public override bool HasInteraction(PlayerController interactor)
	{
		if (base.HasInteraction(interactor))
		{
			return true;
		}
		if (assignedWgo.Data.Inventory.Data.GetTotalCountInInventory("water") >= 1)
		{
			return true;
		}
		return false;
	}

	public override bool Interact(PlayerController interactor)
	{
		if (base.Interact(interactor))
		{
			return true;
		}
		assignedWgo.Data.MakeDrop(assignedWgo.Data.Inventory.Data.RemoveItemFromInventoryById("water"));
		return true;
	}

	protected override InteractionInfos FormInteractionInfo()
	{
		InteractionInfos interactionInfos = new InteractionInfos();
		if (HasInteraction(MainGame.PlayerController))
		{
			interactionInfos.Add(new InteractionInfo(LocalizeHintWithActionIcon("hint_take_water", GameKey.Interaction)));
		}
		if (HasInteraction2(MainGame.PlayerController))
		{
			interactionInfos.Add(new InteractionInfo(LocalizeHintWithActionIcon("hint_draw_water", GameKey.Action)));
		}
		return interactionInfos;
	}
}
