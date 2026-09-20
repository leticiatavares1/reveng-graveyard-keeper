using System;

[Serializable]
public class PickupOrder : OrderBase
{
	public PickupOrder(SGuid targetWgoUniqueId, Item item)
		: base(targetWgoUniqueId, item)
	{
	}

	public override int GetPriority()
	{
		return 1;
	}

	public override void ExecuteOrder(IOrderExecutor orderExecutor)
	{
		orderExecutor.AddItem(base.Item);
		base.ZombieWgoData.CraftableObjectCraftInventory.RemoveItemById(base.Item.id, base.Item.Count);
		WgoData attachedWgoData = base.ZombieWgoData.AttachedWgoData;
		if (attachedWgoData.CraftComponent.Status == CraftComponentStatus.WaitingForWorkerPickUp)
		{
			attachedWgoData.CraftComponent.TryFinishCurCraft();
			attachedWgoData.DropStoredTechPoints();
		}
	}

	public override bool CanOrderBeExecuted(IOrderExecutor orderExecutor, out string reasonIfNot)
	{
		bool flag = orderExecutor.CanAddItem(base.Item);
		reasonIfNot = (flag ? string.Empty : "player_talk_not_enough_space");
		return flag;
	}

	public override string GetInteractionHint()
	{
		return "hint_take";
	}

	public override string GetStatusIcon()
	{
		return "craft_status_not_enough_items";
	}
}
