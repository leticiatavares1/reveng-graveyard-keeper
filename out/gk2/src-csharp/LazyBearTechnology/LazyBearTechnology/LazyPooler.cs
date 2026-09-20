using System;
using System.Collections.Generic;
using UnityEngine;

namespace LazyBearTechnology;

public class LazyPooler : LazySingleton<LazyPooler>
{
	private Dictionary<Type, Pool> pools = new Dictionary<Type, Pool>();

	private Dictionary<string, Pool> poolsById = new Dictionary<string, Pool>();

	public static Pool CreatePool<T>(T prefab, int initialPoolSize = 0, Pool.PoolType poolType = Pool.PoolType.ImmediateActivation, bool parentAllObjectsInPool = false, bool forceActivateAllNewObjects = false, Pool.MonoBehaviourDelegate onCreateNewObject = null) where T : MonoBehaviour
	{
		Type typeFromHandle = typeof(T);
		if (LazySingleton<LazyPooler>.Instance.pools.ContainsKey(typeFromHandle))
		{
			Debug.LogError("Can't create a second pool of: " + typeFromHandle);
			return null;
		}
		Pool pool = CreatePoolInternal(prefab, initialPoolSize, poolType, parentAllObjectsInPool, forceActivateAllNewObjects, onCreateNewObject);
		LazySingleton<LazyPooler>.Instance.pools.Add(typeFromHandle, pool);
		return pool;
	}

	public static Pool CreatePoolById(string poolId, MonoBehaviour prefab, int initialPoolSize = 0, Pool.PoolType poolType = Pool.PoolType.ImmediateActivation, bool parentAllObjectsInPool = false, bool forceActivateAllNewObjects = false, Pool.MonoBehaviourDelegate onCreateNewObject = null)
	{
		if (LazySingleton<LazyPooler>.Instance.poolsById.TryGetValue(poolId, out var value))
		{
			Debug.LogWarning("Returning existing pool with id = " + poolId);
			return value;
		}
		Pool pool = CreatePoolInternal(prefab, initialPoolSize, poolType, parentAllObjectsInPool, forceActivateAllNewObjects, onCreateNewObject);
		LazySingleton<LazyPooler>.Instance.poolsById.Add(poolId, pool);
		return pool;
	}

	private static Pool CreatePoolInternal(MonoBehaviour prefab, int initialPoolSize = 0, Pool.PoolType poolType = Pool.PoolType.ImmediateActivation, bool parentAllObjectsInPool = false, bool forceActivateAllNewObjects = false, Pool.MonoBehaviourDelegate onCreateNewObject = null)
	{
		GameObject gameObject = null;
		if (parentAllObjectsInPool)
		{
			gameObject = new GameObject();
			gameObject.name = $"Pool parent of type:[{prefab.GetType()}]";
			gameObject.transform.parent = LazySingleton<LazyPooler>.Instance.transform;
		}
		return new Pool(prefab, parentAllObjectsInPool ? gameObject.transform : prefab.transform.parent, initialPoolSize, poolType, forceActivateAllNewObjects, onCreateNewObject);
	}

	public static T GetObject<T>() where T : MonoBehaviour
	{
		Pool poolByType = GetPoolByType<T>();
		if (poolByType == null)
		{
			return null;
		}
		return poolByType.GetOrCreateObject<T>();
	}

	public static T GetObject<T>(Transform parent) where T : MonoBehaviour
	{
		T @object = GetObject<T>();
		@object.transform.SetParent(parent);
		return @object;
	}

	public static void ReleaseObject<T>(T obj) where T : MonoBehaviour
	{
		GetPoolByType<T>()?.ReleaseObject(obj);
	}

	public static T GetObject<T>(string poolId) where T : MonoBehaviour
	{
		Pool poolById = GetPoolById(poolId);
		if (poolById == null)
		{
			return null;
		}
		return poolById.GetOrCreateObject<T>();
	}

	public static T GetObject<T>(string poolId, Transform parent) where T : MonoBehaviour
	{
		T @object = GetObject<T>(poolId);
		@object.transform.SetParent(parent);
		return @object;
	}

	public static void ReleaseObject<T>(string poolId, T obj) where T : MonoBehaviour
	{
		GetPoolById(poolId)?.ReleaseObject(obj);
	}

	public static Pool GetPoolByType<T>(T objectUsedOnlyForPoolReference = null) where T : MonoBehaviour
	{
		if (LazySingleton<LazyPooler>.Instance.pools.TryGetValue(typeof(T), out var value))
		{
			return value;
		}
		Debug.LogError("A pool for an object type [" + typeof(T)?.ToString() + "] was not yet created. Call CreatePool() first.");
		return null;
	}

	public static Pool GetPoolById(string poolId)
	{
		if (LazySingleton<LazyPooler>.Instance.poolsById.TryGetValue(poolId, out var value))
		{
			return value;
		}
		Debug.LogError("A pool with id = " + poolId + " was not yet created. Call CreatePool() first.");
		return null;
	}

	public static bool RemovePoolById(string poolId)
	{
		return LazySingleton<LazyPooler>.Instance.poolsById.Remove(poolId);
	}

	public static void DeactivateUnnecessaryObjectsForAllPools()
	{
		foreach (Pool value in LazySingleton<LazyPooler>.Instance.pools.Values)
		{
			value.DeactivateUnnecessaryObjects();
		}
		foreach (Pool value2 in LazySingleton<LazyPooler>.Instance.poolsById.Values)
		{
			value2.DeactivateUnnecessaryObjects();
		}
	}
}
