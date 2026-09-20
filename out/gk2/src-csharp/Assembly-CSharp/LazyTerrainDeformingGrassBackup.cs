using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LazyTerrainDeformingGrassBackup", menuName = "GK2/LazyTerrain/Deforming Grass Backup", order = 2)]
public class LazyTerrainDeformingGrassBackup : ScriptableObject
{
	public string terrainGameObjectName;

	public string sceneName;

	public List<LazyTerrainDeformingGrassTileBackup> tiles = new List<LazyTerrainDeformingGrassTileBackup>();
}
