using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class Wso : MonoBehaviour, IChunkableObject, IChunkVisibilityStateReceiver, IUniqueIdUser
{
	public enum RuntimeStagesLoadMode
	{
		LoadOnSpawn,
		LoadOnlyWhenVisible
	}

	[SerializeField]
	private string id;

	[SerializeField]
	private bool shouldOptimize;

	[SerializeField]
	private WsoData data;

	[SerializeField]
	private List<WsoRepairableStage> repairableStages = new List<WsoRepairableStage>();

	[SerializeField]
	private bool isOptimized;

	[SerializeField]
	private List<WsoOptimizedStageEntry> optimizedStages = new List<WsoOptimizedStageEntry>();

	private List<ConstructorPart> runtimeConstructorParts = new List<ConstructorPart>();

	private List<AsyncOperationHandle<Texture2D>> loadedLutHandles = new List<AsyncOperationHandle<Texture2D>>();

	private List<GameObject> loadedOptimizedInstances = new List<GameObject>();

	private List<AsyncOperationHandle<GameObject>> loadedOptimizedHandles = new List<AsyncOperationHandle<GameObject>>();

	private bool isVisible = true;

	private bool hasData;

	private ChunkBoundsPair bounds;

	private bool boundsCalculated;

	private Vector3 initialBoundsPosition;

	private GameObject loadedPrefabAsset;

	private bool registeredInChunker;

	private bool runtimePartsLoading;

	private bool pendingRuntimePartsRebuild;

	private ChunkVisibilityState chunkVisibilityState;

	public static RuntimeStagesLoadMode GlobalRuntimeStagesLoadMode { get; set; }

	public bool RegisteredInChunker
	{
		get
		{
			return registeredInChunker;
		}
		set
		{
			registeredInChunker = value;
		}
	}

	public SGuid UniqueIdSGuid => data.UniqueId;

	string IUniqueIdUser.UniqueId => data.UniqueId.ToString();

	public string CustomTag
	{
		get
		{
			return data?.CustomTag;
		}
		set
		{
			if (data != null)
			{
				data.CustomTag = value;
			}
		}
	}

	public WsoData Data => data;

	public bool HasData => hasData;

	public bool ShouldOptimize => shouldOptimize;

	public IReadOnlyList<WsoRepairableStage> RepairableStages => repairableStages;

	public IReadOnlyList<ConstructorPart> RuntimeConstructorParts => runtimeConstructorParts;

	private bool IsVisible
	{
		get
		{
			if (isVisible)
			{
				return !(data?.IsHidden ?? false);
			}
			return false;
		}
	}

	[CanBeNull]
	public MultiFlagOR<ChunkingIgnoreType> IgnoreMultiFlag { get; set; }

	public static event Action<Wso> OnWsoSpawn;

	public static event Action<Wso> OnWsoDestroy;

	public BurstableBounds GetChunkableData()
	{
		if (!boundsCalculated)
		{
			WsoSerializedBoundsData wsoSerializedBoundsData = data?.GetComponentData<WsoSerializedBoundsData>();
			if (wsoSerializedBoundsData != null)
			{
				bounds = wsoSerializedBoundsData.Bounds;
			}
			else
			{
				bounds = ChunkSizeCalculator.CalculateChunkBounds(base.gameObject);
			}
			initialBoundsPosition = base.transform.position;
			boundsCalculated = true;
		}
		Vector3 vector = base.transform.position - initialBoundsPosition;
		return new BurstableBounds(bounds.GetBounds().center + vector, bounds.GetBounds().size);
	}

	public void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.cyan;
		this.DrawChunkGizmos();
	}

	public void UpdateChunkVisibility(bool isVisible)
	{
		if (this.isVisible != isVisible && (bool)this)
		{
			this.isVisible = isVisible;
			RefreshVisuals();
		}
	}

	public void UpdateChunkVisibilityState(ChunkVisibilityState state)
	{
		if ((bool)this && chunkVisibilityState != state)
		{
			chunkVisibilityState = state;
			if (!isVisible)
			{
				RefreshVisuals();
			}
		}
	}

	private void RefreshVisuals()
	{
		bool num = IsVisible;
		bool flag = runtimeConstructorParts.Count > 0 || loadedOptimizedInstances.Count > 0;
		if (num)
		{
			base.gameObject.SetActive(value: true);
			if (hasData && (pendingRuntimePartsRebuild || !flag) && !runtimePartsLoading)
			{
				RequestRuntimePartsRebuild(async: false);
			}
			return;
		}
		if (chunkVisibilityState == ChunkVisibilityState.Prewarm)
		{
			base.gameObject.SetActive(value: false);
			if (ShouldLoadRuntimeStagesInPrewarm() && hasData && (pendingRuntimePartsRebuild || !flag) && !runtimePartsLoading)
			{
				RequestRuntimePartsRebuild();
			}
			return;
		}
		LazySingleton<WsoConstructorPartsLoadManager>.Instance.CancelRebuild(this);
		LazySingleton<WsoOptimizedStagesLoadManager>.Instance.CancelLoad(this);
		if (runtimePartsLoading)
		{
			pendingRuntimePartsRebuild = true;
		}
		runtimePartsLoading = false;
		base.gameObject.SetActive(value: false);
	}

	private void TryRegisterInChunkManager()
	{
		if (Application.isPlaying && !registeredInChunker)
		{
			LazySingleton<ChunkManager>.Instance.RegisterStaticChunkableObject(this, ChunkManagerLayerType.StaticWso);
			pendingRuntimePartsRebuild = true;
			registeredInChunker = true;
			UpdateChunkVisibility(isVisible: false);
			if (ShouldLoadRuntimeStagesOnSpawn() && !runtimePartsLoading)
			{
				RequestRuntimePartsRebuild();
			}
		}
	}

	private void TryUnregisterFromChunkManager()
	{
		if (Application.isPlaying && registeredInChunker)
		{
			LazySingleton<ChunkManager>.Instance.UnregisterStaticChunkableObject(this, ChunkManagerLayerType.StaticWso);
			registeredInChunker = false;
		}
	}

	public static Wso Spawn(WsoData wsoData, Transform parentTransform)
	{
		string text = "Assets/AddressableAssets/WSOs/" + wsoData.id + ".prefab";
		AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.LoadAssetAsync<GameObject>(text);
		asyncOperationHandle.WaitForCompletion();
		Wso wso;
		if (asyncOperationHandle.Status == AsyncOperationStatus.Succeeded && asyncOperationHandle.Result != null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(asyncOperationHandle.Result, parentTransform);
			wso = gameObject.GetComponent<Wso>();
			if (wso == null)
			{
				wso = gameObject.AddComponent<Wso>();
			}
			wso.loadedPrefabAsset = asyncOperationHandle.Result;
		}
		else
		{
			wso = new GameObject().AddComponent<Wso>();
			wso.transform.parent = parentTransform;
			Debug.LogError("[Wso] Could not load prefab for '" + wsoData.id + "' from Addressables at '" + text + "'");
		}
		wso.data = wsoData;
		wso.id = wsoData.id;
		wso.transform.position = wsoData.Position;
		wso.transform.localScale = wsoData.Scale;
		wso.Init();
		wso.TryRegisterInChunkManager();
		Wso.OnWsoSpawn?.Invoke(wso);
		wso.HandleHiddenChanged(wso.data.IsHidden);
		Debug.Log("Spawned Wso with id: " + wso.data.id, wso.gameObject);
		return wso;
	}

	public void InitFromScene(string worldId)
	{
		if (data == null)
		{
			data = new WsoData(GameBalance.Me.GetData<WSODef>(id))
			{
				Position = base.transform.position,
				Scale = base.transform.localScale,
				WorldId = worldId
			};
		}
		CollectRepairableStages();
		Init();
	}

	public void InitFromContentPart(Vector3 globalOffset, string gameSceneId)
	{
		if (data == null)
		{
			Debug.LogWarning("[Wso] InitFromContentPart: data is null on " + base.name + ", skipping");
			return;
		}
		data.Position = base.transform.position + globalOffset;
		data.WorldId = gameSceneId;
		Init();
		TryRegisterInChunkManager();
		Wso.OnWsoSpawn?.Invoke(this);
		HandleHiddenChanged(data.IsHidden);
	}

	private void Init()
	{
		hasData = true;
		IgnoreMultiFlag = new MultiFlagOR<ChunkingIgnoreType>();
		if (data != null)
		{
			data.OnHiddenStateChanged += HandleHiddenChanged;
			data.OnRepairStateChanged += OnRepairStateChanged;
			data.PrepareForGame();
		}
	}

	private void DeInit()
	{
		if (hasData)
		{
			hasData = false;
			if (data != null)
			{
				data.OnHiddenStateChanged -= HandleHiddenChanged;
				data.OnRepairStateChanged -= OnRepairStateChanged;
				data.Cleanup();
			}
			LazySingleton<WsoConstructorPartsLoadManager>.Instance.CancelRebuild(this);
			LazySingleton<WsoOptimizedStagesLoadManager>.Instance.CancelLoad(this);
			runtimePartsLoading = false;
			pendingRuntimePartsRebuild = false;
			ClearRuntimeConstructorParts();
			if (loadedPrefabAsset != null)
			{
				Addressables.Release(loadedPrefabAsset);
				loadedPrefabAsset = null;
			}
		}
	}

	private void OnDestroy()
	{
		TryUnregisterFromChunkManager();
		DeInit();
	}

	public void CollectRepairableStages()
	{
		if (data != null && string.IsNullOrEmpty(data.Definition.replacementConfigId))
		{
			return;
		}
		repairableStages.Clear();
		GetComponentsInChildren(includeInactive: true, repairableStages);
		foreach (WsoRepairableStage repairableStage in repairableStages)
		{
			repairableStage.CollectParts();
		}
		if (data != null)
		{
			WsoRepairablePartData orCreateRepairablePartData = data.GetOrCreateRepairablePartData();
			orCreateRepairablePartData.ClearStages();
			for (int i = 0; i < repairableStages.Count; i++)
			{
				WsoStageData stageData = repairableStages[i].CreateStageData(i);
				orCreateRepairablePartData.AddStage(stageData);
			}
		}
	}

	private void RequestRuntimePartsRebuild(bool async = true)
	{
		if (Application.isPlaying && hasData)
		{
			pendingRuntimePartsRebuild = false;
			runtimePartsLoading = true;
			if (IsOptimizedRuntime())
			{
				LazySingleton<WsoOptimizedStagesLoadManager>.Instance.RequestLoad(this, async);
			}
			else
			{
				LazySingleton<WsoConstructorPartsLoadManager>.Instance.RequestRebuild(this, async);
			}
		}
	}

	private bool IsOptimizedRuntime()
	{
		return (data?.GetComponentData<WsoOptimizedStagesData>())?.IsOptimized ?? false;
	}

	private static bool ShouldLoadRuntimeStagesOnSpawn()
	{
		return GlobalRuntimeStagesLoadMode == RuntimeStagesLoadMode.LoadOnSpawn;
	}

	private static bool ShouldLoadRuntimeStagesInPrewarm()
	{
		return ShouldLoadRuntimeStagesOnSpawn();
	}

	public async Awaitable<WsoConstructorPartsBuildResult> BuildRuntimeConstructorPartsAsync()
	{
		WsoConstructorPartsBuildResult buildResult = new WsoConstructorPartsBuildResult();
		WsoRepairablePartData wsoRepairablePartData = data?.GetComponentData<WsoRepairablePartData>();
		if (wsoRepairablePartData == null || wsoRepairablePartData.Stages.Count == 0)
		{
			return buildResult;
		}
		foreach (WsoStageData stage in wsoRepairablePartData.Stages)
		{
			foreach (ConstructorPartStateData partsDatum in stage.PartsData)
			{
				ConstructorPart constructorPart = await SpawnConstructorPartFromDataAsync(partsDatum, buildResult.LutHandles);
				if (constructorPart != null)
				{
					buildResult.Parts.Add(constructorPart);
				}
			}
		}
		return buildResult;
	}

	public WsoConstructorPartsBuildResult BuildRuntimeConstructorPartsSync()
	{
		WsoConstructorPartsBuildResult wsoConstructorPartsBuildResult = new WsoConstructorPartsBuildResult();
		WsoRepairablePartData wsoRepairablePartData = data?.GetComponentData<WsoRepairablePartData>();
		if (wsoRepairablePartData == null || wsoRepairablePartData.Stages.Count == 0)
		{
			return wsoConstructorPartsBuildResult;
		}
		foreach (WsoStageData stage in wsoRepairablePartData.Stages)
		{
			foreach (ConstructorPartStateData partsDatum in stage.PartsData)
			{
				ConstructorPart constructorPart = SpawnConstructorPartFromDataSync(partsDatum, wsoConstructorPartsBuildResult.LutHandles);
				if (constructorPart != null)
				{
					wsoConstructorPartsBuildResult.Parts.Add(constructorPart);
				}
			}
		}
		return wsoConstructorPartsBuildResult;
	}

	public async Awaitable<WsoOptimizedStagesBuildResult> BuildOptimizedStagesAsync()
	{
		WsoOptimizedStagesBuildResult buildResult = new WsoOptimizedStagesBuildResult();
		WsoOptimizedStagesData wsoOptimizedStagesData = data?.GetComponentData<WsoOptimizedStagesData>();
		if (wsoOptimizedStagesData == null || !wsoOptimizedStagesData.IsOptimized)
		{
			return buildResult;
		}
		WsoRepairablePartData repairData = data?.GetComponentData<WsoRepairablePartData>();
		IReadOnlyList<WsoOptimizedStageEntry> stages = wsoOptimizedStagesData.Stages;
		for (int i = 0; i < stages.Count; i++)
		{
			WsoOptimizedStageEntry wsoOptimizedStageEntry = stages[i];
			string address = ((repairData?.GetStage(i)?.IsRepaired).GetValueOrDefault() ? wsoOptimizedStageEntry.repairedPrefabAddress : wsoOptimizedStageEntry.destroyedPrefabAddress);
			if (string.IsNullOrEmpty(address))
			{
				continue;
			}
			AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(address);
			await handle.Task;
			if (GameShutdown.IsRequested)
			{
				if (handle.IsValid())
				{
					Addressables.Release(handle);
				}
				if (!(await GameShutdown.WaitForResumeAsync()) || this == null)
				{
					break;
				}
				i--;
				continue;
			}
			if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
			{
				BuildOptimizedStageInstance(handle, buildResult);
				try
				{
					await BackgroundLoading.YieldIfNeeded(i + 1, 1);
				}
				catch (OperationCanceledException)
				{
					if (!(await GameShutdown.WaitForResumeAsync()) || this == null)
					{
						break;
					}
				}
				continue;
			}
			if (handle.IsValid())
			{
				Addressables.Release(handle);
			}
			Debug.LogError("[Wso] Failed to load optimized stage prefab at '" + address + "' for '" + base.name + "'");
		}
		return buildResult;
	}

	public WsoOptimizedStagesBuildResult BuildOptimizedStagesSync()
	{
		WsoOptimizedStagesBuildResult wsoOptimizedStagesBuildResult = new WsoOptimizedStagesBuildResult();
		WsoOptimizedStagesData wsoOptimizedStagesData = data?.GetComponentData<WsoOptimizedStagesData>();
		if (wsoOptimizedStagesData == null || !wsoOptimizedStagesData.IsOptimized)
		{
			return wsoOptimizedStagesBuildResult;
		}
		WsoRepairablePartData wsoRepairablePartData = data?.GetComponentData<WsoRepairablePartData>();
		IReadOnlyList<WsoOptimizedStageEntry> stages = wsoOptimizedStagesData.Stages;
		for (int i = 0; i < stages.Count; i++)
		{
			WsoOptimizedStageEntry wsoOptimizedStageEntry = stages[i];
			string text = ((wsoRepairablePartData?.GetStage(i)?.IsRepaired).GetValueOrDefault() ? wsoOptimizedStageEntry.repairedPrefabAddress : wsoOptimizedStageEntry.destroyedPrefabAddress);
			if (string.IsNullOrEmpty(text))
			{
				continue;
			}
			AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(text);
			handle.WaitForCompletion();
			if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
			{
				BuildOptimizedStageInstance(handle, wsoOptimizedStagesBuildResult);
				continue;
			}
			if (handle.IsValid())
			{
				Addressables.Release(handle);
			}
			Debug.LogError("[Wso] Failed to load optimized stage prefab at '" + text + "' for '" + base.name + "'");
		}
		return wsoOptimizedStagesBuildResult;
	}

	private void BuildOptimizedStageInstance(AsyncOperationHandle<GameObject> handle, WsoOptimizedStagesBuildResult buildResult)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(handle.Result, base.transform);
		gameObject.transform.localPosition = Vector3.zero;
		buildResult.Instances.Add(gameObject);
		buildResult.Handles.Add(handle);
	}

	private async Awaitable<ConstructorPart> SpawnConstructorPartFromDataAsync(ConstructorPartStateData partData, List<AsyncOperationHandle<Texture2D>> newLutHandles)
	{
		if (partData.IsDeleted)
		{
			return null;
		}
		ConstructorPartReplacementConfig constructorPartReplacementConfig = Data?.Definition.ReplacementConfig;
		string assetPath = partData.GetCurrentAssetPath(constructorPartReplacementConfig);
		if (string.IsNullOrEmpty(assetPath))
		{
			return null;
		}
		string currentModelId = partData.GetCurrentModelId(constructorPartReplacementConfig);
		GameObject partGo = new GameObject(currentModelId ?? "");
		partGo.transform.SetParent(base.transform);
		partGo.transform.localPosition = partData.LocalPosition;
		partGo.transform.localScale = new Vector3(partData.LocalXScale, 1f, 1f);
		ConstructorPart part = partGo.AddComponent<ConstructorPart>();
		part.constructorPartChildData.pathToObject = assetPath;
		part.constructorPartChildData.canNotBeBaked = true;
		Texture2D lut = null;
		if (constructorPartReplacementConfig != null && constructorPartReplacementConfig.TryGetLutAssetPath(partData.IsRepaired, partData.LutName, out var lutAssetPath))
		{
			AsyncOperationHandle<Texture2D> handle = Addressables.LoadAssetAsync<Texture2D>(lutAssetPath);
			lut = await handle.Task;
			if (handle.Status == AsyncOperationStatus.Succeeded)
			{
				newLutHandles.Add(handle);
			}
			else if (handle.IsValid())
			{
				Addressables.Release(handle);
			}
		}
		part.constructorPartChildData.lut = lut;
		if (LazySingletonSO<ConstructorPartBoundsConfig>.Instance.BoundsCollection.TryGetBounds(assetPath, partGo.transform.position, partGo.transform.lossyScale, out var worldBounds))
		{
			part.constructorPartChildData.chunkBounds = worldBounds;
		}
		else
		{
			Debug.LogError("Failed to get chunk bounds for " + assetPath);
		}
		part.UpdateChunkVisibility(isVisible: false);
		return part;
	}

	private ConstructorPart SpawnConstructorPartFromDataSync(ConstructorPartStateData partData, List<AsyncOperationHandle<Texture2D>> newLutHandles)
	{
		if (partData.IsDeleted)
		{
			return null;
		}
		ConstructorPartReplacementConfig constructorPartReplacementConfig = Data?.Definition.ReplacementConfig;
		string currentAssetPath = partData.GetCurrentAssetPath(constructorPartReplacementConfig);
		if (string.IsNullOrEmpty(currentAssetPath))
		{
			return null;
		}
		GameObject gameObject = new GameObject(partData.GetCurrentModelId(constructorPartReplacementConfig) ?? "");
		gameObject.transform.SetParent(base.transform);
		gameObject.transform.localPosition = partData.LocalPosition;
		gameObject.transform.localScale = new Vector3(partData.LocalXScale, 1f, 1f);
		ConstructorPart constructorPart = gameObject.AddComponent<ConstructorPart>();
		constructorPart.constructorPartChildData.pathToObject = currentAssetPath;
		constructorPart.constructorPartChildData.canNotBeBaked = true;
		Texture2D lut = null;
		if (constructorPartReplacementConfig != null && constructorPartReplacementConfig.TryGetLutAssetPath(partData.IsRepaired, partData.LutName, out var lutAssetPath))
		{
			AsyncOperationHandle<Texture2D> asyncOperationHandle = Addressables.LoadAssetAsync<Texture2D>(lutAssetPath);
			lut = asyncOperationHandle.WaitForCompletion();
			if (asyncOperationHandle.Status == AsyncOperationStatus.Succeeded)
			{
				newLutHandles.Add(asyncOperationHandle);
			}
			else if (asyncOperationHandle.IsValid())
			{
				Addressables.Release(asyncOperationHandle);
			}
		}
		constructorPart.constructorPartChildData.lut = lut;
		if (LazySingletonSO<ConstructorPartBoundsConfig>.Instance.BoundsCollection.TryGetBounds(currentAssetPath, gameObject.transform.position, gameObject.transform.lossyScale, out var worldBounds))
		{
			constructorPart.constructorPartChildData.chunkBounds = worldBounds;
		}
		else
		{
			Debug.LogError("Failed to get chunk bounds for " + currentAssetPath);
		}
		constructorPart.UpdateChunkVisibility(isVisible: false);
		return constructorPart;
	}

	public void CompleteRuntimePartsRebuild(WsoConstructorPartsBuildResult buildResult)
	{
		runtimePartsLoading = false;
		if (!this || !hasData)
		{
			ReleasePendingRuntimeParts(buildResult);
			return;
		}
		ClearRuntimeConstructorParts();
		runtimeConstructorParts.AddRange(buildResult.Parts);
		loadedLutHandles.AddRange(buildResult.LutHandles);
		if (runtimeConstructorParts.Count > 0)
		{
			LazySingleton<ChunkManager>.Instance.RegisterChunks(runtimeConstructorParts, ChunkManagerLayerType.WsoConstructorParts);
		}
		if (pendingRuntimePartsRebuild)
		{
			if (IsVisible)
			{
				RequestRuntimePartsRebuild(async: false);
			}
			else if (ShouldLoadRuntimeStagesInPrewarm() && chunkVisibilityState == ChunkVisibilityState.Prewarm)
			{
				RequestRuntimePartsRebuild();
			}
		}
	}

	public void ReleasePendingRuntimeParts(WsoConstructorPartsBuildResult buildResult)
	{
		runtimePartsLoading = false;
		if (buildResult == null)
		{
			return;
		}
		for (int i = 0; i < buildResult.Parts.Count; i++)
		{
			ConstructorPart constructorPart = buildResult.Parts[i];
			if (constructorPart != null)
			{
				UnityEngine.Object.Destroy(constructorPart.gameObject);
			}
		}
		for (int j = 0; j < buildResult.LutHandles.Count; j++)
		{
			AsyncOperationHandle<Texture2D> handle = buildResult.LutHandles[j];
			if (handle.IsValid())
			{
				Addressables.Release(handle);
			}
		}
	}

	public void CompleteOptimizedStagesLoad(WsoOptimizedStagesBuildResult buildResult)
	{
		runtimePartsLoading = false;
		if (!this || !hasData)
		{
			ReleasePendingOptimizedStages(buildResult);
			return;
		}
		ClearRuntimeConstructorParts();
		if (buildResult != null)
		{
			loadedOptimizedInstances.AddRange(buildResult.Instances);
			loadedOptimizedHandles.AddRange(buildResult.Handles);
		}
		if (pendingRuntimePartsRebuild)
		{
			if (IsVisible)
			{
				RequestRuntimePartsRebuild(async: false);
			}
			else if (ShouldLoadRuntimeStagesInPrewarm() && chunkVisibilityState == ChunkVisibilityState.Prewarm)
			{
				RequestRuntimePartsRebuild();
			}
		}
	}

	public void ReleasePendingOptimizedStages(WsoOptimizedStagesBuildResult buildResult)
	{
		runtimePartsLoading = false;
		if (buildResult == null)
		{
			return;
		}
		for (int i = 0; i < buildResult.Instances.Count; i++)
		{
			GameObject gameObject = buildResult.Instances[i];
			if (gameObject != null)
			{
				UnityEngine.Object.Destroy(gameObject);
			}
		}
		for (int j = 0; j < buildResult.Handles.Count; j++)
		{
			AsyncOperationHandle<GameObject> handle = buildResult.Handles[j];
			if (handle.IsValid())
			{
				Addressables.Release(handle);
			}
		}
	}

	private void ClearRuntimeConstructorParts()
	{
		ClearOptimizedInstances();
		foreach (ConstructorPart runtimeConstructorPart in runtimeConstructorParts)
		{
			if (runtimeConstructorPart != null)
			{
				UnityEngine.Object.Destroy(runtimeConstructorPart.gameObject);
			}
		}
		LazySingleton<ChunkManager>.Instance.UnregisterChunks(runtimeConstructorParts, ChunkManagerLayerType.WsoConstructorParts);
		runtimeConstructorParts.Clear();
		ReleaseLutHandles();
	}

	private void ClearOptimizedInstances()
	{
		foreach (GameObject loadedOptimizedInstance in loadedOptimizedInstances)
		{
			if (loadedOptimizedInstance != null)
			{
				UnityEngine.Object.Destroy(loadedOptimizedInstance);
			}
		}
		loadedOptimizedInstances.Clear();
		foreach (AsyncOperationHandle<GameObject> loadedOptimizedHandle in loadedOptimizedHandles)
		{
			if (loadedOptimizedHandle.IsValid())
			{
				Addressables.Release(loadedOptimizedHandle);
			}
		}
		loadedOptimizedHandles.Clear();
	}

	private void ReleaseLutHandles()
	{
		foreach (AsyncOperationHandle<Texture2D> loadedLutHandle in loadedLutHandles)
		{
			if (loadedLutHandle.IsValid())
			{
				Addressables.Release(loadedLutHandle);
			}
		}
		loadedLutHandles.Clear();
	}

	private void OnRepairStateChanged()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		pendingRuntimePartsRebuild = true;
		if (!runtimePartsLoading)
		{
			if (IsVisible)
			{
				RequestRuntimePartsRebuild(async: false);
			}
			else if (chunkVisibilityState == ChunkVisibilityState.Prewarm && ShouldLoadRuntimeStagesInPrewarm())
			{
				RequestRuntimePartsRebuild();
			}
		}
	}

	public void RepairAllStages()
	{
		if (data != null)
		{
			TownUtils.RepairHouse(data);
			_ = 0;
		}
	}

	public void DuplicateAndRepairAllStages()
	{
		_ = data;
	}

	public void RepairStage(int stageIndex)
	{
		if (data == null)
		{
			return;
		}
		WsoRepairablePartData componentData = data.GetComponentData<WsoRepairablePartData>();
		if (componentData == null)
		{
			Debug.LogWarning("[Wso] Cannot repair - no WsoRepairablePartData component on " + base.name);
			return;
		}
		ConstructorPartReplacementConfig replacementConfig = GetReplacementConfig();
		if (replacementConfig == null)
		{
			Debug.LogWarning("[Wso] Cannot repair - no replacement config found for " + base.name);
			return;
		}
		int num = componentData.RepairStage(stageIndex, replacementConfig);
		if (num > 0)
		{
			data.NotifyRepairStateChanged();
			Debug.Log($"[Wso] Repaired {num} parts in stage {stageIndex} of {base.name}");
		}
	}

	public void ResetAllStages()
	{
		if (data != null)
		{
			WsoRepairablePartData componentData = data.GetComponentData<WsoRepairablePartData>();
			if (componentData != null)
			{
				componentData.ResetAllStages();
				data.NotifyRepairStateChanged();
			}
		}
	}

	private ConstructorPartReplacementConfig GetReplacementConfig()
	{
		ConstructorPartReplacementConfig constructorPartReplacementConfig = Data?.Definition.ReplacementConfig;
		if (constructorPartReplacementConfig == null)
		{
			string presetIdFromStages = GetPresetIdFromStages();
			if (!string.IsNullOrEmpty(presetIdFromStages))
			{
				constructorPartReplacementConfig = ConstructorPartReplacementService.GetReplacementConfigForPreset(presetIdFromStages);
			}
		}
		return constructorPartReplacementConfig;
	}

	private string GetPresetIdFromStages()
	{
		WsoRepairablePartData wsoRepairablePartData = data?.GetComponentData<WsoRepairablePartData>();
		if (wsoRepairablePartData == null || wsoRepairablePartData.Stages.Count == 0)
		{
			return null;
		}
		WsoStageData wsoStageData = wsoRepairablePartData.Stages[0];
		if (wsoStageData.PartsData.Count == 0)
		{
			return null;
		}
		string originalModelId = wsoStageData.PartsData[0].OriginalModelId;
		if (string.IsNullOrEmpty(originalModelId))
		{
			return null;
		}
		int num = originalModelId.IndexOf('-');
		if (num <= 0)
		{
			return null;
		}
		return originalModelId.Substring(0, num);
	}

	private void HandleHiddenChanged(bool isHidden)
	{
		RefreshVisuals();
	}
}
