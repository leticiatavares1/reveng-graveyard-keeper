using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class ProjectilePool : LazySingleton<ProjectilePool>
{
	[SerializeField]
	private List<Projectile> projectilePrefabs;

	private Dictionary<Type, Pool> projectilePrefabsByType = new Dictionary<Type, Pool>();

	protected override void Awake()
	{
		base.Awake();
		foreach (Projectile projectilePrefab in projectilePrefabs)
		{
			if (!projectilePrefabsByType.TryAdd(projectilePrefab.GetType(), new Pool(projectilePrefab, base.transform, 10)))
			{
				Debug.LogError($"Duplication of projectile prefab {projectilePrefab.name}, type: {projectilePrefab.GetType()}", this);
			}
		}
	}

	public static T GetOrCreate<T>() where T : Projectile
	{
		if (LazySingleton<ProjectilePool>.Instance.projectilePrefabsByType.TryGetValue(typeof(T), out var value))
		{
			return value.GetOrCreateObject<T>();
		}
		return null;
	}

	public static void Release<T>(T obj) where T : Projectile
	{
		if (LazySingleton<ProjectilePool>.Instance.projectilePrefabsByType.TryGetValue(typeof(T), out var value))
		{
			value.ReleaseObject(obj);
		}
	}
}
