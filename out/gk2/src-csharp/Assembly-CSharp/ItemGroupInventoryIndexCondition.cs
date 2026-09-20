using System;
using UnityEngine;

[Serializable]
public class ItemGroupInventoryIndexCondition : ConditionalDrawerConditionBase
{
	[Tooltip("The group ID of items to check")]
	public string itemGroupId;

	[Tooltip("The expected inventory index (0-based)")]
	public int expectedIndex;

	public override ConditionalEventType EventType => ConditionalEventType.ItemsChanged;

	public override bool IsValid(ConditionalDrawerContext context)
	{
		if (base.IsValid(context) && context.Inventory != null)
		{
			return !string.IsNullOrEmpty(itemGroupId);
		}
		return false;
	}

	public override bool Evaluate(ConditionalDrawerContext context)
	{
		Item itemByGroupId = context.Inventory.GetItemByGroupId(itemGroupId);
		if (itemByGroupId == null)
		{
			return false;
		}
		return context.Inventory.Data.Inventory.IndexOf(itemByGroupId) == expectedIndex;
	}
}
