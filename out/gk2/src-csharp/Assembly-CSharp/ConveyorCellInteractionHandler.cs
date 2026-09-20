using LazyBearTechnology;

public class ConveyorCellInteractionHandler : WGOInteractionHandlerBase
{
	public override void OnInteractionTargetEnter(PlayerController interactor)
	{
		base.OnInteractionTargetEnter(interactor);
		assignedWgo.MainWgoPart?.SetDropViewInteractionState(isUnderInteraction: true);
	}

	public override void OnInteractionTargetExit()
	{
		assignedWgo.MainWgoPart?.SetDropViewInteractionState(isUnderInteraction: false);
		base.OnInteractionTargetExit();
	}

	public override bool Interact(PlayerController interactor)
	{
		if (base.Interact(interactor))
		{
			return true;
		}
		foreach (Item item in assignedWgo.Data.Inventory.RemoveItemById(assignedWgo.Data.Inventory.Data.Inventory[0].id))
		{
			if (item.Definition.itemSize == ItemSize.Small)
			{
				assignedWgo.Data.MakeDrop(item);
			}
			else if (!MainGame.PlayerData.TryAddOverheadItemNoReplace(item))
			{
				assignedWgo.Data.MakeDrop(item);
			}
			assignedWgo.MainWgoPart.UpdateDropViewFromInventory();
		}
		interactor.PlayerInteractionComponent.ResetInteractionState();
		return true;
	}

	public override bool HasInteraction(PlayerController interactor)
	{
		if (base.HasInteraction(interactor))
		{
			return true;
		}
		if (assignedWgo.Data.Inventory.Data.Inventory.Count > 0)
		{
			if (assignedWgo.Data.Inventory.Data.Inventory[0].Definition.itemSize != ItemSize.Small && !MainGame.PlayerData.HasFreeOverheadSlot)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	protected override InteractionInfos FormInteractionInfo()
	{
		InteractionInfos interactionInfos = base.FormInteractionInfo();
		if (!interactionInfos.IsEmpty)
		{
			return interactionInfos;
		}
		string hint;
		return new InteractionInfos(new InteractionInfo(TryGetCustomInteractionStr(out hint) ? hint : LocalizeHintWithActionIcon("hint_take", GameKey.Interaction)));
	}
}
