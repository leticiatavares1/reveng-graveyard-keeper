using System;
using UnityEngine;

[Serializable]
public class ItemCountCondition : ConditionalDrawerConditionBase
{
	[Tooltip("The ID of the item to check")]
	public string itemId;

	[Tooltip("The comparison operator to use")]
	public ComparisonOperator comparison;

	[Tooltip("The target count to compare against")]
	public int targetCount;

	[Tooltip("Second target count (used for Between comparisons)")]
	public int targetCount2;

	public override ConditionalEventType EventType => ConditionalEventType.ItemsChanged;

	public override bool IsValid(ConditionalDrawerContext context)
	{
		if (base.IsValid(context) && context.Inventory != null)
		{
			return !string.IsNullOrEmpty(itemId);
		}
		return false;
	}

	public override bool Evaluate(ConditionalDrawerContext context)
	{
		int value = context.Inventory.GetItemById(itemId)?.Count ?? 0;
		return CompareValues(value, comparison, targetCount, targetCount2);
	}
}
