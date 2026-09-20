using System;
using System.Collections.Generic;
using UnityEngine;

public class BowWeapon : Weapon, IDamageDealer
{
	public ProjectileStats projectileStats;

	public bool useSphereAsEmitter;

	public SphereCollider sphereCollider;

	private List<ArrowProjectile> arrows = new List<ArrowProjectile>();

	public IDamageDealer Dealer => this;

	public event Action<ICombatEntity, AttackContext> OnHit;

	public event Action OnMiss;

	public void Activate(AttackContext ctx)
	{
		ArrowProjectile orCreate = ProjectilePool.GetOrCreate<ArrowProjectile>();
		if (!(orCreate == null))
		{
			if (useSphereAsEmitter && (bool)sphereCollider)
			{
				ctx = GetOriginPosOnSphere(ctx);
			}
			orCreate.Activate(ctx);
			orCreate.Launch(projectileStats, ctx.origin, ctx.direction);
			SubscribeToEvents(orCreate);
		}
	}

	public void Cancel()
	{
	}

	private void OnDestroy()
	{
		foreach (ArrowProjectile arrow in arrows)
		{
			UnsubscribeFromEvents(arrow);
		}
		arrows.Clear();
	}

	private void SubscribeToEvents(ArrowProjectile projectile)
	{
		projectile.OnHit += HandleHit;
		projectile.OnMiss += HandleMiss;
		projectile.OnDespawned += HandleProjectileOnDestroy;
	}

	private void UnsubscribeFromEvents(ArrowProjectile projectile)
	{
		projectile.OnHit -= HandleHit;
		projectile.OnMiss -= HandleMiss;
		projectile.OnDespawned -= HandleProjectileOnDestroy;
	}

	private void HandleHit(ICombatEntity combatEntity, AttackContext ctx)
	{
		this.OnHit?.Invoke(combatEntity, ctx);
	}

	private void HandleMiss()
	{
		this.OnMiss?.Invoke();
	}

	private void HandleProjectileOnDestroy(Projectile projectile)
	{
		if (projectile is ArrowProjectile arrowProjectile)
		{
			UnsubscribeFromEvents(arrowProjectile);
			arrows.Remove(arrowProjectile);
		}
	}

	private AttackContext GetOriginPosOnSphere(AttackContext ctx)
	{
		Vector3 origin = sphereCollider.transform.TransformPoint(sphereCollider.center) + ctx.direction.XZ().normalized * sphereCollider.radius;
		return new AttackContext(ctx.attacker, ctx.teamType, ctx.fighterDef, ctx.weaponDef, origin, ctx.direction);
	}
}
