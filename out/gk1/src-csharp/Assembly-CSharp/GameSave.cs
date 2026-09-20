using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using DG.Tweening;
using DLCRefugees;
using LinqTools;
using UnityEngine;

[Serializable]
public class GameSave
{
	[Serializable]
	public struct SavedDropItem
	{
		public Item res;

		public Vector3 pos;

		public string zone_id;
	}

	[Serializable]
	public class SavedGDPoint
	{
		public string gd_tag;

		public bool enabled;
	}

	[SerializeField]
	private Item _inventory = new Item();

	public QuestSystem quests = new QuestSystem();

	public AchievementsSystem achievements = new AchievementsSystem();

	public int day = 1;

	[HideInInspector]
	public SerializableGameMap map = new SerializableGameMap();

	public CraftsInventory crafts = new CraftsInventory();

	public CraftsInventory obj_crafts = new CraftsInventory();

	public SavedDungeonsList dungeons = new SavedDungeonsList();

	public SavedWorkersList workers = new SavedWorkersList();

	public List<string> unlocked_techs = new List<string>();

	public List<string> unlocked_crafts = new List<string>();

	public List<string> locked_crafts = new List<string>();

	public List<string> unlocked_works = new List<string>();

	public List<string> unlocked_phrases = new List<string>();

	public List<string> unlocked_perks = new List<string>();

	public List<string> black_list_of_phrases = new List<string>();

	public List<string> completed_one_time_crafts = new List<string>();

	public List<string> known_world_zones = new List<string>();

	public List<string> known_fishes = new List<string>();

	public List<string> known_fishes_clear = new List<string>();

	public List<string> last_bait_reservoirs = new List<string>();

	public List<string> last_bait_baits = new List<string>();

	public List<string> revealed_techs = new List<string>();

	public List<string> visible_techs = new List<string>();

	public List<int> unlocked_tech_branches = new List<int>();

	public PlayersTavernEngine players_tavern_engine = new PlayersTavernEngine();

	public RefugeeCampData refugees_camp_data = new RefugeeCampData();

	public string[] equipped_items = new string[4];

	public GameLogics game_logics = new GameLogics();

	public int max_hp = 100;

	public int max_energy = 100;

	public int max_sanity = 100;

	public Vector3 player_position;

	public int dungeon_seed;

	public long unique_id_iterator = 1L;

	public bool has_global_craft_control;

	[SerializeField]
	private EnvironmentEngine.EnvironmentEngineData _environment_engine_data;

	[SerializeField]
	private TimeOfDay.SerializedTimeOfDay _serialized_time_of_day;

	public List<SavedDropItem> drops = new List<SavedDropItem>();

	[SerializeField]
	private string _environment_preset = "";

	private string _stored_environment_preset = "";

	public List<PlayerBuff> buffs = new List<PlayerBuff>();

	public KnownNPCList known_npcs = new KnownNPCList();

	public List<SavedGDPoint> gd_points = new List<SavedGDPoint>();

	public float game_version = LazyConsts.VERSION;

	public float max_version = LazyConsts.VERSION;

	public int cur_time => Mathf.FloorToInt(Time.time * 1f);

	public int day_of_week => day % 6;

	public ItemDefinition GetToolForAction(ItemDefinition.ItemType item_type)
	{
		return null;
	}

	public void QuickSave()
	{
		File.WriteAllText("map.json", map.ToJSON());
		Debug.Log("Map saved");
	}

	public void UnlockAllTechsForTest()
	{
		foreach (TechDefinition techs_datum in GameBalance.me.techs_data)
		{
			UnlockTech(techs_datum.id);
		}
	}

	public void UnlockTech(string tech_id)
	{
		TechDefinition data = GameBalance.me.GetData<TechDefinition>(tech_id);
		if (data == null)
		{
			Debug.LogError("No such tech: " + tech_id);
			return;
		}
		if (!unlocked_techs.Contains(tech_id))
		{
			unlocked_techs.Add(tech_id);
		}
		if (data.hidden)
		{
			RevealHiddenTech(tech_id);
		}
		if (data.invisible)
		{
			MakeVisibleInvisibleTech(tech_id);
		}
		CopyLists(data.crafts, unlocked_crafts);
		CopyLists(data.works, unlocked_works);
		CopyLists(data.phrases, unlocked_phrases);
		data.ApplyTech();
		Stats.DesignEvent("Tech:" + tech_id);
	}

	public void UnlockPhrase(string phrase)
	{
		if (!unlocked_phrases.Contains(phrase))
		{
			unlocked_phrases.Add(phrase);
		}
	}

	public void UnlockCraft(string craft_id)
	{
		if (!unlocked_crafts.Contains(craft_id))
		{
			unlocked_crafts.Add(craft_id);
		}
	}

	public void LockCraft(string craft_id)
	{
		if (unlocked_crafts.Contains(craft_id))
		{
			unlocked_crafts.Remove(craft_id);
		}
	}

	public void LockCraftForever(string craft_id)
	{
		locked_crafts.Add(craft_id);
	}

	public void AddPhraseToBlackList(string phrase)
	{
		if (!black_list_of_phrases.Contains(phrase))
		{
			black_list_of_phrases.Add(phrase);
		}
	}

	public void SetToolbarEquipped(string item_id, int toolbar_index)
	{
		if (toolbar_index >= 0 && toolbar_index < equipped_items.Length)
		{
			UnEquip(item_id);
			equipped_items[toolbar_index] = item_id;
		}
	}

	public int GetEquippedIndex(string item_id)
	{
		for (int i = 0; i < equipped_items.Length; i++)
		{
			if (equipped_items[i] == item_id)
			{
				return i;
			}
		}
		return -1;
	}

	public void UnEquip(int toolbar_index)
	{
		if (toolbar_index >= 0 && toolbar_index < equipped_items.Length)
		{
			equipped_items[toolbar_index] = "";
		}
	}

	public void UnEquip(string item_id)
	{
		for (int i = 0; i < equipped_items.Length; i++)
		{
			if (equipped_items[i] == item_id)
			{
				equipped_items[i] = "";
			}
		}
	}

	public string GetEquippedItem(int toolbar_index)
	{
		if (toolbar_index < 0 || toolbar_index >= equipped_items.Length)
		{
			return "";
		}
		return equipped_items[toolbar_index];
	}

	private void CopyLists(List<string> from, List<string> to)
	{
		foreach (string item in from)
		{
			string text;
			if ((text = item)[0] == '@')
			{
				text = text.Substring(1);
			}
			if (!to.Contains(text))
			{
				to.Add(text);
			}
		}
	}

	public bool IsTechBranchVisible(int branch_id)
	{
		if (branch_id == 0)
		{
			return false;
		}
		TechBranchDefinition dataOrNull = GameBalance.me.GetDataOrNull<TechBranchDefinition>(branch_id.ToString(CultureInfo.InvariantCulture));
		if (dataOrNull != null)
		{
			if (dataOrNull.is_locked)
			{
				return unlocked_tech_branches.Contains(branch_id);
			}
			return true;
		}
		Debug.LogError($"Not found {typeof(TechBranchDefinition)} with id [{branch_id}]");
		return true;
	}

	public void UnlockTechBranch(int branch_id)
	{
		foreach (TechBranchDefinition tech_branches_datum in GameBalance.me.tech_branches_data)
		{
			if (int.Parse(tech_branches_datum.id, CultureInfo.InvariantCulture) == branch_id)
			{
				unlocked_tech_branches.Add(branch_id);
				return;
			}
		}
		Debug.LogError($"Branch with id [{branch_id}] not found");
	}

	public string ToJSON()
	{
		Debug.Log("GameSave.ToJSON");
		map.SaveSceneToMe();
		return JsonUtility.ToJson(this);
	}

	public byte[] ToBinary()
	{
		Debug.Log("GameSave.ToBinary");
		map.SaveSceneToMe();
		byte[] result = SmartSerializer.Serialize(this);
		GC.Collect();
		return result;
	}

