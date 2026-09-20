using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;

public class ChunkManagerLayer
{
	public ChunkManagerLayerType layerType;

	public Chunk[] chunks;

	public NativeArray<BurstableBounds> chunkDataArray;

	public NativeArray<byte> chunkVisibilityStateResults;

	public float expandFactor = 1.05f;

	public float prewarmPlanePadding = 1f;

	private int count;

	private Dictionary<float2, Chunk> chunkByCoords = new Dictionary<float2, Chunk>();

	public readonly HashSet<IChunkableObject> prewarmedObjects = new HashSet<IChunkableObject>();

	public readonly List<IChunkableObject> dynamicUpdateBuffer = new List<IChunkableObject>(256);

	public readonly List<IChunkableObject> objectsLeavingVisibleBuffer = new List<IChunkableObject>(256);

	public readonly List<IChunkableObject> visibleCandidatesBuffer = new List<IChunkableObject>(256);

	public readonly List<IChunkableObject> prewarmCandidatesBuffer = new List<IChunkableObject>(256);

	public readonly HashSet<IChunkableObject> currentlyVisibleObjectsBuffer = new HashSet<IChunkableObject>();

	public readonly HashSet<IChunkableObject> currentlyPrewarmedObjectsBuffer = new HashSet<IChunkableObject>();

	public readonly HashSet<IChunkableObject> objectsToHideBuffer = new HashSet<IChunkableObject>();

	public readonly ChunkVisibilityJobBuffer dynamicBatchJobBuffer = new ChunkVisibilityJobBuffer();

	public readonly ChunkVisibilityJobBuffer visibleJobBuffer = new ChunkVisibilityJobBuffer();

	public readonly ChunkVisibilityJobBuffer prewarmJobBuffer = new ChunkVisibilityJobBuffer();

	public bool IsDynamic
	{
		get
		{
			ChunkManagerLayerType chunkManagerLayerType = layerType;
			return chunkManagerLayerType == ChunkManagerLayerType.DynamicWgo || chunkManagerLayerType == ChunkManagerLayerType.DropView;
		}
	}

	public int Count => count;

	public ChunkManagerLayer(ChunkManagerLayerType layerType)
	{
		this.layerType = layerType;
		count = 0;
		chunks = new Chunk[count];
	}

	public void AddChunk(Chunk chunk)
	{
		Chunk[] array = new Chunk[count + 1];
		Array.Copy(chunks, array, count);
		float2 key = new float2(chunk.chunkBounds.center.x, chunk.chunkBounds.center.z);
		chunkByCoords[key] = chunk;
		array[count] = chunk;
		chunks = array;
		count = array.Length;
	}

	public void RemoveChunk(Chunk chunk)
	{
		chunk.Dispose();
		float2 key = new float2(chunk.chunkBounds.center.x, chunk.chunkBounds.center.z);
		chunkByCoords.Remove(key);
		int num = Array.IndexOf(chunks, chunk);
		Chunk[] array = new Chunk[count - 1];
		for (int i = 0; i < count - 1; i++)
		{
			array[i] = chunks[(i >= num) ? (i + 1) : i];
		}
		chunks = array;
		count = array.Length;
	}

	public void AddChunks(List<Chunk> newChunks)
	{
		Chunk[] array = new Chunk[count + newChunks.Count];
		Array.Copy(chunks, array, count);
		for (int i = 0; i < newChunks.Count; i++)
		{
			Chunk chunk = newChunks[i];
			array[count + i] = chunk;
			float2 key = new float2(chunk.chunkBounds.center.x, chunk.chunkBounds.center.z);
			chunkByCoords[key] = chunk;
		}
		chunks = array;
		count = array.Length;
	}

	public void RemoveChunks(List<Chunk> removeChunks)
	{
		foreach (Chunk removeChunk in removeChunks)
		{
			removeChunk.Dispose();
			float2 key = new float2(removeChunk.chunkBounds.center.x, removeChunk.chunkBounds.center.z);
			chunkByCoords.Remove(key);
		}
		Chunk[] array = new Chunk[count - removeChunks.Count];
		int num = 0;
		for (int i = 0; i < count; i++)
		{
			Chunk chunk = chunks[i];
			if (!removeChunks.Contains(chunk))
			{
				array[num++] = chunk;
			}
		}
		chunks = array;
		count = array.Length;
	}

	public Chunk FindChunk(float3 chunkPos)
	{
		float2 key = new float2(chunkPos.x, chunkPos.z);
		if (chunkByCoords.TryGetValue(key, out var value))
		{
			return value;
		}
		for (int i = 0; i < count; i++)
		{
			Chunk chunk = chunks[i];
			if (chunk.chunkBounds.center.x == chunkPos.x && chunk.chunkBounds.center.z == chunkPos.z)
			{
				chunkByCoords[key] = chunk;
				return chunk;
			}
		}
		return null;
	}

	public void Dispose()
	{
		if (chunkDataArray.IsCreated)
		{
			chunkDataArray.Dispose();
		}
		if (chunkVisibilityStateResults.IsCreated)
		{
			chunkVisibilityStateResults.Dispose();
		}
		prewarmedObjects.Clear();
	}

	public void DisposeVisibilityJobBuffers()
	{
		dynamicBatchJobBuffer.Dispose();
		visibleJobBuffer.Dispose();
		prewarmJobBuffer.Dispose();
	}

	public void DisposeChunks()
	{
		for (int i = 0; i < chunks.Length; i++)
		{
			chunks[i].Dispose();
		}
		chunkByCoords.Clear();
	}

	public void Clear()
	{
		DisposeChunks();
		chunks = Array.Empty<Chunk>();
		count = 0;
		Dispose();
	}

	public void EnsureCapacity()
	{
		if (!chunkDataArray.IsCreated || count != chunkDataArray.Length)
		{
			Dispose();
			chunkDataArray = new NativeArray<BurstableBounds>(count, Allocator.Persistent);
			chunkVisibilityStateResults = new NativeArray<byte>(count, Allocator.Persistent);
		}
	}
}
