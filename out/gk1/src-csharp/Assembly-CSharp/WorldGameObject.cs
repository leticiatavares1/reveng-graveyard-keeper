using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DungeonGenerator;
using FlowCanvas;
using NodeCanvas.BehaviourTrees;
using NodeCanvas.Framework;
using SmartPools;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;

public class WorldGameObject : CustomNetworkBehaviour
{
	private enum CraftState
	{
		None,
		CraftingWithoutPlayer,
		CraftingAndWorking
	}

	[Serializable]
	public class SerializableEvents
	{
		public List<float> event_delays = new List<float>();

		public List<string> event_ids = new List<string>();
	}

	public bool temp_do_work;

	private const string ID_NOT_SET = "_not_set_";

	public const string DISABLED_INTERACTIONS = "disabled_interactions";

	public const string DYING_TRIGGER = "do_dying";

	public const string TIREDNESS = "tiredness";

	public const float MAX_TIREDNESS = 300f;

	public const string TIRED_BUFF_NAME = "buff_tired";

	public const string TIRED_PARAM_NAME = "tired";

	public const string OUT_ANY_CUSTOM_LOOP = "any_custom_loop_out";

	[SerializeField]
	private CustomDrawers _custom_drawers;

	[HideInInspector]
	public string obj_id = "";

	private string _obj_id = "_not_set_";

	public string custom_tag = "";

	public WorldObjectPart wop;

	public List<WorldObjectPart> additional_wops = new List<WorldObjectPart>();

	private WorldGameObject _linked_worker;

	private bool _has_linked_worker;

	public long linked_worker_unique_id;

	public WorldGameObject linked_workbench;

	public long worker_unique_id = -1L;

	private PorterStation _porter_station;

	private Vendor _vendor;

	public Transform content_tf;

	private Vector3 _content_local_pos;

	[HideInInspector]
	public Transform tf;

	public ObjectDefinition obj_def;

	[HideInInspector]
	public int path_cell_size = 1;

	[SerializeField]
	private Item _data = new Item();

	[NonSerialized]
	public long unique_id = -1L;

	private bool _prepared_for_interaction;

	private bool _is_player;

	private ItemDefinition.ItemType _current_item_type;

	private float _anim_action_delay;

	private DockPoint[] _dock_points = new DockPoint[0];

	private Camera _cam;

	private bool _cam_cached;

	private BehaviourTreeOwner _beh_tree;

	private Blackboard _blackboard;

	private CanGoTransparent[] _trnsps;

	private Vector3 _cached_pos = Vector3.zero;

	private int _cached_pos_frame = -1;

	private float _object_alpha = 1f;

	public int variation;

	public int variation_2;

	public float auto_craft_time_spent;

	private FlowScriptController _fsc;

	private WorldZone _zone;

	[NonSerialized]
	public GameRes totem_effect = new GameRes();

	private SkinChanger _skin_changer;

	private ComponentsManager _components_manager;

	private bool _components_inited;

	private bool _has_removal_craft;

	private bool _tried_to_find_removal_craft;

	[NonSerialized]
	public List<string> custom_interaction_events = new List<string>();

	private WGOMark.MarkType _mark_type;

	private WGOMark _mark;

	private bool _obj_modified_this_frame;

	private float _obj_quality;

	private BubbleWidgetDataContainer _bubble;

	[NonSerialized]
	public bool show_quality_hint;

	[NonSerialized]
	private List<IntVector2> _cells = new List<IntVector2>();

	[NonSerialized]
	private List<IntVector2> _cells_totem_local = new List<IntVector2>();

	[NonSerialized]
	public bool is_removed;

	public bool is_dead;

	private bool _already_dropped_drop;

	public string cur_gd_point = string.Empty;

	public string cur_zone = string.Empty;

	[NonSerialized]
	public bool just_built;

	private SerializableEvents _events = new SerializableEvents();

	[NonSerialized]
	private GDPoint _parent_gd_point;

	[NonSerialized]
	private bool _parent_gd_point_inited;

	private Transform _stored_parent_tf;

	private bool _is_marked_removable;

	private bool _is_sort_over_everything;

	public string last_opened_tab = string.Empty;

	private bool _round_and_sort_inited;

	private RoundAndSortComponent _round_and_sort;

	private bool _shown_tutorial_disabled;

	private bool _was_ever_active;

	private bool _ondestroy_was_called;

	public bool is_current_craft_gratitude;

	public bool has_linked_worker => _has_linked_worker;

	public RoundAndSortComponent round_and_sort
	{
		get
		{
			if (!_round_and_sort_inited)
			{
				_round_and_sort_inited = true;
				_round_and_sort = GetComponent<RoundAndSortComponent>();
				if (round_and_sort == null)
				{
					_round_and_sort = base.gameObject.AddComponent<RoundAndSortComponent>();
				}
			}
			return _round_and_sort;
		}
	}

	public ComponentsManager components
	{
		get
		{
			if (!_components_inited)
			{
				_components_inited = true;
				_components_manager = new ComponentsManager(this);
			}
			return _components_manager;
		}
	}

	public CustomDrawers custom_drawers
	{
		get
		{
			if (_custom_drawers == null)
			{
				_custom_drawers = new CustomDrawers();
			}
			if (_custom_drawers.wobj != this)
			{
				_custom_drawers.SetWobj(this);
			}
			return _custom_drawers;
		}
	}

	public Vendor vendor
	{
		get
		{
			if (_vendor == null)
			{
				VendorDefinition dataOrNull = GameBalance.me.GetDataOrNull<VendorDefinition>(obj_id);
				if (data.inventory_size == 0)
				{
					data.SetInventorySize(obj_def.inventory_size);
				}
				if (dataOrNull != null)
				{
					MultiInventory vendor_inventories = new MultiInventory(new Inventory(this));
					_vendor = new Vendor(vendor_inventories, dataOrNull, _data);
				}
			}
			return _vendor;
		}
	}

	public float quality_k => GetParam("quality_k", 1f);

	public float quality
	{
		get
		{
			if (obj_def == null)
			{
				return 0f;
			}
			if (_data == null)
			{
				return obj_def.quality.EvaluateFloat();
			}
			if (Mathf.Abs(obj_def.quality_multiplier) < 0.001f)
			{
				return 0f;
			}
			if (obj_def.quality_type == ObjectDefinition.QualityType.Grave)
			{
				Item bodyFromInventory = GetBodyFromInventory();
				if (bodyFromInventory == null)
				{
					return obj_def.quality.EvaluateFloat();
				}
				string ignore_item_id = null;
				if (components.craft.is_crafting)
				{
					if (components.craft.current_craft.id.StartsWith("set_"))
					{
						ignore_item_id = components.craft.current_craft.needs[0].id;
					}
					else if (components.craft.current_craft.id.StartsWith("rem_"))
					{
						ignore_item_id = components.craft.current_craft.output[0].id;
					}
				}
				bodyFromInventory.GetBodySkulls(out var negative, out var _, out var positive_avaialble);
				return Mathf.Min(Mathf.Floor(_data.GetInventoryQuality(ignore_item_id)) - (float)negative, positive_avaialble);
			}
			float num = 1f - GetDecayFactor();
			return Mathf.Round((obj_def.quality.EvaluateFloat() + _data.GetInventoryQuality() * _data.GetInventoryQualityMultiplier() * num) * obj_def.quality_multiplier * quality_k * 10f) / 10f;
		}
	}

	public BubbleWidgetDataContainer bubble
	{
		get
		{
			if (_bubble == null)
			{
				_bubble = new BubbleWidgetDataContainer(this);
			}
			return _bubble;
		}
	}

	public WorldGameObject linked_worker
	{
		get
		{
			return _linked_worker;
		}
		set
		{
			if (_linked_worker != null)
			{
				_linked_worker.linked_workbench = null;
			}
			_linked_worker = value;
			if (_linked_worker != null && !_linked_worker.IsWorker())
			{
				Debug.LogError("Tried to set non-worker wgo as worker!");
				_linked_worker = null;
			}
			_has_linked_worker = _linked_worker != null;
			linked_worker_unique_id = ((_linked_worker != null) ? _linked_worker.unique_id : (-1));
			if (_linked_worker != null)
			{
				_linked_worker.linked_workbench = this;
			}
			if (obj_def.IsPorterStation())
			{
				porter_station.has_linked_worker = _has_linked_worker;
				porter_station.waiting_point = GetAvailableDockPointForZombie();
				if (_has_linked_worker)
				{
					_linked_worker.worker.UpdateWorkerSkin(Worker.WorkerActivity.Porter);
				}
			}
			else if (_linked_worker != null)
			{
				_linked_worker.worker.UpdateWorkerSkin(Worker.WorkerActivity.Worker);
			}
		}
	}

	public Worker worker
	{
		get
		{
			if (worker_unique_id <= 0)
			{
				return null;
			}
			return MainGame.me.save.workers.GetWorker(worker_unique_id);
		}
		set
		{
			if (value == null)
			{
				worker_unique_id = -1L;
			}
			else
			{
				worker_unique_id = value.worker_unique_id;
			}
		}
	}

	public PorterStation porter_station
	{
		get
		{
			if (obj_def.type != ObjectDefinition.ObjType.PorterStation)
			{
				return null;
			}
			if (_porter_station == null)
			{
				try
				{
					_porter_station = GetComponentInChildren<PorterStation>();
					if (_porter_station == null)
					{
						_porter_station = base.gameObject.AddComponent<PorterStation>();
						Debug.Log("Added PorterStation component to wgo " + base.name + "[" + obj_id + "]");
					}
					if (!_porter_station.is_correctly_inited)
					{
						_porter_station.Init();
					}
					ChunkedGameObject component = GetComponent<ChunkedGameObject>();
					if (component == null)
					{
						Debug.LogError("PorterStation creation error: ChunkedGameObject not found");
					}
					else
					{
						component.active_now_because_of_work = true;
					}
				}
				catch (Exception message)
				{
					Debug.LogError(message);
				}
			}
			return _porter_station;
		}
	}

	public Vector2 pos
	{
		get
		{
			if (MainGame.game_started)
			{
				CheckPositionCache();
			}
			return _cached_pos;
		}
	}

	public Vector3 pos3
	{
		get
		{
			if (MainGame.game_started)
			{
				CheckPositionCache();
			}
			return _cached_pos;
		}
	}

	public Vector2 grid_pos => pos / 96f;

	public bool prepared_for_interaction => _prepared_for_interaction;

	public bool is_player
	{
		get
		{
			return _is_player;
		}
		set
		{
			_is_player = value;
		}
	}

	public bool has_dock_points => _dock_points.Length != 0;

	public bool dont_update { get; set; }

	public Item data => _data;

	public bool is_autopsy_table
	{
		get
		{
			if (!(obj_id == "autopsi_table"))
			{
				return obj_id.Contains("mf_preparation");
			}
			return true;
		}
	}

	public bool is_body_storage
	{
		get
		{
			if (!obj_id.StartsWith("corpse_bed"))
			{
				return obj_id.StartsWith("corpse_fridge");
			}
			return true;
		}
	}

	public bool is_soul_extractor_table
	{
		get
		{
			if (!(obj_id == "soul_extractor") && !(obj_id == "soul_extractor_2"))
			{
				return obj_id == "soul_extractor_3";
			}
			return true;
		}
	}

	public bool is_rat_cell => obj_id.StartsWith("rat_cell");

	public bool playing_disappearing_anim { get; private set; }

	public Transform bubble_pos_tf
	{
		get
		{
			if (wop == null)
			{
				return tf;
			}
			BubbleCornerPoint bubbleCornerPoint = GetBubbleCornerPoint();
			if (!(bubbleCornerPoint != null))
			{
				return tf;
			}
			return bubbleCornerPoint.transform;
		}
	}

	public Vector3 bubble_pos => bubble_pos_tf.position;

	public Camera cam
	{
		get
		{
			if (_cam_cached)
			{
				return _cam;
			}
			_cam_cached = true;
			return _cam = Camera.main;
		}
	}

	public float hp
	{
		get
		{
			return _data.hp;
		}
		set
		{
			_data.hp = value;
		}
	}

	public float progress
	{
		get
		{
			return _data.progress;
		}
		set
		{
			_data.progress = value;
		}
	}

	public float energy
	{
		get
		{
			return GetParam("energy");
		}
		set
		{
			float num = value - GetParam("energy");
			SetParam("energy", (value > (float)MainGame.me.save.max_energy) ? ((float)MainGame.me.save.max_energy) : value);
			if (!_is_player || !(num < 0f))
			{
				return;
			}
			AddToParams("tiredness", Mathf.Abs(num));
			if (GetParam("tiredness") > 300f)
			{
				SetParam("tiredness", 300f);
				if (GetParamInt("tired") == 0)
				{
					BuffsLogics.AddBuff("buff_tired");
				}
			}
		}
	}

	public float gratitude_points
	{
		get
		{
			return GetParam("gratitude_points");
		}
		set
		{
			if (value < 0f)
			{
				value = 0f;
			}
			SetParam("gratitude_points", value);
		}
	}

	public float sanity
	{
		get
		{
			return GetParam("sanity");
		}
		set
		{
			SetParam("sanity", value);
			if (sanity > (float)MainGame.me.save.max_sanity)
			{
				sanity = MainGame.me.save.max_sanity;
			}
		}
	}

	public bool is_removing
	{
		get
		{
			return (double)data.GetParam("_removing") > 0.1;
		}
		set
		{
			data.SetParam("_removing", value ? 1 : 0);
		}
	}

	public bool has_removal_craft
	{
		get
		{
			if (!_tried_to_find_removal_craft)
			{
				_tried_to_find_removal_craft = true;
				ObjectCraftDefinition objectRemoveCraftDefinition = BuildModeLogics.GetObjectRemoveCraftDefinition(obj_id);
				_has_removal_craft = objectRemoveCraftDefinition != null;
				if (objectRemoveCraftDefinition != null && objectRemoveCraftDefinition.IsLocked())
				{
					_has_removal_craft = false;
				}
			}
			return _has_removal_craft;
		}
	}

	public bool player_cant_work
	{
		get
		{
			if (is_removing)
			{
				return false;
			}
			return obj_def.player_cant_work;
		}
	}

	public void ForceDeinitComponents()
	{
		_components_inited = false;
	}

	public WorldObjectPart GetWOP()
	{
		if (wop == null)
		{
			wop = GetComponentInChildren<WorldObjectPart>(includeInactive: true);
		}
		return wop;
	}

	public bool IsWorker()
	{
		return worker_unique_id > 0;
	}

	public bool IsInvisibleWorker()
	{
		return obj_id == "worker_invisible";
	}

	private void CheckPositionCache()
	{
		int frameCount = Time.frameCount;
		if (_cached_pos_frame != frameCount)
		{
			RefreshPositionCache();
		}
	}

	public void RefreshPositionCache()
	{
		try
		{
			_cached_pos = tf.position;
		}
		catch (MissingReferenceException)
		{
			Debug.LogException(new Exception("tf is null for current WGO " + base.name), this);
			tf = base.transform;
			_cached_pos = tf.position;
		}
		_cached_pos_frame = Time.frameCount;
	}

	public void OnValidate()
	{
		custom_drawers.OnValidate();
	}

	public void RestoreSavedInventory(Item inventory)
	{
		_data = inventory;
	}

	public BubbleCornerPoint GetBubbleCornerPoint()
	{
		GameObject gameObject = ((wop == null) ? base.gameObject : wop.gameObject);
		BubbleCornerPoint[] componentsInChildren = gameObject.GetComponentsInChildren<BubbleCornerPoint>(includeInactive: true);
		if (componentsInChildren.Length == 0)
		{
			BubbleCornerPoint bubbleCornerPoint = new GameObject("auto bubble pos").AddComponent<BubbleCornerPoint>();
			bubbleCornerPoint.transform.SetParent(gameObject.transform, worldPositionStays: false);
			bubbleCornerPoint.transform.localPosition = new Vector3(0f, 0.55f, 0f);
			bubbleCornerPoint.transform.localScale = Vector3.one;
			return bubbleCornerPoint;
		}
		if (componentsInChildren.Length == 1)
		{
			return componentsInChildren[0];
		}
		BubbleCornerPoint[] array = componentsInChildren;
		foreach (BubbleCornerPoint bubbleCornerPoint2 in array)
		{
			if (bubbleCornerPoint2.gameObject.activeInHierarchy)
			{
				return bubbleCornerPoint2;
			}
		}
		return componentsInChildren[0];
	}

