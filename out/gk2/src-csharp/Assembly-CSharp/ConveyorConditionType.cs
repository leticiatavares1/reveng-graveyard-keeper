using UnityEngine;

public enum ConveyorConditionType
{
	[Tooltip("Has a connected cell to the specified direction")]
	HasConnectedToDirection,
	[Tooltip("Does not have a connected cell to the specified direction")]
	HasNotConnectedToDirection
}
