using System;
using System.Collections.Generic;
using LinqTools;
using Sirenix.Utilities;
using UnityEngine;

public static class WorldMap
{
	private delegate bool DoesWGOFitDelegate(WorldGameObject wgo);

	public const string IS_DISABLED = "is_disabled";

	private static readonly List<Item> _drop_items = new List<Item>();

	private static readonly List<WorldGameObject> _objs = new List<WorldGameObject>();

	private static HashSet<long> _objs_ids = new HashSet<long>();

	private static List<WorldGameObject> _npcs = new List<WorldGameObject>();

	private static List<GDPoint> _gd_points = new List<GDPoint>();

	private static List<Vendor> _vendors = new List<Vendor>();

	private static HashSet<MobSpawner> _used_spawners = new HashSet<MobSpawner>();

	private static List<MobSpawner> _world_spawners = new List<MobSpawner>();

	public static List<WorldGameObject> objs => _objs;

	public static List<GDPoint> gd_points => _gd_points;

	public static void ClearWGOsList()
	{
		_objs.Clear();
		_npcs.Clear();
		_objs_ids.Clear();
		_used_spawners.Clear();
	}

	public static void RescanWGOsList()
	{
		ClearWGOsList();
		Debug.Log("Rescan WGOs list");
		WorldGameObject[] componentsInChildren = MainGame.me.world.GetComponentsInChildren<WorldGameObject>(includeInactive: true);
		foreach (WorldGameObject worldGameObject in componentsInChildren)
		{
			if (worldGameObject.unique_id == -1)
			{
				worldGameObject.unique_id = UniqueID.GetUniqueID();
			}
			if (worldGameObject.obj_def == null)
			{
				Debug.LogError("obj def is null for obj " + worldGameObject.obj_id, worldGameObject);
				continue;
			}
			_objs_ids.Add(worldGameObject.unique_id);
			_objs.Add(worldGameObject);
			if (worldGameObject.obj_def.IsNPC())
			{
				_npcs.Add(worldGameObject);
			}
			worldGameObject.GetParentGDPoint();
		}
		ItemsDurabilityManager.Init(_objs, _drop_items);
	}

	public static void RescanSpawnersList()
	{
		Debug.Log("Rescan Spawners list");
		_world_spawners.Clear();
		MobSpawner[] componentsInChildren = MainGame.me.world.GetComponentsInChildren<MobSpawner>(includeInactive: true);
		foreach (MobSpawner mobSpawner in componentsInChildren)
		{
			if (mobSpawner.unique_id == -1)
			{
				mobSpawner.unique_id = UniqueID.GetUniqueID();
			}
			_world_spawners.Add(mobSpawner);
			mobSpawner.DelayedDeserialize();
		}
	}

	public static void RescanGDPoints(World world = null)
	{
		_gd_points = ((world == null) ? MainGame.me.world : world).GetComponentsInChildren<GDPoint>(includeInactive: true).ToList();
		SubsceneLoadManager.GetGDPoints(_gd_points);
		foreach (GDPoint gd_point in _gd_points)
		{
			gd_point.ResetPos();
		}
		Debug.Log("RescanGDPoints, count = " + _gd_points.Count);
	}

	public static void ImportGDPointsOnLoadedScene(List<GDPoint> gd_points_list)
	{
		for (int i = 0; i < gd_points_list.Count; i++)
		{
			_gd_points.Add(gd_points_list[i]);
		}
		Debug.Log("All the worlds' gd-points on import count is " + _gd_points.Count);
	}

	public static void ExportGDPointsOnUnloadedScene(List<GDPoint> gd_points_list)
	{
		foreach (GDPoint item in gd_points_list)
		{
			if (_gd_points.Contains(item))
			{
				_gd_points.Remove(item);
			}
		}
		Debug.Log("All the worlds' gd-points on export count is " + _gd_points.Count);
	}

	public static List<MobSpawner> GetMobSpawnersByCustomTag(string c_tag)
	{
		List<MobSpawner> list = new List<MobSpawner>();
		foreach (MobSpawner world_spawner in _world_spawners)
		{
			if (!(world_spawner == null) && world_spawner.custom_tag == c_tag)
			{
				list.Add(world_spawner);
			}
		}
		return list;
	}

