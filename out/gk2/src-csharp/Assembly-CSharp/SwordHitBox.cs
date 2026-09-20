using System;
using UnityEngine;

public class SwordHitBox : MonoBehaviour, IDamageDealer
{
	[SerializeField]
	private DamageSourceType damageSourceType = DamageSourceType.Sword;

	[SerializeField]
	private HitStatesAccumulator hitStatesAccumulator;

	private AttackContext attackContext;

	public event Action<ICombatEntity, AttackContext> OnHit;

	public event Action OnMiss;

	public void Activate(AttackContext ctx)
	{
		attackContext = ctx;
	}

	public void Cancel()
	{
	}

	private void OnTriggerEnter(Collider other)
	{
		ICombatEntity componentInParent = other.GetComponentInParent<ICombatEntity>();
		if (componentInParent == null || !(componentInParent is UnityEngine.Object) || componentInParent.TeamType == attackContext.teamType)
		{
			return;
		}
		if (!hitStatesAccumulator.hitStates.TryGetValue(componentInParent, out var value))
		{
			value = new WeaponHitState();
			hitStatesAccumulator.hitStates[componentInParent] = value;
		}
		if (hitStatesAccumulator.hitColliders.Contains(other))
		{
			return;
		}
		hitStatesAccumulator.hitColliders.Add(other);
		float num = 1f;
		HitZoneReaction hitZoneReaction = null;
		HitResult hitResult = default(HitResult);
		if (!other.TryGetComponent<HitZone>(out var component))
		{
			return;
		}
		HitZoneType zoneType = component.ZoneType;
		if ((zoneType == HitZoneType.Door || zoneType == HitZoneType.DoorFrontArea) && IsBackstabAgainstDoor(component))
		{
			return;
		}
		(hitResult, hitZoneReaction) = ProcessHitZone(component, value, other);
		if (value.DamageDealt && hitResult.ShouldStopProjectile)
		{
			return;
		}
		if (hitResult.type == HitResultType.ZoneMarked)
		{
			if (!value.DamageDealt)
			{
				value.MarkZonePassed(component);
			}
		}
		else if (hitResult.type != HitResultType.Absorbed)
		{
			if (hitResult.ShouldStopProjectile)
			{
				num = 0f;
			}
			else if (hitResult.ShouldDealDamage)
			{
				num = hitResult.damageMultiplier;
			}
			Vector3 contactPosition = other.GetContactPosition(base.transform.position);
			attackContext = new AttackContext(attackContext, contactPosition);
			if (num == 0f)
			{
				attackContext = new AttackContext(attackContext, 0);
			}
			this.OnHit?.Invoke(componentInParent, attackContext);
			component.PlayHitEffect(hitResult, hitZoneReaction, attackContext.hitPosition);
			if (hitResult.ShouldDealDamage)
			{
				value.MarkDamageDealt();
			}
		}
	}

	private (HitResult result, HitZoneReaction reaction) ProcessHitZone(HitZone hitZone, WeaponHitState state, Collider other)
	{
		if (state.HasPassedZone(hitZone))
		{
			return (result: HitResult.Absorbed(), reaction: null);
		}
		if (hitZone.ZoneType == HitZoneType.Body && state.IsInsideProtectedZone)
		{
			return (result: HitResult.Absorbed(), reaction: null);
		}
		Vector3 contactPosition = other.GetContactPosition(base.transform.position);
		return hitZone.ProcessHit(damageSourceType, attackContext, contactPosition);
	}

	private bool IsBackstabAgainstDoor(HitZone hitZone)
	{
		AnimationComponent animationComponent = attackContext.attackersAttackComponent?.animationComponent;
		AnimationComponentBase animationComponent2 = hitZone.AnimationComponent;
		if (animationComponent == null || animationComponent2 == null)
		{
			return false;
		}
		return animationComponent.GetDirection() == animationComponent2.GetDirection();
	}
}
