using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class GameSceneData
{
	public string id;

	public Vector3 offset;

	public List<string> loadedContentsList = new List<string>();

	public List<WgoData> wgoDataList = new List<WgoData>();

	public List<WsoData> wsoDataList = new List<WsoData>();

	public List<DropData> droppedItems = new List<DropData>();

	public List<DropData> queuedDrops = new List<DropData>();

	public List<TechPointDropData> techPointDrops = new List<TechPointDropData>();

	public List<WorldZoneData> worldZones = new List<WorldZoneData>();

	public List<FightingLevelData> fightingLevels = new List<FightingLevelData>();

	private WgoDataCache cache;

	[NonSerialized]
	private QuadTreeRoot<WgoData> quadTreeRoot;

	public bool HasCache => cache != null;

	public QuadTreeRoot<WgoData> QuadTreeRoot => quadTreeRoot;

	[field: NonSerialized]
	public SortedDictionary<int, List<WorldZoneData>> WorldZonesByProcessingPriority { get; private set; } = new SortedDictionary<int, List<WorldZoneData>>();


	public event Action<WgoData, bool> OnWgoDataAdd;

	public event Action<WgoData> OnWgoDataRemove;

	public event Action<WgoData> OnWgoDataPreRemove;

	public static event Action<string, WgoData> OnWgoDataOnScenePreRemove;

	public event Action<WsoData> OnWsoDataAdd;

	public event Action<WsoData> OnWsoDataRemove;

	public static event Action<string, WsoData> OnWsoDataOnScenePreRemove;

	public event Action<FightingLevelData> OnFightingLevelDataAdded;

	public event Action<FightingLevelData> OnFightingLevelDataRemoved;

	public event Action<DropData> OnDropAdd;

	public event Action<DropData> OnDropRemove;

	public static event Action<string, DropData> OnDropOnScenePreRemoved;

	public GameSceneData()
	{
	}

	public GameSceneData(string id, Vector3 offset, WgoDataCache cache)
	{
		this.id = id;
		this.offset = offset;
		this.cache = cache;
	}

	public void PrepareForGame(WgoDataCache cache)
	{
		this.cache = cache;
		RebuildWorldZonesByProcessingPriorityCache();
		foreach (WorldZoneData worldZone in worldZones)
		{
			worldZone.PrepareForGame();
		}
		List<WgoData> list = new List<WgoData>();
		foreach (WgoData wgoData in wgoDataList)
		{
			if (wgoData is ZombieWgoData)
			{
				list.Add(wgoData);
			}
			else
			{
				wgoData.PrepareForGame();
			}
		}
		foreach (WgoData item in list)
		{
			item.PrepareForGame();
		}
		foreach (WsoData wsoData in wsoDataList)
		{
			wsoData.PrepareForGame();
		}
	}

	public void AddWgoData(WgoData wgoData, bool recheckVisibilityOnSpawn = false)
	{
		wgoData.WorldId = id;
		cache.AddWgoDataToCache(wgoData);
		wgoDataList.Add(wgoData);
		this.OnWgoDataAdd?.Invoke(wgoData, recheckVisibilityOnSpawn);
		HandleWgoDataAddWorldZone(wgoData);
	}

	public WgoData AddWgoData(string wgoId, Vector3 position, string customTag = "", bool recheckVisibilityOnSpawn = false)
	{
		WgoData wgoData = null;
		if (GameBalance.Me.conveyorWgosCache.TryGetValue(wgoId, out var value))
		{
			wgoData = new ConveyorWgoData(value.conveyorType, wgoId, position, id);
			if (!string.IsNullOrEmpty(customTag))
			{
				wgoData.CustomTag = customTag;
			}
		}
		else
		{
			wgoData = ((customTag == "") ? new WgoData(wgoId, position, id) : new WgoData(wgoId, position, id, customTag));
		}
		cache.AddWgoDataToCache(wgoData);
		wgoDataList.Add(wgoData);
		this.OnWgoDataAdd?.Invoke(wgoData, recheckVisibilityOnSpawn);
		HandleWgoDataAddWorldZone(wgoData);
		return wgoData;
	}

	public void RemoveWgoData(WgoData wgoData, bool clearCraftComponent = true)
	{
		if (cache.RemoveWgoDataFromCache(wgoData))
		{
			wgoDataList.Remove(wgoData);
			wgoData.OnRemove(clearCraftComponent);
			GameSceneData.OnWgoDataOnScenePreRemove?.Invoke(id, wgoData);
			this.OnWgoDataPreRemove?.Invoke(wgoData);
			this.OnWgoDataRemove?.Invoke(wgoData);
			HandleWgoDataRemoveWorldZone(wgoData);
		}
	}

	public DropData AddDrop(Item droppableItem, Vector3 pos)
	{
		DropData drop = new DropData(droppableItem, pos, id);
		return AddDrop(drop);
	}

	public DropData AddDrop(DropData drop)
	{
		droppedItems.Add(drop);
		this.OnDropAdd?.Invoke(drop);
		drop.TryStartAutoDestroyTimer();
		return drop;
	}

	public void AddDropToQueue(Item droppableItem, Vector3 pos, bool getPosDropFromDockPoint = false)
	{
		for (int i = 0; i < queuedDrops.Count; i++)
		{
			if (!(queuedDrops[i].Id != droppableItem.id))
			{
				int num = queuedDrops[i].CanAddItemCount(droppableItem);
				if (num > 0)
				{
					Item item = droppableItem.Split(num);
					queuedDrops[i].AddItem(item);
				}
				if (droppableItem.Count == 0)
				{
					return;
				}
			}
		}
		DropData item2 = new DropData(droppableItem, pos, id);
		queuedDrops.Add(item2);
	}

	public void RemoveDrop(DropData drop)
	{
		drop.MarkAsRemoving();
		droppedItems.Remove(drop);
		queuedDrops.Remove(drop);
		GameSceneData.OnDropOnScenePreRemoved?.Invoke(id, drop);
		this.OnDropRemove?.Invoke(drop);
	}

	public void AddTechPointDrop(TechPointDropData drop)
	{
		techPointDrops.Add(drop);
	}

	public void RemoveTechPointDrop(TechPointDropData drop)
	{
		techPointDrops.Remove(drop);
	}

	public void ProcessQueuedDrops()
	{
		foreach (DropData queuedDrop in queuedDrops)
		{
			AddDrop(queuedDrop);
		}
		queuedDrops.Clear();
	}

	public void AddWorldZoneData(WorldZoneData worldZoneData, WorldZoneBakedData bakedData)
	{
		worldZones.Add(worldZoneData);
		RegisterWorldZoneInProcessingPriorityCache(worldZoneData);
		foreach (WgoData wgoData2 in wgoDataList)
		{
			worldZoneData.TryAddWgoData(wgoData2);
		}
		if (bakedData.prebuiltWgoParams != null)
		{
			WorldData worldData = MainGame.Instance.GameSave.worldData;
			foreach (WorldZonePrebuiltWgoParams prebuiltWgoParam in bakedData.prebuiltWgoParams)
			{
				if (prebuiltWgoParam.execExpressionAfterBuilding)
				{
					WgoData wgoData = worldData.GetWgoData(prebuiltWgoParam.wgoUniqueId);
					if (wgoData != null && wgoData.Definition.TryGetBuildingDefForWgo(out var buildingDef))
					{
						worldData.TryExecutePrebuiltWgoAfterBuildingExpressions(wgoData, buildingDef);
					}
				}
			}
		}
		Debug.Log("Adding world zone data with id:[" + worldZoneData.id + "] to scene:[" + id + "]");
	}

	public WorldZoneData GetWorldZoneDataById(string id)
	{
		return worldZones.Find((WorldZoneData worldZone) => worldZone.id == id);
	}

	public void RemoveWorldZoneData(WorldZoneData worldZoneData)
	{
		if (worldZones.Remove(worldZoneData))
		{
			DetachWgosFromRemovedWorldZone(worldZoneData);
			RebuildWorldZonesByProcessingPriorityCache();
		}
	}

	private void DetachWgosFromRemovedWorldZone(WorldZoneData worldZoneData)
	{
		if (worldZoneData?.wgoDataList == null)
		{
			return;
		}
		for (int i = 0; i < worldZoneData.wgoDataList.Count; i++)
		{
			WgoData wgoData = MainGame.WorldData.GetWgoData(worldZoneData.wgoDataList[i]);
			if (wgoData != null && wgoData.WorldZoneData == worldZoneData)
			{
				wgoData.WorldZoneData = null;
			}
		}
	}

	private void RebuildWorldZonesByProcessingPriorityCache()
	{
		if (WorldZonesByProcessingPriority == null)
		{
			WorldZonesByProcessingPriority = new SortedDictionary<int, List<WorldZoneData>>();
		}
		WorldZonesByProcessingPriority.Clear();
		foreach (WorldZoneData worldZone in worldZones)
		{
			RegisterWorldZoneInProcessingPriorityCache(worldZone);
		}
	}

	private void RegisterWorldZoneInProcessingPriorityCache(WorldZoneData worldZoneData)
	{
		if (!WorldZonesByProcessingPriority.TryGetValue(worldZoneData.processingPriority, out var value))
		{
			value = new List<WorldZoneData>();
			WorldZonesByProcessingPriority[worldZoneData.processingPriority] = value;
		}
		value.Add(worldZoneData);
	}

	private void HandleWgoDataAddWorldZone(WgoData wgoData)
	{
		foreach (KeyValuePair<int, List<WorldZoneData>> item in WorldZonesByProcessingPriority.Reverse())
		{
			item.Deconstruct(out var _, out var value);
			foreach (WorldZoneData item2 in value)
			{
				if (item2.TryAddWgoData(wgoData))
				{
					return;
				}
			}
		}
	}

	private void HandleWgoDataRemoveWorldZone(WgoData wgoData)
	{
		foreach (KeyValuePair<int, List<WorldZoneData>> item in WorldZonesByProcessingPriority.Reverse())
		{
			item.Deconstruct(out var _, out var value);
			foreach (WorldZoneData item2 in value)
			{
				item2.RemoveWgoData(wgoData);
			}
		}
	}

	public void AddWsoData(WsoData wsoData)
	{
		wsoData.WorldId = id;
		cache.AddWsoDataToCache(wsoData);
		wsoDataList.Add(wsoData);
		this.OnWsoDataAdd?.Invoke(wsoData);
	}

	public WsoData AddWsoData(string defId, Vector3 position)
	{
		WsoData wsoData = new WsoData(GameBalance.Me.GetData<WSODef>(defId), new SGuid(), position, id);
		cache.AddWsoDataToCache(wsoData);
		wsoDataList.Add(wsoData);
		this.OnWsoDataAdd?.Invoke(wsoData);
		return wsoData;
	}

	public void RemoveWsoData(WsoData wsoData)
	{
		if (cache.RemoveWsoDataFromCache(wsoData))
		{
			wsoDataList.Remove(wsoData);
			wsoData.Cleanup();
			GameSceneData.OnWsoDataOnScenePreRemove?.Invoke(id, wsoData);
			this.OnWsoDataRemove?.Invoke(wsoData);
		}
	}

	public void AddFightingLevelData(string id)
	{
		if (fightingLevels.Find((FightingLevelData fightingLevel) => fightingLevel.id == id) != null)
		{
			Debug.LogWarning("Fighting level with id:[" + id + "] already exists in scene:[" + this.id + "]");
		}
		else if (MainGame.Instance.gameSceneConfigs.Find((GameSceneConfig c) => c.name == this.id) == null)
		{
			Debug.LogError("Can't find game scene config with id:[" + this.id + "] in gameSceneConfigs");
		}
		else
		{
			FightingLevelData fightingLevelData = new FightingLevelData(id);
			fightingLevels.Add(fightingLevelData);
			Debug.Log("Added fighting level data with id:[" + id + "] to scene:[" + this.id + "]");
			this.OnFightingLevelDataAdded?.Invoke(fightingLevelData);
		}
	}

	public void RemoveFightingLevelData(string id)
	{
		int num = fightingLevels.FindIndex((FightingLevelData fightingLevel) => fightingLevel.id == id);
		if (num == -1)
		{
			Debug.LogWarning("Fighting level with id:[" + id + "] not found in scene:[" + this.id + "]");
		}
		else
		{
			FightingLevelData obj = fightingLevels[num];
			fightingLevels.RemoveAt(num);
			Debug.Log("Removed fighting level data with id:[" + id + "] from scene:[" + this.id + "]");
			this.OnFightingLevelDataRemoved?.Invoke(obj);
		}
	}

	public void ApplyStageForFightingLevel(string id, int stageId)
	{
		int num = fightingLevels.FindIndex((FightingLevelData fightingLevel) => fightingLevel.id == id);
		if (num == -1)
		{
			Debug.LogWarning("Fighting level with id:[" + id + "] not found in scene:[" + this.id + "]");
		}
		else
		{
			fightingLevels[num].CurStageId = stageId;
		}
	}
}
