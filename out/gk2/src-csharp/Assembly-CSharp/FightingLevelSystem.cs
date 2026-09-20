using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class FightingLevelSystem
{
	private sealed class SceneFightingLevelBindings
	{
		public readonly GameSceneData SceneData;

		public Action<FightingLevelData> OnAdded;

		public Action<FightingLevelData> OnRemoved;

		public SceneFightingLevelBindings(GameSceneData sceneData)
		{
			SceneData = sceneData;
		}
	}

	private readonly Dictionary<string, FightingLevel> fightingLevelsById = new Dictionary<string, FightingLevel>();

	private readonly List<WorldZone> spawnedWorldZones = new List<WorldZone>();

	private readonly List<SceneFightingLevelBindings> sceneBindings = new List<SceneFightingLevelBindings>();

	private bool isSubscribedToStageChanges;

	public FightingLevel GetFightingLevel(string id)
	{
		if (string.IsNullOrEmpty(id))
		{
			return null;
		}
		return fightingLevelsById.GetValueOrDefault(id);
	}

	public WorldZone GetWorldZoneById(string zoneId)
	{
		if (string.IsNullOrEmpty(zoneId))
		{
			return null;
		}
		for (int i = 0; i < spawnedWorldZones.Count; i++)
		{
			WorldZone worldZone = spawnedWorldZones[i];
			if (worldZone != null && worldZone.Id == zoneId)
			{
				return worldZone;
			}
		}
		string text = MainGame.PlayerController?.CurrentGameScene?.Id;
		if (!string.IsNullOrEmpty(text))
		{
			WorldZone worldZoneById = MainGame.PlayerController.CurrentGameScene.GetWorldZoneById(zoneId);
			if (worldZoneById != null)
			{
				return worldZoneById;
			}
		}
		GameSceneManager instance = LazySingleton<GameSceneManager>.Instance;
		if (instance == null)
		{
			return null;
		}
		foreach (GameScene loadedGameScene in instance.LoadedGameScenes)
		{
			if (!(loadedGameScene == null) && !(loadedGameScene.Id == text))
			{
				WorldZone worldZoneById2 = loadedGameScene.GetWorldZoneById(zoneId);
				if (worldZoneById2 != null)
				{
					return worldZoneById2;
				}
			}
		}
		return null;
	}

	public void PrepareForGame()
	{
		UnprepareFromGame();
		FightingGameController.RepairChainedFightsLeftAtWinAfterFollowUpReset();
		FightingLevelData.OnFightingLevelDataChanged += HandleFightingLevelDataChanged;
		isSubscribedToStageChanges = true;
		foreach (GameSceneData gameSceneData in MainGame.WorldData.gameSceneDataList)
		{
			SubscribeSceneData(gameSceneData);
			SpawnFightingLevelsFromData(gameSceneData);
		}
	}

	public void UnprepareFromGame()
	{
		if (isSubscribedToStageChanges)
		{
			FightingLevelData.OnFightingLevelDataChanged -= HandleFightingLevelDataChanged;
			isSubscribedToStageChanges = false;
		}
		for (int num = sceneBindings.Count - 1; num >= 0; num--)
		{
			UnsubscribeSceneData(sceneBindings[num]);
		}
		sceneBindings.Clear();
		fightingLevelsById.Clear();
		spawnedWorldZones.Clear();
	}

	public bool TryGetExistingWorldZone(string zoneId, out WorldZone worldZone)
	{
		worldZone = GetWorldZoneById(zoneId);
		return worldZone != null;
	}

	public bool TryEnsureFightingLevelLoaded(string levelId)
	{
		if (string.IsNullOrEmpty(levelId))
		{
			return false;
		}
		if (GetFightingLevel(levelId) != null)
		{
			return true;
		}
		if (!TryResolveOwnerSceneForLevel(levelId, out var sceneData))
		{
			Debug.LogError("Can't find game scene data for fighting level: " + levelId);
			return false;
		}
		FightingLevelData fightingLevelData = sceneData.fightingLevels.Find((FightingLevelData level) => level.id == levelId);
		if (fightingLevelData != null)
		{
			LoadAndSpawnFightingLevel(sceneData, fightingLevelData, addToGameSceneData: false);
		}
		else
		{
			sceneData.AddFightingLevelData(levelId);
		}
		return GetFightingLevel(levelId) != null;
	}

	public bool TryRemoveFightingLevelData(string levelId)
	{
		if (string.IsNullOrEmpty(levelId))
		{
			return false;
		}
		if (!TryResolveOwnerSceneForLevel(levelId, out var sceneData))
		{
			Debug.LogError("Can't find game scene data for fighting level: " + levelId);
			return false;
		}
		sceneData.RemoveFightingLevelData(levelId);
		return true;
	}

	private static bool TryResolveOwnerSceneForLevel(string levelId, out GameSceneData sceneData)
	{
		if (MainGame.WorldData.TryGetGameSceneDataForContent(levelId, out sceneData, out var _))
		{
			return true;
		}
		sceneData = null;
		return false;
	}

	private void SubscribeSceneData(GameSceneData sceneData)
	{
		if (sceneData == null)
		{
			return;
		}
		foreach (SceneFightingLevelBindings sceneBinding in sceneBindings)
		{
			if (sceneBinding.SceneData == sceneData)
			{
				return;
			}
		}
		SceneFightingLevelBindings sceneFightingLevelBindings = new SceneFightingLevelBindings(sceneData);
		sceneFightingLevelBindings.OnAdded = delegate(FightingLevelData data)
		{
			LoadAndSpawnFightingLevel(sceneData, data, addToGameSceneData: true);
		};
		sceneFightingLevelBindings.OnRemoved = delegate(FightingLevelData data)
		{
			UnloadAndDespawnFightingLevel(sceneData, data);
		};
		sceneData.OnFightingLevelDataAdded += sceneFightingLevelBindings.OnAdded;
		sceneData.OnFightingLevelDataRemoved += sceneFightingLevelBindings.OnRemoved;
		sceneBindings.Add(sceneFightingLevelBindings);
	}

	private static void UnsubscribeSceneData(SceneFightingLevelBindings bindings)
	{
		if (bindings?.SceneData != null)
		{
			bindings.SceneData.OnFightingLevelDataAdded -= bindings.OnAdded;
			bindings.SceneData.OnFightingLevelDataRemoved -= bindings.OnRemoved;
		}
	}

	private void SpawnFightingLevelsFromData(GameSceneData sceneData)
	{
		GameSceneConfig gameSceneConfig = ResolveGameSceneConfig(sceneData.id);
		if (gameSceneConfig == null)
		{
			Debug.LogError("Can't find game scene config with id: " + sceneData.id);
			return;
		}
		foreach (FightingLevelData fightingLevel in sceneData.fightingLevels)
		{
			LoadAndSpawnFightingLevel(sceneData, gameSceneConfig, fightingLevel, addToGameSceneData: false);
		}
	}

	private void LoadAndSpawnFightingLevel(GameSceneData sceneData, FightingLevelData fightingLevelData, bool addToGameSceneData)
	{
		GameSceneConfig gameSceneConfig;
		if (MainGame.WorldData.TryGetGameSceneDataForContent(fightingLevelData.id, out var sceneData2, out var config))
		{
			sceneData = sceneData2;
			gameSceneConfig = config;
		}
		else
		{
			gameSceneConfig = ResolveGameSceneConfig(sceneData.id);
		}
		if (gameSceneConfig == null)
		{
			Debug.LogError("Can't find game scene config for fighting level: " + fightingLevelData.id);
		}
		else
		{
			LoadAndSpawnFightingLevel(sceneData, gameSceneConfig, fightingLevelData, addToGameSceneData);
		}
	}

	private void LoadAndSpawnFightingLevel(GameSceneData sceneData, GameSceneConfig config, FightingLevelData fightingLevelData, bool addToGameSceneData)
	{
		if (!config.TryLoadSceneDataContentByName(fightingLevelData.id, out var sceneWgoContentData))
		{
			Debug.LogError("Can't load fighting level prefab: " + fightingLevelData.id + ", sceneWgoContentData is null");
			return;
		}
		if (addToGameSceneData)
		{
			MainGame.WorldData.AddContentDataFromConfig(sceneData, config, sceneWgoContentData);
		}
		FightingLevel component = sceneWgoContentData.GetComponent<FightingLevel>();
		if (component == null)
		{
			Debug.LogError("Can't load fighting level prefab: " + fightingLevelData.id + ", fighting level is null");
			return;
		}
		SpawnMissingWorldZonesForLoadedContent(sceneData, config, sceneWgoContentData);
		component.ApplyStageIdFromData(fightingLevelData.CurStageId);
		if (!fightingLevelsById.ContainsKey(component.id))
		{
			fightingLevelsById.Add(component.id, component);
		}
	}

	private void UnloadAndDespawnFightingLevel(GameSceneData sceneData, FightingLevelData fightingLevelData)
	{
		if (fightingLevelsById.TryGetValue(fightingLevelData.id, out var value))
		{
			DespawnWorldZonesForUnloadedContent(value);
			fightingLevelsById.Remove(fightingLevelData.id);
		}
		else
		{
			Debug.LogWarning("Can't unload fighting level view, it wasn't found: " + fightingLevelData.id);
		}
		GameSceneConfig gameSceneConfig;
		if (MainGame.WorldData.TryGetGameSceneDataForContent(fightingLevelData.id, out var sceneData2, out var config))
		{
			sceneData = sceneData2;
			gameSceneConfig = config;
		}
		else
		{
			gameSceneConfig = ResolveGameSceneConfig(sceneData.id);
		}
		if (gameSceneConfig == null)
		{
			Debug.LogError("Can't find game scene config for fighting level: " + fightingLevelData.id);
		}
		else
		{
			MainGame.WorldData.UnloadContentData(gameSceneConfig, sceneData, fightingLevelData.id);
		}
	}

	private void HandleFightingLevelDataChanged(FightingLevelData fightingLevelData)
	{
		FightingLevel fightingLevel = GetFightingLevel(fightingLevelData.id);
		if (!(fightingLevel == null))
		{
			fightingLevel.ApplyStageIdFromData(fightingLevelData.CurStageId);
		}
	}

	private void SpawnMissingWorldZonesForLoadedContent(GameSceneData sceneData, GameSceneConfig config, SceneWgoContentData sceneWgoContentData)
	{
		SceneWgoContentPart component = sceneWgoContentData.GetComponent<SceneWgoContentPart>();
		if (component == null)
		{
			return;
		}
		IReadOnlyList<WorldZoneBakedData> worldZones = component.WorldZones;
		for (int i = 0; i < worldZones.Count; i++)
		{
			WorldZoneBakedData worldZoneBakedData = worldZones[i];
			if (worldZoneBakedData == null || TryGetExistingWorldZone(worldZoneBakedData.id, out var _))
			{
				continue;
			}
			WorldZoneData worldZoneDataById = sceneData.GetWorldZoneDataById(worldZoneBakedData.id);
			if (worldZoneDataById != null)
			{
				WorldZone worldZone2 = SpawnWorldZoneFromData(config, worldZoneDataById, component);
				if (!(worldZone2 == null))
				{
					spawnedWorldZones.Add(worldZone2);
					worldZone2.AddWgosOnGameSceneStart();
				}
			}
		}
	}

	private void DespawnWorldZonesForUnloadedContent(SceneWgoContentPart contentPart)
	{
		if (contentPart == null)
		{
			return;
		}
		IReadOnlyList<WorldZoneBakedData> worldZones = contentPart.WorldZones;
		for (int num = spawnedWorldZones.Count - 1; num >= 0; num--)
		{
			WorldZone worldZone = spawnedWorldZones[num];
			if (worldZone == null)
			{
				spawnedWorldZones.RemoveAt(num);
			}
			else if (ContainsBakedZoneId(worldZones, worldZone.Id))
			{
				spawnedWorldZones.RemoveAt(num);
				UnityEngine.Object.Destroy(worldZone.gameObject);
			}
		}
	}

	private static WorldZone SpawnWorldZoneFromData(GameSceneConfig config, WorldZoneData worldZoneData, SceneWgoContentPart preferredContentPart)
	{
		Transform parent = ResolveWorldZoneParent(config, worldZoneData, preferredContentPart);
		return WorldZone.Spawn(worldZoneData, parent);
	}

	private static Transform ResolveWorldZoneParent(GameSceneConfig config, WorldZoneData worldZoneData, SceneWgoContentPart preferredContentPart)
	{
		if (preferredContentPart != null)
		{
			return preferredContentPart.transform;
		}
		if (config.TryGetLoadedContent(worldZoneData.contentPartName, out var contentDataInstance) && contentDataInstance != null && contentDataInstance.TryGetComponent<SceneWgoContentPart>(out var component))
		{
			return component.transform;
		}
		GameSceneManager instance = LazySingleton<GameSceneManager>.Instance;
		if (instance != null)
		{
			GameScene gameScene = instance.LoadedGameScenes.Find((GameScene scene) => scene != null && scene.Id == worldZoneData.gameSceneId);
			if (gameScene != null)
			{
				return gameScene.transform;
			}
		}
		return null;
	}

	private static bool ContainsBakedZoneId(IReadOnlyList<WorldZoneBakedData> bakedZones, string zoneId)
	{
		for (int i = 0; i < bakedZones.Count; i++)
		{
			WorldZoneBakedData worldZoneBakedData = bakedZones[i];
			if (worldZoneBakedData != null && worldZoneBakedData.id == zoneId)
			{
				return true;
			}
		}
		return false;
	}

	private static GameSceneConfig ResolveGameSceneConfig(string sceneId)
	{
		return MainGame.Instance.gameSceneConfigs.Find((GameSceneConfig config) => config.name == sceneId);
	}
}
