using System.Collections.Generic;
using UnityEngine;

public class StoneStockpileCustomDrawer : MonoBehaviour
{
	public List<GameObject> stone_stages;

	public List<GameObject> marble_stages;

	public const string stone_items_name = "stone";

	public const string marble_items_name = "marble";

	public float update_period;

	private WorldGameObject _wobj;

	private bool _obj_is_set;

	private float _cur_update_time;

	public void Update()
	{
		if (!_obj_is_set)
		{
			_wobj = GetComponent<WorldObjectPart>()?.parent;
			_obj_is_set = _wobj != null;
		}
		if (_obj_is_set)
		{
			_cur_update_time += Time.deltaTime;
			if (_cur_update_time > update_period)
			{
				Redraw(_wobj);
				_cur_update_time = 0f;
			}
		}
	}

	public void Redraw(WorldGameObject wobj)
	{
		List<Item> list = new List<Item>(wobj.data.inventory);
		int can_insert_items_limit = wobj.obj_def.can_insert_items_limit;
		if (list != null)
		{
			int num = 0;
			for (int i = 0; i < can_insert_items_limit; i++)
			{
				if (num < list.Count)
				{
					string id = list[i].id;
					if (id == "stone")
					{
						stone_stages[i].SetActive(value: true);
						marble_stages[i].SetActive(value: false);
					}
					else if (id == "marble")
					{
						stone_stages[i].SetActive(value: false);
						marble_stages[i].SetActive(value: true);
					}
					num++;
				}
				else
				{
					stone_stages[i].SetActive(value: false);
					marble_stages[i].SetActive(value: false);
				}
			}
		}
		else
		{
			Debug.LogError("Inventory of object is null");
		}
	}

	public bool IsCorrectDrawer(out string err_mes)
	{
		err_mes = "";
		if (stone_stages == null || stone_stages.Count == 0)
		{
			err_mes += "Wrong stone_stages count'\n";
			return false;
		}
		if (marble_stages == null || marble_stages.Count == 0)
		{
			err_mes += "Wrong marble_stages count\n";
			return false;
		}
		if (stone_stages.Count != marble_stages.Count)
		{
			err_mes += "Stone and marble stages must be the same size\n";
			return false;
		}
		WorldGameObject componentInParent = GetComponentInParent<WorldGameObject>();
		if (componentInParent != null && componentInParent.obj_id == "mf_stones_1" && componentInParent.obj_def.can_insert_items_limit != stone_stages.Count)
		{
			err_mes += "Stages count must be same as WGOs max insert limit\n";
		}
		return string.IsNullOrEmpty(err_mes);
	}
}
