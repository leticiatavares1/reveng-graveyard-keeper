using System;
using UnityEngine;

[Serializable]
public class BuildingModeCondition : ConditionalDrawerConditionBase
{
	[Tooltip("Building mode condition")]
	public BuildingModeConditionType conditionType;

	public override ConditionalEventType EventType => ConditionalEventType.BuildingModeChanged;

	public override bool IsValid(ConditionalDrawerContext context)
	{
		return true;
	}

	public override bool Evaluate(ConditionalDrawerContext context)
	{
		return conditionType switch
		{
			BuildingModeConditionType.IsActive => context.IsBuildingModeActive, 
			BuildingModeConditionType.IsNotActive => !context.IsBuildingModeActive, 
			_ => false, 
		};
	}
}
