using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

[CreateAssetMenu(fileName = "LazyTerrainSurfaceConfiguration", menuName = "GK2/LazyTerrainSurfaceConfiguration", order = 0)]
public class LazyTerrainSurfaceConfiguration : LazySingletonSO<LazyTerrainSurfaceConfiguration>
{
	[Serializable]
	private class TerrainGroupSurfacePair
	{
		public string group;

		public SurfaceType surfaceType;
	}

	[SerializeField]
	private List<TerrainGroupSurfacePair> surfacePairs;

	public SurfaceType GetSurfaceTypeByGroupId(string group)
	{
		return surfacePairs.Find((TerrainGroupSurfacePair p) => p.group == group)?.surfaceType ?? SurfaceType.None;
	}
}
