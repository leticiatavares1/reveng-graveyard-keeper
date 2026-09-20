using System;
using UnityEngine;

public class BaseCharacterAttack : WorldObjectPartComponent
{
	public delegate void AttackResult(bool success);

	protected const string ATTACK_TYPE = "attack_type";

	protected const string ATTACK_TYPE_F = "attack_type_f";

	[NonSerialized]
	protected bool performing;

	[NonSerialized]
	protected bool anim_based_timing;

	[NonSerialized]
	protected bool successed;

	[NonSerialized]
	protected bool using_item;

	[NonSerialized]
	protected AttackResult on_performed;

	[NonSerialized]
	protected float _stopped_time;

	[NonSerialized]
	protected int _callback_setting_frame;

	[NonSerialized]
	public int cur_attack_type = -1;

	private bool _collider_cached;

	private Transform _collider_tf;

	public bool performing_attack => performing;

	public override void StartComponent()
	{
		base.StartComponent();
		InitAnimator();
	}

	public override void UpdateComponent(float delta_time)
	{
		CombatComponent combat = base.components.combat;
		if (!combat.is_started)
		{
			return;
		}
		float dir_angle = base.components.character.dir_angle;
		CombatCollider[] combat_colliders = combat.combat_colliders;
		foreach (CombatCollider combatCollider in combat_colliders)
		{
			if (combatCollider.rotate_collider)
			{
				combatCollider.RotateCollider(dir_angle);
			}
		}
	}

	public void AttackAnimationEnded()
	{
		Stop(call_callback: true);
	}

	protected void OnItemLoop(ItemDefinition.ItemType item_type, bool flag)
	{
		base.components.animated_behaviour.on_item_loop -= OnItemLoop;
		if (!base.wgo.is_player || item_type == ItemDefinition.ItemType.Sword)
		{
			Stop(call_callback: true);
		}
	}

	protected void OnLoop()
	{
		if (anim_based_timing)
		{
			base.components.animated_behaviour.on_loop -= OnLoop;
		}
		else
		{
			base.components.timer.on_loop -= OnLoop;
		}
		Stop(call_callback: true);
	}

	protected void ClearAllLoopCallbacks()
	{
		if (using_item)
		{
			base.components.animated_behaviour.on_item_loop -= OnItemLoop;
		}
		else if (anim_based_timing)
		{
			base.components.animated_behaviour.on_loop -= OnLoop;
		}
		else
		{
			base.components.timer.on_loop -= OnLoop;
		}
	}

	protected void InitAnimator()
	{
		ForceRecache();
	}

	public virtual bool Perform(Direction dir, int type, AttackResult on_performed = null, bool anim_based_timing = true)
	{
		return Perform(type, on_performed, anim_based_timing);
	}

	public virtual bool Perform(WorldGameObject target, int type, AttackResult on_performed = null, bool anim_based_timing = true)
	{
		return Perform(type, on_performed, anim_based_timing);
	}

	public virtual bool Perform(int type, AttackResult on_performed = null, bool anim_based_timing = true)
	{
		if (performing)
		{
			return false;
		}
		base.components.combat.StartCombat();
		UpdateAttackState(is_attacking: true, type);
		performing = true;
		successed = false;
		this.on_performed = on_performed;
		_callback_setting_frame = Time.frameCount;
		this.anim_based_timing = anim_based_timing;
		using_item = anim_based_timing && base.wgo.GetEquippedWeaponType() != ItemDefinition.ItemType.None;
		if (anim_based_timing)
		{
			if (base.wgo.GetEquippedWeaponType() == ItemDefinition.ItemType.None)
			{
				base.components.animated_behaviour.on_loop += OnLoop;
			}
			else
			{
				base.components.animated_behaviour.on_item_loop += OnItemLoop;
			}
		}
		else
		{
			base.components.timer.on_loop += OnLoop;
		}
		Item item = (base.wgo.is_player ? base.wgo.GetEquippedWeapon() : null);
		if (item != null)
		{
			base.wgo.components.character.player.TrySpendEnergy(item.definition.params_on_use.Get("energy") * -1f);
		}
		return true;
	}

	protected virtual void Stop(bool call_callback)
	{
		if (performing)
		{
			_stopped_time = Time.time;
			performing = false;
			UpdateAttackState(is_attacking: false, 0);
			anim_based_timing = false;
			if (call_callback && on_performed != null)
			{
				on_performed(successed);
			}
			if (_callback_setting_frame != Time.frameCount)
			{
				on_performed = null;
			}
			successed = false;
		}
	}

	public virtual void InterruptAttack()
	{
		ClearAllLoopCallbacks();
		Stop(call_callback: false);
	}

	protected virtual void UpdateAttackState(bool is_attacking, int type)
	{
		bool flag = base.wgo.is_player || base.wgo.GetEquippedWeaponType() == ItemDefinition.ItemType.Sword;
		cur_attack_type = type;
		base.components.animator.SetFloat("attack_type_f", type);
		BaseCharacterComponent baseCharacterComponent = base.components.character;
		if (is_attacking)
		{
			if (flag)
			{
				baseCharacterComponent.SetAnimationState(CharAnimState.Tool, base.wgo.GetEquippedWeaponType());
			}
			else
			{
				baseCharacterComponent.SetAnimationState(CharAnimState.Attack);
			}
		}
		else if ((flag && base.components.character.anim_state == CharAnimState.Tool) || baseCharacterComponent.anim_state == CharAnimState.Attack)
		{
			baseCharacterComponent.SetAnimationState(CharAnimState.Idle);
		}
		if (!is_attacking)
		{
			base.components.animator.SetInteger("attack_type", 0);
		}
	}

	public virtual void OnFirstAttackStepDone()
	{
	}

	public virtual void SuccessAttack()
	{
		successed = true;
	}
}
