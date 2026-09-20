using System;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileObject : MonoBehaviour
{
	public WorldGameObject father;

	public string id;

	public ProjectileDefinition definition;

	public ProjectileObjectPart pop;

	public Vector2 direction;

	private Transform _tf;

	private bool _is_wrong;

	private bool _waiting_for_animation_finished;

	private float _dist_to_cover;

	private float _time_to_live;

	private bool _has_max_living_time;

	private bool _update_movement;

	public int damaged_objs;

	public static ProjectileObject Create(string id, Transform parent, Vector2 pos, Vector2 direction, WorldGameObject father, List<Collider2D> skip_colliders = null)
	{
		ProjectileObject projectileObject = UnityEngine.Object.Instantiate(Prefabs.projectile_prefab, parent);
		projectileObject.gameObject.name = id;
		projectileObject.transform.position = pos;
		projectileObject.direction = direction.normalized;
		projectileObject.father = father;
		string[] obj = new string[9] { "Creating projectile: id=", id, "; pos=", null, null, null, null, null, null };
		Vector2 vector = pos;
		obj[3] = vector.ToString();
		obj[4] = "; direction=";
		vector = direction;
		obj[5] = vector.ToString();
		obj[6] = "; father=";
		obj[7] = ((father == null) ? "null" : father.obj_id);
		obj[8] = ".";
		Debug.Log(string.Concat(obj), projectileObject);
		projectileObject.SetID(id);
		if (skip_colliders != null && skip_colliders.Count != 0 && projectileObject.pop.collider_controller != null)
		{
			projectileObject.pop.collider_controller.AddSkipColliders(skip_colliders);
		}
		projectileObject.gameObject.SetActive(value: true);
		return projectileObject;
	}

	public void SetID(string projectile_id)
	{
		_is_wrong = true;
		if (string.IsNullOrEmpty(projectile_id))
		{
			Debug.LogError("Can not SetProjectileID: id is null!");
			return;
		}
		id = projectile_id;
		definition = GameBalance.me.GetData<ProjectileDefinition>(id);
		if (definition == null)
		{
			Debug.LogError("Can not SetProjectileID: definition is null!");
			return;
		}
		_dist_to_cover = definition.GetMaxDist();
		_time_to_live = definition.max_time;
		_has_max_living_time = !_time_to_live.EqualsTo(0f, 0.1f);
		damaged_objs = 0;
		ProjectileObjectPart projectileObjectPart = ProjectileObjectPart.Load(definition.prefab_name);
		if (projectileObjectPart == null)
		{
			Debug.LogError("Can not SetProjectileID: prefab is null!");
			return;
		}
		_tf = base.transform;
		pop = UnityEngine.Object.Instantiate(projectileObjectPart, _tf);
		pop.Init();
		pop.collider_controller.father = this;
		_is_wrong = false;
		_update_movement = false;
		_waiting_for_animation_finished = false;
		OnStart();
	}

	private void DoDestroy()
	{
		_is_wrong = true;
		ChunkedGameObject component = GetComponent<ChunkedGameObject>();
		if (component != null)
		{
			Debug.LogWarning("Projectile \"" + id + "\" has ChunkedGameObject.");
			ChunkManager.OnDestroyObject(component);
			component.destroyed = true;
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public void Update()
	{
		if (!_is_wrong && !_waiting_for_animation_finished)
		{
			if (_dist_to_cover < 0f)
			{
				OnMaxDistReached();
			}
			else if (_has_max_living_time && _time_to_live < 0f)
			{
				OnMaxDistReached();
			}
			else if (_update_movement)
			{
				Vector2 vector = Time.deltaTime * definition.speed * direction;
				_tf.position = (Vector2)_tf.position + 96f * vector;
				_dist_to_cover -= vector.magnitude;
				_time_to_live -= Time.deltaTime;
			}
		}
	}

	public void OnHitCombat(CombatComponent combat)
	{
		if (!combat.components.hp.enabled || combat.wgo.is_dead)
		{
			Debug.LogError("Can not hit to " + combat.wgo.obj_id + " hp disabled or wgo is dead", combat.wgo);
			return;
		}
		combat.components.hp.DecHP(definition.damage);
		Vector2 normalized = ((Vector2)_tf.position - combat.wgo.pos).normalized;
		float damage_direction = Mathf.Atan2(normalized.y, normalized.x) * 57.29578f;
		if (combat.components.character.enabled)
		{
			combat.components.character.OnWasDamaged(damage_direction);
		}
		damaged_objs++;
		bool do_destroy = definition.pierce > 0 && damaged_objs >= definition.pierce;
		Action dlg = delegate
		{
			if (do_destroy)
			{
				Debug.Log("Destroy projectile \"" + id + "\": max damaged objs reached: " + damaged_objs + ">=" + definition.pierce);
				DoDestroy();
			}
		};
		pop.on_hit_combat = definition.on_hit_combat.Invoke(_tf.position, pop.animator, dlg, combat.wgo, out _waiting_for_animation_finished);
	}

	public void OnStart()
	{
		pop.on_start = definition.on_start.Invoke(_tf.position, pop.animator, delegate
		{
			_update_movement = true;
		}, null, out _waiting_for_animation_finished);
	}

	public void OnHitNonCombat(WorldGameObject wgo)
	{
		if (wgo != null)
		{
			if (wgo.components.hp.enabled)
			{
				wgo.components.hp.DecHP(definition.damage);
			}
			Debug.Log("Destroy projectile \"" + id + "\": hit non-combat obj \"" + wgo.gameObject.name + "\"[" + wgo.obj_id + "]", wgo);
		}
		else
		{
			Debug.Log("Destroy projectile \"" + id + "\": hit non-wgo obj");
		}
		pop.on_hit_non_combat = definition.on_hit_non_combat.Invoke(_tf.position, pop.animator, DoDestroy, wgo, out _waiting_for_animation_finished);
	}

	public void OnOutOfScreen()
	{
		Debug.Log("Destroy projectile: out of screen");
		pop.on_out_of_screen = definition.on_out_of_screen.Invoke(_tf.position, pop.animator, DoDestroy, null, out _waiting_for_animation_finished);
	}

	public void OnMaxDistReached()
	{
		Debug.Log("Destroy projectile: max dist reached");
		pop.on_max_dist_reached = definition.on_max_dist_reached.Invoke(_tf.position, pop.animator, DoDestroy, null, out _waiting_for_animation_finished);
	}
}
