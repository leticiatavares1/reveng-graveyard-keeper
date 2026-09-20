using System;
using System.Reflection;
using UnityEngine;

namespace LazyBearTechnology;

[Serializable]
public class LazyUITesterMethodData
{
	[SerializeField]
	private string windowTypeStr;

	[SerializeField]
	private string methodStr;

	public LazyUITesterMethodData(Type type, MethodInfo methodInfo)
	{
		windowTypeStr = type.AssemblyQualifiedName;
		methodStr = methodInfo.Name;
	}

	public LazyUITesterMethodData(string json)
	{
		JsonUtility.FromJsonOverwrite(json, this);
	}

	public string ToJson()
	{
		return JsonUtility.ToJson(this);
	}

	public void Invoke()
	{
		Type type = Type.GetType(windowTypeStr);
		if (type == null)
		{
			Debug.LogError("windowType for windowTypeStr:[" + windowTypeStr + "] is null!!!");
			return;
		}
		MethodInfo method = typeof(LazyUI).GetMethod("GetWindow", BindingFlags.Static | BindingFlags.Public);
		if (method == null)
		{
			Debug.LogError("LazyUI.GetWindow<T>() method not found.");
			return;
		}
		LazyWidgetBase lazyWidgetBase = method.MakeGenericMethod(type).Invoke(null, null) as LazyWidgetBase;
		if (lazyWidgetBase == null)
		{
			Debug.LogError($"Can't get window of type {type} via LazyUI.GetWindow<T>().");
			return;
		}
		Debug.Log("[LazyUITester] windowTypeStr: " + windowTypeStr);
		Debug.Log($"[LazyUITester] Resolved Type: {type}");
		Debug.Log("[LazyUITester] Type Name: " + type?.Name);
		Debug.Log("[LazyUITester] w Name: " + lazyWidgetBase?.name);
		Debug.Log("[LazyUITester] methodStr: " + methodStr);
		MethodInfo method2 = type.GetMethod(methodStr, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
		if (method2 == null)
		{
			Debug.LogError($"Method '{methodStr}' not found on type {type}");
		}
		else
		{
			method2.Invoke(lazyWidgetBase, null);
		}
	}
}
