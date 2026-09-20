using System;
using UnityEngine;

[Serializable]
public class TownBuildingObjectConfiguration
{
	[Tooltip("Берется из баланса таба WGOs")]
	public string wgoId;

	[Tooltip("Глобальная позиция на сцене")]
	public Vector3 position;

	[Tooltip("Scale объекта на сцене")]
	public Vector3 scale = Vector3.one;

	[HideInInspector]
	public SGuid createdWgoUniqueId = SGuid.Empty;
}
