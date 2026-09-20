using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemGroupCountCondition : ConditionalDrawerConditionBase
{
	[Tooltip("The group ID of items to check")]
	public string itemGroupId;

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
			return !string.IsNullOrEmpty(itemGroupId);
		}
		return false;
	}

	public override bool Evaluate(ConditionalDrawerContext context)
	{
		if (context.Inventory == null)
		{
			Debug.LogError("[ItemGroupCountCondition]: Inventory is null");
			return false;
		}
		List<Item> itemsByGroupId = context.Inventory.GetItemsByGroupId(itemGroupId);
		int num = 0;
		foreach (Item item in itemsByGroupId)
		{
			num += item.Count;
		}
		return CompareValues(num, comparison, targetCount, targetCount2);
	}
}
