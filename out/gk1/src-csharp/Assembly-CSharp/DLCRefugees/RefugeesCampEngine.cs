using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DLCRefugees;

public class RefugeesCampEngine : MonoBehaviour
{
	public enum UpdateHappinessItemsMode
	{
		ItemUpdatesGameRes,
		GameResUpdatesItem
	}

	public const string CAMP_STORAGE_CUSTOM_TAG = "refugee_camp_depot";

	public const string CAMP_PROGRESS_OBJ_CUSTOM_TAG = "refugee_camp_progress_obj";

	public const string CAMP_HAPPINESS_RES_NAME = "cur_refugee_happiness";

	public const string CAMP_HAPPINESS_ITEM_NAME = "refugee_happiness_item";

	public const string CAMP_TENT_CUSTOM_TAG = "refuee_camp_tent";

	public const string CAMP_TENT_PLACE_AVAILABLE_ITEM = "refugee_tent_place_available_item";

	public const string CAMP_CURRENT_PROGRESS_BAR = "refugee_current_progress_bar";

	public const string CAMP_AVAILABLE_SLOTS_ITEM_NAME = "refugee_available_slots";

	public const float WATER_NEEDS_FOR_REFUGEE_PER_DAY = 3f;

	public const float ENERGY_NEEDS_FOR_REFUGEE_PER_DAY = 30f;

	public const float MAX_WATER_SATIETY_COEFF = 2f;

	public const float MAX_ENERGY_SATIETY_COEFF = 2f;

	public const int MAX_REFUGEE_CAMP_QUALITY = 16;

	private readonly List<string> _home_additional_tents_gd_point_names_list = new List<string> { "gd_refugee_camp_wp_home_4", "gd_refugee_camp_wp_home_5", "gd_refugee_camp_wp_home_6", "gd_refugee_camp_wp_home_7" };

	private RefugeeCampData _data;

	private WorldGameObject _camp_storage_cached;

	private WorldGameObject _camp_progress_object_cached;

	private MaskProgressBar _camp_progress_bar_cached;

	private WorldZone _camp_zone;

	private WorldZone _master_alarich_zone;

	private static RefugeesCampEngine _instance;

	private RefugeeCampData Data
	{
		get
		{
			if (_data == null)
			{
				_data = MainGame.me.save.refugees_camp_data;
			}
			return _data;
		}
		set
		{
			_data = value;
		}
	}

