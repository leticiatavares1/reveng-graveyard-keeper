using System.Collections.Generic;
using JetBrains.Annotations;
using LazyBearTechnology;
using LinqTools;
using Pathfinding;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

public class GameScene : SerializedMonoBehaviour
{
	private const int MAX_OBJ_SPAWN_COUNT_PER_FRAME = 50;

	[SerializeField]
	private string id;

	private GameSceneData gameSceneData;

	[SerializeField]
	[CanBeNull]
	private GameSceneLinksCollection gameSceneLinksCollection;

	private SceneWaypointContent waypointContent;

	[SerializeField]
	private GameSceneConfig gameSceneConfig;

	[SerializeField]
	[HideInInspector]
	private ChunkableObjectComponent[] chunkComponents;

	[SerializeField]
	[HideInInspector]
	private List<ConstructorPart> constructorParts;

	[OdinSerialize]
	[HideInInspector]
	public List<BakedChunkableObjectComponentData> bakedChunkableObjectComponentDatas = new List<BakedChunkableObjectComponentData>();

	private Dictionary<SGuid, Wgo> wgoViewCache = new Dictionary<SGuid, Wgo>();

	private static Dictionary<SGuid, Wgo> globalWgoViewCache = new Dictionary<SGuid, Wgo>();

	private Dictionary<SGuid, DropView> dropViewCache = new Dictionary<SGuid, DropView>();

	private Dictionary<SGuid, Wso> wsoViewCache = new Dictionary<SGuid, Wso>();

	private static Dictionary<SGuid, Wso> globalWsoViewCache = new Dictionary<SGuid, Wso>();

	private GDPoint[] sceneGdPoints;

	private List<IChunkableObject> chunks;

	private List<WorldZone> worldZones;

	private bool isStartCompleted;

	public bool DisableUnloadOnTeleport => gameSceneConfig?.disableUnloadOnTeleport ?? false;

	public bool IsStartCompleted => isStartCompleted;

	public bool IsInitialized => gameSceneData != null;

	public string Id => id;

	public GameSceneConfig GameSceneConfig => gameSceneConfig;

	public SceneWaypointContent SceneWaypointContent => waypointContent;

	public GameSceneData GameSceneData => gameSceneData;

	public GDPoint[] SceneGdPoints => sceneGdPoints;

	public List<Wgo> Wgos => wgoViewCache.Values.ToList();

	public static void ClearGlobalCaches()
	{
		globalWgoViewCache = new Dictionary<SGuid, Wgo>();
		globalWsoViewCache = new Dictionary<SGuid, Wso>();
	}

	public Wgo AddWgoData(WgoData data, bool recheckVisibilityOnSpawn = false)
	{
		MainGame.Instance.GameSave.worldData.AddWgoData(data, recheckVisibilityOnSpawn);
		return wgoViewCache.GetValueOrDefault(data.UniqueId);
	}

	public Wgo AddWgoData(string id, Vector3 position)
	{
		WgoData data = new WgoData(id, position, this.id);
		return AddWgoData(data);
	}

	public static void ClearGlobalWgoViewCache()
	{
		globalWgoViewCache = new Dictionary<SGuid, Wgo>();
	}

	public static Wgo GetWgoViewGlobal(SGuid uniqueId)
	{
		return globalWgoViewCache.GetValueOrDefault(uniqueId);
	}

	public static void RedrawWgosWithInteractionEvents()
	{
		QuestSystemData questSystemData = MainGame.Instance?.GameSave?.questSystemData;
		foreach (Wgo value in globalWgoViewCache.Values)
		{
			if (!(value == null) && value.Data != null)
			{
				bool num = value.Data.Events.Count > 0;
				bool flag = questSystemData?.WgoHasReadyToFinishQuest(value.Id) ?? false;
				if (num || flag)
				{
					value.DrawWidgets();
				}
			}
		}
	}

	public DropView GetDropView(Item item)
	{
		if (TryGetDropView(item, out var dropView))
		{
			return dropView;
		}
		Debug.LogWarning("No drop view found for item: [" + item.id + "]");
		return null;
	}

	public bool TryGetDropView(Item item, out DropView dropView)
	{
		dropView = null;
		if (item == null)
		{
			return false;
		}
		return dropViewCache.TryGetValue(item.UniqueId, out dropView);
	}

	public FightingLevel GetFightingLevel(string id)
	{
		return MainGame.GetFightingLevel(id);
	}

