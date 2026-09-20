using System;
using System.Collections.Generic;
using LinqTools;
using UnityEngine;

public class ProjectileEmitter : MonoBehaviour
{
	public delegate void ShootingEnded(bool succeed);

	public enum ProjectileEmitterType
	{
		Single,
		Around
	}

	public const string SHOOT_TRIGGER = "do_shot";

	public bool is_paused;

	public string projectile_name;

	public Direction direction;

	public Vector2 direction_vector;

	[Range(0f, 45f)]
	public float direction_delta;

	public GameObject throwing_position_go;

	public float period;

	public float period_delta;

	public float projectile_spawn_delay;

	public WorldGameObject father_wgo;

	public Animator animator;

	public Transform target_tf;

	public ShootingEnded on_anim_end;

	public ProjectileEmitterType type;

	public List<Vector2> directions;

	private bool _has_period;

	private Transform _throwing_position_tf;

	private Transform _parent_tf;

	private float _time_left_for_shot;

	private Collider2D[] _self_colliders;

	private bool _has_shoot_trigger;

	private float _spawn_projectile_after;

	private bool _waiting_for_delay;

	public bool has_period => _has_period;

	public Transform throwing_pos_tf => _throwing_position_tf;

	public void Start()
	{
		father_wgo = GetComponentInParent<WorldGameObject>();
		_has_period = period > 0.01f;
		is_paused = true;
		switch (direction)
		{
		case Direction.None:
			direction_vector = direction_vector.normalized;
			break;
		case Direction.Right:
		case Direction.Up:
		case Direction.Left:
		case Direction.Down:
			direction_vector = direction.ToVec();
			break;
		case Direction.IgnoreDirection:
		case Direction.ToPlayer:
			Debug.LogError("Wrong direction: " + direction);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		_throwing_position_tf = ((throwing_position_go == null) ? base.transform : throwing_position_go.transform);
		_time_left_for_shot = 0f;
		if (father_wgo != null)
		{
			_parent_tf = father_wgo.tf.parent;
			if (animator == null)
			{
				animator = father_wgo.wop.GetComponent<Animator>();
			}
			_self_colliders = father_wgo.wop.GetComponentsInChildren<Collider2D>(includeInactive: true);
		}
		else
		{
			WorldSimpleObject componentInParent = GetComponentInParent<WorldSimpleObject>();
			if (componentInParent != null)
			{
				_parent_tf = componentInParent.transform.parent;
				if (animator == null)
				{
					animator = componentInParent.GetComponent<Animator>();
				}
				_self_colliders = componentInParent.GetComponentsInChildren<Collider2D>(includeInactive: true);
			}
			else
			{
				ChunkedGameObject componentInParent2 = GetComponentInParent<ChunkedGameObject>();
				if (componentInParent2 != null)
				{
					_parent_tf = componentInParent2.transform.parent;
					if (animator == null)
					{
						animator = componentInParent2.GetComponent<Animator>();
					}
					_self_colliders = componentInParent2.GetComponentsInChildren<Collider2D>(includeInactive: true);
				}
				else
				{
					_parent_tf = base.transform;
					if (animator == null)
					{
						animator = GetComponent<Animator>();
					}
					_self_colliders = GetComponentsInChildren<Collider2D>(includeInactive: true);
				}
			}
		}
		_has_shoot_trigger = false;
		if (!(animator != null))
		{
			return;
		}
		AnimatorControllerParameter[] parameters = animator.parameters;
		for (int i = 0; i < parameters.Length; i++)
		{
			if (parameters[i].name == "do_shot")
			{
				_has_shoot_trigger = true;
				break;
			}
		}
	}

	public void Update()
	{
		if (_waiting_for_delay && _spawn_projectile_after > 0f)
		{
			_spawn_projectile_after -= Time.deltaTime;
			if (_spawn_projectile_after < 0f)
			{
				_waiting_for_delay = false;
				SpawnProjectile();
			}
		}
		if (!_has_period || is_paused)
		{
			return;
		}
		_time_left_for_shot -= Time.deltaTime;
		if (!(_time_left_for_shot > 0f))
		{
			DoShot();
			_time_left_for_shot = period;
			if (!period_delta.EqualsTo(0f, 0.01f))
			{
				_time_left_for_shot += UnityEngine.Random.Range(0f - period_delta, period_delta);
			}
		}
	}

	public void DoShot(ShootingEnded anim_end = null)
	{
		if (_has_shoot_trigger)
		{
			animator.SetTrigger("do_shot");
		}
		if (projectile_spawn_delay.EqualsTo(0f))
		{
			SpawnProjectile();
		}
		else
		{
			_waiting_for_delay = true;
			_spawn_projectile_after = projectile_spawn_delay;
		}
		on_anim_end = anim_end;
	}

	private void SpawnProjectile()
	{
		switch (type)
		{
		case ProjectileEmitterType.Single:
			ProjectileObject.Create(projectile_name, _parent_tf, _throwing_position_tf.position, GetThrowingDirection(), father_wgo, _self_colliders.ToList());
			target_tf = null;
			break;
		case ProjectileEmitterType.Around:
			if (directions == null || directions.Count == 0)
			{
				Debug.LogError("Can not spawn projectile: directions list is null or empty.");
				break;
			}
			{
				foreach (Vector2 direction in directions)
				{
					if (direction.magnitude < 0.5f)
					{
						Vector2 vector = direction;
						Debug.LogWarning("ProjectileEmitter skip direction " + vector.ToString(), this);
					}
					else
					{
						ProjectileObject.Create(projectile_name, _parent_tf, _throwing_position_tf.position, direction, father_wgo, _self_colliders.ToList());
					}
				}
				break;
			}
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public void DoShot(Vector2 dir, ShootingEnded anim_end = null)
	{
		type = ProjectileEmitterType.Single;
		direction_vector = dir.normalized;
		direction_delta = 0f;
		DoShot(anim_end);
	}

	public void DoShot(Transform target, ShootingEnded anim_end = null)
	{
		type = ProjectileEmitterType.Single;
		target_tf = target;
		DoShot(anim_end);
	}

	public void DoShots(List<Vector2> dirs, ShootingEnded anim_end = null)
	{
		if (dirs == null || dirs.Count == 0)
		{
			Debug.LogError("Can not DoShots: dirs is nuil or empty!", this);
			return;
		}
		type = ProjectileEmitterType.Around;
		directions = dirs;
		DoShot(anim_end);
	}

	private Vector2 GetThrowingDirection()
	{
		if (target_tf != null)
		{
			return _throwing_position_tf.DirTo(target_tf);
		}
		if (direction_delta.EqualsTo(0f, 0.1f))
		{
			return direction_vector;
		}
		float z = UnityEngine.Random.Range(0f - direction_delta, direction_delta);
		Vector2 vector = Quaternion.Euler(0f, 0f, z) * direction_vector;
		string[] obj = new string[6] { "Projectile rand direction: direction=", null, null, null, null, null };
		Vector2 vector2 = direction_vector;
		obj[1] = vector2.ToString();
		obj[2] = ", degree=";
		obj[3] = z.ToString();
		obj[4] = ", rand_direction=";
		vector2 = vector;
		obj[5] = vector2.ToString();
		Debug.Log(string.Concat(obj));
		return vector;
	}
}
