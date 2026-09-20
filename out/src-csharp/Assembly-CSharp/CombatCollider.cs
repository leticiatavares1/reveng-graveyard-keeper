using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CombatCollider : MonoBehaviour
{
	private const int MAX_SKIP = 20;

	private List<Collider2D> _skip_colliders = new List<Collider2D>();

	private List<Collider2D> _hit_colliders = new List<Collider2D>();

	public CombatComponent combat_component;

	public ObjectDefinition.DamageType damage;

	public bool rotate_collider;

	public bool do_not_round_rotation;

	private long _cc_id;

	private Transform _collider_tf;

	private Vector3 _collider_rotation;

	private bool _started;

	public void StartComponent(CombatComponent cc)
	{
		combat_component = cc;
		_cc_id = cc.GetInstanceID();
		_collider_tf = base.transform;
		_started = true;
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		OnCollision(other);
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		OnCollision(other);
	}

	private void OnCollision(Collider2D other)
	{
		if (combat_component == null || _skip_colliders.Contains(other) || _hit_colliders.Contains(other))
		{
			return;
		}
		if (other.GetComponent<CombatCollider>() != null)
		{
			_skip_colliders.Add(other);
			return;
		}
		WorldGameObject componentInParent = other.GetComponentInParent<WorldGameObject>();
		if (componentInParent == null)
		{
			return;
		}
		CombatComponent combat = componentInParent.components.combat;
		if (combat == null || _cc_id == combat.GetInstanceID() || combat.wgo.is_player == combat_component.wgo.is_player)
		{
			_skip_colliders.Add(other);
			if (_skip_colliders.Count > 20)
			{
				_skip_colliders.RemoveAt(0);
			}
		}
		else if (combat_component.HitOther(combat, damage) && !_hit_colliders.Contains(other))
		{
			_hit_colliders.Add(other);
		}
	}

	public void RotateCollider(float dir_angle)
	{
		if (do_not_round_rotation)
		{
			_collider_rotation.z = (int)dir_angle + 90;
		}
		else
		{
			_collider_rotation.z = Mathf.RoundToInt((dir_angle + 90f) / 90f) * 90;
		}
		_collider_tf.eulerAngles = _collider_rotation;
	}

	public void OnEnable()
	{
		if (_started)
		{
			if (_hit_colliders.Count > 0)
			{
				_hit_colliders.Clear();
			}
			if (combat_component.active_combat_colliders.Count == 0)
			{
				combat_component.StartCombat();
			}
			combat_component.active_combat_colliders.Add(this);
		}
	}

	public void OnDisable()
	{
		if (_started && combat_component.active_combat_colliders.Contains(this))
		{
			combat_component.active_combat_colliders.Remove(this);
		}
	}
}