	public WorldZone GetWorldZoneById(string zoneId)
	{
		if (string.IsNullOrEmpty(zoneId) || worldZones == null)
		{
			return null;
		}
		return worldZones.Find((WorldZone zone) => zone != null && zone.Id == zoneId);
	}

	private async void Start()
	{
		Debug.Log("#time_test# [GameScene] Start entered for id=" + id);
		try
		{
			if (gameSceneData != null)
			{
				return;
			}
			gameSceneData = MainGame.Instance.GameSave.worldData.GetGameSceneDataById(id);
			MainGame.PlayerData.currentGameSceneId = id;
			MainGame.PlayerController.CurrentGameScene = this;
			if (gameSceneData == null)
			{
				Debug.LogError("Not found game scene with id: " + id);
				return;
			}
			sceneGdPoints = GetGDPointsExcludingEditorContent();
			MainGame.Instance.GameSave.worldData.gdPointsData.InitScenePointsFromGameScene(this, sceneGdPoints);
			worldZones = await SpawnWorldZonesFromData();
			await SpawnWgoViewsFromData();
			await SpawnWsoViewsFromData();
			await SpawnDropsFromData();
			await SpawnTechPointsFromData();
			gameSceneData.OnWgoDataAdd += HandleWgoAdd;
			gameSceneData.OnWgoDataRemove += HandleWgoRemove;
			gameSceneData.OnWsoDataAdd += HandleWsoAdd;
			gameSceneData.OnWsoDataRemove += HandleWsoRemoved;
			gameSceneData.OnDropAdd += HandleDropAdd;
			gameSceneData.OnDropRemove += HandleDropRemove;
			await ScanChurchGraph();
			AddWgosToWorldZones();
			gameSceneData.ProcessQueuedDrops();
			chunks = new List<IChunkableObject>();
			for (int i = 0; i < chunkComponents.Length; i++)
			{
				if (!(chunkComponents[i] == null))
				{
					chunkComponents[i].UpdateChunkVisibility(isVisible: false);
					chunks.Add(chunkComponents[i]);
				}
			}
			for (int num = chunks.Count - 1; num >= 0; num--)
			{
				if (chunks[num] == null)
				{
					chunks.RemoveAt(num);
				}
			}
			for (int j = 0; j < constructorParts.Count; j++)
			{
				if (!(constructorParts[j] == null))
				{
					chunks.Add(constructorParts[j]);
				}
			}
			for (int k = 0; k < bakedChunkableObjectComponentDatas.Count; k++)
			{
				if (bakedChunkableObjectComponentDatas[k] != null)
				{
					chunks.Add(bakedChunkableObjectComponentDatas[k]);
				}
			}
			LazySingleton<ChunkManager>.Instance.RegisterChunks(chunks, ChunkManagerLayerType.StaticObjects);
			LazySingleton<ScenePoolPathRegistry>.Instance.RegisterScenePaths(id, CollectScenePoolPaths());
			if (!string.IsNullOrEmpty(MainGame.ConveyorPresetLoadOnNewGame) && !MainGame.Instance.GameSave.conveyorSystemData.isInitialized)
			{
				ConveyorPreset.LoadPreset(MainGame.ConveyorPresetLoadOnNewGame);
				MainGame.Instance.GameSave.conveyorSystemData.isInitialized = true;
			}
			if ((bool)gameSceneLinksCollection)
			{
				gameSceneLinksCollection.RefreshLinks();
			}
			MainGame.Instance.riverDropSystem?.HandleSceneReady(this);
		}
		finally
		{
			isStartCompleted = true;
		}
	}

	public async Awaitable<bool> WaitForStartCompleted(float timeoutSeconds = 30f)
	{
		float timeoutAt = Time.realtimeSinceStartup + timeoutSeconds;
		while (!isStartCompleted)
		{
			if (this == null)
			{
				return false;
			}
			if (Time.realtimeSinceStartup >= timeoutAt)
			{
				Debug.LogWarning("[GameScene] WaitForStartCompleted timeout for id=" + id);
				return false;
			}
			await Awaitable.NextFrameAsync();
		}
		return true;
	}

