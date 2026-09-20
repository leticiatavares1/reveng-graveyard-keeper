using System;
using UnityEngine;

[Serializable]
public class TemporaryObjectCondition : ConditionalDrawerConditionBase
{
	[Tooltip("Temporary object condition")]
	public TemporaryObjectConditionType conditionType;

	public override ConditionalEventType EventType => ConditionalEventType.None;

	public override bool Evaluate(ConditionalDrawerContext context)
	{
		bool flag = context.WgoData != null && context.WgoData.isTempObject;
		return conditionType switch
		{
			TemporaryObjectConditionType.IsTemporary => flag, 
			TemporaryObjectConditionType.IsNotTemporary => !flag, 
			_ => false, 
		};
	}
}
