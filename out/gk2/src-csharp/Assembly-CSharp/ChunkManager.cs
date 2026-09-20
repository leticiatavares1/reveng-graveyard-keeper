using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cinemachine;
using LazyBearTechnology;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[DefaultExecutionOrder(10000)]
public class ChunkManager : LazySingleton<ChunkManager>
{
	private const int JOB_INNER_LOOP_BATCH_COUNT = 4;

	private const float CHUNK_HEIGHT = 30f;

	private static float3 chunkSize = new float3(18f, 30f, 10f);

	private List<ChunkManagerLayer> layers = new List<ChunkManagerLayer>();

	private HashSet<IChunkableObject> pendingVisibilityRecheckObjects = new HashSet<IChunkableObject>();

	private bool isInitialized;

	private bool isProcessingVisibility;

	private readonly Plane[] cachedFrustumPlanes = new Plane[6];

	private readonly BurstablePlane[] cachedBurstablePlanes = new BurstablePlane[6];

	private readonly ChunkVisibilityJobBuffer singleObjectJobBuffer = new ChunkVisibilityJobBuffer();

	public bool IsActive { get; set; }

	public void RegisterChunks<T>(List<T> chunkableObjects, ChunkManagerLayerType layerType) where T : IChunkableObject
	{
		TryInit();
		ChunkManagerLayer chunkManagerLayer = layers[(int)layerType];
		int count = chunkableObjects.Count;
		int count2 = chunkManagerLayer.Count;
		Stopwatch stopwatch = Stopwatch.StartNew();
		Stopwatch stopwatch2 = Stopwatch.StartNew();
		foreach (T chunkableObject in chunkableObjects)
		{
			if (IsChunkableObjectAlive(chunkableObject))
			{
				SplitChunkableObjectBetweenChunks(chunkManagerLayer, chunkableObject);
			}
		}
		stopwatch2.Stop();
		int count3 = chunkManagerLayer.Count;
		Stopwatch stopwatch3 = Stopwatch.StartNew();
		chunkManagerLayer.EnsureCapacity();
		stopwatch3.Stop();
		int num = 0;
		Stopwatch stopwatch4 = Stopwatch.StartNew();
		Chunk[] chunks = chunkManagerLayer.chunks;
		foreach (Chunk chunk in chunks)
		{
			if (chunk.Count == 0)
			{
				chunk.EnsureCapacity();
				continue;
			}
			float num2 = float.PositiveInfinity;
			float num3 = float.NegativeInfinity;
			foreach (IChunkableObject chunkableObject2 in chunk.chunkableObjects)
			{
				if (IsChunkableObjectAlive(chunkableObject2))
				{
					num++;
					BurstableBounds chunkableData = chunkableObject2.GetChunkableData();
					num2 = Mathf.Min(num2, chunkableData.Min.y);
					num3 = Mathf.Max(num3, chunkableData.Max.y);
				}
			}
			chunk.chunkBounds.center.y = (num2 + num3) / 2f;
			chunk.chunkBounds.size.y = num3 - num2;
			chunk.EnsureCapacity();
		}
		stopwatch4.Stop();
		stopwatch.Stop();
		UnityEngine.Debug.Log($"[ChunkManager] RegisterChunks({layerType}) " + $"objects={count}, chunksBeforeSplit={count2}, chunksAfterSplit={count3}, " + $"split={stopwatch2.ElapsedMilliseconds}ms, " + $"rebuild={stopwatch4.ElapsedMilliseconds}ms (memberships={num}), " + $"total={stopwatch.ElapsedMilliseconds}ms");
	}

	public void UnregisterChunks<T>(List<T> chunkList, ChunkManagerLayerType layerType) where T : IChunkableObject
	{
		if (!Application.isPlaying || PlayModeTracker.IsExitingPlayMode)
		{
			return;
		}
		TryInit();
		ChunkManagerLayer chunkManagerLayer = layers[(int)layerType];
		for (int num = chunkManagerLayer.Count - 1; num >= 0; num--)
		{
			Chunk chunk = chunkManagerLayer.chunks[num];
			List<IChunkableObject> list = null;
			foreach (T chunk2 in chunkList)
			{
				if (chunk.ContainsChunkableObject(chunk2))
				{
					if (list == null)
					{
						list = new List<IChunkableObject>();
					}
					list.Add(chunk2);
				}
			}
			if (list != null)
			{
				chunk.RemoveChunkableObjects(list);
			}
			if (chunk.Count == 0)
			{
				chunkManagerLayer.RemoveChunk(chunk);
			}
			else
			{
				chunk.EnsureCapacity();
			}
		}
		chunkManagerLayer.EnsureCapacity();
	}

