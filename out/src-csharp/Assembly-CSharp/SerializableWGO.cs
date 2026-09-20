using System;
using System.Collections.Generic;
using LinqTools;
using UnityEngine;

[Serializable]
public struct SerializableWGO
{
	[Serializable]
	public struct SerializableChunk
	{
		public bool always_active;

		public bool active_now_because_of_movement;

		public bool active_now_because_of_events;

		public bool active_now_because_of_work;
	}

	[Serializable]
	public struct SerializableCraft
	{
		public bool available;

		public bool is_crafting;

		public string cur_craft_id;

		public string cur_item_id;

		public float cur_item_dur;

		public string dur_item_id;

		public float dur_item_dur;

		public string multiquality_item_id;

		public CraftDefinition.MultiqualityCraftResult multiquality_craft_result;

		public int craft_amount;

		public string last_craft_id;

		public string last_craft_id_2;

		public int cur_last_craft_slot;

		public List<CraftComponent.CraftQueueItem> queue;

		public List<Item> cur_craft_items_used;

		public bool is_gratitude_points_spent_for_craft;
	}

	[Serializable]
	public struct SerializebleMovementComponent
	{
		public bool avaliable;

		public string cur_astar_path;

		public int path_waypoint;

		public MovementComponent.MovementState state;

		public string event_on_complete;

		public string anchor_gd_tag;

		public string anchor_custom_tag;

		public bool anchor_is_wgo;

		public bool using_gd_path;

		public int idle_animation;

		public string target_gd_point_tag;

		public Vector2 astar_dest;

		public Vector2 current_point_pos;

		public Vector3 current_pos;

		public float stored_speed;

		public bool in_stored_speed_mode;
	}

	[Serializable]
	public struct SerializeblePorterStation
	{
		public PorterStation.PorterState state;

		public List<string> items_black_list;
	}

	public Vector3 position;

	public string obj_id;

	public Quaternion rotation;

	public Vector3 scale;

	public string custom_tag;

	[SmartDontSerialize]
	public string item_data;

	[SmartSerialize]
	public Item item;

	public int variation;

	public int variation_2;

	public float auto_craft_time_spent;

	public SerializableCraft craft;

	public SerializebleMovementComponent movement_component;

	public string wop_skin_id;

	public long unique_id;

	public long linked_worker_unique_id;

	public long worker_unique_id;

	public SerializableChunk chunk;

	public string interaction_events;

	public string events_json;

	public WorldGameObject.SerializableEvents events_as_class;

	public string cur_gd_point;

	public string cur_zone;

	public float floor_line;

	public int fine_tune_z;

	public string anim_state_json;

	public string parent_gd_point;

	public string last_opened_tab;

	public bool idle_serialized;

	public BaseCharacterIdle.SerializableCharacterIdle idle;

	public bool has_spawner;

	public Vector2 spawner_coords;

	public SerializeblePorterStation porter_station;

	public bool is_current_craft_gratitude;

	public static SerializableWGO FromWGO(WorldGameObject wgo)
	{
		Transform transform = wgo.transform;
		if (string.IsNullOrEmpty(wgo.obj_id))
		{
			Debug.LogError("WGO with an empty ID found, name = " + wgo.name, wgo);
		}
		SerializableWGO serializableWGO = default(SerializableWGO);
		serializableWGO.obj_id = wgo.obj_id;
		serializableWGO.unique_id = wgo.unique_id;
		serializableWGO.linked_worker_unique_id = wgo.linked_worker_unique_id;
		serializableWGO.worker_unique_id = wgo.worker_unique_id;
		serializableWGO.position = transform.position;
		serializableWGO.rotation = transform.localRotation;
		serializableWGO.scale = transform.localScale;
		serializableWGO.custom_tag = wgo.custom_tag;
		serializableWGO.variation = wgo.variation;
		serializableWGO.variation_2 = wgo.variation_2;
		serializableWGO.auto_craft_time_spent = wgo.auto_craft_time_spent;
		serializableWGO.craft = wgo.components.craft.GetSerializedCraftComponent();
		serializableWGO.wop_skin_id = ((wgo.GetWOP() == null) ? "" : wgo.wop.skin_id);
		serializableWGO.interaction_events = ((wgo.custom_interaction_events.Count == 0) ? null : string.Join(",", wgo.custom_interaction_events.ToArray()));
		serializableWGO.cur_gd_point = wgo.cur_gd_point;
		serializableWGO.cur_zone = wgo.cur_zone;
		serializableWGO.anim_state_json = "";
		serializableWGO.last_opened_tab = wgo.last_opened_tab;
		serializableWGO.is_current_craft_gratitude = wgo.is_current_craft_gratitude;
		SerializableWGO d = serializableWGO;
		d.item = wgo.data;
		if (wgo.obj_def.IsPorterStation())
		{
			d.porter_station = wgo.porter_station.Serialize();
		}
		if (wgo.obj_def != null && wgo.obj_def.IsNPC())
		{
			d.anim_state_json = JsonUtility.ToJson(wgo.components.animator.stored_state);
		}
		GDPoint parentGDPoint = wgo.GetParentGDPoint();
		d.parent_gd_point = ((parentGDPoint == null) ? null : parentGDPoint.gd_tag);
		RoundAndSortComponent component = wgo.gameObject.GetComponent<RoundAndSortComponent>();
		if (component != null)
		{
			d.floor_line = component.floor_line;
			d.fine_tune_z = component.fine_tune_z;
		}
		wgo.AdditionalSerialize(ref d);
		ChunkedGameObject chunkedGameObject = wgo.GetComponentInChildren<ChunkedGameObject>();
		if (chunkedGameObject == null)
		{
			chunkedGameObject = wgo.gameObject.AddComponent<ChunkedGameObject>();
		}
		d.chunk = chunkedGameObject.SerializeChunk();
		d.movement_component = wgo.components.character.GetSerializedMovementComponent();
		d.idle = (wgo.components.character.idle_used ? wgo.components.character.idle.Serialize() : null);
		d.idle_serialized = d.idle != null;
		return d;
	}

