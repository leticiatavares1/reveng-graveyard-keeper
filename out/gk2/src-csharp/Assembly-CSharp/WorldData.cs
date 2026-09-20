using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

[Serializable]
public class WorldData
{
	public List<GameSceneData> gameSceneDataList = new List<GameSceneData>();

	public GdPointsData gdPointsData = new GdPointsData();

	public List<WispData> wispDataList = new List<WispData>();

	[SerializeField]
	private GameRes worldGameRes = new GameRes();

	private HashSet<SGuid> prebuiltWgoAfterBuildingExpressionExecuted;

	private WgoDataCache cache;

	public WgoDataCache Cache
	{
		get
		{
			TryInitCache();
			return cache;
		}
	}

	public bool HasCache => cache != null;

	public List<string> LoadedScenes => LazySingleton<GameSceneManager>.Instance.LoadedGameSceneIds;

	public static event Action<SGuid, SGuid> OnDockPointHasToBeDisabled;

	public static event Action<SGuid, SGuid> OnDockPointFreed;

	public void PrepareForGame()
	{
		for (int i = 0; i < gameSceneDataList.Count; i++)
		{
			gameSceneDataList[i].PrepareForGame(Cache);
		}
		gdPointsData.PrepareForGame(MainGame.Instance.gameSceneConfigs);
		TryInitCache();
	}

	public void InitFromSceneConfigs(List<GameSceneConfig> configs)
	{
		gameSceneDataList.Clear();
		foreach (GameSceneConfig config in configs)
		{
			if (!config.TryLoadSceneDataContent(out var sceneWgoContentDatas))
			{
				continue;
			}
			GameSceneData gameSceneData = new GameSceneData(config.name, config.sceneGlobalPosition, Cache);
			gameSceneDataList.Add(gameSceneData);
			CreateGameSceneDataFromConfig(gameSceneData, config, sceneWgoContentDatas);
			foreach (SceneWgoContentData item in sceneWgoContentDatas)
			{
				SceneWgoContentPart component = item.GetComponent<SceneWgoContentPart>();
				if (!(component == null) && !(component is FightingLevel))
				{
					config.UnloadSceneDataContentByObject(component.gameObject);
				}
			}
		}
	}

	public void UnloadContentData(GameSceneConfig config, GameSceneData sceneData, string contentName)
	{
		if (!config.TryGetLoadedContent(contentName, out var contentDataInstance))
		{
			Debug.LogError("Can't get loaded content data ref: " + contentName);
			return;
		}
		if (contentDataInstance == null || !contentDataInstance.TryGetComponent<SceneWgoContentData>(out var component))
		{
			Debug.Log("Can't get content data instance: " + contentName);
			return;
		}
		DeInitDataFromConfig(sceneData, component);
		config.TryUnloadSceneDataContent(contentName);
		Debug.Log("Unloaded content data: " + contentName);
	}

	public void UnPrepareFromGame()
	{
		foreach (GameSceneConfig gameSceneConfig in MainGame.Instance.gameSceneConfigs)
		{
			gameSceneConfig.UnloadSceneDataContents();
		}
	}

	public WgoData GetWgoDataForWisp(WispController wispController)
	{
		if (!HasCache)
		{
			return null;
		}
		WispData orCreateWispData = GetOrCreateWispData(wispController);
		return GetWgoData(orCreateWispData.linkedWgoId);
	}

	public WispData GetOrCreateWispData(WispController wispController, string gameSceneId = "")
	{
		if (string.IsNullOrEmpty(gameSceneId))
		{
			gameSceneId = MainGame.EntrySceneToLoad;
		}
		WispData wispData = wispDataList.Find((WispData d) => d.wispId == wispController.Id);
		if (wispData == null)
		{
			wispData = new WispData();
			wispData.wispId = wispController.Id;
			WgoData wgoData = new WgoData(wispController.Id, Vector3.zero, gameSceneId);
			wgoData.direction.Value = Direction.Left.ConvertToVector3();
			MainGame.Instance.GameSave.worldData.AddWgoData(wgoData);
			wispData.linkedWgoId = wgoData.UniqueId;
			wispDataList.Add(wispData);
		}
		return wispData;
	}

