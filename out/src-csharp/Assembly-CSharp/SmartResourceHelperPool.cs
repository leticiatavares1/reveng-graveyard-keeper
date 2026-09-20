using System;
using System.Collections.Generic;
using UnityEngine;

public class SmartResourceHelperPool
{
	public Type type;

	public Dictionary<string, UnityEngine.Object> loaded = new Dictionary<string, UnityEngine.Object>();

	public SmartResourceHelperPool(Type t)
	{
		type = t;
	}

	public UnityEngine.Object GetObject(string res_name)
	{
		if (loaded.TryGetValue(res_name, out var value))
		{
			return value;
		}
		if (SmartResourceHelper.me.queue.ContainsKey(res_name))
		{
			SmartResourceHelper.me.queue.Remove(res_name);
		}
		value = Resources.Load(res_name);
		if (value == null)
		{
			Debug.LogError("Error loading resource \"" + res_name + "\"");
		}
		loaded.Add(res_name, value);
		return value;
	}
}
