using System;
using UnityEngine;

[Serializable]
public class CraftProgressCondition : ConditionalDrawerConditionBase
{
	[Tooltip("The comparison operator to use")]
	public ComparisonOperator comparison;

	[Tooltip("The target progress value (0-1)")]
	[Range(0f, 1f)]
	public float targetProgress;

	[Tooltip("Second target progress value (used for Between comparisons)")]
	[Range(0f, 1f)]
	public float targetProgress2;

	public override ConditionalEventType EventType => ConditionalEventType.CraftProgressChanged;

	public override bool IsValid(ConditionalDrawerContext context)
	{
		if (base.IsValid(context))
		{
			return context.CraftComponent != null;
		}
		return false;
	}

	public override bool Evaluate(ConditionalDrawerContext context)
	{
		float value = context.CraftComponent.CurrentCraftElement?.ProgressTimeNormalized ?? 0f;
		return CompareValues(value, comparison, targetProgress, targetProgress2);
	}
}
