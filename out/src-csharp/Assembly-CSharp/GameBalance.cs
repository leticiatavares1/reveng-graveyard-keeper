using System.Collections;
using System.Collections.Generic;
using LinqTools;
using UnityEngine;

public class GameBalance : GameBalanceBase
{
	private static GameBalance _instance;

	public List<JobDefinition> jobs_data = new List<JobDefinition>();

	public List<CharDefinition> chars_data = new List<CharDefinition>();

	public List<JobAtomDefinition> jobs_atom_data = new List<JobAtomDefinition>();

	public List<LevelGrades> grade_levels = new List<LevelGrades>();

	public List<ObjectDefinition> objs_data = new List<ObjectDefinition>();

	public List<ItemDefinition> items_data = new List<ItemDefinition>();

	public List<VendorDefinition> vendors_data = new List<VendorDefinition>();

	public List<CraftDefinition> craft_data = new List<CraftDefinition>();

	public List<ObjectCraftDefinition> craft_obj_data = new List<ObjectCraftDefinition>();

	public List<ToolTypeDefinition> tools_data = new List<ToolTypeDefinition>();

	public List<AuraDefinition> auras_data = new List<AuraDefinition>();

	public List<ObjectGroupDefinition> object_groups = new List<ObjectGroupDefinition>();

	public List<QuestDefinition> quests_data = new List<QuestDefinition>();

	public List<SpawnerDefinition> spawners_data = new List<SpawnerDefinition>();

	public List<ProjectileDefinition> projectiles_data = new List<ProjectileDefinition>();

	public List<BodyDefinition> bodies_data = new List<BodyDefinition>();

	public List<SoulDefinition> souls_data = new List<SoulDefinition>();

	public List<GraveRequirement> grave_requirement_data = new List<GraveRequirement>();

	public List<TechBranchDefinition> tech_branches_data = new List<TechBranchDefinition>();

	public List<TechDefinition> techs_data = new List<TechDefinition>();

	public List<WorkDefinition> works_data = new List<WorkDefinition>();

	public List<LogicDefinition> logics_data = new List<LogicDefinition>();

	public List<ProductTypeDefinition> product_types_data = new List<ProductTypeDefinition>();

	public List<WorldZoneDefinition> world_zones_data = new List<WorldZoneDefinition>();

	public List<PerkDefinition> perks_data = new List<PerkDefinition>();

	public List<ReservoirsDefinition> reservoirs_data = new List<ReservoirsDefinition>();

	public List<FishDefinition> fishes_data = new List<FishDefinition>();

	public List<BuffDefinition> buffs_data = new List<BuffDefinition>();

	public List<PrayEventDefinition> pray_events_data = new List<PrayEventDefinition>();

	public List<AchievementDefinition> achievements_data = new List<AchievementDefinition>();

	public List<WorkerDefinition> workers_data = new List<WorkerDefinition>();

	public List<TransportPathsDefinition> transport_paths = new List<TransportPathsDefinition>();

	public List<CutscenesDLCDefinition> cutscenes_data = new List<CutscenesDLCDefinition>();

	public List<TavernEventDefinition> tavern_events = new List<TavernEventDefinition>();

	private Dictionary<string, List<CraftDefinition>> _craft_obj_hash = new Dictionary<string, List<CraftDefinition>>();

	private Dictionary<string, List<ObjectDefinition>> _craft_item_obj_hash = new Dictionary<string, List<ObjectDefinition>>();

	private Dictionary<string, List<string>> _items_basename_cache = new Dictionary<string, List<string>>();

	private Dictionary<ItemDefinition.ItemType, float> _tool_minimum_efficiency = new Dictionary<ItemDefinition.ItemType, float>();

	private const bool NORMALIZE_TOOLS_EFFICIENCY_FOR_MINUMUM_100_PERCENT = false;

	private static readonly List<string> EMPTY_STRING_LIST = new List<string>();

	public static GameBalance me
	{
		get
		{
			if ((bool)_instance)
			{
				return _instance;
			}
			LoadGameBalance();
			return _instance;
		}
	}

	public override Dictionary<string, IList> GetAllDataListsAndGoogleTabs()
	{
		return new Dictionary<string, IList>
		{
			{ "ProductType", product_types_data },
			{ "Perks", perks_data },
			{ "Items", items_data },
			{ "Objects", objs_data },
			{ "Buffs", buffs_data },
			{ "Vendors", vendors_data },
			{ "Craft", craft_data },
			{ "Survey", craft_data },
			{ "Mix Craft", craft_data },
			{ "Alchemy Ingr", craft_data },
			{ "Sermon", craft_data },
			{ "Put_Remove", craft_obj_data },
			{ "Tool Types", tools_data },
			{ "Auras_Buffs", auras_data },
			{ "Obj Groups", object_groups },
			{ "Quests", quests_data },
			{ "Bodies", bodies_data },
			{ "Souls", souls_data },
			{ "Spawners", spawners_data },
			{ "Projectiles", projectiles_data },
			{ "Works", works_data },
			{ "Logics", logics_data },
			{ "TechBranches", tech_branches_data },
			{ "Techs", techs_data },
			{ "WorldZone", world_zones_data },
			{ "Reservoirs", reservoirs_data },
			{ "Fishes", fishes_data },
			{ "PrayEvents", pray_events_data },
			{ "Achievements", achievements_data },
			{ "Workers", workers_data },
			{ "TransportPaths", transport_paths },
			{ "CutscenesDLC", cutscenes_data },
			{ "Tavern Events", tavern_events }
		};
	}

	public static void Unload()
	{
		_instance = null;
	}

