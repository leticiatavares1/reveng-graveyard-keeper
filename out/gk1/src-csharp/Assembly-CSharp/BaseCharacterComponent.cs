using System;
using System.Collections.Generic;
using UnityEngine;

public class BaseCharacterComponent : MovementComponent
{
	public enum Environment
	{
		Outside,
		Inside
	}

	private enum WalkAnimationType
	{
		Standard,
		OverheadItem,
		WithTool
	}

	public const string GLOBAL_STATE = "global_state";

	public const string DIRECTION_X = "direction_x";

	public const string DIRECTION_Y = "direction_y";

	public const string DIRECTION_ANGLE = "direction_angle";

	public const string SUB_STATE = "sub_state";

	public const string WAS_DAMAGED = "was_damaged";

	public const string WAS_DAMAGED_DIRECTION = "was_damaged_direction";

	public const string WORK_TOOL_NUM = "work_tool_num";

	public const string DIAGONAL_DIR_ANGLE = "diagonal_direction_angle";

	private const float ANIM_EPS = 0.1f;

	private const float AUTO_DOCK_DIST = 144f;

	private const float PLAYER_RADIUS = 15.36f;

	private const float RE_ACTION = 0.25f;

	private const int DOCKS_LAYER = 11;

	private const int DOCKS_REFIND_FREQUENCY = 1;

	[SerializeField]
	private bool _control_enabled;

	[Space]
	[SerializeField]
	protected Item overhead_item;

	[HideInInspector]
	public Vector2 direction = Vector2.down;

	public bool auto_do_action;

	private WorldGameObject _docked_obj;

	private Camera _cam;

	private Transform _content_tf;

	private Vector3 _content_local_pos;

	private Direction _anim_direction = Direction.Right;

	private CharAnimState _anim_state;

	private int _global_state;

	private bool _needed_tool_bubble_shown;

	private Vector2 _last_anim_vec_dir;

	[NonSerialized]
	public List<WorldGameObject> chests_in_area = new List<WorldGameObject>();

	private float _anim_dir_angle;

	private float _dir_angle;

	private DockPoint _current_dp;

	public Vector2 spawner_coords;

	public MobSpawner spawner;

	private GameObject _anchor_obj;

	public bool anchor_is_wgo;

	public string anchor_obj_gd_point_tag = "";

	public string anchor_obj_wgo_custom_tag = "";

	private DockPoint _nearest_dock_point;

	private float _last_pressed_attack_time = -1f;

	[SerializeField]
	[RuntimeValue]
	private bool _was_damaged;

	[SerializeField]
	[RuntimeValue]
	private bool _ignore_was_damaged;

	private SpriteRenderer[] _sprs;

	public CharacterSkin skin = new CharacterSkin();

	private bool _disabled_interaction_shown;

	[NonSerialized]
	public bool idle_used;

	public Environment cur_environment;

	[NonSerialized]
	public WorldGameObject wgo_hilighted_for_work;

	private bool _deserialize_idle_on_use;

	private BaseCharacterIdle.SerializableCharacterIdle _serialized_idle_data;

	[NonSerialized]
	public bool dont_work_anymore;

	private bool _attack_cached;

	private BaseCharacterAttack _attack;

	private bool _idle_cached;

	private BaseCharacterIdle _idle;

	public bool can_be_locally_controlled { get; set; }

	public GameObject anchor_obj
	{
		get
		{
			if (_anchor_obj == null)
			{
				if (!anchor_is_wgo && !string.IsNullOrEmpty(anchor_obj_gd_point_tag))
				{
					GDPoint gDPointByGDTag = WorldMap.GetGDPointByGDTag(anchor_obj_gd_point_tag);
					_anchor_obj = ((gDPointByGDTag == null) ? null : gDPointByGDTag.gameObject);
				}
				else if (anchor_is_wgo && !string.IsNullOrEmpty(anchor_obj_wgo_custom_tag))
				{
					_anchor_obj = WorldMap.GetWorldGameObjectByCustomTag(anchor_obj_wgo_custom_tag).gameObject;
				}
			}
			return _anchor_obj;
		}
	}

	public bool playing_work_animation
	{
		get
		{
			if (base.components.tool.enabled && base.components.tool.playing_animation)
			{
				return anim_state == CharAnimState.Tool;
			}
			return false;
		}
	}

	public bool playing_animation
	{
		get
		{
			if (!playing_work_animation)
			{
				if (attack.enabled)
				{
					return attack.performing_attack;
				}
				return false;
			}
			return true;
		}
	}

	public CharAnimState anim_state => _anim_state;

	public Direction anim_direction => _anim_direction;

	public bool has_overhead
	{
		get
		{
			if (overhead_item != null && overhead_item.IsNotEmpty())
			{
				return overhead_item.definition != null;
			}
			return false;
		}
	}

	public bool control_enabled
	{
		get
		{
			return _control_enabled;
		}
		set
		{
			_control_enabled = value;
			if (!_control_enabled && (anim_state != 0 || playing_animation))
			{
				SetMovementDir(Vector2.zero);
				base.components.tool.TryStop();
			}
		}
	}

	public float dir_angle => _dir_angle;

	public float anim_dir_angle => _anim_dir_angle;

	public bool repeat_action => Time.time - _last_pressed_attack_time < 0.25f;

