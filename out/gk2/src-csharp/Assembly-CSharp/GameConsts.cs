using System.Collections.Generic;

public static class GameConsts
{
	public static class Icons
	{
		public const string PLACEHOLDER_ITEM_ICON = "i_placeholder";

		public const string PLACEHOLDER_BODY_ICON = "i_body";

		public const string PLACEHOLDER_CRAFT_RESULT_ICON = "i_b_blueprint_placeholder";

		public const string UNKNOWN_MIX_ICON = "i_slot-question";

		public const string STAR_ICON_PREFIX = "item_star_";

		public const string WHITE_SKULL_ICON = "skull";

		public const string RED_SKULL_ICON = "rskull";

		public const string WREATH_ICON = "wr";

		public const string WREATH_ICON_RED = "wr_red";

		public const string TIME_PARAM_ICON = "icon_time";

		public const string STAR_PARAM_ICON = "icon_star";

		public const string INSPIRATION_HINT_ICON = "hint_inspiration";

		public const string ENERGY_HINT_ICON = "energy";

		public const string INSANITY_HINT_ICON = "insanity";

		public const string INSANITY_LOCK_HINT_ICON = "icon_sanity_lock";

		public const string NOT_ENOUGH_ENERGY_HINT_ICON = "no_energy";

		public const string NOT_ENOUGH_INSANITY_HINT_ICON = "no_insanity";

		public const string MASTERY_BONUS_GARDEN_ICON = "icon_shovel";

		public const string MASTERY_BONUS_GENERATOR_ICON = "icon_hand";

		public const string LOCK_ICON = "icon_lock";

		public const string INFINITY_HINT_ICON = "∞";

		public const string ENERGY_1 = "energy_1";

		public const string ENERGY_2 = "energy_2";

		public const string ENERGY_3 = "energy_3";

		public const string INSANITY_1 = "insanity_1";

		public const string INSANITY_2 = "insanity_2";

		public const string INSANITY_3 = "insanity_3";

		public const string TOWN_QUALITY_ICON = "reputation-citizens";
	}

	public static class WGOs
	{
		public const string EXHUMATION_GRAVE_WGO_ID = "grave_exhume";

		public const string BODY_GRAVE_WGO_ID = "grave_body";

		public const string EMPTY_GRAVE_WGO_ID = "grave_empty";

		public const string GROUND_GRAVE_WGO_ID = "grave_ground";

		public const string SEED_GENERATOR_WGO_ID = "seed_generator";

		public const string MERCENARY_FIGHTER_CONTAINER_WGO_ID = "fighter_container_mercenary";

		public const string FIGHTER_CONTAINER_WGO_ID = "fighter_container";

		public const string MERCENARY_FIGHTER_PREFIX_ID = "npc_town_barracks_mercenary";

		public const string ZOMBIE_FIGHTER_ALLY = "zmb_wild_mob_allie";

		public const string ALCHEMY_WORKBENCH = "alchemy_workbench";

		public const string GARDEN_EMPTY_BED = "garden_empty";

		public const string VINEYARD_EMPTY_BED = "vineyard_empty";

		public const string GARDEN_PREFIX = "garden_";

		public const string VINEYARD_PREFIX = "vineyard_";

		public const string GARDEN_VINEYARD_READY_POSTFIX = "_ready";

		public const string WOOD_CONTAINER = "wood_container";

		public const string CRATES_SMALL = "crates_small";

		public const string WAREHOUSE_CRANE = "warehouse_crane";
	}

	public static class Groups
	{
		public static List<string> fertylizerGroups = new List<string> { "fert_star", "fert_time", "fert_crop" };

		public const string BODY_ITEM_GROUP = "body";

		public const string CORPSE_ITEM_GROUP = "corpse";

		public const string ZOMBIE_ITEM_GROUP = "zombie";

		public const string WILD_ZOMBIE = "wild_zombie";

		public const string TOMBSTONE_ITEM_GROUP = "gravetop";

		public const string FENCE_ITEM_GROUP = "gravebot";

		public const string BURIAL_REWARD_ITEM_GROUP = "burial_reward";

		public const string BODY_PART_ITEM_GROUP = "bodypart";

		public const string TOOL_ITEM_GROUP = "tool";

		public const string WEAPON_ITEM_GROUP = "weapon";

		public const string FAT_ITEM_GROUP = "gr_fat";

		public const string ORGAN_MISTAKE_ITEM_GROUP = "mistake";

		public const string SEED_ITEM_GROUP = "seed";

		public const string SEEDABLE_ITEM_GROUP = "seedable";

		public const string FERTILIZER_ITEM_GROUP = "fertilizer";

		public const string FUEL_ITEM_GROUP = "fuel";

		public const string MERCENARY_WGO_GROUP = "mercenary";

		public const string MELEE_WEAPON_ITEM_GROUP = "melee";

		public const string RANGE_WEAPON_ITEM_GROUP = "range";

		public const string SPAWNGER_WGO_GROUP = "spawner";

		public const string VINEYARD_SEED_ITEM_GROUP = "vineyard_seed";

		public const string VINEYARD_OBJECTS_GROUP = "vineyard_objects";

		public const string GARDEN_BED_WGO_GROUP = "garden_bed";

		public const string BARRICADES_WGO_GROUP = "barricades";

		public const string TOWERS_WGO_GROUP = "towers";

		public const string SUPPLY_BOX_ITEM_GROUP = "town_box";

		public const string BATTLE_POTION_ITEM_GROUP = "battle_potion";
	}

	public static class GameLogic
	{
		public const string FISHING_RESTORE_POSTFIX = "_restore";

		public const int SEED_PURCHASE_STACK_SIZE = 4;
	}

	public static class Items
	{
		public const string BODY_CORPSE_ITEM = "body_corpse";

		public const string BODY_ZOMBIE_ITEM = "body_zombie";

		public const string EXHUME_CERTIFICATE_ITEM = "exhume_certificate";

		public const string NO_BAIT = "no_bait";

		public const string WOOD = "wood";

		public const string WATER = "water";
	}

	public static class Crafts
	{
		public const string CORPSE_TO_ZOMBIE_CRAFT = "corpse_zombie_transition";
	}

	public static class Buildings
	{
		public const string GRAVEYARD_MODULE_PLACE = "graveyard_module_p";
	}

	public static class Hints
	{
		public const string CRAFT = "hint_craft";

		public const string OPEN = "hint_open";

		public const string INSPECT = "action_inspect";

		public const string TAKE = "hint_take";

		public const string PUT = "hint_put";

		public const string TAKE_ALL = "hint_take_all";

		public const string WORK = "hint_work";

		public const string PLACE_BODY = "hint_place_body";

		public const string BUILD = "hint_build";

		public const string CLIMB = "hint_climb";

		public const string PRAY = "hint_pray";

		public const string INTERACT = "hint_interact";

		public const string INTERACTION = "hint_interaction";

		public const string FISHING = "ui_submit_bait";

		public const string PUT_ZOMBIE = "hint_put_zombie";

		public const string SURVEY = "hint_survey";

		public const string SET_TO_HOT_BAR = "hint_set_to_hot_bar";

		public const string MIX = "hint_alchemy";

		public const string PLACE_FLAG = "hint_put_flag";

		public const string TAKE_FLAG = "hint_take_flag";

		public const string PLANT = "hint_plant";

		public const string FERTILIZE = "hint_fertilize";

		public const string CREMATE_BODY = "hint_cremate_body";

		public const string DROP_BODY = "hint_drop_body";

		public const string GRAVE = "hint_grave";
	}
}
