using System;
using UnityEngine;

[Serializable]
public class Worker
{
	public enum WorkerActivity
	{
		None,
		Worker,
		Porter
	}

	public enum WorkerTransformationType
	{
		FromOverheadToWGO,
		FromOverheadToOnGround,
		FromOnGroundToOverhead,
		FromOnGroundToWGO,
		FromWGOToOverhead,
		FromWGOToOnGround
	}

	public enum WorkerState
	{
		WGO,
		ItemOverhead,
		ItemOnGround
	}

	public const string WORKING_K = "working_k";

	public const string BACKPACK_ID = "porter_backpack";

	public const int BACKPACK_SIZE = 10;

	public const float WORKER_MOVEMENT_SPEED = 1.5f;

	public const string SKIN_CARRIER_BACKPACK_EMPTY = "zombie_1_backpack_empty";

	public const string SKIN_CARRIER_BACKPACK_HALF = "zombie_1_backpack_half";

	public const string SKIN_CARRIER_BACKPACK_FULL = "zombie_1_backpack_full";

	public const string SKIN_CARRIER_BOX = "zombie_1_box";

	public const string SKIN_ZOMBIE_WORKER = "zombie_1";

	public const string INVISIBLE_WORKER_ID = "worker_invisible";

	public const float DEFAULT_WORKER_K = 1f;

	public string id;

	public long worker_unique_id;

	private bool _force_worker_k;

	private float _forced_worker_k;

	private WorkerDefinition _definition;

	[SerializeField]
	private long _wgo_unique_id;

	private WorldGameObject _worker_wgo;

	public WorkerDefinition definition
	{
		get
		{
			if (_definition == null)
			{
				if (string.IsNullOrEmpty(id))
				{
					Debug.LogError("Can not get WorkerDefinition: Worker with empty id!");
					return null;
				}
				_definition = GameBalance.me.GetData<WorkerDefinition>(id);
			}
			return _definition;
		}
	}

	public WorldGameObject worker_wgo
	{
		get
		{
			if (_worker_wgo == null)
			{
				if (_wgo_unique_id <= 0)
				{
					Debug.LogError("Can not get worker_wgo: _wgo_unique_id=" + _wgo_unique_id);
					return null;
				}
				_worker_wgo = WorldMap.GetWorldGameObjectByUniqueId(_wgo_unique_id);
			}
			return _worker_wgo;
		}
	}

	public Worker()
	{
	}

	public Worker(WorldGameObject worker_wgo, long worker_unique_id)
	{
		_worker_wgo = worker_wgo;
		_wgo_unique_id = _worker_wgo.unique_id;
		this.worker_unique_id = worker_unique_id;
		_worker_wgo.worker_unique_id = worker_unique_id;
		Debug.Log($"Created new Worker with _wgo_unique_id={_wgo_unique_id}, worker_unique_id={this.worker_unique_id}");
	}

	public Item GetOnGroundItem()
	{
		if (string.IsNullOrEmpty(id))
		{
			Debug.LogError("GetOnGroundItem error: worker id is null!");
			return null;
		}
		string text = definition?.item_on_ground;
		if (string.IsNullOrEmpty(text))
		{
			Debug.LogError("GetOnGroundItem error: on_ground_item_name is null!");
			return null;
		}
		Item item = new Item(text);
		item.worker_unique_id = worker_unique_id;
		item.inventory_size = 99;
		foreach (Item item2 in worker_wgo.data.inventory)
		{
			if (!(item2.id == "porter_backpack"))
			{
				item.inventory.Add(item2);
			}
		}
		return item;
	}

	public Item GetOverheadItem()
	{
		if (string.IsNullOrEmpty(id))
		{
			Debug.LogError("GetOverheadItem error: worker id is null!");
			return null;
		}
		string text = definition?.item_overhead;
		if (string.IsNullOrEmpty(text))
		{
			Debug.LogError("GetOverheadItem error: overhead_item_name is null!");
			return null;
		}
		Item item = new Item(text);
		item.worker_unique_id = worker_unique_id;
		item.inventory_size = 99;
		foreach (Item item2 in worker_wgo.data.inventory)
		{
			if (!(item2.id == "porter_backpack"))
			{
				item.inventory.Add(item2);
			}
		}
		return item;
	}

	public void UpdateWorkerInventoryFromItem(Item worker_item)
	{
		if (worker_item == null)
		{
			Debug.LogError("UpdateWorkerInventoryFromItem: worker_item is NULL!");
			return;
		}
		if (!worker_item.is_worker)
		{
			Debug.LogError("UpdateWorkerInventoryFromItem: worker_item is NOT worker!");
			return;
		}
		if (worker_wgo == null)
		{
			Debug.LogError("UpdateWorkerInventoryFromItem: worker_wgo is NULL!");
			return;
		}
		Item item = null;
		foreach (Item item2 in worker_wgo.data.inventory)
		{
			if (item2.id == "porter_backpack")
			{
				item = item2;
				break;
			}
		}
		worker_wgo.data.inventory = worker_item.inventory;
		if (item == null)
		{
			GetBackpack();
		}
		else
		{
			worker_wgo.data.inventory.Add(item);
		}
		UpdateWorkerLevel();
	}

	public void UpdateWorkerLevel()
	{
		float value;
		if (_force_worker_k)
		{
			value = _forced_worker_k;
		}
		else
		{
			worker_wgo.data.GetBodySkulls(out var _, out var positive, out var _, dont_count_self: true);
			if (positive <= 0)
			{
				positive = 1;
			}
			value = (float)positive / 40f;
		}
		worker_wgo.data.SetParam("working_k", value);
	}

