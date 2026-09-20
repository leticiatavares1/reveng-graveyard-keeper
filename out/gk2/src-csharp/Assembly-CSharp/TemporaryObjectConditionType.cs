using UnityEngine;

public enum TemporaryObjectConditionType
{
	[Tooltip("WGO is a temporary preview object (e.g. build pointer)")]
	IsTemporary,
	[Tooltip("WGO is a real placed object")]
	IsNotTemporary
}
