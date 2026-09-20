using System;
using System.Collections.Generic;
using LinqTools;
using UnityEngine;

namespace LazyBearTechnology;

public sealed class LazyWidgetPrefabContainer : LazySingleton<LazyWidgetPrefabContainer>
{
	private Dictionary<Type, LazyWidgetBase> widgets;

	[SerializeField]
	private GameObject container;

	public void Init()
	{
		List<LazyWidgetBase> list = GetComponentsInChildren<LazyWidgetBase>(includeInactive: true).ToList();
		widgets = new Dictionary<Type, LazyWidgetBase>();
		foreach (LazyWidgetBase item in list)
		{
			if (!(item.transform.parent != container.transform))
			{
				widgets.Add(item.GetDataType(), item);
				Debug.Log($"#lazy_ui# Added new widget prefab with data type:[{item.GetDataType()}] widget type:[{item.GetType()}]");
			}
		}
		base.gameObject.SetActive(value: false);
	}

	public static LazyWidgetBase GetPrefabFromDataType<T>() where T : LazyWidgetDataBase
	{
		if (!LazySingleton<LazyWidgetPrefabContainer>.Instance.widgets.TryGetValue(typeof(T), out var value))
		{
			throw new Exception($"Cannot find prefab for type {typeof(T)}");
		}
		return value;
	}

	public static LazyWidgetBase GetPrefabFromDataType(Type type)
	{
		if (!LazySingleton<LazyWidgetPrefabContainer>.Instance.widgets.TryGetValue(type, out var value))
		{
			throw new Exception($"Cannot find prefab for type {type}");
		}
		return value;
	}

	public static LazyWidgetBase GetPrefabFromDataObject(LazyWidgetDataBase data)
	{
		if (!LazySingleton<LazyWidgetPrefabContainer>.Instance.widgets.TryGetValue(data.GetType(), out var value))
		{
			throw new Exception($"Cannot find prefab for type {data.GetType()}");
		}
		return value;
	}
}
