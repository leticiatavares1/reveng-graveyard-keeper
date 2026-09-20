using System;
using System.Collections.Generic;
using LinqTools;
using UnityEngine;

public class SmartResourceHelper : MonoBehaviour
{
	protected struct AssetRequest
	{
		public ResourceRequest req;

		public SmartResourceHelperPool pool;
	}

	private static bool _inited;

	private static SmartResourceHelper _me;

	private float _time;

	private bool _calculating_time;

	private float _start_time;

	public int max_simultaneous_loading_files = 4;

	private Dictionary<Type, SmartResourceHelperPool> _pools = new Dictionary<Type, SmartResourceHelperPool>();

	public Dictionary<string, SmartResourceHelperPool> queue = new Dictionary<string, SmartResourceHelperPool>();

	protected Dictionary<string, AssetRequest> loading = new Dictionary<string, AssetRequest>();

	public static SmartResourceHelper me
	{
		get
		{
			if (!_inited)
			{
				_me = new GameObject("SmartResourceHelper", typeof(SmartResourceHelper)).GetComponent<SmartResourceHelper>();
				_inited = true;
			}
			return _me;
		}
	}

	private SmartResourceHelperPool GetOrCreatePool<T>()
	{
		Type typeFromHandle = typeof(T);
		if (_pools.TryGetValue(typeFromHandle, out var value))
		{
			return value;
		}
		value = new SmartResourceHelperPool(typeFromHandle);
		_pools.Add(typeFromHandle, value);
		return value;
	}

	public void LoadAsync<T>(string res_name)
	{
		SmartResourceHelperPool orCreatePool = GetOrCreatePool<T>();
		if (!loading.ContainsKey(res_name) && !queue.ContainsKey(res_name) && !orCreatePool.loaded.ContainsKey(res_name))
		{
			queue.Add(res_name, orCreatePool);
			UpdateLoadingQueue();
		}
	}

	public void LoadListAsync<T>(List<string> resources)
	{
		foreach (string resource in resources)
		{
			LoadAsync<T>(resource);
		}
	}

	public void Update()
	{
		_time += Time.deltaTime;
		if (_time < 0.03f)
		{
			return;
		}
		_time = 0f;
		UpdateLoadingQueue();
		if (loading.Count == 0)
		{
			return;
		}
		List<string> list = loading.Keys.ToList();
		int num = 0;
		bool flag = false;
		while (num < list.Count)
		{
			string text = list[num];
			AssetRequest assetRequest = loading[text];
			if (assetRequest.req.isDone)
			{
				if (assetRequest.req.asset == null)
				{
					Debug.LogWarning("Error loading asset \"" + text + "\"");
				}
				if (!assetRequest.pool.loaded.ContainsKey(text))
				{
					assetRequest.pool.loaded.Add(text, assetRequest.req.asset);
				}
				list.RemoveAt(num);
				loading.Remove(text);
				flag = true;
			}
			else
			{
				num++;
			}
		}
		if (flag)
		{
			UpdateLoadingQueue();
		}
	}

	private void UpdateLoadingQueue()
	{
		while (loading.Count < max_simultaneous_loading_files && queue.Count != 0)
		{
			KeyValuePair<string, SmartResourceHelperPool> keyValuePair = queue.First();
			queue.Remove(keyValuePair.Key);
			loading.Add(keyValuePair.Key, new AssetRequest
			{
				req = Resources.LoadAsync(keyValuePair.Key),
				pool = keyValuePair.Value
			});
		}
	}

	public static T GetResource<T>(string res_name) where T : MonoBehaviour
	{
		UnityEngine.Object @object = me.GetOrCreatePool<T>().GetObject(res_name);
		GameObject gameObject = (GameObject)@object;
		if (gameObject == null)
		{
			return null;
		}
		T component = gameObject.GetComponent<T>();
		if (component == null)
		{
			Debug.Log("GetResource is null for res: " + res_name + ", o = " + ((@object == null) ? "null" : @object.ToString()));
		}
		return component;
	}

	public static T GetResourceAs<T>(string res_name) where T : UnityEngine.Object
	{
		UnityEngine.Object @object = me.GetOrCreatePool<T>().GetObject(res_name);
		if (@object == null)
		{
			Debug.Log("GetResource is null for res: " + res_name);
		}
		return @object as T;
	}
}