	public BaseCharacterAttack attack
	{
		get
		{
			if (!_attack_cached)
			{
				try
				{
					BaseCharacterAttack[] componentsInChildren = base.wgo.GetComponentsInChildren<BaseCharacterAttack>(includeInactive: true);
					if (componentsInChildren.Length != 0)
					{
						_attack = componentsInChildren[0];
					}
					else
					{
						WorldObjectPart wOP = base.wgo.GetWOP();
						if (wOP == null)
						{
							Debug.LogError("Can't find a WOP for object " + base.wgo.obj_id, base.wgo);
							return null;
						}
						_attack = wOP.gameObject.AddComponent<BaseCharacterAttack>();
					}
					_attack_cached = true;
				}
				catch (Exception ex)
				{
					Debug.LogError("Exception at WGO \"" + base.wgo.name + "\": " + ex, base.wgo);
					Debug.LogError(ex.StackTrace);
					Debug.LogError("Is WGO null? - " + (base.wgo == null));
					return null;
				}
			}
			return _attack;
		}
	}

	public BaseCharacterIdle idle
	{
		get
		{
			if (!_idle_cached)
			{
				try
				{
					BaseCharacterIdle[] componentsInChildren = base.wgo.GetComponentsInChildren<BaseCharacterIdle>(includeInactive: true);
					if (componentsInChildren.Length != 0)
					{
						_idle = componentsInChildren[0];
					}
					else
					{
						_idle = base.wgo.GetWOP().gameObject.AddComponent<BaseCharacterIdle>();
					}
					_idle_cached = true;
				}
				catch (Exception ex)
				{
					Debug.LogError("Exception at WGO \"" + base.wgo.name + "\": " + ex, base.wgo);
					Debug.LogError(ex.StackTrace);
					Debug.LogError("Is WGO null? - " + (base.wgo == null));
					return null;
				}
			}
			if (_deserialize_idle_on_use)
			{
				_deserialize_idle_on_use = false;
				_idle.Deserialize(_serialized_idle_data);
			}
			return _idle;
		}
	}

	public PlayerComponent player => base.wgo.GetComponent<PlayerComponent>();

	public void SetAnchor(GDPoint anchor_gd_point)
	{
		if (anchor_gd_point == null)
		{
			_anchor_obj = null;
			anchor_obj_gd_point_tag = "";
			anchor_obj_wgo_custom_tag = "";
		}
		else if (string.IsNullOrEmpty(anchor_gd_point.gd_tag))
		{
			Debug.LogError("GD Tag of GDPoint " + anchor_gd_point.name + " is null!", anchor_gd_point);
		}
		else
		{
			_anchor_obj = anchor_gd_point.gameObject;
			anchor_obj_gd_point_tag = anchor_gd_point.gd_tag;
			anchor_obj_wgo_custom_tag = "";
			anchor_is_wgo = false;
		}
	}

	public void SetAnchor(WorldGameObject anchor_wgo)
	{
		if (anchor_wgo == null)
		{
			_anchor_obj = null;
			anchor_obj_gd_point_tag = "";
			anchor_obj_wgo_custom_tag = "";
		}
		else if (string.IsNullOrEmpty(anchor_wgo.custom_tag))
		{
			Debug.LogError("Custom tag of WGO " + anchor_wgo.name + " is null!", anchor_wgo);
		}
		else
		{
			_anchor_obj = anchor_wgo.gameObject;
			anchor_obj_wgo_custom_tag = anchor_wgo.custom_tag;
			anchor_obj_gd_point_tag = "";
			anchor_is_wgo = true;
		}
	}

	public void SetAnchor(GameObject anchor_go)
	{
		if (anchor_go == null)
		{
			_anchor_obj = null;
			anchor_obj_gd_point_tag = "";
			anchor_obj_wgo_custom_tag = "";
			return;
		}
		GDPoint component = anchor_go.GetComponent<GDPoint>();
		if (component != null)
		{
			SetAnchor(component);
			return;
		}
		WorldGameObject component2 = anchor_go.GetComponent<WorldGameObject>();
		if (component2 != null)
		{
			SetAnchor(component2);
		}
		else
		{
			Debug.LogError("Can not set anchor: " + anchor_go.name + " is not GDPoint or WGO!", anchor_go);
		}
	}

	public override void StartComponent()
	{
		if (!started)
		{
			base.StartComponent();
			on_move_dir = OnChangeDir;
			InitAnimator();
			BaseCharacterAttack baseCharacterAttack = attack;
			if (baseCharacterAttack != null)
			{
				baseCharacterAttack.StartComponent();
			}
			BaseCharacterIdle baseCharacterIdle = idle;
			if (baseCharacterIdle != null)
			{
				baseCharacterIdle.StartComponent();
			}
			SetAnimationState(CharAnimState.Idle);
			_cam = Camera.main;
			overhead_item = null;
			ProcessDirection(direction);
			if (base.wgo.is_player)
			{
				base.wgo.SetCurrentItem(ItemDefinition.ItemType.None);
				SetToolGraphics(0);
				GUIElements.me.hud.toolbar.keyboard.SetClickCallback(UseItemFromToolbar);
			}
		}
	}

	public void OnStopped()
	{
		SetAnimationState(CharAnimState.Idle);
	}

	public void OnStartWalking()
	{
		SetAnimationState(CharAnimState.Walking);
	}

	protected override int GetExecutionOrder()
	{
		return 5;
	}

