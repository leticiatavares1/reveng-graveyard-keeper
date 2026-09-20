using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ObjectDefinition : BalanceBaseObject
{
	public enum DamageType
	{
		Damage_0,
		Damage_1,
		Damage_2,
		Damage_3,
		Damage_4,
		Damage_5,
		Damage_6,
		Damage_7,
		Damage_8,
		Damage_9
	}

	public enum ObjType
	{
		Default,
		Mob,
		NPC,
		PorterStation,
		SoulTotem
	}

	public enum InteractionType
	{
		None = 0,
		Craft = 1,
		RunScript = 2,
		Builder = 4,
		Chest = 5,
		Grave = 6
	}

	public enum DropPoint
	{
		Auto,
		Center
	}

	public enum QualityType
	{
		Hidden,
		Shown,
		Grave
	}

	public enum GlobalControlAccess
	{
		None,
		Ignore,
		ForceAdd
	}

	public enum HintPos
	{
		Default,
		AbovePlayer
	}

	public const string DAMAGE = "damage";

	public const string SPEED = "speed";

	public const string SPEED_BUFF = "speed_buff";

	public const string ACCELERATION = "acceleration";

	public const string ACCELERATE_ALWAYS = "accelerate_always";

	public const string FRICTION = "friction";

	public const string DROP_MAGNET_DIST = "drop_magnet_dist";

	public const string KICK_FRICTION = "kick_friction";

	public const string KICK_SPEED = "kick_speed";

	public const int MAX_DAMAGE_TYPES = 10;

	public ObjType type;

	public GameRes res;

	public List<Item> drop_items;

	public SmartExpression hp;

	public int inventory_size;

	public bool drop_inventory_after_remove;

	public GlobalControlAccess global_craft_control_access;

	public bool do_drop_after_dying_anim_finished;

	public bool open_in_multiinventory;

	public bool dynamic_mob;

	public QualityType quality_type;

	public string inventory_preset = "";

	public ToolActions tool_actions = new ToolActions();

	public ChancedStringValue after_hp_0;

	public bool save_variation;

	public InteractionType interaction_type;

	public bool can_insert_zombie;

	public bool player_cant_work;

	public ObjectInteractionDefinition pre_interaction_1;

	public ObjectInteractionDefinition pre_interaction_2;

	public ObjectInteractionDefinition interaction_1;

	public ObjectInteractionDefinition interaction_2;

	public string script_after_hp_0 = "";

	public string craft_after_hp_0 = "";

	public string work = "";

	public string attached_script = "";

	public bool need_unlock_work;

	[SerializeField]
	private List<string> _object_groups;

	[SerializeField]
	private List<string> _affected_object_groups;

	public DropPoint drop_point;

	public bool has_craft;

	public float damage_factor;

	public GameRes set_param_after_hp_0 = new GameRes();

	public GameRes add_param_after_hp_0 = new GameRes();

	public GameRes set_param_after_hp_0_end = new GameRes();

	public GameRes add_player_param_after_hp_0 = new GameRes();

	public SmartExpression add_player_param_after_hp_0_k;

	public string zone_id;

	public GameRes totem_params = new GameRes();

	public float totem_radius;

	public SmartExpression quality;

	public float quality_multiplier;

	public string overrode_quality_icon;

	public bool has_overrode_quality_icon;

	public bool ignore_counting_at_zone;

	public List<string> can_insert_items = new List<string>();

	public List<CustomItemInsertion> custom_insertions = new List<CustomItemInsertion>();

	public int can_insert_items_limit;

	public float durability_modificator;

	public List<string> res_product_types = new List<string>();

	public string craft_preset = "";

	public SmartSpeechEngine.VoiceID voice_id;

	public string ovr_music = "";

	public string custom_interaction_icon = "";

	public string work_sfx = "";

	public string work_end_sfx = "";

	public float mass;

	public float drag;

	public float armor;

	public bool npc_in_list;

	public bool check_only_interactions;

	public string npc_alias;

	public string custom_head_spr;

	public CraftDefinition.CraftSubType filter_craft_subtype;

	public string custom_icon;

	public List<string> additional_header_items;

	public bool can_belong_to_zone = true;

	public bool interactive_in_tutorial = true;

	public HintPos hint_pos;

	public string day_icon;

	public int sort_n = 99999;

	public string drop_sound = "";

	public bool dont_restore_last_craft;

	public List<string> additional_worldzone_inventories = new List<string>();

	public int multi_inventory_priority = 100;

	public bool always_active;

	public string craft_start_sound = "";

	public string anim_on_craft_start = "";

	public string anim_on_craft_finish = "";

	[NonSerialized]
	private List<ObjectGroupDefinition> _object_group_links;

	[NonSerialized]
	private List<ObjectGroupDefinition> _affected_object_group_links;

	public float acceleration => res.Get("acceleration");

	public bool accelerate_always => res.Get("accelerate_always") > 0f;

	public float friction => res.Get("friction");

	public float kick_speed => res.Get("kick_speed");

	public float kick_friction => res.Get("kick_friction");

	public List<ObjectGroupDefinition> object_groups
	{
		get
		{
			if (_object_group_links == null)
			{
				_object_group_links = new List<ObjectGroupDefinition>();
				foreach (string object_group in _object_groups)
				{
					_object_group_links.Add(GameBalance.me.GetData<ObjectGroupDefinition>(object_group));
				}
			}
			return _object_group_links;
		}
	}

	public List<ObjectGroupDefinition> affected_object_groups
	{
		get
		{
			if (_affected_object_group_links == null)
			{
				_affected_object_group_links = new List<ObjectGroupDefinition>();
				foreach (string affected_object_group in _affected_object_groups)
				{
					_affected_object_group_links.Add(GameBalance.me.GetData<ObjectGroupDefinition>(affected_object_group));
				}
			}
			return _affected_object_group_links;
		}
	}

	public bool IsNotInteractive(WorldGameObject wgo)
	{
		if (wgo.is_removing)
		{
			return false;
		}
		if (check_only_interactions)
		{
			return GetValidInteraction(wgo) == null;
		}
		ObjectInteractionDefinition validInteraction = GetValidInteraction(wgo);
		if (interaction_type == InteractionType.None && (validInteraction == null || string.IsNullOrEmpty(validInteraction.script)))
		{
			return tool_actions.no_actions;
		}
		return false;
	}

	public float Damage(int i)
	{
		return res.Get("damage_" + i);
	}

	public float Damage(DamageType dmg_type)
	{
		return Damage((int)dmg_type);
	}

	public ObjectInteractionDefinition GetValidInteraction(WorldGameObject wgo)
	{
		if (!Application.isPlaying)
		{
			return null;
		}
		ObjectInteractionDefinition[] array = ((!can_insert_zombie) ? new ObjectInteractionDefinition[2] { interaction_1, interaction_2 } : new ObjectInteractionDefinition[4] { pre_interaction_1, pre_interaction_2, interaction_1, interaction_2 });
		foreach (ObjectInteractionDefinition objectInteractionDefinition in array)
		{
			if (!string.IsNullOrEmpty(objectInteractionDefinition.hint) && objectInteractionDefinition.condition.EvaluateBoolean(wgo))
			{
				return objectInteractionDefinition;
			}
		}
		return null;
	}

	public string GetInteractionHint(WorldGameObject wgo)
	{
		if (wgo.is_removing)
		{
			return "";
		}
		ObjectInteractionDefinition validInteraction = GetValidInteraction(wgo);
		if (validInteraction == null)
		{
			return "";
		}
		switch (validInteraction.hint.ToLower())
		{
		case "speak":
		case "talk":
			return "(speak)";
		default:
			return validInteraction.hint;
		}
	}

	public bool DoesBelongToGroup(string group_id)
	{
		return _object_groups.Contains(group_id);
	}

	public bool IsTotem()
	{
		return !totem_radius.EqualsTo(0f);
	}

	public bool HasObjectGroupWithID(string obj_group_id)
	{
		foreach (ObjectGroupDefinition object_group in object_groups)
		{
			if (object_group.id == obj_group_id)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsCharacter()
	{
		if (type != ObjType.Mob)
		{
			return type == ObjType.NPC;
		}
		return true;
	}

	public bool IsMob()
	{
		return type == ObjType.Mob;
	}

	public bool IsNPC()
	{
		return type == ObjType.NPC;
	}

	public bool IsPorterStation()
	{
		return type == ObjType.PorterStation;
	}

	public bool IsRelationVisible()
	{
		if (!string.IsNullOrEmpty(npc_alias))
		{
			return true;
		}
		if (IsNPC())
		{
			return npc_in_list;
		}
		return false;
	}
}
