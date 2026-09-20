using LazyBearTechnology;

public class ChoirPlaceInteractionHandler : WGOInteractionHandlerBase
{
	private bool HasInventorySpace => assignedWgo.Data.Inventory.Data.InventorySize > assignedWgo.Data.Inventory.Data.InventoryFillSize;

	private bool CanAddToInventory(Item item)
	{
		return assignedWgo.Data.Inventory.CanAddItemToInventory(item);
	}

	public override bool Interact(PlayerController interactor)
	{
		if (base.Interact(interactor))
		{
			return true;
		}
		if (TryGetInsertableZombieOverhead(out var zombieItem) && CanAddToInventory(zombieItem))
		{
			InsertOverheadItem(zombieItem);
			assignedWgo.Data.WorldZoneData.NotifyWgoDataChanged();
		}
		else if (HasInventorySpace)
		{
			Bubble.Talk(new PhraseData(isPlayer: true, null, "zombie_supplier_station_no_overhead", null, null, SpeechBubbleType.Think));
		}
		return true;
	}

	public override bool Interact2(PlayerController interactor)
	{
		if (base.Interact2(interactor))
		{
			return true;
		}
		Item itemByGroupId = assignedWgo.Data.Inventory.GetItemByGroupId("zombie");
		if (itemByGroupId != null && itemByGroupId.Count > 0)
		{
			Item itemByGroupId2 = assignedWgo.Data.Inventory.GetItemByGroupId("zombie");
			if (!interactor.PlayerData.TryAddOverheadItemNoReplace(itemByGroupId2))
			{
				return true;
			}
			assignedWgo.Data.Inventory.RemoveItemFromInventoryByUID(itemByGroupId2, 1);
			assignedWgo.Data.WorldZoneData.NotifyWgoDataChanged();
		}
		return true;
	}

	public override bool HasInteraction(PlayerController interactor)
	{
		if ((TryGetInsertableZombieOverhead(out var zombieItem) && CanAddToInventory(zombieItem)) || HasInventorySpace)
		{
			return true;
		}
		return false;
	}

	public override bool HasInteraction2(PlayerController interactor)
	{
		if (interactor != null && assignedWgo.Data.Inventory.GetItemsByGroupId("zombie").Count > 0 && interactor.PlayerData.HasFreeOverheadSlot)
		{
			return true;
		}
		return false;
	}

	private void InsertOverheadItem(Item zombieItem)
	{
		if (zombieItem != null)
		{
			MainGame.Instance.GameSave.playerData.RemoveOverheadItem(zombieItem);
			assignedWgo.Data.Inventory.AddItemToInventory(zombieItem);
			LinkZombie(zombieItem);
		}
	}

	protected override InteractionInfos FormInteractionInfo()
	{
		InteractionInfos interactionInfos = base.FormInteractionInfo();
		if (!interactionInfos.IsEmpty)
		{
			return interactionInfos;
		}
		InteractionInfos interactionInfos2 = new InteractionInfos();
		if ((TryGetInsertableZombieOverhead(out var zombieItem) && CanAddToInventory(zombieItem)) || HasInventorySpace)
		{
			interactionInfos2.Add(new InteractionInfo(LocalizeHintWithActionIcon("hint_put_zombie", GameKey.Interaction)));
		}
		Item itemByGroupId = assignedWgo.Data.Inventory.GetItemByGroupId("zombie");
		if (itemByGroupId != null && itemByGroupId.Count > 0 && MainGame.PlayerData.HasFreeOverheadSlot)
		{
			interactionInfos2.Add(new InteractionInfo(LocalizeHintWithActionIcon("hint_take", GameKey.Action)));
		}
		return interactionInfos2;
	}

	private void LinkZombie(Item sourceZombieItem)
	{
		ZombieWgoData zombie = MainGame.ZombieSystemData.GetZombie(sourceZombieItem.UniqueId);
		if (zombie != null)
		{
			Item itemByUniqueId = assignedWgo.Data.Inventory.GetItemByUniqueId(sourceZombieItem.UniqueId.Id);
			if (!itemByUniqueId.IsEmpty)
			{
				zombie.SetZombieItem(itemByUniqueId);
			}
		}
	}
}