	public static void OnNewDropItem(Item drop_item)
	{
		if (!drop_item.is_tech_point && !_drop_items.Contains(drop_item))
		{
			_drop_items.Add(drop_item);
		}
	}

	public static void OnDropItemRemoved(Item drop_item)
	{
		if (_drop_items.Contains(drop_item))
		{
			_drop_items.Remove(drop_item);
		}
	}

	public static void OnAddNewWGO(WorldGameObject wgo)
	{
		if (wgo == null)
		{
			return;
		}
		if (wgo.obj_def == null)
		{
			Debug.LogError("OnAddNewWGO error: obj_def is null for obj: " + wgo.obj_id);
			return;
		}
		if (wgo.unique_id == -1)
		{
			wgo.unique_id = UniqueID.GetUniqueID();
		}
		if (!_objs_ids.Contains(wgo.unique_id))
		{
			_objs_ids.Add(wgo.unique_id);
			_objs.Add(wgo);
			if (wgo.obj_def.IsNPC())
			{
				_npcs.Add(wgo);
			}
		}
		GDPoint[] componentsInChildren = wgo.GetComponentsInChildren<GDPoint>(includeInactive: true);
		foreach (GDPoint item in componentsInChildren)
		{
			if (!_gd_points.Contains(item))
			{
				_gd_points.Add(item);
			}
		}
	}

	public static void OnDestroyWGO(WorldGameObject wgo)
	{
		if (wgo == null || !Application.isPlaying)
		{
			return;
		}
		GDPoint[] componentsInChildren = wgo.GetComponentsInChildren<GDPoint>(includeInactive: true);
		foreach (GDPoint item in componentsInChildren)
		{
			if (_gd_points.Contains(item))
			{
				_gd_points.Remove(item);
			}
		}
		if (wgo.unique_id == -1)
		{
			wgo.unique_id = UniqueID.GetUniqueID();
		}
		if (_objs_ids.Contains(wgo.unique_id))
		{
			_objs.Remove(wgo);
			_objs_ids.Remove(wgo.unique_id);
			if (wgo.obj_def != null && wgo.obj_def.IsNPC())
			{
				_npcs.Remove(wgo);
			}
		}
	}

	public static void RescanDropItemsList()
	{
		_drop_items.Clear();
		DropResGameObject[] componentsInChildren = MainGame.me.world_root.GetComponentsInChildren<DropResGameObject>(includeInactive: true);
		DropResGameObject dropResGameObject = null;
		DropResGameObject[] array = componentsInChildren;
		foreach (DropResGameObject dropResGameObject2 in array)
		{
			if (dropResGameObject2 == null || dropResGameObject2.res == null || dropResGameObject2.res.definition == null)
			{
				dropResGameObject = dropResGameObject2;
			}
			else if (dropResGameObject2.res.definition.has_durability)
			{
				_drop_items.Add(dropResGameObject2.res);
			}
		}
		if (dropResGameObject != null)
		{
			Debug.LogError("Found a wrong item in drops list. Removing. " + dropResGameObject.name, dropResGameObject);
			dropResGameObject.is_collected = true;
		}
	}

	public static WorldGameObject GetWorldGameObjectByCustomTag(string custom_tag, bool ignore_not_found_error = false)
	{
		WorldGameObject worldGameObjectByComparator = GetWorldGameObjectByComparator((WorldGameObject wgo) => wgo.custom_tag == custom_tag);
		if (!ignore_not_found_error && worldGameObjectByComparator == null)
		{
			Debug.LogError("Error finding WGO by tag: " + custom_tag);
		}
		return worldGameObjectByComparator;
	}

	public static WorldGameObject GetWorldGameObjectByName(string name, bool ignore_not_found_error = false)
	{
		WorldGameObject worldGameObjectByComparator = GetWorldGameObjectByComparator((WorldGameObject wgo) => wgo.name == name);
		if (!ignore_not_found_error && worldGameObjectByComparator == null)
		{
			Debug.LogError("Error finding WGO by name: " + name);
		}
		return worldGameObjectByComparator;
	}

