using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class SpitProjectile : Projectile, IDamageDealer
{
	[SerializeField]
	private DamageSourceType damageSourceType = DamageSourceType.Arrow;

	[SerializeField]
	private BoxCollider hitBoxCollider;

	[SerializeField]
	private int sweepLayerMask = -1;

	private AttackContext attackContext;

	private Vector3 previousPosition;

	private bool hasPreviousPosition;

	private readonly List<RaycastHit> sweepHits = new List<RaycastHit>();

	private readonly Dictionary<ICombatEntity, WeaponHitState> hitStates = new Dictionary<ICombatEntity, WeaponHitState>();

	public event Action<ICombatEntity, AttackContext> OnHit;

	public event Action OnMiss;

	public void Activate(AttackContext ctx)
	{
		attackContext = ctx;
	}

	public void Cancel()
	{
	}

	private void LateUpdate()
	{
		if (wasHit || hitBoxCollider == null)
		{
			return;
		}
		Vector3 currentPosition = hitBoxCollider.transform.TransformPoint(hitBoxCollider.center);
		Quaternion rotation = hitBoxCollider.transform.rotation;
		if (hasPreviousPosition && SpecialPhysicsCastUtils.GetBoxLineSweepHits(hitBoxCollider, previousPosition, currentPosition, rotation, sweepLayerMask, sweepHits))
		{
			foreach (RaycastHit sweepHit in sweepHits)
			{
				if (wasHit)
				{
					break;
				}
				ProcessSweepHit(sweepHit);
			}
		}
		previousPosition = currentPosition;
		hasPreviousPosition = true;
	}

	private void ProcessSweepHit(RaycastHit hit)
	{
		Collider collider = hit.collider;
		if (collider == null || (bool)collider.GetComponentInParent<SpitProjectile>())
		{
			return;
		}
		if (collider.gameObject.layer == 11)
		{
			HandleHit();
			LazyAudio.PlayAtPos("spitter_hit", hit.point);
		}
		else
		{
			if (collider.TryGetComponent<DropSearcher>(out var _) || collider.TryGetComponent<PlayerInteractionComponent>(out var _) || collider.TryGetComponent<SwordHitBox>(out var _))
			{
				return;
			}
			ICombatEntity componentInParent = collider.GetComponentInParent<ICombatEntity>();
			if (componentInParent == null)
			{
				return;
			}
			if (!hitStates.TryGetValue(componentInParent, out var value))
			{
				value = new WeaponHitState();
				hitStates[componentInParent] = value;
			}
			if (collider.TryGetComponent<HitZone>(out var component4))
			{
				var (hitResult, reaction) = ProcessHitZone(component4, value, collider, hit.point);
				if (hitResult.ShouldStopProjectile)
				{
					FinalizeHit(collider, componentInParent, 0f, hit.point);
					component4.PlayHitEffect(hitResult, reaction, hit.point);
				}
				else if (hitResult.type == HitResultType.ZoneMarked)
				{
					value.MarkZonePassed(component4);
				}
				else if (hitResult.type != HitResultType.Absorbed && hitResult.ShouldDealDamage)
				{
					FinalizeHit(collider, componentInParent, hitResult.damageMultiplier, hit.point);
					component4.PlayHitEffect(hitResult, reaction, hit.point);
				}
			}
			else
			{
				FinalizeHit(collider, componentInParent, 1f, hit.point);
			}
		}
	}

	private (HitResult result, HitZoneReaction reaction) ProcessHitZone(HitZone hitZone, WeaponHitState state, Collider other, Vector3 hitPoint)
	{
		if (state.HasPassedZone(hitZone))
		{
			return (result: HitResult.Absorbed(), reaction: null);
		}
		if (hitZone.ZoneType == HitZoneType.Body && state.IsInsideProtectedZone)
		{
			return (result: HitResult.Absorbed(), reaction: null);
		}
		return hitZone.ProcessHit(damageSourceType, attackContext, hitPoint);
	}

	private void FinalizeHit(Collider other, ICombatEntity combatEntity, float damageMultiplier, Vector3 hitPoint)
	{
		if (combatEntity is UnityEngine.Object @object && (bool)@object && combatEntity.TeamType != attackContext.teamType)
		{
			HandleHit();
			attackContext = AttackContext.WithHit(attackContext, hitPoint, base.Stats?.damageEffectOverride);
			if (damageMultiplier.EqualsTo(0f))
			{
				attackContext = AttackContext.WithCustomDamage(attackContext, 0);
			}
			else
			{
				TryPlayDamageHitFx(hitPoint);
			}
			this.OnHit?.Invoke(combatEntity, attackContext);
			Despawn();
		}
	}

	private void TryPlayDamageHitFx(Vector3 hitPoint)
	{
		string value = base.Stats?.onDamageHitFxName;
		if (!string.IsNullOrEmpty(value))
		{
			WorldFX.Spawn(hitPoint, value);
		}
	}

	private void HandleHit()
	{
		wasHit = true;
		hitBoxCollider.enabled = false;
	}

	private void OnEnable()
	{
		hitStates.Clear();
		hasPreviousPosition = false;
	}

	private void OnDisable()
	{
		hitStates.Clear();
		hasPreviousPosition = false;
	}

	protected override void Despawn()
	{
		hitBoxCollider.enabled = true;
		base.Despawn();
	}
}
