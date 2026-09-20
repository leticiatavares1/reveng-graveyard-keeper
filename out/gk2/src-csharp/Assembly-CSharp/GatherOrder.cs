using System;

[Serializable]
public class GatherOrder : OrderBase
{
	public GatherOrder(SGuid targetWgoUniqueId, Item item)
		: base(targetWgoUniqueId, item)
	{
	}

	public override void ExecuteOrder(IOrderExecutor orderExecutor)
	{
	}

	public override bool CanOrderBeExecuted(IOrderExecutor orderExecutor, out string reasonIfNot)
	{
		bool flag = true;
		reasonIfNot = (flag ? string.Empty : "player_talk_not_enough_items");
		return flag;
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