	public static void LoadGameBalance()
	{
		_instance = Resources.Load<GameBalance>("game_data");
		if (_instance == null)
		{
			Debug.LogError("Game data load failed");
			return;
		}
		_instance.CreateIDsCache();
		_instance.CreateItemsBaseNameCache();
		_instance.CreateToolsCache();
		_instance.CreateCraftsCache();
		ObjectGroupDefinition.LinkObjectsToGroups();
	}

	public LevelGrades GetLevelGradeById(string id)
	{
		return grade_levels.Find((LevelGrades p) => p.grade_id == id);
	}

	public GameRes GetObjectRes(string obj_id)
	{
		ObjectDefinition dataOrNull = GetDataOrNull<ObjectDefinition>(obj_id);
		if (dataOrNull != null)
		{
			return dataOrNull.res;
		}
		return new GameRes();
	}

	public List<CraftDefinition> GetCraftsForObject(string obj_id)
	{
		if (_craft_obj_hash.ContainsKey(obj_id))
		{
			return _craft_obj_hash[obj_id];
		}
		List<CraftDefinition> list = new List<CraftDefinition>();
		foreach (CraftDefinition craft_datum in craft_data)
		{
			if (craft_datum.craft_in.Contains(obj_id))
			{
				list.Add(craft_datum);
			}
		}
		_craft_obj_hash.Add(obj_id, list);
		return list;
	}

	public CraftDefinition GetFixCraftForItem(string item_id)
	{
		foreach (CraftDefinition craft_datum in craft_data)
		{
			if (craft_datum.craft_type == CraftDefinition.CraftType.Fixing && craft_datum.output.Count > 0 && craft_datum.output[0].id == item_id)
			{
				return craft_datum;
			}
		}
		return null;
	}

	public CraftDefinition GetRemoveCraftForItem(string obj_id, string item_id)
	{
		foreach (CraftDefinition craft_datum in craft_data)
		{
			if (craft_datum.craft_in.Contains(obj_id) && craft_datum.set_out_wgo_params_on_start && craft_datum.output.Count > 0 && craft_datum.output[0].id == item_id)
			{
				return craft_datum;
			}
		}
		Debug.LogError("Couldn't find a removal craft for item: " + item_id);
		return null;
	}

	private void CreateItemsBaseNameCache()
	{
		_items_basename_cache.Clear();
		foreach (ItemDefinition items_datum in items_data)
		{
			if (items_datum.id.Contains(":"))
			{
				string nameWithoutQualitySuffix = items_datum.GetNameWithoutQualitySuffix();
				if (!_items_basename_cache.ContainsKey(nameWithoutQualitySuffix))
				{
					_items_basename_cache.Add(nameWithoutQualitySuffix, new List<string>());
				}
				_items_basename_cache[nameWithoutQualitySuffix].Add(items_datum.id);
			}
		}
	}

	public List<string> GetItemsOfBaseName(string base_name)
	{
		if (!_items_basename_cache.TryGetValue(base_name, out var value))
		{
			return EMPTY_STRING_LIST;
		}
		return value;
	}

	private void CreateToolsCache()
	{
		_tool_minimum_efficiency.Clear();
		foreach (ItemDefinition items_datum in items_data)
		{
			if (items_datum.type != 0)
			{
				if (!_tool_minimum_efficiency.ContainsKey(items_datum.type))
				{
					_tool_minimum_efficiency.Add(items_datum.type, items_datum.efficiency);
				}
				else if (items_datum.efficiency < _tool_minimum_efficiency[items_datum.type])
				{
					_tool_minimum_efficiency[items_datum.type] = items_datum.efficiency;
				}
			}
		}
		foreach (ItemDefinition.ItemType item in _tool_minimum_efficiency.Keys.ToList())
		{
			_tool_minimum_efficiency[item] = 1f;
		}
	}

	public int GetToolEfficiencyPercent(ItemDefinition tool_item)
	{
		if (!_tool_minimum_efficiency.ContainsKey(tool_item.type))
		{
			return 0;
		}
		return Mathf.RoundToInt(tool_item.efficiency * 100f / _tool_minimum_efficiency[tool_item.type]);
	}

	private void CreateCraftsCache()
	{
		_craft_obj_hash.Clear();
		_craft_item_obj_hash.Clear();
		foreach (CraftDefinition craft_datum in craft_data)
		{
			foreach (string item in craft_datum.craft_in)
			{
				if (!_craft_obj_hash.ContainsKey(item))
				{
					_craft_obj_hash.Add(item, new List<CraftDefinition>());
				}
				_craft_obj_hash[item].Add(craft_datum);
				if (item == "grave_ground" || craft_datum.hidden || craft_datum.dont_show_in_hint)
				{
					continue;
				}
				foreach (Item item2 in craft_datum.output)
				{
					if (!_craft_item_obj_hash.ContainsKey(item2.id))
					{
						_craft_item_obj_hash.Add(item2.id, new List<ObjectDefinition>());
					}
					ObjectDefinition data = GetData<ObjectDefinition>(item);
					if (data != null && !_craft_item_obj_hash[item2.id].Contains(data))
					{
						_craft_item_obj_hash[item2.id].Add(data);
					}
				}
			}
		}
	}

	public List<ObjectDefinition> GetItemCraftsIn(string item_id)
	{
		List<ObjectDefinition> value = null;
		if (_craft_item_obj_hash.TryGetValue(item_id, out value))
		{
			return value;
		}
		int num = 0;
		while (item_id.Contains(":"))
		{
			item_id = item_id.Substring(0, item_id.LastIndexOf(":"));
			if (_craft_item_obj_hash.TryGetValue(item_id, out value))
			{
				return value;
			}
			if (num++ > 10)
			{
				break;
			}
		}
		return new List<ObjectDefinition>();
	}
}
