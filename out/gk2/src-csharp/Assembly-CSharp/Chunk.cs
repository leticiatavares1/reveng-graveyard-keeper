using System.Collections.Generic;
using LazyBearTechnology;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class Chunk : IChunkableObject
{
	public BurstableBounds chunkBounds;

	public bool isVisible = true;

	public List<IChunkableObject> chunkableObjects;

	public NativeArray<BurstableBounds> chunkableDataArray;

	public NativeArray<bool> visibilityResults;

	private HashSet<IChunkableObject> chunkableObjectsSet;

	public int Count => chunkableObjects.Count;

	public MultiFlagOR<ChunkingIgnoreType> IgnoreMultiFlag { get; set; }

	public bool IgnoreChunkVisibility => false;

	public Chunk(float3 position, float3 size)
	{
		chunkBounds = new BurstableBounds(position, size);
		chunkableObjects = new List<IChunkableObject>(100);
		chunkableObjectsSet = new HashSet<IChunkableObject>(100);
	}

	public void AddChunkableObject(IChunkableObject chunkableObject)
	{
		chunkableObjects.Add(chunkableObject);
		chunkableObjectsSet.Add(chunkableObject);
	}

	public void RemoveChunkableObject(IChunkableObject chunkableObject)
	{
		chunkableObjects.Remove(chunkableObject);
		chunkableObjectsSet.Remove(chunkableObject);
	}

	public void SetChunkableObjects(List<IChunkableObject> newChunkableObjects)
	{
		chunkableObjects = newChunkableObjects;
		chunkableObjectsSet = new HashSet<IChunkableObject>(chunkableObjects);
	}

	public void ClearChunkableObjects()
	{
		chunkableObjects.Clear();
		chunkableObjectsSet.Clear();
	}

	public void AddChunkableObjects(List<IChunkableObject> newChunkableObjects)
	{
		chunkableObjects.AddRange(newChunkableObjects);
		chunkableObjectsSet.UnionWith(newChunkableObjects);
	}

	public void RemoveChunkableObjects(List<IChunkableObject> removeChunkableObjects)
	{
		chunkableObjects.RemoveAll(removeChunkableObjects.Contains);
		chunkableObjectsSet.ExceptWith(removeChunkableObjects);
	}

	public bool ContainsChunkableObject(IChunkableObject chunkableObject)
	{
		return chunkableObjectsSet.Contains(chunkableObject);
	}

	public void Dispose()
	{
		if (chunkableDataArray.IsCreated)
		{
			chunkableDataArray.Dispose();
		}
		if (visibilityResults.IsCreated)
		{
			visibilityResults.Dispose();
		}
	}

	public void EnsureCapacity()
	{
		if (chunkableObjects.Count != chunkableDataArray.Length)
		{
			Dispose();
			chunkableDataArray = new NativeArray<BurstableBounds>(chunkableObjects.Count, Allocator.Persistent);
			visibilityResults = new NativeArray<bool>(chunkableObjects.Count, Allocator.Persistent);
		}
	}

	public BurstableBounds GetChunkableData()
	{
		return chunkBounds;
	}

	public void UpdateChunkVisibility(bool isVisible)
	{
		for (int i = 0; i < chunkableObjects.Count; i++)
		{
			IChunkableObject chunkableObject = chunkableObjects[i];
			if (chunkableObject != null && (!(chunkableObject is Object @object) || !(@object == null)))
			{
				chunkableObject.UpdateChunkVisibility(isVisible);
			}
		}
	}
}
