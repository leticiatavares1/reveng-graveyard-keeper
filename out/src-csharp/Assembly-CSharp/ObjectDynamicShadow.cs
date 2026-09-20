using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[ExecuteInEditMode]
public class ObjectDynamicShadow : ObjectDynamicShadowChild
{
	public const bool CREATE_SHADOWS_ON_A_FLY = false;

	private const int LIGHTS_ARRAY_SIZE = 20;

	private List<ObjectDynamicShadowChild> _child_shadows = new List<ObjectDynamicShadowChild>();

	private DynamicLight[] _lights = new DynamicLight[20];

	private float[] _distances = new float[20];

	private int _lights_count;

	public Transform light_point;

	private bool _light_point_is_null = true;

	private Vector2?[] _lposes = new Vector2?[4];

	private float[] _ldist = new float[4];

	private Vector2 _last_pos = new Vector2(99999f, 99999f);

	[NonSerialized]
	public Vector3 point_pos = Vector3.zero;

	private bool _shadows_initialized;

	private bool _shadows_init_was_force_immidiate;

	private bool _shadows_created;

	private bool _shadows_really_created;

	private int _parent_go_instance_id = -1;

	private bool _parent_go_set;

	private Action _queued_shadows_action;

	private bool _destroyed;

	public int ParentGoInstanceIDInstanceID
	{
		get
		{
			if (!_parent_go_set)
			{
				_parent_go_set = true;
				WorldGameObject componentInParent = GetComponentInParent<WorldGameObject>();
				if (componentInParent != null)
				{
					_parent_go_instance_id = componentInParent.gameObject.GetInstanceID();
				}
				else
				{
					WorldSimpleObject componentInParent2 = GetComponentInParent<WorldSimpleObject>();
					if (componentInParent2 != null)
					{
						_parent_go_instance_id = componentInParent2.gameObject.GetInstanceID();
					}
				}
			}
			return _parent_go_instance_id;
		}
	}

	public void Start()
	{
		_light_point_is_null = light_point == null;
		_lights_count = 0;
	}

	public void Update()
	{
		if (!Application.isPlaying || visible)
		{
			point_pos = (_light_point_is_null ? base.transform.position : light_point.position);
			Vector2 pos = (_last_pos = Recalculate());
			for (int i = 0; i < _child_shadows.Count; i++)
			{
				ObjectDynamicShadowChild objectDynamicShadowChild = _child_shadows[i];
				objectDynamicShadowChild.SetShadowByNumber(this, objectDynamicShadowChild.shadow_n, pos, is_mirrored);
			}
		}
	}

	public Vector2? GetLight(int n)
	{
		return _lposes[n];
	}

	private void SortLightsList()
	{
		for (int i = 0; i < 4; i++)
		{
			_lposes[i] = null;
			_ldist[i] = float.MaxValue;
		}
		for (int j = 0; j < _lights_count && j <= 20; j++)
		{
			if (_lights[j] == null)
			{
				continue;
			}
			DynamicLight dynamicLight = _lights[j];
			if (!Application.isPlaying)
			{
				dynamicLight.pos = dynamicLight.transform.position;
			}
			Vector2 vector = dynamicLight.pos;
			if (!Application.isPlaying)
			{
				if (_distances == null || j >= _distances.Length)
				{
					break;
				}
				_distances[j] = (vector - (Vector2)point_pos).sqrMagnitude;
			}
			float num = _distances[j] / dynamicLight.intensity_k;
			if (num > 1000000f)
			{
				continue;
			}
			for (int k = 0; k < 4; k++)
			{
				if (!(num > _ldist[k]))
				{
					for (int num2 = 3; num2 > k; num2--)
					{
						_ldist[num2] = _ldist[num2 - 1];
						_lposes[num2] = _lposes[num2 - 1];
					}
					_ldist[k] = num;
					_lposes[k] = vector;
					break;
				}
			}
		}
	}

	public void ClearLightsList()
	{
		_lights_count = 0;
	}

	public void LateUpdate()
	{
		bool flag = is_mirrored;
		is_mirrored = base.transform.lossyScale.x < 0f;
		if (is_mirrored != flag)
		{
			Update();
		}
	}

	private Vector2 Recalculate()
	{
		if (Application.isPlaying && !visible)
		{
			return Vector2.zero;
		}
		if (!Application.isPlaying)
		{
			point_pos = ((light_point == null) ? base.transform.position : light_point.position);
			if (DynamicShadows.me != null)
			{
				_lights = DynamicShadows.me.lights.ToArray();
				_lights_count = _lights.Length;
			}
		}
		SortLightsList();
		return point_pos;
	}

	public static void InstantiateAllAdditionalShadows(int total_n = 4)
	{
	}

