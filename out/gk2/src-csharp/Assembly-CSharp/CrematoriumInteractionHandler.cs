using LazyBearTechnology;

public class CrematoriumInteractionHandler : CraftInteractionHandler
{
	public override bool Interact(PlayerController interactor)
	{
		if (!TryGetInsertableOverheadCorpse(out var overheadItem))
		{
			return false;
		}
		assignedWgo.Data.Inventory.AddItemToInventory(overheadItem);
		MainGame.Instance.GameSave.playerData.RemoveOverheadItem(overheadItem);
		CraftComponent craftComponent = assignedWgo.Data.CraftComponent;
		CraftDefBase craftDefBase = craftComponent.AvailableCrafts[0];
		craftComponent.TryStartCraft(new CraftElement(craftParamsData: new CraftParamsData(craftDefBase.id, assignedWgo.Data), craftId: craftDefBase.id, count: 1));
		MainGame.PlayerData.SubRes("cur_bodies_count", 1f);
		if (MainGame.PlayerData.CurrentWorldZoneData != null && MainGame.PlayerData.CurrentWorldZoneData.Definition.id == "morgue")
		{
			GUIElements.Instance.WorldZoneWidget.Draw(new WorldZoneWidgetData());
		}
		assignedWgo.DrawWidgets();
		return true;
	}

	public override bool HasInteraction(PlayerController interactor)
	{
		if (assignedWgo != null && assignedWgo.Data != null && assignedWgo.Data.CraftComponent != null && !assignedWgo.Data.CraftComponent.IsStarted && assignedWgo.Data.CraftComponent.Status != CraftComponentStatus.ReadyToFinishAutoCraft && !HasCorpseItemInside(out var _) && HasInsertableOverheadBodyItem())
		{
			assignedCraftComponent = assignedWgo.Data.CraftComponent;
			return true;
		}
		return false;
	}

	public override bool HasInteraction2(PlayerController interactor)
	{
		if (assignedWgo != null && assignedWgo.Data != null && assignedWgo.Data.CraftComponent != null && assignedWgo.Data.CraftComponent.Status == CraftComponentStatus.ReadyToFinishAutoCraft)
		{
			assignedCraftComponent = assignedWgo.Data.CraftComponent;
			return base.HasInteraction2(interactor);
		}
		return false;
	}

	protected override InteractionInfos FormInteractionInfo()
	{
		InteractionInfos interactionInfos = new InteractionInfos();
		if (assignedWgo != null && assignedWgo.Data != null && assignedWgo.Data.CraftComponent != null)
		{
			assignedCraftComponent = assignedWgo.Data.CraftComponent;
			if (!assignedWgo.Data.CraftComponent.IsStarted && assignedWgo.Data.CraftComponent.Status != CraftComponentStatus.ReadyToFinishAutoCraft && !HasCorpseItemInside(out var _) && HasInsertableOverheadBodyItem())
			{
				interactionInfos.Add(new InteractionInfo(LocalizeHintWithActionIcon("hint_cremate_body", GameKey.Interaction)));
			}
			else if (assignedWgo.Data.CraftComponent.Status == CraftComponentStatus.ReadyToFinishAutoCraft)
			{
				interactionInfos.Add(new InteractionInfo(LocalizeHintWithActionIcon("hint_take_all", GameKey.Action)));
			}
			else
			{
				interactionInfos = base.FormInteractionInfo();
			}
		}
		else
		{
			interactionInfos = base.FormInteractionInfo();
		}
		return interactionInfos;
	}

	private bool HasInsertableOverheadBodyItem()
	{
		if (!HasCorpseItemInside(out var bodyItem))
		{
			return TryGetInsertableOverheadCorpse(out bodyItem);
		}
		return false;
	}

	private bool TryGetInsertableOverheadCorpse(out Item overheadItem)
	{
		return MainGame.Instance.GameSave.playerData.TryGetOverheadItem((Item item) => !item.HasItemsByItemType(ItemType.Demon) && item.Definition.itemGroupIds.Contains("corpse"), out overheadItem);
	}

	private bool HasCorpseItemInside(out Item bodyItem)
	{
		if (assignedWgo.Data.Inventory.Data.TryGetItemInInventoryByGroupId("corpse", out bodyItem))
		{
			return true;
		}
		bodyItem = null;
		return false;
	}
}
