using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using LinqTools;
using UnityEngine;

public class WorldZone : MonoBehaviour
{
	public static Action<string, float> OnQualityComputed;

	public string id = "";

	private static HashSet<string> _recalculated_zones = new HashSet<string>();

	private List<WorldGameObject> _wgos = new List<WorldGameObject>();

	private List<WorldGameObject> _totems = new List<WorldGameObject>();

	private static List<WorldZone> _all_zones = new List<WorldZone>();

	private List<Collider2D> _colliders;

	private WorldZoneDefinition _definition;

	[NonSerialized]
	public GameObject can_be_removed_group;

	public Collider2D whole_zone_collider;

	[SerializeField]
	private Transform _custom_zone_center;

	public string ovr_music;

	public List<string> smart_sounds = new List<string>();

	private static List<WorldGameObject> _pre_refresh_wgos = null;

	public WorldZoneDefinition definition
	{
		get
		{
			if (_definition == null)
			{
				_definition = GameBalance.me.GetData<WorldZoneDefinition>(id);
			}
			return _definition;
		}
	}

	public Transform center_tf
	{
		get
		{
			if (_custom_zone_center == null)
			{
				return base.transform;
			}
			return _custom_zone_center;
		}
	}

	public bool IsAvailableForBuild()
	{
		return true;
	}

	public bool HasBuilder()
	{
		return GetZoneWGOs().Find((WorldGameObject w) => w.obj_def.interaction_type == ObjectDefinition.InteractionType.Builder) != null;
	}

	public static WorldZone GetZoneByID(string id, bool null_is_error = true)
	{
		foreach (WorldZone all_zone in _all_zones)
		{
			if (!(all_zone.id != id) && !all_zone.IsDisabled())
			{
				return all_zone;
			}
		}
		if (null_is_error)
		{
			Debug.LogError("Could't find zone [" + id + "]");
		}
		return null;
	}

	public static void InitZonesSystem()
	{
		_recalculated_zones.Clear();
		_all_zones = MainGame.me.world_root.GetComponentsInChildren<WorldZone>(includeInactive: true).ToList();
		Debug.Log("InitZonesSystem, zones = " + _all_zones.Count);
		foreach (WorldZone all_zone in _all_zones)
		{
			all_zone.Init();
		}
	}

	public static void RecalculateAllZones()
	{
		Debug.Log("RecalculateAllZones, zones = " + _all_zones.Count);
		foreach (WorldZone all_zone in _all_zones)
		{
			all_zone.Recalculate();
		}
	}

	public void PreRefreshZone()
	{
		Debug.Log("WorldZone.PreRefreshZone " + id, this);
		_pre_refresh_wgos = _wgos;
		_wgos = new List<WorldGameObject>();
	}

	public void PostRefreshZone()
	{
		Debug.Log("WorldZone.PostRefreshZone " + id, this);
		MarkZoneAsDirty(id);
		foreach (WorldGameObject pre_refresh_wgo in _pre_refresh_wgos)
		{
			pre_refresh_wgo.RecalculateZoneBelonging();
		}
		Recalculate();
	}

	private void Init()
	{
		_colliders = GetComponentsInChildren<Collider2D>(includeInactive: true).ToList();
		_wgos.Clear();
	}

	public static void MarkZoneAsDirty(string id)
	{
		if (!(GetZoneByID(id) == null) && _recalculated_zones.Contains(id))
		{
			_recalculated_zones.Remove(id);
		}
	}

	public void Update()
	{
		if (!_recalculated_zones.Contains(id))
		{
			RecalculateZone();
		}
	}

	private void RecalculateZone()
	{
		if (!_recalculated_zones.Contains(id))
		{
			_recalculated_zones.Add(id);
		}
	}

	private bool DoesObjectBelongToZone(WorldGameObject wgo)
	{
		Vector2 point = wgo.transform.position;
		foreach (Collider2D collider in _colliders)
		{
			if (collider == null)
			{
				Debug.Log("WorldZone \"" + base.gameObject.name + "\" has null collider!");
			}
			else
			{
				if (!collider.OverlapPoint(point))
				{
					continue;
				}
				if (!_wgos.Contains(wgo))
				{
					foreach (WorldZone all_zone in _all_zones)
					{
						if (all_zone._wgos.Contains(wgo))
						{
							all_zone._wgos.Remove(wgo);
						}
					}
					_wgos.Add(wgo);
				}
				return true;
			}
		}
		return false;
	}

	public static WorldZone GetZoneOfPoint(Vector2 pos)
	{
		foreach (WorldZone all_zone in _all_zones)
		{
			if (all_zone.IsDisabled())
			{
				continue;
			}
			foreach (Collider2D collider in all_zone._colliders)
			{
				if (collider.OverlapPoint(pos))
				{
					return all_zone;
				}
			}
		}
		return null;
	}

