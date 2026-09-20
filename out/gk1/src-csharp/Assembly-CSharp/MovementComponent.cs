using System;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class MovementComponent : WorldGameObjectComponent
{
	protected delegate void OnMove(Vector2 dir);

	[Serializable]
	public enum MovementState
	{
		None,
		Following,
		GoTo,
		GoToVector,
		AnimCurve,
		Dash
	}

	public enum GoToMethod
	{
		AStar,
		GDGraph,
		Direct
	}

	[Serializable]
	public class Modifier
	{
		public float value;

		public float dec_speed;

		public Modifier(float v = 0f, float decreace_speed = 0.1f)
		{
			value = v;
			dec_speed = decreace_speed;
		}

		public void Update(float delta_time)
		{
			if (!value.EqualsTo(0f))
			{
				float num = Mathf.Sign(value);
				value -= dec_speed * delta_time * num;
				if (Mathf.Sign(value) != num)
				{
					value = 0f;
				}
			}
		}
	}

	public const bool DEBUG_PATHWALKING = false;

	public const bool SET_PLAYER_STATIC = false;

	protected const float DASH_TIME = 0.1f;

	protected const float DASH_DIST = 1.5f;

	protected const float DASH_COST = 2f;

	private const int STEPS_COUNT = 5;

	public const float MIN_DELTA = 0.01f;

	public const float VERTICAL_K = 0.8f;

	public const float STEP_EPSILON = 0.001f;

	public const float MIN_GOTO_DEST_DIF = 0.1f;

	public const float FOLLOW_PATHFINDING_DELAY = 0.5f;

	public const float WALLS_CHECK_DELAY = 0.1f;

	public const float DEFAULT_MASS = 100f;

	public const float DEFAULT_DRAG = 50f;

	[RuntimeValue("Runtime values", false)]
	public float velocity;

	[RuntimeValue]
	public Vector2 movement_dir;

	[RuntimeValue]
	public Vector2 delta_vec;

	[RuntimeValue]
	public bool max_speed_reached;

	[NonSerialized]
	protected int path_waypoint;

	[NonSerialized]
	protected float dir_distance;

	[NonSerialized]
	protected float follow_delay;

	[NonSerialized]
	protected float min_follow_dis;

	[NonSerialized]
	protected float follow_pathfinding_delay;

	[NonSerialized]
	protected float goto_vector_dist;

	[NonSerialized]
	protected float min_follow_dist;

	[NonSerialized]
	protected float last_step;

	[NonSerialized]
	protected float walked_dist;

	[NonSerialized]
	protected float calcultated_average_step;

	[NonSerialized]
	protected float walls_check_delay;

	[NonSerialized]
	protected Vector2 goto_vector_dir;

	[NonSerialized]
	protected Vector2 current_point_pos;

	[NonSerialized]
	protected Vector2 dir_before_path_finding;

	[NonSerialized]
	protected Vector3 current_pos;

	[NonSerialized]
	protected bool astar_following;

	[NonSerialized]
	protected bool no_walls_to_target;

	[NonSerialized]
	protected bool stopped = true;

	[NonSerialized]
	protected MovementState state;

	[NonSerialized]
	protected Transform target;

	[NonSerialized]
	protected GJCommons.VoidDelegate on_complete;

	[NonSerialized]
	protected GJCommons.VoidDelegate on_failed;

	[NonSerialized]
	protected OnMove on_move_dir;

	[NonSerialized]
	protected List<float> last_steps = new List<float>();

	[NonSerialized]
	protected string event_on_complete = "";

	[NonSerialized]
	private Vector2 _nearest_gd_point_pos = Vector2.zero;

	private AnimationCurve _current_curve;

	private Vector2 _curve_delta;

	private Vector2 _prev_curve_pos;

	private float _curve_normalized_time;

	private int _curve_mvmnt_start_frame;

	private bool _curve_based_on_anim_timing;

	[NonSerialized]
	public List<Vector3> cur_astar_path;

	private int _stuck_counter;

	private Vector2 _cur_move_dv = Vector2.zero;

	private AStarSearcher _astar;

	private bool _using_gd_graph;

	[NonSerialized]
	public bool player_controlled_by_script;

	private float _stored_speed;

	private bool _in_stored_speed_mode;

	private string _target_gd_point_tag = "";

	[NonSerialized]
	protected float dash_remaining_time = -3f;

	[NonSerialized]
	protected float last_pressed_dash_time;

	[NonSerialized]
	protected Vector2 dash_direction = Vector2.down;

	private Modifier _mod_accel;

	private Modifier _mod_friction;

	private Modifier _mod_accel_always;

	private Modifier _mod_speed;

	public int idle_animation;

	private Vector2 _prev_pos = Vector2.zero;

	public AStarSearcher astar => _astar ?? (_astar = new AStarSearcher(this));

	public Transform following_target => target;

	public bool is_following_target => target != null;

	public float step => base.wgo.data.GetParam("speed") / 30f;

	public bool IsStopped => stopped;

	public MovementState movement_state => state;

	public float average_step => calcultated_average_step;

	public override void StartComponent()
	{
		if (!started)
		{
			base.StartComponent();
			current_pos = base.tf.position;
			CheckMovementParams();
			UpdateBodyPhysics();
		}
	}

	private void CheckMovementParams()
	{
		if (base.wgo == null)
		{
			Debug.LogError("WGO is null");
			return;
		}
		if (base.wgo.obj_def == null)
		{
			Debug.LogError("Obj def is null for WGO " + base.wgo.name, base.wgo);
			return;
		}
		if (!base.wgo.obj_def.res.Has("acceleration"))
		{
			base.wgo.obj_def.res.Set("acceleration", 1f);
		}
		if (!base.wgo.obj_def.res.Has("friction"))
		{
			base.wgo.obj_def.res.Set("friction", 0f);
		}
	}

	public override bool HasUpdate()
	{
		return true;
	}

	public override void UpdateComponent(float delta_time)
	{
		last_step = current_pos.DistTo(base.tf.position) / 96f;
		current_pos = base.tf.position;
		last_steps.Add(last_step);
		if (last_steps.Count > 5)
		{
			last_steps.RemoveAt(0);
		}
		walked_dist = 0f;
		foreach (float last_step in last_steps)
		{
			walked_dist += last_step;
		}
		calcultated_average_step = ((walked_dist > 0f) ? (walked_dist / (float)last_steps.Count) : 0f);
		if (state != 0 && MainGame.game_started && last_steps.Count == 5 && (walked_dist.EqualsTo(0f, 0.001f) || calcultated_average_step.EqualsTo(0f, 0.001f)) && state == MovementState.GoTo)
		{
			if (cur_astar_path == null)
			{
				return;
			}
			if (++_stuck_counter > 5)
			{
				Debug.LogWarning(base.wgo.name + " stucked, #path## fail", base.wgo);
				OnPathFailed();
			}
		}
		switch (state)
		{
		case MovementState.Following:
			UpdateFollowing(delta_time);
			break;
		case MovementState.GoToVector:
			UpdateGoToVector();
			break;
		case MovementState.None:
		case MovementState.GoTo:
			break;
		}
	}

	private void ChangeMovementState(MovementState new_state, bool reset_callbacks = true, bool force_state_change = false)
	{
		if (base.wgo.is_player)
		{
			Debug.Log("ChangeMovementState from = " + state.ToString() + " ==> " + new_state);
		}
		if (base.wgo.is_player && new_state == state && new_state == MovementState.AnimCurve && _curve_based_on_anim_timing)
		{
			base.components.animated_behaviour.on_loop -= OnCurveLoop;
		}
		if (new_state == state && !force_state_change)
		{
			return;
		}
		if (!started)
		{
			StartComponent();
		}
		last_steps.Clear();
		walked_dist = 0f;
		_stuck_counter = 0;
		if (new_state == MovementState.None)
		{
			if (movement_dir.magnitude > 0f)
			{
				SetMovementDir(Vector2.zero);
			}
			stopped = true;
			_ = base.wgo.is_player;
		}
		switch (state)
		{
		case MovementState.None:
			cur_astar_path = null;
			break;
		case MovementState.Following:
			idle_animation = 0;
			UpdateMovement(Vector2.zero, 0f);
			target = null;
			astar_following = false;
			break;
		case MovementState.GoTo:
			idle_animation = 0;
			astar.Clear();
			break;
		case MovementState.GoToVector:
			idle_animation = 0;
			goto_vector_dist = -1f;
			break;
		case MovementState.AnimCurve:
			idle_animation = 0;
			_prev_curve_pos = (_curve_delta = Vector2.zero);
			_curve_normalized_time = 0f;
			if (_curve_based_on_anim_timing)
			{
				base.components.animated_behaviour.RemoveCallbacks(OnCurveUpdate, OnCurveLoop);
			}
			else
			{
				base.components.timer.RemoveCallbacks(OnCurveUpdate, OnCurveLoop);
			}
			break;
		}
		if (reset_callbacks)
		{
			on_complete = (on_failed = null);
			event_on_complete = "";
		}
		state = new_state;
		if (base.wgo.obj_def != null && base.wgo.obj_def.IsNPC())
		{
			base.wgo.RedrawBubble();
		}
	}

	public bool IsInMovingState()
	{
		if (state == MovementState.None)
		{
			if (astar != null)
			{
				return astar.finding;
			}
			return false;
		}
		return true;
	}

	public override bool HasFixedUpdate()
	{
		return true;
	}

	public override void FixedUpdateComponent(float delta_time)
	{
		if (state == MovementState.Dash)
		{
			UpdateDash(delta_time);
		}
		else if (state == MovementState.AnimCurve)
		{
			CheckCurve();
			Vector2 vector = _curve_delta * _current_curve.Evaluate(_curve_normalized_time);
			Vector2 vector2 = vector - _prev_curve_pos;
			_prev_curve_pos = vector;
			base.body.MovePosition(base.body.position + vector2 * 96f);
		}
		else
		{
			if (state == MovementState.GoTo)
			{
				UpdatePathfinding(Time.fixedDeltaTime);
			}
			UpdateMovement(movement_dir, delta_time);
		}
	}

	private void UpdateDash(float delta_time)
	{
		if (!(dash_remaining_time < 0f))
		{
			if (!MainGame.me.player_char.control_enabled || GUIElements.me.craft.is_shown || GUIElements.me.body_craft.is_shown || GUIElements.me.mixed_craft.is_shown || GUIElements.me.resource_based_craft.is_shown)
			{
				dash_remaining_time = -0.1f;
				return;
			}
			Vector2 position = base.body.position;
			float num = 1.5f * delta_time / 0.1f * 96f;
			Vector2 vector = new Vector2(dash_direction.x * num, dash_direction.y * num * 0.8f);
			dash_remaining_time -= delta_time;
			base.body.MovePosition(position + vector);
		}
	}

	private void UpdateMovement(Vector2 dir, float delta_time)
	{
		if (base.wgo.is_dead)
		{
			return;
		}
		movement_dir = dir.normalized;
		if (movement_dir.magnitude.EqualsTo(0f) && delta_vec.magnitude.EqualsTo(0f))
		{
			if (max_speed_reached)
			{
				max_speed_reached = false;
			}
			return;
		}
		float num = base.wgo.data.GetParam("speed");
		if (num.EqualsTo(LazyConsts.PLAYER_SPEED))
		{
			num += base.wgo.data.GetParam("speed_buff");
		}
		if (stopped)
		{
			stopped = false;
		}
		bool flag = base.wgo.obj_def.accelerate_always;
		if (_mod_accel_always != null)
		{
			flag = flag || _mod_accel_always.value > 1f;
			_mod_accel_always.Update(delta_time);
		}
		float num2 = base.wgo.obj_def.acceleration;
		if (_mod_accel != null)
		{
			num2 += _mod_accel.value;
			_mod_accel.Update(delta_time);
		}
		float num3 = base.wgo.obj_def.friction;
		if (_mod_friction != null)
		{
			num3 += _mod_friction.value;
			_mod_friction.Update(delta_time);
		}
		if (_mod_speed != null)
		{
			num += _mod_speed.value;
			_mod_speed.Update(delta_time);
		}
		if (movement_dir.magnitude > 0f)
		{
			if (max_speed_reached && !flag)
			{
				delta_vec = movement_dir;
			}
			else
			{
				num2.EqualsTo(0f);
				delta_vec += num2 * movement_dir;
			}
		}
		else
		{
			delta_vec *= num3;
			if (Mathf.Abs(delta_vec.x) < 0.01f)
			{
				delta_vec.x = 0f;
			}
			if (Mathf.Abs(delta_vec.y) < 0.01f)
			{
				delta_vec.y = 0f;
			}
		}
		velocity = delta_vec.magnitude;
		if (velocity.EqualsTo(0f))
		{
			max_speed_reached = false;
			return;
		}
		if (velocity > 1f)
		{
			delta_vec *= 1f / velocity;
			velocity = delta_vec.magnitude;
			max_speed_reached = true;
		}
		UpdateBodyPhysics();
		base.wgo.round_and_sort.MarkPositionDirty();
		_ = state;
		_ = 2;
		_cur_move_dv = new Vector2(delta_vec.x, delta_vec.y * 0.8f) * num * 96f;
		base.body.MovePosition(base.body.position + _cur_move_dv * delta_time);
		if (!base.wgo.is_player)
		{
			base.wgo.GetComponent<RoundAndSortComponent>().MarkPositionDirty();
		}
	}

	private void UpdatePathfinding(float delta_time)
	{
		if (base.wgo.is_player && astar.finding)
		{
			return;
		}
		if (astar.finding && dir_before_path_finding.magnitude > 0f)
		{
			SetMovementDir(dir_before_path_finding);
		}
		if (cur_astar_path == null)
		{
			return;
		}
		if (path_waypoint >= cur_astar_path.Count)
		{
			OnCameToLastPoint(stop: false);
			return;
		}
		if (path_waypoint + 1 < cur_astar_path.Count && cur_astar_path[path_waypoint].z >= 1000f)
		{
			base.wgo.transform.position = cur_astar_path[path_waypoint + 1];
			base.wgo.RefreshPositionCache();
			path_waypoint += 2;
			if (path_waypoint >= cur_astar_path.Count)
			{
				OnCameToLastPoint(stop: false);
				return;
			}
		}
		Vector2 zero = Vector2.zero;
		Vector2 vector = (_prev_pos = base.body.position);
		while (true)
		{
			current_point_pos = cur_astar_path[path_waypoint];
			float num = _cur_move_dv.magnitude * delta_time * 0.6f;
			zero = current_point_pos - vector;
			if (zero.magnitude > 0.1f && zero.magnitude > num)
			{
				break;
			}
			if (++path_waypoint >= cur_astar_path.Count)
			{
				OnCameToLastPoint(stop: false);
				return;
			}
		}
		SetMovementDir(zero.normalized);
	}

	private void StoreSpeedBeforeWalking()
	{
		if (!_in_stored_speed_mode)
		{
			_in_stored_speed_mode = true;
			_stored_speed = base.wgo.data.GetParam("speed");
		}
	}

	private void RestoreSpeedAfterWalking()
	{
		if (_in_stored_speed_mode)
		{
			_in_stored_speed_mode = false;
			SetSpeed(_stored_speed);
		}
	}

	public void SetSpeed(float speed)
	{
		if (!(speed <= 0f))
		{
			base.wgo.data.SetParam("speed", speed);
		}
	}

	private void UpdateBodyPhysics()
	{
		if (base.body == null)
		{
			return;
		}
		if (base.wgo.is_player)
		{
			if (base.wgo.is_dead)
			{
				if (base.body.bodyType != RigidbodyType2D.Static)
				{
					base.body.bodyType = RigidbodyType2D.Static;
				}
				return;
			}
			if (MainGame.me.player_char.control_enabled)
			{
				if (base.body.bodyType != 0)
				{
					base.body.bodyType = RigidbodyType2D.Dynamic;
				}
				return;
			}
			switch (state)
			{
			case MovementState.None:
				if (base.body.bodyType != RigidbodyType2D.Static)
				{
					base.body.bodyType = RigidbodyType2D.Static;
				}
				break;
			case MovementState.Following:
			case MovementState.GoTo:
			case MovementState.GoToVector:
			case MovementState.AnimCurve:
				if (base.body.bodyType != RigidbodyType2D.Kinematic)
				{
					base.body.bodyType = RigidbodyType2D.Kinematic;
				}
				break;
			case MovementState.Dash:
				if (base.body.bodyType != 0)
				{
					base.body.bodyType = RigidbodyType2D.Dynamic;
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
		else
		{
			base.body.bodyType = ((!base.wgo.obj_def.dynamic_mob) ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic);
		}
	}

	public void StopImmediate()
	{
		if (base.body.bodyType != RigidbodyType2D.Static)
		{
			base.body.velocity = Vector2.zero;
		}
		max_speed_reached = false;
		velocity = 0f;
		delta_vec = Vector2.zero;
	}

	public MovementComponent CurveMoveTo(WorldGameObject wobj, AnimationCurve curve, float dist = 0f, GJCommons.VoidDelegate on_complete = null, bool based_on_anim_timing = true)
	{
		return CurveMoveTo(wobj.tf, curve, dist, on_complete, based_on_anim_timing);
	}

	public MovementComponent CurveMoveTo(Transform tf, AnimationCurve curve, float dist = 0f, GJCommons.VoidDelegate on_complete = null, bool based_on_anim_timing = true, bool set_active_now_because_of_movement = false)
	{
		Vector2 dir = base.tf.DirTo(tf);
		return CurveMove(dir, curve, dist, on_complete, based_on_anim_timing, "", force_state_change: false, is_dir_change: false, set_active_now_because_of_movement);
	}

	private void CheckCurve()
	{
		if (_current_curve == null)
		{
			Debug.LogError("Null curve, using linear for wgo " + base.wgo.obj_id, base.wgo);
			MovementCurve movementCurve = Resources.Load<MovementCurve>("Curves/linear");
			_current_curve = movementCurve.curve;
			if (_current_curve == null)
			{
				Debug.LogError("Coudln't load curve");
			}
		}
	}

	public MovementComponent CurveMove(Vector2 dir, AnimationCurve curve, float dist = 0f, GJCommons.VoidDelegate on_complete = null, bool based_on_anim_timing = true, string event_on_complete = "", bool force_state_change = false, bool is_dir_change = false, bool set_active_now_because_of_movement = false)
	{
		cur_astar_path = new List<Vector3>();
		_current_curve = curve;
		CheckCurve();
		ChangeMovementState(MovementState.AnimCurve, reset_callbacks: true, force_state_change);
		_curve_delta = (dist.EqualsTo(0f) ? dir : (dist * dir.normalized));
		_prev_curve_pos = Vector2.zero;
		this.on_complete = on_complete;
		this.event_on_complete = event_on_complete;
		_curve_mvmnt_start_frame = Time.frameCount;
		_curve_based_on_anim_timing = based_on_anim_timing;
		if (_curve_based_on_anim_timing)
		{
			base.components.animated_behaviour.SetCallbacks(OnCurveUpdate, OnCurveLoop);
		}
		else
		{
			base.components.timer.SetCallbacks(OnCurveUpdate, OnCurveLoop);
		}
		if (is_dir_change && !_curve_based_on_anim_timing)
		{
			SetMovementDir(dir);
		}
		stopped = false;
		UpdateBodyPhysics();
		if (set_active_now_because_of_movement)
		{
			ChunkedGameObject componentInChildren = base.wgo.GetComponentInChildren<ChunkedGameObject>();
			if (componentInChildren != null)
			{
				componentInChildren.active_now_because_of_movement = true;
			}
		}
		return this;
	}

	private void OnCurveUpdate(float normalized_time)
	{
		_curve_normalized_time = normalized_time;
	}

	private void OnCurveLoop()
	{
		if (base.wgo.is_player)
		{
			Debug.Log("OnCurveLoop, state = " + state.ToString() + ", fr = " + Time.frameCount + ", start fr = " + _curve_mvmnt_start_frame);
		}
		if (Time.frameCount - _curve_mvmnt_start_frame == 0)
		{
			return;
		}
		if (_curve_based_on_anim_timing)
		{
			base.components.animated_behaviour.on_loop -= OnCurveLoop;
		}
		if (state == MovementState.AnimCurve)
		{
			OnComplete();
			ChunkedGameObject componentInChildren = base.wgo.GetComponentInChildren<ChunkedGameObject>();
			if (componentInChildren != null)
			{
				componentInChildren.active_now_because_of_movement = false;
			}
		}
		ChangeMovementState(MovementState.None);
	}

	public void GoTo(GameObject dest, bool snap_to_node = false, GJCommons.VoidDelegate on_complete = null, GJCommons.VoidDelegate on_failed = null, bool with_cinematic = false, GoToMethod goto_method = GoToMethod.AStar, string event_on_complete = "", uint? filter_astar_area = null, bool from_script = false, GDPoint target_gdp = null)
	{
		GoTo(dest.transform.position, snap_to_node, on_complete, on_failed, with_cinematic, goto_method, event_on_complete, filter_astar_area, from_script, target_gdp);
	}

	public void GoTo(Vector2 dest, bool snap_to_node = false, GJCommons.VoidDelegate on_complete = null, GJCommons.VoidDelegate on_failed = null, bool with_cinematic = false, GoToMethod goto_method = GoToMethod.AStar, string event_on_complete = "", uint? filter_astar_area = null, bool from_script = false, GDPoint target_gd_point = null)
	{
		_target_gd_point_tag = ((target_gd_point == null) ? "" : target_gd_point.gd_tag);
		base.wgo.GetComponent<RoundAndSortComponent>().enabled = true;
		if (with_cinematic)
		{
			CameraTools.PlayCinematics(base.wgo, 0.5f, 100f);
		}
		if (!astar_following)
		{
			ChangeMovementState(MovementState.GoTo);
		}
		this.event_on_complete = event_on_complete;
		_using_gd_graph = goto_method == GoToMethod.GDGraph;
		StoreSpeedBeforeWalking();
		if (snap_to_node)
		{
			NNInfo nearest = AstarPath.active.GetNearest(dest, new PathNNConstraint());
			if (nearest.node != null)
			{
				dest = nearest.node.position.ToVector2();
			}
		}
		string name = base.wgo.name;
		Vector2 vector = dest;
		Debug.Log("GoTo " + name + ", dest = " + vector.ToString(), base.wgo);
		Debug.DrawLine(base.wgo.pos, dest, Color.yellow, 1f);
		astar.EnablePathSmoother(goto_method == GoToMethod.AStar);
		if (astar.destination.GridDistTo(dest) < 0.1f)
		{
			return;
		}
		switch (goto_method)
		{
		case GoToMethod.GDGraph:
		{
			if ((!filter_astar_area.HasValue || filter_astar_area.Value == 0) && target_gd_point != null)
			{
				try
				{
					if (target_gd_point.node != null)
					{
						filter_astar_area = target_gd_point.node.Area;
					}
				}
				catch (Exception ex)
				{
					Debug.LogError("Some problems with WGO [" + base.wgo.obj_id + "] target_gd_point: " + ex);
				}
				Debug.LogWarning("Set new filter_astar_area = " + (filter_astar_area.HasValue ? filter_astar_area.Value.ToString() : "null") + " for WGO [" + base.wgo.obj_id + "]", base.wgo);
			}
			Vector2 pos = base.wgo.pos;
			GDPoint gDPoint = null;
			float num2 = float.PositiveInfinity;
			foreach (GDPoint gd_point in WorldMap.gd_points)
			{
				if (!(gd_point == null) && gd_point.node != null && (!filter_astar_area.HasValue || gd_point.node.Area == filter_astar_area.Value))
				{
					float sqrMagnitude = (pos - (Vector2)gd_point.pos).sqrMagnitude;
					if (sqrMagnitude < num2)
					{
						gDPoint = gd_point;
						num2 = sqrMagnitude;
					}
				}
			}
			if (gDPoint == null)
			{
				Debug.LogError("Nearest GDPoint is null! filter_astar_area = " + ((!filter_astar_area.HasValue) ? "null" : filter_astar_area.Value.ToString()), base.wgo);
				{
					foreach (GDPoint gd_point2 in WorldMap.gd_points)
					{
						Debug.Log("GD point " + gd_point2.name + ", area = " + gd_point2.node.Area, gd_point2);
					}
					return;
				}
			}
			string[] obj = new string[6]
			{
				"Nearest GD point: ",
				gDPoint.name,
				", pos = ",
				gDPoint.transform.position.ToString(),
				", obj_pos = ",
				null
			};
			vector = pos;
			obj[5] = vector.ToString();
			Debug.Log(string.Concat(obj), gDPoint);
			cur_astar_path = null;
			_nearest_gd_point_pos = gDPoint.pos;
			astar.Find(gDPoint.transform.position, dest, OnGDPointsPathFound, OnPathFailed, 2);
			break;
		}
		case GoToMethod.AStar:
		{
			int num = (base.wgo.is_player ? 2 : 0);
			if (base.wgo.is_player)
			{
				AStarTools.RefreshPlayerGraph(base.wgo.pos, dest);
			}
			else
			{
				AStarTools.UpdateAstarBounds(base.wgo.pos, dest);
			}
			astar.Find(dest, OnPathFound, OnPathFailed, 1 << num);
			break;
		}
		case GoToMethod.Direct:
			astar.SetDest(dest);
			path_waypoint = 0;
			cur_astar_path = new List<Vector3>
			{
				base.wgo.pos,
				dest
			};
			ProceedToNextPathPoint();
			break;
		}
		if (!base.wgo.is_player)
		{
			dir_before_path_finding = movement_dir;
		}
		path_waypoint = 1;
		if (astar_following)
		{
			return;
		}
		if (base.wgo.is_player)
		{
			player_controlled_by_script = from_script;
		}
		if (with_cinematic)
		{
			this.on_complete = delegate
			{
				CameraTools.StopCinematics();
				RestoreSpeedAfterWalking();
				OnComplete();
			};
		}
		else
		{
			this.on_complete = delegate
			{
				RestoreSpeedAfterWalking();
				if (on_complete != null)
				{
					if (base.wgo.is_player)
					{
						player_controlled_by_script = false;
					}
					GJCommons.VoidDelegate voidDelegate = on_complete;
					on_complete = null;
					voidDelegate();
				}
			};
		}
		this.on_failed = on_failed;
	}

	private void OnComplete()
	{
		if (base.wgo.is_player)
		{
			Debug.Log("Movement.OnComplete");
		}
		if (base.wgo.is_player)
		{
			player_controlled_by_script = false;
		}
		GJCommons.VoidDelegate callback = on_complete;
		on_complete = null;
		on_failed = null;
		string text = event_on_complete;
		event_on_complete = "";
		if (base.wgo.is_player && cur_astar_path != null && cur_astar_path.Count > 0)
		{
			Vector3 position = cur_astar_path[cur_astar_path.Count - 1];
			position.z = base.wgo.tf.position.z;
			base.wgo.tf.position = position;
			cur_astar_path = new List<Vector3>();
		}
		if (!string.IsNullOrEmpty(_target_gd_point_tag))
		{
			GDPoint gDPointByGDTag = WorldMap.GetGDPointByGDTag(_target_gd_point_tag);
			_target_gd_point_tag = string.Empty;
			if (gDPointByGDTag != null)
			{
				base.wgo.OnCameToGDPoint(gDPointByGDTag);
			}
		}
		if (base.wgo.obj_def != null && base.wgo.obj_def.IsNPC())
		{
			base.wgo.RedrawBubble();
		}
		callback.TryInvoke();
		if (!string.IsNullOrEmpty(text))
		{
			base.wgo.FireEvent(text);
		}
	}

	private void OnPathFound()
	{
		path_waypoint = 0;
		dir_before_path_finding = Vector2.zero;
		CacheCurAstarPath();
		ProceedToNextPathPoint();
	}

	private void OnGDPointsPathFound()
	{
		path_waypoint = 0;
		CacheCurAstarPath();
		cur_astar_path.Insert(0, _nearest_gd_point_pos);
		ProceedToNextPathPoint();
	}

	private void CacheCurAstarPath()
	{
		cur_astar_path = new List<Vector3>();
		if (astar.path == null || astar.path.Count == 0)
		{
			Debug.LogError("Can not cache astar path: Incorrect path! #path#", base.wgo);
			return;
		}
		foreach (Vector3 item in astar.path)
		{
			cur_astar_path.Add(item);
		}
	}

	private void OnPathFailed()
	{
		Debug.LogError("Failed pathfinding! [" + base.wgo.obj_id + "]", base.wgo);
		on_failed.TryInvoke();
		on_failed = null;
		ChangeMovementState(MovementState.None);
	}

	public void GoToVector(Vector2 dir, float dist, GJCommons.VoidDelegate on_complete = null, string event_on_complete = "")
	{
		ChangeMovementState(MovementState.GoToVector);
		goto_vector_dir = dir.normalized;
		goto_vector_dist = dist;
		this.on_complete = on_complete;
		this.event_on_complete = event_on_complete;
	}

	public void FollowTarget(WorldGameObject target, float min_follow_dist = -1f, GJCommons.VoidDelegate on_complete = null, bool with_cinematic = false, string event_on_complete = "")
	{
		FollowTarget(target.tf, min_follow_dist, on_complete, with_cinematic, event_on_complete);
	}

	public void FollowTarget(Transform target, float min_follow_dist = -1f, GJCommons.VoidDelegate on_complete = null, bool with_cinematic = false, string event_on_complete = "")
	{
		if (base.tf.DirTo(target).magnitude < this.min_follow_dist)
		{
			on_complete.TryInvoke();
			ChangeMovementState(MovementState.None);
			StopImmediate();
			return;
		}
		ChangeMovementState(MovementState.Following);
		this.target = target;
		this.min_follow_dist = min_follow_dist;
		this.on_complete = on_complete;
		this.event_on_complete = event_on_complete;
		follow_pathfinding_delay = (walls_check_delay = 0f);
		astar_following = true;
		if (with_cinematic)
		{
			CameraTools.PlayCinematics(base.wgo, 0.5f, float.MaxValue);
		}
	}

	public void StopFollowByDelay(float delay)
	{
		follow_delay = delay;
		if (follow_delay > 0f)
		{
			UpdateMovement(Vector2.zero, 0f);
		}
	}

	public void StopMovement()
	{
		ChangeMovementState(MovementState.None);
		StopImmediate();
	}

	public void StopTargetFollowing()
	{
		if (state != 0)
		{
			StopMovement();
		}
	}

	protected void SetMovementDir(Vector2 dir)
	{
		movement_dir = dir;
		if (on_move_dir != null)
		{
			on_move_dir(dir);
		}
	}

	private void UpdateGoToVector()
	{
		goto_vector_dist -= last_step;
		if (goto_vector_dist <= 0f)
		{
			OnComplete();
			ChangeMovementState(MovementState.None);
		}
		else
		{
			SetMovementDir(goto_vector_dir.normalized);
		}
	}

	private void UpdateFollowing(float delta_time)
	{
		if (!is_following_target)
		{
			return;
		}
		follow_delay -= delta_time;
		if (follow_delay > 0f)
		{
			return;
		}
		Vector2 vector = base.tf.DirTo(target);
		if (vector.magnitude < min_follow_dist)
		{
			OnComplete();
			ChangeMovementState(MovementState.None);
			StopImmediate();
			return;
		}
		walls_check_delay -= delta_time;
		if (walls_check_delay <= 0f)
		{
			RaycastHit2D[] array = Physics2D.RaycastAll(base.wgo.pos, vector.normalized, vector.magnitude * 96f, 1);
			no_walls_to_target = array.Length == 0;
			if (no_walls_to_target)
			{
				follow_pathfinding_delay = 0f;
			}
			walls_check_delay = 0.1f;
		}
		follow_pathfinding_delay -= delta_time;
		if (follow_pathfinding_delay <= 0f && !no_walls_to_target)
		{
			GoTo(target.position);
			follow_pathfinding_delay = 0.5f;
		}
		if (no_walls_to_target)
		{
			SetMovementDir(vector.normalized);
		}
		else if (astar.finding || astar.not_avaible)
		{
			SetMovementDir((dir_before_path_finding.magnitude > 0f) ? dir_before_path_finding : vector.normalized);
		}
		else
		{
			UpdatePathfinding(Time.deltaTime);
		}
	}

	private void ProceedToNextPathPoint(bool immediate_stop = true)
	{
		if (cur_astar_path != null && ++path_waypoint >= cur_astar_path.Count)
		{
			OnCameToLastPoint(immediate_stop);
		}
	}

	private void OnCameToLastPoint(bool stop = true)
	{
		if (!astar_following)
		{
			if (base.wgo.is_player)
			{
				base.wgo.tf.position = astar.destination;
			}
			ChangeMovementState(MovementState.None, reset_callbacks: false);
			if (stop || base.wgo.is_player)
			{
				StopImmediate();
			}
		}
		astar.Clear();
		OnComplete();
	}

	public SerializableWGO.SerializebleMovementComponent GetSerializedMovementComponent()
	{
		SerializableWGO.SerializebleMovementComponent result = default(SerializableWGO.SerializebleMovementComponent);
		result.avaliable = base.enabled;
		result.cur_astar_path = JsonUtilityHelper.ToJsonList(cur_astar_path);
		result.path_waypoint = path_waypoint;
		result.state = state;
		result.event_on_complete = event_on_complete;
		result.anchor_gd_tag = base.components.character.anchor_obj_gd_point_tag;
		result.anchor_custom_tag = base.components.character.anchor_obj_wgo_custom_tag;
		result.anchor_is_wgo = base.components.character.anchor_is_wgo;
		result.using_gd_path = _using_gd_graph;
		result.idle_animation = idle_animation;
		result.target_gd_point_tag = _target_gd_point_tag;
		result.astar_dest = ((_astar == null) ? Vector2.zero : _astar.destination);
		result.current_point_pos = current_point_pos;
		result.current_pos = current_pos;
		result.stored_speed = _stored_speed;
		result.in_stored_speed_mode = _in_stored_speed_mode;
		return result;
	}

	public void DeserializeMovementComponent(SerializableWGO.SerializebleMovementComponent data)
	{
		if (data.state != 0)
		{
			Debug.Log("Deserializing moving object: " + data.state, base.wgo);
		}
		base.enabled = data.avaliable;
		cur_astar_path = JsonUtilityHelper.FromJsonList<Vector3>(data.cur_astar_path);
		path_waypoint = data.path_waypoint;
		state = data.state;
		event_on_complete = data.event_on_complete;
		base.components.character.anchor_obj_gd_point_tag = data.anchor_gd_tag;
		base.components.character.anchor_obj_wgo_custom_tag = data.anchor_custom_tag;
		base.components.character.anchor_is_wgo = data.anchor_is_wgo;
		_using_gd_graph = data.using_gd_path;
		idle_animation = data.idle_animation;
		_target_gd_point_tag = data.target_gd_point_tag;
		if (!data.astar_dest.sqrMagnitude.EqualsTo(0f))
		{
			astar.RestoreSerialized(data.astar_dest);
		}
		current_point_pos = data.current_point_pos;
		current_pos = data.current_pos;
		_stored_speed = data.stored_speed;
		_in_stored_speed_mode = data.in_stored_speed_mode;
	}

	public void SetMovementModifiers(Modifier acceleration, Modifier friction, Modifier accel_always, Modifier speed)
	{
		_mod_accel = acceleration;
		_mod_friction = friction;
		_mod_accel_always = accel_always;
		_mod_speed = speed;
	}

	public void Unstuck()
	{
		state = MovementState.GoTo;
	}
}
