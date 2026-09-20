using System;
using System.Collections.Generic;
using UnityEngine;

public class ZombieMouthSpitWeapon : Weapon, IDamageDealer
{
	public ProjectileStats projectileStats;

	public bool useSphereAsEmitter;

	public SphereCollider sphereCollider;

	private List<SpitProjectile> spitProjectiles = new List<SpitProjectile>();

	public IDamageDealer Dealer => this;

	public event Action<ICombatEntity, AttackContext> OnHit;

	public event Action OnMiss;

	public void Activate(AttackContext ctx)
	{
		SpitProjectile orCreate = ProjectilePool.GetOrCreate<SpitProjectile>();
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
		foreach (SpitProjectile spitProjectile in spitProjectiles)
		{
			UnsubscribeFromEvents(spitProjectile);
		}
		spitProjectiles.Clear();
	}

	private void SubscribeToEvents(SpitProjectile projectile)
	{
		projectile.OnHit += HandleHit;
		projectile.OnMiss += HandleMiss;
		projectile.OnDespawned += HandleProjectileOnDestroy;
	}

	private void UnsubscribeFromEvents(SpitProjectile projectile)
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
		if (projectile is SpitProjectile spitProjectile)
		{
			UnsubscribeFromEvents(spitProjectile);
			spitProjectiles.Remove(spitProjectile);
		}
	}

	private AttackContext GetOriginPosOnSphere(AttackContext ctx)
	{
		Vector3 origin = sphereCollider.transform.position + ctx.direction.XZ().normalized * sphereCollider.radius;
		return new AttackContext(ctx.attacker, ctx.teamType, ctx.fighterDef, ctx.weaponDef, origin, ctx.direction);
	}
}