	private HashSet<string> CollectScenePoolPaths()
	{
		HashSet<string> hashSet = new HashSet<string>();
		if (bakedChunkableObjectComponentDatas != null)
		{
			for (int i = 0; i < bakedChunkableObjectComponentDatas.Count; i++)
			{
				BakedChunkableObjectComponentData bakedChunkableObjectComponentData = bakedChunkableObjectComponentDatas[i];
				if (bakedChunkableObjectComponentData != null && !string.IsNullOrEmpty(bakedChunkableObjectComponentData.pathToObject))
				{
					hashSet.Add(bakedChunkableObjectComponentData.pathToObject);
				}
			}
		}
		if (constructorParts != null)
		{
			for (int j = 0; j < constructorParts.Count; j++)
			{
				ConstructorPart constructorPart = constructorParts[j];
				if (constructorPart != null && constructorPart.HasChildPath)
				{
					hashSet.Add(constructorPart.constructorPartChildData.pathToObject);
				}
			}
		}
		if (gameSceneData != null)
		{
			for (int k = 0; k < gameSceneData.wgoDataList.Count; k++)
			{
				WgoPartPool.CollectAddressableKeysForWgoData(gameSceneData.wgoDataList[k], hashSet);
			}
		}
		return hashSet;
	}

	public void RestoreStaticObjectsAfterTeleport()
	{
		if (!IsInitialized || chunks == null || chunks.Count == 0)
		{
			return;
		}
		ChunkManager instance = LazySingleton<ChunkManager>.Instance;
		if (instance == null)
		{
			return;
		}
		instance.RegisterChunks(chunks, ChunkManagerLayerType.StaticObjects);
		foreach (IChunkableObject chunk in chunks)
		{
			if (chunk != null && (!(chunk is Object @object) || !(@object == null)))
			{
				instance.RequestVisibilityRecheck(chunk);
			}
		}
		LazyTerrain componentInChildren = GetComponentInChildren<LazyTerrain>(includeInactive: true);
		if (!(componentInChildren != null) || componentInChildren.meshesData.Count <= 0)
		{
			return;
		}
		instance.RegisterChunks(componentInChildren.meshesData, ChunkManagerLayerType.StaticObjects);
		foreach (LazyTerrainMeshData meshesDatum in componentInChildren.meshesData)
		{
			instance.RequestVisibilityRecheck(meshesDatum);
		}
	}

	private void OnDestroy()
	{
		Debug.Log("[GameScene] OnDestroy for id=" + id);
		if (!Application.isPlaying || gameSceneData == null)
		{
			return;
		}
		gameSceneData.OnWgoDataAdd -= HandleWgoAdd;
		gameSceneData.OnWgoDataRemove -= HandleWgoRemove;
		gameSceneData.OnWsoDataAdd -= HandleWsoAdd;
		gameSceneData.OnWsoDataRemove -= HandleWsoRemoved;
		gameSceneData.OnDropAdd -= HandleDropAdd;
		gameSceneData.OnDropRemove -= HandleDropRemove;
		LazySingleton<ChunkManager>.Instance.UnregisterChunks(chunks, ChunkManagerLayerType.StaticObjects);
		foreach (ConstructorPart constructorPart in constructorParts)
		{
			constructorPart.UpdateChunkVisibility(isVisible: false);
		}
		foreach (BakedChunkableObjectComponentData bakedChunkableObjectComponentData in bakedChunkableObjectComponentDatas)
		{
			bakedChunkableObjectComponentData.UpdateChunkVisibility(isVisible: false);
		}
		List<IChunkableObject> list = new List<IChunkableObject>();
		List<IChunkableObject> list2 = new List<IChunkableObject>();
		foreach (SGuid item in new List<SGuid>(wgoViewCache.Keys))
		{
			Wgo valueOrDefault = wgoViewCache.GetValueOrDefault(item);
			if (valueOrDefault != null)
			{
				valueOrDefault.UpdateChunkVisibilityState(ChunkVisibilityState.OutOfRange);
				valueOrDefault.UpdateChunkVisibility(isVisible: false);
				if (valueOrDefault.RegisteredInChunker)
				{
					if ((valueOrDefault.Data?.Definition?.isMovable).GetValueOrDefault())
					{
						list2.Add(valueOrDefault);
					}
					else
					{
						list.Add(valueOrDefault);
					}
					valueOrDefault.RegisteredInChunker = false;
				}
			}
			globalWgoViewCache.Remove(item);
		}
		LazySingleton<ChunkManager>.Instance.UnregisterChunks(list, ChunkManagerLayerType.StaticWgo);
		LazySingleton<ChunkManager>.Instance.UnregisterChunks(list2, ChunkManagerLayerType.DynamicWgo);
		wgoViewCache.Clear();
		List<IChunkableObject> list3 = new List<IChunkableObject>();
		foreach (SGuid item2 in new List<SGuid>(wsoViewCache.Keys))
		{
			Wso valueOrDefault2 = wsoViewCache.GetValueOrDefault(item2);
			if (valueOrDefault2 != null && valueOrDefault2.RegisteredInChunker)
			{
				list3.Add(valueOrDefault2);
				valueOrDefault2.RegisteredInChunker = false;
			}
			globalWsoViewCache.Remove(item2);
		}
		LazySingleton<ChunkManager>.Instance.UnregisterChunks(list3, ChunkManagerLayerType.StaticWso);
		wsoViewCache.Clear();
		ReleaseFightingBakingContexts();
		LazySingleton<ScenePoolPathRegistry>.Instance.UnregisterScenePaths(id);
	}

