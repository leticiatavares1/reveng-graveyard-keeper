using System;
using LazyBearTechnology;
using UnityEngine;

[Serializable]
public class UIPrefabPool : MonoBehaviour
{
	[SerializeField]
	private MonoBehaviour prefab;

	[SerializeField]
	private int initialPoolSize = 10;

	[SerializeField]
	private Pool.PoolType poolType;

	private Pool pool;

	public MonoBehaviour Prefab => prefab;

	public void Init()
	{
		pool = new Pool(prefab, base.transform, initialPoolSize, poolType);
	}

	public T GetOrCreateObject<T>(Transform newParent) where T : MonoBehaviour
	{
		T orCreateObject = pool.GetOrCreateObject<T>();
		orCreateObject.transform.SetParent(newParent);
		return orCreateObject;
	}

	public void ReleaseObject<T>(T element) where T : MonoBehaviour
	{
		pool.ReleaseObject(element);
		element.transform.SetParent(base.transform);
	}
}
