using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIPrefabsPooler : MonoBehaviour, ILazyGUIElement
{
	private static UIPrefabsPooler instance;

	private List<UIPrefabPool> pools = new List<UIPrefabPool>();

	private Dictionary<Type, UIPrefabPool> poolsCache = new Dictionary<Type, UIPrefabPool>();

	public static UIPrefabsPooler Instance => instance;

	public void Init()
	{
		instance = this;
		pools = GetComponentsInChildren<UIPrefabPool>().ToList();
		foreach (UIPrefabPool pool in pools)
		{
			if (!poolsCache.TryAdd(pool.Prefab.GetType(), pool))
			{
				Debug.LogError(string.Format("Error: {0}: pool type [{1}] is already exists", "UIPrefabPool", pool.Prefab.GetType()));
			}
			else
			{
				pool.Init();
			}
		}
		pools.ForEach(delegate(UIPrefabPool item)
		{
			item.Init();
		});
	}

	public T GetElementFromPool<T>(Transform newParent) where T : MonoBehaviour
	{
		if (poolsCache.TryGetValue(typeof(T), out var value))
		{
			return value.GetOrCreateObject<T>(newParent);
		}
		Debug.LogError($"No pool found for type [{typeof(T)}]");
		return null;
	}

	public void ReleaseElementToPool<T>(T element) where T : MonoBehaviour
	{
		Type type = element.GetType();
		if (poolsCache.TryGetValue(type, out var value))
		{
			value.ReleaseObject(element);
			element.transform.SetParent(value.transform);
		}
		else
		{
			Debug.LogError($"No pool found for type [{type}]");
		}
	}
}
