using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayersTavernEngine
{
	[Serializable]
	public class GDPointLock
	{
		public int num;

		public long locker_unique_id;

		private WorldGameObject _locker;

		public WorldGameObject locker
		{
			get
			{
				if (locker_unique_id == -1)
				{
					return null;
				}
				if (_locker == null)
				{
					_locker = WorldMap.GetWorldGameObjectByUniqueId(locker_unique_id);
				}
				return _locker;
			}
			set
			{
				if (value == null)
				{
					_locker = null;
					locker_unique_id = -1L;
					Debug.Log($"Tavern Engine: unlocked GDPoint num={num}");
				}
				else
				{
					locker_unique_id = value.unique_id;
					Debug.Log($"Tavern Engine: locked GDPoint num={num}, locker_id={locker_unique_id}");
					_locker = value;
				}
			}
		}

		public bool is_locked => locker_unique_id > 0;
	}

	public const string IDLE_POINT_PREFIX = "players_tavern_idle_";

	public const string FORCE_SHUFFLE_FLAG = "force_tavern_shuffle";

	public const string TAVERN_VISITOR_TAG = "npc_event_visitor";

	public const string DO_ROLL_ANIIM = "do_roll_anim";

	public string[] TAVERN_VISITORS = new string[5] { "npc_tavern_visitor_1", "npc_tavern_visitor_2", "npc_tavern_visitor_3", "npc_tavern_visitor_4", "npc_tavern_visitor_5" };

	public string[] ITEMS_SELLING_IN_TAVERN = new string[10] { "cup_beer:1", "cup_beer:2", "cup_beer:3", "cup_mead:1", "cup_mead:2", "cup_mead:3", "bottle_red_vine:1", "bottle_red_vine:2", "bottle_red_vine:3", "bottle_booz_xxx" };

	public const float TAVERN_SELLING_COEFF = 1f;

	public const float MONEY_TO_REPUTATION = 0.01f;

	public const float VISITORS_RATIO = 0.6f;

	[SerializeField]
	private List<int> available_idle_points = new List<int>();

	[SerializeField]
	private List<long> visitors_unique_ids = new List<long>();

	[NonSerialized]
	public List<WorldGameObject> visitors = new List<WorldGameObject>();

	[SerializeField]
	public List<GDPointLock> locks = new List<GDPointLock>();

	[SerializeField]
	public bool visitors_temporarily_removed;

	public void Init()
	{
		if (available_idle_points == null)
		{
			Debug.LogError("PlayersTavernEngine error: available_idle_points list was null! Call Bulat.");
			available_idle_points = new List<int>();
		}
		if (visitors_unique_ids == null)
		{
			Debug.LogError("PlayersTavernEngine error: visitors_unique_ids list was null! Call Bulat.");
			visitors_unique_ids = new List<long>();
		}
		FillVisitorsList();
		if (locks == null)
		{
			Debug.LogError("PlayersTavernEngine error: locks was null! Call Bulat.");
			locks = new List<GDPointLock>();
			foreach (int available_idle_point in available_idle_points)
			{
				locks.Add(new GDPointLock
				{
					num = available_idle_point,
					locker_unique_id = -1L
				});
			}
		}
		visitors_temporarily_removed = false;
		Debug.Log($"Tavern Engine: initialized. available_idle_points={available_idle_points.Count}, visitors_unique_ids={visitors_unique_ids.Count}, locks={locks.Count}");
	}

	private void FillVisitorsList()
	{
		if (visitors_unique_ids == null)
		{
			Debug.LogError("FATAL ERROR: PlayersTavernEngine visitors_unique_ids list is null!");
			return;
		}
		visitors = new List<WorldGameObject>();
		foreach (long visitors_unique_id in visitors_unique_ids)
		{
			WorldGameObject worldGameObjectByUniqueId = WorldMap.GetWorldGameObjectByUniqueId(visitors_unique_id);
			if (worldGameObjectByUniqueId == null)
			{
				Debug.LogError("FATAL ERROR: not found visitor with unique_id = " + visitors_unique_id);
			}
			else
			{
				visitors.Add(worldGameObjectByUniqueId);
			}
		}
		Debug.Log($"Filled visitors list. Count={visitors.Count}");
	}

	public void AddNewVisitor(WorldGameObject visitor_wgo)
	{
		if (visitor_wgo == null || visitor_wgo.unique_id < 0)
		{
			Debug.LogError("AddNewVisitor error: " + ((visitor_wgo == null) ? "visitor is null!" : "visitor_wgo.unique_id < 0"));
			return;
		}
		visitors_unique_ids.Add(visitor_wgo.unique_id);
		visitors.Add(visitor_wgo);
		Debug.Log($"Tavern Engine: added new visitor: id=\"{visitor_wgo.obj_id}\", unique_id=\"{visitor_wgo.unique_id}\"");
	}

	public void RemoveVisitor(WorldGameObject visitor_wgo)
	{
		if (visitor_wgo == null || visitor_wgo.unique_id < 0)
		{
			Debug.LogError("RemoveVisitor error: " + ((visitor_wgo == null) ? "visitor is null!" : "visitor_wgo.unique_id < 0"));
			return;
		}
		if (!visitors_unique_ids.Contains(visitor_wgo.unique_id))
		{
			Debug.LogError($"RemoveVisitor error: not found visitor unique id \"{visitor_wgo.unique_id}\"");
		}
		else
		{
			visitors_unique_ids.Remove(visitor_wgo.unique_id);
		}
		if (!visitors.Contains(visitor_wgo))
		{
			bool flag = false;
			for (int i = 0; i < visitors.Count; i++)
			{
				if (visitors[i].unique_id == visitor_wgo.unique_id)
				{
					visitors.RemoveAt(i);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				Debug.LogError($"RemoveVisitor error: not found visitor \"{visitor_wgo.obj_id}\" with unique_id=\"{visitor_wgo.unique_id}\" in WGOs list. Doing rescan.");
				FillVisitorsList();
			}
		}
		else
		{
			visitors.Remove(visitor_wgo);
		}
		bool flag2 = false;
		foreach (GDPointLock @lock in locks)
		{
			if (@lock.locker_unique_id == visitor_wgo.unique_id)
			{
				@lock.locker_unique_id = -1L;
				Debug.Log($"Tavern Engine: unlocked GDPoint num={@lock.num}");
				flag2 = true;
				break;
			}
		}
		if (!flag2)
		{
			Debug.LogError($"RemoveVisitor error: not removed lock for visitor \"{visitor_wgo.obj_id}\", unique_id=\"{visitor_wgo.unique_id}\"");
		}
		else
		{
			Debug.Log($"Successfully removed visitor id=\"{visitor_wgo.obj_id}\" with unique_id=\"{visitor_wgo.unique_id}\"");
		}
	}

	public List<int> GetAvailablePointsList()
	{
		return available_idle_points;
	}

	public void AddGDPoint(GDPoint gd_point)
	{
		if (gd_point == null)
		{
			Debug.LogError("AddGDPoint error: gd_point is null!");
			return;
		}
		string[] array = gd_point.gd_tag.Split(new char[1] { '_' }, StringSplitOptions.RemoveEmptyEntries);
		if (!int.TryParse(array[array.Length - 1], out var result))
		{
			Debug.LogError("AddGDPoint error: unknown point num gd_point=\"" + gd_point.gd_tag + "\", num=\"" + array[array.Length - 1] + "\"");
		}
		else if (available_idle_points.Contains(result))
		{
			Debug.LogError($"TavernEngine.AddGDPoint error: trying to add point that already exist: \"{gd_point.gd_tag}\", num=\"{result}\"");
		}
		else
		{
			available_idle_points.Add(result);
			locks.Add(new GDPointLock
			{
				num = result,
				locker_unique_id = -1L
			});
			Debug.Log($"TavernEngine: added GDPoint \"{gd_point.gd_tag}\", num=\"{result}\"");
		}
	}

	public bool TryGetAvailablePoint(out GDPoint out_point, long lock_by = -1L)
	{
		out_point = null;
		List<GDPointLock> list = new List<GDPointLock>();
		foreach (GDPointLock @lock in locks)
		{
			if (!@lock.is_locked)
			{
				list.Add(@lock);
			}
		}
		if (list.Count == 0)
		{
			return false;
		}
		GDPointLock gDPointLock = ((list.Count == 1) ? list[0] : list[UnityEngine.Random.Range(0, list.Count)]);
		out_point = WorldMap.GetGDPointByGDTag("players_tavern_idle_" + gDPointLock.num);
		if (out_point == null)
		{
			return false;
		}
		if (lock_by > 0)
		{
			foreach (GDPointLock lock2 in locks)
			{
				if (lock2.locker_unique_id == lock_by)
				{
					lock2.locker_unique_id = -1L;
					Debug.Log($"Tavern Engine: unlocked GDPoint num={lock2.num}");
				}
			}
			gDPointLock.locker_unique_id = lock_by;
			Debug.Log($"Tavern Engine: locked GDPoint num={gDPointLock.num}, locker_id={gDPointLock.locker_unique_id}");
		}
		return true;
	}

	public void TemporarilyRemoveVisitors()
	{
		visitors_temporarily_removed = true;
		GDPoint gDPointByGDTag = WorldMap.GetGDPointByGDTag("default_destroy_point");
		if (gDPointByGDTag == null)
		{
			Debug.LogError("TemporarilyRemoveVisitors error: stock)point not found!");
			return;
		}
		Vector2 vector = gDPointByGDTag.transform.position;
		foreach (WorldGameObject visitor in visitors)
		{
			visitor.transform.position = vector;
			if (!visitor.obj_def.IsCharacter())
			{
				Debug.LogError("WGO is not character!");
				continue;
			}
			BaseCharacterComponent character = visitor.components.character;
			if (character == null)
			{
				Debug.LogError("BaseCharacterComponent is null!");
			}
			else
			{
				character.StopMovement();
			}
		}
		Debug.Log($"Tavern Engine: temporarily removed visitors. Count={visitors.Count}");
	}

	public void PlaceVisitorsBackAfterEvent()
	{
		int num = 0;
		foreach (GDPointLock @lock in locks)
		{
			if (@lock.is_locked)
			{
				GDPoint gDPointByGDTag = WorldMap.GetGDPointByGDTag("players_tavern_idle_" + @lock.num);
				if (gDPointByGDTag == null)
				{
					Debug.LogError(string.Format("FATAL ERROR: PlaceVisitorsBackAfterEvent error: not found GDPoint with tag \"{0}{1}\"!", "players_tavern_idle_", @lock));
					continue;
				}
				@lock.locker.transform.position = gDPointByGDTag.transform.position;
				@lock.locker.RefreshPositionCache();
				@lock.locker.gameObject.SetActive(value: true);
				@lock.locker.OnCameToGDPoint(gDPointByGDTag);
				num++;
			}
		}
		visitors_temporarily_removed = false;
		Debug.Log($"Tavern Engine: placed back visitors after event. Count={num}");
	}

	public List<string> GetAvailableIdlePoints(TavernEventDefinition event_def)
	{
		List<string> list = new List<string>();
		List<int> availablePointsList = GetAvailablePointsList();
		if (availablePointsList == null || availablePointsList.Count == 0)
		{
			return new List<string>();
		}
		List<string> list2 = new List<string>();
		List<string> list3 = new List<string>();
		if (event_def != null)
		{
			list2.AddRange(event_def.idle_points_blacklist);
			list3.AddRange(event_def.idle_points_whitelist);
		}
		foreach (int item2 in availablePointsList)
		{
			string item = "players_tavern_idle_" + item2;
			if (!list2.Contains(item))
			{
				list.Add(item);
			}
		}
		foreach (string item3 in list3)
		{
			if (!list.Contains(item3))
			{
				list.Remove(item3);
			}
		}
		list.InsertRange(0, list3);
		return list;
	}

	public static float CalculateCorrecterCoeff(float quality, float average_item_price)
	{
		float num = -3f / 38f * quality + 7.394737f;
		float num2 = quality * num * average_item_price;
		if ((double)num2 < 0.01)
		{
			num2 = 100f;
		}
		return 100f / num2;
	}

	public static float CalculateAlcoholSellingBonus(float money_earned)
	{
		float totalQuality = WorldZone.GetZoneByID("players_tavern").GetTotalQuality();
		float num = 0f;
		if (totalQuality > 30f && totalQuality <= 55f)
		{
			num = 0.1f;
		}
		else if (totalQuality > 55f)
		{
			num = 0.2f;
		}
		return num * money_earned;
	}
}
