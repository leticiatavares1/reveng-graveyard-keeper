using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkedGameObject : MonoBehaviour
{
	public const bool RESCAN_ASTAR_ON_ENABLE = true;

	public int chunk_x;

	public int chunk_y;

	public int chunk_x_min;

	public int chunk_x_max;

	public int chunk_y_min;

	public int chunk_y_max;

	protected bool reset_transform_change;

	private bool _obj_really_visible = true;

	[NonSerialized]
	public bool obj_visible = true;

	public bool always_active;

	private bool _active_now_because_of_movement;

	private bool _active_now_because_of_events;

	private bool _active_now_because_of_work;

	[NonSerialized]
	public int instance_id = -1;

	private DynamicLight _dyn_light;

	private bool _dyn_light_cached;

	private const int CHUNK_SIZE = 96;

	private GraphicsObjectOptimizer _optimizer;

	private bool _optimized_inited;

	private bool _inited;

	private bool _inited_after_change_wgo;

	private Bounds _bounds;

	private float _bounds_x_plus;

	private float _bounds_y_plus;

	private float _bounds_x_minus;

	private float _bounds_y_minus;

	public float out_x_1;

	public float out_x_2;

	public float out_y_1;

	public float out_y_2;

	private bool _shadow_inited;

	private ObjectDynamicShadow _shadow;

	[NonSerialized]
	public bool destroyed;

	[NonSerialized]
	public bool destroy_instead_of_turnong_off;

	private bool _started;

	[NonSerialized]
	public bool is_temp;

	[NonSerialized]
	public bool pending_to_remove;

	public bool can_go_inactive
	{
		get
		{
			if (!always_active && !active_now_because_of_movement && !_active_now_because_of_events)
			{
				return !_active_now_because_of_work;
			}
			return false;
		}
	}

	public bool active_now_because_of_events
	{
		get
		{
			return _active_now_because_of_events;
		}
		set
		{
			_active_now_because_of_events = value;
			if (!base.gameObject.activeInHierarchy && _active_now_because_of_events)
			{
				base.gameObject.SetActive(value: true);
			}
		}
	}

	public bool active_now_because_of_movement
	{
		get
		{
			return _active_now_because_of_movement;
		}
		set
		{
			_active_now_because_of_movement = value;
			if (!base.gameObject.activeInHierarchy && _active_now_because_of_movement)
			{
				base.gameObject.SetActive(value: true);
			}
		}
	}

	public bool active_now_because_of_work
	{
		get
		{
			return _active_now_because_of_work;
		}
		set
		{
			_active_now_because_of_work = value;
			if (!base.gameObject.activeInHierarchy && _active_now_because_of_work)
			{
				base.gameObject.SetActive(value: true);
			}
		}
	}

	public DynamicLight dynamic_light
	{
		get
		{
			if (_dyn_light_cached)
			{
				return _dyn_light;
			}
			_dyn_light_cached = true;
			DynamicLight[] componentsInChildren = GetComponentsInChildren<DynamicLight>(includeInactive: true);
			if (componentsInChildren.Length != 0)
			{
				_dyn_light = componentsInChildren[0];
			}
			return _dyn_light;
		}
	}

	public Bounds bounds
	{
		get
		{
			if (!_inited)
			{
				Init();
			}
			return _bounds;
		}
	}

	public virtual void Update()
	{
		if (!MainGame.disable_all_game && base.transform.hasChanged)
		{
			reset_transform_change = true;
			RecalculateChunk();
		}
	}

	public void UpdateVisibility()
	{
		if (destroyed || pending_to_remove)
		{
			return;
		}
		try
		{
			if (base.gameObject == null)
			{
				destroyed = true;
			}
		}
		catch (Exception)
		{
			destroyed = true;
		}
		if (destroyed || _obj_really_visible == obj_visible)
		{
			return;
		}
		WorldGameObject component = GetComponent<WorldGameObject>();
		if (component != null && !obj_visible)
		{
			component.PreDisable();
		}
		base.gameObject.SetActive(obj_visible);
		_obj_really_visible = obj_visible;
		if (obj_visible && !_optimized_inited)
		{
			_optimized_inited = true;
			_optimizer = new GraphicsObjectOptimizer(base.gameObject);
			RescanAStar();
		}
		if (obj_visible && !_shadow_inited)
		{
			_shadow_inited = true;
			_shadow = GetComponentInChildren<ObjectDynamicShadow>(includeInactive: true);
			if (_shadow != null)
			{
				_shadow.EnsureObjectHasShadows(force_immediate: true);
			}
		}
		if (!obj_visible && destroy_instead_of_turnong_off)
		{
			ChunkManager.OnDestroyObject(this);
			destroyed = true;
			ProjectileObject component2 = base.gameObject.GetComponent<ProjectileObject>();
			if (component2 != null)
			{
				component2.OnOutOfScreen();
			}
			else
			{
				Debug.LogError("Found object, that can not destroy from chunk manager (because need instructions here)!");
			}
		}
	}

	public void RescanAStar()
	{
		bool flag = false;
		Bounds b = default(Bounds);
		OptimizedCollider2D[] componentsInChildren = GetComponentsInChildren<OptimizedCollider2D>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Init();
		}
		Collider2D[] componentsInChildren2 = GetComponentsInChildren<Collider2D>();
		foreach (Collider2D collider2D in componentsInChildren2)
		{
			if (collider2D.gameObject.layer == 0)
			{
				if (!flag)
				{
					b = collider2D.bounds;
					flag = true;
				}
				else
				{
					b.Encapsulate(collider2D.bounds);
				}
			}
		}
		b.Expand(Vector2.one / 6f);
		if (flag)
		{
			ChunkManager.RecalcAStarBounds(b);
		}
	}

	public virtual void LateUpdate()
	{
		if (reset_transform_change)
		{
			base.transform.hasChanged = false;
			reset_transform_change = false;
		}
	}

	public virtual void Start()
	{
		if (!_started)
		{
			_started = true;
			RecalculateChunk();
			if (Application.isPlaying)
			{
				ChunkManager.OnAddNewObject(this);
			}
		}
	}

	private static void GetChunk(Transform t, out int x, out int y)
	{
		Vector2 vector = t.position;
		x = Mathf.RoundToInt(vector.x / 96f);
		y = Mathf.RoundToInt(vector.y / 96f);
	}

	public void RecalculateChunk()
	{
		GetChunk(base.transform, out chunk_x, out chunk_y);
		chunk_x_min = chunk_x + Mathf.CeilToInt(_bounds_x_minus / 96f);
		chunk_x_max = chunk_x + Mathf.CeilToInt(_bounds_x_plus / 96f);
		chunk_y_min = chunk_y + Mathf.CeilToInt(_bounds_y_minus / 96f);
		chunk_y_max = chunk_y + Mathf.CeilToInt(_bounds_y_plus / 96f);
	}

	public virtual void OnDestroy()
	{
		if (Application.isPlaying)
		{
			ChunkManager.OnDestroyObject(this);
		}
	}

	public void OnJustSpawnedWGO()
	{
		if (can_go_inactive)
		{
			_obj_really_visible = (obj_visible = false);
		}
		else
		{
			_obj_really_visible = (obj_visible = true);
			base.gameObject.SetActive(value: true);
		}
		Start();
	}

	public void ResetAtTheBeginning()
	{
		_obj_really_visible = (obj_visible = base.gameObject.activeSelf);
	}

	public void Init(bool init_after_change_wgo = false)
	{
		if (!init_after_change_wgo)
		{
			if (_inited)
			{
				return;
			}
		}
		else
		{
			if (_inited_after_change_wgo)
			{
				return;
			}
			_inited_after_change_wgo = init_after_change_wgo;
		}
		WorldGameObject component = GetComponent<WorldGameObject>();
		if (component != null)
		{
			ObjectDefinition obj_def = component.obj_def;
			if (obj_def != null && obj_def.always_active)
			{
				always_active = true;
			}
		}
		_inited = true;
		Vector3 position = base.transform.position;
		_shadow_inited = false;
		_bounds = new Bounds(position, LazyConsts.GRID_SIZE_VECTOR2);
		Component[] componentsInChildren = GetComponentsInChildren<Component>(includeInactive: true);
		List<GameObject> list = new List<GameObject>();
		Component[] array = componentsInChildren;
		foreach (Component component2 in array)
		{
			ObjectDynamicShadowChild objectDynamicShadowChild = component2 as ObjectDynamicShadowChild;
			if (objectDynamicShadowChild != null)
			{
				list.Add(objectDynamicShadowChild.gameObject);
				continue;
			}
			Light light = component2 as Light;
			if (!(light == null) && light.isActiveAndEnabled)
			{
				Bounds bounds = new Bounds(light.transform.position, Vector3.one * light.range * 2f);
				_bounds.Encapsulate(bounds);
			}
		}
		array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			SpriteRenderer spriteRenderer = array[i] as SpriteRenderer;
			if (!(spriteRenderer == null) && !list.Contains(spriteRenderer.gameObject))
			{
				_bounds.Encapsulate(spriteRenderer.bounds);
			}
		}
		_bounds.center -= position;
		_bounds_x_plus = _bounds.center.x + _bounds.extents.x;
		_bounds_y_plus = _bounds.center.y + _bounds.extents.y;
		_bounds_x_minus = _bounds.center.x - _bounds.extents.x;
		_bounds_y_minus = _bounds.center.y - _bounds.extents.y;
		RecalculateChunk();
	}

	public void OnDrawGizmosSelected()
	{
		if (_inited && Application.isPlaying && !(GetComponent<PlayerComponent>() != null))
		{
			Vector3 position = base.transform.position;
			Gizmos.color = Color.blue;
			Gizmos.DrawWireCube(position + _bounds.center, new Vector3(_bounds.size.x, _bounds.size.y, 0f));
		}
	}

	public void OnEnable()
	{
		StartCoroutine(LateEnableCoroutine());
	}

	private IEnumerator LateEnableCoroutine()
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		LateEnable();
	}

	protected virtual void LateEnable()
	{
		OptimizedCollider2D[] componentsInChildren = base.gameObject.GetComponentsInChildren<OptimizedCollider2D>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Init();
		}
	}

	public SerializableWGO.SerializableChunk SerializeChunk()
	{
		SerializableWGO.SerializableChunk serializableChunk = default(SerializableWGO.SerializableChunk);
		serializableChunk.active_now_because_of_movement = _active_now_because_of_movement;
		serializableChunk.active_now_because_of_events = _active_now_because_of_events;
		serializableChunk.active_now_because_of_work = _active_now_because_of_work;
		serializableChunk.always_active = always_active;
		SerializableWGO.SerializableChunk result = serializableChunk;
		if (result.active_now_because_of_movement)
		{
			Debug.Log("TO: data.active_now_bacause_of_action " + base.gameObject.name, this);
		}
		return result;
	}

	public void DeserializeChunk(SerializableWGO data)
	{
		if (data.chunk.active_now_because_of_movement)
		{
			Debug.Log("FROM: data.active_now_bacause_of_action " + base.gameObject.name, this);
		}
		active_now_because_of_movement = data.chunk.active_now_because_of_movement;
		active_now_because_of_events = data.chunk.active_now_because_of_events;
		active_now_because_of_work = data.chunk.active_now_because_of_work;
		always_active = data.chunk.always_active;
	}
}