	public void TeleportWithFade(Vector2 dest, GJCommons.VoidDelegate middle_delegate = null, GJCommons.VoidDelegate finished_delegate = null)
	{
		MainGame.me.save.quests.CheckKeyQuests("teleport");
		if (!base.wgo.is_player)
		{
			base.tf.position = dest * 96f;
		}
		else
		{
			if (base.wgo.dont_update)
			{
				return;
			}
			base.wgo.dont_update = true;
			GUIElements.ChangeBubblesVisibility(show: false);
			CameraTools.Fade(delegate
			{
				base.tf.position = dest * 96f;
				base.wgo.GetComponent<ChunkedGameObject>().RecalculateChunk();
				CameraTools.MoveToPos(base.tf.position);
				middle_delegate.TryInvoke();
				CameraTools.UnFade(delegate
				{
					base.wgo.dont_update = false;
					GUIElements.ChangeBubblesVisibility(show: true);
					finished_delegate.TryInvoke();
				});
			});
		}
	}

	public void TeleportWithFade(WorldGameObject wgo, GJCommons.VoidDelegate middle_delegate = null, GJCommons.VoidDelegate finished_delegate = null)
	{
		TeleportWithFade(wgo.grid_pos, middle_delegate, finished_delegate);
	}

	public void TeleportWithFade(Transform trnsfrm, GJCommons.VoidDelegate middle_delegate = null, GJCommons.VoidDelegate finished_delegate = null)
	{
		TeleportWithFade(trnsfrm.position / 96f, middle_delegate, finished_delegate);
	}

	public bool IsInSector(BaseCharacterComponent other_char, int sector_index = 0, bool ignore_obstacles = true)
	{
		return IsInSector(other_char.wgo.pos, sector_index, ignore_obstacles);
	}

	public bool IsInSector(WorldGameObject wgo, int sector_index = 0, bool ignore_obstacles = true)
	{
		return IsInSector(wgo.pos, sector_index, ignore_obstacles);
	}

	public bool IsInSector(Vector3 pos, int sector_index = 0, bool ignore_obstacles = true)
	{
		WorldObjectPart wOP = base.wgo.GetWOP();
		if (wOP == null)
		{
			Debug.LogError("IsInSector(): NULL WorldObjectPart", base.wgo);
			return false;
		}
		if (sector_index >= wOP.visibility_sectors.Length)
		{
			Debug.LogError("IsInSector(): wrong sector index: " + sector_index, base.wgo);
			return false;
		}
		VisibilitySector visibilitySector = wOP.visibility_sectors[sector_index];
		if (visibilitySector == null)
		{
			Debug.LogError("IsInSector(): NULL sector", base.wgo);
			return false;
		}
		return visibilitySector.IsTouching(pos, ignore_obstacles);
	}

	public override bool HasUpdate()
	{
		return true;
	}

	public override void UpdateComponent(float delta_time)
	{
		if (base.wgo.is_player && !player_controlled_by_script)
		{
			UpdatePlayer(delta_time);
		}
		base.UpdateComponent(delta_time);
		BaseCharacterAttack baseCharacterAttack = attack;
		if (baseCharacterAttack != null)
		{
			baseCharacterAttack.UpdateComponent(delta_time);
		}
		BaseCharacterIdle baseCharacterIdle = idle;
		if (baseCharacterIdle != null)
		{
			baseCharacterIdle.UpdateComponent(delta_time);
		}
		CheckAnimatorStates();
		if (base.components.animator.ParamExists("global_state"))
		{
			int integer = base.components.animator.GetInteger("global_state");
			if (integer != _global_state)
			{
				Debug.LogError("Animator error: _global_state: " + _global_state + " differs from the animator: " + integer + " for wgo: " + base.wgo.name, base.wgo);
			}
		}
	}

	private bool PlayerControlIsDisabled()
	{
		if (control_enabled && BaseGUI.all_guis_closed && can_be_locally_controlled)
		{
			return MainGame.me.build_mode_logics.IsBuilding();
		}
		return true;
	}

	private void UpdatePlayer(float delta_time)
	{
		if (PlayerControlIsDisabled())
		{
			if (base.movement_state != 0 || movement_dir.magnitude > 0f)
			{
				StopMovement();
				movement_dir = Vector2.zero;
			}
			return;
		}
		if (LazyInput.GetKeyDown(GameKey.GameGUI))
		{
			GUIElements.me.game_gui.Open();
			return;
		}
		if (LazyInput.GetKeyDown(GameKey.IngameMenu))
		{
			GUIElements.me.ingame_menu.Open();
			return;
		}
		if (LazyInput.GetKeyDown(GameKey.Inventory))
		{
			GUIElements.me.game_gui.OpenAtTab(GameGUI.TabType.Inventory);
		}
		if (LazyInput.GetKeyDown(GameKey.KnownNPCs))
		{
			GUIElements.me.game_gui.OpenAtTab(GameGUI.TabType.NPCs);
		}
		if (LazyInput.GetKeyDown(GameKey.Techs))
		{
			GUIElements.me.game_gui.OpenAtTab(GameGUI.TabType.Techs);
		}
		if (LazyInput.GetKeyDown(GameKey.Map))
		{
			GUIElements.me.game_gui.OpenAtTab(GameGUI.TabType.Map);
		}
		ProcessToolbar();
		if (!ProcessAttack() && !ProcessWork())
		{
			RefindDocks();
			if (LoadingGUI.is_shown || !ProcessInteraction())
			{
				OnChangeDir(LazyInput.GetDirection());
			}
		}
	}

	private void ProcessToolbar()
	{
		for (int i = 0; i < 4; i++)
		{
			if (LazyInput.GetKeyDown(LazyInput.toolbar_keys[i]))
			{
				UseItemFromToolbar(i);
				LazyInput.ClearKeyDown(LazyInput.toolbar_keys[i]);
				break;
			}
		}
	}

