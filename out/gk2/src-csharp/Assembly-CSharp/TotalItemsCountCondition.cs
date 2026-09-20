using System;
using UnityEngine;

[Serializable]
public class TotalItemsCountCondition : ConditionalDrawerConditionBase
{
	[Tooltip("Whether to use percentage-based comparison instead of absolute count")]
	public bool usePercentage;

	[Tooltip("The comparison operator to use")]
	public ComparisonOperator comparison;

	[Tooltip("The target value to compare against (count or percentage)")]
	public float targetValue;

	[Tooltip("Second target value (used for Between comparisons)")]
	public float targetValue2;

	public override ConditionalEventType EventType => ConditionalEventType.ItemsChanged;

	public override bool IsValid(ConditionalDrawerContext context)
	{
		if (base.IsValid(context))
		{
			return context.Inventory != null;
		}
		return false;
	}

	public override bool Evaluate(ConditionalDrawerContext context)
	{
		int totalItemsCount = context.Inventory.Data.TotalItemsCount;
		if (!usePercentage)
		{
			return CompareValues(totalItemsCount, comparison, (int)targetValue, (int)targetValue2);
		}
		int num = CalculateTotalPossibleItemsCount(context);
		if (num == 0)
		{
			return false;
		}
		float value = (float)totalItemsCount / (float)num * 100f;
		return CompareValues(value, comparison, targetValue, targetValue2);
	}

	private int CalculateTotalPossibleItemsCount(ConditionalDrawerContext context)
	{
		int num = 0;
		foreach (Item item in context.Inventory.Data.Inventory)
		{
			num = ((!item.IsEmpty) ? (num + item.Definition.stackCount) : (num + context.WgoData.Definition.emptyCellStackCount));
		}
		if (context.Inventory.Data.Inventory.Count < context.Inventory.Data.InventorySize)
		{
			num += context.WgoData.Definition.emptyCellStackCount * (context.Inventory.Data.InventorySize - context.Inventory.Data.Inventory.Count);
		}
		return num;
	}
}