	private static List<WorldGameObject> GetNPCsByComparator(DoesWGOFitDelegate dlg, bool ignore_not_found_error = false)
	{
		List<WorldGameObject> list = new List<WorldGameObject>();
		int num = 0;
		for (num = 0; num < _npcs.Count; num++)
		{
			WorldGameObject worldGameObject = _npcs[num];
			if (worldGameObject == null)
			{
				Debug.LogError("Null NPC in NPC list");
			}
			else if (dlg(worldGameObject))
			{
				list.Add(worldGameObject);
			}
		}
		num = 0;
		while (list.Count > 0)
		{
			if (list[num].transform.parent == null || string.IsNullOrEmpty(list[num].gameObject.scene.name))
			{
				list.RemoveAt(num);
			}
			else
			{
				num++;
			}
			if (num >= list.Count)
			{
				break;
			}
		}
		if (!ignore_not_found_error && list.Count == 0)
		{
			Debug.LogError("NPC not found", MainGame.me.world);
		}
		return list;
	}

	public static WorldGameObject GetNPCByObjID(string npc_obj_id, bool ignore_not_found_error = false)
	{
		List<WorldGameObject> nPCsByComparator = GetNPCsByComparator((WorldGameObject npc) => npc.obj_id == npc_obj_id, ignore_not_found_error);
		if (nPCsByComparator == null)
		{
			Debug.LogError("Weird shit happen while finding NPC by ObjID: " + npc_obj_id);
			return null;
		}
		if (nPCsByComparator.Count != 0)
		{
			return nPCsByComparator[0];
		}
		return null;
	}

	private static WorldGameObject GetWorldGameObjectByComparator(DoesWGOFitDelegate dlg)
	{
		List<WorldGameObject> list = new List<WorldGameObject>();
		List<int> list2 = new List<int>();
		for (int i = 0; i < _objs.Count; i++)
		{
			WorldGameObject worldGameObject = _objs[i];
			if (worldGameObject == null)
			{
				Debug.LogError("Null WGO in WGO list");
				list2.Insert(0, i);
			}
			else if (!worldGameObject.IsDisabled() && dlg(worldGameObject))
			{
				list.Add(worldGameObject);
			}
		}
		foreach (int item in list2)
		{
			if (_objs[item] == null)
			{
				_objs.RemoveAt(item);
			}
		}
		while (list.Count > 0 && (list[0].transform.parent == null || string.IsNullOrEmpty(list[0].gameObject.scene.name)))
		{
			list.RemoveAt(0);
		}
		if (list.Count == 0)
		{
			Debug.LogError("World Object not found", MainGame.me.world);
			return null;
		}
		if (list.Count > 1)
		{
			Debug.LogError("Warning! Multiple objects match this condition. Count = " + list.Count);
			int num = -1;
			foreach (WorldGameObject item2 in list)
			{
				num++;
				Debug.Log("Object #" + num + ": " + item2.name, item2.gameObject);
			}
		}
		return list[0];
	}

	public static List<WorldGameObject> GetWorldGameObjectsByCustomTag(string custom_tag, bool log_if_not_found = false)
	{
		List<WorldGameObject> worldGameObjectsByComparator = GetWorldGameObjectsByComparator((WorldGameObject wgo) => wgo.custom_tag == custom_tag, log_if_not_found);
		if (worldGameObjectsByComparator == null)
		{
			Debug.LogError("Weird shit happen while finding WGO by tag: " + custom_tag);
		}
		return worldGameObjectsByComparator;
	}

	private static List<WorldGameObject> GetWorldGameObjectsByComparator(DoesWGOFitDelegate dlg, bool log_if_not_found = false)
	{
		List<WorldGameObject> list = new List<WorldGameObject>();
		foreach (WorldGameObject obj in _objs)
		{
			if (dlg(obj))
			{
				list.Add(obj);
			}
		}
		if (log_if_not_found && list.Count == 0)
		{
			Debug.LogError("World Object not found", MainGame.me.world);
			return null;
		}
		return list;
	}

	public static WorldGameObject GetWorldGameObjectByUniqueId(long instance_id, bool log_if_null = true)
	{
		if (instance_id == 0L)
		{
			Debug.LogError("World Object not found, zero id");
			return null;
		}
		foreach (WorldGameObject obj in _objs)
		{
			if (obj.unique_id == instance_id)
			{
				return obj;
			}
		}
		if (log_if_null)
		{
			Debug.LogError("World Object not found, object id = " + instance_id + ", total objects: " + _objs.Count);
		}
		return null;
	}