	public void AddGameSceneData(GameSceneData data)
	{
		gameSceneDataList.Add(data);
	}

	[NetworkMethod(typeof(WorldDataCommand), "AddWgoDataToGameScene", new object[] { })]
	public virtual void AddWgoData(WgoData data)
	{
		if (TryGetGameSceneDataById(data.WorldId, out var gameSceneData))
		{
			gameSceneData.AddWgoData(data);
		}
	}

	public void AddWgoData(WgoData data, bool recheckVisibilityOnSpawn)
	{
		if (TryGetGameSceneDataById(data.WorldId, out var gameSceneData))
		{
			gameSceneData.AddWgoData(data, recheckVisibilityOnSpawn);
		}
	}

	public bool AddWgoData(string wgoId, Vector3 position, string gameSceneId, string customTag, out WgoData wgoData, bool recheckVisibilityOnSpawn = false)
	{
		wgoData = null;
		if (TryGetGameSceneDataById(gameSceneId, out var gameSceneData))
		{
			wgoData = gameSceneData.AddWgoData(wgoId, position, customTag, recheckVisibilityOnSpawn);
		}
		return wgoData != null;
	}

	[NetworkMethod(typeof(WorldDataCommand), "RemoveWgoDataFromGameScene", new object[] { })]
	public virtual void RemoveWgoDataFromGameScene(WgoData wgoData, bool clearCraftComponent = true)
	{
		if (TryGetGameSceneDataById(wgoData.WorldId, out var gameSceneData))
		{
			string id = wgoData.id;
			gameSceneData.RemoveWgoData(wgoData, clearCraftComponent);
			GlobalEventsSystem.FireTrigger(GlobalEventsSystem.Event.Type.RemoveWgoDataFromScene, id);
		}
	}

	public void RemoveWgoDataFromGameScene(SGuid sGuid)
	{
		WgoData wgoData = GetWgoData(sGuid);
		if (wgoData != null && TryGetGameSceneDataById(wgoData.WorldId, out var gameSceneData))
		{
			string id = wgoData.id;
			gameSceneData.RemoveWgoData(wgoData);
			GlobalEventsSystem.FireTrigger(GlobalEventsSystem.Event.Type.RemoveWgoDataFromScene, id);
		}
		else
		{
			Debug.LogError($"WgoData by uniqueId [{sGuid}] wasn't found");
		}
	}

	public WgoData ReplaceWgoData(WgoData wgoData, string newWgoId)
	{
		RemoveWgoDataFromGameScene(wgoData);
		WgoData wgoData2 = new WgoData(newWgoId, wgoData.Position, wgoData.WorldId);
		wgoData2.SpawnWGOComponent.SpawnStages = wgoData.SpawnWGOComponent.SpawnStages;
		AddWgoData(wgoData2);
		return wgoData2;
	}

	public void ChangeWgoData(WgoData wgoData, string newWgoId)
	{
		RemoveWgoDataFromGameScene(wgoData, clearCraftComponent: false);
		wgoData.ChangeId(newWgoId);
		AddWgoData(wgoData);
	}

	public void MoveWgoDataToAnotherGameScene(WgoData wgoData, string gameSceneIdTo)
	{
		RemoveWgoDataFromGameScene(wgoData);
		wgoData.WorldId = gameSceneIdTo;
		AddWgoData(wgoData);
	}

	public GameSceneData GetGameSceneDataById(string id)
	{
		return gameSceneDataList.Find((GameSceneData x) => x.id == id);
	}

