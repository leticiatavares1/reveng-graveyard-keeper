using System;

[Serializable]
public class ConveyorPickupOrder : OrderBase
{
	public WgoData TargetWgoData => MainGame.WorldData.GetWgoData(base.TargetWgoUniqueId);

	public ConveyorPickupOrder(SGuid targetWgoUniqueId, Item item)
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
		WgoData targetWgoData = TargetWgoData;
		if (targetWgoData != null)
		{
			targetWgoData.Inventory.RemoveItemById(base.Item.id, base.Item.Count);
			Wgo wgoViewGlobal = GameScene.GetWgoViewGlobal(targetWgoData.UniqueId);
			if (wgoViewGlobal != null && wgoViewGlobal.MainWgoPart != null)
			{
				wgoViewGlobal.MainWgoPart.UpdateDropViewFromInventory();
			}
		}
	}

	public override bool CanOrderBeExecuted(IOrderExecutor orderExecutor, out string reasonIfNot)
	{
		if (!orderExecutor.CanAddItem(base.Item))
		{
			reasonIfNot = "player_talk_not_enough_space";
			return false;
		}
		WgoData targetWgoData = TargetWgoData;
		if (targetWgoData == null || targetWgoData.Inventory.Data.GetTotalCountInInventory(base.Item.id) < base.Item.Count)
		{
			reasonIfNot = "player_talk_not_enough_items";
			return false;
		}
		reasonIfNot = string.Empty;
		return true;
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