	public static string TransformWorker(WorkerState from_state, Item in_item, WorldGameObject in_wgo, WorkerState to_state, out Item out_item, out WorldGameObject out_wgo)
	{
		string empty = string.Empty;
		out_item = null;
		out_wgo = null;
		Worker worker = null;
		switch (from_state)
		{
		case WorkerState.WGO:
			if (in_wgo == null)
			{
				return empty + "TransformWorker error: in_zombie_wgo is null!";
			}
			worker = in_wgo.worker;
			break;
		case WorkerState.ItemOverhead:
		case WorkerState.ItemOnGround:
			if (in_item == null)
			{
				return empty + "TransformWorker error: input item is null!";
			}
			worker = in_item.worker;
			if (in_item != null)
			{
				worker.UpdateWorkerInventoryFromItem(in_item);
			}
			break;
		default:
			throw new ArgumentOutOfRangeException("from_state", from_state, null);
		}
		if (worker == null)
		{
			return empty + "TransformWorker error: Worker is null!";
		}
		switch (to_state)
		{
		case WorkerState.WGO:
			out_wgo = worker.worker_wgo;
			if (out_wgo == null)
			{
				return empty + "Failed worker transformation: worker_wgo is null!";
			}
			break;
		case WorkerState.ItemOverhead:
			out_item = worker.GetOverheadItem();
			if (out_item == null)
			{
				return empty + "Failed worker transformation: out_item is null!";
			}
			break;
		case WorkerState.ItemOnGround:
			out_item = worker.GetOnGroundItem();
			if (out_item == null)
			{
				return empty + "Failed worker transformation: out_item is null!";
			}
			break;
		default:
			throw new ArgumentOutOfRangeException("to_state", to_state, null);
		}
		return empty;
	}

	public Item GetBackpack()
	{
		if (string.IsNullOrEmpty(id))
		{
			Debug.LogError("GetBackpack error: worker id is null!");
			return null;
		}
		foreach (Item item2 in worker_wgo.data.inventory)
		{
			if (item2.id == "porter_backpack")
			{
				return item2;
			}
		}
		Item item = new Item("porter_backpack", 1)
		{
			inventory_size = 10
		};
		if (!worker_wgo.data.AddItem(item))
		{
			Debug.LogError("Error while adding backpack item to worker_wgo!");
		}
		foreach (Item item3 in worker_wgo.data.inventory)
		{
			if (item3.id == "porter_backpack")
			{
				return item3;
			}
		}
		Debug.LogError("FATAL ERROR: impossible shit happen with worker_wgo backpack! Call Bulat.");
		return null;
	}

	public void UpdateWorkerSkin(WorkerActivity worker_activity)
	{
		if (worker_wgo == null)
		{
			return;
		}
		WorldObjectPart worldObjectPart = _worker_wgo?.GetWOP();
		if (worldObjectPart == null)
		{
			Debug.LogError("UpdateWorkerSkin error: worker_wop is null!");
			return;
		}
		string skin_id = worldObjectPart.skin_id;
		string text = skin_id;
		if (_worker_wgo.components.character == null)
		{
			Debug.LogError("UpdateWorkerSkin error: worker_char is null!");
			return;
		}
		switch (worker_activity)
		{
		case WorkerActivity.None:
			text = "zombie_1";
			_worker_wgo.components.character.SetCarryingItem(null);
			break;
		case WorkerActivity.Worker:
			text = "zombie_1";
			_worker_wgo.components.character.SetCarryingItem(null);
			break;
		case WorkerActivity.Porter:
		{
			Item backpack = GetBackpack();
			if (backpack == null)
			{
				Debug.LogError("UpdateWorkerSkin error: backpack is null!");
				return;
			}
			int count = backpack.inventory.Count;
			if (count == 0)
			{
				text = "zombie_1_backpack_empty";
				_worker_wgo.components.character.SetCarryingItem(null);
			}
			else if (backpack.GetItemOfType(ItemDefinition.ItemType.Crate) != null)
			{
				text = "zombie_1_box";
				_worker_wgo.components.character.SetCarryingItem(backpack.GetItemOfType(ItemDefinition.ItemType.Crate));
			}
			else if (count >= 7)
			{
				text = "zombie_1_backpack_full";
				_worker_wgo.components.character.SetCarryingItem(null);
			}
			else
			{
				text = "zombie_1_backpack_half";
				_worker_wgo.components.character.SetCarryingItem(null);
			}
			break;
		}
		default:
			throw new ArgumentOutOfRangeException("worker_activity", worker_activity, null);
		}
		if (text != skin_id)
		{
			_worker_wgo.ApplySkin(text);
		}
	}

	public void ForcingWorkerK(bool do_force_working_k, float forced_working_k = 1f)
	{
		_force_worker_k = do_force_working_k;
		_forced_worker_k = forced_working_k;
	}

	public string GetWorkerEfficiencyText()
	{
		UpdateWorkerLevel();
		return GJL.L("work_effeciency", Mathf.RoundToInt(worker_wgo.data.GetParam("working_k") * 100f) + "%");
	}

	public string GetWorkerEfficiencyTextOnlyPercent()
	{
		UpdateWorkerLevel();
		return Mathf.RoundToInt(worker_wgo.data.GetParam("working_k") * 100f) + "%";
	}
}