	public void RegisterStaticChunkableObject(IChunkableObject chunkableObject, ChunkManagerLayerType layerType)
	{
		if (!Application.isPlaying || PlayModeTracker.IsExitingPlayMode)
		{
			return;
		}
		TryInit();
		ChunkManagerLayer chunkManagerLayer = layers[(int)layerType];
		SplitChunkableObjectBetweenChunks(chunkManagerLayer, chunkableObject);
		chunkManagerLayer.EnsureCapacity();
		for (int i = 0; i < chunkManagerLayer.Count; i++)
		{
			if (IsObjectIntersectsChunk(chunkableObject, chunkManagerLayer.chunks[i]) && !chunkManagerLayer.chunks[i].ContainsChunkableObject(chunkableObject))
			{
				chunkManagerLayer.chunks[i].AddChunkableObject(chunkableObject);
			}
		}
		Chunk[] chunks = chunkManagerLayer.chunks;
		foreach (Chunk chunk in chunks)
		{
			if (chunk.Count == 0)
			{
				chunk.EnsureCapacity();
				continue;
			}
			float num = float.PositiveInfinity;
			float num2 = float.NegativeInfinity;
			foreach (IChunkableObject chunkableObject2 in chunk.chunkableObjects)
			{
				BurstableBounds chunkableData = chunkableObject2.GetChunkableData();
				num = Mathf.Min(num, chunkableData.Min.y);
				num2 = Mathf.Max(num2, chunkableData.Max.y);
			}
			chunk.chunkBounds.center.y = (num + num2) / 2f;
			chunk.chunkBounds.size.y = num2 - num;
			chunk.EnsureCapacity();
		}
	}

	public void UnregisterStaticChunkableObject(IChunkableObject chunkableObject, ChunkManagerLayerType layerType)
	{
		if (!Application.isPlaying || PlayModeTracker.IsExitingPlayMode)
		{
			return;
		}
		TryInit();
		ChunkManagerLayer chunkManagerLayer = layers[(int)layerType];
		for (int num = chunkManagerLayer.Count - 1; num >= 0; num--)
		{
			Chunk chunk = chunkManagerLayer.chunks[num];
			if (chunk.ContainsChunkableObject(chunkableObject))
			{
				chunk.RemoveChunkableObject(chunkableObject);
				if (chunk.Count == 0)
				{
					chunkManagerLayer.RemoveChunk(chunk);
				}
				else
				{
					chunk.EnsureCapacity();
				}
			}
		}
		chunkManagerLayer.EnsureCapacity();
	}

	public void RegisterDynamicChunkableObject(IChunkableObject chunkableObject, ChunkManagerLayerType layerType)
	{
		if (Application.isPlaying && !PlayModeTracker.IsExitingPlayMode)
		{
			TryInit();
			ChunkManagerLayer chunkManagerLayer = layers[(int)layerType];
			chunkManagerLayer.chunks[0].AddChunkableObject(chunkableObject);
			chunkManagerLayer.chunks[0].EnsureCapacity();
		}
	}

	public void UnregisterDynamicChunkableObject(IChunkableObject chunkableObject, ChunkManagerLayerType layerType)
	{
		if (Application.isPlaying && !PlayModeTracker.IsExitingPlayMode)
		{
			TryInit();
			ChunkManagerLayer chunkManagerLayer = layers[(int)layerType];
			chunkManagerLayer.chunks[0].RemoveChunkableObject(chunkableObject);
			chunkManagerLayer.chunks[0].EnsureCapacity();
		}
	}

