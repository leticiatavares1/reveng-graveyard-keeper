using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LazyTerrainDeformingGrassTileBackup
{
	public int tileX;

	public int tileY;

	public Mesh grassMesh;

	public List<Vector3> coords = new List<Vector3>();

	public DeformingGrassAtlas atlas;
}
