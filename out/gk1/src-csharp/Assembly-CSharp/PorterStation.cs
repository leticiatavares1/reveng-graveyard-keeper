using System;
using System.Collections.Generic;
using UnityEngine;

public class PorterStation : MonoBehaviour
{
	public enum PorterState
	{
		None,
		Waiting,
		GoingToDestination,
		GoingToSource
	}

	public const string WORLD_ZONE_POINT_SUFFIX = "_d";

	public const string EVENT_ON_CAME_TO_DESTINATION = "porter_on_came_to_destination";

	public const string EVENT_ON_CAME_TO_SOURCE = "porter_on_came_to_source";

	public const int TWO_HANDED_ITEM_SIZE = 4;

	private bool _is_correctly_inited;

	private WorldGameObject _wgo;

	private WorldZone _source;

	private WorldZone _destination;

	public PorterState state;

	public bool has_linked_worker;

	public DockPoint waiting_point;

	private List<string> _items_black_list = new List<string>();

	public bool is_correctly_inited => _is_correctly_inited;

	public WorldZone source
	{
		get
		{
			if (_source == null)
			{
				_source = _wgo.GetMyWorldZone();
				if (_source == null)
				{
					Debug.LogError("Source WorldZone is null!");
				}
			}
			return _source;
		}
	}

	public WorldZone destination
	{
		get
		{
			if (_destination == null)
			{
				if (string.IsNullOrEmpty(source?.id))
				{
					Debug.LogError("Source WorldZone id is null!");
					return null;
				}
				string text = string.Empty;
				foreach (TransportPathsDefinition transport_path in GameBalance.me.transport_paths)
				{
					if (!(transport_path.source_zone_id != source.id) && !(transport_path.station_wgo_id != _wgo?.obj_id))
					{
						text = transport_path.destination_zone_id;
					}
				}
				if (string.IsNullOrEmpty(text))
				{
					Debug.LogError("FATAL ERROR: Not found transport path for source_zone_id=\"" + source.id + "\"; wgo=\"" + _wgo?.obj_id + "\"");
					return null;
				}
				_destination = WorldZone.GetZoneByID(text);
				if (_destination == null)
				{
					Debug.LogError("Destination WorldZone is null!");
				}
			}
			return _destination;
		}
	}

	public List<string> blacklist
	{
		get
		{
			return _items_black_list ?? (_items_black_list = new List<string>());
		}
		set
		{
			_items_black_list = value ?? new List<string>();
		}
	}

	public bool HasLinkedWorker()
	{
		if (_wgo == null)
		{
			return false;
		}
		if (!_wgo.has_linked_worker)
		{
			return false;
		}
		if (!_wgo.linked_worker.IsWorker())
		{
			return false;
		}
		return true;
	}

	public void Start()
	{
		if (!_is_correctly_inited)
		{
			Init();
		}
	}

	public void Init()
	{
		_is_correctly_inited = false;
		try
		{
			_wgo = GetComponent<WorldGameObject>();
		}
		catch (Exception message)
		{
			Debug.LogError(message);
		}
		if (_wgo == null)
		{
			Debug.LogError("Can not init PorterStation: wgo not found!");
			return;
		}
		has_linked_worker = HasLinkedWorker();
		_is_correctly_inited = true;
		state = PorterState.None;
		_items_black_list = new List<string>();
		Debug.Log("Inited porter station on wgo \"" + _wgo.obj_id + "\"");
	}

	public void Update()
	{
		if (_is_correctly_inited && state != 0 && has_linked_worker && state == PorterState.Waiting && TrySendPorter())
		{
			Debug.Log("Porter send to " + _destination.id);
		}
	}

	private bool TrySendPorter()
	{
		if (!_is_correctly_inited)
		{
			return false;
		}
		if (source == null)
		{
			return false;
		}
		if (destination == null)
		{
			return false;
		}
		if (!has_linked_worker)
		{
			return false;
		}
		Item item = _wgo.linked_worker.worker?.GetBackpack();
		if (item == null)
		{
			Debug.LogError("TrySendPorter error: porter_backpack is null!");
			return false;
		}
		GDPoint gDPoint = null;
		gDPoint = ((_destination.id == "mf_wood") ? WorldMap.GetGDPointByGDTag(_destination.id + "_d" + (ShortWayFromSteepWorkyardToHomeIsAvailable() ? "" : "_2")) : ((!(_destination.id == "player_tavern_cellar")) ? WorldMap.GetGDPointByGDTag(_destination.id + "_d") : WorldMap.GetGDPointByGDTag(_destination.id + "_d" + (ShortWayFromCellarToTavernCellarIsAvailable() ? "" : "_2"))));
		if (gDPoint == null)
		{
			Debug.LogError("TrySendPorter error: not found destination_point by tag \"" + _destination.id + "_d\"!");
			return false;
		}
		BaseCharacterComponent character = _wgo.linked_worker.components.character;
		if (character == null)
		{
			Debug.LogError("TrySendPorter error: linked_worker has no BaseCharacterComponent!");
			return false;
		}
		if (!FillPorterInventoryFromSource(item))
		{
			Debug.LogError("TrySendPorter error: error while FillPorterInventoryFromSource");
		}
		if (item.inventory.Count == 0)
		{
			return false;
		}
		_wgo.linked_worker.TeleportToGDPoint(source.id + "_d");
		_wgo.linked_worker.DrawPuffFX();
		character.GoTo(gDPoint.gameObject, snap_to_node: false, null, null, with_cinematic: false, MovementComponent.GoToMethod.GDGraph, "porter_on_came_to_destination", null, from_script: false, gDPoint);
		character.SetSpeed(1.5f);
		_wgo.linked_worker.worker.UpdateWorkerSkin(Worker.WorkerActivity.Porter);
		ChunkedGameObject component = _wgo.linked_worker.GetComponent<ChunkedGameObject>();
		if (component != null)
		{
			component.active_now_because_of_movement = true;
		}
		state = PorterState.GoingToDestination;
		return true;
	}

