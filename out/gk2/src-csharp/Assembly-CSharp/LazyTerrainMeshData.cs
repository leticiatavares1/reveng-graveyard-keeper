using System;
using LazyBearTechnology;
using UnityEngine;

[Serializable]
public class LazyTerrainMeshData : IChunkableObject
{
	public Mesh mesh;

	public Vector3 localPosition;

	public Vector3 center;

	public Bounds bounds;

	public DeformingGrassData deformingGrassData;

	public bool hasGrass;

	[NonSerialized]
	public LazyTerrain lazyTerrain;

	private bool isVisible = true;

	private BurstableBounds chunkBounds;

	private LazyTerrainMesh meshView;

	private DeformingGrass grassView;

	public MultiFlagOR<ChunkingIgnoreType> IgnoreMultiFlag { get; set; }

	public bool IgnoreChunkVisibility => false;

	public LazyTerrainMeshData(Mesh mesh, Vector3 localPosition, Vector3 center, Bounds bounds)
	{
		this.mesh = mesh;
		this.localPosition = localPosition;
		this.center = center;
		this.bounds = bounds;
	}

	public void Init(LazyTerrain lazyTerrain)
	{
		this.lazyTerrain = lazyTerrain;
		chunkBounds = new BurstableBounds(localPosition + lazyTerrain.transform.position, new Vector3(7f, 0f, 7f) * ((float)lazyTerrain.meshSize / 5f));
	}

	public BurstableBounds GetChunkableData()
	{
		return chunkBounds;
	}

	public void UpdateChunkVisibility(bool isVisible)
	{
		if (this.isVisible == isVisible)
		{
			return;
		}
		this.isVisible = isVisible;
		if (isVisible)
		{
			meshView = LazyTerrainMeshPool.GetMesh();
			if (hasGrass)
			{
				grassView = LazyTerrainMeshPool.GetGrass();
				meshView.DrawFromData(this, lazyTerrain, grassView);
			}
			else
			{
				meshView.DrawFromData(this, lazyTerrain);
			}
			return;
		}
		if (hasGrass)
		{
			LazyTerrainMeshPool.ReleaseGrass(grassView);
			grassView = null;
		}
		LazyTerrainMeshPool.ReleaseMesh(meshView);
		meshView = null;
	}
}