	public bool TryGetGameSceneDataForContent(string contentName, out GameSceneData sceneData, out GameSceneConfig config)
	{
		sceneData = null;
		config = null;
		if (string.IsNullOrEmpty(contentName))
		{
			return false;
		}
		foreach (GameSceneConfig gameSceneConfig in MainGame.Instance.gameSceneConfigs)
		{
			if (gameSceneConfig.GetContentDataRef(contentName) != null)
			{
				config = gameSceneConfig;
				sceneData = GetGameSceneDataById(gameSceneConfig.name);
				return sceneData != null;
			}
		}
		return false;
	}

	public bool TryGetGameSceneDataForContentGuid(string assetGuid, out GameSceneData sceneData, out GameSceneConfig config)
	{
		return TryGetGameSceneDataForContentGuid(assetGuid, MainGame.Instance.gameSceneConfigs, out sceneData, out config);
	}

	public bool TryGetGameSceneDataForContentGuid(string assetGuid, IList<GameSceneConfig> configs, out GameSceneData sceneData, out GameSceneConfig config)
	{
		sceneData = null;
		config = null;
		if (string.IsNullOrEmpty(assetGuid) || configs == null)
		{
			return false;
		}
		GameSceneConfig gameSceneConfig = null;
		foreach (GameSceneConfig config2 in configs)
		{
			if (!(config2 == null) && config2.GetContentDataRefByGuid(assetGuid) != null)
			{
				GameSceneData gameSceneDataById = GetGameSceneDataById(config2.name);
				if (gameSceneDataById != null)
				{
					config = config2;
					sceneData = gameSceneDataById;
					return true;
				}
				if ((object)gameSceneConfig == null)
				{
					gameSceneConfig = config2;
				}
			}
		}
		config = gameSceneConfig;
		return false;
	}

	public WgoData GetWgoData(SGuid sGuid)
	{
		if (sGuid == null)
		{
			return null;
		}
		return Cache.wgoDataByUidCache.GetValueOrDefault(sGuid.Guid);
	}

	public WgoData GetWgoData(string id)
	{
		if (string.IsNullOrEmpty(id))
		{
			return null;
		}
		if (Cache.wgoDataByIdsCache.TryGetValue(id, out var value))
		{
			return value[0];
		}
		return null;
	}

	public List<WgoData> GetWgoDataList(string id)
	{
		if (string.IsNullOrEmpty(id))
		{
			return new List<WgoData>();
		}
		if (Cache.wgoDataByIdsCache.TryGetValue(id, out var value))
		{
			return value;
		}
		return new List<WgoData>();
	}

	public WgoData GetWgoDataByCustomTag(string customTag)
	{
		if (string.IsNullOrEmpty(customTag))
		{
			return null;
		}
		if (Cache.wgoDataByCustomTagsCache.TryGetValue(customTag, out var value))
		{
			return value[0];
		}
		return null;
	}

	public List<WgoData> GetWgoDataListByCustomTag(string customTag)
	{
		if (string.IsNullOrEmpty(customTag))
		{
			return null;
		}
		if (Cache.wgoDataByCustomTagsCache.TryGetValue(customTag, out var value))
		{
			return value;
		}
		return null;
	}

	public List<WgoData> GetWgoDataListByGroup(string wgoGroup)
	{
		if (string.IsNullOrEmpty(wgoGroup))
		{
			return null;
		}
		if (Cache.wgoDataByGroup.TryGetValue(wgoGroup, out var value))
		{
			return value;
		}
		return null;
	}

	public WsoData GetWsoDataByCustomTag(string customTag)
	{
		if (string.IsNullOrEmpty(customTag))
		{
			return null;
		}
		if (Cache.wsoDataByCustomTagsCache.TryGetValue(customTag, out var value))
		{
			return value[0];
		}
		return null;
	}

	public List<WsoData> GetWsoDataListByCustomTag(string customTag)
	{
		if (string.IsNullOrEmpty(customTag))
		{
			return null;
		}
		if (Cache.wsoDataByCustomTagsCache.TryGetValue(customTag, out var value))
		{
			return value;
		}
		return null;
	}

