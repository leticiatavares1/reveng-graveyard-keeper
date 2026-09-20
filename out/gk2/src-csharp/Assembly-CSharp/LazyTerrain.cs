using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class LazyTerrain : MonoBehaviour
{
	public const float Y_OFFSET = -0.005f;

	public int width = 100;

	public int height = 100;

	public int centerHor;

	public int centerVert;

	public int meshSize = 5;

	public Vector2 meshTileSize = new Vector2(0.96f, 1.2f);

	public Material material;

	public LazyTerrainConfig lazyTerrainConfig;

	public List<LazyTerrainMesh> meshes = new List<LazyTerrainMesh>();

	public List<LazyTerrainMeshData> meshesData = new List<LazyTerrainMeshData>();

	public bool castShadows;

	private void Awake()
	{
		if (Application.isPlaying && meshesData.Count > 0)
		{
			for (int i = 0; i < meshesData.Count; i++)
			{
				meshesData[i].Init(this);
			}
			LazySingleton<ChunkManager>.Instance.RegisterChunks(meshesData, ChunkManagerLayerType.StaticObjects);
		}
	}

	private void OnDestroy()
	{
		if (!Application.isPlaying || meshesData.Count == 0 || GameShutdown.IsQuitting)
		{
			return;
		}
		foreach (LazyTerrainMeshData meshesDatum in meshesData)
		{
			meshesDatum.UpdateChunkVisibility(isVisible: false);
		}
		LazySingleton<ChunkManager>.Instance.UnregisterChunks(meshesData, ChunkManagerLayerType.StaticObjects);
	}
}