	private void ReleaseFightingBakingContexts()
	{
		FightingLevel[] componentsInChildren = GetComponentsInChildren<FightingLevel>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i] != null)
			{
				componentsInChildren[i].ForceUnregisterBakingContext();
			}
		}
		FightingStage[] componentsInChildren2 = GetComponentsInChildren<FightingStage>(includeInactive: true);
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			if (componentsInChildren2[j] != null)
			{
				componentsInChildren2[j].ForceUnregisterBakingContext();
			}
		}
	}

	private async Awaitable<List<WorldZone>> SpawnWorldZonesFromData()
	{
		List<WorldZone> spawnedZones = new List<WorldZone>();
		foreach (WorldZoneData worldZone2 in gameSceneData.worldZones)
		{
			WorldZone worldZone = TrySpawnWorldZoneFromData(worldZone2);
			if ((bool)worldZone)
			{
				spawnedZones.Add(worldZone);
			}
		}
		await Awaitable.NextFrameAsync();
		return spawnedZones;
	}

	private async Awaitable SpawnWgoViewsFromData()
	{
		List<IChunkableObject> staticViews = new List<IChunkableObject>();
		List<IChunkableObject> dynamicViews = new List<IChunkableObject>();
		int spawnCount = 0;
		List<WgoData> list = new List<WgoData>(gameSceneData.wgoDataList);
		Debug.Log($"SpawnWgoViewsFromData: {list.Count}");
		foreach (WgoData wgoData in list)
		{
			if (spawnCount >= 50)
			{
				spawnCount = 0;
				await Awaitable.NextFrameAsync();
			}
			Wgo wgo = CreateWgoView(wgoData, ignoreChunkRegistration: true);
			if (wgoData.Definition?.isMovable ?? false)
			{
				dynamicViews.Add(wgo);
			}
			else
			{
				staticViews.Add(wgo);
			}
			wgo.RegisteredInChunker = true;
			spawnCount++;
		}
		LazySingleton<ChunkManager>.Instance.RegisterChunks(staticViews, ChunkManagerLayerType.StaticWgo);
		LazySingleton<ChunkManager>.Instance.RegisterChunks(dynamicViews, ChunkManagerLayerType.DynamicWgo);
	}

	private async Awaitable SpawnWsoViewsFromData()
	{
		if (gameSceneData.wsoDataList == null || gameSceneData.wsoDataList.Count == 0)
		{
			return;
		}
		List<IChunkableObject> createdWsos = new List<IChunkableObject>();
		int spawnCount = 0;
		List<WsoData> list = new List<WsoData>(gameSceneData.wsoDataList);
		foreach (WsoData wsoData in list)
		{
			if (spawnCount >= 50)
			{
				spawnCount = 0;
				await Awaitable.NextFrameAsync();
			}
			Wso wso = CreateWsoView(wsoData);
			if (wso != null && !wso.RegisteredInChunker)
			{
				wso.RegisteredInChunker = true;
				createdWsos.Add(wso);
			}
		}
		if (createdWsos.Count > 0)
		{
			LazySingleton<ChunkManager>.Instance.RegisterChunks(createdWsos, ChunkManagerLayerType.StaticWso);
		}
	}

	private async Awaitable SpawnDropsFromData()
	{
		foreach (DropData droppedItem in gameSceneData.droppedItems)
		{
			CreateDropView(droppedItem);
		}
		await Awaitable.NextFrameAsync();
	}

	private async Awaitable SpawnTechPointsFromData()
	{
		foreach (TechPointDropData techPointDrop in gameSceneData.techPointDrops)
		{
			TechPointDrop.Spawn(techPointDrop, base.transform);
		}
		await Awaitable.NextFrameAsync();
	}

	private async Awaitable ScanChurchGraph()
	{
		if (AstarPath.active.graphs[6] is RecastGraph recastGraph)
		{
			HashSet<IChunkableObject> allChunkableObjectsInBounds = LazySingleton<ChunkManager>.Instance.GetAllChunkableObjectsInBounds(recastGraph.bounds);
			foreach (IChunkableObject item in allChunkableObjectsInBounds)
			{
				item?.UpdateFlag(ChunkingIgnoreType.Building, newValue: true);
			}
			recastGraph.Scan();
			foreach (IChunkableObject item2 in allChunkableObjectsInBounds)
			{
				item2?.UpdateFlag(ChunkingIgnoreType.Building, newValue: false);
			}
		}
		await Awaitable.NextFrameAsync();
	}

	private void HandleWgoAdd(WgoData data, bool recheckVisibilityOnSpawn)
	{
		HashSet<string> paths = new HashSet<string>();
		WgoPartPool.CollectAddressableKeysForWgoData(data, paths);
		LazySingleton<ScenePoolPathRegistry>.Instance.RegisterScenePaths(id, paths);
		CreateWgoView(data, ignoreChunkRegistration: false, recheckVisibilityOnSpawn);
	}

	private Wgo CreateWgoView(WgoData data, bool ignoreChunkRegistration = false, bool recheckVisibilityOnSpawn = false)
	{
		Wgo wgo = Wgo.Spawn(data, base.transform, registerInChunkManagerIfStatic: true, ignoreChunkRegistration, applyDefaultWgoPartState: false, recheckVisibilityOnSpawn);
		wgoViewCache.Add(data.UniqueId, wgo);
		globalWgoViewCache.Add(data.UniqueId, wgo);
		return wgo;
	}

	private void HandleWgoRemove(WgoData data)
	{
		globalWgoViewCache.Remove(data.UniqueId);
		if (wgoViewCache.Remove(data.UniqueId, out var value) && !(value == null) && !value.IsDespawning)
		{
			value.DespawnAfterDataWasRemoved();
		}
	}

	private void HandleWsoAdd(WsoData data)
	{
		CreateWsoView(data);
	}

	private Wso CreateWsoView(WsoData data)
	{
		Wso wso = Wso.Spawn(data, base.transform);
		wsoViewCache.Add(data.UniqueId, wso);
		globalWsoViewCache.Add(data.UniqueId, wso);
		return wso;
	}

	private void HandleWsoRemoved(WsoData data)
	{
		globalWsoViewCache.Remove(data.UniqueId);
		if (wsoViewCache.Remove(data.UniqueId, out var value))
		{
			Object.Destroy(value.gameObject);
		}
	}

	private void HandleDropAdd(DropData drop)
	{
		CreateDropView(drop);
	}

	private void HandleDropRemove(DropData drop)
	{
		if (dropViewCache.Remove(drop.UniqueId, out var value))
		{
			value.DespawnView();
		}
	}

	private DropView CreateDropView(DropData drop)
	{
		DropView dropView = DropView.SpawnDrop(drop, base.transform);
		dropViewCache.Add(drop.UniqueId, dropView);
		return dropView;
	}

	private void AddWgosToWorldZones()
	{
		foreach (WorldZone worldZone in worldZones)
		{
			worldZone.AddWgosOnGameSceneStart();
		}
	}

	private GDPoint[] GetGDPointsExcludingEditorContent()
	{
		return GetComponentsInChildren<GDPoint>(includeInactive: true);
	}

	private GameSceneConfig GetResolvedGameSceneConfig()
	{
		return MainGame.Instance.gameSceneConfigs.Find((GameSceneConfig c) => c.name == gameSceneData.id);
	}

	private WorldZone TrySpawnWorldZoneFromData(WorldZoneData worldZoneData, SceneWgoContentPart preferredContentPart = null)
	{
		if (MainGame.Instance.fightingLevelSystem.TryGetExistingWorldZone(worldZoneData.id, out var worldZone))
		{
			return worldZone;
		}
		Transform parent = ResolveWorldZoneParent(worldZoneData, preferredContentPart);
		return WorldZone.Spawn(worldZoneData, parent);
	}

	private Transform ResolveWorldZoneParent(WorldZoneData worldZoneData, SceneWgoContentPart preferredContentPart)
	{
		SceneWgoContentPart sceneWgoContentPart = preferredContentPart;
		if (sceneWgoContentPart == null && gameSceneConfig.TryGetLoadedContent(worldZoneData.contentPartName, out var contentDataInstance) && contentDataInstance != null && contentDataInstance.TryGetComponent<SceneWgoContentPart>(out var component))
		{
			sceneWgoContentPart = component;
		}
		if (!(sceneWgoContentPart != null))
		{
			return base.transform;
		}
		return sceneWgoContentPart.transform;
	}
}
