using UnityEngine;

public enum WorkConditionType
{
	[Tooltip("Player is currently working on this WGO")]
	IsInWork,
	[Tooltip("Player is not working on this WGO")]
	IsNotInWork,
	[Tooltip("WGO has an assigned worker")]
	HasWorker,
	[Tooltip("WGO does not have an assigned worker")]
	HasNoWorker
}