	public static GDPoint GetGDPointByGDTag(string tag, bool log_if_null = true, bool skip_disabled = true)
	{
		foreach (GDPoint gd_point in _gd_points)
		{
			if (gd_point.gd_tag == tag && (!skip_disabled || !gd_point.IsDisabled()))
			{
				return gd_point;
			}
		}
		if (log_if_null)
		{
			Debug.LogError("GD point not found, object tag = " + tag);
		}
		return null;
	}

	public static List<GameObject> GetGDPointsByGDTag(string tag)
	{
		List<GameObject> list = new List<GameObject>();
		foreach (GDPoint gd_point in _gd_points)
		{
			if (gd_point.gd_tag == tag && !gd_point.IsDisabled())
			{
				list.Add(gd_point.gameObject);
			}
		}
		return list;
	}

	public static GDPoint GetGDPointByName(string name, bool log_if_null = true)
	{
		foreach (GDPoint gd_point in _gd_points)
		{
			if (gd_point.name == name && !gd_point.IsDisabled())
			{
				return gd_point;
			}
		}
		if (log_if_null)
		{
			Debug.LogError("GD point not found, object name) = " + name);
		}
		return null;
	}

	public static List<GameObject> GetGDPointsByName(string name)
	{
		List<GameObject> list = new List<GameObject>();
		foreach (GDPoint gd_point in _gd_points)
		{
			if (gd_point.name == name && !gd_point.IsDisabled())
			{
				list.Add(gd_point.gameObject);
			}
		}
		return list;
	}

	public static GDPoint FIndNearestGDPointFromList(List<string> gd_point_tags, WorldGameObject wgo)
	{
		GDPoint result = null;
		float num = float.PositiveInfinity;
		foreach (string gd_point_tag in gd_point_tags)
		{
			GDPoint gDPointByGDTag = GetGDPointByGDTag(gd_point_tag);
			float num2 = Vector2.Distance(wgo.transform.position, gDPointByGDTag.transform.position);
			if (num2 <= num)
			{
				num = num2;
				result = gDPointByGDTag;
			}
		}
		return result;
	}

	public static List<WorldGameObject> GetWorldGameObjectsByObjId(string obj_id)
	{
		List<WorldGameObject> list = new List<WorldGameObject>();
		foreach (WorldGameObject obj in _objs)
		{
			if (obj.obj_id == obj_id)
			{
				list.Add(obj);
			}
		}
		return list;
	}

	public static WorldGameObject GetWorldGameObjectByObjId(string obj_id, bool ignore_not_found_error = false)
	{
		foreach (WorldGameObject obj in _objs)
		{
			if (obj.obj_id == obj_id)
			{
				return obj;
			}
		}
		if (!ignore_not_found_error)
		{
			Debug.LogError("WGO with obj ID [" + obj_id + "] not found!");
		}
		return null;
	}

	public static List<WorldGameObject> GetWorldGameObjectsByObjId(string[] obj_ids)
	{
		List<WorldGameObject> list = new List<WorldGameObject>();
		foreach (string obj_id in obj_ids)
		{
			list.AddRange(GetWorldGameObjectsByObjId(obj_id));
		}
		return list;
	}

	public static WorldGameObject SpawnWGO(Transform parent, string obj_id, Vector3? pos = null)
	{
		WorldGameObject wgo = UnityEngine.Object.Instantiate(Prefabs.wgo_prefab, parent);
		if (pos.HasValue)
		{
			wgo.transform.position = pos.Value;
		}
		ActivateGameObject(wgo.gameObject);
		wgo.SetObject(obj_id);
		GJTimer.AddTimer(0.5f, delegate
		{
			wgo.just_built = true;
			wgo.RedrawGroundSprites();
		});
		wgo.OnJustSpawned();
		OnAddNewWGO(wgo);
		if (pos.HasValue)
		{
			wgo.transform.position = pos.Value;
		}
		return wgo;
	}

