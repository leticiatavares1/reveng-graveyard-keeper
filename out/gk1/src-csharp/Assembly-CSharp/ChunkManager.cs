using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
	private static ChunkManager _me = null;

	private static bool _inited = false;

	private static List<ChunkedGameObject> _objs = new List<ChunkedGameObject>();

	private static HashSet<int> _objs_ids = new HashSet<int>();

	private static List<ChunkedGameObject> _objs_to_add = new List<ChunkedGameObject>();

	private static List<ChunkedGameObject> _objs_to_remove = new List<ChunkedGameObject>();

	[Range(1f, 30f)]
	public int visible_chunk_radius_x = 18;

	[Range(1f, 30f)]
	public int visible_chunk_radius_y = 12;

	private const int EXTRA_VISIBLE_CHUNKS = 1;

	private const float SCREEN_TO_CHUNK_KOEFF = 160f;

	private static Thread _thread = null;

	private static List<ChunkedGameObject> _changed_objs = new List<ChunkedGameObject>();

	private bool _thread_calculating;

	private bool _thread_done_calculating;

	private bool _thread_ready_to_start;

	private static Vector3 _camera_pos = Vector3.zero;

	private bool _need_astar_recalc;

	private Bounds _astar_recalc_bounds;

	private bool _is_destroy_temp_mode;

	private bool _pending_temp_remove;

	public static void Init()
	{
		if (Application.isPlaying)
		{
			_me = SingletonGameObjects.FindOrCreate<ChunkManager>();
			_inited = true;
			RecalculateResolution();
			_thread = new Thread(_me.UpdateThread);
			_thread.Start();
		}
	}

	public static void RecalculateResolution(int w = -1, int h = -1)
	{
		if (_inited)
		{
			if (w == -1)
			{
				w = Screen.width;
			}
			if (h == -1)
			{
				h = Screen.height;
			}
			_me.visible_chunk_radius_x = Mathf.CeilToInt((float)w / 160f) + 1;
			_me.visible_chunk_radius_y = Mathf.CeilToInt((float)h / 160f) + 1;
		}
	}

	public static void ClearChunksList()
	{
		if (_thread != null && _thread.IsAlive)
		{
			_thread.Abort();
		}
		_objs.Clear();
		_objs_ids.Clear();
		_objs_to_add.Clear();
		_objs_to_remove.Clear();
		_thread = new Thread(_me.UpdateThread);
		_thread.Start();
	}

	public static void OnAddNewObject(ChunkedGameObject go)
	{
		if (go == null)
		{
			return;
		}
		ChunkedGameObject[] componentsInChildren = go.GetComponentsInChildren<ChunkedGameObject>();
		foreach (ChunkedGameObject chunkedGameObject in componentsInChildren)
		{
			if (chunkedGameObject.gameObject != go.gameObject)
			{
				return;
			}
			chunkedGameObject.Init();
		}
		_objs_to_add.Add(go);
	}

	public static void OnDestroyObject(WorldGameObject wgo)
	{
		if (!wgo.is_removed)
		{
			ChunkedGameObject componentInChildren = wgo.GetComponentInChildren<ChunkedGameObject>();
			if (componentInChildren != null)
			{
				OnDestroyObject(componentInChildren);
				componentInChildren.destroyed = true;
			}
		}
	}

	public static void OnDestroyObject(ChunkedGameObject go)
	{
		if (go == null)
		{
			return;
		}
		ChunkedGameObject[] componentsInChildren = go.GetComponentsInChildren<ChunkedGameObject>();
		foreach (ChunkedGameObject chunkedGameObject in componentsInChildren)
		{
			if (chunkedGameObject.is_temp)
			{
				chunkedGameObject.pending_to_remove = true;
			}
			else if (_objs_to_add.Contains(chunkedGameObject))
			{
				_objs_to_add.Remove(chunkedGameObject);
			}
			else
			{
				_objs_to_remove.Add(chunkedGameObject);
			}
		}
	}

	private void ProcessRemove()
	{
		foreach (ChunkedGameObject item in _objs_to_remove)
		{
			int num = item.instance_id;
			if (num == -1)
			{
				if (MainGame.game_started)
				{
					Debug.LogWarning("Strange. Trying to remove an object without iid", item);
				}
				try
				{
					num = (item.instance_id = item.gameObject.GetInstanceID());
				}
				catch (Exception)
				{
				}
			}
			if (_objs_ids.Contains(num))
			{
				_objs.Remove(item);
				_objs_ids.Remove(num);
			}
		}
		_objs_to_remove.Clear();
		if (_pending_temp_remove)
		{
			for (int num2 = _objs.Count - 1; num2 >= 0; num2--)
			{
				ChunkedGameObject chunkedGameObject = _objs[num2];
				if (chunkedGameObject.is_temp && chunkedGameObject.pending_to_remove)
				{
					_objs.RemoveAt(num2);
					int num3 = chunkedGameObject.instance_id;
					if (num3 == -1)
					{
						try
						{
							num3 = (chunkedGameObject.instance_id = chunkedGameObject.gameObject.GetInstanceID());
						}
						catch (Exception)
						{
						}
					}
					_objs_ids.Remove(num3);
				}
			}
		}
		_pending_temp_remove = false;
	}

	public void Update()
	{
		if (MainGame.me.player == null || !Application.isPlaying)
		{
			return;
		}
		_camera_pos = MainGame.me.transform.position;
		ProcessRemove();
		foreach (ChunkedGameObject item in _objs_to_add)
		{
			int num = item.instance_id;
			if (num == -1)
			{
				num = (item.instance_id = item.gameObject.GetInstanceID());
			}
			if (!_objs_ids.Contains(num))
			{
				_objs.Add(item);
				_objs_ids.Add(num);
			}
		}
		_objs_to_add.Clear();
		if (!_thread_calculating)
		{
			if (_thread_done_calculating)
			{
				UpdateObjectsVisibility();
			}
			StartThread();
		}
		if (_need_astar_recalc)
		{
			_need_astar_recalc = false;
			AStarTools.UpdateAstarBounds(_astar_recalc_bounds);
		}
	}

	public void LateUpdate()
	{
		if (_thread_done_calculating)
		{
			UpdateObjectsVisibility();
		}
	}

	private void UpdateObjectsVisibility()
	{
		int count = _changed_objs.Count;
		if (count != 0)
		{
			ProcessRemove();
			for (int i = 0; i < count; i++)
			{
				_changed_objs[i].UpdateVisibility();
			}
			_changed_objs.Clear();
			_thread_done_calculating = false;
		}
	}

	private void StartThread()
	{
		if (!_thread_calculating)
		{
			_thread_ready_to_start = true;
			_thread_done_calculating = false;
			_thread_calculating = true;
		}
	}

	private void UpdateThread()
	{
		while (true)
		{
			if (!_thread_ready_to_start)
			{
				Thread.Sleep(1);
				continue;
			}
			UpdateThreadFunction();
			_thread_done_calculating = true;
			_thread_ready_to_start = false;
			_thread_calculating = false;
		}
	}

	private void UpdateThreadFunction()
	{
		Vector2 vector = _camera_pos / 96f;
		int num = Mathf.RoundToInt(vector.x);
		int num2 = Mathf.RoundToInt(vector.y);
		for (int i = 0; i < _objs.Count; i++)
		{
			ChunkedGameObject chunkedGameObject = _objs[i];
			if (chunkedGameObject.can_go_inactive)
			{
				chunkedGameObject.out_x_1 = chunkedGameObject.chunk_x_max - num;
				chunkedGameObject.out_x_2 = chunkedGameObject.chunk_x_min - num;
				chunkedGameObject.out_y_1 = chunkedGameObject.chunk_y_max - num2;
				chunkedGameObject.out_y_2 = chunkedGameObject.chunk_y_min - num2;
				bool flag = chunkedGameObject.out_x_1 > (float)(-visible_chunk_radius_x) && chunkedGameObject.out_x_2 < (float)visible_chunk_radius_x && chunkedGameObject.out_y_1 > (float)(-visible_chunk_radius_y) && chunkedGameObject.out_y_2 < (float)visible_chunk_radius_y;
				if (chunkedGameObject.obj_visible != flag)
				{
					chunkedGameObject.obj_visible = flag;
					_changed_objs.Add(chunkedGameObject);
				}
			}
		}
	}

	public static void RescanAllObjects()
	{
		Debug.Log("RescanAllObjects");
		if (_me == null)
		{
			Init();
		}
		ClearChunksList();
		ChunkedGameObject[] componentsInChildren = MainGame.me.world_root.GetComponentsInChildren<ChunkedGameObject>(includeInactive: true);
		int num = 0;
		ChunkedGameObject[] array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			OnAddNewObject(array[i]);
			num++;
		}
		Debug.Log("RescanAllObjects: added objects = " + num);
	}

	public void OnDestroy()
	{
		if (_thread != null && _thread.IsAlive)
		{
			_thread.Abort();
		}
	}

	public static void RecalcAStarBounds(Bounds b)
	{
		if (!_me._need_astar_recalc)
		{
			_me._need_astar_recalc = true;
			_me._astar_recalc_bounds = b;
		}
		else
		{
			_me._astar_recalc_bounds.Encapsulate(b);
		}
	}

	public static void RemovePedingTempObjects()
	{
		_me._pending_temp_remove = true;
	}
}
