using System;
using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
	private bool isSpawned;

	private ProjectileStats data;

	private float age;

	private Vector3 position;

	private Vector3 direction;

	protected bool wasHit;

	protected ProjectileStats Stats => data;

	protected bool IsSpawned => isSpawned;

	public event Action<Projectile> OnDespawned;

	public void Launch(ProjectileStats stats, Vector3 position, Vector3 direction)
	{
		isSpawned = true;
		data = stats;
		this.position = position;
		this.direction = direction;
		base.transform.position = position;
		if (!direction.sqrMagnitude.EqualsTo(0f))
		{
			base.transform.rotation = Quaternion.FromToRotation(Vector3.right, direction);
		}
		age = 0f;
	}

	private void Update()
	{
		if (!MainGame.IsGamePaused)
		{
			age += Time.deltaTime;
			if (age >= data.maxLifeTime)
			{
				Despawn();
			}
			else
			{
				Tick();
			}
		}
	}

	private void Tick()
	{
		if (!wasHit)
		{
			base.transform.position += direction * (data.speed * Time.deltaTime);
		}
	}

	protected virtual void Despawn()
	{
		if (isSpawned)
		{
			isSpawned = false;
			wasHit = false;
			base.transform.position = Vector3.zero;
			base.transform.rotation = Quaternion.identity;
			base.gameObject.SetActive(value: false);
			ProjectilePool.Release(this);
			this.OnDespawned?.Invoke(this);
		}
	}
}
