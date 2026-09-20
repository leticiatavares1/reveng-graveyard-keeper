using LazyBearTechnology;

public class ResurrectionInteractionHandler : WGOInteractionHandlerBase
{
	public override bool Interact(PlayerController interactor)
	{
		if (base.Interact(interactor))
		{
			return true;
		}
		if (assignedWgo.Data.CraftComponent.IsDestroyingCraftActive)
		{
			return false;
		}
		if (TryGetInsertableCorpseOverhead(out var corpseItem))
		{
			MainGame.PlayerData.InsertOverheadItemTo(assignedWgo.Data, corpseItem);
			interactor.PlayerInteractionComponent.ResetInteractionState();
			return true;
		}
		if (HasTakableZombieOverhead() && assignedWgo.Data.GetGameResInt("resurrection_prepared") == 0)
		{
			Item itemByGroupId = assignedWgo.Data.Inventory.GetItemByGroupId("zombie");
			if (itemByGroupId == null || itemByGroupId.IsEmpty)
			{
				return false;
			}
			PlayerData playerData = MainGame.PlayerData;
			if (!playerData.HasFreeOverheadSlot)
			{
				return false;
			}
			playerData.TryAddOverheadItemNoReplace(itemByGroupId);
			assignedWgo.Data.Inventory.RemoveItemFromInventoryByUID(itemByGroupId);
			interactor.PlayerInteractionComponent.ResetInteractionState();
			return true;
		}
		if (assignedWgo.Data.GetGameResInt("resurrection_prepared") == 0)
		{
			LazyUI.GetWindow<UIResurrectionWindow>().Open(new UIResurrectionWindowData(assignedWgo.Data));
			return true;
		}
		return false;
	}

	public override bool HasInteraction(PlayerController interactor)
	{
		if (base.HasInteraction(interactor))
		{
			return true;
		}
		if (HasInsertableCorpseOverhead())
		{
			return true;
		}
		if (HasTakableZombieOverhead() && assignedWgo.Data.GetGameResInt("resurrection_prepared") == 0)
		{
			return true;
		}
		if (assignedWgo.Data.GetGameResInt("resurrection_prepared") == 0)
		{
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
		InteractionInfos interactionInfos2 = new InteractionInfos();
		if (assignedWgo.Data.CraftComponent.IsDestroyingCraftActive)
		{
			interactionInfos2.Add(GetInteractionInfoByUsingTool(isForCurrentCraft: true));
			return interactionInfos2;
		}
		if (HasInsertableCorpseOverhead())
		{
			interactionInfos2.Add(new InteractionInfo(LocalizeHintWithActionIcon("hint_place_body", GameKey.Interaction)));
			return interactionInfos2;
		}
		if (HasTakableZombieOverhead() && assignedWgo.Data.GetGameResInt("resurrection_prepared") == 0)
		{
			interactionInfos2.Add(new InteractionInfo(LocalizeHintWithActionIcon("hint_take", GameKey.Interaction)));
			return interactionInfos2;
		}
		if (assignedWgo.Data.GetGameResInt("resurrection_prepared") == 0)
		{
			interactionInfos2.Add(new InteractionInfo(LocalizeHintWithActionIcon("action_inspect", GameKey.Interaction)));
			return interactionInfos2;
		}
		return interactionInfos2;
	}

	private bool HasInsertableCorpseOverhead()
	{
		Item corpseItem;
		return TryGetInsertableCorpseOverhead(out corpseItem);
	}

	private bool TryGetInsertableCorpseOverhead(out Item corpseItem)
	{
		corpseItem = null;
		if (interactor == null)
		{
			return false;
		}
		if (!assignedWgo.Data.Inventory.GetItemByGroupId("body").IsEmpty)
		{
			return false;
		}
		return interactor.PlayerData.TryGetOverheadItem((Item item) => item.Definition.itemGroupIds.Contains("corpse"), out corpseItem);
	}

	private bool HasTakableZombieOverhead()
	{
		if (interactor != null && interactor.PlayerData.HasFreeOverheadSlot)
		{
			return !assignedWgo.Data.Inventory.GetItemByGroupId("zombie").IsEmpty;
		}
		return false;
	}
}
