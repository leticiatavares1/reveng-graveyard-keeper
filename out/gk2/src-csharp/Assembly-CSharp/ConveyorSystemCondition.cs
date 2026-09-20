using System;
using UnityEngine;

[Serializable]
public class ConveyorSystemCondition : ConditionalDrawerConditionBase
{
	[Tooltip("The type of conveyor system condition to check")]
	public ConveyorSystemConditionType conditionType;

	public override ConditionalEventType EventType => ConditionalEventType.ConveyorSystemChanged;

	public override bool Evaluate(ConditionalDrawerContext context)
	{
		bool hasEnoughPower = MainGame.Instance.conveyorSystem.HasEnoughPower;
		if (conditionType == ConveyorSystemConditionType.HasEnoughPower)
		{
			return hasEnoughPower;
		}
		return false;
	}
}
