using System;
using UnityEngine;

[Serializable]
public class GameResCondition : ConditionalDrawerConditionBase
{
	[Tooltip("The ID of the game resource to check")]
	public string gameResId;

	[Tooltip("The comparison operator to use")]
	public ComparisonOperator comparison;

	[Tooltip("The target value to compare against")]
	public float targetValue;

	[Tooltip("Second target value (used for Between comparisons)")]
	public float targetValue2;

	public override ConditionalEventType EventType => ConditionalEventType.GameResChanged;

	public override bool IsValid(ConditionalDrawerContext context)
	{
		if (base.IsValid(context))
		{
			return !string.IsNullOrEmpty(gameResId);
		}
		return false;
	}

	public override bool Evaluate(ConditionalDrawerContext context)
	{
		float value = context.WgoData.GetGameResInt(gameResId);
		return CompareValues(value, comparison, targetValue, targetValue2);
	}
}
