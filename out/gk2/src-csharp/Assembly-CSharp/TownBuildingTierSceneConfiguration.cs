using System;
using UnityEngine;

[Serializable]
public class TownBuildingTierSceneConfiguration
{
	[Tooltip("Палатка")]
	public TownBuildingObjectConfiguration tent = new TownBuildingObjectConfiguration();

	[Tooltip("Вывеска")]
	public TownBuildingObjectConfiguration sign = new TownBuildingObjectConfiguration();

	[Tooltip("Двор")]
	public TownBuildingObjectConfiguration yard = new TownBuildingObjectConfiguration();

	[Tooltip("Декор №1")]
	public TownBuildingObjectConfiguration decor1 = new TownBuildingObjectConfiguration();

	[Tooltip("Декор №2")]
	public TownBuildingObjectConfiguration decor2 = new TownBuildingObjectConfiguration();

	[Tooltip("Декор №3")]
	public TownBuildingObjectConfiguration decor3 = new TownBuildingObjectConfiguration();
}
