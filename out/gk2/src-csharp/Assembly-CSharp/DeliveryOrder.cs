using System;

[Serializable]
public class DeliveryOrder : OrderBase
{
	public DeliveryOrder(SGuid targetWgoUniqueId, Item item)
		: base(targetWgoUniqueId, item)
	{
	}

	public override void ExecuteOrder(IOrderExecutor orderExecutor)
	{
		orderExecutor.RemoveItem(base.Item);
		base.ZombieWgoData.CraftableObjectCraftInventory.AddItemToInventory(new Item(base.Item.id, base.Item.Count));
	}

	public override bool CanOrderBeExecuted(IOrderExecutor orderExecutor, out string reasonIfNot)
	{
		bool flag = orderExecutor.HasItem(base.Item);
		reasonIfNot = (flag ? string.Empty : "player_talk_not_enough_items");
		return flag;
	}

	public override string GetInteractionHint()
	{
		return "hint_put";
	}

	public override string GetStatusIcon()
	{
		return "craft_status_wait";
	}
}
