using System;

[Serializable]
public class PlantOrder : OrderBase
{
	public bool isStarGroupItem;

	public PlantOrder(SGuid targetWgoUniqueId, Item item, bool isStarGroupItem = false)
		: base(targetWgoUniqueId, item)
	{
		this.isStarGroupItem = isStarGroupItem;
	}

	public override void ExecuteOrder(IOrderExecutor orderExecutor)
	{
	}

	public override int GetPriority()
	{
		return 10;
	}

	public override bool CanOrderBeExecuted(IOrderExecutor orderExecutor, out string reasonIfNot)
	{
		bool flag = orderExecutor.HasItem(base.Item);
		reasonIfNot = (flag ? string.Empty : "player_talk_not_enough_items");
		return true;
	}

	public override string GetInteractionHint()
	{
		return string.Empty;
	}

	public override string GetStatusIcon()
	{
		return string.Empty;
	}
}