	private List<Item> GetPossibleForCarryingItems_OLD()
	{
		if (!_is_correctly_inited)
		{
			return null;
		}
		if (source == null)
		{
			return null;
		}
		List<Item> list = new List<Item>();
		List<Inventory> multiInventory = _source.GetMultiInventory();
		if (multiInventory == null || multiInventory.Count == 0)
		{
			return null;
		}
		for (int i = 0; i < multiInventory.Count; i++)
		{
			List<Item> list2 = multiInventory[i].data?.inventory;
			if (list2 == null || list2.Count == 0)
			{
				continue;
			}
			foreach (Item item in list2)
			{
				if (CanCarryItem(item))
				{
					list.Add(item);
				}
			}
		}
		return list;
	}

	private bool FillPorterInventoryFromSource(Item porter_backpack)
	{
		if (!_is_correctly_inited)
		{
			return false;
		}
		if (source == null)
		{
			return false;
		}
		List<Inventory> multiInventory = _source.GetMultiInventory();
		if (multiInventory == null)
		{
			return false;
		}
		for (int i = 0; i < multiInventory.Count; i++)
		{
			List<Item> list = multiInventory[i].data?.inventory;
			if (list == null || list.Count == 0)
			{
				continue;
			}
			for (int j = 0; j < list.Count; j++)
			{
				if (!CanCarryItem(list[j]))
				{
					continue;
				}
				Item item = list[j];
				int num = CanAddToBackPackCount(porter_backpack, item);
				if (num != 0)
				{
					if (item.value - num > 0)
					{
						Item item2 = new Item(item)
						{
							value = num
						};
						porter_backpack.AddItem(item2);
						item.value -= num;
					}
					else
					{
						porter_backpack.AddItem(new Item(item));
						list.RemoveAt(j);
						j--;
					}
				}
			}
		}
		return true;
	}

	private int CanAddToBackPackCount(Item porter_backpack, Item item)
	{
		if (item == null || item.IsEmpty())
		{
			return 0;
		}
		if (porter_backpack == null)
		{
			return 0;
		}
		if (porter_backpack.inventory == null)
		{
			porter_backpack.inventory = new List<Item>();
		}
		int num = 10;
		int num2 = 0;
		bool is_big = item.definition.is_big;
		bool is_crate = item.definition.is_crate;
		int stack_count = item.definition.stack_count;
		foreach (Item item2 in porter_backpack.inventory)
		{
			if (item2.definition.is_crate)
			{
				num = 0;
				num2 = 0;
				break;
			}
			num -= ((!item2.definition.is_big) ? 1 : 4);
			if (!(is_big || is_crate) && item2.id == item.id)
			{
				num2 += stack_count - item2.value;
			}
		}
		if (is_big)
		{
			return num / 4;
		}
		if (is_crate)
		{
			if (num != 10)
			{
				return 0;
			}
			return 1;
		}
		return num * stack_count + num2;
	}

	private bool CanCarryItem(Item item)
	{
		if (!_is_correctly_inited)
		{
			Debug.LogError("CanCarryItem error: porter_station is not inited!");
			return false;
		}
		if (item == null || item.IsEmpty())
		{
			return false;
		}
		if (_source == null)
		{
			Debug.LogError("CanCarryItem error: _source WorldZone is null!");
			return false;
		}
		bool flag = false;
		foreach (TransportPathsDefinition transport_path in GameBalance.me.transport_paths)
		{
			if (!(transport_path.source_zone_id != _source.id) && !(transport_path.destination_zone_id != destination.id) && !(transport_path.station_wgo_id != _wgo.obj_id) && transport_path.transport_items.Contains(item.id))
			{
				flag = true;
			}
		}
		if (!flag)
		{
			return false;
		}
		if (blacklist.Contains(item.id))
		{
			return false;
		}
		return true;
	}

