using System;
using System.Collections.Generic;
using UnityEngine;

public class SingletonGameObjects : MonoBehaviour
{
	private static bool _me_is_set = false;

	private static SingletonGameObjects _me = null;

	private static Dictionary<Type, Component> _members = new Dictionary<Type, Component>();

	private static SingletonGameObjects me
	{
		get
		{
			if (!_me_is_set)
			{
				GameObject obj = new GameObject("~ Singletons");
				_me = obj.AddComponent<SingletonGameObjects>();
				_me_is_set = true;
				UnityEngine.Object.DontDestroyOnLoad(obj);
			}
			return _me;
		}
	}

	public static T FindOrCreate<T>() where T : Component
	{
		if (_members.TryGetValue(typeof(T), out var value))
		{
			return (T)value;
		}
		GameObject obj = new GameObject(typeof(T).ToString());
		obj.transform.SetParent(me.transform, worldPositionStays: false);
		T val = obj.AddComponent<T>();
		_members.Add(typeof(T), val);
		return val;
	}
}
