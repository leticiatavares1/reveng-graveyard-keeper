using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CustomDrawers
{
	private WorldGameObject _wobj;

	public const string GRAVE_PATH_PREFIX = "objects/grave parts/";

	private GardenCustomDrawer _garden_drawer;

	private StoneStockpileCustomDrawer _stone_stockpile_drawer;

	private List<WOPPrefabName> _wop_prefabs_names;

	private int grave_stage_decay => Mathf.FloorToInt(_wobj.GetDecayFactor() * 100f / 35f) + 1;

	public WorldGameObject wobj => _wobj;

	public void SetWobj(WorldGameObject obj)
	{
		_wobj = obj;
	}

	public void OnValidate()
	{
	}

	public void OnObjectRedraw(bool force_redraw = false)
	{
		_garden_drawer = null;
		_stone_stockpile_drawer = null;
		if (_wobj == null)
		{
			return;
		}
		if (_wobj.obj_def == null)
		{
			Debug.LogError("OnObjectRedraw: no obj definition for id = " + _wobj.obj_id);
			return;
		}
		string id = _wobj.obj_def.id;
		if (id != null)
		{
			switch (id)
			{
			case "autopsi_table":
				OnDrawAutopsy();
				break;
			case "grave_empty":
			case "grave_ground":
				OnDrawGrave(force_redraw);
				break;
			case "working_table":
				OnDrawSawmill();
				break;
			case "garden_wheat":
			case "garden_cannabis":
			case "garden_cabbage":
			case "garden_carrot":
			case "garden_beet":
			case "garden_grapes":
			case "garden_lentils":
			case "garden_pumpkin":
			case "garden_onion":
			case "garden_hop":
			case "bush_berry_garden":
			case "tree_apple_garden_empty":
			case "tree_apple_garden":
			case "zombie_garden_desk_wheat":
			case "zombie_garden_desk_cabbage":
			case "zombie_garden_desk_carrot":
			case "zombie_garden_desk_beet":
			case "zombie_garden_desk_lentils":
			case "zombie_garden_desk_pumpkin":
			case "zombie_garden_desk_onion":
			case "zombie_garden_desk_0":
			case "zombie_garden_desk_1":
			case "zombie_garden_desk_2":
			case "zombie_vineyard_desk_0":
			case "zombie_vineyard_desk_1":
			case "zombie_vineyard_desk_2":
			case "refugee_camp_garden_bed_1":
			case "refugee_camp_garden_bed_2":
			case "refugee_camp_garden_bed_3":
			case "refugee_garden_desk_wheat":
			case "refugee_garden_desk_cabbage":
			case "refugee_garden_desk_carrot":
			case "refugee_garden_desk_beet":
			case "refugee_garden_desk_lentils":
			case "refugee_garden_desk_pumpkin":
			case "refugee_garden_desk_onion":
				OnDrawGarden();
				break;
			case "mf_stones_1":
				OnDrawStoneStockpile();
				break;
			}
		}
	}

	private void OnDrawAutopsy()
	{
		foreach (WorldObjectPart additional_wop in _wobj.additional_wops)
		{
			if (!(additional_wop == null))
			{
				GJCommons.Destroy(additional_wop.gameObject);
			}
		}
		_wobj.additional_wops.Clear();
		foreach (Item item in _wobj.data.inventory)
		{
			if (item.definition.type == ItemDefinition.ItemType.Body)
			{
				WorldObjectPart o = null;
				_wobj.RedrawPart(ref o, "body_autopsi", "objects/grave parts/", 0f);
				if (!(o == null))
				{
					_wobj.additional_wops.Add(o);
				}
			}
		}
	}

	public bool NeedRedrawGrave()
	{
		if (_wobj == null)
		{
			return false;
		}
		if (_wobj.obj_def == null)
		{
			return false;
		}
		if (_wobj.obj_def.id != "grave_ground")
		{
			return false;
		}
		if (_wop_prefabs_names == null)
		{
			_wop_prefabs_names = new List<WOPPrefabName>();
		}
		List<WOPPrefabName> newWOPPrefabsNames = GetNewWOPPrefabsNames();
		if (_wop_prefabs_names.Count != newWOPPrefabsNames.Count)
		{
			_wop_prefabs_names = newWOPPrefabsNames;
			return true;
		}
		for (int i = 0; i < _wop_prefabs_names.Count; i++)
		{
			if (_wop_prefabs_names[i] == null)
			{
				_wop_prefabs_names = newWOPPrefabsNames;
				return true;
			}
			if (!_wop_prefabs_names[i].EqualsTo(newWOPPrefabsNames[i]))
			{
				_wop_prefabs_names = newWOPPrefabsNames;
				return true;
			}
		}
		_wop_prefabs_names = newWOPPrefabsNames;
		return false;
	}

	private List<WOPPrefabName> GetNewWOPPrefabsNames()
	{
		List<WOPPrefabName> list = new List<WOPPrefabName>();
		List<Item> list2 = new List<Item>();
		foreach (Item item in _wobj.data.inventory)
		{
			if (item.definition == null)
			{
				Debug.LogError("WGO has an item without a definition, id = " + item.id, _wobj);
				list2.Add(item);
			}
			else
			{
				if (item.definition.type != ItemDefinition.ItemType.GraveCover && item.definition.type != ItemDefinition.ItemType.GraveFence && item.definition.type != ItemDefinition.ItemType.GraveStone)
				{
					continue;
				}
				if (_wobj.GetParam(item.id) < 1f)
				{
					switch (item.definition.type)
					{
					case ItemDefinition.ItemType.GraveFence:
						list.Add(new WOPPrefabName
						{
							part_1 = "grave_bot_building_1_stg_1"
						});
						break;
					case ItemDefinition.ItemType.GraveStone:
						list.Add(new WOPPrefabName
						{
							part_1 = "grave_top_building_1_stg_1"
						});
						break;
					}
					continue;
				}
				int num = grave_stage_decay;
				num = 4 - Mathf.CeilToInt(item.durability * 3f);
				if (num > 3)
				{
					num = 3;
				}
				WOPPrefabName wOPPrefabName = new WOPPrefabName
				{
					part_1 = item.id + "_stg_",
					stage = num,
					part_2 = "",
					need_herb = false
				};
				if (_wobj.GetParamInt("vegetation") != 0)
				{
					wOPPrefabName.need_herb = true;
				}
				list.Add(wOPPrefabName);
			}
		}
		foreach (Item item2 in list2)
		{
			_wobj.data.inventory.Remove(item2);
		}
		return list;
	}

	public void OnDrawGrave(bool forced = false)
	{
		if (_wobj.obj_def.id != "grave_ground")
		{
			return;
		}
		ForceNonNegativeParamValue("decay");
		ForceNonNegativeParamValue("vegetation");
		if (forced)
		{
			_wop_prefabs_names = GetNewWOPPrefabsNames();
		}
		else if (!NeedRedrawGrave())
		{
			return;
		}
		RemoveAllWOPs();
		foreach (WOPPrefabName wop_prefabs_name in _wop_prefabs_names)
		{
			WorldObjectPart o = null;
			if (wop_prefabs_name.stage <= 0)
			{
				_wobj.RedrawPart(ref o, wop_prefabs_name.GetName(), "objects/grave parts/", 0f);
			}
			else
			{
				while (wop_prefabs_name.stage > 0)
				{
					_wobj.RedrawPart(ref o, wop_prefabs_name.GetName(), "objects/grave parts/", 0f);
					if (!(o == null))
					{
						break;
					}
					wop_prefabs_name.stage--;
				}
			}
			if (!(o != null))
			{
				continue;
			}
			_wobj.additional_wops.Add(o);
			if (wop_prefabs_name.need_herb)
			{
				WorldObjectPart o2 = null;
				_wobj.RedrawPart(ref o2, wop_prefabs_name.GetName() + "_herb", "objects/grave parts/", 0f);
				if (o2 != null)
				{
					_wobj.additional_wops.Add(o2);
				}
			}
		}
	}

	private void RemoveAllWOPs()
	{
		if (_wobj == null || _wobj.additional_wops == null || _wobj.additional_wops.Count == 0)
		{
			return;
		}
		foreach (WorldObjectPart additional_wop in _wobj.additional_wops)
		{
			if (!(additional_wop == null))
			{
				GJCommons.Destroy(additional_wop.gameObject);
			}
		}
		_wobj.additional_wops.Clear();
	}

	private void ForceNonNegativeParamValue(string param_name)
	{
		if (_wobj.GetParamInt(param_name) < 0)
		{
			_wobj.SetParam(param_name, 0f);
		}
	}

	public void OnDrawSawmill()
	{
	}

	public void OnDrawGarden()
	{
		WorldObjectPart[] componentsInChildren = _wobj.GetComponentsInChildren<WorldObjectPart>();
		WorldObjectPart worldObjectPart = null;
		if (componentsInChildren.Length == 1)
		{
			worldObjectPart = componentsInChildren[0];
		}
		else
		{
			WorldObjectPart[] array = componentsInChildren;
			foreach (WorldObjectPart worldObjectPart2 in array)
			{
				if (!(worldObjectPart2 == null) && worldObjectPart2.enabled && (worldObjectPart2.name.Contains("garden") || worldObjectPart2.name.Contains("vineyard")))
				{
					worldObjectPart = worldObjectPart2;
					break;
				}
			}
		}
		if (worldObjectPart == null)
		{
			Debug.LogError("OnDrawGarden: WOP to draw is null!");
			return;
		}
		if (_garden_drawer == null)
		{
			_garden_drawer = worldObjectPart.GetComponentInChildren<GardenCustomDrawer>();
			if (_garden_drawer == null)
			{
				Debug.LogError("GardenCastomDrawer is null!");
				return;
			}
		}
		_garden_drawer.Redraw(_wobj);
	}

	public void FastRedraw()
	{
		if (!(_wobj == null) && _garden_drawer != null)
		{
			_garden_drawer.Redraw(_wobj);
		}
	}

	public void OnDrawStoneStockpile()
	{
		WorldObjectPart[] componentsInChildren = _wobj.GetComponentsInChildren<WorldObjectPart>();
		WorldObjectPart worldObjectPart = null;
		if (componentsInChildren.Length == 1)
		{
			worldObjectPart = componentsInChildren[0];
		}
		else
		{
			WorldObjectPart[] array = componentsInChildren;
			foreach (WorldObjectPart worldObjectPart2 in array)
			{
				if (!(worldObjectPart2 == null) && worldObjectPart2.enabled)
				{
					worldObjectPart = worldObjectPart2;
					break;
				}
			}
		}
		if (worldObjectPart == null)
		{
			Debug.LogError("OnDrawStoneStockpile: WOP to draw is null!");
			return;
		}
		if (_stone_stockpile_drawer == null)
		{
			_stone_stockpile_drawer = worldObjectPart.GetComponentInChildren<StoneStockpileCustomDrawer>();
			if (_stone_stockpile_drawer == null)
			{
				Debug.LogError("StoneStockpileCustomDrawer is null!");
				return;
			}
		}
		_stone_stockpile_drawer.Redraw(_wobj);
	}
}