	public void Start()
	{
		_was_ever_active = true;
		if (_bubble == null)
		{
			_bubble = new BubbleWidgetDataContainer(this);
		}
		NetRegisterDelegate(DoActionNetSynced);
		_prepared_for_interaction = false;
		tf = base.transform;
		_beh_tree = base.gameObject.GetComponent<BehaviourTreeOwner>();
		_blackboard = base.gameObject.GetComponent<Blackboard>();
		RefindContentParent();
		if (Application.isPlaying)
		{
			InitNewObject(at_obj_start: true);
			components.StartComponents();
			_ = round_and_sort;
			InitDockPoints();
			UpdateTransparentParts();
			custom_drawers.OnObjectRedraw(force_redraw: true);
			if (GetWOP() != null && !string.IsNullOrEmpty(wop.skin_id))
			{
				ApplySkin(wop.skin_id);
			}
			WorldMap.OnAddNewWGO(this);
		}
	}

	public void Awake()
	{
		_was_ever_active = true;
		DynamicLights.SearchForLightsInNewObject(base.gameObject);
	}

	public void OnDestroy()
	{
		if (!_ondestroy_was_called)
		{
			_ondestroy_was_called = true;
			is_removed = true;
			DynamicLights.SearchForLightsInDestroyedObject(base.gameObject);
			if (Application.isPlaying)
			{
				InteractionBubbleGUI.RemoveBubble(unique_id);
			}
			WorldMap.OnDestroyWGO(this);
		}
	}

	private void RefindContentParent()
	{
		if (tf.childCount == 0)
		{
			content_tf = base.transform;
			_content_local_pos = Vector3.zero;
		}
		else if (tf.GetChild(0).gameObject.GetComponent<WorldObjectPart>() != null)
		{
			content_tf = base.transform;
			_content_local_pos = Vector3.zero;
		}
		else
		{
			content_tf = tf.GetChild(0);
			_content_local_pos = content_tf.localPosition;
		}
	}

	public Vector2 DirTo(WorldGameObject other_obj)
	{
		return DirTo(other_obj.pos);
	}

	public Vector2 DirTo(Vector2 other_pos)
	{
		return (other_pos - pos) / 96f;
	}

	public void RoundContentPos()
	{
		if (!(content_tf == null))
		{
			content_tf.RoundCamPos(cam, _content_local_pos);
		}
	}

