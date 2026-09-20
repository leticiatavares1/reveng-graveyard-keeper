using System;
using UnityEngine;

[Serializable]
public class ZombieCaretakerStateCondition : ConditionalDrawerConditionBase
{
	[Tooltip("Caretaker state condition")]
	public CaretakerStateConditionType conditionType;

	public override ConditionalEventType EventType => ConditionalEventType.CaretakerStateChanged;

	public override bool Evaluate(ConditionalDrawerContext context)
	{
		return conditionType switch
		{
			CaretakerStateConditionType.IsOnStation => context.IsZombieCareTakerOnStation, 
			CaretakerStateConditionType.IsNotOnStation => !context.IsZombieCareTakerOnStation, 
			_ => false, 
		};
	}
}