	public void SetChunkableObjectToDynamicLayer(List<IChunkableObject> chunkableObjects, ChunkManagerLayerType layerType)
	{
		if (Application.isPlaying && !PlayModeTracker.IsExitingPlayMode)
		{
			TryInit();
			ChunkManagerLayer chunkManagerLayer = layers[(int)layerType];
			if (chunkManagerLayer.IsDynamic)
			{
				chunkManagerLayer.chunks[0].SetChunkableObjects(chunkableObjects);
				chunkManagerLayer.chunks[0].EnsureCapacity();
			}
		}
	}

	public void ClearChunkableObjectFromDynamicLayer(ChunkManagerLayerType layerType)
	{
		if (Application.isPlaying && !PlayModeTracker.IsExitingPlayMode)
		{
			TryInit();
			ChunkManagerLayer chunkManagerLayer = layers[(int)layerType];
			if (chunkManagerLayer.IsDynamic)
			{
				chunkManagerLayer.chunks[0].ClearChunkableObjects();
				chunkManagerLayer.chunks[0].EnsureCapacity();
			}
		}
	}

	public void ClearAll()
	{
		if (!Application.isPlaying || PlayModeTracker.IsExitingPlayMode || !isInitialized)
		{
			return;
		}
		pendingVisibilityRecheckObjects.Clear();
		foreach (ChunkManagerLayer layer in layers)
		{
			layer.Clear();
			if (layer.IsDynamic)
			{
				layer.AddChunk(CreateInfiniteChunk());
			}
			layer.EnsureCapacity();
		}
	}

