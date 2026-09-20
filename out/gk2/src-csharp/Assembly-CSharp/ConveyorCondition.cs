using System;
using UnityEngine;

[Serializable]
public class ConveyorCondition : ConditionalDrawerConditionBase
{
	[Tooltip("The type of conveyor condition to check")]
	public ConveyorConditionType conditionType;

	[Tooltip("The direction to check (as integer value of Direction enum)")]
	public int direction;

	[Tooltip("Should check if connection leads to in(parent) element or out(child) element")]
	public bool requiresInOutFlag;

	[Tooltip("True - Connection leads to in(parent) element. False - Connection leads to out(child) element")]
	public bool isIn;

	public override ConditionalEventType EventType => ConditionalEventType.ConveyorChanged;

	public override bool IsValid(ConditionalDrawerContext context)
	{
		if (base.IsValid(context))
		{
			return context.ConveyorComponent != null;
		}
		return false;
	}

	public override bool Evaluate(ConditionalDrawerContext context)
	{
		bool flag = HasConnectedWgoToDirection(context);
		return conditionType switch
		{
			ConveyorConditionType.HasConnectedToDirection => flag, 
			ConveyorConditionType.HasNotConnectedToDirection => !flag, 
			_ => false, 
		};
	}

	private bool HasConnectedWgoToDirection(ConditionalDrawerContext context)
	{
		foreach (Direction occupiedConnectorsDirection in context.ConveyorComponent.occupiedConnectorsDirections)
		{
			if (occupiedConnectorsDirection == (Direction)direction)
			{
				if (!requiresInOutFlag)
				{
					return true;
				}
				return isIn ? context.ConveyorComponent.HasParentsInDirection((Direction)direction) : context.ConveyorComponent.HasChildsInDirection((Direction)direction);
			}
		}
		return false;
	}
}