	public bool TryGetWgoData(string wgoId, out WgoData foundWgoData, out GameSceneData foundGameScene)
	{
		foundWgoData = null;
		foundGameScene = null;
		if (Cache.wgoDataByIdsCache.TryGetValue(wgoId, out var value))
		{
			foundWgoData = value[0];
			foundGameScene = GetGameSceneDataById(value[0].WorldId);
			return true;
		}
		return false;
	}

	public WorldZoneData GetWorldZoneDataById(string id)
	{
		foreach (GameSceneData gameSceneData in gameSceneDataList)
		{
			WorldZoneData worldZoneDataById = gameSceneData.GetWorldZoneDataById(id);
			if (worldZoneDataById != null)
			{
				return worldZoneDataById;
			}
		}
		return null;
	}

	public void NotifyDockPointHasToBeDisabled(SGuid parent, SGuid whoWasStayed)
	{
		Debug.Log(string.Format("{0}: [{1}], [{2}]", "NotifyDockPointHasToBeDisabled", parent, whoWasStayed));
		WorldData.OnDockPointHasToBeDisabled?.Invoke(parent, whoWasStayed);
	}

	public void NotifyDockPointFreed(SGuid parent, SGuid whoFreesIt)
	{
		WorldData.OnDockPointFreed?.Invoke(parent, whoFreesIt);
	}

	public void AddWsoData(WsoData data)
	{
		if (TryGetGameSceneDataById(data.WorldId, out var gameSceneData))
		{
			gameSceneData.AddWsoData(data);
		}
	}

	public bool AddWsoData(string defId, Vector3 position, string gameSceneId, out WsoData wsoData)
	{
		wsoData = null;
		if (TryGetGameSceneDataById(gameSceneId, out var gameSceneData))
		{
			wsoData = gameSceneData.AddWsoData(defId, position);
		}
		return wsoData != null;
	}

	public void RemoveWsoDataFromGameScene(WsoData wsoData)
	{
		if (TryGetGameSceneDataById(wsoData.WorldId, out var gameSceneData))
		{
			gameSceneData.RemoveWsoData(wsoData);
		}
	}

	public void RemoveWsoDataFromGameScene(SGuid sGuid)
	{
		WsoData wsoData = GetWsoData(sGuid);
		if (wsoData != null && TryGetGameSceneDataById(wsoData.WorldId, out var gameSceneData))
		{
			gameSceneData.RemoveWsoData(wsoData);
		}
		else
		{
			Debug.LogError($"WsoData by uniqueId [{sGuid}] wasn't found");
		}
	}

	public WsoData GetWsoData(SGuid sGuid)
	{
		if (sGuid == null)
		{
			return null;
		}
		return Cache.wsoDataByUidCache.GetValueOrDefault(sGuid.Guid);
	}

	public List<WsoData> GetWsoDataByDefId(string defId)
	{
		if (string.IsNullOrEmpty(defId))
		{
			return new List<WsoData>();
		}
		if (Cache.wsoDataByIdCache.TryGetValue(defId, out var value))
		{
			return new List<WsoData>(value);
		}
		return new List<WsoData>();
	}

	public List<WsoData> GetWsoDataForScene(string worldId)
	{
		return GetGameSceneDataById(worldId)?.wsoDataList ?? new List<WsoData>();
	}

	public void TryInitCache()
	{
		if (HasCache)
		{
			return;
		}
		cache = new WgoDataCache();
		foreach (GameSceneData gameSceneData in gameSceneDataList)
		{
			foreach (WgoData wgoData in gameSceneData.wgoDataList)
			{
				cache.AddWgoDataToCache(wgoData);
			}
			foreach (WsoData wsoData in gameSceneData.wsoDataList)
			{
				cache.AddWsoDataToCache(wsoData);
			}
		}
	}

