using System;
using System.Collections.Generic;
using UnityEngine;

namespace SmartPools;

public class SmartPooler : MonoBehaviour
{
	private Dictionary<Type, Pool> _pools = new Dictionary<Type, Pool>();

	private static bool _inited;

	private static SmartPooler _me;

	private static SmartPooler me
	{
		get
		{
			if (!_inited)
			{
				_inited = true;
				_me = new GameObject("Smart Pooler").AddComponent<SmartPooler>();
				UnityEngine.Object.DontDestroyOnLoad(_me);
			}
			return _me;
		}
	}

	public static Pool CreatePool<T>(T prefab, int pool_size = 0) where T : MonoBehaviour
	{
		Type typeFromHandle = typeof(T);
		if (me._pools.ContainsKey(typeFromHandle))
		{
			Debug.LogError("Can't create a second pool of: " + typeFromHandle);
			return null;
		}
		Debug.Log("Creating pool of objects " + typeFromHandle?.ToString() + ", n = " + pool_size);
		Pool pool = new Pool
		{
			prefab = prefab,
			pool_go = new GameObject(typeFromHandle.ToString()),
			max_pool_size = pool_size,
			prefab_local_scale = prefab.transform.localScale
		};
		pool.pool_go.transform.parent = me.transform;
		me._pools.Add(typeFromHandle, pool);
		return pool;
	}

	private static Pool GetPoolByType<T>() where T : MonoBehaviour
	{
		if (me._pools.TryGetValue(typeof(T), out var value))
		{
			return value;
		}
		Debug.LogError("Pooler for object " + typeof(T)?.ToString() + " was not created");
		return null;
	}

	public static T CreateObject<T>() where T : MonoBehaviour
	{
		Pool poolByType = GetPoolByType<T>();
		if (poolByType != null)
		{
			return poolByType.CreateObject<T>();
		}
		return null;
	}

	public static void DestroyObject<T>(T obj) where T : MonoBehaviour
	{
		GetPoolByType<T>()?.DestroyObject(obj);
	}

	public void Update()
	{
		foreach (KeyValuePair<Type, Pool> pool in _pools)
		{
			if (!pool.Value.paused)
			{
				pool.Value.Update();
			}
		}
	}

	public static void PausePool<T>() where T : MonoBehaviour
	{
		Pool poolByType = GetPoolByType<T>();
		if (poolByType != null)
		{
			poolByType.paused = true;
		}
	}

	public static void ResumePool<T>() where T : MonoBehaviour
	{
		Pool poolByType = GetPoolByType<T>();
		if (poolByType != null)
		{
			poolByType.paused = false;
		}
	}
}
