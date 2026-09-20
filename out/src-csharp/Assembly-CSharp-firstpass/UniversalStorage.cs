using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UniversalStorage
{
	[SerializeField]
	private List<string> _keys = new List<string>();

	[SerializeField]
	private List<object> _vals = new List<object>();

	public T Get<T>(string name)
	{
		int num = _keys.IndexOf(name);
		if (num == -1)
		{
			throw new Exception("Variable not found: " + name);
		}
		return (T)_vals[num];
	}

	public void Set<T>(string name, T val)
	{
		int num = _keys.IndexOf(name);
		if (num == -1)
		{
			_keys.Add(name);
			_vals.Add(val);
		}
		else
		{
			_vals[num] = val;
		}
	}
}