	public void AddContentDataFromConfig(GameSceneData sceneData, GameSceneConfig gameSceneConfig, SceneWgoContentData sceneWgoContentData)
	{
		AddDataToSceneFromContentData(sceneData, sceneWgoContentData, gameSceneConfig.sceneGlobalPosition);
	}

	public bool TryExecutePrebuiltWgoAfterBuildingExpressions(WgoData wgoData, BuildingDef buildingDef)
	{
		if (wgoData == null || buildingDef == null || buildingDef.expressionAfterBuilding.Count == 0)
		{
			return false;
		}
		if (prebuiltWgoAfterBuildingExpressionExecuted == null)
		{
			prebuiltWgoAfterBuildingExpressionExecuted = new HashSet<SGuid>();
		}
		if (!prebuiltWgoAfterBuildingExpressionExecuted.Add(wgoData.UniqueId))
		{
			return false;
		}
		foreach (LazyExpression item in buildingDef.expressionAfterBuilding)
		{
			item.EvaluateBool(wgoData);
		}
		return true;
	}

	public bool TryExecutePrebuiltWgoAfterBuildingExpressions(WgoData wgoData, WorldZonePrebuiltWgoParams prebuiltWgoParam)
	{
		if (wgoData == null || prebuiltWgoParam == null || !prebuiltWgoParam.execExpressionAfterBuilding)
		{
			return false;
		}
		if (wgoData.Definition == null || !wgoData.Definition.TryGetBuildingDefForWgo(out var buildingDef))
		{
			return false;
		}
		return TryExecutePrebuiltWgoAfterBuildingExpressions(wgoData, buildingDef);
	}

	private void CreateGameSceneDataFromConfig(GameSceneData sceneData, GameSceneConfig gameSceneConfig, List<SceneWgoContentData> sceneWgoContentDatas)
	{
		foreach (SceneWgoContentData sceneWgoContentData in sceneWgoContentDatas)
		{
			AddDataToSceneFromContentData(sceneData, sceneWgoContentData, gameSceneConfig.sceneGlobalPosition);
			if (sceneWgoContentData.GetComponent<SceneWgoContentPart>() is FightingLevel fightingLevel)
			{
				Debug.Log("Adding fighting level data: " + fightingLevel.id);
				FightingLevelData item = new FightingLevelData(fightingLevel.id);
				sceneData.fightingLevels.Add(item);
			}
			Debug.Log("Loaded SceneWgoContent: " + sceneWgoContentData.name);
		}
	}

	private void AddDataToSceneFromContentData(GameSceneData sceneData, SceneWgoContentData sceneWgoContentData, Vector3 offset)
	{
		SceneWgoContentPart component = sceneWgoContentData.GetComponent<SceneWgoContentPart>();
		foreach (WgoData wgo in component.Wgos)
		{
			if (string.IsNullOrEmpty(wgo.id))
			{
				continue;
			}
			WgoData wgoData = wgo.CreateDataFromMe(offset, sceneData.id, copySGuid: true);
			if (wgo.startReses != null)
			{
				foreach (StartReses.StartItemData startItem in wgo.startReses.startItems)
				{
					Item item = new Item(startItem.id, startItem.count);
					wgoData.Inventory.AddItemToInventory(item);
				}
				wgoData.SetGameRes(wgo.startReses.startGameRes);
				wgoData.GameResStr.Set(wgo.startReses.startGameResStr);
			}
			sceneData.AddWgoData(wgoData);
		}
		foreach (WsoData wso in component.Wsos)
		{
			if (wso != null)
			{
				WsoData wsoData = wso.CreateDataFromMe(offset, sceneData.id, copySGuid: true);
				sceneData.AddWsoData(wsoData);
			}
		}
		IReadOnlyList<WorldZoneBakedData> worldZones = component.WorldZones;
		List<WorldZoneBakedData> list = new List<WorldZoneBakedData>(worldZones.Count);
		for (int i = 0; i < worldZones.Count; i++)
		{
			WorldZoneBakedData worldZoneBakedData = worldZones[i];
			if (worldZoneBakedData != null)
			{
				list.Add(worldZoneBakedData);
			}
		}
		list.Sort((WorldZoneBakedData a, WorldZoneBakedData b) => a.processingPriority.CompareTo(b.processingPriority));
		foreach (WorldZoneBakedData item2 in list)
		{
			WorldZoneData worldZoneData = WorldZoneData.CreateFromBaked(item2, sceneData.id, sceneData.offset);
			if (worldZoneData != null)
			{
				sceneData.AddWorldZoneData(worldZoneData, item2);
			}
		}
	}

