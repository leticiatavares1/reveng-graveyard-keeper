using System;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public static class LazyConsts
{
	public static class Layers
	{
		public const int DEFAULT = 0;

		public const int INTERACTABLE = 6;

		public const int DOCK_POINT = 7;

		public const int OBSTACLES = 8;

		public const int FAKE_LIGHTING_OBJECT = 9;

		public const int PLAYER = 10;

		public const int GROUND = 11;

		public const int STEP = 12;

		public const int CUSTOM_GRAVITY_FIELD = 13;

		public const int PLAYER_INVISIBLE_WALLS = 14;

		public const int TRANSPARENCY_OCCLUDER = 15;

		public const int DROP = 16;

		public const int WORLD_ZONE = 17;

		public const int SOUND_ZONE = 18;

		public const int BUILD_AREA = 19;

		public const int GD_ZONE = 23;

		public const int PLAYER_DROP = 24;

		public const int IGNORE_COLLISION_WITH_PLAYER = 26;

		public const int CONVEYOR_CONNECTOR = 27;

		public const int BUFF_AREA = 28;

		public const int FIGHTER = 29;

		public const int WATER = 4;

		public const int DEFAULT_MASK = 1;

		public const int INTERACTABLE_MASK = 64;

		public const int DOCK_POINT_MASK = 128;

		public const int OBSTACLES_MASK = 256;

		public const int FAKE_LIGHTING_OBJECT_MASK = 512;

		public const int GROUND_MASK = 2048;

		public const int STEP_MASK = 4096;

		public const int CUSTOM_GRAVITY_FIELD_MASK = 8192;

		public const int TRANSPARENCY_OCCLUDER_MASK = 32768;

		public const int DROP_MASK = 65536;

		public const int PLAYER_MASK = 1024;

		public const int PLAYER_DROP_MASK = 16777216;

		public const int WORLD_ZONE_MASK = 131072;

		public const int BUILD_AREA_MASK = 524288;

		public const int GD_ZONE_MASK = 8388608;

		public const int CONVEYOR_CONNECTOR_MASK = 134217728;

		public const int IGNORE_COLLISION_WITH_PLAYER_MASK = 67108864;

		public const int BUFF_AREA_MASK = 268435456;

		public const int FIGHTER_MASK = 536870912;
	}

	public static class Tags
	{
		public const string WATER = "Water";

		public const string RECAST_FLOOR = "RecastFloor";
	}

	public static class WgoCustomTags
	{
		public const string TOWN_BUILDING_TAG_POSTFIX_PART = "place_";

		public const string TOWN_BUILDING_SIGNBOARD_TAG_PREFIX = "t_b_signboard_";

		public const string TOWN_BUILDING_CHARACTER_TAG_PREFIX = "t_b_character_";

		public const string TOWN_BUILDING_TENT_TAG_PREFIX = "t_b_tent_";

		public const string TOWN_BUILDING_YARD_TAG_PREFIX = "t_b_yard_";

		public const string TOWN_BUILDING_SIGN_TAG_PREFIX = "t_b_sign_";

		public const string TOWN_BUILDING_DECOR_1_TAG_PREFIX = "t_b_decor_1_";

		public const string TOWN_BUILDING_DECOR_2_TAG_PREFIX = "t_b_decor_2_";

		public const string TOWN_BUILDING_DECOR_3_TAG_PREFIX = "t_b_decor_3_";

		public const string PANIC_REDUCTION_MACHINE_TAG = "panic_reduction_machine";
	}

	public static class SFX
	{
		public const string TOOL_AXE = "tool_axe";

		public const string TOOL_SHOVEL = "tool_shovel";

		public const string TOOL_PICKAXE = "tool_pickaxe";

		public const string TOOL_HAMMER = "tool_hammer";

		public const string TREE_FALL = "tree_fall";

		public const string DOOR = "door";

		public const string TECH_POINT = "tech_point_collect";

		public const string TECH_POINT_FAILED = "tech_point_collect_failed";

		public const string BELL = "donkey_bell";

		public const string ITEM_PICKUP = "item_pickup";

		public const string PLANTING = "planting";

		public const string BAG_OPEN = "bag_open";

		public const string BAG_CLOSE = "bag_close";

		public const string UNLOCK = "unlock";

		public const string EQUIP = "equip_tool";

		public const string UNEQUIP = "unequip_tool";

		public const string HOVER = "gui_hover";

		public const string HOVER_LIGHT = "gui_hover_light";

		public const string CLICK = "gui_click";

		public const string ITEM_PUT = "item_put";

		public const string COINS = "coins_sound";

		public const string CLIMB_START = "ladder_climb_start";

		public const string CLIMB_STEP = "ladder_climb";

		public const string CLIMB_FINISH = "ladder_climb_finish";

		public const string SERMON_SUCCESS = "sermon_success";

		public const string SERMON_FAIL = "sermon_fail";

		public const string PLAYER_ATTACK = "sword_attack";

		public const string SWORD_HIT = "sword_hit";

		public const string BOW_AIM_START = "bow_aim_start";

		public const string BOW_AIM_LOOP = "bow_aim_loop";

		public const string BOW_AIM_SHOT = "bow_aim_shot";

		public const string BOW_HIT = "bow_hit_zombie";

		public const string SPEAR_ATTACK = "spear_attack";

		public const string SPEAR_HIT = "spear_hit_zombie";

		public const string ZOMBIE_ATTACK = "zombie_attack";

		public const string ZOMBIE_HIT_WOOD = "zombie_hit_wood";

		public const string ZOMBIE_HIT = "zombie_hit_player";

		public const string SPITTER_ATTACK = "spitter_attack";

		public const string SPITTER_HIT = "spitter_hit";

		public const string ZOMBIE_IDLE = "zombie_idle";

		public const string FIGHT_LOSE = "fight_lose";

		public const string FIGHT_WIN = "fight_win";

		public const string FIGHT_START = "fight_start";

		public const string WPN_HIT_DOOR_ZOMBIE = "hit_door_zombie";

		public const string ARROW_HIT_DOOR_ZOMBIE = "arrow_hit_door_zombie";

		public const string ALLY_DEATH_HUMAN = "ally_death_human";

		public const string ALLY_DEATH_ZOMBIE = "ally_death_zombie";

		public const string FISHING_START = "fishing_start";

		public const string FISHING_CAST = "fishing_cast_swoosh";

		public const string FISHING_BLOP = "fishing_blop";

		public const string FISHING_BITE_SUCCESS = "fishing_bite_success";

		public const string FISHING_LINE_BREAK = "fishing_line_break";

		public const string FISHING_BITE = "fishing_bite";

		public const string FISHING_SUCCESS = "fishing_success";

		public const string FISHING_FAIL = "fishing_fail";

		public const string FISHING_REEL_SHORT = "fishing_reel_short";

		public const string FISHING_REEL_LONG = "fishing_reel_long";

		public const string FISHING_SPLASHES = "fishing_floundering";

		public const string RIVER_DUMP_WATER = "fishing_blop";

		public const string OH_ZOMBIE_TAKE = "oh_zombie_grab";

		public const string OH_ZOMBIE_DROP = "oh_zombie_drop";

		public const string OH_CORPSE_TAKE = "oh_corpse_grab";

		public const string OH_CORPSE_DROP = "oh_corpse_drop";

		public const string OH_CORPSE_GRAVE_DROP = "oh_corpse_grave_drop";

		public const string OH_WOOD_TAKE = "oh_wood_grab";

		public const string OH_WOOD_DROP = "oh_wood_drop";

		public const string OH_WOOD_CONTAINER_DROP = "oh_wood_container_drop";

		public const string ZOMBIE_CHEST_RUMMAGE = "zombie_chest_rummage";

		public const string TAB_CLICK = "tab_click";

		public const string BLIMP_GROW = "blimp_grow";

		public const string BUILD_PLACE = "build_place";
	}

	public static class Music
	{
		public const string MAIN_MENU = "main_menu";

		public const string GAMEPLAY = "gameplay";

		public const string FIGHT = "fight";

		public const string SEWER_FIGHT = "sewer_fight";
	}

	public static class Perks
	{
		public const string LACK_OF_SLEEP_DEBUFF = "lack_of_sleep_debuff";

		public const string EXCESSIVE_ZOMBIE_DEBUFF = "debuff_excessive_zombie";
	}

	public static class ConstDefs
	{
		public const string TOWN_BUY_COEFFICIENT = "town_buy_k";

		public const string PRODUCT_GROUP_1_COEFFICIENT = "product_group_1";

		public const string PRODUCT_GROUP_2_COEFFICIENT = "product_group_2";

		public const string PRODUCT_GROUP_3_COEFFICIENT = "product_group_3";

		public const string PRODUCT_GROUP_HAPPIINESS_1_COEFFICIENT = "product_group_happiness_1";

		public const string PRODUCT_GROUP_HAPPIINESS_2_COEFFICIENT = "product_group_happiness_2";

		public const string PRODUCT_GROUP_HAPPIINESS_3_COEFFICIENT = "product_group_happiness_3";

		public const string NEW_GAME_START_QUEST = "new_game_start_quest";

		public const string ZOMBIE_CRAFT_SUB_TICKS_COUNT = "zombie_craft_sub_ticks_count";

		public const string ZOMBIE_CARETAKER_PICKING_UP_TIME = "zombie_caretaker_picking_up_time";

		public const string CONVEYOR_SYSTEM_UPDATE_INTERVAL = "conveyor_system_update_interval";

		public const string BASE_PARISHIONER_CRIT_POWER = "base_parishioner_crit_power";

		public const string MAX_CRAFT_CELLS_PER_ONE_HIT = "max_cells_per_one_hit";

		public const string ENERGY_CRAFT_BORDER_1 = "energy_craft_border_1";

		public const string ENERGY_CRAFT_BORDER_2 = "energy_craft_border_2";

		public const string INSANITY_CRAFT_BORDER_1 = "insanity_craft_border_1";

		public const string INSANITY_CRAFT_BORDER_2 = "insanity_craft_border_2";

		public const string STAMINA_REGENERATION = "stamina_regeneration";

		public const string STAMINA_REGENERATION_STANCE = "stamina_regeneration_stance";

		public const string STAMINA_REGENERATION_DELAY = "stamina_regeneration_delay";

		public const string START_ORDERS_COUNT = "start_orders_count";

		public const string CORPSE_AUTO_DESTROY_TIMER = "corpse_auto_destroy_timer";

		public const string MIN_SPAWN_DISTANCE = "spawn_distance";

		public const string DAY_PRIDE = "day_pride";

		public const string DAY_LUST = "day_lust";

		public const string DAY_GLUTTONY = "day_gluttony";

		public const string DAY_ENVY = "day_envy";

		public const string DAY_WRATH = "day_wrath";

		public const string DAY_SLOTH = "day_sloth";

		public static string[] AllDays = new string[6] { "day_pride", "day_lust", "day_gluttony", "day_envy", "day_wrath", "day_sloth" };
	}

	public static class Fighting
	{
		public enum TeamType
		{
			Player,
			WildZombie
		}

		public enum TargetAttackPriority
		{
			Low = 0,
			Medium = 10,
			High = 20,
			Critical = 30
		}

		[Flags]
		public enum EntityType
		{
			None = 0,
			Player = 1,
			Soldier = 2,
			Zombie = 4
		}

		public const int FIGHTING_STAGE_VISIBLE = 1;

		public const int FIGHTING_STAGE_AVAILABLE_FOR_START = 2;

		public const int FIGHTING_STAGE_PRE_FIGHT = 3;

		public const int FIGHTING_STAGE_FIGHT = 4;

		public const int FIGHTING_STAGE_FIGHT_WIN = 5;

		public const int FIGHTING_STAGE_FINAL = 6;

		public const string DEV_GENERIC_FIGHTERS_TIER_RES_ID = "dev_generic_fighters_tier";

		public const float MAIN_HERO_RVO_PRIORITY = 0.85f;

		public const float MAIN_HERO_PUSH_ALLY_RVO_PRIORITY = 0.15f;

		public const float MAIN_HERO_PUSH_HOLD_TIME = 0.35f;
	}

	public static class Navigation
	{
		public enum Graph
		{
			None = -1,
			PortArea = 1,
			DriedGatewayArea = 4,
			GdPointGraph = 2,
			PlayerGraph = 3,
			DevPlayground = 5,
			Church = 6,
			ZombieFighters = 7,
			WZYard = 8,
			TestZombieZone = 9,
			VillageForestArea = 10,
			RuinedTemple = 11,
			Fighting_Recast = 12,
			Carrier = 13,
			Sawmill = 14,
			Mine = 15,
			SandClay = 16,
			Garden = 17,
			Vineyard = 18,
			Conveyors = 19,
			VineyardBasement = 20,
			BasementWriting = 21,
			AlchemyLab = 22,
			PlayerHouse = 23
		}

		public enum GraphMask
		{
			PortAreaMask = 2,
			DriedGatewayAreaMask = 16,
			GdPointGraphAreaMask = 4,
			PlayerGraphMask = 8,
			DevPlaygroundMask = 32,
			VillageForesAreaMask = 1024,
			ChurchMask = 64,
			ZombieFightersMask = 128,
			WZYardMask = 256,
			TestZombieZoneMask = 512,
			RuinedTempleMask = 2048,
			Fighting_RecastMask = 4096
		}

		public static Pathfinding.GraphMask GraphMaskFromAllRecastGraphs
		{
			get
			{
				Pathfinding.GraphMask result = default(Pathfinding.GraphMask);
				NavGraph[] graphs = AstarPath.active.graphs;
				foreach (NavGraph navGraph in graphs)
				{
					if (navGraph is RecastGraph)
					{
						result |= new Pathfinding.GraphMask((uint)(1 << (int)navGraph.graphIndex));
					}
				}
				return result;
			}
		}
	}

	public static class MovementComponent
	{
		public const float SPEED_DEFAULT = 1.5f;

		public const float NPC_SIM_SPEED_DEFAULT = 1.125f;
	}

	public static class WorldZones
	{
		public const string MORGUE = "morgue";

		public const string RESURRECTION = "resurrection";
	}

	public const string PLACEHOLDER_ITEM_ICON = "i_placeholder";

	public const string PLACEHOLDER_CRAFT_RESULT_ICON = "i_b_blueprint_placeholder";

	public const string STAR_ICON_PREFIX = "item_star_";

	public const string ITEM_FROM_GAME_RES_ATOM_PREFIX = "game_res_";

	public const string WORLD_ZONE_RES_ATOM_PREFIX = "wz_";

	public const string TELEPORT_POINT_PREFIX = "tp_point_";

	public const string TOWN_BUILDING_CRAFT_PREFIX = "town_building_craft:";

	public const string ENERGY_KEY = "energy";

	public const string INSANITY_KEY = "insanity";

	public const string HAPPINESS_KEY = "happiness";

	public const string INSANITY_LOCK_KEY = "insanity_lock";

	public const string TECH_RED_SPHERE = "tech_red";

	public const string TECH_GREEN_SPHERE = "tech_green";

	public const string TECH_BLUE_SPHERE = "tech_blue";

	public const string RUNE_RED = "rune_r";

	public const string RUNE_GREEN = "rune_g";

	public const string RUNE_BLUE = "rune_b";

	public const string MONEY_KEY = "money";

	public const string GLOBAL_PPL_KEY = "global_ppl";

	public const string CUR_PRAY_PPL_KEY = "cur_pray_ppl";

	public const string LAST_GO_TO_PATH_LENGTH_KEY = "lastGoToPathLength";

	public const string DURATION_KEY = "duration";

	public const string QUALITY_BONUS_KEY = "quality_bonus";

	public const string CROP_BONUS_KEY = "crop_bonus";

	public const string DURATION_BONUS_KEY = "duration_bonus";

	public const string FAILED_PROGRESS_TICKS_KEY = "failed_cells";

	public const string SUCCEDED_PROGRESS_TICKS_KEY = "succeded_cells";

	public const string GARDEN_FERTILIZERS_SLOTS_KEY = "g_garden_fertilizer_slots";

	public const string GARDEN_CRAFT_DECREASE_CRAFT_TIME_KEY = "g_garden_autocraft_dec";

	public const string GARDEN_BED_FARMING_MASTERY_KEY = "g_garden_farming_base";

	public const string GARDEN_BED_FARMING_LEVEL_KEY = "g_garden_lvl";

	public const string GARDEN_BED_VINEYARD_MASTERY_KEY = "g_vineyard_farming_base";

	public const string GARDEN_BED_VINEYARD_LEVEL_KEY = "g_vineyard_lvl";

	public const string GARDEN_PERK_SLOT_PREFIX = "perk_slot_";

	public const string GARDEN_SEED_MASTERY_LOCK_KEY = "seed_mastery_lock";

	public const string GARDEN_COMMON_CROP_OUTPUT_KEY = "crop";

	public const string GARDEN_BRONZE_CROP_OUTPUT_KEY = "crop_b";

	public const string GARDEN_SILVER_CROP_OUTPUT_KEY = "crop_s";

	public const string GARDEN_GOLDEN_CROP_OUTPUT_KEY = "crop_g";

	public const string MILESTONES_ACTIVATED_KEY = "milestones_activated";

	public const string GARDEN_FERTILIZER_PERK_PREFIX = "perk_fertilize_";

	public const string FAKE_CRAFT_PREFIX = "fake_";

	public const string ZOMBIE_CARETAKER_DEFAULT_GD_POINT_ID = "zombie_porter_station_gd_point";

	public const string ZOMBIE_CARETAKER_STATION_WGO_ID = "zombie_supplier_station";

	public const string ZOMBIE_CARETAKER_STATION_NO_WORKER_ICON = "i_no_zombie_delivery";

	public const string RED_CROSS_BIG_ICON = "i_red_cross_big_icon";

	public const string ZOMBIE_GARDENER_DEFAULT_GD_POINT_ID = "zombie_garden_crafter_gd_point";

	public const string ZOMBIE_SAWMILL_WOOD_CRAFTER_GD_POINT = "zombie_sawmill_wood_crafter_gd_point";

	public const string ZOMBIE_SAWMILL_WOOD_CONTAINER_GD_POINT = "zombie_sawmill_wood_container_gd_point";

	public const string ZOMBIE_MINE_CRAFTER_GD_POINT = "zombie_mine_crafter_gd_point";

	public const string ZOMBIE_CLAY_CRAFTER_GD_POINT = "zombie_clay_sand_crafter_gd_point";

	public const string ZOMBIE_SAND_CRAFTER_GD_POINT = "zombie_clay_sand_crafter_gd_point";

	public const string STAMINA_KEY = "stamina";

	public const string LINKED_FIGHTERS_FLAG_KEY = "fighters_flag";

	public const string CHULK_BOARD_ENABLED_RES_NAME = "chalk_board_enabled";

	public const string DONKEY_BODY_DROP_CHANCE = "donkey_body_drop_chance";

	public const string NPC_LIFE_SIM_HOME_RES_ID = "npc_life_sim_home";

	public const string NPC_LIFE_SIM_DISABLED_FLAG = "npc_life_sim_disabled_flag";

	public const string CUR_BODIES_COUNT_KEY = "cur_bodies_count";

	public const string LIFT_TARGET_STORAGE = "target_storage_wgo";

	public const string CUR_ZOMBIES_COUNT_KEY = "cur_zombies_count";

	public const string ZOMBIE_LIMIT_MECHANIC_KEY = "zombies_limit_mechanic";

	public const string RESURRECTION_PREPARED_KEY = "resurrection_prepared";

	public const string RESURRECTION_HAS_POWER = "resurrection_has_power";

	public const string DONKEY_ADDITIONAL_BODY_DROP_KEY = "donkey_drop_additional";

	public const string NUN_MANY_BODY_DROP_KEY = "nun_many_body_drop";

	public const string DONKEY_DROP_CHANCE_REDUCE_AFTER_SUCCESS = "donkey_drop_chance_reduce_after_success";

	public const string DONKEY_DROP_CHANCE_INCREASE_AFTER_FAIL = "donkey_drop_chance_increase_after_fail";

	public const string DONKEY_DROP_CHANCE_INCREASE_AFTER_SKIP = "donkey_drop_chance_increase_after_skip";

	public const string NUN_MANY_DROP_CHANCE_REDUCE_AFTER_SUCCESS = "nun_many_drop_chance_reduce_after_success";

	public const string NUN_MANY_DROP_CHANCE_INCREASE_AFTER_FAIL = "nun_many_drop_chance_increase_after_fail";

	public const string NUN_MANY_DROP_CHANCE_INCREASE_AFTER_SKIP = "nun_many_drop_chance_increase_after_skip";

	public const string CRAFT_REACHED_GOLD_LEVEL_KEY = "reached_gold";

	public const string CRAFT_REACHED_SILVER_LEVEL_KEY = "reached_silver";

	public const string CRAFT_REACHED_BRONZE_LEVEL_KEY = "reached_bronze";

	public const string ZOMBIE_PORTER_STAYING_AT_STATION_FLAG = "is_staying_at_porter_station";

	public const string ZOMBIE_PORTER_MOVING_FLAG = "is_moving_to_target_world_zone";

	public const float MORNING = 0.25f;

	public const float EVENING = 0.8f;

	public const string GARDEN_WORLD_ZONE_ID = "garden";

	public const string MILITARY_BASE_WORLD_ZONE_ID = "town_guard_barracks";

	public const string CONVEYOR_STORAGE_WORLD_ZONE_ID = "conveyor_storage";

	public const string CONVEYOR_BUILDING_NOT_REMOVABLE_KEY = "conveyor_build_is_not_removable";

	public const string LOCK_BUILDING_REMOVAL_RES = "lock_building_removal";

	public const string NULL_WGO_ID = "0";

	public const string TREES_WGO_GROUP = "trees";

	public const string GRAVEYARD_MODULES_WGO_GROUP = "graveyard_modules";

	public const string ITEM_PRICE_GLOBAL_MODIFICATOR_POSTFIX = "_base_price_global_mod";

	public const string ITEM_COUNT_GLOBAL_MODIFICATOR_POSTFIX = "_base_count_global_mod";

	public const string BOOST_CRAFT_POSTFIX = "_boost";

	public const string MIX_CRAFT_PREFIX = "mix";

	public const string PARISHIONER_CRIT_CHANCE_MOD = "parishioner_crit_chance_mod";

	public const string PARISHIONER_CRIT_POWER_MOD = "parishioner_crit_power_mod";

	public const string ITEM_FAITH_ID = "faith";

	public const string ITEM_TEMPTATION_ID = "temptation";

	public const string RESERVOIR_FISH_CAUGHT_POSTFIX = "_caught";

	public const string BUILDING_WINDOW_DEFAULT_TAB_ID = "tab_building_default";

	public const int CRAFT_INV_SIZE_FOR_ZOMBIE_INSERTABLE_WGO = 50;

	public const int MAX_PRODUCT_TIER = 3;

	public const int START_MONEY = 50;

	public const int MAX_INGREDIENTS_IN_ALCHEMY = 3;

	public const int MAX_RUNES_IN_ALCHEMY = 5;

	public const int MIN_RUNES_IN_ALCHEMY = 0;

	public const int CRAFT_MAX_DURATION = 18;

	public const int MAX_FIGHTER_CONTAINERS = 4;

	public const int MERCENARIES_AMOUNT = 4;

	public const string FIGHTBACK_PLAYER_SPAWN = "RT_fightback_player_spawn";

	public const float TALENT_COEFFICIENT_VALUE = 0.1f;

	public const float DOCK_SEARCH_RADIUS = 1.5f;

	public const float PLAYER_GRAPH_SIZE = 2.2f;

	public const float PLAYER_GRAPH_NODE_SIZE = 0.1f;

	public const float PLAYER_GRAPH_COLLIDER_DIAMETER = 0.28f;

	public const float MAGNETISM_DELAY_DURATION = 0.25f;

	public const float INTERACTIVE_DROPS_MAGNET_SPEED_K = 0.2f;

	public const float INTERACTIVE_DROPS_MAGNET_SPEED = 3f;

	public const float DROP_MOVEMENT_TO_POSITION_SPEED = 2f;

	public const float DROP_MIN_DISTANCE_TO_STOP = 0.1f;

	public const float DROP_MAX_DISTANCE_TO_STOP = 4f;

	public const float SLEEP_GAME_RES_DROP_COLLECT_DURATION = 1f;

	public const float PLAYER_DROP_OFFSET = 0.65f;

	public const float DROP_OFFSET_RIGHT = 0.3f;

	public const float DROP_OFFSET_FORWARD = 0.4f;

	public const float PLAYER_BACK_OFFSET = 0.5f;

	public const float PLAYER_COLLIDER_RADIUS = 0.22f;

	public const int DROP_ITEM_COUNT_LIMIT_TO_STACKING = 5;

	public const float TELEPORT_FADE_DELAY = 0.3f;

	public static Vector3 BIG_DROP_COLLIDER_SIZE = new Vector3(0.8f, 0.2f, 0.4f);

	public static readonly Vector3 OVERHEAD_STACK_OFFSET = new Vector3(0f, 0.3f, -0.04f);

	public const string EXTRA_OVERHEAD_KEY = "extra_overhead";

	public const float BIG_DROP_MAX_ELEVATION_DELTA = 1.5f;

	public const float DISTANCE_ANGLE_COMPENSTATION_COEFF = 0.0026666666f;

	public const string EXTRACT_ORGAN_CRAFT_PREFIX = "extract_";

	public const string INSERT_ORGAN_CRAFT_PREFIX = "insert_";

	public const string CHANGE_ORGAN_CRAFT_PREFIX = "change_";

	public const string EMBALM_CRAFT_PREFIX = "embalm_";

	public const string EXTRACT_ITEM_FROM_POCKET_CRAFT = "pocket_extract_item";

	public static List<ItemType> MAIN_ORGANS_TYPES = new List<ItemType>
	{
		ItemType.Bones,
		ItemType.Brain,
		ItemType.Heart,
		ItemType.Guts,
		ItemType.Skin,
		ItemType.Skull
	};

	public const string ZOMBIE_DEFAULT_COLLAR_ID = "collar_bronze";

	public const string MORGUE_PALLETS_GROUP = "morgue_pallets";

	public const string DONATION_BOX_SERMON_EVENT_ID = "sermon_reward";

	public const string PALETTE_TRADING_REWARD = "palette_trading_reward";

	public const string CASHBOX_TRADE_EVENT_ID = "cashbox_reward";

	public const string HAPPINESSBOX_EVENT_ID = "happinessbox_reward";

	public const string SERMON_READY = "sermon_ready";

	public const string CHURCH_CHOIR_WGO_ID = "zmb_choir_place";

	public const string CHURCH_ORGAN_WGO_ID = "zmb_organ_place";

	public const string PS5_ACTIVITY_ID = "continue";

	public const float MAX_WIND_SPEED = 1f;

	public const string CONVEYOR_WORLD_ZONE_ID = "conveyor";

	public static string NO_LUT = "NO LUT";
}