	public void ToWGO(WorldGameObject wgo, out Item wgo_data)
	{
		Transform transform = wgo.transform;
		transform.position = position;
		transform.localRotation = rotation;
		transform.localScale = scale;
		wgo.unique_id = unique_id;
		wgo.linked_worker_unique_id = linked_worker_unique_id;
		wgo.worker_unique_id = worker_unique_id;
		wgo.custom_tag = custom_tag;
		wgo.custom_interaction_events = ((interaction_events == null) ? new List<string>() : interaction_events.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList());
		wgo.is_current_craft_gratitude = is_current_craft_gratitude;
		wgo.round_and_sort.floor_line = floor_line;
		wgo.round_and_sort.fine_tune_z = fine_tune_z;
		if (string.IsNullOrEmpty(item_data))
		{
			wgo_data = ((item == null) ? new Item() : item);
		}
		else
		{
			wgo_data = JsonUtility.FromJson<Item>(item_data);
		}
		wgo.variation = variation;
		wgo.variation_2 = variation_2;
		wgo.auto_craft_time_spent = auto_craft_time_spent;
		wgo.obj_id = obj_id;
		wgo.obj_def = GameBalance.me.GetData<ObjectDefinition>(obj_id);
		wgo.cur_gd_point = cur_gd_point;
		wgo.cur_zone = cur_zone;
		wgo.last_opened_tab = last_opened_tab;
		wgo.components.InitAllComponents();
		wgo.GetComponentInChildren<ChunkedGameObject>().DeserializeChunk(this);
		if (!string.IsNullOrEmpty(wop_skin_id))
		{
			wgo.ApplySkin(wop_skin_id);
		}
		wgo.components.character.DeserializeMovementComponent(movement_component);
		wgo.components.craft.DeserializeCraftComponent(craft);
		wgo.AdditionalDeserialize(ref this);
		if (!string.IsNullOrEmpty(parent_gd_point))
		{
			GDPoint gDPointByGDTag = WorldMap.GetGDPointByGDTag(parent_gd_point, log_if_null: true, skip_disabled: false);
			if (gDPointByGDTag == null)
			{
				Debug.LogError("Couldn't find GD point with tag = " + parent_gd_point);
			}
			else
			{
				wgo.transform.SetParent(gDPointByGDTag.transform);
			}
		}
		if (idle_serialized)
		{
			wgo.components.character.DeserializeIdle(idle);
		}
	}

	public void ToWGOAfterSetID(WorldGameObject wgo)
	{
		if (!string.IsNullOrEmpty(anim_state_json))
		{
			wgo.components.animator.DeserializeFromSavedState(anim_state_json);
			if (wgo.components.animator.stored_state.HasParameter("direction_angle"))
			{
				float parameterFloat = wgo.components.animator.stored_state.GetParameterFloat("direction_angle");
				if (parameterFloat.EqualsTo(90f))
				{
					wgo.components.character.direction = Vector2.up;
				}
				else if (parameterFloat.EqualsTo(180f) || parameterFloat.EqualsTo(-180f))
				{
					wgo.components.character.direction = Vector2.left;
				}
				else if (parameterFloat.EqualsTo(0f))
				{
					wgo.components.character.direction = Vector2.right;
				}
			}
		}
		if (wgo.obj_def.IsPorterStation())
		{
			wgo.porter_station.Deserialize(porter_station);
		}
	}
}