	private void DeInitDataFromConfig(GameSceneData sceneData, SceneWgoContentData sceneWgoContentData)
	{
		SceneWgoContentPart component = sceneWgoContentData.GetComponent<SceneWgoContentPart>();
		IReadOnlyList<SGuid> wgoUniqueIds = component.WgoUniqueIds;
		Debug.Log($"Removing {wgoUniqueIds.Count} WgoData from scene: {sceneData.id}");
		foreach (SGuid item in wgoUniqueIds)
		{
			WgoData wgoData = GetWgoData(item);
			if (wgoData != null)
			{
				sceneData.RemoveWgoData(wgoData);
			}
		}
		IReadOnlyList<SGuid> wsoUniqueIds = component.WsoUniqueIds;
		Debug.Log($"Removing {wsoUniqueIds.Count} WsoData from scene: {sceneData.id}");
		foreach (SGuid item2 in wsoUniqueIds)
		{
			WsoData wsoData = GetWsoData(item2);
			if (wsoData != null)
			{
				sceneData.RemoveWsoData(wsoData);
			}
		}
		IReadOnlyList<WorldZoneBakedData> worldZones = component.WorldZones;
		Debug.Log($"Removing {worldZones.Count} WorldZoneData from scene: {sceneData.id}");
		foreach (WorldZoneBakedData item3 in worldZones)
		{
			if (!(item3 == null))
			{
				WorldZoneData worldZoneDataById = sceneData.GetWorldZoneDataById(item3.id);
				if (worldZoneDataById != null)
				{
					sceneData.RemoveWorldZoneData(worldZoneDataById);
				}
			}
		}
	}

	private bool TryGetGameSceneDataById(string id, out GameSceneData gameSceneData)
	{
		gameSceneData = gameSceneDataList.Find((GameSceneData x) => x.id == id);
		if (gameSceneData == null)
		{
			Debug.LogError("[WorldData]: incorrect game scene id was provided [" + id + "]");
		}
		return gameSceneData != null;
	}

	public float GetGameRes(string id)
	{
		return worldGameRes.Get(id);
	}

	public int GetGameResInt(string id)
	{
		return worldGameRes.GetInt(id);
	}

	public void SetGameRes(string id, int value)
	{
		worldGameRes.Set(id, value);
	}

	public void SetGameRes(GameRes gameRes)
	{
		worldGameRes.Set(gameRes);
	}

	public void SetGameRes(string id, float value)
	{
		worldGameRes.Set(id, value);
	}

	public bool IsGameResEmpty()
	{
		return worldGameRes.IsEmpty();
	}

	public void AddGameRes(GameRes gameRes)
	{
		worldGameRes.Add(gameRes);
	}

	public void SubGameRes(string id, int value)
	{
		worldGameRes.Sub(id, value);
	}

	public void SubGameRes(string id, float value)
	{
		worldGameRes.Sub(id, value);
	}

	public void AddGameRes(string id, int value)
	{
		worldGameRes.Add(id, value);
	}

	public void AddGameRes(string id, float value)
	{
		worldGameRes.Add(id, value);
	}

	public void MultiplyGameRes(string id, float value)
	{
		worldGameRes.Multiply(id, value);
	}
}