	public static WorldZone GetZoneOfObject(WorldGameObject wgo)
	{
		foreach (WorldZone all_zone in _all_zones)
		{
			if (!all_zone.IsDisabled() && all_zone.DoesObjectBelongToZone(wgo))
			{
				return all_zone;
			}
		}
		return null;
	}

	public void Recalculate()
	{
		_totems.Clear();
		if (IsDisabled())
		{
			_wgos.Clear();
			return;
		}
		List<WorldGameObject> list = new List<WorldGameObject>();
		for (int i = 0; i < _wgos.Count; i++)
		{
			if (_wgos[i] == null)
			{
				_wgos.RemoveAt(i);
				i--;
				continue;
			}
			WorldGameObject worldGameObject = _wgos[i];
			if (worldGameObject.is_removed)
			{
				list.Add(worldGameObject);
				continue;
			}
			if (worldGameObject.obj_def.IsTotem())
			{
				_totems.Add(worldGameObject);
			}
			worldGameObject.RecalculateGridShape();
		}
		foreach (WorldGameObject item in list)
		{
			_wgos.Remove(item);
		}
		RecalculateTotems();
	}

	public void RecalculateTotems()
	{
		bool flag = false;
		bool flag2 = false;
		WorldGameObject worldGameObject = null;
		if (MainGame.me.gui_elements.build_mode_gui.is_shown && FloatingWorldGameObject.cur_floating != null)
		{
			worldGameObject = FloatingWorldGameObject.cur_floating.wobj;
			if (worldGameObject != null)
			{
				if (worldGameObject.obj_def.IsTotem())
				{
					if (!_totems.Contains(worldGameObject))
					{
						_totems.Add(worldGameObject);
						flag = true;
					}
				}
				else if (!_wgos.Contains(worldGameObject))
				{
					_wgos.Add(worldGameObject);
					flag2 = true;
				}
			}
		}
		foreach (WorldGameObject wgo in _wgos)
		{
			if (wgo.obj_def.IsTotem())
			{
				continue;
			}
			wgo.totem_effect = new GameRes();
			foreach (WorldGameObject totem in _totems)
			{
				if (totem.HasTotemInfluenceOnWGO(wgo))
				{
					wgo.totem_effect += totem.obj_def.totem_params;
				}
			}
		}
		if (flag)
		{
			_totems.Remove(worldGameObject);
		}
		if (flag2)
		{
			_wgos.Remove(worldGameObject);
		}
	}

	public bool HasSoulsTotemInZone()
	{
		for (int i = 0; i < _wgos.Count; i++)
		{
			if (_wgos[i].obj_def.type == ObjectDefinition.ObjType.SoulTotem)
			{
				return true;
			}
		}
		return false;
	}

	private void SortWGOSByPriority()
	{
		_wgos = _wgos.OrderByDescending((WorldGameObject p) => p.obj_def.multi_inventory_priority).ToList();
	}

	public float GetTotalQuality()
	{
		WorldZoneDefinition.QualityCalcMethod qualityCalcMethod = ((definition == null) ? WorldZoneDefinition.QualityCalcMethod.Sum : definition.calc_method);
		if (qualityCalcMethod == WorldZoneDefinition.QualityCalcMethod.None)
		{
			return 0f;
		}
		float num = 0f;
		foreach (WorldGameObject wgo in _wgos)
		{
			if (!wgo.obj_def.ignore_counting_at_zone)
			{
				num += wgo.quality;
			}
		}
		if (qualityCalcMethod == WorldZoneDefinition.QualityCalcMethod.Average)
		{
			num /= (float)_wgos.Count;
		}
		OnQualityComputed?.Invoke(id, num);
		return num;
	}

	public string GetQualityString()
	{
		string text = definition.string_format.Replace("@", definition.quality_icon);
		Regex regex = new Regex("^(.*?)\\{\\$([a-zA-Z0-9_]+):([^\\}]+)\\}(.*?)$");
		while (true)
		{
			Match match = regex.Match(text);
			if (!match.Success)
			{
				break;
			}
			text = match.Groups[1].Captures[0]?.ToString() + string.Format("{0:" + match.Groups[3].Captures[0]?.ToString() + "}", MainGame.me.player.GetParam(match.Groups[2].Captures[0].ToString())) + match.Groups[4].Captures[0];
		}
		Match match2 = new Regex("(.*)%([a-zA-Z_]+)(.*)").Match(text);
		if (match2.Success)
		{
			text = match2.Groups[1].Captures[0]?.ToString() + GetZoneByID(match2.Groups[2].Captures[0].ToString()).GetTotalQuality() + match2.Groups[3].Captures[0];
		}
		return string.Format(text, GetTotalQuality());
	}