	public void OnCameToDestination()
	{
		if (destination == null)
		{
			Debug.LogError("Destination is null!");
			return;
		}
		if (source == null)
		{
			Debug.LogError("Source is null!");
			return;
		}
		Debug.Log("Porter " + _wgo.linked_worker.obj_id + " is came to destination " + _destination.id);
		Item backpack = _wgo.linked_worker.worker.GetBackpack();
		if (backpack == null)
		{
			return;
		}
		_destination.PutToAllPossibleInventoriesSmart(backpack.inventory, out var _);
		GDPoint gDPointByGDTag = WorldMap.GetGDPointByGDTag(_source.id + "_d");
		if (gDPointByGDTag == null)
		{
			Debug.LogError("OnCameToDestination error: not found destination_point by tag \"" + _source.id + "_d\"!");
			return;
		}
		BaseCharacterComponent character = _wgo.linked_worker.components.character;
		if (character == null)
		{
			Debug.LogError("OnCameToDestination error: linked_worker has no BaseCharacterComponent!");
			return;
		}
		character.GoTo(gDPointByGDTag.gameObject, snap_to_node: false, null, null, with_cinematic: false, MovementComponent.GoToMethod.GDGraph, "porter_on_came_to_source", null, from_script: false, gDPointByGDTag);
		character.SetSpeed(1.5f);
		_wgo.linked_worker.worker.UpdateWorkerSkin(Worker.WorkerActivity.Porter);
		ChunkedGameObject component = _wgo.linked_worker.GetComponent<ChunkedGameObject>();
		if (component != null)
		{
			component.active_now_because_of_movement = true;
		}
		state = PorterState.GoingToSource;
	}

	public void OnCameToSource()
	{
		WorldGameObject linked_worker = _wgo.linked_worker;
		ChunkedGameObject component = linked_worker.GetComponent<ChunkedGameObject>();
		if (component != null)
		{
			component.active_now_because_of_movement = false;
		}
		if (source == null)
		{
			Debug.LogError("Source on WGO name=\"" + _wgo?.name + "\" with obj_id=\"" + _wgo?.obj_id + "\" is null!");
		}
		Item backpack = linked_worker.worker.GetBackpack();
		List<Item> list = new List<Item>();
		for (int i = 0; i < backpack.inventory.Count; i++)
		{
			if (!CanCarryItem(backpack.inventory[i]))
			{
				list.Add(new Item(backpack.inventory[i]));
				backpack.inventory.RemoveAt(i);
				i--;
			}
		}
		if (list.Count > 0)
		{
			source.PutToAllPossibleInventoriesSmart(list, out var cant_insert);
			if (cant_insert != null && cant_insert.Count > 0)
			{
				linked_worker.DropItems(cant_insert);
			}
		}
		if (!TrySendPorter())
		{
			state = PorterState.Waiting;
			_wgo.linked_worker.worker.UpdateWorkerSkin(Worker.WorkerActivity.None);
			if (waiting_point == null)
			{
				RefindWaitingPoint();
			}
			if (waiting_point != null)
			{
				linked_worker.transform.position = waiting_point.transform.position;
				linked_worker.RefreshPositionCache();
				linked_worker.gameObject.SetActive(value: true);
				linked_worker.components.character.LookAt(waiting_point.action_dir);
				linked_worker.DrawPuffFX();
			}
		}
	}

	private void RefindWaitingPoint()
	{
		DockPoint[] array = _wgo.RefindDockPointsAndGet();
		if (array != null && array.Length != 0)
		{
			waiting_point = array[0];
		}
	}

	public SerializableWGO.SerializeblePorterStation Serialize()
	{
		SerializableWGO.SerializeblePorterStation result = default(SerializableWGO.SerializeblePorterStation);
		result.state = state;
		result.items_black_list = _items_black_list;
		return result;
	}

	public void Deserialize(SerializableWGO.SerializeblePorterStation data)
	{
		Init();
		state = data.state;
		_items_black_list = data.items_black_list;
	}

	public static bool ShortWayFromSteepWorkyardToHomeIsAvailable()
	{
		Vector2 vector = new Vector2(2208f, 2112f);
		foreach (WorldGameObject item in WorldMap.GetWorldGameObjectsByObjId("steep_yellow_blockage"))
		{
			try
			{
				if (((Vector2)item.transform.position - vector).sqrMagnitude < 1f)
				{
					return false;
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("ShortWayFromSteepWorkyardToHomeIsAvailable exeption: " + ex);
			}
		}
		return true;
	}

	public static bool ShortWayFromCellarToTavernCellarIsAvailable()
	{
		Vector2 vector = new Vector2(10992f, -9840f);
		foreach (WorldGameObject item in WorldMap.GetWorldGameObjectsByObjId("blockage_H_high"))
		{
			try
			{
				if (((Vector2)item.transform.position - vector).sqrMagnitude < 1f)
				{
					return false;
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("ShortWayFromCellarToTavernCellarIsAvailable exeption: " + ex);
			}
		}
		return true;
	}
}
