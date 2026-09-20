using System;
using System.Collections.Generic;
using UnityEngine;

public class ArrowProjectile : Projectile, IDamageDealer
{
	public TrailRenderer trailRenderer;

	[SerializeField]
	private DamageSourceType damageSourceType = DamageSourceType.Arrow;

	[SerializeField]
	private BoxCollider hitBoxCollider;

	[SerializeField]
	private int sweepLayerMask = -1;

	[SerializeField]
	private Transform endPointTransform;

	private AttackContext attackContext;

	private int wgoPartInstanceId;

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
		if (collider == null || (bool)collider.GetComponentInParent<ArrowProjectile>())
		{
			return;
		}
		if (collider.gameObject.layer == 11)
		{
			HandleHit(collider.transform, hit.point);
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
		if (collider.TryGetComponent<HitZone>(out var component))
		{
			var (hitResult, reaction) = ProcessHitZone(component, value, collider, hit.point);
			if (hitResult.ShouldStopProjectile)
			{
				FinalizeHit(collider, componentInParent, 0f, hit.point);
				component.PlayHitEffect(hitResult, reaction, hit.point);
			}
			else if (hitResult.type == HitResultType.ZoneMarked)
			{
				value.MarkZonePassed(component);
			}
			else if (hitResult.type != HitResultType.Absorbed && hitResult.ShouldDealDamage)
			{
				FinalizeHit(collider, componentInParent, hitResult.damageMultiplier, hit.point);
				component.PlayHitEffect(hitResult, reaction, hit.point);
			}
		}
		else
		{
			FinalizeHit(collider, componentInParent, 1f, hit.point);
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
		WgoPart componentInParent = other.GetComponentInParent<WgoPart>();
		if ((bool)componentInParent && componentInParent.Wgo.TeamType != attackContext.teamType)
		{
			Wgo.OnWgoDestroy -= OnWgoParentDestroy;
			Wgo.OnWgoDestroy += OnWgoParentDestroy;
			wgoPartInstanceId = componentInParent.Wgo.transform.GetInstanceID();
			HandleHit(other.transform, hitPoint);
			attackContext = AttackContext.WithHit(attackContext, hitPoint, base.Stats?.damageEffectOverride);
			if (damageMultiplier == 0f)
			{
				attackContext = AttackContext.WithCustomDamage(attackContext, 0);
			}
			this.OnHit?.Invoke(componentInParent.Wgo, attackContext);
		}
	}

	private void HandleHit(Transform parent, Vector3 hitPosition)
	{
		wasHit = true;
		Vector3 vector = endPointTransform.position - base.transform.position;
		base.transform.localPosition = hitPosition - vector;
		base.transform.SetParent(parent);
		trailRenderer.enabled = false;
		hitBoxCollider.enabled = false;
	}

	private void OnWgoParentDestroy(Wgo parentWgo)
	{
		if (parentWgo.transform.GetInstanceID() == wgoPartInstanceId)
		{
			Wgo.OnWgoDestroy -= OnWgoParentDestroy;
			Despawn();
		}
	}

	private void OnEnable()
	{
		trailRenderer.enabled = true;
		hitStates.Clear();
		hasPreviousPosition = false;
	}

	private void OnDisable()
	{
		Wgo.OnWgoDestroy -= OnWgoParentDestroy;
		if (base.IsSpawned)
		{
			Despawn();
		}
		trailRenderer.enabled = false;
		hitStates.Clear();
		hasPreviousPosition = false;
	}

	protected override void Despawn()
	{
		if (base.IsSpawned)
		{
			hitBoxCollider.enabled = true;
			base.Despawn();
		}
	}
}