	public bool IsPlayerInZone()
	{
		return DoesObjectBelongToZone(MainGame.me.player);
	}

	public List<Inventory> GetMultiInventory(List<WorldGameObject> exceptions = null, MultiInventory.PlayerMultiInventory player_mi = MultiInventory.PlayerMultiInventory.DontChange, bool include_toolbelt = false, bool sortWGOS = false)
	{
		if (_wgos == null || _wgos.Count == 0)
		{
			return null;
		}
		List<Inventory> list = new List<Inventory>();
		bool flag = false;
		if (sortWGOS)
		{
			SortWGOSByPriority();
		}
		foreach (WorldGameObject wgo in _wgos)
		{
			if ((exceptions != null && exceptions.Contains(wgo)) || wgo.IsWorker())
			{
				continue;
			}
			if (wgo.is_player)
			{
				if (player_mi != MultiInventory.PlayerMultiInventory.IncludePlayer)
				{
					if (player_mi == MultiInventory.PlayerMultiInventory.ExcludePlayer)
					{
						continue;
					}
				}
				else
				{
					flag = true;
				}
			}
			if (wgo.obj_def.open_in_multiinventory)
			{
				list.Add(new Inventory(wgo));
			}
		}
		if (player_mi == MultiInventory.PlayerMultiInventory.IncludePlayer && !flag)
		{
			list.Add(new Inventory(MainGame.me.player.data));
			if (include_toolbelt)
			{
				Item data = new Item
				{
					inventory = MainGame.me.player.data.secondary_inventory,
					inventory_size = 7
				};
				list.Add(new Inventory(data));
			}
		}
		return list;
	}

	public float GetTotalDarkPoints()
	{
		if (definition == null)
		{
			return 0f;
		}
		float num = 0f;
		foreach (WorldGameObject wgo in _wgos)
		{
			if (!(wgo == null) && !(wgo.obj_id != "grave_ground"))
			{
				Item bodyFromInventory = wgo.GetBodyFromInventory();
				if (bodyFromInventory != null && bodyFromInventory.GetParam("dark") > 0f)
				{
					num += bodyFromInventory.GetParam("dark");
				}
			}
		}
		return num;
	}

	public List<WorldGameObject> GetDarkGraves()
	{
		if (definition == null)
		{
			return null;
		}
		List<WorldGameObject> list = new List<WorldGameObject>();
		foreach (WorldGameObject wgo in _wgos)
		{
			if (!(wgo == null) && !(wgo.obj_id != "grave_ground"))
			{
				Item bodyFromInventory = wgo.GetBodyFromInventory();
				if (bodyFromInventory != null && bodyFromInventory.GetParam("dark") > 0f)
				{
					list.Add(wgo);
				}
			}
		}
		return list;
	}

	public List<WorldGameObject> GetZoneWGOs()
	{
		return _wgos;
	}

	public void OnPlayerEnter()
	{
		if (!MainGame.game_started)
		{
			return;
		}
		RedrawQualities();
		if (!string.IsNullOrEmpty(ovr_music))
		{
			SmartAudioEngine.me.PlayOvrMusic(ovr_music);
		}
		foreach (string smart_sound in smart_sounds)
		{
			SmartAudioEngine.me.PlaySoundWithFade(smart_sound);
		}
		MainGame.me.save.OnEnteredWorldZone(this);
	}

	public void OnPlayerExit()
	{
		RedrawQualities(false);
		if (!string.IsNullOrEmpty(ovr_music))
		{
			SmartAudioEngine.me.StopOvrMusic(ovr_music);
		}
		foreach (string smart_sound in smart_sounds)
		{
			SmartAudioEngine.me.StopSoundWithFade(smart_sound);
		}
	}

	public Bounds GetBounds()
	{
		Bounds result = default(Bounds);
		if (whole_zone_collider != null)
		{
			result = whole_zone_collider.bounds;
		}
		else
		{
			if (_colliders.Count == 0)
			{
				Debug.LogError("Cannot get bounds of zone without colliders");
				return result;
			}
			result = _colliders[0].bounds;
			foreach (Collider2D collider in _colliders)
			{
				result.Encapsulate(collider.bounds);
			}
		}
		result.min = (Vector2)result.min;
		result.max = (Vector2)result.max;
		return result;
	}

	public void RedrawQualities(bool? show = null, bool separate_k = false)
	{
		if (string.IsNullOrEmpty(definition.quality_icon))
		{
			return;
		}
		if (!show.HasValue)
		{
			show = MainGame.me.player_char.player.show_wgo_qualities;
		}
		foreach (WorldGameObject wgo in _wgos)
		{
			wgo.SetQualityHint(show.Value);
		}
	}