	private bool IsEnoughEnergyForDash()
	{
		if (base.wgo.energy > 2f)
		{
			return true;
		}
		EffectBubblesManager.ShowImmediately(base.wgo.bubble_pos, GJL.L("not_enough_something", "(en)"), EffectBubblesManager.BubbleColor.Energy);
		return false;
	}

	private bool ProcessDash()
	{
		if (EnvironmentEngine.me != null && EnvironmentEngine.me.IsTimeStopped())
		{
			return false;
		}
		if (LazyInput.GetKey(GameKey.Work) || base.wgo.temp_do_work)
		{
			return false;
		}
		if ((LazyInput.GetKeyDown(GameKey.Dash) || LazyInput.GetKeyDown(GameKey.Dash2)) && IsEnoughEnergyForDash())
		{
			last_pressed_dash_time = Time.time;
		}
		if (dash_remaining_time < 0f)
		{
			if (!(Time.time - last_pressed_dash_time < 0.025f))
			{
				if (dash_remaining_time > -1f)
				{
					state = MovementState.None;
					dash_remaining_time = -3f;
					Debug.Log("Ended dash");
				}
				return false;
			}
			if (!base.components.character.player.TrySpendEnergy(2f))
			{
				Debug.LogError("FATAL ERROR! Not enough energy for dash, but dash was started!");
				return false;
			}
			ProcessDirection(LazyInput.GetDirection());
			dash_direction = direction.normalized;
			movement_dir = Vector2.zero;
			state = MovementState.Dash;
			dash_remaining_time = 0.1f;
			Debug.Log("Started Dash");
		}
		return true;
	}

	private bool CheckEnegryForPlayerAtack()
	{
		Item item = (base.wgo.is_player ? base.wgo.GetEquippedWeapon() : null);
		if (base.wgo.is_player && item != null && item.definition != null && item.definition.params_on_use != null && !item.definition.params_on_use.IsEmpty() && base.wgo.energy < item.definition.params_on_use.Get("energy") * -1f)
		{
			EffectBubblesManager.ShowImmediately(base.wgo.bubble_pos, GJL.L("not_enough_something", "(en)"), EffectBubblesManager.BubbleColor.Energy);
			return false;
		}
		return true;
	}

	private bool ProcessAttack()
	{
		if (state == MovementState.Dash)
		{
			return false;
		}
		if (LazyInput.GetKeyDown(GameKey.Attack) && CheckEnegryForPlayerAtack())
		{
			_last_pressed_attack_time = Time.time;
		}
		if (attack.performing_attack)
		{
			return true;
		}
		if ((LazyInput.GetKeyDown(GameKey.Attack) || repeat_action) && !playing_work_animation)
		{
			ItemDefinition.ItemType itemType = (base.wgo.is_player ? base.wgo.GetEquippedWeapon() : null)?.definition.type ?? ItemDefinition.ItemType.None;
			if ((!base.wgo.is_player && itemType == ItemDefinition.ItemType.None) || itemType == ItemDefinition.ItemType.Sword)
			{
				if (!CheckEnegryForPlayerAtack())
				{
					return false;
				}
				Debug.Log("ATTACK!!!");
				ProcessDirection(LazyInput.GetDirection());
				SetWeaponGraphics((int)itemType);
				if (attack.Perform(anim_direction, 0, OnPlayersAttackPerformed))
				{
					CheckPossibleStopWalking();
				}
			}
			if (has_overhead)
			{
				DropOverheadItem(attack.performing_attack);
			}
		}
		return attack.performing_attack;
	}

	private bool ProcessWork()
	{
		bool flag = LazyInput.GetKey(GameKey.Work) || base.wgo.temp_do_work;
		if (base.components.interaction.nearest != null && base.components.interaction.nearest.has_linked_worker)
		{
			return false;
		}
		if (base.wgo.player_cant_work)
		{
			return false;
		}
		if (dont_work_anymore)
		{
			if (flag)
			{
				flag = false;
			}
			else
			{
				dont_work_anymore = false;
			}
		}
		if (!flag)
		{
			_needed_tool_bubble_shown = false;
			base.components.tool.TryStop();
			ResetDockPoints();
			if (base.movement_state == MovementState.Following || base.movement_state == MovementState.GoTo)
			{
				StopMovement();
			}
		}
		if (!flag && !playing_animation)
		{
			base.components.tool.ResetLastActionTime();
			return false;
		}
		bool keyDown = LazyInput.GetKeyDown(GameKey.Work);
		if (flag)
		{
			if (!DoDockCheck(keyDown))
			{
				if (has_overhead)
				{
					DropOverheadItem();
				}
				return false;
			}
			if (_current_dp != null && _docked_obj == null)
			{
				if (has_overhead)
				{
					DropOverheadItem(to_right: true);
				}
				return true;
			}
		}
		bool placed_on_dock_point = DockIsOk();
		if (base.components.tool.UseTool(placed_on_dock_point))
		{
			CheckPossibleStopWalking();
			if (has_overhead)
			{
				DropOverheadItem();
			}
			return true;
		}
		return _current_dp != null;
	}

	private bool ProcessInteraction()
	{
		if (!LazyInput.GetKey(GameKey.Interaction))
		{
			base.components.interaction.StopInteraction();
		}
		if (PlayerControlIsDisabled())
		{
			return false;
		}
		if (!LazyInput.GetKeyDown(GameKey.Interaction))
		{
			return false;
		}
		bool flag = TryOtherInteractions();
		if (!flag && base.components.interaction.Interact(interaction_start: true))
		{
			flag = true;
			CheckPossibleStopWalking();
		}
		if (!flag && has_overhead)
		{
			DropOverheadItem();
		}
		return flag;
	}

