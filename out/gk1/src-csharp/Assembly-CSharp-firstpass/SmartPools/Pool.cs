using System.Collections.Generic;
using UnityEngine;

namespace SmartPools;

public class Pool
{
	public Stack<MonoBehaviour> members = new Stack<MonoBehaviour>();

	public MonoBehaviour prefab;

	public GameObject pool_go;

	public int max_pool_size;

	public Vector3 prefab_local_scale;

	private int _cur_pool_size;

	public int max_objects_per_frame = 5;

	public bool paused;

	public bool activate_on_creation = true;

	public void AddObjectToPool()
	{
		MonoBehaviour monoBehaviour = Object.Instantiate(prefab);
		members.Push(monoBehaviour);
		monoBehaviour.transform.parent = pool_go.transform;
		monoBehaviour.gameObject.SetActive(value: false);
	}

	public void DestroyObject<T>(T obj) where T : MonoBehaviour
	{
		members.Push(obj);
		obj.gameObject.SetActive(value: false);
	}

	public T CreateObject<T>() where T : MonoBehaviour
	{
		if (members.Count == 0)
		{
			AddObjectToPool();
		}
		MonoBehaviour monoBehaviour = members.Pop();
		monoBehaviour.transform.localScale = prefab_local_scale;
		if (activate_on_creation)
		{
			monoBehaviour.gameObject.SetActive(value: true);
		}
		return monoBehaviour as T;
	}

	public void Update()
	{
		_cur_pool_size = members.Count;
		if (_cur_pool_size < max_pool_size)
		{
			for (int i = 0; i < max_objects_per_frame; i++)
			{
				AddObjectToPool();
			}
		}
	}

	public void ConfigurePool(int object_per_frame, bool activate_on_creation)
	{
		this.activate_on_creation = activate_on_creation;
		max_objects_per_frame = object_per_frame;
	}
}