	public static WorldSimpleObject SpawnWSO(Transform parent, string obj_id, Vector2? pos = null)
	{
		WorldSimpleObject worldSimpleObject = Resources.Load<WorldSimpleObject>("objects/WorldSimpleObjects/" + obj_id);
		if (worldSimpleObject == null)
		{
			Debug.LogError("Can not spawn WSO \"" + obj_id + "\"");
			return null;
		}
		WorldSimpleObject worldSimpleObject2 = UnityEngine.Object.Instantiate(worldSimpleObject, parent);
		if (pos.HasValue)
		{
			worldSimpleObject2.transform.position = pos.Value;
		}
		return worldSimpleObject2;
	}

	public static void ActivateGameObject(GameObject go)
	{
		if (go == null)
		{
			Debug.LogError("ActivateGameObject: Trying to activate a null object");
			return;
		}
		try
		{
			go.SetActive(value: true);
		}
		catch (Exception ex)
		{
			Debug.LogError("ActivateGameObject ERROR: " + ex);
			return;
		}
		if (!Application.isPlaying)
		{
			return;
		}
		GJTimer.AddTimer(0.01f, delegate
		{
			try
			{
				go.SetActive(value: true);
			}
			catch (Exception ex2)
			{
				Debug.LogError("ActivateGameObject ERROR after timer: " + ex2);
			}
		});
	}

	public static Ground.GroudType GetGroundType(Vector2 point)
	{
		Collider2D[] array = Physics2D.OverlapPointAll(point);
		Ground.GroudType result = Ground.GroudType.None;
		int num = int.MinValue;
		Collider2D[] array2 = array;
		foreach (Collider2D collider2D in array2)
		{
			SpriteRenderer component = collider2D.gameObject.GetComponent<SpriteRenderer>();
			if (component == null)
			{
				continue;
			}
			Ground component2 = collider2D.gameObject.GetComponent<Ground>();
			if (!(component2 == null))
			{
				int layerValueFromID = SortingLayer.GetLayerValueFromID(component.sortingLayerID);
				if (layerValueFromID > num)
				{
					num = layerValueFromID;
					result = component2.type;
				}
			}
		}
		return result;
	}

	public static void VendorsTradeWithBank()
	{
		FillVendorsList();
		Debug.Log("Started vendors level up. Total vendors count: " + _vendors.Count);
		foreach (Vendor vendor in _vendors)
		{
			if (vendor == null)
			{
				Debug.LogError("Found null vendor in vendors list! Call Bulat!");
			}
			else
			{
				vendor.OnEndOfDay();
			}
		}
	}

	public static void FillVendorsList()
	{
		_vendors = new List<Vendor>();
		for (int i = 0; i < _objs.Count; i++)
		{
			if (!(_objs[i] == null))
			{
				Vendor vendor = _objs[i].vendor;
				if (vendor != null)
				{
					_vendors.Add(vendor);
				}
			}
		}
	}

	public static void AddVendor(Vendor new_vendor)
	{
		if (_vendors == null || _vendors.Count == 0)
		{
			FillVendorsList();
		}
		else
		{
			_vendors.Add(new_vendor);
		}
	}

	public static void ToGameSave(GameSave save)
	{
		DropsList.me.ToGameSave(save);
	}

	public static void FromGameSave(GameSave save)
	{
		DropsList.me.FromGameSave(save);
	}

	public static void RestoreBubbles()
	{
		foreach (WorldGameObject obj in _objs)
		{
			obj.components.RefreshBubblesData(false);
		}
	}

	public static ChurchPulpit GetChurchPulpit()
	{
		WorldGameObject worldGameObjectByCustomTag = GetWorldGameObjectByCustomTag("church_pulpit");
		ChurchPulpit result = null;
		if (worldGameObjectByCustomTag == null)
		{
			Debug.LogError("Couldn't find a church pulpit");
		}
		else
		{
			result = worldGameObjectByCustomTag.GetComponentsInChildren<ChurchPulpit>(includeInactive: true)[0];
		}
		return result;
	}

	public static void OnUsedSpawner(MobSpawner spawner)
	{
		if (!_used_spawners.Contains(spawner))
		{
			_used_spawners.Add(spawner);
		}
		if (!_world_spawners.Contains(spawner))
		{
			_world_spawners.Add(spawner);
		}
	}

