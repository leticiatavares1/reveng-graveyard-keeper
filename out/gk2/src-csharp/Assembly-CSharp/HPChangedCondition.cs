using System;
using UnityEngine;

[Serializable]
public class HPChangedCondition : ConditionalDrawerConditionBase
{
	[Tooltip("Whether to use percentage-based comparison instead of absolute count")]
	public bool usePercentage;

	[Tooltip("The comparison operator to use")]
	public ComparisonOperator comparison;

	[Tooltip("The target value to compare against (count or percentage)")]
	public float targetValue;

	[Tooltip("Second target value (used for Between comparisons)")]
	public float targetValue2;

	public override ConditionalEventType EventType => ConditionalEventType.HPChanged;

	public override bool IsValid(ConditionalDrawerContext context)
	{
		if (base.IsValid(context) && context.WgoData != null)
		{
			return context.WgoData.HpComponent != null;
		}
		return false;
	}

	public override bool Evaluate(ConditionalDrawerContext context)
	{
		int hp = context.WgoData.HpComponent.Hp;
		int maxHpValue = context.WgoData.HpComponent.MaxHpValue;
		if (!usePercentage)
		{
			return CompareValues(hp, comparison, (int)targetValue, (int)targetValue2);
		}
		float value = (float)hp / (float)maxHpValue * 100f;
		return CompareValues(value, comparison, targetValue, targetValue2);
	}
}
