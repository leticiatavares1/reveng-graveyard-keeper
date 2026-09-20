using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TownBuildingSceneConfiguration
{
	[Tooltip("Место где стоит за прилавком персонаж")]
	public string gdPointTent;

	[Tooltip("Место куда идет персонаж от прилавка когда надо домой")]
	public string gdPointHomeOutside;

	[Tooltip("Место куда телепортируется персонаж когда дошел до gdPointHomeOutside")]
	public string gdPointHomeInside;

	public List<TownBuildingTierSceneConfiguration> tierDataList = new List<TownBuildingTierSceneConfiguration>();
}
