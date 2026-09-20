using System;
using System.Collections;
using System.Collections.Generic;
using LazyBearTechnology;
using LinqTools;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class FightingLevel : SceneWgoContentPart, IBakingContext
{
	[Serializable]
	private class SerializedStagePrefabRef
	{
		public AssetReferenceGameObject fightingStagePrefabRef;

		public int stageMask;

		[NonSerialized]
		public AsyncOperationHandle<GameObject> prefabHandle;

		[NonSerialized]
		public GameObject fightingStagePrefab;

		public void ApplyByStageId(int stageId, Transform parent)
		{
			if (stageId == 0 || (stageMask & (1 << stageId)) == 0)
			{
				if ((bool)fightingStagePrefab && prefabHandle.IsValid())
				{
					UnityEngine.Object.Destroy(fightingStagePrefab);
					prefabHandle.Release();
					fightingStagePrefab = null;
				}
			}
			else
			{
				if (!fightingStagePrefab || !prefabHandle.IsValid())
				{
					prefabHandle = fightingStagePrefabRef.InstantiateAsync(parent);
					fightingStagePrefab = prefabHandle.WaitForCompletion();
				}
				ApplyChunkableVisibilityForInstantiatedStage(fightingStagePrefab, stageId);
			}
		}

		private static void ApplyChunkableVisibilityForInstantiatedStage(GameObject stageRoot, int stageId)
		{
			if (!stageRoot)
			{
				return;
			}
			FightingStage fightingStage = stageRoot.GetComponent<FightingStage>();
			if (!fightingStage)
			{
				fightingStage = stageRoot.GetComponentInChildren<FightingStage>(includeInactive: true);
			}
			if (fightingStage != null)
			{
				fightingStage.ApplyStageFromRuntimeInstance(stageId);
				return;
			}
			ChunkableObjectComponent[] componentsInChildren = stageRoot.GetComponentsInChildren<ChunkableObjectComponent>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].CustomVisibilityDisabled = false;
			}
		}
	}

	[Serializable]
	public class FightinStageWorldObject
	{
		public enum ObjType
		{
			Wgo,
			Wso
		}

		public SGuid uniqueId;

		public ObjType type;

		public int stageMask;
	}

	[Serializable]
	public class FightingStageWorldZone
	{
		public string worldZoneId;

		public int stageMask;
	}

	public string id;

	[SerializeField]
	private FightingLevelPreset fightingLevelPreset;

	[SerializeField]
	private List<FightingLine> fightingLines = new List<FightingLine>();

	[SerializeField]
	private LazyConsts.Navigation.Graph navigationGraph = LazyConsts.Navigation.Graph.None;

	[SerializeField]
	private List<BoxCollider> zoneDefineColliders = new List<BoxCollider>();

	[SerializeField]
	private List<GameObject> borderObjects = new List<GameObject>();

	[SerializeField]
	private string levelGdPointId;

	[SerializeField]
	private string levelWorldZoneId;

	[SerializeField]
	private string fightbackGdPointId;

	[SerializeField]
	private string fightbackAfterWinGdPointId;

	[SerializeField]
	private bool isInsideIndoor;

	[SerializeField]
	private bool isInsideDungeon;

	[Space]
	[SerializeField]
	private List<AlliesSpawn> alliesSpawns = new List<AlliesSpawn>();

	[Space]
	public List<EnemySpawnZone> customSpawnZones = new List<EnemySpawnZone>();

	[SerializeField]
	private FightingCapturePoint baseCapturePoint;

	[SerializeField]
	private bool hideCapturePoint;

	public bool graphWasInitialized;

	private int currentStageId;

	private Dictionary<int, List<EnemySpawnZone>> spawnZones = new Dictionary<int, List<EnemySpawnZone>>();

	private Dictionary<FightingLine, int> pointByLineDict = new Dictionary<FightingLine, int>();

	private HashSet<ZombieFog> zombieFogs;

	[SerializeField]
	private List<SerializedStagePrefabRef> stagePrefabsRefs = new List<SerializedStagePrefabRef>();

	[SerializeField]
	private List<FightingStage> nonPrefabStages = new List<FightingStage>();

	[SerializeField]
	private List<FightinStageWorldObject> stageWorldObjects = new List<FightinStageWorldObject>();

	[SerializeField]
	private List<FightingStageWorldZone> stageWorldZones = new List<FightingStageWorldZone>();

	[SerializeField]
	private List<ChunkableObjectComponent> nonStageChunkableObjects = new List<ChunkableObjectComponent>();

	[SerializeField]
	private List<BakedChunkableObjectComponentData> bakedData = new List<BakedChunkableObjectComponentData>();

	[SerializeField]
	private bool disabledBakingContext;

	[NonSerialized]
	private readonly List<BakedChunkableObjectComponentData> registeredBakedChunkCaches = new List<BakedChunkableObjectComponentData>();

	[NonSerialized]
	private readonly List<ChunkableObjectComponent> registeredLiveChunkables = new List<ChunkableObjectComponent>();

	public FightingLevelPreset FightingLevelPreset => fightingLevelPreset;

	public IReadOnlyList<BoxCollider> ZoneDefineColliders => zoneDefineColliders;

	public IReadOnlyDictionary<int, List<EnemySpawnZone>> SpawnZones => spawnZones;

	public LazyConsts.Navigation.Graph NavigationGraph => navigationGraph;

	public FightingCapturePoint BaseCapturePoint => baseCapturePoint;

	public string LevelGdPointId => levelGdPointId;

	public string LevelWorldZoneId => levelWorldZoneId;

	public string FightbackGdPointId => fightbackGdPointId;

	public string FightbackAfterWinGdPointId => fightbackAfterWinGdPointId;

	public bool IsInsideIndoor => isInsideIndoor;

	public bool IsInsideDungeon => isInsideDungeon;

	public string EnvironmentPreset
	{
		get
		{
			if (!isInsideIndoor)
			{
				if (!isInsideDungeon)
				{
					return "outdoor";
				}
				return "dungeons";
			}
			return "indoor";
		}
	}

	public IReadOnlyList<FightingLine> FightingLines => fightingLines;

	public IReadOnlyList<FightingStage> NonPrefabStages => nonPrefabStages;

	public IReadOnlyList<FightinStageWorldObject> StageWorldObjects => stageWorldObjects;

	public IReadOnlyList<FightingStageWorldZone> StageWorldZones => stageWorldZones;

	public HashSet<ZombieFog> ZombieFogs => zombieFogs;

	public Bounds LevelBounds
	{
		get
		{
			if (zoneDefineColliders.Count == 0)
			{
				return default(Bounds);
			}
			Bounds bounds = zoneDefineColliders[0].bounds;
			for (int i = 1; i < zoneDefineColliders.Count; i++)
			{
				bounds.Encapsulate(zoneDefineColliders[i].bounds);
			}
			return bounds;
		}
	}

	public List<AlliesSpawn> AlliesSpawns
	{
		get
		{
			if (alliesSpawns == null || alliesSpawns.Count == 0)
			{
				alliesSpawns = GetComponentsInChildren<AlliesSpawn>().ToList();
			}
			return alliesSpawns;
		}
	}

	public List<BakedChunkableObjectComponentData> GetBakedData => bakedData;

	public int Editor_BakingContextPriority => 0;

	public GDPointData GetLevelGdPointData()
	{
		if (string.IsNullOrEmpty(levelGdPointId))
		{
			return null;
		}
		return MainGame.WorldData.gdPointsData.GetGDPointDataById(levelGdPointId);
	}

	public FightingCapturePoint FindCapturePointForFlagStand(Vector3 worldPosition)
	{
		FightingCapturePoint result = null;
		float num = float.MaxValue;
		for (int i = 0; i < fightingLines.Count; i++)
		{
			FightingLine fightingLine = fightingLines[i];
			for (int j = 0; j < fightingLine.sectors.Count; j++)
			{
				FightingCapturePoint point = fightingLine.sectors[j].point;
				if ((bool)point && point.MatchesAllyFlagStandPosition(worldPosition))
				{
					float sqrMagnitude = (worldPosition - point.transform.position).XZ().sqrMagnitude;
					if (!(sqrMagnitude >= num))
					{
						num = sqrMagnitude;
						result = point;
					}
				}
			}
		}
		return result;
	}

	public IEnumerator Init()
	{
		baseCapturePoint.Init();
		baseCapturePoint.SetOwnedByTeam(LazyConsts.Fighting.TeamType.Player);
		for (int i = 0; i < fightingLines.Count; i++)
		{
			FightingLine fightingLine = fightingLines[i];
			fightingLine.lineIdx = i;
			for (int j = 0; j < fightingLine.sectors.Count; j++)
			{
				fightingLine.sectors[j].sectorIdx = j;
			}
		}
		foreach (FightingLine fightingLine2 in fightingLines)
		{
			if (!pointByLineDict.ContainsKey(fightingLine2))
			{
				spawnZones.TryAdd(fightingLine2.lineIdx, fightingLine2.spawnZones);
				pointByLineDict.TryAdd(fightingLine2, spawnZones.Count - 1);
			}
		}
		if (!graphWasInitialized)
		{
			graphWasInitialized = true;
			yield return InitGraphAsync();
		}
	}

	public void OnPlay()
	{
		if (LazySingleton<FightingGameController>.Instance.UIFightingOverlayData == null)
		{
			return;
		}
		foreach (FightingLine fightingLine in fightingLines)
		{
			foreach (FightingSector sector in fightingLine.sectors)
			{
				if (sector.point.OwnedByTeam == LazyConsts.Fighting.TeamType.Player)
				{
					LazySingleton<FightingGameController>.Instance.UIFightingOverlayData.TrackCapturePoint(sector.point);
				}
			}
		}
		LazySingleton<FightingGameController>.Instance.UIFightingOverlayData.TrackCapturePoint(baseCapturePoint);
	}

	public void OnStop()
	{
		if (LazySingleton<FightingGameController>.Instance.UIFightingOverlayData == null)
		{
			return;
		}
		foreach (FightingLine fightingLine in fightingLines)
		{
			foreach (FightingSector sector in fightingLine.sectors)
			{
				LazySingleton<FightingGameController>.Instance.UIFightingOverlayData.UntrackCapturePoint(sector.point);
			}
		}
		LazySingleton<FightingGameController>.Instance.UIFightingOverlayData.UntrackCapturePoint(baseCapturePoint);
	}

	public void InitGraph()
	{
		Bounds levelBounds = LevelBounds;
		LazySingleton<GlobalNavigationManager>.Instance.InitRecastGraph(navigationGraph, levelBounds.center, levelBounds.size.XZ2(), dontUseRecastFloor: true, levelBounds.size.y);
	}

	public void SetSpawnerActive(int lineId, string spawnZoneName, bool isActive)
	{
		if (spawnZones.TryGetValue(lineId, out var value))
		{
			EnemySpawnZone enemySpawnZone = value.Find((EnemySpawnZone z) => z.id == spawnZoneName);
			if (!enemySpawnZone)
			{
				Debug.LogWarning($"SpawnZone with id {spawnZoneName} on line {lineId} not found!");
			}
			else
			{
				enemySpawnZone.gameObject.SetActive(isActive);
			}
		}
	}

	public void StartLevel()
	{
		LazySingleton<FightingGameController>.Instance.Play(id);
		baseCapturePoint.OnCapturedByTeam += HandleCaptureBy;
		foreach (ZombieFog zombieFog in zombieFogs)
		{
			zombieFog.ResetActiveState();
		}
	}

	public void StopLevel()
	{
		baseCapturePoint.OnCapturedByTeam -= HandleCaptureBy;
		LazySingleton<FightingGameController>.Instance.Stop();
	}

	private void Awake()
	{
		zombieFogs = new HashSet<ZombieFog>();
	}

	private IEnumerator InitGraphAsync()
	{
		Bounds levelBounds = LevelBounds;
		yield return LazySingleton<GlobalNavigationManager>.Instance.InitRecastGraphAsync(navigationGraph, levelBounds.center, levelBounds.size.XZ2(), dontUseRecastFloor: true, levelBounds.size.y);
	}

	public void SetActive(bool isActive)
	{
		foreach (ZombieFog zombieFog in zombieFogs)
		{
			zombieFog.ResetActiveState();
		}
		foreach (FightingLine fightingLine in fightingLines)
		{
			fightingLine.SetActive(isActive);
		}
		if (!hideCapturePoint)
		{
			baseCapturePoint.SetActiveState(isActive);
		}
		else
		{
			baseCapturePoint.SetActiveState(isActive: false);
		}
	}

	public void InitLines()
	{
		foreach (FightingLine fightingLine in fightingLines)
		{
			fightingLine.Init();
		}
	}

	public void Deactivate()
	{
		baseCapturePoint.DeInit();
		baseCapturePoint.gameObject.SetActive(value: false);
		foreach (FightingLine fightingLine in fightingLines)
		{
			fightingLine.Deinit();
		}
	}

	public bool GetAvailablePositionForSpawning(int lineIndex, out Vector3 position, out List<PathfindingPenalty> penalties)
	{
		penalties = new List<PathfindingPenalty>();
		position = Vector3.zero;
		if (spawnZones.Count == 0 || lineIndex < 0 || lineIndex >= spawnZones.Count)
		{
			return false;
		}
		IEnumerable<EnemySpawnZone> source = spawnZones[lineIndex].Where((EnemySpawnZone zone) => zone.gameObject.activeSelf);
		if (!source.Any())
		{
			return false;
		}
		position = source.ToList().GetRandom().GetRandomPosFromZone(out penalties);
		return true;
	}

	public FightingLine GetFightingLine(int lineIndex)
	{
		if (lineIndex >= 0 && lineIndex < fightingLines.Count)
		{
			return fightingLines[lineIndex];
		}
		return null;
	}

	public void SetBorderObjectsActive(bool isActive)
	{
		foreach (GameObject borderObject in borderObjects)
		{
			if ((bool)borderObject)
			{
				borderObject.SetActive(isActive);
			}
		}
	}

	private void HandleCaptureBy(FightingCapturePoint capturePoint)
	{
		if (capturePoint.OwnedByTeam == LazyConsts.Fighting.TeamType.WildZombie)
		{
			LazySingleton<FightingGameController>.Instance.FinishAsLost();
		}
	}

	public void SpawnAllies()
	{
		List<WgoData> list = new List<WgoData>();
		foreach (SGuid item in MainGame.Instance.GameSave.militaryBaseData.fighterContainersSelectedForFight)
		{
			list.Add(MainGame.WorldData.GetWgoData(item));
		}
		for (int i = 0; i < list.Count; i++)
		{
			AlliesSpawns[i].SpawnFromContainer(list[i]);
		}
	}

	public void DeSpawnAllies()
	{
		foreach (AlliesSpawn alliesSpawn in AlliesSpawns)
		{
			alliesSpawn.Clear();
		}
	}

	public void SetEnabledFlagControllers(bool isEnabled)
	{
		foreach (AlliesSpawn alliesSpawn in AlliesSpawns)
		{
			if (!(alliesSpawn.FlagController == null))
			{
				alliesSpawn.FlagController.SetEnabled(isEnabled);
			}
		}
	}

	public void ClearFlagStands()
	{
		foreach (FightinStageWorldObject stageWorldObject in stageWorldObjects)
		{
			if (stageWorldObject.type != 0)
			{
				continue;
			}
			Wgo wgoViewGlobal = GameScene.GetWgoViewGlobal(stageWorldObject.uniqueId);
			if ((bool)wgoViewGlobal)
			{
				FlagStandComponent componentInChildren = wgoViewGlobal.GetComponentInChildren<FlagStandComponent>(includeInactive: true);
				if ((bool)componentInChildren)
				{
					componentInChildren.DetachFlag();
				}
				wgoViewGlobal.Data.GameResStr.Set("flag_stand_sguid", string.Empty);
			}
		}
		foreach (FlagStandComponent flagStandComponent in LazySingleton<FightingGameController>.Instance.FlagStandComponents)
		{
			flagStandComponent.DetachFlag();
		}
	}

	public bool IsValid()
	{
		GameSave gameSave = MainGame.Instance.GameSave;
		if (string.IsNullOrEmpty(levelWorldZoneId) || gameSave.worldData.GetWorldZoneDataById(levelWorldZoneId) == null)
		{
			Debug.LogError("WorldZoneId is not set for fighting level: " + id);
			return false;
		}
		int num = gameSave.militaryBaseData.fighterContainers.Count;
		if (gameSave.militaryBaseData.IsMercenaryPayed)
		{
			num++;
		}
		if (alliesSpawns == null || alliesSpawns.Count < num)
		{
			Debug.LogError("AlliesSpawns count is not set for fighting level: " + id);
			return false;
		}
		if (string.IsNullOrEmpty(levelGdPointId) || gameSave.WorldData.gdPointsData.GetGDPointDataById(levelGdPointId) == null)
		{
			Debug.LogError("GdPointId is not set for fighting level: " + id);
			return false;
		}
		return true;
	}

	public void ApplyStageId(int stageId)
	{
		if (string.IsNullOrEmpty(base.WorldId))
		{
			Debug.LogWarning("WorldId is not set for fighting level: " + id);
			return;
		}
		currentStageId = stageId;
		GameSceneData gameSceneDataById = MainGame.WorldData.GetGameSceneDataById(base.WorldId);
		if (gameSceneDataById == null)
		{
			Debug.LogWarning("GameSceneData is not found for fighting level: " + id);
		}
		else
		{
			gameSceneDataById.ApplyStageForFightingLevel(id, stageId);
		}
	}

	public void ApplyStageIdFromData(int stageId)
	{
		foreach (SerializedStagePrefabRef stagePrefabsRef in stagePrefabsRefs)
		{
			stagePrefabsRef.ApplyByStageId(stageId, base.transform);
		}
		foreach (FightingStage nonPrefabStage in nonPrefabStages)
		{
			nonPrefabStage.ApplyStage(stageId);
		}
		foreach (FightinStageWorldObject stageWorldObject in stageWorldObjects)
		{
			bool flag = stageId != 0 && (stageWorldObject.stageMask & (1 << stageId)) != 0;
			switch (stageWorldObject.type)
			{
			case FightinStageWorldObject.ObjType.Wgo:
			{
				WgoData wgoData = MainGame.WorldData.GetWgoData(stageWorldObject.uniqueId);
				if (wgoData != null)
				{
					wgoData.IsHidden = !flag;
				}
				break;
			}
			case FightinStageWorldObject.ObjType.Wso:
			{
				WsoData wsoData = MainGame.WorldData.GetWsoData(stageWorldObject.uniqueId);
				if (wsoData != null)
				{
					wsoData.IsHidden = !flag;
				}
				break;
			}
			}
		}
		foreach (FightingStageWorldZone stageWorldZone in stageWorldZones)
		{
			bool flag2 = stageId != 0 && (stageWorldZone.stageMask & (1 << stageId)) != 0;
			WorldZoneData worldZoneDataById = MainGame.WorldData.GetWorldZoneDataById(stageWorldZone.worldZoneId);
			if (worldZoneDataById != null)
			{
				worldZoneDataById.IsActive = flag2;
			}
			WorldZone worldZoneById = MainGame.Instance.fightingLevelSystem.GetWorldZoneById(stageWorldZone.worldZoneId);
			if (worldZoneById != null)
			{
				worldZoneById.gameObject.SetActive(flag2);
			}
		}
		Debug.Log($"Apply stage id: {stageId} to fighting level: {id}");
	}

	public GameObject GetStageInstanceFromAssetReference(AssetReferenceGameObject assetReference)
	{
		foreach (SerializedStagePrefabRef stagePrefabsRef in stagePrefabsRefs)
		{
			if (stagePrefabsRef.fightingStagePrefabRef.AssetGUID == assetReference.AssetGUID)
			{
				return stagePrefabsRef.fightingStagePrefab;
			}
		}
		return null;
	}

	public void Editor_SetContextEnableState(bool isActive)
	{
		disabledBakingContext = !isActive;
	}

	public void CollectNonStageChunkableObjects()
	{
		nonStageChunkableObjects.Clear();
		ChunkableObjectComponent[] componentsInChildren = GetComponentsInChildren<ChunkableObjectComponent>(includeInactive: true);
		foreach (ChunkableObjectComponent chunkableObjectComponent in componentsInChildren)
		{
			if (!(chunkableObjectComponent == null) && !(chunkableObjectComponent.GetComponentInParent<FightingStage>(includeInactive: true) != null) && !(chunkableObjectComponent.GetComponentInParent<Wgo>(includeInactive: true) != null) && !(chunkableObjectComponent.GetComponentInParent<Wso>(includeInactive: true) != null))
			{
				nonStageChunkableObjects.Add(chunkableObjectComponent);
			}
		}
	}

	private void OnEnable()
	{
		if (!disabledBakingContext)
		{
			BakingContextRuntimeRegistration.Register(this, registeredBakedChunkCaches);
			RegisterLiveChunkableObjects();
		}
	}

	private void OnDisable()
	{
		ForceUnregisterBakingContext();
	}

	public void ForceUnregisterBakingContext()
	{
		if (!disabledBakingContext)
		{
			BakingContextRuntimeRegistration.Unregister(registeredBakedChunkCaches);
			UnregisterLiveChunkableObjects();
		}
	}

	private void RegisterLiveChunkableObjects()
	{
		UnregisterLiveChunkableObjects();
		if (nonStageChunkableObjects.Count == 0)
		{
			CollectNonStageChunkableObjects();
		}
		for (int i = 0; i < nonStageChunkableObjects.Count; i++)
		{
			ChunkableObjectComponent chunkableObjectComponent = nonStageChunkableObjects[i];
			if (!(chunkableObjectComponent == null))
			{
				chunkableObjectComponent.UpdateChunkVisibility(isVisible: false);
				registeredLiveChunkables.Add(chunkableObjectComponent);
			}
		}
		if (registeredLiveChunkables.Count != 0)
		{
			ChunkManager instance = LazySingleton<ChunkManager>.Instance;
			if (!(instance == null))
			{
				instance.RegisterChunks(registeredLiveChunkables, ChunkManagerLayerType.FightingLevelStaticObjects);
			}
		}
	}

	private void UnregisterLiveChunkableObjects()
	{
		if (registeredLiveChunkables.Count == 0)
		{
			return;
		}
		ChunkManager instance = LazySingleton<ChunkManager>.Instance;
		if (instance != null)
		{
			instance.UnregisterChunks(registeredLiveChunkables, ChunkManagerLayerType.FightingLevelStaticObjects);
		}
		for (int i = 0; i < registeredLiveChunkables.Count; i++)
		{
			ChunkableObjectComponent chunkableObjectComponent = registeredLiveChunkables[i];
			if (!(chunkableObjectComponent == null))
			{
				chunkableObjectComponent.UpdateChunkVisibility(isVisible: false);
			}
		}
		registeredLiveChunkables.Clear();
	}
}
