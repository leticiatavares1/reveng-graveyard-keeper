using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemsDurabilityManager : MonoBehaviour
{
	private static List<WorldGameObject> _objs = null;

	private static List<Item> _drop_items = new List<Item>();

	private static bool _inited = false;

	private static float _delta_time = 0f;

	private static int _frames = 0;

	private bool _coroutine_is_active;

	private float _time_since_last_coroutine_start;

	private const int FRAMES_PERIOD = 10;

	private const int BREAK_WGOS_CYCLE_INTO_FRAMES = 4;

	private static ItemsDurabilityManager _me = null;

	public static void Init(List<WorldGameObject> wgos_list, List<Item> drops_list)
	{
		if (_me == null)
		{
			GameObject obj = new GameObject("Items Durability Manager");
			_me = obj.AddComponent<ItemsDurabilityManager>();
			Object.DontDestroyOnLoad(obj);
		}
		_objs = wgos_list;
		_drop_items = drops_list;
		_inited = true;
		_delta_time = 0f;
		_frames = 0;
	}

	public static void Stop()
	{
		_inited = false;
	}

	private void DoRecalc(float delta_time)
	{
		if (!_inited)
		{
			return;
		}
		Item item = null;
		if (MainGame.me.player != null)
		{
			item = MainGame.me.player.components.character.GetOverheadItem();
		}
		item?.UpdateDurability(delta_time);
		foreach (Item drop_item in _drop_items)
		{
			drop_item.UpdateDurability(delta_time);
		}
		if (!_coroutine_is_active)
		{
			if (_time_since_last_coroutine_start == 0f)
			{
				_time_since_last_coroutine_start = delta_time;
			}
			StartCoroutine(DoRecalcStep(_time_since_last_coroutine_start));
			_time_since_last_coroutine_start = 0f;
		}
		else
		{
			_time_since_last_coroutine_start += delta_time;
		}
		if (!(MainGame.me?.player != null))
		{
			return;
		}
		foreach (GameResAtom item2 in MainGame.me.player.data.GetParams().ToAtomList())
		{
			if (item2.type.StartsWith("_cooldown_"))
			{
				MainGame.me.player.data.SubFromParams(item2.type, delta_time);
			}
		}
	}

	private IEnumerator DoRecalcStep(float delta_time)
	{
		_coroutine_is_active = true;
		int cur_part = 0;
		int part_len = _objs.Count / 4;
		for (int i = 0; i <= _objs.Count; i++)
		{
			if (part_len > 0)
			{
				int num = Mathf.FloorToInt((float)i / (float)part_len);
				if (num > cur_part)
				{
					cur_part = num;
					yield return new WaitForEndOfFrame();
				}
			}
			if (i > _objs.Count)
			{
				break;
			}
			WorldGameObject worldGameObject;
			float parent_modificator;
			if (i == _objs.Count)
			{
				worldGameObject = MainGame.me.player;
				parent_modificator = 1f;
			}
			else
			{
				worldGameObject = _objs[i];
				if (worldGameObject.is_removed || worldGameObject.data.inventory.Count == 0)
				{
					continue;
				}
				parent_modificator = worldGameObject.obj_def.durability_modificator;
			}
			foreach (Item item in worldGameObject.data.inventory)
			{
				item.UpdateDurability(delta_time, parent_modificator);
			}
		}
		_coroutine_is_active = false;
	}

	public static void EveryFrameUpdate(float delta_time)
	{
		if (_inited)
		{
			_frames++;
			_delta_time += delta_time;
			if (_frames >= 10)
			{
				_me.DoRecalc(_delta_time);
				_frames = 0;
				_delta_time = 0f;
			}
		}
	}
}
