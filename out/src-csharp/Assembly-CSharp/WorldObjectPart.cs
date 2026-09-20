using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldObjectPart : MonoBehaviour
{
	public int floor_line;

	[SerializeField]
	private WorldGameObject _parent;

	[SerializeField]
	private MovementCurve _curve_idle;

	[SerializeField]
	private MovementCurve _curve_movement;

	[SerializeField]
	private MovementCurve _curve_attack;

	[SerializeField]
	private MovementCurve _curve_jump;

	[SerializeField]
	private VisibilitySector[] _visibility_sectors;

	private bool _cached;

	public List<GameObject> variations;

	public bool variations_are_radiobutton = true;

	public bool variation_can_be_none = true;

	public List<GOsList> variations_2;

	public bool variations_2_are_radiobutton = true;

	public bool variation_2_can_be_none = true;

	public string skin_id = "";

	public Transform center;

	public SpriteRenderer carrying_item_sprite;

	public WorldGameObject parent
	{
		get
		{
			if (!_cached)
			{
				Cache();
			}
			return _parent;
		}
	}

	public VisibilitySector[] visibility_sectors
	{
		get
		{
			if (_visibility_sectors == null || _visibility_sectors.Length == 0)
			{
				return CacheSectors();
			}
			return _visibility_sectors;
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

	private void Cache()
	{
		_parent = GetComponentInParent<WorldGameObject>();
		if (_parent == null)
		{
			Transform transform = base.transform.parent;
			if (transform != null)
			{
				_parent = transform.GetComponent<WorldGameObject>();
				if (_parent == null)
				{
					transform = transform.parent;
					if (transform != null)
					{
						_parent = transform.GetComponent<WorldGameObject>();
					}
				}
			}
		}
		_cached = _parent != null;
	}

	public VisibilitySector[] CacheSectors()
	{
		_visibility_sectors = GetComponentsInChildren<VisibilitySector>();
		if (parent != null)
		{
			VisibilitySector[] array = _visibility_sectors;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Init(parent.components.character);
			}
		}
		return _visibility_sectors;
	}

	public AnimationCurve GetCurve(CharAnimState anim_state)
	{
		return anim_state switch
		{
			CharAnimState.Idle => GetCurve("idle"), 
			CharAnimState.Walking => GetCurve("movement"), 
			CharAnimState.Attack => GetCurve("attack"), 
			CharAnimState.Jump => GetCurve("jump"), 
			_ => null, 
		};
	}

	public AnimationCurve GetCurve(string curve_name)
	{
		MovementCurve movementCurve = null;
		switch (curve_name)
		{
		case "idle":
			movementCurve = _curve_idle;
			break;
		case "movement":
			movementCurve = _curve_movement;
			break;
		case "attack":
			movementCurve = _curve_attack;
			break;
		case "jump":
			movementCurve = _curve_jump;
			break;
		}
		if (!(movementCurve == null))
		{
			return movementCurve.curve;
		}
		return null;
	}
}