	public static MobSpawner GetSpawnerByCoords(Vector2 pos)
	{
		foreach (MobSpawner world_spawner in _world_spawners)
		{
			if (ExtentionTools.EqualsTo(world_spawner.transform.position, pos))
			{
				return world_spawner;
			}
		}
		Vector2 vector = pos;
		Debug.LogWarning("Couldn't find a spawner by coords " + vector.ToString() + ", total_spawners = " + _world_spawners.Count);
		return null;
	}

	public static void DeserializeAllLinkedWorkers()
	{
		for (int i = 0; i < _objs.Count; i++)
		{
			WorldGameObject worldGameObject = _objs[i];
			if (worldGameObject.linked_worker_unique_id <= 0)
			{
				worldGameObject.linked_worker = null;
				continue;
			}
			WorldGameObject worldGameObjectByUniqueId = GetWorldGameObjectByUniqueId(worldGameObject.linked_worker_unique_id);
			if (worldGameObjectByUniqueId == null)
			{
				Debug.LogError("FATAL ERROR: failed to deserialize linked worker: WGO with unique_id=" + worldGameObject.unique_id + " not found!");
			}
			else
			{
				worldGameObject.linked_worker = worldGameObjectByUniqueId;
			}
		}
	}

	public static string RemoveZombieWorkerToStock(WorldGameObject worker, string gd_point_tag = "")
	{
		string empty = string.Empty;
		GDPoint gDPointByGDTag = GetGDPointByGDTag((!string.IsNullOrEmpty(gd_point_tag)) ? gd_point_tag : "default_destroy_point");
		if (gDPointByGDTag == null)
		{
			return empty + "Can't find GD point: " + gd_point_tag;
		}
		Debug.Log("Teleporting " + worker.obj_id + " to GD point: " + gDPointByGDTag.name, gDPointByGDTag.gameObject);
		worker.transform.position = gDPointByGDTag.transform.position;
		worker.RefreshPositionCache();
		worker.OnCameToGDPoint(gDPointByGDTag);
		worker.SetParam("is_disabled", 1f);
		if (worker.linked_workbench != null)
		{
			if (worker.linked_workbench.components.craft.is_crafting)
			{
				worker.linked_workbench.OnWorkFinished();
			}
			if (worker.linked_workbench.obj_def.type == ObjectDefinition.ObjType.PorterStation)
			{
				worker.linked_workbench.porter_station.state = PorterStation.PorterState.None;
				Item backpack = worker.worker.GetBackpack();
				if (backpack != null && backpack.inventory.Count > 0)
				{
					MainGame.me.player.DropItems(backpack.inventory);
					backpack.inventory = new List<Item>();
				}
			}
			worker.linked_workbench.linked_worker = null;
		}
		worker.components.character.SetNoWorkerTool();
		worker.components.character.StopMovement();
		worker.worker.UpdateWorkerSkin(Worker.WorkerActivity.None);
		return empty;
	}

	public static string SpawnZombieWorkerFromStock(WorldGameObject workbench, Item zombie_worker_item, out WorldGameObject o, out bool is_success)
	{
		string empty = string.Empty;
		is_success = false;
		empty += Worker.TransformWorker(Worker.WorkerState.ItemOverhead, zombie_worker_item, null, Worker.WorkerState.WGO, out var _, out o);
		if (o == null)
		{
			empty += "FATAL ERROR: not found worker_wgo!\n";
			if (zombie_worker_item == null)
			{
				return empty + "FATAL ERROR: worker_item is NULL!\n";
			}
			if (zombie_worker_item.worker == null)
			{
				return empty + "FATAL ERROR: worker_item is NOT worker!\n";
			}
			return empty + "FATAL ERROR: worker " + zombie_worker_item.worker.worker_unique_id + " has no worker_wgo!\n";
		}
		if (o.GetParamInt("is_disabled") == 0)
		{
			Debug.LogError("Zombie_worker \"" + o.name + "\" is NOT disabled!");
		}
		if (workbench == null)
		{
			return empty + "FATAL ERROR: workbench is null!";
		}
		DockPoint availableDockPointForZombie = workbench.GetAvailableDockPointForZombie();
		if (availableDockPointForZombie == null)
		{
			Debug.Log("Can not spawn zombie_worker: not found any available dock point!");
			return empty;
		}
		Debug.Log("Teleporting " + o.obj_id + " (" + o.name + ") to GD point: " + workbench.name, workbench.gameObject);
		o.transform.position = availableDockPointForZombie.transform.position;
		o.RefreshPositionCache();
		o.gameObject.SetActive(value: true);
		o.components.character.LookAt(availableDockPointForZombie.action_dir);
		workbench.linked_worker = o;
		if (workbench.obj_def.type == ObjectDefinition.ObjType.PorterStation)
		{
			workbench.porter_station.state = PorterStation.PorterState.Waiting;
		}
		o.SetParam("is_disabled", 0f);
		Debug.Log("Teleporting, output name = " + o.name + ", obj_id = " + o.obj_id + ", instance_id = " + o.gameObject.GetInstanceID());
		is_success = true;
		return empty;
	}