	public void RefindDocks(bool force = false)
	{
		if (force || Time.frameCount % 1 == 0)
		{
			_nearest_dock_point = GetNearestDockPoint();
			_ = _nearest_dock_point != null;
		}
	}

	private void UseItemFromToolbar(int index)
	{
		Item itemById = base.wgo.GetItemById(MainGame.me.save.GetEquippedItem(index));
		if (itemById != null)
		{
			base.wgo.UseItemFromInventory(itemById);
		}
	}

	public void ShowCustomNeedBubble(string text)
	{
		if (!_needed_tool_bubble_shown)
		{
			base.wgo.Say(text);
			_needed_tool_bubble_shown = true;
		}
	}

	public void ShowNeededToolBubble(bool show_tech_lock)
	{
		if (_needed_tool_bubble_shown)
		{
			return;
		}
		ObjectDefinition nearest_definition = base.components.interaction.nearest_definition;
		if (nearest_definition != null && !nearest_definition.tool_actions.no_actions)
		{
			if (show_tech_lock)
			{
				ShowCustomNeedBubble("no_tech_bubble");
			}
			else
			{
				ShowCustomNeedBubble("no_" + nearest_definition.tool_actions.action_tools[0].ToString().ToLower() + "_bubble");
			}
		}
	}

	public void OnPlayersAttackPerformed(bool success)
	{
		if (!repeat_action)
		{
			SetWeaponGraphics(0);
			return;
		}
		ProcessDirection(LazyInput.GetDirection());
		_last_pressed_attack_time = 0f;
		attack.Perform(anim_direction, 0, OnPlayersAttackPerformed);
	}

	public void InterruptAttack()
	{
		attack.InterruptAttack();
		SetAnimationState(CharAnimState.Idle);
	}

	protected virtual bool TryOtherInteractions()
	{
		if (DropResGameObject.currently_higlighted_obj != null)
		{
			if (!DropResGameObject.currently_higlighted_obj.CanPickupWithInteraction(this))
			{
				return false;
			}
			if (has_overhead)
			{
				DropOverheadItem();
			}
			Item item = new Item(DropResGameObject.currently_higlighted_obj.res);
			if (DropResGameObject.currently_higlighted_obj.res != null)
			{
				item.sub_name = DropResGameObject.currently_higlighted_obj.res.sub_name;
			}
			SetOverheadItem(item);
			DropResGameObject.currently_higlighted_obj.is_collected = true;
			DropResGameObject.currently_higlighted_obj.DestroyLinkedHint();
			DropResGameObject.currently_higlighted_obj = null;
			OnChangeDir(LazyInput.GetDirection());
			return true;
		}
		return false;
	}

	private bool DoDockCheck(bool try_same_dir_point = true)
	{
		if (base.components.interaction.nearest != null && base.components.interaction.nearest.has_linked_worker)
		{
			return false;
		}
		if (base.wgo.player_cant_work)
		{
			return false;
		}
		try_same_dir_point = false;
		if (playing_animation)
		{
			return true;
		}
		if (_current_dp == null)
		{
			FindDockPoint();
			if (_current_dp == null)
			{
				WorldGameObject nearest = base.components.interaction.nearest;
				if (nearest != null && nearest.obj_def.tool_actions.no_actions)
				{
					_ = nearest.is_removing;
				}
				return false;
			}
		}
		if (!_current_dp.parent_wgo.CanProcessWork())
		{
			_current_dp = (_nearest_dock_point = null);
			return false;
		}
		_current_dp.CheckIfReached();
		if (_current_dp.just_rotate)
		{
			ProcessDirection(_current_dp.reach_dir);
		}
		if (_current_dp.reached || _current_dp.just_rotate)
		{
			StopMovement();
			_docked_obj = _current_dp.parent_wgo;
			base.tf.SetXY(_current_dp.tf.position);
		}
		else
		{
			Direction actionDir = _current_dp.GetActionDir();
			Direction direction = ((Vector2)_current_dp.tf.position - base.wgo.pos).ToDirection();
			if (try_same_dir_point && actionDir != direction)
			{
				Debug.Log("dp = " + actionDir.ToString() + ", to_dir = " + direction);
				DockPoint[] componentsInChildren = _current_dp.parent_wgo.GetComponentsInChildren<DockPoint>();
				foreach (DockPoint dockPoint in componentsInChildren)
				{
					if (dockPoint.GetActionDir() == direction)
					{
						dockPoint.SetTarget(_current_dp.target);
						_current_dp.SetTarget(null);
						_current_dp = dockPoint;
						return DoDockCheck(try_same_dir_point: false);
					}
				}
			}
			GoTo(_current_dp.tf.position);
		}
		return true;
	}

	public void ResetDockPoints()
	{
		if (_current_dp != null)
		{
			_current_dp.SetTarget(null);
			_current_dp = null;
		}
	}

	private bool DockIsOk()
	{
		if (DropResGameObject.currently_higlighted_obj == null && _current_dp != null && base.components.interaction.nearest != null && _current_dp.parent_wgo == _docked_obj && _current_dp.GetActionDir() == anim_direction)
		{
			return _current_dp.CalcDistToTarget().EqualsTo(0f, 0.002f);
		}
		return false;
	}

