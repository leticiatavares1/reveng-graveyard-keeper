using System;
using UnityEngine;

public class CurvedAttack : BaseCharacterAttack
{
	[Serializable]
	public struct CurvedAttackType
	{
		public float dist;

		public float time;

		public MovementCurve curve;

		public int sub_types;

		public ProjectileEmitter enable_projectile_emitter;
	}

	public CurvedAttackType[] types;

	public float max_delay_for_subtype;

	[NonSerialized]
	protected int _last_subtype;

	private ProjectileEmitter _last_enabled_emitter;

	public override bool Perform(WorldGameObject target, int type, AttackResult on_performed = null, bool anim_based_timing = true)
	{
		if (!CommonPerformStuff(type, on_performed))
		{
			return false;
		}
		base.components.character.CurveMoveTo(target, GetCurve(type), GetDist(type), null, IsAnimBasedTiming(type));
		return true;
	}

	public override bool Perform(Direction dir, int type, AttackResult on_performed = null, bool anim_based_timing = true)
	{
		if (!CommonPerformStuff(type, on_performed))
		{
			return false;
		}
		Vector2 dir2 = ((dir == Direction.IgnoreDirection) ? base.components.character.direction : dir.ToVec());
		base.components.character.CurveMove(dir2, GetCurve(type), GetDist(type), null, IsAnimBasedTiming(type));
		return true;
	}

	private bool CommonPerformStuff(int type, AttackResult on_performed = null)
	{
		if (!performing && !IsAnimBasedTiming(type))
		{
			base.components.timer.Play(GetTime(type));
		}
		if (types != null && types.Length > type && type >= 0 && types[type].enable_projectile_emitter != null && types[type].enable_projectile_emitter.is_paused && types[type].enable_projectile_emitter.has_period)
		{
			_last_enabled_emitter = types[type].enable_projectile_emitter;
			_last_enabled_emitter.is_paused = false;
		}
		return base.Perform(type, on_performed, IsAnimBasedTiming(type));
	}

	public override void OnFirstAttackStepDone()
	{
		base.components.character.StopMovement();
	}

	public override void InterruptAttack()
	{
		base.InterruptAttack();
		base.components.timer.ClearTimer();
		base.components.character.StopMovement();
		if (_last_enabled_emitter != null)
		{
			_last_enabled_emitter.is_paused = true;
			_last_enabled_emitter = null;
		}
	}

	protected override void Stop(bool call_callback)
	{
		base.Stop(call_callback);
		if (_last_enabled_emitter != null)
		{
			_last_enabled_emitter.is_paused = true;
			_last_enabled_emitter = null;
		}
	}

	protected override void UpdateAttackState(bool is_attacking, int type)
	{
		base.UpdateAttackState(is_attacking, type);
		if (!is_attacking)
		{
			base.components.animator.SetInteger("attack_type", 0);
			return;
		}
		if (Time.time - _stopped_time > max_delay_for_subtype)
		{
			_last_subtype = 0;
		}
		else if (type > types.Length)
		{
			_last_subtype = 0;
		}
		else if (++_last_subtype >= types[type].sub_types)
		{
			_last_subtype = 0;
		}
		base.components.animator.SetInteger("attack_type", _last_subtype);
		Debug.Log("ATTACK STATE!!!!");
	}

	private AnimationCurve GetCurve(int type)
	{
		if (type >= types.Length || types[type].curve == null)
		{
			return base.wgo.GetWOP().GetCurve("attack");
		}
		return types[type].curve.curve;
	}

	private float GetDist(int type)
	{
		if (type < types.Length)
		{
			return types[type].dist;
		}
		return 0f;
	}

	private float GetTime(int type)
	{
		if (type < types.Length)
		{
			return types[type].time;
		}
		return 0f;
	}

	private bool IsAnimBasedTiming(int type)
	{
		return GetTime(type).EqualsTo(0f);
	}
}