	public static bool AttachInvisibleWorker(WorldGameObject workbench, out WorldGameObject worker_wgo)
	{
		worker_wgo = null;
		if (workbench == null)
		{
			Debug.LogError("AttachInvisibleWorker error: workbench is null!");
			return false;
		}
		if (workbench.is_dead || workbench.is_removed || workbench.is_removing)
		{
			Debug.LogError("AttachInvisibleWorker error: workbench is dead or removed!");
			return false;
		}
		DockPoint availableDockPointForZombie = workbench.GetAvailableDockPointForZombie();
		if (availableDockPointForZombie == null)
		{
			Debug.Log("AttachInvisibleWorker error: not found any available dock point!");
			return false;
		}
		WorkerDefinition data = GameBalance.me.GetData<WorkerDefinition>("worker_invisible");
		if (data == null)
		{
			Debug.Log("AttachInvisibleWorker error: WorkerDefinition is null!");
			return false;
		}
		Item base_body = MainGame.me.save.GenerateBody(1, 3);
		worker_wgo = SpawnWGO(MainGame.me.world_root, data.worker_wgo, availableDockPointForZombie.transform.position);
		Worker worker = MainGame.me.save.workers.CreateNewWorker(worker_wgo, data.id, base_body);
		worker.ForcingWorkerK(do_force_working_k: true);
		worker.UpdateWorkerLevel();
		workbench.linked_worker = worker_wgo;
		return true;
	}

	public static bool DoesWGOOnCoordsExist(string obj_id, Vector2 coords_vector2)
	{
		bool result = false;
		float epsilon = 2f;
		List<WorldGameObject> worldGameObjectsByObjId = GetWorldGameObjectsByObjId(obj_id);
		if (!worldGameObjectsByObjId.IsNullOrEmpty())
		{
			foreach (WorldGameObject item in worldGameObjectsByObjId)
			{
				if (!(item == null))
				{
					Vector3 position = item.transform.position;
					if (position.x.EqualsTo(coords_vector2.x, epsilon) && position.y.EqualsTo(coords_vector2.y, epsilon))
					{
						result = true;
						break;
					}
				}
			}
		}
		return result;
	}

	public static WorldGameObject SpawnWGO(Transform root, string obj_id, Vector2 coords, string custom_tag)
	{
		WorldGameObject worldGameObject = SpawnWGO(root, obj_id, coords);
		worldGameObject.custom_tag = custom_tag;
		worldGameObject.RecalculateZoneBelonging();
		return worldGameObject;
	}

	public static bool DeleteWGO(string[] possible_ids, Vector2 coords, string custom_tag)
	{
		if (possible_ids == null || possible_ids.Length == 0)
		{
			Debug.LogError("DeleteWGO error: possible_ids is null or empty!");
			return false;
		}
		string text = string.Empty;
		foreach (string text2 in possible_ids)
		{
			if (!string.IsNullOrEmpty(text))
			{
				text += ", ";
			}
			text += text2;
		}
		List<WorldGameObject> list = FindWGOs(possible_ids, coords, custom_tag);
		if (list != null && list.Count > 0)
		{
			if (list.Count > 1)
			{
				Debug.LogWarning($"Found more than one wgo with ids={{{text}}}, coords={coords}, custom_tag={custom_tag}");
			}
			Debug.Log($"Destroying WGO with id={list[0].obj_id}, coords={list[0].transform.position}, custom_tag={list[0].custom_tag}");
			list[0].DestroyMe();
			return true;
		}
		Debug.LogError($"DeleteWGO error: not deleted WGO: ids={{{text}}}, coords={coords}, custom_tag={custom_tag}");
		return false;
	}