	public static GameSave FromJSON(string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			Debug.LogError("Error loading an empty save file");
			return null;
		}
		return JsonUtility.FromJson<GameSave>(s);
	}

	public static GameSave FromBinary(byte[] data)
	{
		return SmartSerializer.Deserialize<GameSave>(data);
	}

	public static void CreateNewSave(Action on_complete)
	{
		Debug.Log("CreateNewSave");
		MainGame.me.save = new GameSave
		{
			player_position = World.player_default_pos,
			dungeon_seed = UnityEngine.Random.Range(0, 100000)
		};
		LoadingGUI.SetProgressBar(0.2f);
		GJTimer.AddTimer(0f, delegate
		{
			GDPoint.RestoreGDPointsState();
			MainGame.me.save.map.RestoreSceneToInitialState();
			MainGame.me.save.InitPlayersInventory();
			MainGame.me.save.quests.InitQuestSystem();
			LoadingGUI.SetProgressBar(0.3f);
			GJTimer.AddTimer(0f, on_complete.TryInvoke);
		});
	}

	public void InitPlayersInventory()
	{
		Debug.Log("InitPlayersInventory");
		_inventory = new Item("inventory");
		_inventory.SetInventorySize(20);
		_inventory.hp = max_hp;
		_inventory.SetParam("sanity", max_sanity);
		_inventory.SetParam("energy", max_energy);
		for (int i = 1; i < 7; i++)
		{
			_inventory.SetParam(Sins.SIN_NAMES[i], 250f);
		}
	}

	public void PrepareForSave()
	{
		if (MainGame.me.player == null)
		{
			return;
		}
		_inventory = MainGame.me.player.data;
		game_version = LazyConsts.VERSION;
		if (game_version > max_version)
		{
			max_version = game_version;
		}
		player_position = MainGame.me.player.transform.localPosition;
		EnvironmentEngine.me.PrepareForSave();
		_environment_engine_data = EnvironmentEngine.me.data;
		_serialized_time_of_day = TimeOfDay.me.ToSerialized();
		WorldMap.ToGameSave(this);
		foreach (GDPoint gd_point in WorldMap.gd_points)
		{
			bool flag = false;
			foreach (SavedGDPoint gd_point2 in gd_points)
			{
				if (gd_point2.gd_tag == gd_point.gd_tag)
				{
					gd_point2.enabled = gd_point.gameObject.activeSelf;
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				gd_points.Add(new SavedGDPoint
				{
					gd_tag = gd_point.gd_tag,
					enabled = gd_point.gameObject.activeSelf
				});
			}
		}
		unique_id_iterator = UniqueID.GetUniqueID();
	}

	public void PrepareAfterLoad()
	{
		Debug.Log("Save.PrepareAfterLoad");
		EnvironmentEngine.me.DeserializeData(_environment_engine_data);
		if (achievements == null)
		{
			achievements = new AchievementsSystem();
		}
		achievements.VerifyAndSetMissedAchievements();
		if (_serialized_time_of_day != null)
		{
			TimeOfDay.me.FromSerialized(_serialized_time_of_day);
			Debug.Log("Restoring time: " + _serialized_time_of_day.time_of_day);
			EnvironmentEngine.SetTime(_serialized_time_of_day.time_of_day);
		}
		else
		{
			TimeOfDay.me.SetTimeK(0.5f);
		}
		if (gd_points != null)
		{
			foreach (SavedGDPoint gd_point in gd_points)
			{
				bool flag = false;
				foreach (GDPoint gd_point2 in WorldMap.gd_points)
				{
					if (gd_point.gd_tag == gd_point2.gd_tag)
					{
						gd_point2.gameObject.SetActive(gd_point.enabled);
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					Debug.LogError("Couldn't de-serialize GDPoint with gd_tag = " + gd_point.gd_tag);
				}
			}
		}
		GameSettings.me.ApplyVolume();
		if (last_bait_baits == null)
		{
			last_bait_baits = new List<string>();
		}
		if (last_bait_reservoirs == null)
		{
			last_bait_reservoirs = new List<string>();
		}
		if (last_bait_baits.Count != last_bait_reservoirs.Count)
		{
			last_bait_baits = new List<string>();
			last_bait_reservoirs = new List<string>();
		}
		if (known_fishes_clear != null && (known_fishes_clear.Count != 0 || known_fishes.Count <= 0))
		{
			return;
		}
		known_fishes_clear = new List<string>();
		Debug.Log("OLD_SAVE_FIX: Recreating known fishes list");
		foreach (string known_fish in known_fishes)
		{
			string[] array = known_fish.Split(new string[1] { ":" }, StringSplitOptions.RemoveEmptyEntries);
			string empty = string.Empty;
			if (array.Length < 3)
			{
				Debug.LogError("Wrong known fish: \"" + known_fishes?.ToString() + "\"");
				continue;
			}
			if (array.Length > 3)
			{
				empty = array[2];
				for (int i = 3; i < array.Length; i++)
				{
					empty = empty + ":" + array[i];
				}
			}
			else
			{
				empty = array[2];
			}
			if (!string.IsNullOrEmpty(empty) && !known_fishes_clear.Contains(empty))
			{
				known_fishes_clear.Add(empty);
			}
		}
	}

	public void LateSaveFixer()
	{
		int num = Mathf.RoundToInt(game_version * 1000f);
		if ((double)game_version < 1.014)
		{
			if (unlocked_techs.Contains("Improvement"))
			{
				UnlockCraft("mining_builddesk:p:mf_box_stuff_place");
				UnlockCraft("vineyard_builddesk:p:mf_box_stuff_place");
				UnlockCraft("graveyard_builddesk:p:mf_box_stuff_place");
				UnlockCraft("cremation_builddesk:p:mf_box_stuff_place");
			}
			if (unlocked_techs.Contains("Price of faith"))
			{
				UnlockPerk("p_cardinal");
			}
			if (unlocked_techs.Contains("Strong alcohol"))
			{
				UnlockCraft("cellar_builddesk:p:mf_distcube_3_place");
				UnlockCraft("booze_from_bottle_apple_braga");
				UnlockCraft("booze_from_bottle_berry_braga");
				UnlockCraft("booze_from_bottle_red_vine_1");
				UnlockCraft("booze_from_bottle_red_vine_2");
				UnlockCraft("booze_from_bottle_red_vine_3");
			}
		}
		if ((double)game_version < 1.021)
		{
			WorldGameObject worldGameObjectByCustomTag = WorldMap.GetWorldGameObjectByCustomTag("donkey");
			if (worldGameObjectByCustomTag == null)
			{
				Debug.LogError("Can't fix donkey stuck: not found donkey!");
			}
			else if (worldGameObjectByCustomTag.GetParam("donkey_must_wait_at_cemetery") > 0.5f && (worldGameObjectByCustomTag.custom_interaction_events == null || worldGameObjectByCustomTag.custom_interaction_events.Count == 0) && WorldMap.GetWorldGameObjectByCustomTag("carrot_box", ignore_not_found_error: true) == null)
			{
				Debug.Log("Fixed donkey stuck");
				worldGameObjectByCustomTag.FireEvent("add_donkey_strike");
			}
		}
		if ((double)game_version < 1.023)
		{
			if (black_list_of_phrases.Contains("@gypsy_recipe_done_1"))
			{
				UnlockTech("Fish shish kebab");
			}
			if (black_list_of_phrases.Contains("snake_back_12b"))
			{
				UnlockCraft("pail_blood");
			}
		}
		if ((double)game_version < 1.026)
		{
			List<WorldGameObject> worldGameObjectsByObjId = WorldMap.GetWorldGameObjectsByObjId("bush_horizontal_2");
			if (worldGameObjectsByObjId == null || worldGameObjectsByObjId.Count == 0)
			{
				Debug.LogError("FATAL ERROR: Not found any \"bush_horizontal_3\"!");
			}
			else
			{
				for (int i = 0; i < worldGameObjectsByObjId.Count; i++)
				{
					if (!(worldGameObjectsByObjId[i] == null))
					{
						Vector3 position = worldGameObjectsByObjId[i].transform.position;
						if (position.x.EqualsTo(872f, 0.1f) && position.y.EqualsTo(-984f, 0.1f))
						{
							worldGameObjectsByObjId[i].DestroyMe();
							Vector3 vector = position;
							Debug.Log("Destroyed redundand bush_horizontal_2 with coords " + vector.ToString());
							break;
						}
					}
				}
			}
		}
		if (num <= 1026 && unlocked_techs.Contains("Glass-blower 2"))
		{
			UnlockPerk("p_t_sand_improve");
		}
		if (num <= 1026)
		{
			WorldGameObject worldGameObjectByObjId = WorldMap.GetWorldGameObjectByObjId("npc_astrologer");
			if (worldGameObjectByObjId != null && worldGameObjectByObjId.GetParamInt("astrologer_2a_check_for_lock") >= 2)
			{
				if (!black_list_of_phrases.Contains("@astrologer_2a_1a_1"))
				{
					UnlockPhrase("@astrologer_trade");
				}
				if (!black_list_of_phrases.Contains("astrologer_2a_1b_6c"))
				{
					MainGame.me.player.DropItem(new Item("quest_key_astrologer"));
					SetTaskState("npc_astrologer", "astrologer_diary", KnownNPC.TaskState.State.Visible);
					UnlockPhrase("@snake_give_key");
					UnlockPhrase("@astrologer_diary");
				}
			}
			List<WorldGameObject> worldGameObjectsByObjId2 = WorldMap.GetWorldGameObjectsByObjId("stone_ore");
			bool flag = false;
			for (int j = 0; j < worldGameObjectsByObjId2.Count; j++)
			{
				if (!(worldGameObjectsByObjId2[j] == null))
				{
					Vector3 position2 = worldGameObjectsByObjId2[j].transform.position;
					if (position2.x.EqualsTo(-3996f, 0.1f) && position2.y.EqualsTo(6672f, 0.1f))
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				Debug.Log("Save fix: added \"stone_ore\" to map.");
				WorldMap.SpawnWGO(MainGame.me.world_root, "stone_ore", new Vector3(-3996f, 6672f, 1389.167f));
			}
			if (black_list_of_phrases.Contains("@horadric_2_1h_4b") && !unlocked_phrases.Contains("@actress_2b_1a"))
			{
				UnlockPhrase("@actress_2b_1a");
			}
		}
		if (num <= 1027 && black_list_of_phrases.Contains("@actor_2_1e") && black_list_of_phrases.Contains("@actor_2_1d") && (!unlocked_phrases.Contains("@actor_2_1e") || !unlocked_phrases.Contains("@actor_2_1d")))
		{
			UnlockPhrase("@astrologer_2a_1a_1");
		}
		if (num <= 1028)
		{
			WorldMap.SpawnWGO(MainGame.me.world_root, "teleport_point", new Vector3(25728f, -2208f)).custom_tag = "tp_lighthouse_a";
		}
		if (num <= 1030)
		{
			if (black_list_of_phrases.Contains("@snake_magic_100") && !black_list_of_phrases.Contains("@snake_instrument_ready"))
			{
				MainGame.me.player.DropItem(new Item("restoration_instrument"));
			}
			if (black_list_of_phrases.Contains("@snake_magic_100"))
			{
				black_list_of_phrases.Add("@snake_final_ritual");
			}
		}
		if (num <= 1030)
		{
			WorldGameObject worldGameObjectByObjId2 = WorldMap.GetWorldGameObjectByObjId("donkey");
			if (worldGameObjectByObjId2.GetParamInt("strike_completed") >= 2)
			{
				worldGameObjectByObjId2.SetParam("get_carrots_not_locked", 1f);
			}
		}
		if (num <= 1032)
		{
			float param = MainGame.me.player.GetParam("body_max");
			float param2 = MainGame.me.player.GetParam("body_min");
			if ((BuffsLogics.FindBuffByID("buff_skull") != null && Math.Abs(param2 - param) < 0.01f) || (BuffsLogics.FindBuffByID("buff_skull") == null && param < param2 && Math.Abs(param2 - param) > 0.01f))
			{
				MainGame.me.player.SetParam("body_max", param + 1f);
				MainGame.me.player.SetParam("cur_bodies_count", 0f);
			}
		}
		if (num <= 1032 && MainGame.me.save.black_list_of_phrases.Contains("@merchant_2b") && !MainGame.me.save.unlocked_phrases.Contains("@merchant_favore") && !MainGame.me.save.unlocked_phrases.Contains("@merchant_forgive"))
		{
			UnlockPhrase("@merchant_favore");
		}
		if (num <= 1034 && !MainGame.me.save.unlocked_phrases.Contains("@merchant_2e") && !MainGame.me.save.unlocked_phrases.Contains("@merchant_forgive") && !MainGame.me.save.unlocked_phrases.Contains("@merchant_on_deal_done_4a") && MainGame.me.save.unlocked_phrases.Contains("@merchant_favore"))
		{
			WorldMap.GetWorldGameObjectByObjId("npc_merchant");
			MainGame.me.player.AddToParams("_rel_npc_merchant", 10f);
			UnlockPhrase("@merchant_on_deal_done_4a");
			AddPhraseToBlackList("@merchant_on_deal_done_4a");
			MainGame.me.save.SetTaskState("npc_merchant", "merchant_trade", KnownNPC.TaskState.State.Visible);
			UnlockPhrase("@merchant_traide_license");
		}
		if (num <= 1034 && MainGame.me.save.black_list_of_phrases.Contains("@snake_magic_100"))
		{
			WorldMap.GetWorldGameObjectByObjId("npc_cultist").SetParam("lock_spawn_and_despawn", 0f);
		}
		if (num <= 1034)
		{
			if (MainGame.me.save.unlocked_techs.Contains("Simple gravestones"))
			{
				MainGame.me.save.UnlockCraft("destroy_wd_cross");
				MainGame.me.save.UnlockCraft("destroy_wd_cross_2");
				MainGame.me.save.UnlockCraft("destroy_wd_fence");
				MainGame.me.save.UnlockCraft("destroy_grave_top_stn_plate_1");
			}
			if (MainGame.me.save.unlocked_techs.Contains("Stone gravestones"))
			{
				MainGame.me.save.UnlockCraft("destroy_grave_bot_stn_2");
				MainGame.me.save.UnlockCraft("destroy_grave_top_stella_stn_1");
				MainGame.me.save.UnlockCraft("destroy_grave_top_stn_cross_2");
			}
			if (MainGame.me.save.unlocked_techs.Contains("Grave monuments"))
			{
				MainGame.me.save.UnlockCraft("destroy_grave_top_sculpt_stn_1");
				MainGame.me.save.UnlockCraft("destroy_grave_top_sculpt_stn_2");
			}
		}
		if (num <= 1037 && MainGame.me.save.unlocked_crafts.Contains("garden_builddesk:p:packing_table_place"))
		{
			MainGame.me.save.UnlockCraft("mf_wood_builddesk::elevator_place");
		}
		if (num <= 1037 && MainGame.me.save.unlocked_techs.Contains("Engineer"))
		{
			MainGame.me.save.UnlockCraft("mf_wood_builddesk::well_pump_place");
		}
		if (num < 1100)
		{
			new Scene1037_To_Scene1100().Execute();
		}
		if (num == 1100 || num == 1101)
		{
			WorldGameObject worldGameObjectByCustomTag2 = WorldMap.GetWorldGameObjectByCustomTag("steep_yellow_blockage_R_o");
			if (worldGameObjectByCustomTag2 == null)
			{
				Debug.LogError("Can't fix zombie: steep not found!");
			}
			else if (worldGameObjectByCustomTag2.GetParam("allowed_for_player_interaction").EqualsTo(1f) && worldGameObjectByCustomTag2.GetParam("zombie_was_found").EqualsTo(1f))
			{
				worldGameObjectByCustomTag2.custom_tag = "steep_yellow_blockage_R_o____";
				WorldGameObject worldGameObjectByCustomTag3 = WorldMap.GetWorldGameObjectByCustomTag("steep_yellow_blockage_R_o", ignore_not_found_error: true);
				if (worldGameObjectByCustomTag3 != null)
				{
					worldGameObjectByCustomTag3.DestroyMe();
				}
				worldGameObjectByCustomTag2.custom_tag = "steep_yellow_blockage_R_o";
				worldGameObjectByCustomTag2.hp = 50f;
				worldGameObjectByCustomTag2.AddInteractionEvent("info");
			}
		}
		if (num >= 1100 && num <= 1103)
		{
			GDPoint gDPointByGDTag = WorldMap.GetGDPointByGDTag("gd_zmb_sawmill_decor_place", log_if_null: false, skip_disabled: false);
			if (gDPointByGDTag != null)
			{
				WorldMap.SpawnWGO(MainGame.me.world_root, "zombie_sawmill_decor_object", gDPointByGDTag.pos);
			}
		}
		if (num <= 1106)
		{
			List<WorldGameObject> worldGameObjectsByObjId3 = WorldMap.GetWorldGameObjectsByObjId("lantern_2");
			bool flag2 = false;
			foreach (WorldGameObject item3 in worldGameObjectsByObjId3)
			{
				if (!(item3 == null))
				{
					Vector3 position3 = item3.transform.position;
					if (position3.x.EqualsTo(15928f, 0.1f) && position3.y.EqualsTo(-1848f, 0.1f))
					{
						flag2 = true;
						item3.DestroyMe();
						break;
					}
				}
			}
			if (flag2)
			{
				Debug.Log("Save fix: added \"lantern_2\" to map.");
				WorldMap.SpawnWGO(MainGame.me.world_root, "lantern_2", new Vector3(15944f, -1912f, -396.0533f)).transform.localScale = new Vector3(-1f, 1f, 1f);
			}
		}
		if (num <= 1111)
		{
			foreach (string completed_one_time_craft in completed_one_time_crafts)
			{
				if (completed_one_time_craft.StartsWith("mix:"))
				{
					string text = "mix";
					string[] array = completed_one_time_craft.Split(':');
					foreach (string text2 in array)
					{
						if (!(text2 == "mix") && !text2.StartsWith("mf_alchemy_craft_0") && !string.IsNullOrEmpty(text2) && !(text2 == "_") && !(text2 == "taste_booster"))
						{
							text = text + "_" + text2;
						}
					}
					if (!(text == "mix"))
					{
						UnlockCraft(text);
						Debug.Log("Save fixer: Unlocked zombie alchemy mix craft: " + text);
					}
				}
				else
				{
					if (!completed_one_time_craft.StartsWith("alch:"))
					{
						continue;
					}
					string[] array2 = completed_one_time_craft.Split(':');
					if (array2.Length >= 3)
					{
						string text = "alchemy_";
						switch (array2[1])
						{
						case "mf_alchemy_mill":
							text += "1_";
							break;
						case "mf_alchemy_stirrer_01":
							text += "2_";
							break;
						case "mf_distcube_2_clay":
						case "mf_distcube_2_cuprum":
							text += "3_";
							break;
						default:
							continue;
						}
						text += array2[2];
						UnlockCraft(text);
						Debug.Log("Save fixer: Unlocked zombie alchemy decompose craft: " + text);
					}
				}
			}
		}
		List<WorldGameObject> worldGameObjectsByObjId4 = WorldMap.GetWorldGameObjectsByObjId("zombie_sawmill_completed");
		if (worldGameObjectsByObjId4 != null && worldGameObjectsByObjId4.Count > 0)
		{
			foreach (WorldGameObject item4 in worldGameObjectsByObjId4)
			{
				try
				{
					if (((Vector2)item4.transform.position - new Vector2(1920f, 3744f)).sqrMagnitude < 10f)
					{
						if (!item4.components.craft.is_crafting)
						{
							Debug.LogError("FATAL ERROR: zombie_sawmill_completed has no craft! Fixing in GameSave");
							item4.TryStartCraft("zombie_sawmill_wood_production");
						}
						break;
					}
				}
				catch (Exception message)
				{
					Debug.LogError(message);
				}
			}
		}
		List<WorldGameObject> worldGameObjectsByObjId5 = WorldMap.GetWorldGameObjectsByObjId("mine_zombie_bench");
		if (worldGameObjectsByObjId5 != null && worldGameObjectsByObjId5.Count > 0)
		{
			foreach (WorldGameObject item5 in worldGameObjectsByObjId5)
			{
				try
				{
					Vector2 vector2 = item5.transform.position;
					if (((vector2 - new Vector2(-2560f, 6914f)).sqrMagnitude < 10f || (vector2 - new Vector2(-2254f, 6872f)).sqrMagnitude < 10f) && !item5.components.craft.is_crafting)
					{
						Debug.LogError("FATAL ERROR: mine_zombie_bench has no craft! Fixing in GameSave");
						item5.TryStartCraft("mine_zombie_bench_iron_production");
					}
				}
				catch (Exception message2)
				{
					Debug.LogError(message2);
				}
			}
		}
		List<WorldGameObject> worldGameObjectsByObjId6 = WorldMap.GetWorldGameObjectsByObjId("zombie_mine_fence_left_front");
		if (worldGameObjectsByObjId6 != null && worldGameObjectsByObjId6.Count > 0)
		{
			foreach (WorldGameObject item6 in worldGameObjectsByObjId6)
			{
				try
				{
					if (((Vector2)item6.transform.position - new Vector2(-4128f, 6524f)).sqrMagnitude < 10f && !item6.components.craft.is_crafting)
					{
						Debug.LogError("FATAL ERROR: zombie_mine_fence_left_front has no craft! Fixing in GameSave");
						item6.TryStartCraft("zombie_mine_stone_production");
					}
				}
				catch (Exception message3)
				{
					Debug.LogError(message3);
				}
			}
		}
		List<WorldGameObject> worldGameObjectsByObjId7 = WorldMap.GetWorldGameObjectsByObjId("zombie_mine_fence_front");
		if (worldGameObjectsByObjId7 != null && worldGameObjectsByObjId7.Count > 0)
		{
			foreach (WorldGameObject item7 in worldGameObjectsByObjId7)
			{
				try
				{
					Vector2 vector3 = item7.transform.position;
					if ((vector3 - new Vector2(-3932f, 6622f)).sqrMagnitude < 10f)
					{
						if (!item7.components.craft.is_crafting)
						{
							Debug.LogError("FATAL ERROR: zombie_mine_fence_front has no craft! Fixing in GameSave");
							item7.TryStartCraft("zombie_mine_stone_production");
						}
					}
					else if (((vector3 - new Vector2(-3732f, 6616f)).sqrMagnitude < 10f || (vector3 - new Vector2(-3382f, 6622f)).sqrMagnitude < 10f) && !item7.components.craft.is_crafting)
					{
						Debug.LogError("FATAL ERROR: zombie_mine_fence_front has no craft! Fixing in GameSave");
						item7.TryStartCraft("zombie_mine_marble_production");
					}
				}
				catch (Exception message4)
				{
					Debug.LogError(message4);
				}
			}
		}
		if (num <= 1111)
		{
			bool flag3 = false;
			bool flag4 = false;
			foreach (WorldGameObject item8 in WorldMap.GetWorldGameObjectsByObjId("church_candle"))
			{
				Vector2 vector4 = item8.transform.position;
				if (Mathf.Abs((vector4 - new Vector2(7788f, -10224f)).magnitude) < 1f)
				{
					flag3 = true;
				}
				else if (Mathf.Abs((vector4 - new Vector2(7536f, -10224f)).magnitude) < 1f)
				{
					flag4 = true;
				}
			}
			if (!flag3)
			{
				WorldMap.SpawnWGO(MainGame.me.world_root, "church_candle", new Vector3(7788f, -10224f, -2127.544f));
			}
			if (!flag4)
			{
				WorldMap.SpawnWGO(MainGame.me.world_root, "church_candle", new Vector3(7536f, -10224f, -2127.597f));
			}
		}
		if (num <= 1111 && MainGame.me.save.black_list_of_phrases.Contains("@horadric_2_1h_4b"))
		{
			MainGame.me.save.LockCraft("stamped_meat");
			if (MainGame.me.save.unlocked_crafts.Contains("stamped_meat_new"))
			{
				MainGame.me.save.LockCraft("insert_stamp_into_table");
			}
			else
			{
				MainGame.me.save.UnlockCraft("insert_stamp_into_table");
			}
		}
		if (num <= 1111 && MainGame.me.save.revealed_techs.Contains("Random text generator"))
		{
			MainGame.me.save.RevealHiddenTech("Zombie alchemy");
		}
		if (num <= 1111 && MainGame.me.save.unlocked_techs.Contains("Rules of burning"))
		{
			MainGame.me.save.UnlockCraft("mining_builddesk::lantern_network");
			MainGame.me.save.UnlockCraft("mining_builddesk:p:lantern_place");
		}
		if (num <= 1111)
		{
			GDPoint gDPointByGDTag2 = WorldMap.GetGDPointByGDTag("gd_lantern_2");
			if (gDPointByGDTag2 == null)
			{
				Debug.LogError("FATAL ERROR: not found GDPoint with custom tag \"gd_lantern_2\"!");
			}
			else
			{
				List<WorldGameObject> list = WorldMap.FindWGOs(new string[4] { "lantern_place", "lantern_1_clone", "lantern_2_clone", "lantern_3_clone" }, gDPointByGDTag2.transform.position, string.Empty);
				if (list != null && list.Count > 0)
				{
					Debug.Log("Save fixer: do nothing with lanterns shit");
				}
				else if (MainGame.me.save.completed_one_time_crafts.Contains("mining_builddesk::lantern_network"))
				{
					MainGame.me.save.completed_one_time_crafts.Remove("mining_builddesk::lantern_network");
				}
			}
		}
		WorldGameObject worldGameObjectByCustomTag4 = WorldMap.GetWorldGameObjectByCustomTag("well_pump", ignore_not_found_error: true);
		if (worldGameObjectByCustomTag4 != null)
		{
			try
			{
				if (!worldGameObjectByCustomTag4.components.craft.is_crafting)
				{
					Debug.LogError("FATAL ERROR: well_pump has no craft");
					worldGameObjectByCustomTag4.TryStartCraft("water_pumping");
				}
			}
			catch (Exception message5)
			{
				Debug.LogError(message5);
			}
		}
		WorldGameObject worldGameObjectByObjId3 = WorldMap.GetWorldGameObjectByObjId("refugee_camp_well", ignore_not_found_error: true);
		if (worldGameObjectByObjId3 != null)
		{
			try
			{
				if (!worldGameObjectByObjId3.components.craft.is_crafting)
				{
					Debug.LogError("FATAL ERROR: refugee_camp_well has no craft");
					worldGameObjectByObjId3.TryStartCraft("refugee_well");
				}
			}
			catch (Exception message6)
			{
				Debug.LogError(message6);
			}
		}
		WorldGameObject worldGameObjectByObjId4 = WorldMap.GetWorldGameObjectByObjId("refugee_camp_well_2", ignore_not_found_error: true);
		if (worldGameObjectByObjId4 != null)
		{
			try
			{
				if (!worldGameObjectByObjId4.components.craft.is_crafting)
				{
					Debug.LogError("FATAL ERROR: refugee_camp_well has no craft");
					worldGameObjectByObjId4.TryStartCraft("refugee_well_2");
				}
			}
			catch (Exception message7)
			{
				Debug.LogError(message7);
			}
		}
		foreach (WorldGameObject item9 in WorldMap.GetWorldGameObjectsByObjId("refugee_camp_hive"))
		{
			if (!(item9 != null))
			{
				continue;
			}
			try
			{
				if (!item9.components.craft.is_crafting)
				{
					Debug.LogError("FATAL ERROR: one of refugee_camp_hive has no craft");
					item9.TryStartCraft("refugee_honey_production");
				}
			}
			catch (Exception message8)
			{
				Debug.LogError(message8);
			}
		}
		if (num < 1200)
		{
			if (unlocked_techs.Contains("Zombie logistic"))
			{
				List<WorldGameObject> worldGameObjectsByObjId8 = WorldMap.GetWorldGameObjectsByObjId("wood_obstacle_v");
				if (worldGameObjectsByObjId8 != null && worldGameObjectsByObjId8.Count > 0)
				{
					Vector2 vector5 = new Vector2(5738f, 750f);
					foreach (WorldGameObject item10 in worldGameObjectsByObjId8)
					{
						if (((Vector2)item10.transform.position - vector5).magnitude < 1f)
						{
							UnlockCraft("vineyard_builddesk:p:porter_station");
							Debug.Log("Fix for DLC: unlocked Porter Station building on vineyard zone");
							break;
						}
					}
				}
			}
			List<WorldGameObject> worldGameObjectsByObjId9 = WorldMap.GetWorldGameObjectsByObjId("cellar_builddesk");
			Vector2 vector6 = new Vector2(11112f, -9216f);
			Vector2 vector7 = new Vector2(11136f, -9216f);
			bool flag5 = false;
			foreach (WorldGameObject item11 in worldGameObjectsByObjId9)
			{
				try
				{
					if (((Vector2)item11.transform.position - vector6).magnitude < 1f)
					{
						item11.transform.position = vector7;
						flag5 = true;
					}
				}
				catch (Exception ex)
				{
					Debug.LogError("SaveFixer exception: " + ex);
				}
				if (flag5)
				{
					break;
				}
			}
			WorldMap.SpawnWGO(MainGame.me.world_root, "wall_cellar_1tile", new Vector2(11040f, -9216f)).custom_tag = "cellar_porter_station_2";
			WorldMap.SpawnWGO(MainGame.me.world_root, "teleport_point", new Vector2(9840f, -14720f)).custom_tag = "tp_adam_b";
			WorldMap.SpawnWGO(MainGame.me.world_root, "teleport_inside", new Vector2(9840f, -14745f)).custom_tag = "tp_adam_a_";
			WorldMap.SpawnWGO(MainGame.me.world_root, "teleport_point", new Vector2(14049.6f, -2829.6f)).custom_tag = "tp_adam_a";
			WorldMap.SpawnWGO(MainGame.me.world_root, "teleport_outside", new Vector2(14049.6f, -2793.6f)).custom_tag = "tp_adam_b_";
			WorldGameObject worldGameObject = WorldMap.SpawnWGO(MainGame.me.world_root, "chest", new Vector2(9608f, -14424f));
			worldGameObject.custom_tag = "adams_house_chest";
			worldGameObject.AddToInventory("tr_key", 1);
			worldGameObject.AddToInventory("ceramic_1", 20);
			worldGameObject.AddToInventory("ceramic_2", 5);
			worldGameObject.AddToInventory("ceramic_3", 3);
			worldGameObject.AddToInventory("meal:beet_slice", 1);
			worldGameObject.AddToInventory("dessert:jelly_red", 3);
			worldGameObject.AddToInventory("bottle_berry_juice", 2);
			worldGameObject.AddToInventory("hammer_0", 1);
			WorldMap.SpawnWGO(MainGame.me.world_root, "teleport_point", new Vector2(19398f, -8760f)).custom_tag = "tp_tavern_from_cellar_b";
			WorldMap.SpawnWGO(MainGame.me.world_root, "teleport_micro", new Vector2(19398f, -8688f)).custom_tag = "tp_tavern_to_cellar_b_";
			WorldMap.SpawnWGO(MainGame.me.world_root, "teleport_point", new Vector2(16944f, -8928f)).custom_tag = "tp_tavern_to_cellar_b";
			WorldMap.SpawnWGO(MainGame.me.world_root, "teleport_inside", new Vector2(16944f, -8976f)).custom_tag = "tp_tavern_from_cellar_b_";
			WorldMap.SpawnWGO(MainGame.me.world_root, "players_tavern_builddesk", new Vector2(19872f, -8736f)).custom_tag = "players_tavern_builddesk";
			WorldMap.SpawnWGO(MainGame.me.world_root, "players_tavern_cellar_builddesk", new Vector2(16800f, -8736f)).custom_tag = "players_tavern_cellar_builddesk";
			WorldMap.SpawnWGO(MainGame.me.world_root, "teleport_inside", new Vector2(12912f, -15312f)).custom_tag = "tp_farmer_a_";
			WorldMap.SpawnWGO(MainGame.me.world_root, "teleport_point", new Vector2(12912f, -15312f)).custom_tag = "tp_farmer_b";
			WorldMap.SpawnWGO(MainGame.me.world_root, "teleport_outside", new Vector2(12420f, -3684f)).custom_tag = "tp_farmer_b_";
			WorldMap.SpawnWGO(MainGame.me.world_root, "teleport_point", new Vector2(12420f, -3684f)).custom_tag = "tp_farmer_a";
			WorldGameObject worldGameObject2 = WorldMap.SpawnWGO(MainGame.me.world_root, "chest", new Vector2(12734f, -14424f));
			worldGameObject2.custom_tag = "farmers_house_chest";
			worldGameObject2.AddToInventory("tr_helmet", 1);
			worldGameObject2.AddToInventory("onion_seed:2", 20);
			worldGameObject2.AddToInventory("carrot_seed", 14);
			worldGameObject2.AddToInventory("pumpkin_crop:2", 16);
			worldGameObject2.AddToInventory("lentils_crop:2", 12);
			worldGameObject2.AddToInventory("meal:soup_red_yellow:2", 3);
			worldGameObject2.AddToInventory("meal:soup_yellow_green:2", 1);
			worldGameObject2.AddToInventory("meal:baked_pumpkin:3", 2);
			worldGameObject2.AddToInventory("sack_clock_gold", 6);
			worldGameObject2.AddToInventory("sack_star_gold", 4);
			WorldMap.SpawnWGO(MainGame.me.world_root, "tavern_kitchen", new Vector2(17184f, -8832f)).custom_tag = "tavern_kitchen";
			WorldMap.SpawnWGO(MainGame.me.world_root, "tavern_oven", new Vector2(17184f, -8832f)).custom_tag = "tavern_oven";
			WorldMap.SpawnWGO(MainGame.me.world_root, "tavern_time_machin_wall_inactive", new Vector2(16176f, -9024f), "tavern_time_machine");
			WorldMap.SpawnWGO(MainGame.me.world_root, "tavern_cellar_rack", new Vector2(16588f, -9090f), "tavern_cellar_rack");
			List<string> list2 = new List<string>();
			foreach (string unlocked_craft in unlocked_crafts)
			{
				CraftDefinition data = GameBalance.me.GetData<CraftDefinition>(unlocked_craft);
				if (data != null && (data.craft_in.Contains("oven") || data.craft_in.Contains("cooking_table") || data.craft_in.Contains("cooking_table_2") || data.craft_in.Contains("cooking_bonfire")))
				{
					string text3 = "t_" + unlocked_craft;
					if (GameBalance.me.GetDataOrNull<CraftDefinition>(text3) != null)
					{
						list2.Add(text3);
					}
				}
			}
			if (list2.Count > 0)
			{
				unlocked_crafts.AddRange(list2);
			}
			WorldGameObject worldGameObject3 = null;
			Vector2 vector8 = new Vector2(11280f, -1152f);
			foreach (WorldGameObject item12 in WorldMap.GetWorldGameObjectsByObjId("grave_ground"))
			{
				if (((Vector2)item12.transform.position - vector8).sqrMagnitude < 1f)
				{
					worldGameObject3 = item12;
					break;
				}
			}
			if (worldGameObject3 == null)
			{
				Debug.Log("Not found Bella's grave in the world.");
			}
			else
			{
				Item bodyFromInventory = worldGameObject3.GetBodyFromInventory();
				if (bodyFromInventory != null && bodyFromInventory.IsNotEmpty())
				{
					if (bodyFromInventory.inventory_size < 99)
					{
						bodyFromInventory.inventory_size = 99;
					}
					if (bodyFromInventory.inventory.Count == 0 || bodyFromInventory.inventory_size <= bodyFromInventory.inventory.Count)
					{
						string[] array = new string[8] { "skull", "bone", "flesh", "blood", "skin", "brain:brain_4_1", "heart:heart_4_1", "intestine:intestine_0_0" };
						for (int k = 0; k < array.Length; k++)
						{
							Item item = new Item(array[k], 1);
							if (item?.definition != null)
							{
								bodyFromInventory.AddItem(item);
							}
						}
						Debug.Log("SaveFixer: fixed Bella's body");
					}
				}
			}
		}
		if (num < 1123)
		{
			foreach (WorldGameObject item13 in WorldMap.GetWorldGameObjectsByObjId("grave_ground"))
			{
				Item item2 = item13.data.inventory.Find((Item body) => body.id == "body");
				if (item2 != null)
				{
					item2.inventory_size = 99;
				}
			}
		}
		if (num < 1202)
		{
			GDPoint[] componentsInChildren = MainGame.me.world_root.GetComponentsInChildren<GDPoint>(includeInactive: true);
			for (int k = 0; k < componentsInChildren.Length; k++)
			{
				WorldGameObject[] componentsInParent = componentsInChildren[k].GetComponentsInParent<WorldGameObject>(includeInactive: true);
				if (componentsInParent != null && componentsInParent.Length != 0)
				{
					componentsInParent[0].Redraw(force_redraw: false, force_redraw_part: true);
					Debug.Log("Fixed object with GDPoint inside: " + componentsInParent[0].name);
				}
			}
			GJTimer.AddTimer(0.1f, delegate
			{
				WorldMap.RescanGDPoints();
			});
		}
		if (num < 1202 && unlocked_techs.Contains("Zombie logistic") && !unlocked_crafts.Contains("vineyard_builddesk:p:porter_station"))
		{
			List<WorldGameObject> worldGameObjectsByObjId10 = WorldMap.GetWorldGameObjectsByObjId("wood_obstacle_v");
			bool flag6 = false;
			if (worldGameObjectsByObjId10 != null && worldGameObjectsByObjId10.Count > 0)
			{
				Vector2 vector9 = new Vector2(5738f, 750f);
				foreach (WorldGameObject item14 in worldGameObjectsByObjId10)
				{
					if (((Vector2)item14.transform.position - vector9).magnitude < 1f)
					{
						flag6 = true;
						break;
					}
				}
			}
			if (!flag6)
			{
				UnlockCraft("vineyard_builddesk:p:porter_station");
				Debug.Log("Fix for DLC: unlocked Porter Station building on vineyard zone");
			}
		}
		if (num < 1205 && revealed_techs.Contains("Zombie mining"))
		{
			RevealHiddenTech("Zombie vineyard");
			RevealHiddenTech("Zombie brewing");
			RevealHiddenTech("Zombie winemaking");
			Debug.Log("Revealed zombie technologies: Zombie vineyard, Zombie brewing, Zombie winemaking");
		}
		WorldGameObject worldGameObjectByCustomTag5 = WorldMap.GetWorldGameObjectByCustomTag("refugee_camp_cooking_table", ignore_not_found_error: true);
		if (worldGameObjectByCustomTag5 != null)
		{
			worldGameObjectByCustomTag5.GetComponent<ChunkedGameObject>().always_active = true;
			worldGameObjectByCustomTag5.SetActive(active: true);
		}
		if (unlocked_techs.Contains("Persistence") && !buffs.Exists((PlayerBuff p) => p.buff_id == "buff_dlc_refugee_persistence"))
		{
			BuffsLogics.AddBuff("buff_dlc_refugee_persistence");
		}
		if (num <= 1301)
		{
			GameSave save = MainGame.me.save;
			_ = MainGame.me.save.achievements;
			if (save.unlocked_phrases.Contains("@zone_refugees_camp_tp"))
			{
				string id = "dlc_refugees_s1";
				AchievementDefinition dataOrNull = GameBalance.me.GetDataOrNull<AchievementDefinition>(id);
				if (dataOrNull != null)
				{
					PlatformSpecific.OnAchievementComplete(dataOrNull);
				}
			}
			KnownNPC nPC = known_npcs.GetNPC("npc_tavern owner");
			if (nPC != null)
			{
				KnownNPC.TaskState.State questState = nPC.GetQuestState("s_ev_5_find_the_vampire");
				if (questState == KnownNPC.TaskState.State.Complete || questState == KnownNPC.TaskState.State.Visible)
				{
					string id = "dlc_refugees_s5";
					AchievementDefinition dataOrNull = GameBalance.me.GetDataOrNull<AchievementDefinition>(id);
					if (dataOrNull != null)
					{
						PlatformSpecific.OnAchievementComplete(dataOrNull);
					}
				}
			}
			nPC = known_npcs.GetNPC("npc_master_alarich");
			if (nPC != null)
			{
				KnownNPC.TaskState.State questState = nPC.GetQuestState("s_ev_9_2_night_wait");
				if (questState == KnownNPC.TaskState.State.Complete)
				{
					string id = "dlc_refugees_s10";
					AchievementDefinition dataOrNull = GameBalance.me.GetDataOrNull<AchievementDefinition>(id);
					if (dataOrNull != null)
					{
						PlatformSpecific.OnAchievementComplete(dataOrNull);
					}
				}
			}
			nPC = known_npcs.GetNPC("npc_master_alarich");
			if (nPC != null)
			{
				KnownNPC.TaskState.State questState = nPC.GetQuestState("s_ev_12_details");
				if (questState == KnownNPC.TaskState.State.Visible || questState == KnownNPC.TaskState.State.Complete)
				{
					string id = "dlc_refugees_s12";
					AchievementDefinition dataOrNull = GameBalance.me.GetDataOrNull<AchievementDefinition>(id);
					if (dataOrNull != null)
					{
						PlatformSpecific.OnAchievementComplete(dataOrNull);
					}
				}
			}
			nPC = known_npcs.GetNPC("npc_marquis_teodoro_jr");
			if (nPC != null)
			{
				KnownNPC.TaskState.State questState = nPC.GetQuestState("s_ev_15_1_teodoro_inform");
				if (questState == KnownNPC.TaskState.State.Visible || questState == KnownNPC.TaskState.State.Complete)
				{
					string id = "dlc_refugees_s15_1";
					AchievementDefinition dataOrNull = GameBalance.me.GetDataOrNull<AchievementDefinition>(id);
					if (dataOrNull != null)
					{
						PlatformSpecific.OnAchievementComplete(dataOrNull);
					}
				}
			}
			nPC = known_npcs.GetNPC("npc_master_alarich");
			if (nPC != null)
			{
				KnownNPC.TaskState.State questState = nPC.GetQuestState("s_ev_12_bring_blood");
				if (questState == KnownNPC.TaskState.State.Complete)
				{
					string id = "dlc_refugees_s15_2";
					AchievementDefinition dataOrNull = GameBalance.me.GetDataOrNull<AchievementDefinition>(id);
					if (dataOrNull != null)
					{
						PlatformSpecific.OnAchievementComplete(dataOrNull);
					}
				}
			}
			if (save.black_list_of_phrases.Contains("@refugees_s15_3"))
			{
				string id = "dlc_refugees_s15_3";
				AchievementDefinition dataOrNull = GameBalance.me.GetDataOrNull<AchievementDefinition>(id);
				if (dataOrNull != null)
				{
					PlatformSpecific.OnAchievementComplete(dataOrNull);
				}
			}
			if (save.unlocked_phrases.Contains("@refugees_s19_1"))
			{
				string id = "dlc_refugees_s18";
				AchievementDefinition dataOrNull = GameBalance.me.GetDataOrNull<AchievementDefinition>(id);
				if (dataOrNull != null)
				{
					PlatformSpecific.OnAchievementComplete(dataOrNull);
				}
			}
			if (save.unlocked_phrases.Contains("@refugees_s24_1"))
			{
				string id = "dlc_refugees_s23";
				AchievementDefinition dataOrNull = GameBalance.me.GetDataOrNull<AchievementDefinition>(id);
				if (dataOrNull != null)
				{
					PlatformSpecific.OnAchievementComplete(dataOrNull);
				}
			}
			if (MainGame.me.player.GetParamInt("ev_28_golem_killed_count") >= 3)
			{
				string id = "dlc_refugees_s28";
				AchievementDefinition dataOrNull = GameBalance.me.GetDataOrNull<AchievementDefinition>(id);
				if (dataOrNull != null)
				{
					PlatformSpecific.OnAchievementComplete(dataOrNull);
				}
			}
			if (save.unlocked_phrases.Contains("@refugee_s34"))
			{
				string id = "dlc_refugees_s33";
				AchievementDefinition dataOrNull = GameBalance.me.GetDataOrNull<AchievementDefinition>(id);
				if (dataOrNull != null)
				{
					PlatformSpecific.OnAchievementComplete(dataOrNull);
				}
			}
			if (MainGame.me.player.GetParamInt("event_s38_complete") == 1)
			{
				string id = "\tdlc_refugees_s37";
				AchievementDefinition dataOrNull = GameBalance.me.GetDataOrNull<AchievementDefinition>(id);
				if (dataOrNull != null)
				{
					PlatformSpecific.OnAchievementComplete(dataOrNull);
				}
			}
			if (save.black_list_of_phrases.Contains("refugee_s40_12"))
			{
				string id = "dlc_refugees_s40";
				AchievementDefinition dataOrNull = GameBalance.me.GetDataOrNull<AchievementDefinition>(id);
				if (dataOrNull != null)
				{
					PlatformSpecific.OnAchievementComplete(dataOrNull);
				}
			}
			if (save.black_list_of_phrases.Contains("@refugee_s45") || save.black_list_of_phrases.Contains("@refugee_s53"))
			{
				string id = "dlc_refugees_s46_or_s53";
				AchievementDefinition dataOrNull = GameBalance.me.GetDataOrNull<AchievementDefinition>(id);
				if (dataOrNull != null)
				{
					PlatformSpecific.OnAchievementComplete(dataOrNull);
				}
			}
			nPC = known_npcs.GetNPC("donkey");
			if (nPC != null)
			{
				KnownNPC.TaskState.State questState = nPC.GetQuestState("dlc_refugees_s_ev_d1_box");
				if (questState == KnownNPC.TaskState.State.Visible || questState == KnownNPC.TaskState.State.Complete)
				{
					string id = "dlc_refugees_d1";
					AchievementDefinition dataOrNull = GameBalance.me.GetDataOrNull<AchievementDefinition>(id);
					if (dataOrNull != null)
					{
						PlatformSpecific.OnAchievementComplete(dataOrNull);
					}
				}
			}
			nPC = known_npcs.GetNPC("donkey");
			if (nPC != null)
			{
				KnownNPC.TaskState.State questState = nPC.GetQuestState("dlc_refugees_s_ev_d3_aphorisms");
				if (questState == KnownNPC.TaskState.State.Visible || questState == KnownNPC.TaskState.State.Complete)
				{
					string id = "dlc_refugees_d3";
					AchievementDefinition dataOrNull = GameBalance.me.GetDataOrNull<AchievementDefinition>(id);
					if (dataOrNull != null)
					{
						PlatformSpecific.OnAchievementComplete(dataOrNull);
					}
				}
			}
			nPC = known_npcs.GetNPC("donkey");
			if (nPC != null)
			{
				KnownNPC.TaskState.State questState = nPC.GetQuestState("dlc_refugees_s_ev_d6_battle_supply");
				if (questState == KnownNPC.TaskState.State.Visible || questState == KnownNPC.TaskState.State.Complete)
				{
					string id = "dlc_refugees_d6";
					AchievementDefinition dataOrNull = GameBalance.me.GetDataOrNull<AchievementDefinition>(id);
					if (dataOrNull != null)
					{
						PlatformSpecific.OnAchievementComplete(dataOrNull);
					}
				}
			}
			nPC = known_npcs.GetNPC("donkey");
			if (nPC != null)
			{
				KnownNPC.TaskState.State questState = nPC.GetQuestState("dlc_refugees_s_ev_d11_cans");
				if (questState == KnownNPC.TaskState.State.Visible || questState == KnownNPC.TaskState.State.Complete)
				{
					string id = "dlc_refugees_d10";
					AchievementDefinition dataOrNull = GameBalance.me.GetDataOrNull<AchievementDefinition>(id);
					if (dataOrNull != null)
					{
						PlatformSpecific.OnAchievementComplete(dataOrNull);
					}
				}
			}
			WorldGameObject worldGameObjectByCustomTag6 = WorldMap.GetWorldGameObjectByCustomTag("donkey");
			if (worldGameObjectByCustomTag6 != null && worldGameObjectByCustomTag6.GetParamInt("is_d16_completed") == 1)
			{
				string id = "dlc_refugees_d16";
				AchievementDefinition dataOrNull = GameBalance.me.GetDataOrNull<AchievementDefinition>(id);
				if (dataOrNull != null)
				{
					PlatformSpecific.OnAchievementComplete(dataOrNull);
				}
			}
			if (MainGame.me.player.GetParamInt("has_witchers_eye") == 1)
			{
				string id = "dlc_refugees_city_1_fin";
				AchievementDefinition dataOrNull = GameBalance.me.GetDataOrNull<AchievementDefinition>(id);
				if (dataOrNull != null)
				{
					PlatformSpecific.OnAchievementComplete(dataOrNull);
				}
			}
			if (MainGame.me.player.GetParamInt("is_citybuilder_fin_2_completed") == 1)
			{
				string id = "dlc_refugees_city_2_fin";
				AchievementDefinition dataOrNull = GameBalance.me.GetDataOrNull<AchievementDefinition>(id);
				if (dataOrNull != null)
				{
					PlatformSpecific.OnAchievementComplete(dataOrNull);
				}
			}
			if (MainGame.me.player.GetParamInt("event_city_3_3_complete") == 1)
			{
				string id = "dlc_refugees_city_3_fin";
				AchievementDefinition dataOrNull = GameBalance.me.GetDataOrNull<AchievementDefinition>(id);
				if (dataOrNull != null)
				{
					PlatformSpecific.OnAchievementComplete(dataOrNull);
				}
			}
			if (save.unlocked_phrases.Contains("@cook_vendor_inventory_1"))
			{
				string id = "dlc_refugees_cook";
				AchievementDefinition dataOrNull = GameBalance.me.GetDataOrNull<AchievementDefinition>(id);
				if (dataOrNull != null)
				{
					PlatformSpecific.OnAchievementComplete(dataOrNull);
				}
			}
			if (save.unlocked_phrases.Contains("@advanced_gravestones"))
			{
				string id = "dlc_refugees_undertaker";
				AchievementDefinition dataOrNull = GameBalance.me.GetDataOrNull<AchievementDefinition>(id);
				if (dataOrNull != null)
				{
					PlatformSpecific.OnAchievementComplete(dataOrNull);
				}
			}
			if (save.unlocked_phrases.Contains("@tech_bag_alchemy"))
			{
				string id = "dlc_refugees_tanner";
				AchievementDefinition dataOrNull = GameBalance.me.GetDataOrNull<AchievementDefinition>(id);
				if (dataOrNull != null)
				{
					PlatformSpecific.OnAchievementComplete(dataOrNull);
				}
			}
		}
		if (num <= 1301 && MainGame.me.save.black_list_of_phrases.Contains("@refugees_s17"))
		{
			MainGame.me.save.UnlockPhrase("@buy_memory_powder");
		}
		if (num <= 1302)
		{
			KnownNPC nPC2 = known_npcs.GetNPC("donkey");
			if (nPC2 != null)
			{
				KnownNPC.TaskState.State questState2 = nPC2.GetQuestState("s_ev_22_make_amulet");
				KnownNPC.TaskState.State questState3 = nPC2.GetQuestState("s_ev_21_take_hair");
				if ((questState2 == KnownNPC.TaskState.State.Complete || questState2 == KnownNPC.TaskState.State.Visible) && questState3 == KnownNPC.TaskState.State.Visible)
				{
					nPC2.SetQuestState("s_ev_21_take_hair", KnownNPC.TaskState.State.Complete);
				}
			}
		}
		if (num <= 1302)
		{
			KnownNPC nPC3 = known_npcs.GetNPC("npc_marquis_teodoro_jr");
			if (nPC3 != null && nPC3.GetQuestState("dlc_refugees_s_ev_68_teodoro") == KnownNPC.TaskState.State.Visible && MainGame.me.player.GetParamInt("is_need_activate_ev_s51") == 0)
			{
				GS.RunFlowScript("refugee_ev_s51_prepare");
			}
		}
		if (num <= 1303)
		{
			GDPoint gDPointByGDTag3 = WorldMap.GetGDPointByGDTag("gd_refugees_camp_garden");
			if (gDPointByGDTag3 != null && !gDPointByGDTag3.IsDisabled())
			{
				List<WorldGameObject> list3 = WorldMap.FindWGOs(new string[1] { "bush_6" }, new Vector2(3744f, 4428f), string.Empty);
				if (list3 != null && list3.Count != 0)
				{
					list3[0].DestroyMe();
				}
			}
		}
		if (num <= 1304)
		{
			foreach (string unlocked_tech in unlocked_techs)
			{
				TechDefinition data2 = GameBalance.me.GetData<TechDefinition>(unlocked_tech);
				if (data2 != null && data2.invisible && !visible_techs.Contains(unlocked_tech))
				{
					visible_techs.Add(unlocked_tech);
				}
			}
		}
		if (num <= 1304)
		{
			WorldGameObject worldGameObjectByObjId5 = WorldMap.GetWorldGameObjectByObjId("golem_ev_28_red", ignore_not_found_error: true);
			if (worldGameObjectByObjId5 != null)
			{
				GDPoint gDPointByGDTag4 = WorldMap.GetGDPointByGDTag("refugee_s28_golem_red_anchor");
				worldGameObjectByObjId5.transform.position = gDPointByGDTag4.transform.position;
			}
		}
		if (num <= 1304)
		{
			KnownNPC nPC4 = known_npcs.GetNPC("npc_marquis_teodoro_jr");
			KnownNPC nPC5 = known_npcs.GetNPC("npc_master_alarich");
			if (nPC4 != null && nPC5 != null && nPC5.GetQuestState("s_ev_8_alarich") == KnownNPC.TaskState.State.Complete)
			{
				nPC4.SetQuestState("s_ev_8_teodoro", KnownNPC.TaskState.State.Complete);
			}
		}
		if (num <= 1304)
		{
			KnownNPC nPC6 = known_npcs.GetNPC("npc_hunchback");
			if (nPC6 != null && MainGame.me.save.black_list_of_phrases.Contains("@refugee_s42_1_3"))
			{
				nPC6.SetQuestState("s_ev_41_koukol", KnownNPC.TaskState.State.Complete);
			}
		}
		WorldGameObject worldGameObjectByObjId6 = WorldMap.GetWorldGameObjectByObjId("church_big_quality_obj", ignore_not_found_error: true);
		if (worldGameObjectByObjId6 != null)
		{
			worldGameObjectByObjId6.DestroyMe();
			Debug.Log("WGO church_big_quality_obj was destroyed");
		}
		if (num <= 1305)
		{
			WorldGameObject worldGameObjectByCustomTag7 = WorldMap.GetWorldGameObjectByCustomTag("npc_mrs chain");
			GDPoint gDPointByGDTag5 = WorldMap.GetGDPointByGDTag("refugee_s34_chain_point_1");
			if (worldGameObjectByCustomTag7 != null && gDPointByGDTag5 != null && worldGameObjectByCustomTag7.GetParamInt("s34_gd_zone_active") == 1)
			{
				worldGameObjectByCustomTag7.SetParam("is_busy", 1f);
				if (worldGameObjectByCustomTag7.transform.position != gDPointByGDTag5.transform.position)
				{
					worldGameObjectByCustomTag7.components.character?.StopMovement();
					worldGameObjectByCustomTag7.TeleportToGDPoint("refugee_s34_chain_point_1");
					worldGameObjectByCustomTag7.components.character?.LookAt(Direction.Right);
				}
			}
		}
		if (num <= 1307)
		{
			KnownNPC nPC7 = known_npcs.GetNPC("npc_marquis_teodoro_jr");
			if (MainGame.me.save.black_list_of_phrases.Contains("@refugees_s26_1_ready"))
			{
				nPC7?.SetQuestState("s_ev_26_3_teodoro_ask", KnownNPC.TaskState.State.Complete);
			}
		}
		foreach (WorldGameObject item15 in WorldMap.GetWorldGameObjectsByObjId("worker_invisible"))
		{
			if (item15.linked_workbench == null)
			{
				Debug.Log("Destroyed invisible worker without workbench: " + item15);
				item15.DestroyMe();
			}
		}
		List<WorldGameObject> worldGameObjectsByObjId11 = WorldMap.GetWorldGameObjectsByObjId("refugee_camp_garden_bed_1");
		worldGameObjectsByObjId11.AddRange(WorldMap.GetWorldGameObjectsByObjId("refugee_camp_garden_bed_2"));
		worldGameObjectsByObjId11.AddRange(WorldMap.GetWorldGameObjectsByObjId("refugee_camp_garden_bed_3"));
		foreach (WorldGameObject item16 in worldGameObjectsByObjId11)
		{
			if (!item16.has_linked_worker)
			{
				if (!item16.gameObject.activeSelf)
				{
					item16.gameObject.SetActive(value: true);
				}
				WorldGameObject worker_wgo;
				bool flag7 = WorldMap.AttachInvisibleWorker(item16, out worker_wgo);
				Debug.Log("Reattached Invisible worker for: " + item16?.ToString() + ", is success: " + flag7);
			}
		}
		if (num <= 1309)
		{
			GDPoint gDPointByGDTag6 = WorldMap.GetGDPointByGDTag("gd_flat_under_waterflow_3_before_refugee");
			GDPoint gDPointByGDTag7 = WorldMap.GetGDPointByGDTag("gd_refugee_buildzone");
			if (MainGame.me.save.unlocked_phrases.Contains("@zone_refugees_camp_tp"))
			{
				if (gDPointByGDTag6.gameObject.activeSelf)
				{
					WorldZone.GetZoneByID("flat_under_waterflow_3").DisableWorldZone();
					gDPointByGDTag6.gameObject.SetActive(value: false);
				}
				gDPointByGDTag7.gameObject.SetActive(value: true);
				WorldZone.GetZoneByID("refugees_camp").EnableWorldZone();
			}
			else
			{
				if (gDPointByGDTag7.gameObject.activeSelf)
				{
					WorldZone.GetZoneByID("refugees_camp").DisableWorldZone();
					gDPointByGDTag7.gameObject.SetActive(value: false);
				}
				gDPointByGDTag6.gameObject.SetActive(value: true);
				WorldZone.GetZoneByID("flat_under_waterflow_3").EnableWorldZone();
			}
			MainGame.me.save.black_list_of_phrases.Remove("mailbox_aristocrat_paper");
			for (int l = 0; l < DropsList.me.drops.Count; l++)
			{
				DropResGameObject dropResGameObject = DropsList.me.drops[l];
				if (dropResGameObject.res.id == "stone" || dropResGameObject.res.id == "marble")
				{
					Vector2 vector10 = dropResGameObject.transform.position;
					if (vector10.x >= -5096f && vector10.x <= -3075f && vector10.y >= 6680f && vector10.y <= 19476f)
					{
						string id2 = dropResGameObject.res.id;
						Vector2 vector11 = vector10;
						Debug.Log("Destroy drop: " + id2 + " with position: " + vector11.ToString());
						UnityEngine.Object.Destroy(dropResGameObject);
						DropsList.me.drops.Remove(dropResGameObject);
						l--;
					}
				}
			}
		}
		if (num <= 1310)
		{
			WorldMap.SpawnWGO(MainGame.me.world_root, "souls_zone_wall_closed", new Vector2(10752f, -11040f), "souls_zone_wall_closed");
			WorldMap.SpawnWGO(MainGame.me.world_root, "smilers_box_closed", new Vector2(11280f, -10808f));
			WorldMap.SpawnWGO(MainGame.me.world_root, "pile_of_broken_glass_3", new Vector2(11054f, -10910f));
			WorldMap.SpawnWGO(MainGame.me.world_root, "pile_of_broken_glass_2", new Vector2(12038f, -11472f));
			WorldMap.SpawnWGO(MainGame.me.world_root, "pile_of_broken_glass_1", new Vector2(11970f, -10966f));
			WorldMap.SpawnWGO(MainGame.me.world_root, "pile_of_broken_glass_6", new Vector2(11618f, -11336f));
			WorldMap.SpawnWGO(MainGame.me.world_root, "pile_of_broken_glass_5", new Vector2(12376f, -10918f));
			WorldMap.SpawnWGO(MainGame.me.world_root, "pile_of_broken_glass_4", new Vector2(11088f, -11502f));
			WorldMap.SpawnWGO(MainGame.me.world_root, "hatch_rust", new Vector2(11520f, -10944f));
			WorldMap.SpawnWGO(MainGame.me.world_root, "soul_healer_broken", new Vector2(12192f, -11092f));
			WorldMap.SpawnWGO(MainGame.me.world_root, "souls_builddesk", new Vector2(11058f, -10748f));
			WorldMap.SpawnWGO(MainGame.me.world_root, "soul_portal_broken", new Vector2(12078f, -11344f));
			WorldMap.SpawnWGO(MainGame.me.world_root, "soul_extractor_broken", new Vector2(11334f, -11334f), "soul_extractor");
			WorldMap.SpawnWGO(MainGame.me.world_root, "candelabrum_3_3_souls", new Vector2(12332f, -11518f));
			WorldMap.SpawnWGO(MainGame.me.world_root, "dungeon_source_diamond", new Vector2(11802f, -11514f));
			WorldMap.SpawnWGO(MainGame.me.world_root, "eurics_room_old_bed", new Vector2(20064f, -11904f));
			WorldMap.SpawnWGO(MainGame.me.world_root, "eurics_room_table_broken", new Vector2(20516f, -11536f));
			WorldMap.SpawnWGO(MainGame.me.world_root, "eurics_room_woodbox_abandoned", new Vector2(20304f, -11544f));
			WorldMap.SpawnWGO(MainGame.me.world_root, "eurics_room_side_rack_destroyed", new Vector2(20568f, -11808f));
			WorldMap.SpawnWGO(MainGame.me.world_root, "eurics_room_front_wardrobe", new Vector2(20208f, -12048f), "eurics_room_front_wardrobe_left");
			WorldMap.SpawnWGO(MainGame.me.world_root, "eurics_room_front_wardrobe", new Vector2(20400f, -12048f), "eurics_room_front_wardrobe_right");
			WorldMap.SpawnWGO(MainGame.me.world_root, "eurics_room_carpet_destroyed", new Vector2(20208f, -11928f)).SetVariationByIndex(0);
			WorldMap.SpawnWGO(MainGame.me.world_root, "stained_glass_window", new Vector2(20254f, -11806f));
			WorldMap.SpawnWGO(MainGame.me.world_root, "candelabrum_2_1", new Vector2(20366f, -11818f));
			WorldMap.SpawnWGO(MainGame.me.world_root, "teleport_inside_euric_room", new Vector2(20110f, -11530f), "tp_mortuary_from_euric_b_");
			WorldMap.SpawnWGO(MainGame.me.world_root, "teleport_point", new Vector2(20110f, -11582f), "tp_euric_from_mortuary_b");
			WorldMap.SpawnWGO(MainGame.me.world_root, "teleport_point", new Vector2(11424f, -10944f), "tp_mortuary_from_euric_b");
		}
		if (num <= 1312)
		{
			WorldMap.SpawnWGO(MainGame.me.world_root, "cow", new Vector2(16070f, 446f));
			WorldMap.SpawnWGO(MainGame.me.world_root, "drying_rack_repaired", new Vector2(15552f, -3048f));
		}
		if (num <= 1314)
		{
			WorldMap.SpawnWGO(MainGame.me.world_root, "keeper_room_builddesk", new Vector2(3276f, -6166f));
			WorldMap.SpawnWGO(MainGame.me.world_root, "keeper_room_carpet", new Vector2(2496f, -6312f));
			if (MainGame.me.save.unlocked_techs.Contains("The Beginning Of Alchemy"))
			{
				MainGame.me.save.UnlockCraft("keeper_room_builddesk:p:keeper_room_carpet_blue_1");
				MainGame.me.save.UnlockCraft("keeper_room_builddesk:p:keeper_room_carpet_brown_1");
				MainGame.me.save.UnlockCraft("keeper_room_builddesk:p:keeper_room_carpet_green_1");
				MainGame.me.save.UnlockCraft("keeper_room_builddesk:p:keeper_room_carpet_green_2");
				MainGame.me.save.UnlockCraft("keeper_room_builddesk:p:keeper_room_carpet_red_1");
				MainGame.me.save.UnlockCraft("keeper_room_builddesk:p:keeper_room_carpet_violet_1");
				MainGame.me.save.UnlockCraft("keeper_room_builddesk:p:keeper_room_carpet_violet_2");
				MainGame.me.save.UnlockCraft("keeper_room_builddesk:p:keeper_room_carpet_yellow_1");
				MainGame.me.save.UnlockCraft("keeper_room_builddesk::keeper_room_bed_blue_1");
				MainGame.me.save.UnlockCraft("keeper_room_builddesk::keeper_room_bed_brown_1");
				MainGame.me.save.UnlockCraft("keeper_room_builddesk::keeper_room_bed_green_1");
				MainGame.me.save.UnlockCraft("keeper_room_builddesk::keeper_room_bed_green_2");
				MainGame.me.save.UnlockCraft("keeper_room_builddesk::keeper_room_bed_red_1");
				MainGame.me.save.UnlockCraft("keeper_room_builddesk::keeper_room_bed_violet_1");
				MainGame.me.save.UnlockCraft("keeper_room_builddesk::keeper_room_bed_violet_2");
				MainGame.me.save.UnlockCraft("keeper_room_builddesk::keeper_room_bed_yellow_1");
				MainGame.me.save.UnlockCraft("keeper_room_builddesk:p:keeper_room_picture_1");
			}
			if (MainGame.me.player.GetParam("met_donkey") >= 1f)
			{
				MainGame.me.save.UnlockCraft("keeper_room_builddesk:p:keeper_room_picture_7");
			}
			if (MainGame.me.player.GetParam("met_bishop") >= 1f)
			{
				MainGame.me.save.UnlockCraft("keeper_room_builddesk:p:keeper_room_picture_4");
			}
			if (MainGame.me.player.GetParam("met_inquisitor") >= 1f)
			{
				MainGame.me.save.UnlockCraft("keeper_room_builddesk:p:keeper_room_picture_5");
			}
			if (WorldMap.GetWorldGameObjectByObjId("npc_actress").GetParam("met_inquisitor") >= 1f)
			{
				MainGame.me.save.UnlockCraft("keeper_room_builddesk:p:keeper_room_picture_6");
			}
		}
		if (num <= 1315)
		{
			WorldGameObject worldGameObjectByObjId7 = WorldMap.GetWorldGameObjectByObjId("soul_extractor_broken");
			if (worldGameObjectByObjId7 == null)
			{
				worldGameObjectByObjId7 = WorldMap.GetWorldGameObjectByObjId("soul_extractor");
			}
			if (worldGameObjectByObjId7 != null && string.IsNullOrEmpty(worldGameObjectByObjId7.custom_tag))
			{
				worldGameObjectByObjId7.custom_tag = "soul_extractor";
			}
		}
		if (num <= 1316 && MainGame.me.save.unlocked_techs.Contains("The Beginning Of Alchemy"))
		{
			WorldMap.SpawnWGO(MainGame.me.world_root, "keeper_room_walls", new Vector2(2688f, -6528f), "home_walls");
			MainGame.me.save.UnlockCraft("keeper_room_builddesk::keeper_room_walls_blue_1");
			MainGame.me.save.UnlockCraft("keeper_room_builddesk::keeper_room_walls_brown_1");
			MainGame.me.save.UnlockCraft("keeper_room_builddesk::keeper_room_walls_green_1");
			MainGame.me.save.UnlockCraft("keeper_room_builddesk::keeper_room_walls_green_2");
			MainGame.me.save.UnlockCraft("keeper_room_builddesk::keeper_room_walls_red_1");
			MainGame.me.save.UnlockCraft("keeper_room_builddesk::keeper_room_walls_violet_1");
			MainGame.me.save.UnlockCraft("keeper_room_builddesk::keeper_room_walls_violet_2");
			MainGame.me.save.UnlockCraft("keeper_room_builddesk::keeper_room_walls_yellow_1");
			MainGame.me.save.UnlockCraft("keeper_room_builddesk::keeper_room_walls_white_1");
			MainGame.me.save.UnlockCraft("keeper_room_builddesk::keeper_room_walls_black_1");
			MainGame.me.save.UnlockCraft("keeper_room_builddesk::keeper_room_bed_white_1");
			MainGame.me.save.UnlockCraft("keeper_room_builddesk::keeper_room_bed_black_1");
			MainGame.me.save.UnlockCraft("keeper_room_builddesk:p:keeper_room_carpet_white_1");
			MainGame.me.save.UnlockCraft("keeper_room_builddesk:p:keeper_room_carpet_black_1");
		}
		if (num <= 1320)
		{
			WorldGameObject worldGameObjectByObjId8 = WorldMap.GetWorldGameObjectByObjId("stained_glass_window");
			if (worldGameObjectByObjId8 != null)
			{
				worldGameObjectByObjId8.transform.position = new Vector2(20254f, -11806f);
			}
			List<WorldGameObject> list4 = WorldMap.FindWGOs(new string[1] { "candelabrum_3_3" }, new Vector2(12332f, -11518f), string.Empty);
			if (list4 != null && list4.Count > 0)
			{
				list4[0].ReplaceWithObject("candelabrum_3_3_souls");
			}
			List<WorldGameObject> list5 = WorldMap.FindWGOs(new string[1] { "teleport_outside" }, new Vector2(20110f, -11584f), string.Empty);
			if (list5 != null && list5.Count > 0)
			{
				list5[0].DestroyMe();
				WorldMap.SpawnWGO(MainGame.me.world_root, "teleport_inside_euric_room", new Vector2(20110f, -11530f), "tp_mortuary_from_euric_b_");
			}
			WorldGameObject worldGameObjectByObjId9 = WorldMap.GetWorldGameObjectByObjId("cooking_table");
			if (worldGameObjectByObjId9 != null)
			{
				worldGameObjectByObjId9.custom_tag = "home_cook_table";
			}
			WorldGameObject worldGameObjectByObjId10 = WorldMap.GetWorldGameObjectByObjId("cooking_stand");
			if (worldGameObjectByObjId10 != null)
			{
				worldGameObjectByObjId10.custom_tag = "home_cook_stand";
			}
		}
		if (num <= 1323)
		{
			if (MainGame.me.save.unlocked_techs.Contains("First slice"))
			{
				MainGame.me.save.UnlockCraft("souls_builddesk:p:corpse_bed_place");
			}
			WorldGameObject worldGameObjectByObjId11 = WorldMap.GetWorldGameObjectByObjId("cupboard");
			if (worldGameObjectByObjId11 != null)
			{
				worldGameObjectByObjId11.custom_tag = "home_cupboard";
			}
		}
		if (num <= 1328)
		{
			WorldGameObject worldGameObjectByCustomTag8 = WorldMap.GetWorldGameObjectByCustomTag("home_cupboard");
			if (worldGameObjectByCustomTag8 != null)
			{
				worldGameObjectByCustomTag8.round_and_sort.floor_line = 0f;
				worldGameObjectByCustomTag8.round_and_sort.MarkPositionDirty();
			}
		}
		if (num <= 1330)
		{
			List<WorldGameObject> list6 = WorldMap.FindWGOs(new string[1] { "bush_4" }, new Vector2(5664f, -1896f), "");
			if (list6 != null)
			{
				foreach (WorldGameObject item17 in list6)
				{
					item17.DestroyMe();
				}
			}
			List<WorldGameObject> list7 = WorldMap.FindWGOs(new string[1] { "npc_farmer" }, new Vector2(4990f, -7974f), "");
			if (list7 != null)
			{
				foreach (WorldGameObject item18 in list7)
				{
					item18.DestroyMe();
				}
			}
			WorldGameObject worldGameObjectByCustomTag9 = WorldMap.GetWorldGameObjectByCustomTag("home_cupboard");
			if (worldGameObjectByCustomTag9 != null)
			{
				worldGameObjectByCustomTag9.ReplaceWithObject("cupboard_home");
			}
		}
		if (num <= 1400)
		{
			WorldGameObject worldGameObjectByCustomTag10 = WorldMap.GetWorldGameObjectByCustomTag("home_walls", ignore_not_found_error: true);
			Debug.Log($"#dbg_wall# keeper_room_wall is null: {worldGameObjectByCustomTag10 == null}");
			if (worldGameObjectByCustomTag10 == null)
			{
				Debug.Log("#dbg_wall# keeper_room_wall has spawned");
				WorldMap.SpawnWGO(MainGame.me.world_root, "keeper_room_walls", new Vector2(2688f, -6528f), "home_walls");
			}
		}
		if (num <= 1402)
		{
			List<WorldGameObject> list8 = WorldMap.FindWGOs(new string[1] { "tavern_cellar_rack" }, new Vector2(16588f, -9090f), "");
			if (list8 != null)
			{
				foreach (WorldGameObject item19 in list8)
				{
					WorldMap.SpawnWGO(MainGame.me.world_root, "tavern_cellar_rack_2", new Vector2(16588f, -9090f)).data.inventory = item19.data.inventory;
					item19.data.inventory = null;
					item19.DestroyMe();
				}
			}
			else
			{
				WorldMap.SpawnWGO(MainGame.me.world_root, "tavern_cellar_rack_2", new Vector2(16588f, -9090f));
			}
		}
		if (num <= 1403)
		{
			WorldMap.TryRemoveStackedChurchVisitors();
		}
		if (num <= 1404 && MainGame.me.save.black_list_of_phrases.Contains("@souls_s_s46_ask"))
		{
			WorldMap.GetWorldGameObjectByObjId("smilers_box_opened")?.ReplaceWithObject("smilers_box_abandoned");
		}
		if (num <= 1406 && MainGame.me.save.unlocked_crafts.Contains("glass_broken_0"))
		{
			MainGame.me.save.UnlockCraft("glass_broken_2");
		}
	}

	public void GlobalEventsCheck()
	{
		List<GlobalEventBase> list = new List<GlobalEventBase>();
		GlobalEventBase item = new GlobalEventBase("halloween", new DateTime(2018, 10, 29), new TimeSpan(14, 0, 0, 0))
		{
			on_start_script = new Scene1100_To_SceneHelloween(),
			on_finish_script = new SceneHelloween_To_Scene1100()
		};
		list.Add(item);
		foreach (GlobalEventBase item2 in list)
		{
			item2.Process();
		}
	}

	public Item GetSavedPlayerInventory()
	{
		Debug.Log("GetSavedPlayerInventory");
		return _inventory;
	}

	public bool BuyTech(string tech_id)
	{
		if (!CanBuyTech(tech_id))
		{
			return false;
		}
		TechDefinition data = GameBalance.me.GetData<TechDefinition>(tech_id);
		MainGame.me.player.data.SubFromParams(data.price);
		UnlockTech(tech_id);
		return true;
	}

	public bool CanBuyTech(string tech_id)
	{
		if (unlocked_techs.Contains(tech_id))
		{
			return false;
		}
		TechDefinition data = GameBalance.me.GetData<TechDefinition>(tech_id);
		if (data == null)
		{
			return false;
		}
		if (!MainGame.me.player.data.IsEnoughParams(data.price))
		{
			return false;
		}
		foreach (TechDefinition parent in data.parents)
		{
			if (!unlocked_techs.Contains(parent.id))
			{
				return false;
			}
		}
		return true;
	}

	public void SetEnvironmentPreset(string[] parts)
	{
		string text = parts.Last();
		string environment_preset = parts[1];
		switch (text)
		{
		case "a":
			_environment_preset = null;
			break;
		case "b":
			_environment_preset = environment_preset;
			break;
		}
		ApplyCurrentEnvironmentPreset();
	}

	public void SetEnvironmentPreset(string preset_name)
	{
		_environment_preset = preset_name;
		ApplyCurrentEnvironmentPreset();
	}

	public void ApplyCurrentEnvironmentPreset()
	{
		string text = (string.IsNullOrEmpty(_environment_preset) ? null : _environment_preset);
		Debug.Log("ApplyCurrentEnvironmentPreset, id = " + text);
		EnvironmentPreset preset = EnvironmentPreset.Load(text);
		EnvironmentEngine.me.ApplyEnvironmentPreset(preset);
	}

	public bool GetSinState(Sins.SinType sin)
	{
		return MainGame.me.player.GetParam(Sins.SIN_NAMES[(int)sin]) >= 100f;
	}

	public float GetHPPercentage()
	{
		return MainGame.me.player.hp / (float)max_hp;
	}

	public void OnFinishedCraft(CraftDefinition craft)
	{
		MainGame.me.player.GetComponent<PlayerComponent>().ResetSpentCounters();
		quests.CheckKeyQuests("craft_" + craft.id);
		Stats.DesignEvent("Craft:" + craft.id.Replace(":", "_") + ":Finished");
		if (!completed_one_time_crafts.Contains(craft.id) && !craft.id.Contains(":_:"))
		{
			completed_one_time_crafts.Add(craft.id);
			quests.CheckKeyQuests("newcraft_" + craft.id);
			if (!string.IsNullOrEmpty(craft.ach_key))
			{
				MainGame.me.save.achievements.CheckKeyQuests("new_" + craft.ach_key);
			}
		}
		if (!string.IsNullOrEmpty(craft.ach_key))
		{
			MainGame.me.save.achievements.CheckKeyQuests(craft.ach_key);
		}
		if (craft.needs.Count > 0 && craft.craft_type == CraftDefinition.CraftType.AlchemyDecompose)
		{
			string text = craft.needs[0]?.id;
			if (string.IsNullOrEmpty(text))
			{
				return;
			}
			ItemDefinition.AlchemyType? alchemyType = craft.GetFirstRealOutput()?.definition?.alch_type;
			if (!alchemyType.HasValue)
			{
				return;
			}
			int value = (int)alchemyType.Value;
			if (value >= 1 && value <= 3)
			{
				CraftDefinition data = GameBalance.me.GetData<CraftDefinition>("alchemy_" + value + "_" + text);
				if (data != null)
				{
					UnlockCraft(data.id);
				}
			}
		}
		else
		{
			if (craft.needs.Count <= 0 || craft.craft_type != CraftDefinition.CraftType.MixedCraft)
			{
				return;
			}
			string text2 = "mix";
			foreach (Item need in craft.needs)
			{
				if (!TechDefinition.TECH_POINTS.Contains(need.id))
				{
					string text3 = need.id;
					if (text3.Contains(":"))
					{
						string[] array = text3.Split(new string[1] { ":" }, StringSplitOptions.RemoveEmptyEntries);
						text3 = array[array.Length - 1];
					}
					text2 = text2 + "_" + text3;
				}
			}
			if (text2 != "mix")
			{
				UnlockCraft(text2);
			}
		}
	}

	public bool IsCraftVisible(CraftDefinition craft)
	{
		if (craft.hidden)
		{
			return false;
		}
		if (craft.IsLocked())
		{
			return false;
		}
		if (craft.one_time_craft && completed_one_time_crafts.Contains(craft.id))
		{
			return false;
		}
		return true;
	}

	public bool IsWorkAvailible(ObjectDefinition work_obj)
	{
		string item = ((work_obj.object_groups.Count > 0) ? work_obj.object_groups[0].id : "");
		if (work_obj.need_unlock_work && !unlocked_works.Contains(item))
		{
			return false;
		}
		return true;
	}

	public bool IsSurveyComplete(string item_id)
	{
		return IsSurveyComplete(CraftDefinition.CraftSubType.None, item_id);
	}

	public bool IsSurveyComplete(CraftDefinition.CraftSubType type, string item_id)
	{
		if (item_id.Contains(":") && completed_one_time_crafts.Contains("surv:" + ItemDefinition.StaticGetNameWithoutQualitySuffix(item_id)))
		{
			return true;
		}
		return completed_one_time_crafts.Contains("surv:" + item_id);
	}

	public void RevealHiddenTech(string tech_id)
	{
		TechDefinition data = GameBalance.me.GetData<TechDefinition>(tech_id);
		if (data == null)
		{
			Debug.LogError("Couldn't find technology id = " + tech_id);
			return;
		}
		if (!data.hidden)
		{
			Debug.LogError("Can't reveal non-hidden tech id = " + tech_id);
			return;
		}
		if (!revealed_techs.Contains(tech_id))
		{
			revealed_techs.Add(tech_id);
		}
		if (data.invisible && !visible_techs.Contains(tech_id))
		{
			visible_techs.Add(tech_id);
		}
	}

	public void MakeVisibleInvisibleTech(string tech_id)
	{
		TechDefinition data = GameBalance.me.GetData<TechDefinition>(tech_id);
		if (data == null)
		{
			Debug.LogError("Couldn't find technology id = " + tech_id);
		}
		else if (!data.invisible)
		{
			Debug.LogError("Can't make visible non-invisible tech id = " + tech_id);
		}
		else if (!visible_techs.Contains(tech_id))
		{
			visible_techs.Add(tech_id);
		}
	}

	public void OnMetNPC(string npc_id)
	{
		known_npcs.GetOrCreateNPC(npc_id);
	}

	public void SetTaskState(string npc_id, string task_id, KnownNPC.TaskState.State state, Action on_finished = null)
	{
		Debug.Log("SetTaskState npc=" + npc_id + ", task=" + task_id + ", state=" + state);
		if (state == KnownNPC.TaskState.State.Visible)
		{
			if (known_npcs.GetOrCreateNPC(npc_id).GetQuestState(task_id) == KnownNPC.TaskState.State.Complete)
			{
				on_finished.TryInvoke();
				return;
			}
			known_npcs.GetOrCreateNPC(npc_id).SetQuestState(task_id, state);
			GUIElements.me.relation.npc_tasks.Redraw();
		}
		WorldGameObject worldGameObject = WorldMap.GetWorldGameObjectByObjId(npc_id, ignore_not_found_error: true);
		if (worldGameObject == null)
		{
			worldGameObject = MainGame.me.player;
		}
		Transform bubble_pos_tf = MainGame.me.player.bubble_pos_tf;
		Transform markerPointOfTask = GUIElements.me.relation.npc_tasks.GetMarkerPointOfTask(npc_id, task_id);
		string gui_sprite_name = "icon_quest_mark_small";
		if (task_id.StartsWith("dlc_stories_"))
		{
			gui_sprite_name = "dlc_quest_mrk";
		}
		else if (task_id.StartsWith("dlc_refugees") || task_id.StartsWith("s_ev"))
		{
			gui_sprite_name = "quest_marker_violet";
		}
		else if (task_id.StartsWith("dlc_souls"))
		{
			gui_sprite_name = "Icon_quest_mark_small_blue";
		}
		if (markerPointOfTask == null && worldGameObject != MainGame.me.player)
		{
			GUIElements.me.relation_additional.npc_tasks.Redraw();
			GUIElements.me.relation_additional.Open(worldGameObject);
			Transform additional_point = GUIElements.me.relation_additional.npc_tasks.GetMarkerPointOfTask(npc_id, task_id);
			if (additional_point != null)
			{
				UIWidget relation_widget = GUIElements.me.relation_additional.GetComponent<UIWidget>();
				relation_widget.alpha = 0f;
				DOTween.To(() => relation_widget.alpha, delegate(float x)
				{
					relation_widget.alpha = x;
				}, 1f, 0.2f);
				FlyingObject flyingObject = FlyingObject.CreateFlyingGUISprite(gui_sprite_name, bubble_pos_tf);
				flyingObject.StartSmoothFlyAndBounceToAMovingObject(() => additional_point);
				UIWidget component = additional_point.gameObject.GetComponentInParent<HUDTaskItemGUI>().GetComponent<UIWidget>();
				if (state == KnownNPC.TaskState.State.Visible)
				{
					component.alpha = 0f;
					GUIElements.me.relation_additional.npc_tasks.SetTaskHiddenState(hidden: true, task_id);
				}
				flyingObject.on_reached_dest = delegate
				{
					GUIElements.me.relation_additional.npc_tasks.SetTaskHiddenState(state != KnownNPC.TaskState.State.Visible, task_id);
					if (state == KnownNPC.TaskState.State.Complete)
					{
						known_npcs.GetOrCreateNPC(npc_id).SetQuestState(task_id, state);
						GUIElements.me.relation_additional.npc_tasks.Redraw();
					}
					GJTimer.AddTimer(2.5f, delegate
					{
						DOTween.To(() => relation_widget.alpha, delegate(float x)
						{
							relation_widget.alpha = x;
						}, 0f, 0.2f).OnComplete(delegate
						{
							GUIElements.me.relation_additional.Hide();
						});
					});
				};
			}
			else
			{
				GUIElements.me.relation_additional.Hide();
			}
		}
		if (markerPointOfTask != null)
		{
			bubble_pos_tf = worldGameObject.bubble_pos_tf;
			FlyingObject flyingObject2 = FlyingObject.CreateFlyingGUISprite(gui_sprite_name, bubble_pos_tf);
			flyingObject2.StartSmoothFlyAndBounceToAMovingObject(() => GUIElements.me.relation.npc_tasks.GetMarkerPointOfTask(npc_id, task_id));
			Debug.Log("Create flying object", flyingObject2);
			UIWidget component2 = markerPointOfTask.gameObject.GetComponentInParent<HUDTaskItemGUI>().GetComponent<UIWidget>();
			if (state == KnownNPC.TaskState.State.Visible)
			{
				component2.alpha = 0f;
				GUIElements.me.relation.npc_tasks.SetTaskHiddenState(hidden: true, task_id);
			}
			flyingObject2.on_reached_dest = delegate
			{
				GUIElements.me.relation.npc_tasks.SetTaskHiddenState(state != KnownNPC.TaskState.State.Visible, task_id);
				if (state == KnownNPC.TaskState.State.Complete)
				{
					known_npcs.GetOrCreateNPC(npc_id).SetQuestState(task_id, state);
					GUIElements.me.relation.npc_tasks.Redraw();
				}
				on_finished.TryInvoke();
			};
		}
		else
		{
			if (state == KnownNPC.TaskState.State.Complete)
			{
				known_npcs.GetOrCreateNPC(npc_id).SetQuestState(task_id, state);
			}
			on_finished.TryInvoke();
		}
		EffectBubblesManager.ShowImmediately(bubble_pos_tf.position, GJL.L("task_bubble_" + state.ToString().ToLower()), EffectBubblesManager.BubbleColor.Relation, ignore_timescale: true, 1f);
	}

	public void UnlockPerk(string perk_id)
	{
		if (!unlocked_perks.Contains(perk_id))
		{
			unlocked_perks.Add(perk_id);
			PerkDefinition data = GameBalance.me.GetData<PerkDefinition>(perk_id);
			if (data != null)
			{
				MainGame.me.player.AddToParams(data.output_res);
			}
		}
	}

	public Item GenerateBody(int tier_min, int tier_max, int soul_tier_min = -1, int soul_tier_max = -1)
	{
		List<BodyDefinition> list = new List<BodyDefinition>();
		foreach (BodyDefinition bodies_datum in GameBalance.me.bodies_data)
		{
			if (bodies_datum.tier >= tier_min && bodies_datum.tier <= tier_max)
			{
				list.Add(bodies_datum);
			}
		}
		if (list.Count == 0)
		{
			Debug.LogError("Couldn't generate body - no suitable BodyDefenition found. tier_min = " + tier_min + ", tier_max = " + tier_max);
			return null;
		}
		BodyDefinition bodyDefinition = list.RandomElement();
		Item item = bodyDefinition.GenerateBodyItem();
		Debug.Log("Generated body with tiers " + tier_min + ".." + tier_max + ", id = " + bodyDefinition.id + ", tier = " + bodyDefinition.tier);
		if (soul_tier_min > -1 && soul_tier_max > -1)
		{
			Item item2 = GenerateSoul(soul_tier_min, soul_tier_max);
			if (item2 != null)
			{
				item.inventory.Add(item2);
			}
		}
		MixerLightIntegration.ProcessBody(item);
		return item;
	}

	public Item GenerateSoul(int tier_min, int tier_max)
	{
		List<SoulDefinition> list = new List<SoulDefinition>();
		foreach (SoulDefinition souls_datum in GameBalance.me.souls_data)
		{
			if (souls_datum.tier >= tier_min && souls_datum.tier <= tier_max)
			{
				list.Add(souls_datum);
			}
		}
		if (list.Count == 0)
		{
			Debug.LogError($"There's no body to generate withing tiers range: [{tier_min}-{tier_max}]");
			return null;
		}
		SoulDefinition soulDefinition = list.RandomElement();
		Item result = soulDefinition.GenerateSoulItem();
		Debug.Log("Generated soul with tiers " + tier_min + ".." + tier_max + ", id = " + soulDefinition.id + ", tier = " + soulDefinition.tier);
		return result;
	}

	public bool IsInTutorial()
	{
		return MainGame.me.player.GetParamInt("in_tutorial") > 0;
	}

	public void OnEnteredWorldZone(WorldZone z)
	{
		if (!known_world_zones.Contains(z.id))
		{
			known_world_zones.Add(z.id);
			achievements.CheckKeyQuests("newzone_" + z.id);
		}
	}

	public bool IsWorldZoneKnown(string id)
	{
		return known_world_zones.Contains(id);
	}

	public void StoreEnvironmentPreset()
	{
		_stored_environment_preset = _environment_preset;
	}

	public void RestoreEnvironmentPreset()
	{
		_environment_preset = _stored_environment_preset;
		ApplyCurrentEnvironmentPreset();
		_stored_environment_preset = "";
	}
}