	protected void InitDockPoints()
	{
		DockPoint[] array = RefindDockPoints();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].StartDocks(this);
		}
	}

	public void PlaceAtPos(Vector3 position)
	{
		position.z = tf.position.z;
		tf.position = position;
		if (is_player)
		{
			CameraTools.MoveToPos(pos);
		}
	}

	public int GetParamInt(string param_name)
	{
		return Mathf.RoundToInt(GetParam(param_name));
	}

	public float GetParam(string param_name, float default_value = 0f)
	{
		if (default_value.EqualsTo(1f))
		{
			return _data.GetParam(param_name, default_value) * totem_effect.Get(param_name, default_value);
		}
		return _data.GetParam(param_name, default_value) + totem_effect.Get(param_name, default_value);
	}

	public void AddToParams(GameRes game_res)
	{
		bool flag = IsPlayerInvulnerable();
		foreach (GameResAtom item in game_res.ToAtomList())
		{
			if (is_player)
			{
				switch (item.type)
				{
				case "hp":
				{
					float num2 = item.value;
					if (flag && num2 < 0f)
					{
						num2 = 0f;
					}
					hp += num2;
					if (hp > (float)MainGame.me.save.max_hp)
					{
						hp = MainGame.me.save.max_hp;
					}
					continue;
				}
				case "energy":
				{
					float num = item.value;
					if (flag && num < 0f)
					{
						num = 0f;
					}
					energy += num;
					continue;
				}
				}
			}
			_data.AddToParams(item);
		}
	}

	public void SubParam(string param_name, float value)
	{
		_data.SubFromParams(param_name, value);
	}

	public void AddToParams(string param_name, float value)
	{
		_data.AddToParams(param_name, value);
	}

	public void SetParam(string param_name, float value)
	{
		_data.SetParam(param_name, value);
		if (_is_player)
		{
			MainGame.me.save.quests.CheckQuestsState();
		}
	}

	public void SetParam(GameRes game_res)
	{
		_data.SetParam(game_res);
		foreach (GameResAtom item in game_res.ToAtomList())
		{
			bool flag = false;
			switch (item.type)
			{
			case "dur_cross":
			{
				Item itemOfType = GetItemOfType(ItemDefinition.ItemType.GraveStone);
				if (itemOfType != null)
				{
					itemOfType.durability = item.value;
				}
				flag = true;
				break;
			}
			case "dur_fence":
			{
				Item itemOfType = GetItemOfType(ItemDefinition.ItemType.GraveFence);
				if (itemOfType != null)
				{
					itemOfType.durability = item.value;
				}
				flag = true;
				break;
			}
			}
			if (flag)
			{
				_data.SetParam(item.type, 0f);
				Redraw(force_redraw: true);
			}
		}
	}

	public bool AddToInventory(string id, int value)
	{
		return AddToInventory(new Item(id, value));
	}

	public bool AddToInventory(List<Item> items)
	{
		bool flag = true;
		foreach (Item item in items)
		{
			flag = flag && AddToInventory(item);
		}
		return flag;
	}

	public bool AddToInventory(Item item)
	{
		OnBeganObjectModifications();
		if (!is_player && _data.inventory_size < obj_def.inventory_size)
		{
			_data.SetInventorySize(obj_def.inventory_size);
		}
		if (obj_def.custom_insertions.Count != 0)
		{
			foreach (CustomItemInsertion custom_insertion in obj_def.custom_insertions)
			{
				if (!(custom_insertion.item_id == item.id))
				{
					continue;
				}
				List<Item> list = new List<Item>();
				if (custom_insertion.insertion_type == CustomItemInsertion.InsertionType.OnUse)
				{
					foreach (Item item2 in item.definition.drop_on_use)
					{
						if (!item2.is_tech_point)
						{
							list.Add(item2);
						}
					}
					if (list.Count > 0)
					{
						bool flag = true;
						foreach (Item item3 in list)
						{
							flag = flag && _data.AddItem(item3);
						}
						return flag;
					}
					return true;
				}
				throw new ArgumentOutOfRangeException();
			}
		}
		if (_data.AddItem(item))
		{
			if (is_player)
			{
				MainGame.me.save.quests.CheckKeyQuests("inventory_change");
				MainGame.me.save.quests.CheckKeyQuests("item_" + item.id);
				GUIElements.me.hud.toolbar.Redraw();
			}
			CheckItemAutouse(item);
			return true;
		}
		return false;
	}

	public bool CheckItemAutouse(Item item)
	{
		if (item == null)
		{
			return false;
		}
		if (item.definition == null)
		{
			return false;
		}
		if (!item.definition.can_be_used)
		{
			return false;
		}
		if (!item.definition.autouse)
		{
			return false;
		}
		UseItemFromInventory(item);
		return true;
	}

	public MultiInventory GetMultiInventoryForInteraction(List<WorldGameObject> exceptions = null)
	{
		if (MainGame.me.build_mode_logics.IsBuilding())
		{
			return MainGame.me.build_mode_logics.multi_inventory;
		}
		if (GlobalCraftControlGUI.is_global_control_active)
		{
			return GetMultiInventory(exceptions, null, MultiInventory.PlayerMultiInventory.IncludePlayer, include_toolbelt: true);
		}
		WorldGameObject nearest = components.interaction.nearest;
		if (nearest != null)
		{
			if (nearest.GetMyWorldZone() != null)
			{
				return nearest.GetMultiInventory(exceptions, null, MultiInventory.PlayerMultiInventory.IncludePlayer, include_toolbelt: true);
			}
			return MainGame.me.player.GetMultiInventory(exceptions, null, MultiInventory.PlayerMultiInventory.DontChange, include_toolbelt: true);
		}
		return GetMultiInventory(exceptions, null);
	}

	public MultiInventory GetMultiInventoryOfWGOWithoutWorldZone(bool duplicate_bags = false)
	{
		MultiInventory multiInventory = new MultiInventory(new Inventory(this));
		if (duplicate_bags)
		{
			foreach (Item item in data.inventory)
			{
				if (item != null && !item.IsEmpty() && item.is_bag)
				{
					multiInventory.AddInventory(new Inventory(item, item.id));
				}
			}
		}
		return multiInventory;
	}

	public MultiInventory GetMultiInventory(List<WorldGameObject> exceptions = null, string force_world_zone = "", MultiInventory.PlayerMultiInventory player_mi = MultiInventory.PlayerMultiInventory.DontChange, bool include_toolbelt = false, bool sortWGOS = false, bool include_bags = false)
	{
		MultiInventory multiInventory = (obj_def.open_in_multiinventory ? (IsWorker() ? new MultiInventory() : new MultiInventory(new Inventory(this))) : new MultiInventory());
		bool flag = false;
		bool flag2 = false;
		if (GlobalCraftControlGUI.is_global_control_active && !is_player)
		{
			WorldZone zoneOfObject = WorldZone.GetZoneOfObject(this);
			if (zoneOfObject != null && !zoneOfObject.IsPlayerInZone())
			{
				player_mi = MultiInventory.PlayerMultiInventory.ExcludePlayer;
				include_bags = false;
			}
			else
			{
				player_mi = MultiInventory.PlayerMultiInventory.IncludePlayer;
				flag2 = true;
			}
		}
		bool flag3 = false;
		if (player_mi == MultiInventory.PlayerMultiInventory.IncludePlayer || flag2 || is_player)
		{
			Inventory inventory = new Inventory(MainGame.me.player);
			multiInventory.AddInventory(inventory);
			flag = true;
			if (include_toolbelt)
			{
				Item item = new Item
				{
					inventory = MainGame.me.player.data.secondary_inventory,
					inventory_size = 7
				};
				multiInventory.AddInventory(new Inventory(item));
			}
			int specific_position_num = 1;
			foreach (Item item2 in inventory.data.inventory)
			{
				if (item2 != null && !item2.IsEmpty() && item2.is_bag)
				{
					multiInventory.AddInventory(new Inventory(item2, item2.id), specific_position_num);
				}
			}
			flag3 = true;
		}
		if (include_bags && !flag3)
		{
			foreach (Item item3 in data.inventory)
			{
				if (item3 != null && !item3.IsEmpty() && item3.is_bag)
				{
					multiInventory.AddInventory(new Inventory(item3, item3.id));
				}
			}
		}
		WorldZone worldZone = null;
		if (!is_player && IsWorker())
		{
			if (linked_workbench == null)
			{
				return multiInventory;
			}
			worldZone = (string.IsNullOrEmpty(force_world_zone) ? linked_workbench.GetMyWorldZone() : WorldZone.GetZoneByID(force_world_zone));
		}
		if (worldZone == null)
		{
			worldZone = (string.IsNullOrEmpty(force_world_zone) ? GetMyWorldZone() : WorldZone.GetZoneByID(force_world_zone));
		}
		if (worldZone != null)
		{
			List<Inventory> multiInventory2 = worldZone.GetMultiInventory(exceptions, flag ? MultiInventory.PlayerMultiInventory.ExcludePlayer : player_mi, include_toolbelt, sortWGOS);
			if (multiInventory2 != null)
			{
				foreach (Inventory item4 in multiInventory2)
				{
					multiInventory.AddInventory(item4);
				}
			}
		}
		if (obj_def.additional_worldzone_inventories != null && obj_def.additional_worldzone_inventories.Count > 0)
		{
			MultiInventory.PlayerMultiInventory playerMultiInventory = player_mi;
			if (playerMultiInventory == MultiInventory.PlayerMultiInventory.IncludePlayer)
			{
				playerMultiInventory = MultiInventory.PlayerMultiInventory.DontChange;
			}
			foreach (string additional_worldzone_inventory in obj_def.additional_worldzone_inventories)
			{
				WorldZone zoneByID = WorldZone.GetZoneByID(additional_worldzone_inventory);
				if (zoneByID == null || zoneByID == worldZone)
				{
					continue;
				}
				foreach (Inventory item5 in zoneByID.GetMultiInventory(null, playerMultiInventory, include_toolbelt: false, sortWGOS))
				{
					multiInventory.AddInventory(item5);
				}
			}
		}
		else if (!is_player && IsWorker())
		{
			WorldGameObject worldGameObject = linked_workbench;
			if (worldGameObject?.obj_def?.additional_worldzone_inventories != null && (object)worldGameObject != null && worldGameObject.obj_def?.additional_worldzone_inventories.Count > 0)
			{
				MultiInventory.PlayerMultiInventory playerMultiInventory2 = player_mi;
				if (playerMultiInventory2 == MultiInventory.PlayerMultiInventory.IncludePlayer)
				{
					playerMultiInventory2 = MultiInventory.PlayerMultiInventory.DontChange;
				}
				foreach (string additional_worldzone_inventory2 in worldGameObject.obj_def.additional_worldzone_inventories)
				{
					WorldZone zoneByID2 = WorldZone.GetZoneByID(additional_worldzone_inventory2);
					if (zoneByID2 == null || zoneByID2 == worldZone)
					{
						continue;
					}
					foreach (Inventory item6 in zoneByID2.GetMultiInventory(null, playerMultiInventory2, include_toolbelt: false, sortWGOS))
					{
						multiInventory.AddInventory(item6);
					}
				}
			}
		}
		return multiInventory;
	}

	public void CustomUpdate()
	{
		if (Application.isPlaying && !dont_update && !is_dead)
		{
			float deltaTime = Time.deltaTime;
			_anim_action_delay -= deltaTime;
			components.Update(deltaTime);
			if (_trnsps != null)
			{
				UpdateTransparentParts();
			}
			UpdateDelayedEvents(deltaTime);
		}
	}

	private void UpdateDelayedEvents(float delta_time)
	{
		for (int i = 0; i < _events.event_ids.Count; i++)
		{
			_events.event_delays[i] -= delta_time;
			if (!(_events.event_delays[i] > 0f))
			{
				FireEvent(_events.event_ids[i]);
				_events.event_ids.RemoveAt(i);
				_events.event_delays.RemoveAt(i);
				i--;
			}
		}
		if (_events.event_ids.Count == 0)
		{
			GetComponent<ChunkedGameObject>().active_now_because_of_events = false;
		}
	}

	private void UpdateTransparentParts()
	{
		if (!Application.isPlaying || MainGame.disable_all_game)
		{
			return;
		}
		if (_trnsps == null)
		{
			_trnsps = base.gameObject.GetComponentsInChildren<CanGoTransparent>();
		}
		float num = 1f;
		Vector3 vector = pos3;
		Vector3 player_pos = MainGame.me.player_pos;
		float num2 = player_pos.z - vector.z;
		if (Mathf.Abs(vector.x - player_pos.x) < 100f && num2 > 0f && num2 < 50f)
		{
			num = 0.3f;
		}
		if (!num.EqualsTo(_object_alpha))
		{
			_object_alpha = num;
			CanGoTransparent[] trnsps = _trnsps;
			for (int i = 0; i < trnsps.Length; i++)
			{
				trnsps[i].SetAlpha(num);
			}
		}
	}

	public void CustomFixedUpdate()
	{
		if (!dont_update && !MainGame.game_starting)
		{
			components.FixedUpdate();
		}
	}

	public void CustomLateUpdate()
	{
		if (!Application.isPlaying || dont_update)
		{
			return;
		}
		if (!MainGame.paused)
		{
			components.LateUpdate();
		}
		if (_skin_changer != null)
		{
			_skin_changer.CustomLateUpdate();
		}
		if (MainGame.paused)
		{
			return;
		}
		if (_obj_modified_this_frame)
		{
			_obj_modified_this_frame = false;
			float num = quality - _obj_quality;
			if ((double)Mathf.Abs(num) > 0.05 && !is_player)
			{
				OnQualityChanged(num);
			}
		}
		just_built = false;
	}

	public bool DoAction(WorldGameObject other_obj, float delta_time = -1f)
	{
		if (CheckIfDisabledInTutorial())
		{
			return false;
		}
		if (delta_time < 0f)
		{
			delta_time = Time.deltaTime;
		}
		bool result = components.DoAction(other_obj, delta_time);
		DoAnimAction();
		return result;
	}

	private void DoActionNetSynced(WorldGameObject other_obj, float delta_time)
	{
		components.DoAction(other_obj, delta_time);
		DoAnimAction();
	}

	public void DoAnimAction()
	{
		if (!(_anim_action_delay > 0f))
		{
			_anim_action_delay = 0.5f;
			InteractionAnimation[] componentsInChildren = GetComponentsInChildren<InteractionAnimation>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].DoAction();
			}
		}
	}

	public void Interact(WorldGameObject other_obj, bool interaction_start, float delta_time = -1f)
	{
		if (delta_time < 0f)
		{
			delta_time = Time.deltaTime;
		}
		if (CheckDisabledInteractions())
		{
			return;
		}
		bool flag = false;
		if (!string.IsNullOrEmpty(obj_def.attached_script))
		{
			string text = "interaction";
			if ((!obj_def.IsNPC() || components.character.anim_state != CharAnimState.Walking) && custom_interaction_events.Count > 0)
			{
				text = custom_interaction_events[0];
				custom_interaction_events.RemoveAt(0);
				flag = true;
			}
			Debug.Log("Fire interaction event = '" + text + "' on wgo = '" + ((other_obj == null) ? "null" : base.name) + "'", this);
			if (obj_def.IsNPC())
			{
				AnimatorExitAnyCustomLoop();
			}
			FireEvent(text);
			RedrawBubble();
			if (flag)
			{
				return;
			}
		}
		switch (obj_def.interaction_type)
		{
		case ObjectDefinition.InteractionType.Craft:
		{
			if (!interaction_start)
			{
				break;
			}
			ObjectInteractionDefinition validInteraction3 = obj_def.GetValidInteraction(this);
			if (validInteraction3 == null)
			{
				break;
			}
			string text4 = ReplaceStringParams(validInteraction3.script);
			if (!string.IsNullOrEmpty(text4))
			{
				Debug.Log("<color=yellow>Running interaction script</color> \"" + text4 + "\" on WGO: '" + base.name + "' by: '" + ((other_obj == null) ? "null" : other_obj.name) + "'", this);
				AttachFlowScript(text4, other_obj, delegate
				{
					MainGame.me.player.components.interaction.UpdateNearestHint();
				});
				RedrawBubble();
				return;
			}
			break;
		}
		case ObjectDefinition.InteractionType.RunScript:
			if (interaction_start)
			{
				ObjectInteractionDefinition validInteraction2 = obj_def.GetValidInteraction(this);
				if (validInteraction2 != null)
				{
					string text3 = ReplaceStringParams(validInteraction2.script);
					if (!string.IsNullOrEmpty(text3))
					{
						Debug.Log("<color=yellow>Running interaction script</color> \"" + text3 + "\" on WGO: '" + base.name + "' by: '" + ((other_obj == null) ? "null" : other_obj.name) + "'", this);
						AttachFlowScript(text3, other_obj, delegate
						{
							MainGame.me.player.components.interaction.UpdateNearestHint();
						});
					}
				}
			}
			RedrawBubble();
			return;
		case ObjectDefinition.InteractionType.Builder:
			if (obj_id == "mf_wood_builddesk" && MainGame.me.player.GetParamInt("tut_build_wndw_not_shown") == 1)
			{
				MainGame.me.player.SetParam("tut_build_wndw_not_shown", 0f);
				GUIElements.me.tutorial.Open("tut_build", delegate
				{
				});
				if (custom_interaction_events != null && custom_interaction_events.Contains("first_use"))
				{
					custom_interaction_events.Remove("first_use");
				}
			}
			else
			{
				MainGame.me.OpenBuildObjectGUI(this);
				RedrawBubble();
			}
			return;
		case ObjectDefinition.InteractionType.Chest:
		{
			ObjectInteractionDefinition validInteraction = obj_def.GetValidInteraction(this);
			if (validInteraction != null)
			{
				string text2 = ReplaceStringParams(validInteraction.script);
				if (!string.IsNullOrEmpty(text2))
				{
					Debug.Log("<color=yellow>Running interaction script</color> \"" + text2 + "\" on WGO: '" + base.name + "' by: '" + ((other_obj == null) ? "null" : other_obj.name) + "'", this);
					AttachFlowScript(text2, other_obj, delegate
					{
						MainGame.me.player.components.interaction.UpdateNearestHint();
					});
				}
			}
			else
			{
				GUIElements.me.chest.Open(this);
			}
			RedrawBubble();
			return;
		}
		case ObjectDefinition.InteractionType.Grave:
			GUIElements.me.grave.Open(this);
			RedrawBubble();
			return;
		}
		components.Interact(other_obj, interaction_start, delta_time);
	}

	public void AnimatorExitAnyCustomLoop()
	{
		if (!(components.animator == null))
		{
			components.animator.SetTrigger("any_custom_loop_out");
			GJTimer.AddTimer(0f, delegate
			{
				components.animator.ResetTrigger("any_custom_loop_out");
			});
		}
	}

	public virtual void PrepareForInteraction(BaseCharacterComponent for_whom)
	{
		if (!_prepared_for_interaction && for_whom.wgo.is_player && obj_def.type != ObjectDefinition.ObjType.Mob)
		{
			if (obj_def.IsRelationVisible())
			{
				GUIElements.me.relation.Open(this);
			}
			components.PrepareForInteraction(for_whom);
			_prepared_for_interaction = true;
			RedrawBubble(true);
		}
	}

	public virtual void UnprepareForInteraction()
	{
		if (_prepared_for_interaction)
		{
			if (obj_def.IsNPC() || !string.IsNullOrEmpty(obj_def.npc_alias))
			{
				GUIElements.me.relation.Hide();
			}
			components.UnprepareForInteraction();
			_prepared_for_interaction = false;
			RedrawBubble(false);
		}
	}

	public void OnWorkAction()
	{
		try
		{
			if (!base.gameObject.activeInHierarchy)
			{
				base.gameObject.SetActive(value: true);
			}
		}
		catch (Exception message)
		{
			Debug.LogError(message);
			return;
		}
		Animator animator = ((GetWOP() == null) ? null : wop.GetComponent<Animator>());
		if (animator == null)
		{
			animator = GetComponentInChildren<Animator>(includeInactive: true);
			if (animator == null)
			{
				return;
			}
		}
		if (animator.HasParam("work_action"))
		{
			animator.SetTrigger("work_action");
		}
		if (components.craft.enabled && animator.HasParam("mf_state") && components.craft.is_crafting && !is_removing)
		{
			animator.SetInteger("mf_state", 2);
		}
	}

	public void OnWorkFinished()
	{
		if (components.craft.enabled)
		{
			UpdateCraftStateAnim();
		}
	}

	public void OnCraftStateChanged()
	{
		if (Application.isPlaying)
		{
			UpdateCraftStateAnim();
			RedrawBubble();
		}
	}

	private void UpdateCraftStateAnim()
	{
		if (!components.craft.enabled)
		{
			return;
		}
		Animator animator = ((GetWOP() == null) ? null : wop.GetComponent<Animator>());
		if (animator == null)
		{
			animator = GetComponentInChildren<Animator>(includeInactive: true);
			if (animator == null)
			{
				return;
			}
		}
		if (animator.HasParam("mf_state"))
		{
			animator.SetInteger("mf_state", components.craft.is_crafting ? 1 : 0);
		}
	}

	public void EquipItem(Item item, int toolbar_index = -1, Item try_from_bag = null)
	{
		if (item == null || item.IsEmpty() || item.durability_state == Item.DurabilityState.Broken)
		{
			return;
		}
		if (toolbar_index != -1)
		{
			MainGame.me.save.SetToolbarEquipped(item.id, toolbar_index);
			return;
		}
		ItemDefinition.EquipmentType equipment_type = item.definition.equipment_type;
		if (equipment_type == ItemDefinition.EquipmentType.None)
		{
			return;
		}
		foreach (Item item2 in _data.inventory)
		{
			if (!item2.IsEmpty() && item2.equipped_as == equipment_type)
			{
				item2.equipped_as = ItemDefinition.EquipmentType.None;
			}
		}
		Item itemFromToolbelt = GetItemFromToolbelt(equipment_type);
		if (itemFromToolbelt != null)
		{
			UnequipItemFromToolbelt(itemFromToolbelt);
		}
		EquipItemToToolbelt(item, try_from_bag);
	}

	public Item GetItemFromToolbelt(ItemDefinition.EquipmentType equipment_type)
	{
		foreach (Item item in _data.secondary_inventory)
		{
			if (item != null && item.definition?.equipment_type == equipment_type)
			{
				return item;
			}
		}
		return null;
	}

	private void UnequipItemFromToolbelt(Item item)
	{
		if (!_data.secondary_inventory.Contains(item))
		{
			Debug.LogError("Error: Can't unequip item from the toolbelt: not equipped, id = " + item.id);
			return;
		}
		_data.secondary_inventory.Remove(item);
		_data.inventory.Add(item);
	}

	private bool EquipItemToToolbelt(Item item, Item try_from_bag = null)
	{
		if (_data.secondary_inventory.Contains(item))
		{
			Debug.LogError("Error: Can't equip item to the toolbelt: already equipped, id = " + item.id);
			return false;
		}
		_data.secondary_inventory.Add(item);
		_data.RemoveItem(item, 1, try_from_bag);
		return true;
	}

	public void TryEquipPickupedDrop(Item item, bool check_last_item = true)
	{
		Item item2 = data.inventory.LastElement();
		if (check_last_item)
		{
			if (item2 == null || !(item.id == item2.id))
			{
				return;
			}
			item = item2;
		}
		else
		{
			foreach (Item item3 in _data.inventory)
			{
				if (item3.id == item.id)
				{
					item = item3;
					break;
				}
			}
		}
		ItemDefinition.EquipmentType equipment_type = item.definition.equipment_type;
		if (equipment_type != 0 && GetEquippedItem(equipment_type) == null)
		{
			EquipItem(item);
		}
	}

	public void UnEquipItem(Item item)
	{
		if (item == null || item.IsEmpty())
		{
			return;
		}
		item.equipped_as = ItemDefinition.EquipmentType.None;
		if (is_player)
		{
			if (MainGame.me.player.data.secondary_inventory.Contains(item))
			{
				UnequipItemFromToolbelt(item);
				DropItemIfDoesntFitInventory(item);
			}
			else if (item.equipped_as != 0 && item.is_equipped_to_toolbar)
			{
				MainGame.me.save.UnEquip(item.id);
			}
		}
	}

	private void DropItemIfDoesntFitInventory(Item item)
	{
		if (data.inventory.Count > data.inventory_size)
		{
			DropItem(item);
			data.inventory.Remove(item);
		}
	}

	public bool SetCurrentItem(ItemDefinition.ItemType new_item)
	{
		Debug.Log("set " + new_item, this);
		_current_item_type = new_item;
		return true;
	}

	public ItemDefinition.ItemType GetCurrentItemType()
	{
		return _current_item_type;
	}

	public ItemDefinition.ItemType GetEquippedWeaponType()
	{
		if (!is_player)
		{
			return ItemDefinition.ItemType.None;
		}
		return GetEquippedWeapon()?.definition.type ?? ItemDefinition.ItemType.None;
	}

	public Item GetEquippedWeapon()
	{
		return GetEquippedTool(ItemDefinition.ItemType.Sword);
	}

	public Item GetEquippedTool()
	{
		if (components.interaction.enabled)
		{
			return components.interaction.GetWorkToolTypeForNearest();
		}
		return null;
	}

	public ItemDefinition.ItemType GetEquippedToolType()
	{
		return GetEquippedTool()?.definition.type ?? ItemDefinition.ItemType.None;
	}

	public Item GetEquippedTool(ItemDefinition.ItemType item_type)
	{
		if (item_type == ItemDefinition.ItemType.Hand)
		{
			return new Item("hand_tool", 1);
		}
		foreach (Item item in _data.secondary_inventory)
		{
			if (item != null && item.definition?.type == item_type)
			{
				return item;
			}
		}
		foreach (Item item2 in _data.inventory)
		{
			if (item2 != null && item2.equipped_as != 0 && item2.definition.type == item_type)
			{
				return item2;
			}
		}
		return null;
	}

	public Item GetEquippedItem(ItemDefinition.EquipmentType eq_type)
	{
		foreach (Item item in _data.secondary_inventory)
		{
			if (item != null && item.definition?.equipment_type == eq_type)
			{
				return item;
			}
		}
		foreach (Item item2 in _data.inventory)
		{
			if (item2 != null && item2.equipped_as == eq_type)
			{
				return item2;
			}
		}
		return null;
	}

	public float GetDamage(ObjectDefinition.DamageType damage_type)
	{
		int num = (int)damage_type;
		string param_name = "damage" + ((num == 0) ? "" : ("_" + num));
		if (is_player)
		{
			Item equippedWeapon = GetEquippedWeapon();
			if (equippedWeapon != null)
			{
				return equippedWeapon.GetCalculatedParam(param_name) + GetParam("add_damage");
			}
			Debug.LogError("Weapon is nul!!!");
			return 10f;
		}
		return _data.GetParam(param_name);
	}

	public bool ActionCanBeDone(ItemDefinition.ItemType item_type)
	{
		return obj_def.tool_actions.HasToolK(item_type);
	}

	public void GetAllComponentsAndSort()
	{
	}

	public void DoPreZeroHPActivity()
	{
		Debug.Log("Do pre zero HP zctivity on " + base.name, this);
		if (is_player || obj_def.type == ObjectDefinition.ObjType.Mob)
		{
			BaseCharacterComponent character = components.character;
			if (character.enabled && character.attack.enabled && character.attack.performing_attack)
			{
				character.InterruptAttack();
			}
			if (is_player && character.movement_state != 0)
			{
				character.StopMovement();
			}
		}
		_already_dropped_drop = false;
		if (!is_player && !is_dead && obj_def.type == ObjectDefinition.ObjType.Default)
		{
			DoZeroHPActivity();
			return;
		}
		bool flag = true;
		if (components.animator != null)
		{
			AnimatorControllerParameter[] parameters = components.animator.parameters;
			foreach (AnimatorControllerParameter animatorControllerParameter in parameters)
			{
				if (animatorControllerParameter.type == AnimatorControllerParameterType.Trigger && animatorControllerParameter.name == "do_dying")
				{
					flag = false;
				}
			}
		}
		if (flag)
		{
			DoZeroHPActivity();
			return;
		}
		components.animator.SetTrigger("do_dying");
		components.character.SetAnimationState(CharAnimState.Dying);
		is_dead = true;
		components.character.body.bodyType = RigidbodyType2D.Static;
		if (obj_def.type == ObjectDefinition.ObjType.Mob)
		{
			BehaviourTreeOwner component = wop.GetComponent<BehaviourTreeOwner>();
			if (component != null)
			{
				component.enabled = false;
			}
		}
		if (obj_def.do_drop_after_dying_anim_finished)
		{
			return;
		}
		List<Item> list = ResModificator.ProcessItemsListBeforeDrop(obj_def.drop_items, this, MainGame.me.player);
		if (obj_def.drop_inventory_after_remove && string.IsNullOrEmpty(obj_def.after_hp_0.GetValue(this, MainGame.me.player)))
		{
			foreach (Item item in data.inventory)
			{
				if (item != null && !string.IsNullOrEmpty(item.id) && item.value >= 1)
				{
					list.Add(item);
				}
			}
		}
		if (!string.IsNullOrEmpty(obj_def.drop_sound))
		{
			Sounds.PlaySound(obj_def.drop_sound);
		}
		DropItems(list);
		_already_dropped_drop = true;
	}

	public void DoZeroHPActivity()
	{
		Debug.Log("do zero activity", this);
		Stats.DesignEvent("HP:ZeroHP:" + obj_id);
		if (is_player)
		{
			MainGame.me.OnPlayerDied();
			return;
		}
		UnlinkWithSpawnerIfExists();
		RewardForWork();
		ObjectDefinition def = obj_def;
		SetParam(def.set_param_after_hp_0);
		AddToParams(def.add_param_after_hp_0);
		GameRes gameRes = new GameRes(def.add_player_param_after_hp_0);
		if (def.add_player_param_after_hp_0_k.has_expression)
		{
			float num = def.add_player_param_after_hp_0_k.EvaluateFloat(this);
			foreach (GameResAtom item in gameRes.ToAtomList())
			{
				gameRes.Set(item.type, item.value * num);
			}
		}
		MainGame.me.player.AddToParams(gameRes);
		if (gameRes.Get("hp") < 0f)
		{
			EffectBubblesManager.ShowStackedHP(MainGame.me.player, gameRes.Get("hp"));
		}
		ResetDocks(shouldnt_be_used: true);
		MainGame.me.save.quests.CheckKeyQuests("zerohp_" + obj_id);
		string after_hp_0 = def.after_hp_0.GetValue(this, MainGame.me.player);
		if (!string.IsNullOrEmpty(after_hp_0))
		{
			string craft_name = null;
			if (!string.IsNullOrEmpty(def.craft_after_hp_0))
			{
				craft_name = def.craft_after_hp_0;
			}
			bool need_save_var = def.save_variation;
			int t_varioation = variation;
			GJCommons.VoidDelegate voidDelegate = delegate
			{
				if (!_already_dropped_drop)
				{
					List<Item> list2 = ResModificator.ProcessItemsListBeforeDrop(def.drop_items, this, MainGame.me.player);
					if (def.drop_inventory_after_remove)
					{
						foreach (Item item2 in data.inventory)
						{
							if (item2 != null && !string.IsNullOrEmpty(item2.id) && item2.value >= 1)
							{
								list2.Add(item2);
							}
						}
						data.inventory.Clear();
					}
					if (!string.IsNullOrEmpty(def.drop_sound))
					{
						Sounds.PlaySound(def.drop_sound);
					}
					DropItems(list2);
					_already_dropped_drop = true;
				}
				ReplaceWithObject(after_hp_0);
				SetParam(def.set_param_after_hp_0_end);
				ForceRedrawInSmartDrawer();
				variation = (need_save_var ? t_varioation : 0);
				RedrawVariation();
				UpdatePathCell();
				playing_disappearing_anim = false;
				TryStartCraft(craft_name);
			};
			DisappearAnimation componentInChildren = GetComponentInChildren<DisappearAnimation>();
			if (componentInChildren == null)
			{
				voidDelegate();
			}
			else
			{
				componentInChildren.StartAnimation(voidDelegate);
				playing_disappearing_anim = true;
			}
			if (!string.IsNullOrEmpty(def.script_after_hp_0))
			{
				if (def.script_after_hp_0.StartsWith("g:"))
				{
					GS.RunFlowScript(ReplaceStringParams(def.script_after_hp_0.Substring(2)));
				}
				else
				{
					AttachFlowScript(ReplaceStringParams(def.script_after_hp_0));
				}
			}
			return;
		}
		if (!_already_dropped_drop)
		{
			List<Item> list = ResModificator.ProcessItemsListBeforeDrop(def.drop_items, this, MainGame.me.player);
			if (def.drop_inventory_after_remove)
			{
				foreach (Item item3 in data.inventory)
				{
					if (item3 != null && !string.IsNullOrEmpty(item3.id) && item3.value >= 1)
					{
						list.Add(item3);
					}
				}
			}
			DropItems(list);
			if (!string.IsNullOrEmpty(def.drop_sound))
			{
				Sounds.PlaySound(def.drop_sound);
			}
			_already_dropped_drop = true;
		}
		if (!string.IsNullOrEmpty(def.script_after_hp_0))
		{
			if (def.script_after_hp_0.StartsWith("g:"))
			{
				GS.RunFlowScript(ReplaceStringParams(def.script_after_hp_0.Substring(2)));
			}
			else
			{
				AttachFlowScript(ReplaceStringParams(def.script_after_hp_0));
			}
		}
		SetParam(def.set_param_after_hp_0_end);
		CameraTools.RemoveFromCameraTargets(tf);
		DestroyMe();
		UpdatePathCell();
	}

	public void ProcessMultiQualityOutput_OLD(List<Item> output_items)
	{
		bool flag = obj_id.Contains("garden_") && obj_id.Contains("_ready");
		foreach (Item output_item in output_items)
		{
			if (!output_item.is_multiquality)
			{
			}
		}
	}

	public void RewardForWork()
	{
		if (!is_player && !string.IsNullOrEmpty(obj_def.work))
		{
			WorkDefinition workDefinition = GameBalance.me.GetData<WorkDefinition>(obj_def.work);
			if (workDefinition != null)
			{
				MainGame.me.player.AddToParams(workDefinition.reward);
			}
		}
	}

	public void UpdatePathCell()
	{
		GetComponent<ChunkedGameObject>().RescanAStar();
	}

	public void ReplaceWithObject(string new_obj_id, bool show_puff = false, int obj_variation_index = -1)
	{
		if (!is_removed && !(base.gameObject == null))
		{
			Debug.Log($"ReplaceWithObject id = {obj_id}, new_id = {new_obj_id}, show_puff = {show_puff}");
			if (Application.isPlaying && MainGame.game_started && show_puff)
			{
				DrawPuffFX();
			}
			DynamicLights.SearchForLightsInDestroyedObject(base.gameObject);
			OnBeganObjectModifications();
			SetParam("hp_inited", 0f);
			SetObject(ReplaceStringParams(new_obj_id));
			if (obj_variation_index != -1)
			{
				SetVariationByIndex(obj_variation_index);
			}
			DynamicLights.SearchForLightsInNewObject(base.gameObject);
			ForceRedrawInSmartDrawer();
			bubble.ClearData();
			_tried_to_find_removal_craft = false;
			GetComponent<ChunkedGameObject>().Init(init_after_change_wgo: true);
		}
	}

	public string ReplaceStringParams(string s)
	{
		while (s.Contains("{"))
		{
			Match match = Regex.Match(s, "(.*){([^}]+)}(.*)");
			if (match.Success)
			{
				s = match.Groups[1].Captures[0]?.ToString() + _data.GetParamAsString(match.Groups[2].Captures[0].ToString()) + match.Groups[3].Captures[0];
				continue;
			}
			Debug.LogError("Syntax error in obj_id: " + s);
			return s;
		}
		return s;
	}

	public void DropItemAndFly(Item item, Vector2 dest_point)
	{
		DropResGameObject.DropAndFly(GetDropPos(), item, tf.parent, dest_point);
	}

	public void DropItem(Item item, Direction direction = Direction.None, Vector3 pos = default(Vector3), float force = 1f, bool check_walls = true)
	{
		Vector3 vector = (pos.magnitude.EqualsTo(0f) ? GetDropPos() : pos);
		if (direction == Direction.ToPlayer)
		{
			Vector2 vec = MainGame.me.player_pos - vector;
			force = 1f;
			direction = vec.ToDirection();
			vector = (Vector2)MainGame.me.player_pos - direction.ToVec() * 80f;
		}
		DropResGameObject.Drop(vector, item, tf.parent, direction, force, -1, check_walls);
	}

	private Vector3 GetDropPos()
	{
		if (obj_def.drop_point == ObjectDefinition.DropPoint.Center || _dock_points == null || _dock_points.Length == 0)
		{
			return tf.position;
		}
		DockPoint dockPoint = null;
		bool flag = false;
		DockPoint[] dock_points = _dock_points;
		foreach (DockPoint dockPoint2 in dock_points)
		{
			flag = dockPoint2 == null || dockPoint2.tf == null;
			if (flag)
			{
				break;
			}
		}
		if (flag)
		{
			InitDockPoints();
		}
		dockPoint = _dock_points[0];
		if (_dock_points.Length > 1)
		{
			Vector3 position = MainGame.me.player.tf.position;
			float num = dockPoint.tf.position.DistSqrTo(position);
			for (int j = 1; j < _dock_points.Length; j++)
			{
				float num2 = _dock_points[j].tf.position.DistSqrTo(position);
				if (num2 < num)
				{
					num = num2;
					dockPoint = _dock_points[j];
				}
			}
		}
		if (!(dockPoint == null))
		{
			return dockPoint.GetDropPos();
		}
		return Vector3.zero;
	}

	public void DropItems(List<Item> items, Direction direction = Direction.None)
	{
		if (items == null || items.Count == 0)
		{
			return;
		}
		Vector3 dropPos = GetDropPos();
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		foreach (Item item in items)
		{
			if (item.is_tech_point)
			{
				switch (item.id)
				{
				case "r":
					num += item.value;
					break;
				case "g":
					num2 += item.value;
					break;
				case "b":
					num3 += item.value;
					break;
				}
			}
		}
		if (num + num2 + num3 > 0)
		{
			TechPointsDrop.Drop(dropPos, num, num2, num3);
		}
		foreach (Item item2 in items)
		{
			if (!item2.is_tech_point)
			{
				DropItem(item2, direction, dropPos);
			}
		}
	}

	private void InitNewObject(bool at_obj_start)
	{
		if (!is_player)
		{
			_data.SetItemID(obj_id);
		}
		if (obj_id == "")
		{
			return;
		}
		ResetDocks(shouldnt_be_used: false);
		obj_def = GameBalance.me.GetData<ObjectDefinition>(obj_id);
		if (obj_def == null)
		{
			Debug.LogError("null object definition for [" + obj_id + "]");
			return;
		}
		AuraEmitter aura_emitter = components.aura_emitter;
		if (aura_emitter.enabled)
		{
			aura_emitter.Clear();
		}
		AuraReceiver aura_receiver = components.aura_receiver;
		if (aura_receiver.enabled)
		{
			aura_receiver.Clear();
		}
		foreach (ObjectGroupDefinition object_group in obj_def.object_groups)
		{
			ApplyObjectGroup(object_group);
		}
		if (!at_obj_start)
		{
			components.UpdateComponentsSet();
		}
		_data.SetInventorySize(obj_def.inventory_size);
		if (_skin_changer != null)
		{
			_skin_changer.OnWGOChanged();
		}
	}

	private void ResetDocks(bool shouldnt_be_used)
	{
		DockPoint[] array = RefindDockPoints();
		foreach (DockPoint dockPoint in array)
		{
			if (dockPoint == null)
			{
				Debug.LogError("Dock point is null at: " + base.name, this);
			}
			else
			{
				dockPoint.Reset(shouldnt_be_used);
			}
		}
	}

	public void ApplyObjectGroup(ObjectGroupDefinition grp)
	{
	}

	public void SetBuildingColor(Color c)
	{
		SpriteRenderer[] componentsInChildren = GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
		foreach (SpriteRenderer spriteRenderer in componentsInChildren)
		{
			if ((!(spriteRenderer.color != Color.white) || !(spriteRenderer.color != Color.red)) && !(spriteRenderer.gameObject.GetComponent<DynamicSprite>() != null))
			{
				spriteRenderer.color = c;
			}
		}
	}

	public void MoveWhenPlacingGlobalPos(Vector2 pos)
	{
		Vector3 position = pos;
		position.z = tf.position.z;
		tf.position = position;
		round_and_sort.DoRound();
	}

	public void MoveWhenPlacingLocalPos(Vector2 pos)
	{
		Vector3 localPosition = pos;
		localPosition.z = tf.localPosition.z;
		tf.localPosition = localPosition;
		round_and_sort.DoRound();
	}

	public void SetObject(string id)
	{
		if (id == "0")
		{
			DestroyMe();
			return;
		}
		obj_id = id;
		RoundAndSortComponent roundAndSortComponent = null;
		try
		{
			roundAndSortComponent = GetComponentInChildren<RoundAndSortComponent>();
		}
		catch (MissingReferenceException ex)
		{
			Debug.LogError("MissingReferenceException: obj_id = " + obj_id + ": " + ex.ToString(), this);
			return;
		}
		ChunkManager.OnDestroyObject(roundAndSortComponent.GetComponent<ChunkedGameObject>());
		InitNewObject(at_obj_start: false);
		if (obj_def == null)
		{
			return;
		}
		Redraw();
		if ((bool)roundAndSortComponent)
		{
			roundAndSortComponent.OnChangedSprite();
		}
		DockPoint[] array = RefindDockPoints();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].StartDocks(this);
		}
		if (base.name.Contains("wgo") && !string.IsNullOrEmpty(id))
		{
			base.name = id;
		}
		ChunkManager.OnAddNewObject(GetComponentInChildren<ChunkedGameObject>());
		if (Application.isPlaying && !string.IsNullOrEmpty(id) && !(MainGame.me.player == null))
		{
			if (this == MainGame.me.player.components.interaction.nearest)
			{
				MainGame.me.player.components.interaction.UpdateNearestHint();
			}
			components.PreStartComponents();
			WorldMap.OnAddNewWGO(this);
			CheckNeededAttachedScript();
		}
	}

	public void OnQualityChanged(float diff)
	{
		if (diff.EqualsTo(0f))
		{
			return;
		}
		WorldZone myWorldZone = GetMyWorldZone();
		if (!(myWorldZone == null) && !string.IsNullOrEmpty(myWorldZone.definition.quality_icon))
		{
			string arg = ((!obj_def.has_overrode_quality_icon) ? myWorldZone.definition.quality_icon : obj_def.overrode_quality_icon);
			if (!just_built)
			{
				EffectBubblesManager.ShowImmediately(bubble_pos, ((diff < 0f) ? "-" : "+") + string.Format("{1}{0:0.0}", Mathf.Abs(diff), arg), (!(diff < 0f)) ? EffectBubblesManager.BubbleColor.Green : EffectBubblesManager.BubbleColor.Red, ignore_timescale: true, 3f);
			}
		}
	}

	private DockPoint[] RefindDockPoints()
	{
		if (wop == null)
		{
			return new DockPoint[0];
		}
		return _dock_points = wop.GetComponentsInChildren<DockPoint>(includeInactive: true);
	}

	public void ResetObject()
	{
		if (is_player)
		{
			Debug.LogError("cant reset player");
		}
		else if (string.IsNullOrEmpty(obj_id))
		{
			Debug.LogError("null or empty obj_id", this);
		}
		else
		{
			SetObject(obj_id);
		}
	}

	[ContextMenu("Redraw")]
	public void EditorRedraw()
	{
		Redraw(force_redraw: true);
	}

	public void DrawPuffFX(Bounds? bounds = null)
	{
		PuffFX.Create(this, bounds);
	}

	public virtual void Redraw(bool force_redraw = false, bool force_redraw_part = false, bool draw_puff = false)
	{
		try
		{
			if (base.gameObject == null)
			{
				Debug.LogError("Error: Trying to redraw a null WGO, obj_id = " + obj_id);
				return;
			}
		}
		catch (Exception arg)
		{
			Debug.LogError($"Error: Exception in redraw a WGO, obj_id = {obj_id}: {arg}");
			return;
		}
		Bounds totalBounds = GetTotalBounds();
		if (obj_id == "")
		{
			InitDockPoints();
		}
		else
		{
			if (obj_def == null)
			{
				return;
			}
			GetWOP();
			string path_prefix = "objects/";
			ObjectDefinition.ObjType type = obj_def.type;
			if ((uint)(type - 1) <= 1u)
			{
				path_prefix = "mobs/";
			}
			if (!Application.isPlaying || _obj_id != obj_id || force_redraw_part)
			{
				RedrawPart(ref wop, obj_id, path_prefix, 0f);
				if (Application.isPlaying)
				{
					if (_obj_id != "_not_set_" && !string.IsNullOrEmpty(_obj_id))
					{
						draw_puff = true;
					}
					_obj_id = obj_id;
				}
			}
			InteractionBubbleGUI interactionBubbleGUI = InteractionBubbleGUI.GetBubble(unique_id);
			if (interactionBubbleGUI != null)
			{
				interactionBubbleGUI.LinkTransform(bubble_pos_tf);
			}
			if (wop != null)
			{
				Blackboard component = GetComponent<Blackboard>();
				BehaviourTreeOwner component2 = wop.GetComponent<BehaviourTreeOwner>();
				if (component != null && component2 != null)
				{
					component2.blackboard = component;
				}
			}
			playing_disappearing_anim = false;
			custom_drawers.OnObjectRedraw(force_redraw);
			components.UpdateComponentsSet();
			if (components.craft.enabled)
			{
				components.craft.FillCraftsList();
			}
			InitDockPoints();
			_trnsps = base.gameObject.GetComponentsInChildren<CanGoTransparent>();
			UpdateTransparentParts();
			_ = is_removing;
			if (wop != null)
			{
				if (wop.variations != null)
				{
					if (!wop.variation_can_be_none && variation == 0 && wop.variations.Count > 1)
					{
						variation = 1;
					}
					for (int i = 0; i < wop.variations.Count; i++)
					{
						int num = 1 << i;
						GameObject gameObject = wop.variations[i];
						if (!(gameObject == null))
						{
							gameObject.SetActive((variation & num) > 0);
						}
					}
				}
				if (wop.variations_2 != null)
				{
					if (!wop.variation_2_can_be_none && variation_2 == 0 && wop.variations_2.Count > 1)
					{
						variation_2 = 1;
					}
					for (int j = 0; j < wop.variations_2.Count; j++)
					{
						int num2 = 1 << j;
						List<GameObject> list = wop.variations_2[j].list;
						if (list == null || list.Count == 0)
						{
							continue;
						}
						bool active = (variation_2 & num2) > 0;
						foreach (GameObject item in list)
						{
							item.SetActive(active);
						}
					}
				}
			}
			RedrawGroundSprites();
			if (Application.isPlaying && MainGame.game_started)
			{
				if (draw_puff)
				{
					DrawPuffFX(totalBounds);
				}
				BuffsLogics.CheckBuffsGiveConditions();
			}
		}
	}

	public bool CanBeRotatedWhilePlacing()
	{
		if (wop.variations_are_radiobutton && wop.variations != null)
		{
			return wop.variations.Count != 0;
		}
		return false;
	}

	public void NextVariationRadiobutton()
	{
		if (CanBeRotatedWhilePlacing())
		{
			if (variation == 0)
			{
				variation = 1;
				RedrawVariation();
			}
			else if ((double)variation >= Math.Pow(2.0, wop.variations.Count - 1))
			{
				variation = 1;
				RedrawVariation();
			}
			else
			{
				variation <<= 1;
				RedrawVariation();
			}
		}
	}

	public void SetVariationByIndex(int index)
	{
		variation = (int)Math.Pow(2.0, index);
		RedrawVariation();
	}

	public void PrevVariationRadiobutton()
	{
		if (CanBeRotatedWhilePlacing())
		{
			if (variation == 0)
			{
				variation = 1;
				RedrawVariation();
			}
			else if (variation == 1)
			{
				variation = 1 << wop.variations.Count - 1;
				RedrawVariation();
			}
			else
			{
				variation >>= 1;
				RedrawVariation();
			}
		}
	}

	private void RedrawVariation()
	{
		if (!(wop != null))
		{
			return;
		}
		if (wop.variations != null)
		{
			if (!wop.variation_can_be_none && variation == 0 && wop.variations.Count > 1)
			{
				variation = 1;
			}
			for (int i = 0; i < wop.variations.Count; i++)
			{
				int num = 1 << i;
				GameObject gameObject = wop.variations[i];
				if (!(gameObject == null))
				{
					gameObject.SetActive((variation & num) > 0);
				}
			}
		}
		if (wop.variations_2 == null)
		{
			return;
		}
		if (!wop.variation_2_can_be_none && variation_2 == 0 && wop.variations_2.Count > 1)
		{
			variation_2 = 1;
		}
		for (int j = 0; j < wop.variations_2.Count; j++)
		{
			int num2 = 1 << j;
			List<GameObject> list = wop.variations_2[j].list;
			if (list == null || list.Count == 0)
			{
				continue;
			}
			bool active = (variation_2 & num2) > 0;
			foreach (GameObject item in list)
			{
				item.SetActive(active);
			}
		}
	}

	public void Say(string text, GJCommons.VoidDelegate on_disappeared = null, bool? to_left = null, SpeechBubbleGUI.SpeechBubbleType type = SpeechBubbleGUI.SpeechBubbleType.Talk, SmartSpeechEngine.VoiceID force_voice = SmartSpeechEngine.VoiceID.None, bool say_as_player = false, Transform overrode_pos = null)
	{
		Debug.Log("Say \"" + text + "\" on wgo = " + base.name + " (" + obj_id + ")");
		SmartSpeechEngine.VoiceID voice = ((force_voice != 0) ? force_voice : ((is_player || say_as_player) ? SmartSpeechEngine.VoiceID.Player : obj_def.voice_id));
		SpeechBubbleGUI.ShowMessage(unique_id, text, (overrode_pos == null) ? bubble_pos_tf : overrode_pos, on_disappeared, ShowBubbleToLeft(to_left), use_world_cam: true, type, is_player || say_as_player, voice);
	}

	public void ShowMultianswer(List<AnswerVisualData> answers, MultiAnswerGUI.MultiAnswerResult on_chosen, bool? to_left = null, GJCommons.VoidDelegate on_disappeared = null, WorldGameObject talker = null)
	{
		bool control_was_enabled = is_player && components.character.control_enabled;
		if (is_player)
		{
			components.character.control_enabled = false;
		}
		Debug.Log("ShowMultianswer, talker = " + ((talker == null) ? "null" : talker.name) + ", answers = " + answers.Count);
		MultiAnswerGUI.ShowAnswers(answers, bubble_pos_tf, delegate(string chosen)
		{
			on_chosen(chosen);
			if (is_player && control_was_enabled)
			{
				components.character.control_enabled = true;
			}
		}, components.character.enabled && components.character.ShowBubbleToLeft(to_left), on_disappeared, talker);
	}

	private bool ShowBubbleToLeft(bool? to_left)
	{
		if (components.character.enabled)
		{
			return components.character.ShowBubbleToLeft(to_left);
		}
		return to_left ?? (MainGame.me.player.tf.position.x > tf.position.x);
	}

	public void RedrawPart(ref WorldObjectPart o, string object_id, string path_prefix, float z)
	{
		if (o != null)
		{
			ChunkedGameObject componentInChildren = o.gameObject.GetComponentInChildren<ChunkedGameObject>();
			if (componentInChildren != null)
			{
				ChunkManager.OnDestroyObject(componentInChildren);
			}
			GJCommons.Destroy(o.gameObject);
		}
		WorldObjectPart worldObjectPart = ((object_id == "" || object_id == "0") ? null : SmartResourceHelper.GetResource<WorldObjectPart>(path_prefix + object_id));
		if (worldObjectPart != null)
		{
			o = SmartInstantiate(worldObjectPart, z);
			if (obj_def.IsCharacter())
			{
				o.gameObject.layer = 9;
			}
		}
		if (GetComponent<FloatingWorldGameObject>() != null)
		{
			Collider2D[] componentsInChildren = GetComponentsInChildren<Collider2D>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
		}
	}

	protected WorldObjectPart SmartInstantiate(WorldObjectPart prefab, float z)
	{
		RefindContentParent();
		WorldObjectPart worldObjectPart = UnityEngine.Object.Instantiate(prefab, content_tf);
		worldObjectPart.transform.localPosition = new Vector3(prefab.transform.localPosition.x, prefab.transform.localPosition.y, z);
		worldObjectPart.CacheSectors();
		return worldObjectPart;
	}

	public static void ResetAllObjects()
	{
		WorldGameObject[] array = UnityEngine.Object.FindObjectsOfType<WorldGameObject>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].ResetObject();
		}
	}

	public static void ResetMobs()
	{
		WorldGameObject[] array = UnityEngine.Object.FindObjectsOfType<WorldGameObject>();
		foreach (WorldGameObject worldGameObject in array)
		{
			if (worldGameObject.obj_def.type == ObjectDefinition.ObjType.Mob)
			{
				worldGameObject.ResetObject();
			}
		}
	}

	public CustomFlowScript AttachFlowScript(string flowscript_name, WorldGameObject interractor = null, CustomFlowScript.OnFinishedDelegate on_finished = null)
	{
		if (string.IsNullOrEmpty(flowscript_name))
		{
			on_finished?.Invoke(flowscript_name);
			return null;
		}
		if (flowscript_name[0] == ':')
		{
			SmartExpression.ParseExpression(flowscript_name.Substring(1)).Evaluate(this);
			on_finished?.Invoke(flowscript_name);
			return null;
		}
		CustomFlowScript customFlowScript = CustomFlowScript.Create(base.gameObject, flowscript_name, is_global: false, on_finished);
		if (customFlowScript == null)
		{
			return null;
		}
		customFlowScript.current_interractor = interractor;
		return customFlowScript;
	}

	public Item GetItemOfType(ItemDefinition.ItemType item_type)
	{
		foreach (Item item in _data.inventory)
		{
			if (item != null && item.definition != null && !item.definition.is_placeholder && item.definition.type == item_type)
			{
				return item;
			}
		}
		return null;
	}

	public Item GetItemById(string item_id, bool check_bags = true)
	{
		foreach (Item item in _data.inventory)
		{
			if (item == null || item.IsEmpty())
			{
				continue;
			}
			if (item.id == item_id)
			{
				return item;
			}
			if (!item.is_bag)
			{
				continue;
			}
			foreach (Item item2 in item.inventory)
			{
				if (item2 != null && !item2.IsEmpty() && !(item2.id != item_id))
				{
					return item2;
				}
			}
		}
		return null;
	}

	public void ClearTiredness()
	{
		SetParam("tiredness", 0f);
	}

	public GameRes UseItemFromInventory(Item item, Vector3? effect_bubble_pos = null, Item use_from_bag = null)
	{
		if (item == null)
		{
			Debug.LogError("Use item is null");
			return new GameRes();
		}
		if (!item.definition.can_be_used)
		{
			Debug.LogError("This item can't be used: " + item);
			return new GameRes();
		}
		if (!item.definition.stay_on_use && !data.RemoveItem(item.id, 1, use_from_bag))
		{
			Debug.LogError("Trying to use absent item", this);
			return new GameRes();
		}
		return item.UseItem(this, effect_bubble_pos);
	}

	public bool CanInsertItem(Item item)
	{
		if (item == null)
		{
			return false;
		}
		if (is_removing)
		{
			return false;
		}
		Item itemOfType = GetItemOfType(item.definition.type);
		if (obj_def.can_insert_items.Count > 0)
		{
			if (!obj_def.can_insert_items.Contains(item.id))
			{
				if (obj_def.custom_insertions.Count == 0)
				{
					return false;
				}
				CustomItemInsertion customItemInsertion = null;
				foreach (CustomItemInsertion custom_insertion in obj_def.custom_insertions)
				{
					if (custom_insertion.item_id == item.id)
					{
						customItemInsertion = custom_insertion;
						break;
					}
				}
				if (customItemInsertion == null)
				{
					return false;
				}
				List<Item> list = new List<Item>();
				if (customItemInsertion.insertion_type == CustomItemInsertion.InsertionType.OnUse)
				{
					foreach (Item item2 in item.definition.drop_on_use)
					{
						list.Add(item2);
					}
					foreach (Item item3 in list)
					{
						if (!obj_def.can_insert_items.Contains(item3.id))
						{
							Debug.Log($"Can not insert item \"{item3}\" through custom insertion of item \"{item.id}\" to wgo=\"{obj_id}\"");
							return false;
						}
					}
					if (obj_def.can_insert_items_limit == 0)
					{
						return true;
					}
					int num = 0;
					foreach (Item item4 in data.inventory)
					{
						if (obj_def.can_insert_items.Contains(item4.id))
						{
							num += item4.value;
						}
					}
					int num2 = 0;
					foreach (Item item5 in list)
					{
						num2 += item5.value;
					}
					return num2 + num <= obj_def.can_insert_items_limit;
				}
				throw new ArgumentOutOfRangeException();
			}
			if (obj_def.can_insert_items_limit == 0)
			{
				return true;
			}
			int num3 = 0;
			foreach (Item item6 in data.inventory)
			{
				if (obj_def.can_insert_items.Contains(item6.id))
				{
					num3 += item6.value;
				}
			}
			return num3 < obj_def.can_insert_items_limit;
		}
		switch (obj_def.id)
		{
		case "autopsi_table":
			if (item.definition.type == ItemDefinition.ItemType.Body)
			{
				return itemOfType == null;
			}
			if (item.definition.type == ItemDefinition.ItemType.ZombieWorker)
			{
				return itemOfType == null;
			}
			break;
		case "grave_empty":
			if (item.definition.type == ItemDefinition.ItemType.Body)
			{
				return itemOfType == null;
			}
			break;
		case "grave_ground":
		{
			ItemDefinition.ItemType type = item.definition.type;
			if ((uint)(type - 101) <= 2u)
			{
				return itemOfType == null;
			}
			break;
		}
		case "working_table":
			return item.id == "wood";
		case "mf_timber_1":
			if (item.id != "wood")
			{
				return false;
			}
			if (data.GetTotalCount("wood") >= data.inventory_size)
			{
				return false;
			}
			return true;
		case "mf_stones_1":
			if (item.id != "stone")
			{
				return false;
			}
			if (data.GetTotalCount("stone") >= data.inventory_size)
			{
				return false;
			}
			return true;
		case "mf_ore_1_complete":
			if (item.id != "ore_metal")
			{
				return false;
			}
			if (data.GetTotalCount("ore_metal") >= data.inventory_size)
			{
				return false;
			}
			return true;
		case "witch_pylon":
			return item.id == "wood";
		case "zombie_crafting_table":
			if (item.is_worker)
			{
				return false;
			}
			if (item.definition.type == ItemDefinition.ItemType.Body)
			{
				return itemOfType == null;
			}
			return false;
		case "mf_pyre":
		case "mf_crematorium":
			if (item.is_worker)
			{
				return false;
			}
			if (item.definition.type == ItemDefinition.ItemType.Body && itemOfType == null)
			{
				return !item.is_worker;
			}
			return false;
		case "bar_barmens_place":
			if (item.id != "bartender_doll")
			{
				return false;
			}
			if (data.GetTotalCount("bartender_doll") > 0)
			{
				return false;
			}
			return true;
		case "tavern_outside_zombie_fence":
			if (item.id != "working_zombie_pseudoitem_1")
			{
				return false;
			}
			if (data.GetTotalCount("working_zombie_pseudoitem_1") > 0)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public void DestroyMe()
	{
		if (is_removed)
		{
			return;
		}
		try
		{
			Debug.Log("WGO:DestroyMe " + ((base.gameObject == null) ? "NULL" : base.gameObject.name), this);
		}
		catch (MissingReferenceException)
		{
		}
		if (obj_def != null && obj_def.drop_inventory_after_remove && data != null && data.inventory != null && data.inventory.Count > 0)
		{
			foreach (Item item in data.inventory)
			{
				if (item != null && !string.IsNullOrEmpty(item.id) && item.value >= 1)
				{
					DropItem(item);
				}
			}
		}
		components.craft.enabled = false;
		components.timer.enabled = false;
		components.hp.enabled = false;
		ChunkManager.OnDestroyObject(this);
		if (_bubble != null)
		{
			InteractionBubbleGUI.RemoveBubble(unique_id);
			_bubble = null;
		}
		UnlinkWithSpawnerIfExists();
		is_removed = true;
		UnityEngine.Object.Destroy(base.gameObject);
		if (!_was_ever_active)
		{
			OnDestroy();
		}
		if (_zone != null)
		{
			_zone.Recalculate();
		}
	}

	private void UnlinkWithSpawnerIfExists()
	{
		BaseCharacterComponent character = components.character;
		if (character != null && character.spawner != null && character.spawner.spawned_mobs != null)
		{
			if (character.spawner.spawned_mobs.Contains(this))
			{
				character.spawner.spawned_mobs.Remove(this);
			}
			character.spawner = null;
		}
	}

	public bool IsInRange(WorldGameObject other_wgo, float range)
	{
		Vector2 to = other_wgo.pos;
		return pos.GridDistTo(to) < range;
	}

	public bool IsInRange(GameObject other_go, float range)
	{
		return pos.GridDistTo(other_go.transform.position) < range;
	}

	public void RestoreFromSerializedObject(SerializableWGO o, bool change_hierarchy = true)
	{
		if (string.IsNullOrEmpty(o.obj_id))
		{
			Debug.LogError("Can't deserialize WGO with an empty id.", this);
			return;
		}
		if (change_hierarchy)
		{
			base.transform.SetParent(MainGame.me.world_root, worldPositionStays: false);
		}
		o.ToWGO(this, out _data);
		SetObject(o.obj_id);
		NetworkIdentity component = base.gameObject.GetComponent<NetworkIdentity>();
		if (component != null)
		{
			UnityEngine.Object.Destroy(component);
		}
		o.ToWGOAfterSetID(this);
		GetComponent<ChunkedGameObject>().OnJustSpawnedWGO();
		round_and_sort.DoUpdateStuff();
		base.name = "[wgo] " + o.obj_id;
		if (Application.isPlaying)
		{
			SaveGameFixer.UnstuckWGO(this);
		}
	}

	public void MarkForRemoval()
	{
		if (is_removing)
		{
			is_removing = false;
			if (components.craft.current_craft == BuildModeLogics.GetObjectRemoveCraftDefinition(obj_id))
			{
				components.craft.CancelRemovalCraft();
				RedrawBubble();
			}
		}
		else
		{
			ObjectCraftDefinition objectRemoveCraftDefinition = BuildModeLogics.GetObjectRemoveCraftDefinition(obj_id);
			if (objectRemoveCraftDefinition == null)
			{
				Debug.Log("Object id = " + obj_id + " has no removal craft");
				return;
			}
			if (objectRemoveCraftDefinition.IsLocked())
			{
				return;
			}
			is_removing = true;
			if (obj_id.EndsWith("_place", StringComparison.InvariantCulture))
			{
				ObjectCraftDefinition objectPutCraftDefinition = BuildModeLogics.GetObjectPutCraftDefinition(obj_id);
				if (objectPutCraftDefinition != null)
				{
					if (!string.IsNullOrEmpty(objectRemoveCraftDefinition.end_script))
					{
						GS.RunFlowScript(objectRemoveCraftDefinition.end_script);
					}
					if (objectPutCraftDefinition.one_time_craft && MainGame.me.save.completed_one_time_crafts.Contains(objectPutCraftDefinition.id))
					{
						MainGame.me.save.completed_one_time_crafts.Remove(objectPutCraftDefinition.id);
					}
					DropItems(objectPutCraftDefinition.needs);
					DestroyMe();
					return;
				}
				Debug.LogError("Error: Couldn't find obj craft = " + obj_id);
			}
			else if (objectRemoveCraftDefinition.is_remove_without_hp_work)
			{
				if (!string.IsNullOrEmpty(objectRemoveCraftDefinition.end_script))
				{
					GS.RunFlowScript(objectRemoveCraftDefinition.end_script);
				}
				DropItems(objectRemoveCraftDefinition.output);
				if (objectRemoveCraftDefinition.is_destroy_worker_on_remove && linked_worker != null)
				{
					linked_worker.DestroyMe();
				}
				DestroyMe();
				return;
			}
			components.craft.StartRemovalCraft(objectRemoveCraftDefinition);
			RedrawBubble();
			InteractionBubbleGUI.GetBubble(unique_id).Activate();
		}
		Redraw();
	}

	public void TryStartCraft(string craft_name)
	{
		if (string.IsNullOrEmpty(craft_name))
		{
			return;
		}
		GJTimer.AddTimer(0.1f, delegate
		{
			CraftDefinition dataOrNull = GameBalance.me.GetDataOrNull<CraftDefinition>(craft_name);
			if (dataOrNull != null)
			{
				if (components == null)
				{
					Debug.LogError("Components is null!");
				}
				else if (!components.craft.Craft(dataOrNull))
				{
					Debug.LogError("Failed to start craft!");
				}
				else
				{
					Debug.Log("Started craft: " + dataOrNull.id);
				}
			}
			else
			{
				Debug.LogError("Craft definition [" + craft_name + "] is null!");
			}
		});
	}

	public void OnEnable()
	{
		if (Application.isPlaying)
		{
			CustomUpdateManager.wgos.Add(this);
		}
		if (!string.IsNullOrEmpty(cur_gd_point))
		{
			GDPoint gDPointByGDTag = WorldMap.GetGDPointByGDTag(cur_gd_point);
			if (gDPointByGDTag != null && !string.IsNullOrEmpty(gDPointByGDTag.smart_anim_trigger))
			{
				TriggerSmartAnimation(gDPointByGDTag.smart_anim_trigger);
			}
		}
	}

	public void ResetAnimator()
	{
		components.animator.SetTrigger("reset");
	}

	public void PreDisable()
	{
		ObjectDefinition objectDefinition = obj_def;
		if (objectDefinition == null || objectDefinition.type != ObjectDefinition.ObjType.Mob)
		{
			return;
		}
		GraphOwner[] componentsInChildren = GetComponentsInChildren<GraphOwner>();
		if (componentsInChildren != null && componentsInChildren.Length != 0)
		{
			GraphOwner[] array = componentsInChildren;
			for (int i = 0; i < array.Length; i++)
			{
				array[i]?.StopBehaviour();
			}
		}
	}

	public void OnDisable()
	{
		if (Application.isPlaying)
		{
			CustomUpdateManager.wgos.Remove(this);
		}
	}

	public void FireEvent(string event_id, float delay = 0f)
	{
		if (event_id == "porter_on_came_to_destination")
		{
			linked_workbench.porter_station.OnCameToDestination();
		}
		else if (event_id == "porter_on_came_to_source")
		{
			linked_workbench.porter_station.OnCameToSource();
		}
		if (is_removed)
		{
			return;
		}
		if (!base.gameObject.activeInHierarchy && delay.EqualsTo(0f))
		{
			delay = 0.1f;
		}
		if (delay.EqualsTo(0f))
		{
			if (_fsc != null)
			{
				_fsc.SendEvent(event_id);
			}
		}
		else
		{
			_events.event_delays.Add(delay);
			_events.event_ids.Add(event_id);
			GetComponent<ChunkedGameObject>().active_now_because_of_events = true;
		}
	}

	public void FireEvent(string event_id, float delay, string param)
	{
		if (event_id == "porter_on_came_to_destination")
		{
			linked_workbench.porter_station.OnCameToDestination();
		}
		else if (event_id == "porter_on_came_to_source")
		{
			linked_workbench.porter_station.OnCameToSource();
		}
		if (is_removed)
		{
			return;
		}
		if (!base.gameObject.activeInHierarchy && delay.EqualsTo(0f))
		{
			delay = 0.1f;
		}
		if (delay.EqualsTo(0f))
		{
			if (_fsc != null)
			{
				_fsc.SendEvent(event_id, param);
			}
		}
		else
		{
			_events.event_delays.Add(delay);
			_events.event_ids.Add(event_id);
			GetComponent<ChunkedGameObject>().active_now_because_of_events = true;
		}
	}

	public bool ContainsSerializedEvent(string event_id)
	{
		foreach (string event_id2 in _events.event_ids)
		{
			if (event_id2 == event_id)
			{
				return true;
			}
		}
		return false;
	}

	public static void InitAllWorldWGOs()
	{
		WorldGameObject[] componentsInChildren = MainGame.me.world.GetComponentsInChildren<WorldGameObject>(includeInactive: true);
		Debug.Log("InitAllWorldWGOs, count = " + componentsInChildren.Length);
		WorldGameObject[] array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].OnJustSpawned();
		}
	}

	public void OnJustSpawned()
	{
		if (obj_def == null)
		{
			obj_def = GameBalance.me.GetData<ObjectDefinition>(obj_id);
			if (obj_def == null)
			{
				Debug.LogError("obj_def is null for id = " + obj_id);
				return;
			}
		}
		components.InitAllComponents();
		CheckNeededAttachedScript();
		RecalculateZoneBelonging();
		_cached_pos = base.transform.position;
	}

	public void CheckNeededAttachedScript()
	{
		Blackboard blackboard = null;
		BehaviourTreeOwner[] componentsInChildren = GetComponentsInChildren<BehaviourTreeOwner>(includeInactive: true);
		foreach (BehaviourTreeOwner behaviourTreeOwner in componentsInChildren)
		{
			if (behaviourTreeOwner.blackboard == null)
			{
				if (blackboard == null)
				{
					blackboard = base.gameObject.AddComponent<Blackboard>();
				}
				behaviourTreeOwner.blackboard = blackboard;
			}
		}
		if (string.IsNullOrEmpty(obj_def.attached_script))
		{
			return;
		}
		FlowGraph graph = CustomFlowScript.GetGraph("WGO Scripts/" + obj_def.attached_script);
		if (!(graph != null))
		{
			return;
		}
		bool flag = false;
		if (_fsc != null)
		{
			_fsc.PauseBehaviour();
			flag = true;
		}
		else
		{
			_fsc = base.gameObject.AddComponent<FlowScriptController>();
			if (blackboard == null)
			{
				blackboard = base.gameObject.AddComponent<Blackboard>();
			}
			_fsc.blackboard = blackboard;
			_fsc.disableAction = GraphOwner.DisableAction.DoNothing;
		}
		_fsc.graph = graph;
		_fsc.graph.blackboard = _fsc.blackboard;
		if (flag)
		{
			_fsc.StartBehaviour();
		}
	}

	public void RecalculateZoneBelonging()
	{
		if (obj_def.can_belong_to_zone)
		{
			_zone = WorldZone.GetZoneOfObject(this);
		}
	}

	public int CanCollectDrop(DropResGameObject drop)
	{
		if (drop.res.is_tech_point || data.CanCollectItemAsDrop(drop.res))
		{
			return drop.res.value;
		}
		ItemDefinition itemDefinition = drop.res?.definition;
		if (itemDefinition != null && itemDefinition.item_size == 1 && data.HasItemInInventory(itemDefinition.id))
		{
			return data.CanAddCount(itemDefinition.id, count_empty: true);
		}
		return 0;
	}

	public static WorldGameObject InstantiateWGOPrefab()
	{
		return SmartPooler.CreateObject<WorldGameObject>();
	}

	public WorldZone GetMyWorldZone()
	{
		if (is_removed)
		{
			Debug.LogError("ERROR: Trying to get world zone of a removed game object, id: " + obj_id);
			return null;
		}
		if (base.gameObject == null)
		{
			Debug.LogError("ERROR: Trying to get world zone of a null game object, id: " + obj_id);
			return null;
		}
		Collider2D[] array = Physics2D.OverlapPointAll(base.gameObject.transform.position, 524288);
		if (array == null || array.Length == 0)
		{
			return null;
		}
		Collider2D[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			WorldZone componentInParent = array2[i].gameObject.GetComponentInParent<WorldZone>();
			if (componentInParent != null)
			{
				return componentInParent;
			}
		}
		return null;
	}

	public bool HasSoulsTotemInZone()
	{
		WorldZone myWorldZone = GetMyWorldZone();
		if (myWorldZone == null)
		{
			return false;
		}
		return myWorldZone.HasSoulsTotemInZone();
	}

	public string GetMyWorldZoneId()
	{
		WorldZone myWorldZone = GetMyWorldZone();
		if (!(myWorldZone == null))
		{
			return myWorldZone.id;
		}
		return "";
	}

	[ContextMenu("Apply Current Skin")]
	public void ApplyCurrentSkin()
	{
		if (!(wop == null))
		{
			if (_skin_changer == null)
			{
				_skin_changer = new SkinChanger(this);
			}
			SkinPreset skin = (string.IsNullOrEmpty(wop.skin_id) ? null : SkinPreset.Load(wop.skin_id));
			_skin_changer.ApplySkin(skin);
		}
	}

	public void ApplySkin(string skin_id)
	{
		if (!(GetWOP() == null))
		{
			wop.skin_id = skin_id;
			ApplyCurrentSkin();
		}
	}

	public Bounds GetTotalBounds()
	{
		Bounds? bounds = null;
		Collider2D[] componentsInChildren = GetComponentsInChildren<Collider2D>(includeInactive: true);
		foreach (Collider2D collider2D in componentsInChildren)
		{
			if (!bounds.HasValue)
			{
				bounds = collider2D.bounds;
			}
			else
			{
				bounds.Value.Encapsulate(collider2D.bounds);
			}
		}
		return bounds.GetValueOrDefault();
	}

	public void AnimationEventAction()
	{
		if (components.tool.enabled)
		{
			components.tool.AnimationEventAction();
		}
	}

	public void FailAnimationEventAction()
	{
		if (components.tool.enabled)
		{
			components.tool.FailAnimationEventAction();
		}
	}

	public bool IsEnough(SmartRes res)
	{
		if (res == null)
		{
			Debug.LogError("SmartRes is null in IsEnough()");
			return true;
		}
		switch (res.res_type)
		{
		case SmartRes.ResType.Empty:
			return true;
		case SmartRes.ResType.Item:
			return data.IsEnoughItems(res.item);
		case SmartRes.ResType.GameRes:
			return data.IsEnoughParam(res.res);
		default:
			Debug.LogError("IsEnough() is not supported for res type: " + res.res_type, this);
			return false;
		}
	}

	public bool IsEnough(GameRes res)
	{
		return data.IsEnoughParams(res);
	}

	public void RemoveSmartRes(SmartRes res)
	{
		if (res != null)
		{
			switch (res.res_type)
			{
			case SmartRes.ResType.Empty:
				break;
			case SmartRes.ResType.Item:
				data.RemoveItem(res.item);
				break;
			case SmartRes.ResType.GameRes:
			{
				GameResAtom res2 = res.res;
				data.SetParam(res2.type, data.GetParam(res2.type) - res2.value);
				break;
			}
			default:
				Debug.LogError("RemoveSmartRes() is not supported for res type: " + res.res_type, this);
				break;
			}
		}
	}

	public void ReceiveSmartRes(SmartRes res, WorldGameObject giver = null)
	{
		switch (res.res_type)
		{
		case SmartRes.ResType.Item:
			((giver == null) ? MainGame.me.player : giver).DropItem(res.item);
			break;
		case SmartRes.ResType.GameRes:
			data.AddToParams(res.res);
			if (res.res.type == "money")
			{
				DropCollectGUI.OnMoneyCollected(res.res.value);
			}
			if (res.res.type.Contains("_rel") && giver != null)
			{
				giver.ShowRelationChangeBubble((int)res.res.value);
			}
			break;
		default:
			Debug.LogError("RemoveSmartRes() is not supported for res type: " + res.res_type, this);
			throw new ArgumentOutOfRangeException();
		}
	}

	public UniversalObjectInfo GetUniversalObjectInfo()
	{
		UniversalObjectInfo universalObjectInfo = new UniversalObjectInfo();
		universalObjectInfo.header = GJL.L(obj_def.id);
		universalObjectInfo.descr = "id: " + obj_def.id;
		if (is_autopsy_table)
		{
			if (GetBodyFromInventory() == null)
			{
				universalObjectInfo.header = GJL.L("hdr_autopsy_empty");
				universalObjectInfo.descr = "";
			}
			else
			{
				universalObjectInfo.header = GJL.L("hdr_autopsy_body");
				universalObjectInfo.descr = "";
			}
			universalObjectInfo.icon = (string.IsNullOrEmpty(obj_def.custom_icon) ? ("i_b_" + obj_def.id) : obj_def.custom_icon);
			return universalObjectInfo;
		}
		switch (obj_def.interaction_type)
		{
		case ObjectDefinition.InteractionType.RunScript:
		case ObjectDefinition.InteractionType.Builder:
		{
			WorldZone zoneByID = WorldZone.GetZoneByID(obj_def.zone_id, null_is_error: false);
			if (zoneByID == null || string.IsNullOrEmpty(zoneByID.definition.gui_descr_str))
			{
				universalObjectInfo.descr = "";
			}
			else
			{
				universalObjectInfo.descr = GJL.L(zoneByID.definition.gui_descr_str, zoneByID.GetQualityString());
			}
			universalObjectInfo.icon = (string.IsNullOrEmpty(obj_def.custom_icon) ? ("i_z_" + obj_def.id) : obj_def.custom_icon);
			break;
		}
		case ObjectDefinition.InteractionType.Craft:
		{
			universalObjectInfo.descr = GJL.L("select_item_craft");
			universalObjectInfo.icon = (string.IsNullOrEmpty(obj_def.custom_icon) ? ("i_b_" + obj_def.id) : obj_def.custom_icon);
			ObjectCraftDefinition objectPutCraftDefinition = BuildModeLogics.GetObjectPutCraftDefinition(obj_def.id);
			if (objectPutCraftDefinition != null)
			{
				universalObjectInfo.icon = objectPutCraftDefinition.icon;
			}
			if (obj_def.additional_header_items.Count <= 0)
			{
				break;
			}
			foreach (string additional_header_item in obj_def.additional_header_items)
			{
				universalObjectInfo.right_items.Add(additional_header_item, data.GetItemsCount(additional_header_item));
			}
			break;
		}
		}
		if (HasSoulsTotemInZone() && GlobalCraftControlGUI.is_global_control_active && !GlobalCraftControlGUI.current_instance.is_shown && !universalObjectInfo.right_items.ContainsKey("gratitude_as_item"))
		{
			universalObjectInfo.right_items.Add("gratitude_as_item", (int)MainGame.me.player.gratitude_points);
		}
		switch (obj_def.id)
		{
		case "grave_ground":
		{
			Item bodyFromInventory = GetBodyFromInventory();
			universalObjectInfo.icon = "i_b_grave_place";
			if (bodyFromInventory == null)
			{
				universalObjectInfo.header = GJL.L("grave_empty_hdr");
				universalObjectInfo.descr = "";
			}
			else
			{
				universalObjectInfo.header = GJL.L("grave_body_hdr");
				universalObjectInfo.descr = "";
			}
			break;
		}
		case "mf_balsamation_1":
		case "mf_balsamation_2":
			universalObjectInfo.icon = "i_b_" + obj_def.id;
			universalObjectInfo.descr = "";
			break;
		}
		return universalObjectInfo;
	}

	public Item GetBodyFromInventory(bool first = true)
	{
		return data.GetItemOfType(ItemDefinition.ItemType.Body, first);
	}

	public bool CanSeeDarkness()
	{
		return true;
	}

	public string GetObjectConditionString(string separator = "")
	{
		return "(hp)" + separator + Item.FloatNumberToPercentString(1f - GetDecayFactor());
	}

	public float GetDecayFactor()
	{
		float num = GetParam("decay") / 100f;
		if (num < 0f)
		{
			num = 0f;
		}
		if (num > 1f)
		{
			num = 1f;
		}
		return num;
	}

	public void ShowMark(WGOMark.MarkType mark_type)
	{
		if (_mark == null)
		{
			_mark = Prefabs.mark_prefab.Copy(base.transform);
			_mark.transform.localPosition = Vector3.zero;
		}
		if (_mark_type != 0)
		{
			RemoveMark();
		}
		_mark.gameObject.SetActive(value: true);
		_mark_type = mark_type;
		_mark.Draw(_mark_type);
	}

	public void MarkObjectAsCanBeRemoved(GameObject group)
	{
		if (_is_marked_removable)
		{
			return;
		}
		_is_marked_removable = true;
		_stored_parent_tf = base.transform.parent;
		base.transform.SetParent(group.transform);
		Redraw();
		SpriteRenderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
		foreach (SpriteRenderer spriteRenderer in componentsInChildren)
		{
			WGOSpriteInCanBeRemovedMode wGOSpriteInCanBeRemovedMode = spriteRenderer.GetComponent<WGOSpriteInCanBeRemovedMode>();
			if (wGOSpriteInCanBeRemovedMode == null)
			{
				wGOSpriteInCanBeRemovedMode = spriteRenderer.gameObject.AddComponent<WGOSpriteInCanBeRemovedMode>();
			}
			wGOSpriteInCanBeRemovedMode.sorting_order = spriteRenderer.sortingOrder;
			spriteRenderer.sortingOrder = -Mathf.RoundToInt(spriteRenderer.transform.position.z);
		}
	}

	public void CancelCanBeRemoved()
	{
		if (_is_marked_removable)
		{
			_is_marked_removable = false;
			base.transform.SetParent((_stored_parent_tf != null) ? _stored_parent_tf : MainGame.me.world_root);
			Redraw();
			WGOSpriteInCanBeRemovedMode[] componentsInChildren = base.gameObject.GetComponentsInChildren<WGOSpriteInCanBeRemovedMode>(includeInactive: true);
			foreach (WGOSpriteInCanBeRemovedMode wGOSpriteInCanBeRemovedMode in componentsInChildren)
			{
				wGOSpriteInCanBeRemovedMode.GetComponent<SpriteRenderer>().sortingOrder = wGOSpriteInCanBeRemovedMode.sorting_order;
				UnityEngine.Object.Destroy(wGOSpriteInCanBeRemovedMode);
			}
		}
	}

	public void SetSortOverEverything()
	{
		_is_sort_over_everything = true;
		_stored_parent_tf = base.transform.parent;
		SortingGroup sortingGroup = content_tf.gameObject.AddComponent<SortingGroup>();
		sortingGroup.sortingLayerName = "over everything";
		sortingGroup.enabled = true;
		Redraw();
	}

	public void CancelSortOverEverything()
	{
		if (_is_sort_over_everything)
		{
			_is_sort_over_everything = false;
			SortingGroup component = content_tf.GetComponent<SortingGroup>();
			component.enabled = false;
			UnityEngine.Object.Destroy(component);
			base.transform.SetParent((_stored_parent_tf != null) ? _stored_parent_tf : MainGame.me.world_root);
			Redraw();
		}
	}

	public void RemoveMark()
	{
		if (_mark_type != 0)
		{
			if (_mark != null)
			{
				_mark.gameObject.SetActive(value: false);
			}
			_mark_type = WGOMark.MarkType.None;
		}
	}

	public void OnBeganObjectModifications()
	{
		if (!_obj_modified_this_frame)
		{
			_obj_modified_this_frame = true;
			_obj_quality = quality;
		}
	}

	public void OnCameToGDPoint(GDPoint p)
	{
		if (!(p == null))
		{
			components.character.idle_animation = p.idle_animation;
			if (obj_def.IsNPC())
			{
				components.animator.ResetTrigger("any_custom_loop_out");
			}
			if (p.direction != 0)
			{
				components.character.LookAt(p.direction);
			}
			if (!string.IsNullOrEmpty(p.smart_anim_trigger))
			{
				TriggerSmartAnimation(p.smart_anim_trigger);
			}
			cur_gd_point = p.gd_tag;
		}
	}

	public void WGOLog(string s)
	{
	}

	public bool CheckDisabledInteractions()
	{
		bool flag = GetParamInt("disabled_interactions") != 0;
		if (obj_def != null && obj_def.IsNPC() && components.character.astar != null && components.character.astar.finding)
		{
			flag = true;
		}
		if (flag)
		{
			MainGame.me.player.components.character.ShowDisabledInteractionBubble(this);
		}
		return flag;
	}

	public BubbleWidgetTextData GetQualityWidgetData()
	{
		if (obj_def == null)
		{
			Debug.LogError("GetQualityWidgetData: obj_def is null", this);
			return null;
		}
		try
		{
			if (obj_def.quality_type == ObjectDefinition.QualityType.Hidden || _zone == null || !show_quality_hint)
			{
				return null;
			}
		}
		catch (NullReferenceException message)
		{
			Debug.LogError(message, this);
			return null;
		}
		bool is_shown = MainGame.me.gui_elements.build_mode_gui.is_shown;
		float num = quality;
		bool num2 = is_shown & !quality_k.EqualsTo(0f);
		if (num2)
		{
			num /= quality_k;
		}
		string text = ((!obj_def.has_overrode_quality_icon) ? _zone.definition.quality_icon : obj_def.overrode_quality_icon);
		text += num.ToString("0.#");
		if (num2 && !quality_k.EqualsTo(1f))
		{
			text = text + "\n" + ("×" + quality_k.ToString("0.#")).ColorizeText(InteractionBubbleGUI.quality_k_color);
		}
		if (num2 && !totem_effect.IsEmpty())
		{
			string text2 = totem_effect.ToPrintableString(use_colors: false, default(Color), default(Color), force_parentheses: true, float_format: true, new List<string> { "quality_k" });
			text2 = text2.Replace("(quality)", _zone.definition.quality_icon);
			if (!string.IsNullOrEmpty(text2))
			{
				text = text + "\n+" + text2;
			}
		}
		if (text.Contains("(wskull)"))
		{
			text = text.Replace("(wskull)", "(wskull)\n");
			if (num < 0f)
			{
				text += " ";
			}
		}
		return new BubbleWidgetTextData(text, UITextStyles.TextStyle.QualityHint)
		{
			widget_id = BubbleWidgetData.WidgetID.Quality
		};
	}

	public BubbleWidgetTextData SetBubbleWidgetData(string text, BubbleWidgetData.WidgetID widget_id)
	{
		if (string.IsNullOrEmpty(text))
		{
			SetBubbleWidgetData((BubbleWidgetData)null, widget_id);
			return null;
		}
		BubbleWidgetTextData bubbleWidgetTextData = new BubbleWidgetTextData(text, UITextStyles.TextStyle.InteractionHint);
		SetBubbleWidgetData(bubbleWidgetTextData, widget_id);
		return bubbleWidgetTextData;
	}

	public void SetBubbleWidgetData(BubbleWidgetData wdata, BubbleWidgetData.WidgetID widget_id)
	{
		bubble.SetWidgetDataWithID(wdata, widget_id);
		bubble.Redraw();
	}

	public void DrawFishingPullBubble(string hint)
	{
		bubble.ClearData();
		if (!string.IsNullOrEmpty(hint))
		{
			SetBubbleWidgetData(GameKeyTip.Get(GameKey.Interaction, GJL.L(hint)), BubbleWidgetData.WidgetID.Interaction);
		}
		bubble.Redraw();
		if (!string.IsNullOrEmpty(hint))
		{
			bubble.GetBubbleGUI().MakeBottomAligned();
		}
	}

	public void RedrawBubble(bool? show_interaction_buttons = null)
	{
		components.RefreshBubblesData(show_interaction_buttons);
		if (GUIElements.me.IsAnyMassiveWindowOpened())
		{
			bubble.SetWidgetDataWithID(null, BubbleWidgetData.WidgetID.Interaction);
		}
		bubble.Redraw();
		InteractionBubbleGUI bubbleGUI = bubble.GetBubbleGUI();
		if (bubbleGUI != null)
		{
			bubbleGUI.RefreshAlign(this);
		}
	}

	public void SetQualityHint(bool show_quality)
	{
		show_quality_hint = show_quality;
		RedrawBubble();
	}

	public bool CanProcessWork()
	{
		if (components.craft.enabled && !components.craft.is_crafting && components.craft.IsCraftQueueEmpty())
		{
			return false;
		}
		return true;
	}

	public void RecalculateGridShape()
	{
		if (is_removed)
		{
			return;
		}
		OptimizedCollider2D[] componentsInChildren = GetComponentsInChildren<OptimizedCollider2D>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Init();
		}
		Bounds totalBounds = GetTotalBounds();
		totalBounds.Expand(Vector2.one * 32f);
		List<Collider2D> list = new List<Collider2D>();
		Collider2D[] componentsInChildren2 = GetComponentsInChildren<Collider2D>(includeInactive: true);
		foreach (Collider2D collider2D in componentsInChildren2)
		{
			if (!BuildGrid.SkipCollider(collider2D, this))
			{
				list.Add(collider2D);
			}
		}
		_cells.Clear();
		_cells_totem_local.Clear();
		for (int j = Mathf.FloorToInt(totalBounds.min.x / 32f); j <= Mathf.CeilToInt(totalBounds.max.x / 32f); j++)
		{
			for (int k = Mathf.FloorToInt(totalBounds.min.y / 32f); k <= Mathf.CeilToInt(totalBounds.max.y / 32f); k++)
			{
				if (BuildGrid.IsCellBusy(new Vector2(j, k) * 32f, list))
				{
					_cells.Add(new IntVector2(j, k));
				}
			}
		}
		if (!obj_def.IsTotem())
		{
			return;
		}
		float num = obj_def.totem_radius * obj_def.totem_radius;
		Vector2 totemCenterInLocalCoords = GetTotemCenterInLocalCoords();
		for (int l = -Mathf.CeilToInt(obj_def.totem_radius + 1f); (float)l < obj_def.totem_radius + 1f; l++)
		{
			for (int m = -Mathf.CeilToInt(obj_def.totem_radius + 1f); (float)m < obj_def.totem_radius + 1f; m++)
			{
				if ((new Vector2(l, m) - totemCenterInLocalCoords).sqrMagnitude <= num)
				{
					_cells_totem_local.Add(new IntVector2(l, m));
				}
			}
		}
	}

	public Vector2 GetTotemCenterInLocalCoords()
	{
		return Vector2.zero;
	}

	public bool DoesIncludeGridPos(int x, int y)
	{
		foreach (IntVector2 cell in _cells)
		{
			if (cell.x == x && cell.y == y)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasTotemInfluenceOnWGO(WorldGameObject o)
	{
		if (!obj_def.IsTotem())
		{
			Debug.LogError("HasTotemInfluenceOnWGO must be called only on a totem");
			return false;
		}
		int num = Mathf.RoundToInt(pos.x / 32f);
		int num2 = Mathf.RoundToInt(pos.y / 32f);
		foreach (IntVector2 item in _cells_totem_local)
		{
			if (o.DoesIncludeGridPos(item.x + num, item.y + num2))
			{
				return true;
			}
		}
		return false;
	}

	public void ProcessRemove()
	{
		InteractionBubbleGUI.RemoveBubble(unique_id, immediate: true);
		DestroyMe();
	}

	public override string ToString()
	{
		return $"[WGO name={base.gameObject.name} obj_id={obj_id} instance_id={base.gameObject.GetInstanceID()}]";
	}

	public void ForceInitOptimizedColliders()
	{
		OptimizedCollider2D[] componentsInChildren = base.gameObject.GetComponentsInChildren<OptimizedCollider2D>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Init();
		}
	}

	public void AdditionalDeserialize(ref SerializableWGO d)
	{
		_events = ((d.events_as_class == null) ? new SerializableEvents() : d.events_as_class);
	}

	public void AdditionalSerialize(ref SerializableWGO d)
	{
		d.events_as_class = _events;
	}

	public void AddInteractionEvent(string event_id)
	{
		if (custom_interaction_events == null)
		{
			custom_interaction_events = new List<string>();
		}
		if (custom_interaction_events.Contains(event_id))
		{
			Debug.LogWarning("AddInteractionEvent: Trying to add existing event " + event_id + " to WGO " + base.name, this);
		}
		else
		{
			custom_interaction_events.Add(event_id);
		}
		RedrawBubble();
	}

	public Sprite GetHeadSprite()
	{
		Sprite result = null;
		ObjectDefinition objectDefinition = (string.IsNullOrEmpty(obj_def.npc_alias) ? obj_def : GameBalance.me.GetData<ObjectDefinition>(obj_def.npc_alias));
		if (!string.IsNullOrEmpty(objectDefinition.custom_head_spr))
		{
			return EasySpritesCollection.GetSprite(objectDefinition.custom_head_spr);
		}
		if (obj_def.IsCharacter())
		{
			SkinPreset skinPreset = (string.IsNullOrEmpty(wop.skin_id) ? null : SkinPreset.Load(wop.skin_id));
			if (skinPreset != null)
			{
				result = EasySpritesCollection.GetSprite(skinPreset.head.ToString("0##") + "_hed_down");
			}
		}
		return result;
	}

	public static int GetRelation(string obj_id)
	{
		string text = obj_id;
		ObjectDefinition dataOrNull = GameBalance.me.GetDataOrNull<ObjectDefinition>(obj_id);
		if (dataOrNull != null && !string.IsNullOrEmpty(dataOrNull.npc_alias))
		{
			text = dataOrNull.npc_alias;
		}
		string param_name = "_rel_" + text;
		int num = MainGame.me.player.GetParamInt(param_name);
		if (num < 0)
		{
			num = 0;
			MainGame.me.player.SetParam(param_name, num);
		}
		if (num > 100)
		{
			num = 100;
			MainGame.me.player.SetParam(param_name, num);
		}
		return num;
	}

	public GDPoint GetParentGDPoint()
	{
		if (!_parent_gd_point_inited)
		{
			_parent_gd_point_inited = true;
			GDPoint[] componentsInParent = base.gameObject.GetComponentsInParent<GDPoint>(includeInactive: true);
			_parent_gd_point = ((componentsInParent.Length != 0) ? componentsInParent[0] : null);
		}
		return _parent_gd_point;
	}

	public bool IsDisabled()
	{
		GDPoint parentGDPoint = GetParentGDPoint();
		if (parentGDPoint != null && !parentGDPoint.gameObject.activeSelf)
		{
			return true;
		}
		return false;
	}

	public void TeleportToGDPoint(string gd_point_tag, bool dont_move_camera_while_tp = false)
	{
		GDPoint gDPointByGDTag = WorldMap.GetGDPointByGDTag(gd_point_tag);
		if (gDPointByGDTag == null)
		{
			Debug.LogError("Can't find GD point: " + gd_point_tag);
			return;
		}
		Debug.Log("Teleporting " + base.name + " to GD point: " + gDPointByGDTag.name, gDPointByGDTag.gameObject);
		base.transform.position = gDPointByGDTag.transform.position;
		if (is_player)
		{
			if (!dont_move_camera_while_tp)
			{
				CameraTools.MoveToPos(gDPointByGDTag.transform.position);
			}
			GameAwakenerEngine.OnPlayerMoved();
		}
		else
		{
			OnCameToGDPoint(gDPointByGDTag);
		}
		RefreshPositionCache();
		GetComponent<ChunkedGameObject>().RecalculateChunk();
	}

	public void ShowRelationChangeBubble(int delta)
	{
		Debug.Log("ShowRelationChangeBubble " + delta + " of obj " + obj_id, this);
		EffectBubblesManager.ShowImmediately(bubble_pos, HUDRelationBubble.GetRelationChangeString(delta), EffectBubblesManager.BubbleColor.Relation, ignore_timescale: true, 1f);
		GUIElements.me.relation.OnShownRelationBubble(this);
	}

	public void TriggerSmartAnimation(string smart_anim_trigger)
	{
		if (!string.IsNullOrEmpty(smart_anim_trigger))
		{
			SmartAnimationController componentInChildren = GetComponentInChildren<SmartAnimationController>();
			if (componentInChildren == null)
			{
				Debug.LogError("Can't trigger animation " + smart_anim_trigger + " because SmartAnimationController is not found", this);
			}
			else
			{
				componentInChildren.TriggerAimation(smart_anim_trigger);
			}
		}
	}

	public void TriggerSmartAnimation(string smart_anim_trigger, Action on_anim_finished, float workaround_time = 1f)
	{
		if (GetComponent<StateAnimationListener>() != null)
		{
			Debug.LogError("WGO already has a StateAnimationListener, anim \"" + smart_anim_trigger + "\" for object \"" + base.name + "\"", this);
		}
		StateAnimationListener stateAnimationListener = base.gameObject.AddComponent<StateAnimationListener>();
		stateAnimationListener.AddWorkaroundTimer(workaround_time, "Forcing workaround anim \"" + smart_anim_trigger + "\" for object \"" + base.name + "\"");
		stateAnimationListener.on_exit = on_anim_finished;
		TriggerSmartAnimation(smart_anim_trigger);
	}

	public void DropStory(float bronze, float silver, float gold)
	{
		float num = bronze + silver + gold;
		if (!num.EqualsTo(0f))
		{
			float num2 = UnityEngine.Random.Range(0f, num);
			string item_id = "story:1";
			if (num2 < gold)
			{
				item_id = "story:3";
			}
			else if (num2 < gold + silver)
			{
				item_id = "story:2";
			}
			DropItem(new Item(item_id, 1), Direction.ToPlayer);
		}
	}

	public void GiveItemToPlayersHands(Item item)
	{
		data.RemoveItem(item);
		BaseCharacterComponent character = MainGame.me.player.components.character;
		if (character.has_overhead)
		{
			character.DropOverheadItem();
		}
		character.SetOverheadItem(item);
		Redraw();
		SmartDrawer componentInChildren = GetComponentInChildren<SmartDrawer>();
		if (componentInChildren != null)
		{
			componentInChildren.Redraw(force: true);
		}
		if (item.is_worker)
		{
			item.worker.UpdateWorkerInventoryFromItem(item);
		}
		Sounds.PlaySound("item_2h_drop");
	}

	public bool CheckIfDisabledInTutorial()
	{
		if (!obj_def.interactive_in_tutorial && MainGame.me.save.IsInTutorial())
		{
			if (!_shown_tutorial_disabled)
			{
				MainGame.me.player.Say("disabled_interactions", delegate
				{
					_shown_tutorial_disabled = false;
				}, null, SpeechBubbleGUI.SpeechBubbleType.Think);
				_shown_tutorial_disabled = true;
			}
			return true;
		}
		return false;
	}

	public bool IsMoving()
	{
		if (obj_def != null && obj_def.IsNPC())
		{
			return components.character.IsInMovingState();
		}
		return false;
	}

	public bool CanPutToAllPossibleInventories(List<Item> items_to_put, out List<Item> can_not_put)
	{
		if (items_to_put == null || items_to_put.Count == 0)
		{
			can_not_put = new List<Item>();
			return true;
		}
		can_not_put = new List<Item>();
		foreach (Item item3 in items_to_put)
		{
			can_not_put.Add(new Item(item3));
		}
		WorldZone myWorldZone = GetMyWorldZone();
		if (myWorldZone != null)
		{
			List<WorldGameObject> list = myWorldZone.GetZoneWGOs();
			if (obj_id == "tavern_kitchen" || obj_id == "tavern_oven")
			{
				WorldGameObject worldGameObjectByObjId = WorldMap.GetWorldGameObjectByObjId("npc_tavern_barman");
				if (worldGameObjectByObjId == null)
				{
					Debug.LogError("Can not put tavern_kitchen output to barmen: not found barmen WGO! Call Bulat.");
				}
				else
				{
					list = new List<WorldGameObject> { worldGameObjectByObjId };
				}
			}
			foreach (WorldGameObject item4 in list)
			{
				if (item4 == null)
				{
					continue;
				}
				Item item = item4.data.MakeInventoryCopy();
				ObjectDefinition objectDefinition = item4.obj_def;
				if (objectDefinition == null)
				{
					Debug.LogError("Not found object definition for WGO \"" + item4.name + "\", obj_def=" + item4.obj_id);
				}
				else
				{
					if (!objectDefinition.open_in_multiinventory)
					{
						continue;
					}
					bool flag = objectDefinition.can_insert_items != null && objectDefinition.can_insert_items.Count > 0;
					for (int i = 0; i < can_not_put.Count; i++)
					{
						Item item2 = can_not_put[i];
						if ((flag || item2.definition.is_big) && (objectDefinition.can_insert_items == null || !objectDefinition.can_insert_items.Contains(item2.id) || (objectDefinition.can_insert_items_limit != 0 && objectDefinition.can_insert_items_limit <= item.GetItemsCount(item2.id))))
						{
							continue;
						}
						int num = item.CanAddCount(item2.id, count_empty: true);
						if (num > 0)
						{
							int num2 = item2.value - num;
							item.AddItem(item2.id, Mathf.Min(item2.value, num));
							if (num2 > 0)
							{
								item2.value = num2;
								continue;
							}
							can_not_put.RemoveAt(i);
							i--;
						}
					}
				}
			}
		}
		if (can_not_put.Count == 0)
		{
			return true;
		}
		foreach (Item item5 in can_not_put)
		{
			if (!item5.IsEmpty() && item5.value != 0)
			{
				return false;
			}
		}
		return true;
	}

	public void PutToAllPossibleInventories(List<Item> drop_list, out List<Item> cant_insert)
	{
		WorldZone myWorldZone = GetMyWorldZone();
		if (obj_id == "tavern_kitchen" || obj_id == "tavern_oven")
		{
			WorldGameObject worldGameObjectByObjId = WorldMap.GetWorldGameObjectByObjId("npc_tavern_barman");
			if (worldGameObjectByObjId == null)
			{
				Debug.LogError("Can not put tavern_kitchen output to barmen: not found barmen WGO! Call Bulat. #1");
				cant_insert = drop_list;
				return;
			}
			int count = drop_list.Count;
			worldGameObjectByObjId.TryPutToInventory(drop_list, out cant_insert);
			if (count > cant_insert.Count)
			{
				SetParam("do_roll_anim", 1f);
			}
		}
		else if (myWorldZone != null)
		{
			myWorldZone.PutToAllPossibleInventoriesSmart(drop_list, out cant_insert);
		}
		else
		{
			cant_insert = drop_list;
		}
	}

	public void TryPutToInventory(List<Item> items_to_insert, out List<Item> cant_insert)
	{
		for (int i = 0; i < items_to_insert.Count; i++)
		{
			Item item = items_to_insert[i];
			if (item.IsEmpty())
			{
				items_to_insert.RemoveAt(i);
				i--;
			}
			else
			{
				if (!is_player && (!obj_def.can_insert_items.Contains(item.id) || (obj_def.can_insert_items_limit != 0 && obj_def.can_insert_items_limit <= item.value)))
				{
					continue;
				}
				int num = data.CanAddCount(item.id, count_empty: true);
				if (num > 0)
				{
					int num2 = item.value - num;
					if (num2 > 0)
					{
						Item item2 = new Item(item)
						{
							value = num
						};
						data.AddItem(item2);
						item.value = num2;
					}
					else
					{
						data.AddItem(item);
						items_to_insert.RemoveAt(i);
						i--;
					}
				}
			}
		}
		cant_insert = items_to_insert;
	}

	public DockPoint GetAvailableDockPointForZombie()
	{
		if (_dock_points == null || _dock_points.Length == 0)
		{
			RefindDockPoints();
		}
		if (_dock_points == null || _dock_points.Length == 0)
		{
			if (MainGame.game_started)
			{
				Debug.LogError("Not found any dock point at wgo \"" + base.name + "\", obj_id=" + obj_id);
			}
			return null;
		}
		List<DockPoint> list = new List<DockPoint>();
		List<DockPoint> list2 = new List<DockPoint>();
		DockPoint[] dock_points = _dock_points;
		foreach (DockPoint dockPoint in dock_points)
		{
			if (dockPoint == null || dockPoint.tf == null || dockPoint.gameObject == null || !dockPoint.gameObject.activeInHierarchy || !dockPoint.can_place_worker)
			{
				continue;
			}
			list2.Add(dockPoint);
			Collider2D[] array = Physics2D.OverlapCircleAll(dockPoint.tf.position, 19.2f, 1);
			if (array != null && array.Length != 0)
			{
				bool flag = false;
				Collider2D[] array2 = array;
				for (int j = 0; j < array2.Length; j++)
				{
					WorldGameObject componentInParent = array2[j].GetComponentInParent<WorldGameObject>();
					if ((!(componentInParent != null) || componentInParent.is_player) && (!(componentInParent != null) || !(componentInParent == this)))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					continue;
				}
			}
			list.Add(dockPoint);
		}
		if (obj_def.type == ObjectDefinition.ObjType.PorterStation && list2.Count > 0)
		{
			return list2[0];
		}
		if (list.Count == 0)
		{
			return null;
		}
		if (list.Count == 1)
		{
			return list[0];
		}
		DockPoint dockPoint2 = list[0];
		Vector2 vector = MainGame.me.player.pos;
		float num = (vector - (Vector2)dockPoint2.tf.position).magnitude;
		for (int k = 1; k < list.Count; k++)
		{
			float magnitude = ((Vector2)list[k].tf.position - vector).magnitude;
			if (num > magnitude)
			{
				num = magnitude;
				dockPoint2 = list[k];
			}
		}
		return dockPoint2;
	}

	public DockPoint[] RefindDockPointsAndGet()
	{
		RefindDockPoints();
		return _dock_points;
	}

	public int GetItemInsertionCoeff(Item item)
	{
		return data.GetItemsCount(item.id);
	}

	public string GetCraftAmountCounter(CraftDefinition craft_definition, int amount = 1)
	{
		List<Item> list = ResModificator.ProcessItemsListBeforeDrop(craft_definition.output, this, MainGame.me.player);
		int num = 0;
		if (list.Count > 0)
		{
			Item item = list[0];
			int num2 = Mathf.RoundToInt(item.min_value.EvaluateFloat(this, MainGame.me.player));
			int num3 = Mathf.RoundToInt(item.max_value.EvaluateFloat(this, MainGame.me.player));
			if (num2 < 0)
			{
				num2 = 0;
			}
			num = ((num3 >= num2) ? ((num2 + num3) / 2) : num2);
		}
		if (craft_definition.output_to_wgo.Count > 0 && craft_definition.output_to_wgo[0].id == "fire")
		{
			num = craft_definition.output_to_wgo[0].value;
		}
		num *= amount;
		if (num > 1)
		{
			return num.ToString();
		}
		return "";
	}

	private void ForceRedrawInSmartDrawer()
	{
		SmartDrawer[] componentsInChildren = GetComponentsInChildren<SmartDrawer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Redraw(force: true);
		}
	}

	public void RedrawGroundSprites()
	{
		if (is_dead)
		{
			return;
		}
		try
		{
			if (base.gameObject == null)
			{
				return;
			}
		}
		catch (NullReferenceException)
		{
			return;
		}
		bool can_move = components.character.enabled || just_built;
		GroundObject[] componentsInChildren = GetComponentsInChildren<GroundObject>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].can_move = can_move;
			if (just_built && !components.character.enabled)
			{
				componentsInChildren[i].can_move = false;
			}
		}
	}

	public bool IsPlayerInvulnerable()
	{
		if (is_player)
		{
			return MainGame.me.player.GetParamInt("is_invulnerable") == 1;
		}
		return false;
	}
}
