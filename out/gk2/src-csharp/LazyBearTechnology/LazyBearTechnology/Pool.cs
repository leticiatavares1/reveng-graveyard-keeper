using System.Collections.Generic;
using UnityEngine;

namespace LazyBearTechnology;

public class Pool
{
	public enum PoolType
	{
		ImmediateActivation,
		SmartActivation
	}

	public delegate void MonoBehaviourDelegate(MonoBehaviour obj);

	private Stack<MonoBehaviour> objects = new Stack<MonoBehaviour>();

	private MonoBehaviour prefab;

	private Transform poolParent;

	private bool forceActivateAllNewObjects;

	private PoolType poolType;

	private MonoBehaviourDelegate onDeactivate;

	private MonoBehaviourDelegate onCreateNewObject;

	private static GameObject instantiateHideRoot;

	public MonoBehaviour Prefab => prefab;

	public Stack<MonoBehaviour> Objects => objects;

	public Pool(MonoBehaviour prefab, Transform poolParent, int initialSize, PoolType poolType = PoolType.ImmediateActivation, bool forceActivateAllNewObjects = false, MonoBehaviourDelegate onCreateNewObject = null)
	{
		this.prefab = prefab;
		this.poolParent = poolParent;
		this.poolType = poolType;
		this.forceActivateAllNewObjects = forceActivateAllNewObjects;
		this.onCreateNewObject = onCreateNewObject;
		for (int i = 0; i < initialSize; i++)
		{
			AddObjectToPool();
		}
	}

	public void AddObjectToPool()
	{
		MonoBehaviour monoBehaviour = Object.Instantiate(prefab, poolParent);
		objects.Push(monoBehaviour);
		if (poolType == PoolType.ImmediateActivation)
		{
			monoBehaviour.gameObject.SetActive(value: false);
		}
		if (forceActivateAllNewObjects)
		{
			monoBehaviour.gameObject.SetActive(value: true);
		}
		onCreateNewObject?.Invoke(monoBehaviour);
	}

	public void ReleaseObject<T>(T obj) where T : MonoBehaviour
	{
		objects.Push(obj);
		obj.transform.SetParent(poolParent);
		if (poolType == PoolType.ImmediateActivation)
		{
			obj.gameObject.SetActive(value: false);
		}
		if (obj is IPoolable poolable)
		{
			poolable.OnPoolableObjReleased();
		}
	}

	public void ReleaseAllObjectsAndClearList<T>(ref List<T> objs) where T : MonoBehaviour
	{
		if (objs == null)
		{
			return;
		}
		foreach (T obj in objs)
		{
			ReleaseObject(obj);
		}
		objs.Clear();
	}

	public T GetOrCreateObject<T>() where T : MonoBehaviour
	{
		if (objects.Count == 0)
		{
			AddObjectToPool();
		}
		MonoBehaviour monoBehaviour = objects.Pop();
		if (poolType == PoolType.ImmediateActivation || !monoBehaviour.gameObject.activeSelf)
		{
			monoBehaviour.gameObject.SetActive(value: true);
		}
		return monoBehaviour as T;
	}

	public T GetOrCreateInactiveObject<T>() where T : MonoBehaviour
	{
		if (objects.Count == 0)
		{
			AddInactiveObjectToPool();
		}
		return objects.Pop() as T;
	}

	public void AddInactiveObjectToPool()
	{
		Transform parent = GetInstantiateHideRoot();
		MonoBehaviour monoBehaviour = Object.Instantiate(prefab, parent);
		monoBehaviour.gameObject.SetActive(value: false);
		monoBehaviour.transform.SetParent(poolParent, worldPositionStays: false);
		objects.Push(monoBehaviour);
		onCreateNewObject?.Invoke(monoBehaviour);
	}

	private static Transform GetInstantiateHideRoot()
	{
		if (instantiateHideRoot == null)
		{
			instantiateHideRoot = new GameObject("PoolInstantiateInactive");
			Object.DontDestroyOnLoad(instantiateHideRoot);
			instantiateHideRoot.hideFlags = HideFlags.HideAndDontSave;
			instantiateHideRoot.SetActive(value: false);
		}
		return instantiateHideRoot.transform;
	}

	public void DeactivateUnnecessaryObjects()
	{
		if (poolType != PoolType.SmartActivation)
		{
			return;
		}
		bool flag = onDeactivate != null;
		foreach (MonoBehaviour @object in objects)
		{
			if (flag)
			{
				onDeactivate(@object);
			}
			else if (@object.gameObject.activeSelf)
			{
				@object.gameObject.SetActive(value: false);
			}
		}
	}

	public void SetCustomSmartDeactivationMethod(MonoBehaviourDelegate onDeactivate)
	{
		this.onDeactivate = onDeactivate;
	}

	public void SetCustomOnCreateNewObjectDelegate(MonoBehaviourDelegate onCreateNewObject)
	{
		this.onCreateNewObject = onCreateNewObject;
	}

	public int TrimIdleToSize(int maxIdleCount)
	{
		int num = 0;
		while (objects.Count > maxIdleCount)
		{
			MonoBehaviour monoBehaviour = objects.Pop();
			if (monoBehaviour != null)
			{
				Object.Destroy(monoBehaviour.gameObject);
			}
			num++;
		}
		return num;
	}
}
