using System.Collections.Generic;
using UnityEngine;

public class CombatComponent : WorldGameObjectComponent
{
	public CombatCollider[] combat_colliders;

	private Transform[] _collider_transforms;

	private Vector3[] _collider_local_poss;

	private List<long> _collided_ids = new List<long>();

	private int _colliders_count;

	public List<CombatCollider> active_combat_colliders = new List<CombatCollider>();

	public Transform[] collider_tf => _collider_transforms;

	public bool is_started => started;

	public override void StartComponent()
	{
		base.StartComponent();
		combat_colliders = base.wgo.GetComponentsInChildren<CombatCollider>(includeInactive: true);
		_colliders_count = combat_colliders.Length;
		if (combat_colliders != null && _colliders_count > 0)
		{
			_collider_transforms = new Transform[_colliders_count];
			_collider_local_poss = new Vector3[_colliders_count];
			for (int i = 0; i < combat_colliders.Length; i++)
			{
				combat_colliders[i].StartComponent(this);
				combat_colliders[i].enabled = true;
				_collider_transforms[i] = combat_colliders[i].transform;
				_collider_local_poss[i] = _collider_transforms[i].localPosition;
			}
		}
		active_combat_colliders = new List<CombatCollider>();
	}

	public void WasHitBy(CombatComponent other, ObjectDefinition.DamageType damage_type)
	{
		if (!base.components.hp.enabled || base.wgo.is_dead)
		{
			return;
		}
		base.components.hp.DecHP(other.wgo.GetDamage(damage_type));
		Vector2 normalized = (other.wgo.pos - base.wgo.pos).normalized;
		float damage_direction = Mathf.Atan2(normalized.y, normalized.x) * 57.29578f;
		if (base.components.character.enabled)
		{
			base.components.character.OnWasDamaged(damage_direction);
		}
		if (combat_colliders != null && base.wgo.hp <= 0f)
		{
			CombatCollider[] array = combat_colliders;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].gameObject.SetActive(value: false);
			}
		}
	}

	public void StartCombat()
	{
		if (_collided_ids.Count > 0)
		{
			_collided_ids.Clear();
		}
	}

	public override bool HasUpdate()
	{
		return true;
	}

	public override void UpdateComponent(float delta_time)
	{
		for (int i = 0; i < _colliders_count; i++)
		{
			if ((bool)_collider_transforms[i])
			{
				_collider_transforms[i].localPosition = _collider_local_poss[i];
			}
		}
	}

	public bool HitOther(CombatComponent other_combat_component, ObjectDefinition.DamageType damage_type)
	{
		if (!base.wgo.components.character.attack.performing_attack && active_combat_colliders.Count == 0)
		{
			return false;
		}
		if (_collided_ids.Contains(other_combat_component.GetInstanceID()))
		{
			return false;
		}
		if (base.components.character.is_following_target && other_combat_component.tf != base.components.character.following_target)
		{
			return false;
		}
		_collided_ids.Add(other_combat_component.GetInstanceID());
		other_combat_component.WasHitBy(this, damage_type);
		base.wgo.components.character.attack.SuccessAttack();
		return true;
	}

	protected override int GetExecutionOrder()
	{
		return 6;
	}

	public override void UpdateEnableState(ObjectDefinition.ObjType obj_type)
	{
		base.enabled = obj_type == ObjectDefinition.ObjType.Mob || base.wgo.is_player;
	}
}