	public DockPoint GetNearestDockPoint()
	{
		List<DockPoint> list = new List<DockPoint>();
		if (MainGame.me.player.components.character.wgo_hilighted_for_work != null)
		{
			DockPoint[] componentsInChildren = MainGame.me.player.components.character.wgo_hilighted_for_work.GetComponentsInChildren<DockPoint>();
			if (componentsInChildren.Length != 0)
			{
				list.AddRange(componentsInChildren);
			}
		}
		if (list.Count == 0)
		{
			Collider2D[] array = Physics2D.OverlapCircleAll(base.wgo.pos, 144f, 2048);
			if (array.Length == 0)
			{
				return null;
			}
			WorldGameObject nearest = base.components.interaction.nearest;
			bool nearest_has_action = base.components.interaction.nearest_has_action;
			if (nearest != null && LazyInput.GetKey(GameKey.Select))
			{
				Debug.Log("nearest: " + nearest, nearest);
			}
			Collider2D[] array2 = array;
			foreach (Collider2D collider2D in array2)
			{
				if (!(collider2D == null))
				{
					DockPoint component = collider2D.GetComponent<DockPoint>();
					if (!(component == null) && !component.shouldnt_be_used && (!nearest_has_action || !(component.parent_wgo != nearest)) && !(component.parent_wgo == null) && component.parent_wgo.obj_def != null && (!component.parent_wgo.obj_def.tool_actions.no_actions || component.parent_wgo.is_removing) && !component.IsUnreachable(15.36f) && !component.parent_wgo.has_linked_worker && !component.parent_wgo.player_cant_work && (!component.craft.enabled || ((!component.craft.is_crafting || !component.craft.current_craft.is_auto || component.craft.current_craft.hidden) && (component.craft.is_crafting || !component.craft.IsCraftQueueEmpty()))))
					{
						list.Add(component);
					}
				}
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		if (list.Count == 1)
		{
			return list[0];
		}
		Vector2 vector = (Vector2)base.tf.position + (Vector2)anim_direction.ToVec3() * 19.2f;
		float num = float.MaxValue;
		int index = 0;
		for (int j = 0; j < list.Count; j++)
		{
			DockPoint dockPoint = list[j];
			float num2 = ((Vector2)dockPoint.tf.position - vector).sqrMagnitude;
			if (dockPoint.GetActionDir() != anim_direction)
			{
				num2 += 3500f;
			}
			if (num2 < num)
			{
				num = num2;
				index = j;
			}
		}
		return list[index];
	}

	public void FindDockPoint()
	{
		_current_dp = _nearest_dock_point;
		if (_current_dp == null)
		{
			_current_dp = GetNearestDockPoint();
		}
		if (_current_dp != null)
		{
			_current_dp.SetTarget(this);
		}
	}

	public override bool HasLateUpdate()
	{
		return true;
	}

	public override void LateUpdateComponent()
	{
		base.wgo.RoundContentPos();
	}

	public void SetAnimationState(CharAnimState state, ItemDefinition.ItemType item_type = ItemDefinition.ItemType.None)
	{
		_anim_state = state;
		SetGlobalState(state, item_type);
	}

	private void SetGlobalState(CharAnimState state, ItemDefinition.ItemType item_type = ItemDefinition.ItemType.None)
	{
		int num = ((state == CharAnimState.Tool) ? ((int)(100 + item_type)) : ((int)state));
		if (_global_state != num)
		{
			if (state == CharAnimState.Tool)
			{
				Debug.Log("Set global state: " + item_type.ToString() + " tool");
			}
			if (num == 4)
			{
				Debug.Log("Trigger " + num);
				base.components.animator.SetTrigger("use_tool_" + num);
			}
			SetGlobalState(num);
		}
	}

	public void DeserializeGlobalState(int new_state)
	{
		_global_state = new_state;
	}

	public void SetGlobalState(int new_state)
	{
		if (_global_state != new_state)
		{
			_global_state = new_state;
			CheckAnimatorStates();
			base.components.animator.enabled = true;
			base.components.animator.SetInteger("global_state", _global_state);
			if (new_state == 0)
			{
				base.components.animator.SetInteger("sub_state", base.components.character.idle_animation);
			}
		}
	}

	public void StartPrayAnimation(bool success)
	{
		control_enabled = false;
		SetGlobalState(-12);
		base.components.animator.SetTrigger("start_pray");
		WorldMap.GetChurchPulpit().animator.SetBool("success", success);
	}

	private void ProcessDirection(Vector2 s)
	{
		if (!playing_animation && !s.magnitude.Equals(0f))
		{
			if (!started)
			{
				base.components.StartComponents();
			}
			direction = s;
			SetDirectionVectorForAnimator(s);
		}
	}

	public void LookAt(WorldGameObject wobj)
	{
		ProcessDirection(base.wgo.DirTo(wobj.pos));
	}

	public void LookAt(GameObject go)
	{
		ProcessDirection(base.wgo.DirTo(go.transform.position));
	}

	public void LookAt(Direction dir)
	{
		ProcessDirection(dir.ToVec());
	}

	public void LookAt(Vector2 dir)
	{
		ProcessDirection(dir);
	}

	private void OnChangeDir(Vector2 dir)
	{
		ProcessDirection(dir);
		ProcessMovement(dir);
	}

	private void ProcessMovement(Vector2 s)
	{
		if (s.magnitude > 0f)
		{
			OnStartWalking();
			_docked_obj = null;
			movement_dir = s.normalized;
			if (base.wgo.is_player && control_enabled)
			{
				FloatingWorldGameObject.MoveCurrentFloatingObject(MainGame.me.player.transform.localPosition, is_global_pos: false);
			}
		}
		else
		{
			OnStopped();
			movement_dir = Vector2.zero;
		}
	}

	private void CheckPossibleStopWalking()
	{
		movement_dir = Vector2.zero;
		if (anim_state == CharAnimState.Walking || _global_state == -1)
		{
			SetAnimationState(CharAnimState.Idle);
		}
	}

	private void SetDirectionVectorForAnimator(Vector2 dir)
	{
		if (!dir.EqualsTo(_last_anim_vec_dir, 0.1f))
		{
			float x = dir.x;
			float y = dir.y;
			_dir_angle = Mathf.Atan2(y, x) * 57.29578f;
			_anim_dir_angle = Mathf.Round(_dir_angle / 90f) * 90f;
			float v = Mathf.Round(_dir_angle / 45f) * 45f;
			base.components.animator.SetFloat("diagonal_direction_angle", v);
			CheckAnimatorStates();
			base.components.animator.SetFloat("direction_angle", _anim_dir_angle);
			_last_anim_vec_dir.x = x;
			_last_anim_vec_dir.y = y;
			_anim_direction = _last_anim_vec_dir.ToDirection();
		}
	}

	public void OnEnterChestArea(WorldGameObject chest)
	{
		if (!chests_in_area.Contains(chest))
		{
			chests_in_area.Add(chest);
		}
	}

	public void OnExitChestArea(WorldGameObject chest)
	{
		if (chests_in_area.Contains(chest))
		{
			chests_in_area.Remove(chest);
		}
	}

	public Item GetOverheadItem()
	{
		if (!has_overhead)
		{
			return null;
		}
		return overhead_item;
	}

	public void TryDropOverheadItem()
	{
		if (has_overhead)
		{
			base.wgo.DropItem(overhead_item);
			SetOverheadItem(null);
		}
	}

	public void SetOverheadItem(Item item)
	{
		Debug.Log("SetOverheadItem = " + ((item == null) ? "null" : item.id));
		Item item2 = overhead_item;
		overhead_item = item;
		PlayerComponent playerComponent = player;
		if (playerComponent.spr_overhead_obj == null)
		{
			return;
		}
		if ((bool)playerComponent.spr_tool)
		{
			playerComponent.spr_tool.sprite = null;
		}
		if ((bool)playerComponent.spr_tool_2)
		{
			playerComponent.spr_tool_2.sprite = null;
		}
		if (!MainGame.game_starting && item2 != item)
		{
			Sounds.PlaySound((item == null) ? "item_2h_drop" : "item_2h_pickup");
		}
		playerComponent.spr_overhead_obj.gameObject.SetActive(item?.IsNotEmpty() ?? false);
		if (item == null || item.IsEmpty())
		{
			SetWalkAnimationType(WalkAnimationType.Standard);
			if (Application.isPlaying)
			{
				MainGame.me.save.quests.CheckKeyQuests("overhead_none");
			}
		}
		else
		{
			SetWalkAnimationType(WalkAnimationType.OverheadItem);
			playerComponent.spr_overhead_obj.sprite = EasySpritesCollection.GetSprite(item.GetOverheadIcon());
			if (Application.isPlaying)
			{
				string id = item.id;
				MainGame.me.save.quests.CheckKeyQuests("overhead_" + id);
			}
		}
		if (item?.definition != null && item.definition.autouse)
		{
			item.UseItem(MainGame.me.player);
		}
	}

	public void SetCarryingItem(Item item)
	{
		Debug.Log("SetCarryingItem = " + ((item == null) ? "null" : item.id));
		SpriteRenderer spriteRenderer = base.wgo?.wop?.carrying_item_sprite;
		if (spriteRenderer == null)
		{
			Debug.LogError("SetCarryingItem error: spr is null!");
			return;
		}
		Collider2D component = spriteRenderer.GetComponent<Collider2D>();
		if (item == null || item.IsEmpty())
		{
			spriteRenderer.sprite = null;
			if (component != null)
			{
				component.enabled = false;
			}
		}
		else
		{
			spriteRenderer.sprite = EasySpritesCollection.GetSprite(item.GetIcon());
			if (component != null)
			{
				component.enabled = true;
			}
		}
	}

	public void SwitchTorch()
	{
		ItemDefinition.ItemType itemType = ((base.wgo.GetCurrentItemType() != ItemDefinition.ItemType.Torch) ? ItemDefinition.ItemType.Torch : ItemDefinition.ItemType.None);
		base.wgo.SetCurrentItem(itemType);
		SetToolGraphics((int)itemType);
	}

	public void SetToolGraphics(int tool_n)
	{
		PlayerComponent playerComponent = player;
		if (!(playerComponent.spr_tool == null))
		{
			Debug.Log("SetToolGraphics " + tool_n);
			skin.weapon = tool_n;
			if (tool_n > 0)
			{
				SetWalkAnimationType(WalkAnimationType.WithTool);
				Debug.Log("Set sprite: " + playerComponent.spr_tool.sprite);
				playerComponent.spr_overhead_obj.gameObject.SetActive(value: false);
			}
			else
			{
				playerComponent.spr_tool.sprite = null;
				SetOverheadItem(overhead_item);
			}
			if ((bool)playerComponent.spr_tool_2)
			{
				playerComponent.spr_tool_2.sprite = playerComponent.spr_tool.sprite;
			}
			if ((bool)playerComponent.light_go)
			{
				playerComponent.light_go.SetActive(tool_n == 9);
			}
		}
	}

	public void SetWeaponGraphics(int tool_n)
	{
		PlayerComponent playerComponent = player;
		if (!(playerComponent.spr_tool == null))
		{
			skin.weapon = tool_n;
			if ((bool)playerComponent.spr_tool_2)
			{
				playerComponent.spr_tool_2.sprite = playerComponent.spr_tool.sprite;
			}
			if ((bool)playerComponent.light_go)
			{
				playerComponent.light_go.SetActive(tool_n == 9);
			}
		}
	}

	private void SetWalkAnimationType(WalkAnimationType a)
	{
		float num = 0f;
		num = a switch
		{
			WalkAnimationType.Standard => 0f, 
			WalkAnimationType.OverheadItem => 0.1f, 
			WalkAnimationType.WithTool => -0.1f, 
			_ => throw new ArgumentOutOfRangeException("a"), 
		};
		Debug.Log("Set walk animation type = " + a);
		base.components.animator.SetFloat("walk_type_f", num);
	}

	protected void InitAnimator()
	{
	}

	private void CheckAnimatorStates()
	{
	}

	public void DropOverheadItem(bool to_right = false)
	{
		if (!has_overhead)
		{
			Debug.LogError("Trying to drop null overhead item");
			return;
		}
		DropResGameObject.Drop(base.tf.position, overhead_item, base.tf.parent, to_right ? anim_direction.ClockwiseDir() : anim_direction, 3f, UnityEngine.Random.Range(0, 2));
		SetOverheadItem(null);
	}

	public void OnWasDamaged(float damage_direction)
	{
		if (!_ignore_was_damaged)
		{
			_was_damaged = true;
		}
		damage_direction = Mathf.Round(damage_direction / 90f) * 90f;
		base.components.animator.SetFloat("was_damaged_direction", damage_direction);
		base.components.animator.SetTrigger("was_damaged");
	}

	public bool WasDamaged(bool clear_flag)
	{
		if (!_was_damaged)
		{
			return false;
		}
		if (clear_flag)
		{
			_was_damaged = false;
		}
		return true;
	}

	public void ChangeDamageFlagIgnoring(bool ignore)
	{
		_ignore_was_damaged = ignore;
		_was_damaged = false;
	}

	public void SetLocalPlayerState(bool is_local_player)
	{
		Debug.Log("SetLocalPlayerState, is_local = " + is_local_player, base.wgo);
		can_be_locally_controlled = is_local_player;
		if (is_local_player)
		{
			MainGame.me.SetMainPlayer(player);
		}
		base.wgo.GetComponent<ChunkedGameObject>().always_active = true;
	}

	public bool ShowBubbleToLeft(bool? to_left)
	{
		return to_left ?? (anim_direction == Direction.Right);
	}

	private void RescanChildSprites()
	{
		_sprs = base.wgo.GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
	}

	public void LateUpdate()
	{
		if (_sprs == null)
		{
			RescanChildSprites();
		}
		SpriteRenderer[] sprs = _sprs;
		foreach (SpriteRenderer spriteRenderer in sprs)
		{
			if (!(spriteRenderer == null) && !(spriteRenderer.sprite == null))
			{
				string name = spriteRenderer.sprite.name;
				string text = skin.ReplaceSpriteName(name);
				if (name != text)
				{
					spriteRenderer.sprite = EasySpritesCollection.GetSprite(text);
				}
			}
		}
	}

	public Ground.GroudType GetGroundTypeUnderCharacter()
	{
		return WorldMap.GetGroundType(base.tf.position);
	}

	public override void UpdateEnableState(ObjectDefinition.ObjType obj_type)
	{
		base.enabled = base.wgo.is_player || base.wgo.obj_def.IsCharacter();
	}

	public void Recache()
	{
		_attack_cached = (_idle_cached = false);
	}

	public void ShowDisabledInteractionBubble(WorldGameObject wgo)
	{
		if (!_disabled_interaction_shown)
		{
			_disabled_interaction_shown = true;
			MainGame.me.player.Say("disabled_interactions", delegate
			{
				_disabled_interaction_shown = false;
			});
		}
	}

	public override void RefreshComponentBubbleData(bool show_interaction_buttons)
	{
	}

	public void DeserializeIdle(BaseCharacterIdle.SerializableCharacterIdle data)
	{
		_deserialize_idle_on_use = true;
		_serialized_idle_data = data;
	}

	public void SetWorkerToolNum(ItemDefinition.ItemType tool_type)
	{
		try
		{
			if (!base.wgo.gameObject.activeInHierarchy)
			{
				base.wgo.gameObject.SetActive(value: true);
			}
			ChunkedGameObject component = base.wgo.GetComponent<ChunkedGameObject>();
			if (component != null)
			{
				component.active_now_because_of_work = tool_type != ItemDefinition.ItemType.None;
			}
		}
		catch (Exception message)
		{
			Debug.LogError(message);
		}
		if (base.components.animator.ParamExists("work_tool_num"))
		{
			base.components.animator.SetInteger("work_tool_num", (int)tool_type);
		}
	}

	public void SetNoWorkerTool()
	{
		try
		{
			if (!base.wgo.gameObject.activeInHierarchy)
			{
				base.wgo.gameObject.SetActive(value: true);
			}
		}
		catch (Exception message)
		{
			Debug.LogError(message);
		}
		if (base.components.animator.ParamExists("work_tool_num"))
		{
			base.components.animator.SetInteger("work_tool_num", 0);
		}
		try
		{
			ChunkedGameObject component = base.wgo.GetComponent<ChunkedGameObject>();
			if (component != null)
			{
				component.active_now_because_of_work = false;
			}
		}
		catch (Exception message2)
		{
			Debug.LogError(message2);
		}
	}
}