	public static RefugeesCampEngine instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = SingletonGameObjects.FindOrCreate<RefugeesCampEngine>();
				if (_instance == null)
				{
					Debug.LogError("RefugeesCampEngine: error finding type");
				}
			}
			return _instance;
		}
	}

	private WorldGameObject camp_storage
	{
		get
		{
			if (_camp_storage_cached == null)
			{
				_camp_storage_cached = WorldMap.GetWorldGameObjectByCustomTag("refugee_camp_depot");
			}
			return _camp_storage_cached;
		}
	}

	public WorldZone camp_zone
	{
		get
		{
			if (_camp_zone == null)
			{
				_camp_zone = WorldZone.GetZoneByID("refugees_camp");
			}
			return _camp_zone;
		}
	}

	public WorldZone master_alarich_zone
	{
		get
		{
			if (_master_alarich_zone == null)
			{
				_master_alarich_zone = WorldZone.GetZoneByID("alarich_tent_inside");
			}
			return _master_alarich_zone;
		}
	}

	public WorldGameObject camp_progress_object
	{
		get
		{
			if (_camp_progress_object_cached == null)
			{
				_camp_progress_object_cached = WorldMap.GetWorldGameObjectByCustomTag("refugee_camp_progress_obj");
			}
			return _camp_progress_object_cached;
		}
	}

	private MaskProgressBar camp_progress_bar
	{
		get
		{
			if (_camp_progress_bar_cached == null)
			{
				_camp_progress_bar_cached = camp_progress_object.GetComponentInChildren<MaskProgressBar>();
			}
			return _camp_progress_bar_cached;
		}
	}

	public int refugees_count => Data.active_refugee_list.Count;

	public float camp_zone_quality { get; set; }

	public void Init()
	{
		Data.Init();
		WorldZone.OnQualityComputed = (Action<string, float>)Delegate.Combine(WorldZone.OnQualityComputed, new Action<string, float>(WorldZoneComputed));
		if (Data.camp_was_started_at_once)
		{
			_camp_storage_cached = WorldMap.GetWorldGameObjectByCustomTag("refugee_camp_depot");
			_camp_progress_object_cached = WorldMap.GetWorldGameObjectByCustomTag("refugee_camp_progress_obj");
			_camp_progress_bar_cached = camp_progress_object.GetComponentInChildren<MaskProgressBar>();
		}
		SetCampMusic(Data.camp_music);
		SetCampMusicAlarich(Data.camp_music_alarich);
	}

	public void DeInit()
	{
		WorldZone.OnQualityComputed = (Action<string, float>)Delegate.Remove(WorldZone.OnQualityComputed, new Action<string, float>(WorldZoneComputed));
		if (Data.camp_was_started_at_once)
		{
			_camp_storage_cached = null;
			_camp_progress_object_cached = null;
			_camp_progress_bar_cached = null;
		}
		Data = null;
	}

	public void StartCampLive()
	{
		Data.is_camp_living = true;
		Data.camp_was_started_at_once = true;
		MainGame.me.player.SetParam("camp_is_live", 1f);
		UpdateCampProgressObjectBar();
	}

	public void StopCampLive()
	{
		Data.is_camp_living = false;
		MainGame.me.player.SetParam("camp_is_live", 0f);
	}

	public WorldGameObject SpawnNextRefugeeAtTransform(Transform transform, string home_point_gd_tag)
	{
		RefugeeInfo nextRefugeeInfoToSpawn = GetNextRefugeeInfoToSpawn();
		WorldGameObject worldGameObject = null;
		Refugee refugee = new Refugee();
		if (nextRefugeeInfoToSpawn != null)
		{
			worldGameObject = WorldMap.SpawnWGO(MainGame.me.world_root, nextRefugeeInfoToSpawn.obj_id, transform.position);
			worldGameObject.custom_tag = nextRefugeeInfoToSpawn.custom_tag;
			refugee = AddRefugee(worldGameObject);
		}
		else
		{
			Debug.LogError("There is no refugee for spawn");
		}
		refugee.home_gd_point_tag = home_point_gd_tag;
		Debug.Log("Spawned refugee: " + worldGameObject?.ToString() + ", with home gd point tag: " + refugee.home_gd_point_tag);
		return worldGameObject;
	}

	public WorldGameObject SpawnNextRefugeeAtTransformFromCraft(Transform transform)
	{
		WorldGameObject vacantTentForRefugee = GetVacantTentForRefugee();
		string homeGDPointForTent = GetHomeGDPointForTent(vacantTentForRefugee);
		return SpawnNextRefugeeAtTransform(transform, homeGDPointForTent);
	}

	public void FeedRefugeeForCycle(float time_in_days)
	{
		RefugeeWealth refugee_wealth = FeedRefuge(time_in_days);
		float happiness_delta = CalculateOverallHappinessDelta(refugee_wealth, time_in_days);
		Debug.Log("happiness_delta: " + happiness_delta);
		UpdateRefugeeCampValues(happiness_delta);
	}

	public float PredictRefugeeHappinessChange(float time_in_days)
	{
		if (refugees_count == 0)
		{
			return 0f;
		}
		RefugeeWealth refugee_wealth = PredictRefugeeFeeding(time_in_days);
		return CalculateOverallHappinessDelta(refugee_wealth, time_in_days);
	}

	public void AddItemsOnTentAppear(WorldGameObject tent_object)
	{
		tent_object.AddToInventory("refugee_tent_place_available_item", tent_object.obj_def.can_insert_items_limit);
	}

	public string GetRefugeesHomeGDTag(WorldGameObject refugee_object)
	{
		for (int i = 0; i < Data.active_refugee_list.Count; i++)
		{
			if (refugee_object == Data.active_refugee_list[i].world_game_object)
			{
				return Data.active_refugee_list[i].home_gd_point_tag;
			}
		}
		Debug.LogError("Refugee object not found list", refugee_object);
		return string.Empty;
	}

	public List<WorldGameObject> GetSpawnedRefugeeList()
	{
		List<WorldGameObject> list = new List<WorldGameObject>();
		foreach (Refugee item in Data.active_refugee_list)
		{
			list.Add(item.world_game_object);
		}
		return list;
	}

	public void PlaceRoadAndAuxiliaryForTent(WorldGameObject tent_wgo)
	{
		string homeGDPointForTent = GetHomeGDPointForTent(tent_wgo);
		string gd_point_tag_to_enable = string.Empty;
		string gd_point_tag_to_enable2 = string.Empty;
		switch (homeGDPointForTent)
		{
		case "gd_refugee_camp_wp_home_4":
			gd_point_tag_to_enable = "gd_refugees_road_add_tent_1";
			gd_point_tag_to_enable2 = "gd_refugee_stage_2_tent_4";
			break;
		case "gd_refugee_camp_wp_home_5":
			gd_point_tag_to_enable = "gd_refugees_road_add_tent_2";
			gd_point_tag_to_enable2 = "gd_refugee_stage_2_tent_5";
			break;
		case "gd_refugee_camp_wp_home_6":
			gd_point_tag_to_enable = "gd_refugees_road_add_tent_3";
			gd_point_tag_to_enable2 = "gd_refugee_stage_2_tent_6";
			break;
		case "gd_refugee_camp_wp_home_7":
			gd_point_tag_to_enable = "gd_refugees_road_add_tent_4";
			gd_point_tag_to_enable2 = "gd_refugee_stage_2_tent_7";
			break;
		}
		EnableGDPoint(gd_point_tag_to_enable);
		EnableGDPoint(gd_point_tag_to_enable2);
	}

	public float StartCooking(WorldGameObject wgo)
	{
		CraftComponent craft = wgo.components.craft;
		MultiInventory multiInventory = wgo.GetMultiInventory();
		List<CraftDefinition> list = new List<CraftDefinition>();
		foreach (CraftDefinition craft2 in craft.crafts)
		{
			if (!MainGame.me.save.locked_crafts.Contains(craft2.id) && (!craft2.needs_unlock || MainGame.me.save.unlocked_crafts.Contains(craft2.id)) && multiInventory.IsEnoughItems(craft2.needs))
			{
				list.Add(craft2);
			}
		}
		if (list.Count == 0)
		{
			return UnityEngine.Random.Range(5, 30);
		}
		if (TimeOfDay.me.GetTimeK() > 0.75f)
		{
			return TimeOfDay.me.GetSecondsToTheMorning() + 24.75f;
		}
		CraftDefinition craftDefinition = list[UnityEngine.Random.Range(0, list.Count - 1)];
		wgo.TryStartCraft(craftDefinition.id);
		return craftDefinition.craft_time.EvaluateFloat() + 1f;
	}

	public void SetCampMusic(RefugeeCampMusic campMusic = RefugeeCampMusic.Default)
	{
		Data.camp_music = campMusic;
		if (camp_zone != null)
		{
			switch (campMusic)
			{
			case RefugeeCampMusic.Default:
				camp_zone.ovr_music = "dlc_refugee_theme";
				break;
			case RefugeeCampMusic.Sad:
				camp_zone.ovr_music = "dlc_refugee_theme_sad";
				break;
			case RefugeeCampMusic.Happy:
				camp_zone.ovr_music = "dlc_refugee_theme_happy";
				break;
			}
		}
	}

	public void SetCampMusicAlarich(RefugeeCampMusicAlarich campMusicAlarich = RefugeeCampMusicAlarich.None)
	{
		Data.camp_music_alarich = campMusicAlarich;
		if (master_alarich_zone != null)
		{
			switch (campMusicAlarich)
			{
			case RefugeeCampMusicAlarich.None:
				master_alarich_zone.ovr_music = string.Empty;
				break;
			case RefugeeCampMusicAlarich.AlarichsTheme:
				master_alarich_zone.ovr_music = "master_alarich_music";
				break;
			}
		}
	}

	public void UpdateRefugeeCampValues(float happiness_delta, UpdateHappinessItemsMode mode = UpdateHappinessItemsMode.GameResUpdatesItem)
	{
		if (mode == UpdateHappinessItemsMode.ItemUpdatesGameRes)
		{
			UpdateCampProgressObjectSlots();
		}
		UpdateHappiness(happiness_delta);
		UpdateCampProgressObjectHappinessItems(mode);
		UpdateCampProgressObjectBar();
	}

	public static float GetPlayersDebt()
	{
		float result = 0f;
		WorldGameObject worldGameObjectByCustomTag = WorldMap.GetWorldGameObjectByCustomTag("npc_refugee_6", ignore_not_found_error: true);
		if (worldGameObjectByCustomTag != null && worldGameObjectByCustomTag.GetParamInt("is_player_took_money") == 1)
		{
			result = worldGameObjectByCustomTag.GetParam("money_debd_amount");
		}
		return result;
	}

	private RefugeeWealth FeedRefuge(float time_in_days)
	{
		GetRefugeeNeeds(time_in_days, out var total_water_need_for_cycle, out var total_energy_need_for_cycle);
		float num = GetWaterFromStorage(Mathf.CeilToInt(total_water_need_for_cycle));
		float num2 = GetEnergyFromMealInStorage(total_energy_need_for_cycle);
		if (num > 2f)
		{
			num = 2f;
		}
		if (num2 > 2f)
		{
			num2 = 2f;
		}
		return new RefugeeWealth(num, num2, refugees_count);
	}

	private void GetRefugeeNeeds(float time_in_days, out float total_water_need_for_cycle, out float total_energy_need_for_cycle)
	{
		total_water_need_for_cycle = 3f * (float)refugees_count * time_in_days;
		total_energy_need_for_cycle = 30f * (float)refugees_count * time_in_days;
	}

	private void WorldZoneComputed(string world_zone_id, float world_zone_quality)
	{
		if (Data.is_camp_living && world_zone_id == "refugees_camp")
		{
			camp_zone_quality = world_zone_quality;
			UpdateRefugeeCampValues(0f, UpdateHappinessItemsMode.ItemUpdatesGameRes);
			if (world_zone_quality == 16f)
			{
				MainGame.me.save.quests.CheckKeyQuests("dlc_refugees_paradise");
			}
		}
	}

	private RefugeeWealth PredictRefugeeFeeding(float time_in_days)
	{
		GetRefugeeNeeds(time_in_days, out var total_water_need_for_cycle, out var total_energy_need_for_cycle);
		int water_amount_to_remove;
		float water_satiety_coeff = NeedWaterFromStorage(Mathf.CeilToInt(total_water_need_for_cycle), out water_amount_to_remove);
		List<SimplifiedItem> items_for_remove;
		float energy_satiety_coeff = NeedEnergyMealFromStorage(total_energy_need_for_cycle, out items_for_remove);
		return new RefugeeWealth(water_satiety_coeff, energy_satiety_coeff, refugees_count);
	}

	private float CalculateOverallHappinessDelta(RefugeeWealth refugee_wealth, float time_in_days)
	{
		return (3f * (refugee_wealth.energy_satiety_coeff + refugee_wealth.water_satiety_coeff) - 2f * time_in_days) / 100f * (float)refugee_wealth.refugees_count;
	}

	private Refugee AddRefugee(WorldGameObject world_game_object)
	{
		Refugee refugee = new Refugee(world_game_object);
		Data.active_refugee_list.Add(refugee);
		return refugee;
	}

	private RefugeeInfo GetNextRefugeeInfoToSpawn()
	{
		RefugeeInfo result = null;
		if (Data.refugee_to_spawn_ordered_list.Count > 0)
		{
			Data.refugee_already_spawned_list.Add(Data.refugee_to_spawn_ordered_list[0]);
			Data.refugee_to_spawn_ordered_list.RemoveAt(0);
			result = Data.refugee_already_spawned_list[Data.refugee_already_spawned_list.Count - 1];
		}
		else
		{
			Debug.LogError("No refugee for spawn");
		}
		return result;
	}

	private float GetWaterFromStorage(int water_amount_need)
	{
		MultiInventory multiInventory = camp_storage.GetMultiInventory();
		int water_amount_to_remove;
		float result = NeedWaterFromStorage(water_amount_need, out water_amount_to_remove);
		multiInventory.RemoveItem("water", water_amount_to_remove);
		Debug.Log("water satiety: " + result);
		return result;
	}

	private float NeedWaterFromStorage(int water_amount_need, out int water_amount_to_remove)
	{
		int totalCount = camp_storage.GetMultiInventory().GetTotalCount("water");
		water_amount_to_remove = 0;
		if (totalCount > water_amount_need)
		{
			water_amount_to_remove = water_amount_need;
		}
		else
		{
			water_amount_to_remove = totalCount;
		}
		return (float)water_amount_to_remove / (float)water_amount_need;
	}

	private float NeedEnergyMealFromStorage(float energy_amount, out List<SimplifiedItem> items_for_remove)
	{
		List<SimplifiedItem> list = new List<SimplifiedItem>();
		items_for_remove = new List<SimplifiedItem>();
		List<Item> inventory = camp_storage.data.inventory;
		for (int i = 0; i < inventory.Count; i++)
		{
			Item item = inventory[i];
			if (item == null || item.IsEmpty() || item.value <= 0)
			{
				continue;
			}
			ItemDefinition definition = item.definition;
			float num = definition.params_on_use.Get("energy");
			if (!definition.can_be_used || !(num > 0f))
			{
				continue;
			}
			bool flag = false;
			for (int j = 0; j < list.Count; j++)
			{
				if (list[j].id == item.id)
				{
					flag = true;
					list[j].count += item.value;
					break;
				}
			}
			if (flag)
			{
				continue;
			}
			int k = 0;
			if (list.Count > 0)
			{
				for (k = 0; k < list.Count && !(list[k].give_energy < num); k++)
				{
				}
			}
			if (k == list.Count)
			{
				list.Add(new SimplifiedItem(item.id, item.value, num));
			}
			else
			{
				list.Insert(k, new SimplifiedItem(item.id, item.value, num));
			}
		}
		Debug.Log("camp_storage_inventory count:" + inventory.Count);
		Debug.Log("items_for_roll count:" + list.Count);
		float num2 = energy_amount;
		while (num2 > 0f && list.Count != 0)
		{
			int index = 0;
			if (list.Count > 4)
			{
				index = UnityEngine.Random.Range(0, 4);
			}
			SimplifiedItem simplifiedItem = list[index];
			num2 -= simplifiedItem.give_energy;
			bool flag2 = false;
			for (int l = 0; l < items_for_remove.Count; l++)
			{
				if (items_for_remove[l].id == simplifiedItem.id)
				{
					items_for_remove[l].count++;
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				items_for_remove.Add(new SimplifiedItem(simplifiedItem.id, 1, simplifiedItem.give_energy));
			}
			simplifiedItem.count--;
			if (simplifiedItem.count == 0)
			{
				list.RemoveAt(index);
			}
		}
		return (energy_amount - num2) / energy_amount;
	}

	private float GetEnergyFromMealInStorage(float energy_amount)
	{
		List<SimplifiedItem> items_for_remove;
		float result = NeedEnergyMealFromStorage(energy_amount, out items_for_remove);
		for (int i = 0; i < items_for_remove.Count; i++)
		{
			if (!camp_storage.data.RemoveItem(items_for_remove[i].id, items_for_remove[i].count))
			{
				Debug.LogError("FATAL ERROR: can not remove item id=\"" + items_for_remove[i].id + "\", count=" + items_for_remove[i].count + "; from camp storage!");
			}
		}
		Debug.Log("energy satiety: " + result);
		return result;
	}

	private void UpdateCampProgressObjectSlots()
	{
		int num = Mathf.RoundToInt(camp_zone_quality);
		camp_progress_object.SetParam("refugee_available_slots", num);
	}

	private void UpdateHappiness(float happiness_delta)
	{
		float param = MainGame.me.player.GetParam("cur_refugee_happiness");
		int paramInt = camp_progress_object.GetParamInt("refugee_available_slots");
		param += happiness_delta;
		if (param < 0f)
		{
			param = 0f;
		}
		if (param > (float)paramInt)
		{
			param = paramInt;
		}
		MainGame.me.player.SetParam("cur_refugee_happiness", param);
	}

	private void UpdateCampProgressObjectHappinessItems(UpdateHappinessItemsMode mode = UpdateHappinessItemsMode.GameResUpdatesItem)
	{
		int itemsCount = camp_progress_object.data.GetItemsCount("refugee_happiness_item");
		int paramInt = camp_progress_object.GetParamInt("refugee_available_slots");
		int totalHappiness = GetTotalHappiness();
		if (mode == UpdateHappinessItemsMode.GameResUpdatesItem)
		{
			if (totalHappiness >= itemsCount)
			{
				camp_progress_object.data.AddItem("refugee_happiness_item", totalHappiness - itemsCount);
			}
			else if (totalHappiness < itemsCount)
			{
				camp_progress_object.data.RemoveItem("refugee_happiness_item", itemsCount - totalHappiness);
			}
		}
		else
		{
			if (itemsCount > paramInt)
			{
				camp_progress_object.data.RemoveItem("refugee_happiness_item", itemsCount - paramInt);
			}
			itemsCount = camp_progress_object.data.GetItemsCount("refugee_happiness_item");
			MainGame.me.player.AddToParams("cur_refugee_happiness", itemsCount - totalHappiness);
		}
	}

	private void UpdateCampProgressObjectBar()
	{
		float campHappinessProgress = GetCampHappinessProgress();
		camp_progress_object.SetParam("refugee_current_progress_bar", campHappinessProgress);
		camp_progress_bar.UpdateBar();
	}

	public float GetCampHappinessProgress()
	{
		return MainGame.me.player.GetParam("cur_refugee_happiness") - (float)GetTotalHappiness();
	}

	public int GetTotalHappiness()
	{
		return Mathf.FloorToInt(MainGame.me.player.GetParam("cur_refugee_happiness"));
	}

	private WorldGameObject GetVacantTentForRefugee()
	{
		List<WorldGameObject> worldGameObjectsByCustomTag = WorldMap.GetWorldGameObjectsByCustomTag("refuee_camp_tent");
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		for (int i = 0; i < worldGameObjectsByCustomTag.Count; i++)
		{
			dictionary[worldGameObjectsByCustomTag[i].obj_id] = worldGameObjectsByCustomTag[i].obj_def.can_insert_items_limit;
		}
		for (int j = 0; j < worldGameObjectsByCustomTag.Count; j++)
		{
			string obj_id = worldGameObjectsByCustomTag[j].obj_id;
			Debug.Log(obj_id);
			dictionary[obj_id] -= worldGameObjectsByCustomTag[j].data.GetItemsCount("refugee_tent_place_available_item");
			Debug.Log("refugee_tent_place_available_item count: " + worldGameObjectsByCustomTag[j].data.GetItemsCount("refugee_tent_place_available_item"));
			dictionary[obj_id] -= worldGameObjectsByCustomTag[j].data.GetItemsCount("refugee_tent_place_busy_item");
			Debug.Log("refugee_tent_place_busy_item count: " + worldGameObjectsByCustomTag[j].data.GetItemsCount("refugee_tent_place_busy_item"));
			if (dictionary[obj_id] == 0)
			{
				dictionary.Remove(worldGameObjectsByCustomTag[j].obj_id);
			}
		}
		if (dictionary.Count != 1)
		{
			Debug.LogError("Found vacant tents count: " + dictionary.Count);
		}
		int value = dictionary.First().Value;
		if (value != 1)
		{
			Debug.LogError("Wrong vacant places count in tent: " + value);
		}
		string key = dictionary.First().Key;
		Debug.Log("Vacant tent obj id place:" + key);
		WorldGameObject worldGameObjectByObjId = WorldMap.GetWorldGameObjectByObjId(key);
		worldGameObjectByObjId.AddToInventory("refugee_tent_place_busy_item", 1);
		return worldGameObjectByObjId;
	}

	private string GetHomeGDPointForTent(WorldGameObject tent_object)
	{
		float num = float.MaxValue;
		string text = string.Empty;
		for (int i = 0; i < _home_additional_tents_gd_point_names_list.Count; i++)
		{
			string text2 = _home_additional_tents_gd_point_names_list[i];
			float magnitude = ((Vector2)WorldMap.GetGDPointByGDTag(text2).transform.position - (Vector2)tent_object.transform.position).magnitude;
			if (magnitude < num)
			{
				text = text2;
				num = magnitude;
			}
		}
		Debug.Log("nearest gd point with tag: " + text + ", and distance: " + num);
		return text;
	}

	private void TEST_ADD_HAPPINESS()
	{
		MainGame.me.player.AddToParams("cur_refugee_happiness", 0.1f);
		UpdateCampProgressObjectBar();
		UpdateCampProgressObjectHappinessItems();
	}

	private void EnableGDPoint(string gd_point_tag_to_enable)
	{
		GDPoint gDPointByGDTag = WorldMap.GetGDPointByGDTag(gd_point_tag_to_enable);
		if (gDPointByGDTag != null)
		{
			gDPointByGDTag.gameObject.SetActive(value: true);
			RoundAndSortComponent[] componentsInChildren = gDPointByGDTag.GetComponentsInChildren<RoundAndSortComponent>(includeInactive: true);
			if (componentsInChildren != null && componentsInChildren.Length != 0)
			{
				RoundAndSortComponent[] array = componentsInChildren;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].DoUpdateStuff(force: true);
				}
			}
		}
		else
		{
			Debug.LogError("GD Point not found by gd tag: " + gd_point_tag_to_enable);
		}
	}
}
