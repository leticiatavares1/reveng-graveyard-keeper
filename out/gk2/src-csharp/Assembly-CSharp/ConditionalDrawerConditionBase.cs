using System;
using UnityEngine;

[Serializable]
public abstract class ConditionalDrawerConditionBase : IConditionalDrawerCondition
{
	public abstract ConditionalEventType EventType { get; }

	public abstract bool Evaluate(ConditionalDrawerContext context);

	public virtual bool IsValid(ConditionalDrawerContext context)
	{
		return context?.WgoData != null;
	}

	protected bool CompareValues(float value, ComparisonOperator op, float target, float target2 = 0f)
	{
		return op switch
		{
			ComparisonOperator.Less => value < target, 
			ComparisonOperator.LessOrEqual => value <= target, 
			ComparisonOperator.Equal => Mathf.Approximately(value, target), 
			ComparisonOperator.GreaterOrEqual => value >= target, 
			ComparisonOperator.Greater => value > target, 
			ComparisonOperator.Between => value >= target && value <= target2, 
			ComparisonOperator.BetweenExcludeLeft => value > target && value <= target2, 
			ComparisonOperator.BetweenExcludeRight => value >= target && value < target2, 
			ComparisonOperator.BetweenExcludeBoth => value > target && value < target2, 
			_ => false, 
		};
	}

	protected bool CompareValues(int value, ComparisonOperator op, int target, int target2 = 0)
	{
		return op switch
		{
			ComparisonOperator.Less => value < target, 
			ComparisonOperator.LessOrEqual => value <= target, 
			ComparisonOperator.Equal => value == target, 
			ComparisonOperator.GreaterOrEqual => value >= target, 
			ComparisonOperator.Greater => value > target, 
			ComparisonOperator.Between => value >= target && value <= target2, 
			ComparisonOperator.BetweenExcludeLeft => value > target && value <= target2, 
			ComparisonOperator.BetweenExcludeRight => value >= target && value < target2, 
			ComparisonOperator.BetweenExcludeBoth => value > target && value < target2, 
			_ => false, 
		};
	}
}