	public void InstantiateAdditionalShadows(int total_n, bool force_immediate = false)
	{
		if (force_immediate || !Application.isPlaying)
		{
			ReallyInstantiateAdditionalShadows(total_n);
		}
		else if (!_shadows_created && _queued_shadows_action == null)
		{
			_queued_shadows_action = delegate
			{
				ReallyInstantiateAdditionalShadows(total_n);
			};
			ObjectDynamicShadowsManager.QueueShadowCreation(_queued_shadows_action, delegate
			{
				_queued_shadows_action = null;
			});
		}
	}

	private void ReallyInstantiateAdditionalShadows(int total_n)
	{
		if (_destroyed || base.gameObject == null || _shadows_really_created)
		{
			return;
		}
		_shadows_initialized = true;
		_shadows_created = true;
		_child_shadows.Clear();
		_lights_count = 0;
		_shadows_really_created = true;
		ObjectDynamicShadowChild[] componentsInChildren = base.gameObject.GetComponentsInChildren<ObjectDynamicShadowChild>(includeInactive: true);
		foreach (ObjectDynamicShadowChild objectDynamicShadowChild in componentsInChildren)
		{
			if (!(objectDynamicShadowChild is ObjectDynamicShadow))
			{
				objectDynamicShadowChild.transform.SetParent(null, worldPositionStays: false);
				NGUITools.Destroy(objectDynamicShadowChild.gameObject);
			}
		}
		SpriteRenderer component = GetComponent<SpriteRenderer>();
		for (int j = 0; j < total_n; j++)
		{
			ObjectDynamicShadowChild shadow = ShadowsPool.GetShadow();
			GameObject obj = shadow.gameObject;
			obj.name = "[dynamic shadow] #" + j;
			SpriteRenderer component2 = obj.GetComponent<SpriteRenderer>();
			component2.sprite = component.sprite;
			component2.sharedMaterial = component.sharedMaterial;
			component2.sortingLayerID = component.sortingLayerID;
			component2.color = new Color(1f, 1f, 1f, 0f);
			_child_shadows.Add(shadow);
			shadow.shadow_n = j;
			shadow.shadow_alpha = shadow_alpha;
			obj.transform.SetParent(base.transform, worldPositionStays: false);
			obj.transform.localScale = Vector3.one;
			obj.transform.localPosition = Vector3.zero;
		}
		GetComponent<SpriteRenderer>().enabled = false;
		base.gameObject.layer = 12;
		shadow_n = -1;
	}

	public void CheckLightsRange(List<DynamicLight> lights)
	{
		int num = 0;
		for (int i = 0; i < lights.Count; i++)
		{
			DynamicLight dynamicLight = lights[i];
			if (dynamicLight == null || !dynamicLight.active_in_hierarchy)
			{
				continue;
			}
			float num2 = dynamicLight.pos.x - point_pos.x;
			float num3 = dynamicLight.pos.y - point_pos.y;
			float num4 = num2 * num2 + num3 * num3;
			if (!(num4 > 1000000f) && !dynamicLight.DoesLightBelongsToTheSameObjectAsShadow(this))
			{
				_lights[num] = dynamicLight;
				_distances[num] = num4;
				num++;
				if (num >= 20)
				{
					break;
				}
			}
		}
		_lights_count = num;
	}

	public override void SetShadowSprite(Sprite spr)
	{
		base.SetShadowSprite(spr);
		foreach (ObjectDynamicShadowChild child_shadow in _child_shadows)
		{
			child_shadow.SetShadowSprite(spr);
		}
	}

	public void EnsureObjectHasShadows(bool force_immediate = false)
	{
		if (Application.isPlaying && (!_shadows_initialized || (force_immediate && !_shadows_init_was_force_immidiate && !_shadows_really_created)))
		{
			_shadows_initialized = true;
			_shadows_init_was_force_immidiate = force_immediate;
			if (shadow_n == 0)
			{
				InstantiateAdditionalShadows(4, force_immediate);
			}
		}
	}

	public void Awake()
	{
		if (Application.isPlaying && !_shadows_initialized)
		{
			EnsureObjectHasShadows();
		}
	}

	public void OnEnable()
	{
		if (Application.isPlaying && !_shadows_initialized)
		{
			EnsureObjectHasShadows();
		}
		DynamicLights.shadows.Add(this);
	}

	public void OnDisable()
	{
		DynamicLights.shadows.Remove(this);
	}

	private void OnBecameVisible()
	{
		if (!_shadows_created && Application.isPlaying)
		{
			if (_queued_shadows_action != null)
			{
				ObjectDynamicShadowsManager.ForceShadowAction(_queued_shadows_action);
			}
			else
			{
				ReallyInstantiateAdditionalShadows(4);
			}
		}
	}

	public static void InitOnGameStart()
	{
	}

	private void OnDestroy()
	{
		_destroyed = true;
	}
}