	public static bool MoveWGO(string[] possible_ids, Vector2 coords, string custom_tag, Vector2 new_coords)
	{
		if (possible_ids == null || possible_ids.Length == 0)
		{
			Debug.LogError("MoveWGO error: possible_ids is null or empty!");
			return false;
		}
		string text = string.Empty;
		foreach (string text2 in possible_ids)
		{
			if (!string.IsNullOrEmpty(text))
			{
				text += ", ";
			}
			text += text2;
		}
		List<WorldGameObject> list = FindWGOs(possible_ids, coords, custom_tag);
		if (list != null && list.Count > 0)
		{
			if (list.Count > 1)
			{
				Debug.LogWarning($"Found more than one wgo with ids={{{text}}}, coords={coords}, custom_tag={custom_tag}");
			}
			Debug.Log($"Moveing WGO with id={list[0].obj_id}, coords={list[0].transform.position}, custom_tag={list[0].custom_tag} to new pos={new_coords}");
			list[0].transform.position = new_coords;
			list[0].RecalculateZoneBelonging();
			return true;
		}
		Debug.LogError($"MoveWGO error: not moved WGO: ids={{{text}}}, coords={coords}, custom_tag={custom_tag}");
		return false;
	}

	public static List<WorldGameObject> FindWGOs(string[] possible_ids, Vector2 coords, string custom_tag)
	{
		if (possible_ids == null || possible_ids.Length == 0)
		{
			Debug.LogError("FindWGO error: possible_ids is null or empty!");
			return null;
		}
		bool check_custom_tag = !string.IsNullOrEmpty(custom_tag);
		string[] array = possible_ids;
		foreach (string possible_id in array)
		{
			List<WorldGameObject> worldGameObjectsByComparator = GetWorldGameObjectsByComparator(delegate(WorldGameObject o)
			{
				if (o.obj_id != possible_id)
				{
					return false;
				}
				if (((Vector2)o.transform.position - coords).sqrMagnitude > 1f)
				{
					return false;
				}
				return (!check_custom_tag || !(o.custom_tag != custom_tag)) ? true : false;
			});
			if (worldGameObjectsByComparator != null && worldGameObjectsByComparator.Count > 0)
			{
				return worldGameObjectsByComparator;
			}
		}
		string text = string.Empty;
		array = possible_ids;
		foreach (string text2 in array)
		{
			if (!string.IsNullOrEmpty(text))
			{
				text += ", ";
			}
			text += text2;
		}
		Debug.LogWarning($"FindWGO warning: not found WGO: id={{{text}}}, coords={coords}, custom_tag={custom_tag}");
		return null;
	}

	public static void ImportWGOsList(List<WorldGameObject> wgo_list)
	{
		foreach (WorldGameObject item in wgo_list)
		{
			_objs.Add(item);
		}
		Debug.Log("WorldMap:ImportWGOsList, imported WGOs : " + wgo_list.Count);
	}

	public static void ExportWGOsList(List<WorldGameObject> wgo_list)
	{
		int num = 0;
		foreach (WorldGameObject item in wgo_list)
		{
			if (_objs.Contains(item))
			{
				num++;
				_objs.Remove(item);
			}
		}
		Debug.Log("WorldMap:ExportWGOsList, exported WGOs: " + num + "/" + wgo_list.Count);
	}

	public static void TryRemoveStackedChurchVisitors()
	{
		List<WorldGameObject> worldGameObjectsByObjId = GetWorldGameObjectsByObjId("npc_church_visitor");
		if (worldGameObjectsByObjId == null)
		{
			return;
		}
		int num = 0;
		foreach (WorldGameObject item in worldGameObjectsByObjId)
		{
			if (item.pos.x >= -667f && item.pos.x <= 1131f && item.pos.y >= -8818f && item.pos.y <= -7919f && !item.IsMoving())
			{
				num++;
				item.DestroyMe();
			}
		}
		Debug.Log($"Remove stacked church visitors, count: {num}");
	}
}