	public bool IsDisabled()
	{
		GDPoint[] componentsInParent = base.gameObject.GetComponentsInParent<GDPoint>(includeInactive: true);
		for (int i = 0; i < componentsInParent.Length; i++)
		{
			if (!componentsInParent[i].gameObject.activeSelf)
			{
				return true;
			}
		}
		return false;
	}

	public void PutToAllPossibleInventories(List<Item> drop_list, out List<Item> cant_insert)
	{
		foreach (WorldGameObject zoneWGO in GetZoneWGOs())
		{
			if (zoneWGO == null)
			{
				continue;
			}
			ObjectDefinition obj_def = zoneWGO.obj_def;
			if (obj_def == null)
			{
				Debug.LogError("Not found object definition for WGO \"" + zoneWGO.name + "\", obj_def=" + zoneWGO.obj_id);
			}
			else
			{
				if (!obj_def.open_in_multiinventory)
				{
					continue;
				}
				bool flag = obj_def.can_insert_items != null && obj_def.can_insert_items.Count > 0;
				for (int i = 0; i < drop_list.Count; i++)
				{
					Item item = drop_list[i];
					if ((flag || item.definition.is_big) && (obj_def.can_insert_items == null || !obj_def.can_insert_items.Contains(item.id) || (obj_def.can_insert_items_limit != 0 && obj_def.can_insert_items_limit <= zoneWGO.data.GetItemsCount(item.id))))
					{
						continue;
					}
					int num = zoneWGO.data.CanAddCount(item.id, count_empty: true);
					if (num > 0)
					{
						int num2 = item.value - num;
						if (num2 > 0)
						{
							Item item2 = new Item(item)
							{
								value = num
							};
							zoneWGO.data.AddItem(item2);
							item.value = num2;
						}
						else
						{
							zoneWGO.data.AddItem(item);
							drop_list.RemoveAt(i);
							i--;
						}
					}
				}
			}
		}
		cant_insert = drop_list;
	}

	public void PutToAllPossibleInventoriesSmart(List<Item> drop_list, out List<Item> cant_insert)
	{
		cant_insert = new List<Item>();
		try
		{
			List<WorldGameObject> zoneWGOs = GetZoneWGOs();
			for (int i = 0; i < drop_list.Count; i++)
			{
				Item item = drop_list[i];
				if (string.IsNullOrEmpty(item?.id) || item.value <= 0)
				{
					continue;
				}
				SortedListWithDuplicatableKeys<WorldGameObject> sortedListWithDuplicatableKeys = new SortedListWithDuplicatableKeys<WorldGameObject>();
				foreach (WorldGameObject item3 in zoneWGOs)
				{
					if (item3?.obj_def != null && item3.obj_def.open_in_multiinventory)
					{
						sortedListWithDuplicatableKeys.Insert(item3.GetItemInsertionCoeff(item), item3);
					}
				}
				if (sortedListWithDuplicatableKeys.Count == 0)
				{
					continue;
				}
				for (int num = sortedListWithDuplicatableKeys.Count - 1; num >= 0; num--)
				{
					WorldGameObject worldGameObject = sortedListWithDuplicatableKeys.values[num];
					if ((!item.definition.is_big && worldGameObject.obj_def.can_insert_items.Count <= 0) || worldGameObject.CanInsertItem(item))
					{
						int num2 = worldGameObject.data.CanAddCount(item.id, count_empty: true);
						if (num2 > 0)
						{
							int num3 = item.value - num2;
							if (num3 <= 0)
							{
								worldGameObject.AddToInventory(item);
								drop_list.RemoveAt(i);
								i--;
								break;
							}
							Item item2 = new Item(item)
							{
								value = num2
							};
							worldGameObject.AddToInventory(item2);
							item.value = num3;
						}
					}
				}
			}
			cant_insert = drop_list;
		}
		catch (Exception message)
		{
			Debug.LogError(message);
		}
	}

	public void EnableWorldZone()
	{
		foreach (Collider2D collider in _colliders)
		{
			collider.enabled = true;
		}
		Collider2D[] components = GetComponents<Collider2D>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].enabled = true;
		}
		RefreshWGOsBelongingsToZone();
		Recalculate();
	}

	public void DisableWorldZone()
	{
		foreach (Collider2D collider in _colliders)
		{
			collider.enabled = false;
		}
		Collider2D[] components = GetComponents<Collider2D>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].enabled = false;
		}
		_wgos.Clear();
	}

	private void RefreshWGOsBelongingsToZone()
	{
		_wgos.Clear();
		foreach (WorldGameObject obj in WorldMap.objs)
		{
			if (obj != null && DoesObjectBelongToZone(obj))
			{
				obj.RecalculateZoneBelonging();
			}
		}
	}
}
