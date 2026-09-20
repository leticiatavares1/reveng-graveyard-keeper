using System;
using UnityEngine;

public class DockPoint : MonoBehaviour, IComparable<DockPoint>
{
	private const float DROP_OFFSET_FORWARD = 19.2f;

	private const float DROP_OFFSET_RIGHT = 24f;

	public const float EPS = 0.002f;

	private const int REACHABILITY_MASK = 1;

	public Direction action_dir;

	private Vector2 _direction_vec;

	private WorldGameObject _parent_wgo;

	private CraftComponent _craft;

	private bool _parent_cached;

	private BaseCharacterComponent _target;

	private Transform _tf;

	private bool _tf_cached;

	private float _dist_to_target = float.MaxValue;

	public bool can_place_worker = true;

	public bool is_busy => _target != null;

	public bool shouldnt_be_used { get; private set; }

	public Vector2 reach_dir { get; private set; }

	public bool reached { get; private set; }

	public bool just_rotate { get; private set; }

	public BaseCharacterComponent target => _target;

	public WorldGameObject parent_wgo
	{
		get
		{
			if (_parent_cached)
			{
				return _parent_wgo;
			}
			_parent_cached = true;
			_parent_wgo = GetComponentInParent<WorldGameObject>();
			_craft = ((_parent_wgo != null) ? _parent_wgo.components.craft : null);
			return _parent_wgo;
		}
	}

	public CraftComponent craft => _craft;

	public Transform tf
	{
		get
		{
			if (!_tf_cached)
			{
				return this.Cache<Transform>(out _tf, out _tf_cached, deep: false);
			}
			return _tf;
		}
	}

	public void StartDocks(WorldGameObject parent)
	{
		shouldnt_be_used = false;
		_parent_wgo = parent;
		_craft = _parent_wgo.components.craft;
		_parent_cached = true;
		if (action_dir == Direction.None)
		{
			ReFindActionDir();
		}
		_direction_vec = GetActionDir().ToVec();
	}

	public void ReFindActionDir()
	{
		if (_parent_wgo == null)
		{
			_parent_wgo = GetComponentInParent<WorldGameObject>();
			if (_parent_wgo != null)
			{
				_craft = _parent_wgo.components.craft;
			}
		}
		Transform transform = ((_parent_wgo != null) ? _parent_wgo.tf : GetComponentInParent<WorldObjectPart>().transform);
		action_dir = tf.DirTo(transform.transform).normalized.ToDirection();
	}

	public bool IsUnreachable(float player_radius)
	{
		Collider2D[] array = Physics2D.OverlapCircleAll(tf.position, player_radius, 1);
		foreach (Collider2D collider2D in array)
		{
			if (!(collider2D == null) && !collider2D.isTrigger)
			{
				WorldGameObject worldGameObject = collider2D.GetComponent<WorldGameObject>() ?? collider2D.GetComponentInParent<WorldGameObject>();
				if (worldGameObject == null || worldGameObject.GetInstanceID() != parent_wgo.GetInstanceID())
				{
					return true;
				}
			}
		}
		return false;
	}

	public void SetTarget(BaseCharacterComponent target)
	{
		_target = target;
		_dist_to_target = ((_target != null) ? CalcDistToTarget(_target) : float.MaxValue);
	}

	public void Reset(bool shouldnt_be_used)
	{
		this.shouldnt_be_used = shouldnt_be_used;
		if (_target != null)
		{
			_target.ResetDockPoints();
		}
	}

	public void UpdateOverlapedCollider()
	{
	}

	public float CalcDistToTarget(BaseCharacterComponent target = null)
	{
		if (target == null)
		{
			if (_target == null)
			{
				Debug.LogError("null target");
				return 0f;
			}
			target = _target;
		}
		_dist_to_target = tf.position.DistTo(target.tf.position + target.anim_direction.ToVec3() * 48f);
		return _dist_to_target;
	}

	public void CheckIfReached()
	{
		bool flag2 = (just_rotate = false);
		reached = flag2;
		Vector2 vector = _target.tf.DirTo(tf);
		_dist_to_target = vector.magnitude;
		if (_dist_to_target < _target.step || _dist_to_target.EqualsTo(0f, 0.002f))
		{
			reached = _dist_to_target.EqualsTo(0f, 0.002f) && GetActionDir() == _target.anim_direction;
			if (!reached)
			{
				just_rotate = true;
				reach_dir = _direction_vec;
			}
		}
		else
		{
			reach_dir = vector.normalized;
		}
	}

	public int CompareTo(DockPoint other)
	{
		if (_dist_to_target.EqualsTo(other._dist_to_target))
		{
			return 0;
		}
		if (!(_dist_to_target > other._dist_to_target))
		{
			return -1;
		}
		return 1;
	}

	public Vector3 GetDropPos()
	{
		Direction actionDir = GetActionDir();
		return (Vector2)(tf.position + actionDir.ToVec3() * 19.2f + actionDir.ClockwiseDir().ToVec3() * 24f);
	}

	public Direction GetActionDir()
	{
		Direction direction = action_dir;
		if ((direction == Direction.Left || direction == Direction.Right) && base.transform.lossyScale.x < 0f)
		{
			switch (direction)
			{
			case Direction.Left:
				direction = Direction.Right;
				break;
			case Direction.Right:
				direction = Direction.Left;
				break;
			}
		}
		return direction;
	}
}
