using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class SaveFixContext
{
	private readonly struct WgoIndexEntry
	{
		public readonly GameSceneData sceneData;

		public readonly WgoData wgoData;

		public WgoIndexEntry(GameSceneData sceneData, WgoData wgoData)
		{
			this.sceneData = sceneData;
			this.wgoData = wgoData;
		}
	}

	private readonly struct LoadedContent
	{
		public readonly SceneWgoContentPart part;

		public readonly GameSceneData sceneData;

		public readonly GameSceneConfig config;

		public LoadedContent(SceneWgoContentPart part, GameSceneData sceneData, GameSceneConfig config)
		{
			this.part = part;
			this.sceneData = sceneData;
			this.config = config;
		}
	}

	private readonly GameSave gameSave;

	private readonly IList<GameSceneConfig> gameSceneConfigs;

	private readonly Dictionary<Guid, WgoIndexEntry> wgoByUid = new Dictionary<Guid, WgoIndexEntry>();

	private readonly Dictionary<string, LoadedContent> contentByGuid = new Dictionary<string, LoadedContent>();

	private readonly List<(GameSceneConfig config, AssetReference contentRef)> contentLoadedByFixer = new List<(GameSceneConfig, AssetReference)>();

	public GameSave GameSave => gameSave;

	public SaveFixContext(GameSave gameSave, IList<GameSceneConfig> gameSceneConfigs)
	{
		this.gameSave = gameSave;
		this.gameSceneConfigs = gameSceneConfigs ?? Array.Empty<GameSceneConfig>();
		RebuildWgoIndex();
	}

	public void Log(string message)
	{
		Debug.Log("[SaveFixer] " + message);
	}

	public void LogWarning(string message)
	{
		Debug.LogWarning("[SaveFixer] " + message);
	}

	public void LogError(string message)
	{
		Debug.LogError("[SaveFixer] " + message);
	}

	public bool TryGetWgo(SGuid uniqueId, out WgoData wgoData, out GameSceneData sceneData)
	{
		wgoData = null;
		sceneData = null;
		if (SGuid.IsNullOrEmpty(uniqueId))
		{
			return false;
		}
		if (!wgoByUid.TryGetValue(uniqueId.Guid, out var value))
		{
			return false;
		}
		wgoData = value.wgoData;
		sceneData = value.sceneData;
		return true;
	}

	public bool HasWgo(SGuid uniqueId)
	{
		if (!SGuid.IsNullOrEmpty(uniqueId))
		{
			return wgoByUid.ContainsKey(uniqueId.Guid);
		}
		return false;
	}

	public bool TryFindWgoByIdNear(string wgoId, Vector3 origin, float radius, out WgoData wgoData, out GameSceneData sceneData)
	{
		return TryFindWgoNear(origin, radius, out wgoData, out sceneData, (WgoData candidate) => candidate.id == wgoId);
	}

	public bool TryFindWgoByGroupNear(string wgoGroup, Vector3 origin, float radius, out WgoData wgoData, out GameSceneData sceneData)
	{
		wgoData = null;
		sceneData = null;
		if (string.IsNullOrEmpty(wgoGroup) || GameBalance.Me == null)
		{
			return false;
		}
		return TryFindWgoNear(origin, radius, out wgoData, out sceneData, (WgoData candidate) => GameBalance.Me.HasWgoIdByGroup(wgoGroup, candidate.id));
	}

	private bool TryFindWgoNear(Vector3 origin, float radius, out WgoData wgoData, out GameSceneData sceneData, Func<WgoData, bool> match)
	{
		wgoData = null;
		sceneData = null;
		if (match == null || radius < 0f)
		{
			return false;
		}
		List<GameSceneData> list = gameSave.worldData?.gameSceneDataList;
		if (list == null)
		{
			return false;
		}
		float num = radius * radius;
		float num2 = float.MaxValue;
		for (int i = 0; i < list.Count; i++)
		{
			GameSceneData gameSceneData = list[i];
			if (gameSceneData?.wgoDataList == null)
			{
				continue;
			}
			for (int j = 0; j < gameSceneData.wgoDataList.Count; j++)
			{
				WgoData wgoData2 = gameSceneData.wgoDataList[j];
				if (wgoData2 != null && !string.IsNullOrEmpty(wgoData2.id) && match(wgoData2))
				{
					float num3 = DistanceXzSq(wgoData2.Position, origin);
					if (!(num3 > num) && !(num3 >= num2))
					{
						num2 = num3;
						wgoData = wgoData2;
						sceneData = gameSceneData;
					}
				}
			}
		}
		return wgoData != null;
	}

	private static float DistanceXzSq(Vector3 a, Vector3 b)
	{
		float num = a.x - b.x;
		float num2 = a.z - b.z;
		return num * num + num2 * num2;
	}

	public void AddWgoData(GameSceneData sceneData, WgoData wgoData)
	{
		if (sceneData != null && wgoData != null)
		{
			if (!sceneData.HasCache)
			{
				LogError("AddWgoData: scene [" + sceneData.id + "] has no cache, run after PrepareForGame");
				return;
			}
			wgoData.WorldId = sceneData.id;
			sceneData.AddWgoData(wgoData);
			wgoByUid[wgoData.UniqueId.Guid] = new WgoIndexEntry(sceneData, wgoData);
		}
	}

	public bool RemoveWgoData(SGuid uniqueId)
	{
		if (!TryGetWgo(uniqueId, out var wgoData, out var sceneData))
		{
			return false;
		}
		if (!sceneData.HasCache)
		{
			LogError("RemoveWgoData: scene [" + sceneData.id + "] has no cache, run after PrepareForGame");
			return false;
		}
		RemoveConveyorSystemLinks(wgoData);
		PrepareDataLayerForRemove(sceneData, wgoData);
		sceneData.RemoveWgoData(wgoData);
		wgoByUid.Remove(uniqueId.Guid);
		return true;
	}

	public bool MoveWgo(SGuid uniqueId, Vector3 newPosition)
	{
		if (!TryGetWgo(uniqueId, out var wgoData, out var sceneData))
		{
			return false;
		}
		if (!sceneData.HasCache)
		{
			LogError("MoveWgo: scene [" + sceneData.id + "] has no cache, run after PrepareForGame");
			return false;
		}
		RemoveFromWorldZonesLive(sceneData, wgoData);
		wgoData.Position = newPosition;
		AssignWorldZoneLive(sceneData, wgoData);
		return true;
	}

	public bool TryGetContentPart(string assetGuid, out SceneWgoContentPart part, out GameSceneData sceneData, out GameSceneConfig config)
	{
		part = null;
		sceneData = null;
		config = null;
		if (string.IsNullOrEmpty(assetGuid))
		{
			return false;
		}
		if (contentByGuid.TryGetValue(assetGuid, out var value))
		{
			part = value.part;
			sceneData = value.sceneData;
			config = value.config;
			if (part != null && sceneData != null)
			{
				return config != null;
			}
			return false;
		}
		if (!gameSave.worldData.TryGetGameSceneDataForContentGuid(assetGuid, gameSceneConfigs, out sceneData, out config))
		{
			if (config != null)
			{
				LogError("GameSceneConfig [" + config.name + "] contains content guid [" + assetGuid + "] but scene is not in the save");
			}
			else
			{
				LogError("No GameSceneConfig contains content guid [" + assetGuid + "]");
			}
			return false;
		}
		AssetReference contentDataRefByGuid = config.GetContentDataRefByGuid(assetGuid);
		if (contentDataRefByGuid == null)
		{
			LogError("GameSceneConfig [" + config.name + "] has no AssetReference for guid [" + assetGuid + "]");
			return false;
		}
		bool flag = config.IsSceneContentDataLoaded(contentDataRefByGuid);
		if (!config.TryLoadSceneDataContentByRef(contentDataRefByGuid, out var sceneWgoContentData) || sceneWgoContentData == null)
		{
			LogError("Failed to load content for guid [" + assetGuid + "]");
			return false;
		}
		part = sceneWgoContentData.GetComponent<SceneWgoContentPart>();
		if (part == null)
		{
			LogError("Loaded content [" + sceneWgoContentData.name + "] has no SceneWgoContentPart");
			return false;
		}
		contentByGuid[assetGuid] = new LoadedContent(part, sceneData, config);
		if (!flag)
		{
			contentLoadedByFixer.Add((config, contentDataRefByGuid));
		}
		return true;
	}

	public void UnloadLoadedContent()
	{
		for (int i = 0; i < contentLoadedByFixer.Count; i++)
		{
			var (gameSceneConfig, assetReference) = contentLoadedByFixer[i];
			if (!(gameSceneConfig == null) && assetReference != null)
			{
				gameSceneConfig.TryUnloadSceneDataContent(assetReference);
			}
		}
		contentLoadedByFixer.Clear();
		contentByGuid.Clear();
	}

	public void WarnAboutDanglingReferences(SGuid uniqueId, string wgoId)
	{
		if (!SGuid.IsNullOrEmpty(uniqueId))
		{
			WarnIfDelayedSpawnReferences(uniqueId, wgoId);
			WarnIfNpcLifeSimulatorReferences(uniqueId, wgoId);
			WarnIfZombieSystemReferences(uniqueId, wgoId);
			WarnIfConveyorSystemReferences(uniqueId, wgoId);
		}
	}

	private void RebuildWgoIndex()
	{
		wgoByUid.Clear();
		List<GameSceneData> gameSceneDataList = gameSave.worldData.gameSceneDataList;
		if (gameSceneDataList == null)
		{
			return;
		}
		foreach (GameSceneData item in gameSceneDataList)
		{
			if (item?.wgoDataList == null)
			{
				continue;
			}
			foreach (WgoData wgoData in item.wgoDataList)
			{
				if (!(wgoData?.UniqueId == null) && !wgoByUid.TryAdd(wgoData.UniqueId.Guid, new WgoIndexEntry(item, wgoData)))
				{
					LogWarning($"Duplicate uniqueId [{wgoData.UniqueId}] id [{wgoData.id}] scene [{item.id}]");
				}
			}
		}
	}

	private static void AssignWorldZoneLive(GameSceneData sceneData, WgoData wgoData)
	{
		if (sceneData?.worldZones == null || wgoData == null)
		{
			return;
		}
		List<WorldZoneData> zonesByDescendingPriority = GetZonesByDescendingPriority(sceneData);
		for (int i = 0; i < zonesByDescendingPriority.Count; i++)
		{
			WorldZoneData worldZoneData = zonesByDescendingPriority[i];
			if (worldZoneData != null && worldZoneData.TryAddWgoData(wgoData))
			{
				break;
			}
		}
	}

	private void PrepareDataLayerForRemove(GameSceneData sceneData, WgoData wgoData)
	{
		RemoveOrdersTargetingWgo(sceneData, wgoData);
		RemoveFromAllNpcLifeSimGroups(wgoData);
		UnlinkRemainingWorkbenchExtensions(wgoData);
	}

	private static void RemoveOrdersTargetingWgo(GameSceneData sceneData, WgoData wgoData)
	{
		if (sceneData?.worldZones == null || wgoData == null)
		{
			return;
		}
		foreach (WorldZoneData worldZone in sceneData.worldZones)
		{
			worldZone?.RemoveOrdersByTarget(wgoData.UniqueId);
		}
	}

	private void RemoveFromAllNpcLifeSimGroups(WgoData wgoData)
	{
		NPCLifeSimulatorData npcLifeSimulatorData = gameSave.npcLifeSimulatorData;
		if (npcLifeSimulatorData?.AllGroups == null || wgoData == null)
		{
			return;
		}
		foreach (NPCGroupPointOfInterestData allGroup in npcLifeSimulatorData.AllGroups)
		{
			allGroup?.RemoveWgoFromGroup(wgoData);
		}
	}

	private void UnlinkRemainingWorkbenchExtensions(WgoData wgoData)
	{
		if (wgoData?.AttachedWorkbenchExtensions == null)
		{
			return;
		}
		SGuid uniqueId = wgoData.UniqueId;
		IReadOnlyList<SGuid> attachedWorkbenchExtensions = wgoData.AttachedWorkbenchExtensions;
		for (int i = 0; i < attachedWorkbenchExtensions.Count; i++)
		{
			if (TryGetWgo(attachedWorkbenchExtensions[i], out var wgoData2, out var _))
			{
				wgoData2.RemoveWorkbenchParent(uniqueId);
			}
		}
	}

	private void RemoveConveyorSystemLinks(WgoData wgoData)
	{
		ConveyorSystemData conveyorSystemData = gameSave.conveyorSystemData;
		if (conveyorSystemData?.conveyorComponents == null)
		{
			return;
		}
		SGuid uniqueId = wgoData.UniqueId;
		List<ConveyorComponent> conveyorComponents = conveyorSystemData.conveyorComponents;
		for (int num = conveyorComponents.Count - 1; num >= 0; num--)
		{
			ConveyorComponent conveyorComponent = conveyorComponents[num];
			if (conveyorComponent == null || conveyorComponent.wgoDataUniqueId == uniqueId)
			{
				conveyorComponents.RemoveAt(num);
				RemoveFromConveyorCache(conveyorSystemData.graphStartElements, conveyorComponent);
				RemoveFromConveyorCache(conveyorSystemData.graphEndElements, conveyorComponent);
				RemoveFromConveyorCache(conveyorSystemData.workbenchElements, conveyorComponent);
				RemoveFromConveyorCache(conveyorSystemData.splitterElements, conveyorComponent);
			}
			else
			{
				conveyorComponent.RemoveParentLink(uniqueId);
				if (TryGetWgo(conveyorComponent.wgoDataUniqueId, out var wgoData2, out var _) && wgoData2 is ConveyorWgoData conveyorWgoData)
				{
					RemoveUid(conveyorWgoData.HardConnectedWGOs, uniqueId);
				}
			}
		}
	}

	private static void RemoveFromConveyorCache<T>(List<T> cache, ConveyorComponent component) where T : ConveyorComponent
	{
		if (cache == null)
		{
			return;
		}
		for (int num = cache.Count - 1; num >= 0; num--)
		{
			if (cache[num] == component)
			{
				cache.RemoveAt(num);
			}
		}
	}

	private static void RemoveFromWorldZonesLive(GameSceneData sceneData, WgoData wgoData)
	{
		if (sceneData?.worldZones == null || wgoData == null)
		{
			return;
		}
		foreach (WorldZoneData worldZone in sceneData.worldZones)
		{
			worldZone?.RemoveWgoData(wgoData);
		}
	}

	private static List<WorldZoneData> GetZonesByDescendingPriority(GameSceneData sceneData)
	{
		List<WorldZoneData> list = new List<WorldZoneData>(sceneData.worldZones);
		list.Sort(delegate(WorldZoneData a, WorldZoneData b)
		{
			int value = a?.processingPriority ?? int.MinValue;
			return (b?.processingPriority ?? int.MinValue).CompareTo(value);
		});
		return list;
	}

	private static bool ContainsUid(List<SGuid> list, SGuid uniqueId)
	{
		if (list == null || SGuid.IsNullOrEmpty(uniqueId))
		{
			return false;
		}
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] == uniqueId)
			{
				return true;
			}
		}
		return false;
	}

	private static void RemoveUid(List<SGuid> list, SGuid uniqueId)
	{
		if (list == null || SGuid.IsNullOrEmpty(uniqueId))
		{
			return;
		}
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (list[num] == uniqueId)
			{
				list.RemoveAt(num);
			}
		}
	}

	private void WarnIfDelayedSpawnReferences(SGuid uniqueId, string wgoId)
	{
		List<SpawnDelayedObject> list = gameSave.wgoDelayedSpawnSystemData?.spawnDelayedObjects;
		if (list == null)
		{
			return;
		}
		foreach (SpawnDelayedObject item in list)
		{
			if (item != null)
			{
				SGuid sGuid = item.wgoUniqueId;
				if (SGuid.IsNullOrEmpty(sGuid) && item.wgoData != null)
				{
					sGuid = item.wgoData.UniqueId;
				}
				if (sGuid == uniqueId)
				{
					LogWarning($"Removed Wgo [{wgoId}] [{uniqueId}] is still referenced by wgoDelayedSpawnSystemData");
					break;
				}
			}
		}
	}

	private void WarnIfNpcLifeSimulatorReferences(SGuid uniqueId, string wgoId)
	{
		NPCLifeSimulatorData npcLifeSimulatorData = gameSave.npcLifeSimulatorData;
		if (npcLifeSimulatorData == null)
		{
			return;
		}
		if (npcLifeSimulatorData.GetGroupByWGOId(uniqueId) != null)
		{
			LogWarning($"Removed Wgo [{wgoId}] [{uniqueId}] is still referenced by npcLifeSimulatorData groups");
			return;
		}
		foreach (NPCPointOfInterestAnimationData animationData in npcLifeSimulatorData.AnimationDatas)
		{
			if (animationData != null && animationData.WgoId == uniqueId)
			{
				LogWarning($"Removed Wgo [{wgoId}] [{uniqueId}] is still referenced by npcLifeSimulatorData animations");
				return;
			}
		}
		foreach (NPCLifeSimulatorActionData actionsDatum in npcLifeSimulatorData.ActionsData)
		{
			if (actionsDatum != null && actionsDatum.WgoId == uniqueId)
			{
				LogWarning($"Removed Wgo [{wgoId}] [{uniqueId}] is still referenced by npcLifeSimulatorData actions");
				break;
			}
		}
	}

	private void WarnIfZombieSystemReferences(SGuid uniqueId, string wgoId)
	{
		List<SGuid> list = gameSave.zombieSystemData?.zombieOnSceneWgoIds;
		if (list != null && ContainsUid(list, uniqueId))
		{
			LogWarning($"Removed Wgo [{wgoId}] [{uniqueId}] is still referenced by zombieSystemData.zombieOnSceneWgoIds");
		}
	}

	private void WarnIfConveyorSystemReferences(SGuid uniqueId, string wgoId)
	{
		ConveyorSystemData conveyorSystemData = gameSave.conveyorSystemData;
		if (conveyorSystemData == null)
		{
			return;
		}
		if (conveyorSystemData.conveyorComponents != null)
		{
			foreach (ConveyorComponent conveyorComponent in conveyorSystemData.conveyorComponents)
			{
				if (conveyorComponent != null && conveyorComponent.wgoDataUniqueId == uniqueId)
				{
					LogWarning($"Removed Wgo [{wgoId}] [{uniqueId}] is still referenced by conveyorSystemData.conveyorComponents");
					break;
				}
			}
		}
		if (conveyorSystemData.zombieCraftActivities == null)
		{
			return;
		}
		foreach (ZombieCraftActivity zombieCraftActivity in conveyorSystemData.zombieCraftActivities)
		{
			if (zombieCraftActivity != null && zombieCraftActivity.WgoUniqueId == uniqueId)
			{
				LogWarning($"Removed Wgo [{wgoId}] [{uniqueId}] is used by a zombie craft activity in conveyorSystemData");
				break;
			}
		}
	}
}