	private static Chunk CreateInfiniteChunk()
	{
		return new Chunk(float3.zero, new float3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity));
	}

	public void ForceProcessVisibility()
	{
		TryProcessChunkVisibility();
	}

	public void RequestVisibilityRecheck(IChunkableObject chunkableObject)
	{
		if (IsActive)
		{
			TryInit();
			if (IsChunkableObjectAlive(chunkableObject))
			{
				pendingVisibilityRecheckObjects.Add(chunkableObject);
			}
		}
	}

	public HashSet<IChunkableObject> GetAllChunkableObjectsInBounds(Bounds bounds)
	{
		return GetAllChunkableObjectsInBoundsForSelectedLayers(Enum.GetValues(typeof(ChunkManagerLayerType)).Cast<ChunkManagerLayerType>().ToList(), bounds);
	}

	public HashSet<IChunkableObject> GetAllChunkableObjectsInBoundsForSelectedLayers(List<ChunkManagerLayerType> selectedTypes, Bounds bounds)
	{
		HashSet<IChunkableObject> hashSet = new HashSet<IChunkableObject>();
		for (int i = 0; i < layers.Count; i++)
		{
			ChunkManagerLayer chunkManagerLayer = layers[i];
			if (!selectedTypes.Contains(chunkManagerLayer.layerType))
			{
				continue;
			}
			for (int j = 0; j < chunkManagerLayer.Count; j++)
			{
				Chunk chunk = chunkManagerLayer.chunks[j];
				List<IChunkableObject> list = new List<IChunkableObject>();
				for (int k = 0; k < chunk.Count; k++)
				{
					IChunkableObject chunkableObject = chunk.chunkableObjects[k];
					if (chunkableObject != null && (!(chunkableObject is UnityEngine.Object @object) || !(@object == null)))
					{
						list.Add(chunkableObject);
					}
				}
				if (list.Count == 0)
				{
					continue;
				}
				ChunkDataIntersectsJob jobData = default(ChunkDataIntersectsJob);
				jobData.sourceData = BurstConverter.ConvertBoundsToBurstable(bounds);
				jobData.objectsData = new NativeArray<BurstableBounds>(list.Count, Allocator.Persistent);
				jobData.results = new NativeArray<bool>(list.Count, Allocator.Persistent);
				for (int l = 0; l < list.Count; l++)
				{
					jobData.objectsData[l] = list[l].GetChunkableData();
				}
				IJobParallelForExtensions.Schedule(jobData, list.Count, 4).Complete();
				for (int m = 0; m < list.Count; m++)
				{
					if (jobData.results[m])
					{
						hashSet.Add(list[m]);
					}
				}
				jobData.objectsData.Dispose();
				jobData.results.Dispose();
			}
		}
		return hashSet;
	}

	protected override void Awake()
	{
		base.Awake();
		TryInit();
	}

	private void OnEnable()
	{
		CinemachineCore.CameraUpdatedEvent.AddListener(OnCameraUpdated);
	}

	private void OnDisable()
	{
		CinemachineCore.CameraUpdatedEvent.RemoveListener(OnCameraUpdated);
	}

	private void OnCameraUpdated(CinemachineBrain brain)
	{
		TryProcessChunkVisibility();
	}

	private void TryProcessChunkVisibility()
	{
		if (!IsActive || isProcessingVisibility)
		{
			return;
		}
		Camera camera = CameraSystem.Instance?.MainCamera?.Camera;
		if (camera == null)
		{
			return;
		}
		isProcessingVisibility = true;
		try
		{
			ProcessChunkVisibility(camera);
		}
		finally
		{
			isProcessingVisibility = false;
		}
	}

	private void SplitChunkableObjectBetweenChunks(ChunkManagerLayer layer, IChunkableObject chunkableObject)
	{
		BurstableBounds chunkableData = chunkableObject.GetChunkableData();
		float2 @float = new float2(Mathf.Floor(chunkableData.Min.x / chunkSize.x) * chunkSize.x, Mathf.Floor(chunkableData.Min.z / chunkSize.z) * chunkSize.z);
		float2 float2 = new float2(Mathf.Ceil(chunkableData.Max.x / chunkSize.x) * chunkSize.x, Mathf.Ceil(chunkableData.Max.z / chunkSize.z) * chunkSize.z);
		float num = ((float2.x > @float.x) ? @float.x : float2.x);
		float num2 = ((float2.x > @float.x) ? float2.x : @float.x);
		float num3 = ((float2.y > @float.y) ? @float.y : float2.y);
		float num4 = ((float2.y > @float.y) ? float2.y : @float.y);
		for (float num5 = num; num5 <= num2; num5 += chunkSize.x)
		{
			for (float num6 = num3; num6 <= num4; num6 += chunkSize.z)
			{
				float3 float3 = new float3(num5, 30f, num6);
				Chunk chunk = layer.FindChunk(float3);
				if (chunk == null)
				{
					chunk = new Chunk(float3, chunkSize);
					if (IsObjectIntersectsChunk(chunkableObject, chunk))
					{
						layer.AddChunk(chunk);
						chunk.AddChunkableObject(chunkableObject);
					}
				}
				else if (!chunk.ContainsChunkableObject(chunkableObject) && IsObjectIntersectsChunk(chunkableObject, chunk))
				{
					chunk.AddChunkableObject(chunkableObject);
				}
			}
		}
	}

	private void TryInit()
	{
		if (!isInitialized)
		{
			isInitialized = true;
			pendingVisibilityRecheckObjects = new HashSet<IChunkableObject>();
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			layers.Add(new ChunkManagerLayer(ChunkManagerLayerType.StaticObjects));
			ChunkManagerLayer chunkManagerLayer = new ChunkManagerLayer(ChunkManagerLayerType.DynamicWgo);
			chunkManagerLayer.AddChunk(CreateInfiniteChunk());
			chunkManagerLayer.EnsureCapacity();
			layers.Add(chunkManagerLayer);
			ChunkManagerLayer chunkManagerLayer2 = new ChunkManagerLayer(ChunkManagerLayerType.DropView);
			chunkManagerLayer2.AddChunk(CreateInfiniteChunk());
			chunkManagerLayer2.EnsureCapacity();
			layers.Add(chunkManagerLayer2);
			layers.Add(new ChunkManagerLayer(ChunkManagerLayerType.StaticWgo));
			layers.Add(new ChunkManagerLayer(ChunkManagerLayerType.StaticWso));
			layers.Add(new ChunkManagerLayer(ChunkManagerLayerType.WsoConstructorParts));
			layers.Add(new ChunkManagerLayer(ChunkManagerLayerType.FightingLevelStaticObjects));
		}
	}

	private void ProcessChunkVisibility(Camera worldCamera)
	{
		GeometryUtility.CalculateFrustumPlanes(worldCamera, cachedFrustumPlanes);
		BurstConverter.ConvertToBurstablePlanes(cachedFrustumPlanes, cachedBurstablePlanes);
		BurstablePlane plane = cachedBurstablePlanes[0];
		BurstablePlane plane2 = cachedBurstablePlanes[1];
		BurstablePlane plane3 = cachedBurstablePlanes[2];
		BurstablePlane plane4 = cachedBurstablePlanes[3];
		BurstablePlane plane5 = cachedBurstablePlanes[4];
		BurstablePlane plane6 = cachedBurstablePlanes[5];
		foreach (ChunkManagerLayer layer in layers)
		{
			if (layer.Count <= 0)
			{
				continue;
			}
			if (layer.IsDynamic)
			{
				for (int i = 0; i < layer.Count; i++)
				{
					layer.chunkVisibilityStateResults[i] = 2;
				}
			}
			else
			{
				for (int j = 0; j < layer.Count; j++)
				{
					layer.chunkDataArray[j] = layer.chunks[j].GetChunkableData();
				}
				ChunkVisibilityJob jobData = default(ChunkVisibilityJob);
				jobData.plane0 = plane;
				jobData.plane1 = plane2;
				jobData.plane2 = plane3;
				jobData.plane3 = plane4;
				jobData.plane4 = plane5;
				jobData.plane5 = plane6;
				jobData.chunkableDataArray = layer.chunkDataArray;
				jobData.visibilityStateResults = layer.chunkVisibilityStateResults;
				jobData.expandFactor = layer.expandFactor;
				jobData.prewarmPlanePadding = layer.prewarmPlanePadding;
				IJobParallelForExtensions.Schedule(jobData, layer.Count, 4).Complete();
			}
			if (layer.IsDynamic)
			{
				List<IChunkableObject> dynamicUpdateBuffer = layer.dynamicUpdateBuffer;
				dynamicUpdateBuffer.Clear();
				for (int k = 0; k < layer.Count; k++)
				{
					Chunk chunk = layer.chunks[k];
					chunk.isVisible = layer.chunkVisibilityStateResults[k] == 2;
					if (!chunk.isVisible)
					{
						continue;
					}
					for (int l = 0; l < chunk.Count; l++)
					{
						IChunkableObject chunkableObject = chunk.chunkableObjects[l];
						if (chunkableObject != null && (!(chunkableObject is UnityEngine.Object @object) || !(@object == null)) && !chunkableObject.IgnoreChunkVisibility)
						{
							dynamicUpdateBuffer.Add(chunkableObject);
						}
					}
				}
				int count = dynamicUpdateBuffer.Count;
				if (count <= 0)
				{
					continue;
				}
				layer.dynamicBatchJobBuffer.EnsureCapacity(count);
				NativeArray<BurstableBounds> data = layer.dynamicBatchJobBuffer.data;
				NativeArray<byte> results = layer.dynamicBatchJobBuffer.results;
				for (int m = 0; m < count; m++)
				{
					data[m] = dynamicUpdateBuffer[m].GetChunkableData();
				}
				ChunkVisibilityJob jobData = default(ChunkVisibilityJob);
				jobData.plane0 = plane;
				jobData.plane1 = plane2;
				jobData.plane2 = plane3;
				jobData.plane3 = plane4;
				jobData.plane4 = plane5;
				jobData.plane5 = plane6;
				jobData.chunkableDataArray = data;
				jobData.visibilityStateResults = results;
				jobData.expandFactor = layer.expandFactor;
				jobData.prewarmPlanePadding = layer.prewarmPlanePadding;
				ChunkVisibilityJob jobData2 = jobData;
				IJobParallelForExtensions.Schedule(jobData2, count, 4).Complete();
				for (int n = 0; n < count; n++)
				{
					IChunkableObject chunkableObject2 = dynamicUpdateBuffer[n];
					if (chunkableObject2 != null && (!(chunkableObject2 is UnityEngine.Object object2) || !(object2 == null)))
					{
						ChunkVisibilityState state = (ChunkVisibilityState)jobData2.visibilityStateResults[n];
						DispatchChunkVisibilityState(chunkableObject2, state);
					}
				}
				continue;
			}
			List<IChunkableObject> objectsLeavingVisibleBuffer = layer.objectsLeavingVisibleBuffer;
			objectsLeavingVisibleBuffer.Clear();
			List<IChunkableObject> visibleCandidatesBuffer = layer.visibleCandidatesBuffer;
			visibleCandidatesBuffer.Clear();
			List<IChunkableObject> prewarmCandidatesBuffer = layer.prewarmCandidatesBuffer;
			prewarmCandidatesBuffer.Clear();
			for (int num = 0; num < layer.Count; num++)
			{
				Chunk chunk2 = layer.chunks[num];
				ChunkVisibilityState chunkVisibilityState = (ChunkVisibilityState)layer.chunkVisibilityStateResults[num];
				bool isVisible = chunk2.isVisible;
				bool flag = (chunk2.isVisible = chunkVisibilityState == ChunkVisibilityState.Visible);
				if (isVisible && !flag)
				{
					objectsLeavingVisibleBuffer.AddRange(chunk2.chunkableObjects);
				}
				if (chunk2.IgnoreChunkVisibility)
				{
					continue;
				}
				switch (chunkVisibilityState)
				{
				case ChunkVisibilityState.Visible:
				{
					for (int num3 = 0; num3 < chunk2.Count; num3++)
					{
						IChunkableObject chunkableObject4 = chunk2.chunkableObjects[num3];
						if (IsChunkableObjectAlive(chunkableObject4) && !chunkableObject4.IgnoreChunkVisibility)
						{
							visibleCandidatesBuffer.Add(chunkableObject4);
						}
					}
					break;
				}
				case ChunkVisibilityState.Prewarm:
				{
					for (int num2 = 0; num2 < chunk2.Count; num2++)
					{
						IChunkableObject chunkableObject3 = chunk2.chunkableObjects[num2];
						if (IsChunkableObjectAlive(chunkableObject3) && !chunkableObject3.IgnoreChunkVisibility)
						{
							prewarmCandidatesBuffer.Add(chunkableObject3);
						}
					}
					break;
				}
				}
			}
			HashSet<IChunkableObject> currentlyVisibleObjectsBuffer = layer.currentlyVisibleObjectsBuffer;
			currentlyVisibleObjectsBuffer.Clear();
			if (visibleCandidatesBuffer.Count > 0)
			{
				layer.visibleJobBuffer.EnsureCapacity(visibleCandidatesBuffer.Count);
				NativeArray<BurstableBounds> data2 = layer.visibleJobBuffer.data;
				NativeArray<byte> results2 = layer.visibleJobBuffer.results;
				for (int num4 = 0; num4 < visibleCandidatesBuffer.Count; num4++)
				{
					data2[num4] = visibleCandidatesBuffer[num4].GetChunkableData();
				}
				ChunkVisibilityJob jobData = default(ChunkVisibilityJob);
				jobData.plane0 = plane;
				jobData.plane1 = plane2;
				jobData.plane2 = plane3;
				jobData.plane3 = plane4;
				jobData.plane4 = plane5;
				jobData.plane5 = plane6;
				jobData.chunkableDataArray = data2;
				jobData.visibilityStateResults = results2;
				jobData.expandFactor = layer.expandFactor;
				jobData.prewarmPlanePadding = layer.prewarmPlanePadding;
				IJobParallelForExtensions.Schedule(jobData, visibleCandidatesBuffer.Count, 4).Complete();
				for (int num5 = 0; num5 < visibleCandidatesBuffer.Count; num5++)
				{
					if (results2[num5] == 2)
					{
						currentlyVisibleObjectsBuffer.Add(visibleCandidatesBuffer[num5]);
					}
				}
			}
			HashSet<IChunkableObject> currentlyPrewarmedObjectsBuffer = layer.currentlyPrewarmedObjectsBuffer;
			currentlyPrewarmedObjectsBuffer.Clear();
			if (prewarmCandidatesBuffer.Count > 0)
			{
				layer.prewarmJobBuffer.EnsureCapacity(prewarmCandidatesBuffer.Count);
				NativeArray<BurstableBounds> data3 = layer.prewarmJobBuffer.data;
				NativeArray<byte> results3 = layer.prewarmJobBuffer.results;
				for (int num6 = 0; num6 < prewarmCandidatesBuffer.Count; num6++)
				{
					data3[num6] = prewarmCandidatesBuffer[num6].GetChunkableData();
				}
				ChunkVisibilityJob jobData = default(ChunkVisibilityJob);
				jobData.plane0 = plane;
				jobData.plane1 = plane2;
				jobData.plane2 = plane3;
				jobData.plane3 = plane4;
				jobData.plane4 = plane5;
				jobData.plane5 = plane6;
				jobData.chunkableDataArray = data3;
				jobData.visibilityStateResults = results3;
				jobData.expandFactor = layer.expandFactor;
				jobData.prewarmPlanePadding = layer.prewarmPlanePadding;
				IJobParallelForExtensions.Schedule(jobData, prewarmCandidatesBuffer.Count, 4).Complete();
				for (int num7 = 0; num7 < prewarmCandidatesBuffer.Count; num7++)
				{
					IChunkableObject item = prewarmCandidatesBuffer[num7];
					if (!currentlyVisibleObjectsBuffer.Contains(item))
					{
						ChunkVisibilityState chunkVisibilityState2 = (ChunkVisibilityState)results3[num7];
						if (chunkVisibilityState2 == ChunkVisibilityState.Prewarm || chunkVisibilityState2 == ChunkVisibilityState.Visible)
						{
							currentlyPrewarmedObjectsBuffer.Add(item);
						}
					}
				}
			}
			HashSet<IChunkableObject> objectsToHideBuffer = layer.objectsToHideBuffer;
			objectsToHideBuffer.Clear();
			foreach (IChunkableObject item2 in objectsLeavingVisibleBuffer)
			{
				if (!currentlyVisibleObjectsBuffer.Contains(item2) && !currentlyPrewarmedObjectsBuffer.Contains(item2))
				{
					objectsToHideBuffer.Add(item2);
				}
			}
			foreach (IChunkableObject item3 in visibleCandidatesBuffer)
			{
				if (!currentlyVisibleObjectsBuffer.Contains(item3) && !currentlyPrewarmedObjectsBuffer.Contains(item3))
				{
					objectsToHideBuffer.Add(item3);
				}
			}
			foreach (IChunkableObject prewarmedObject in layer.prewarmedObjects)
			{
				if (!currentlyVisibleObjectsBuffer.Contains(prewarmedObject) && !currentlyPrewarmedObjectsBuffer.Contains(prewarmedObject))
				{
					objectsToHideBuffer.Add(prewarmedObject);
				}
			}
			layer.prewarmedObjects.Clear();
			layer.prewarmedObjects.UnionWith(currentlyPrewarmedObjectsBuffer);
			foreach (IChunkableObject item4 in currentlyVisibleObjectsBuffer)
			{
				DispatchChunkVisibilityState(item4, ChunkVisibilityState.Visible);
			}
			foreach (IChunkableObject item5 in currentlyPrewarmedObjectsBuffer)
			{
				DispatchChunkVisibilityState(item5, ChunkVisibilityState.Prewarm);
			}
			foreach (IChunkableObject item6 in objectsToHideBuffer)
			{
				DispatchChunkVisibilityState(item6, ChunkVisibilityState.OutOfRange);
			}
		}
		ProcessPendingVisibilityRechecks(plane, plane2, plane3, plane4, plane5, plane6);
	}

	private void ProcessPendingVisibilityRechecks(BurstablePlane plane0, BurstablePlane plane1, BurstablePlane plane2, BurstablePlane plane3, BurstablePlane plane4, BurstablePlane plane5)
	{
		if (pendingVisibilityRecheckObjects.Count == 0)
		{
			return;
		}
		List<IChunkableObject> list = pendingVisibilityRecheckObjects.ToList();
		pendingVisibilityRecheckObjects.Clear();
		foreach (IChunkableObject item in list)
		{
			if (IsChunkableObjectAlive(item))
			{
				ChunkManagerLayer ownerLayer;
				if (item.IgnoreChunkVisibility)
				{
					DispatchChunkVisibilityState(item, ChunkVisibilityState.Visible);
				}
				else if (TryGetLayerForChunkableObject(item, out ownerLayer))
				{
					ChunkVisibilityState state = CalculateObjectVisibilityState(item, ownerLayer, plane0, plane1, plane2, plane3, plane4, plane5);
					DispatchChunkVisibilityState(item, state);
				}
			}
		}
	}

	private bool TryGetLayerForChunkableObject(IChunkableObject chunkableObject, out ChunkManagerLayer ownerLayer)
	{
		for (int i = 0; i < layers.Count; i++)
		{
			ChunkManagerLayer chunkManagerLayer = layers[i];
			for (int j = 0; j < chunkManagerLayer.Count; j++)
			{
				if (chunkManagerLayer.chunks[j].ContainsChunkableObject(chunkableObject))
				{
					ownerLayer = chunkManagerLayer;
					return true;
				}
			}
		}
		ownerLayer = null;
		return false;
	}

	private ChunkVisibilityState CalculateObjectVisibilityState(IChunkableObject chunkableObject, ChunkManagerLayer layer, BurstablePlane plane0, BurstablePlane plane1, BurstablePlane plane2, BurstablePlane plane3, BurstablePlane plane4, BurstablePlane plane5)
	{
		singleObjectJobBuffer.EnsureCapacity(1);
		NativeArray<BurstableBounds> data = singleObjectJobBuffer.data;
		NativeArray<byte> results = singleObjectJobBuffer.results;
		data[0] = chunkableObject.GetChunkableData();
		ChunkVisibilityJob jobData = default(ChunkVisibilityJob);
		jobData.plane0 = plane0;
		jobData.plane1 = plane1;
		jobData.plane2 = plane2;
		jobData.plane3 = plane3;
		jobData.plane4 = plane4;
		jobData.plane5 = plane5;
		jobData.chunkableDataArray = data;
		jobData.visibilityStateResults = results;
		jobData.expandFactor = layer.expandFactor;
		jobData.prewarmPlanePadding = layer.prewarmPlanePadding;
		IJobParallelForExtensions.Schedule(jobData, 1, 1).Complete();
		return (ChunkVisibilityState)results[0];
	}

	private void OnDestroy()
	{
		pendingVisibilityRecheckObjects.Clear();
		foreach (ChunkManagerLayer layer in layers)
		{
			layer.DisposeChunks();
			layer.Dispose();
			layer.DisposeVisibilityJobBuffers();
		}
		singleObjectJobBuffer.Dispose();
	}

	private bool IsObjectIntersectsChunk(IChunkableObject chunkableObject, Chunk chunk)
	{
		if (!IsChunkableObjectAlive(chunkableObject))
		{
			return false;
		}
		BurstableBounds chunkableData = chunk.GetChunkableData();
		BurstableBounds chunkableData2 = chunkableObject.GetChunkableData();
		chunkableData.center = new float3(chunkableData.center.x, 0f, chunkableData.center.z);
		chunkableData2.center = new float3(chunkableData2.center.x, 0f, chunkableData2.center.z);
		return chunkableData2.Intersects(chunkableData);
	}

	private static bool IsChunkableObjectAlive(IChunkableObject chunkableObject)
	{
		if (chunkableObject != null)
		{
			if (chunkableObject is UnityEngine.Object @object)
			{
				return @object != null;
			}
			return true;
		}
		return false;
	}

	private void DispatchChunkVisibilityState(IChunkableObject chunkableObject, ChunkVisibilityState state)
	{
		if (IsChunkableObjectAlive(chunkableObject) && !chunkableObject.IgnoreChunkVisibility)
		{
			if (chunkableObject is IChunkVisibilityStateReceiver chunkVisibilityStateReceiver)
			{
				chunkVisibilityStateReceiver.UpdateChunkVisibilityState(state);
			}
			chunkableObject.UpdateChunkVisibility(state == ChunkVisibilityState.Visible);
		}
	}
}
