using System;
using System.Collections.Generic;
using UnityEngine;

public static class WgoCustomComponentSerializer
{
	public static void SaveToData(Wgo wgo, WgoData wgoData)
	{
		if (wgo == null || wgoData == null)
		{
			return;
		}
		List<WgoCustomComponentData> customComponentsData = wgoData.customComponentsData;
		customComponentsData.Clear();
		MonoBehaviour[] componentsInChildren = wgo.GetComponentsInChildren<MonoBehaviour>();
		foreach (MonoBehaviour monoBehaviour in componentsInChildren)
		{
			if (monoBehaviour == null || monoBehaviour is Wgo)
			{
				continue;
			}
			Type type = monoBehaviour.GetType();
			Type customComponentInterface = GetCustomComponentInterface(type);
			if (customComponentInterface == null)
			{
				continue;
			}
			try
			{
				object obj = customComponentInterface.GetMethod("OnSave").Invoke(monoBehaviour, null);
				if (obj != null)
				{
					string text = JsonUtility.ToJson(obj, prettyPrint: true);
					if (!string.IsNullOrEmpty(text) && !(text == "{}"))
					{
						customComponentsData.Add(new WgoCustomComponentData
						{
							typeName = type.AssemblyQualifiedName,
							json = text
						});
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("[WgoCustomComponentSerializer] SaveToData failed for " + type.Name + ": " + ex.Message, wgo);
			}
		}
	}

	public static void RestoreComponents(Wgo wgo, WgoData wgoData)
	{
		if (wgo == null || wgoData == null)
		{
			return;
		}
		foreach (WgoCustomComponentData customComponentsDatum in wgoData.customComponentsData)
		{
			Type type = Type.GetType(customComponentsDatum.typeName);
			if (type == null)
			{
				continue;
			}
			MonoBehaviour monoBehaviour = wgo.GetComponentInChildren(type, includeInactive: true) as MonoBehaviour;
			if (monoBehaviour == null)
			{
				Debug.LogError("[WgoCustomComponentSerializer] Component not found: " + type.Name + ", for wgo: " + wgo.Id, wgo);
				continue;
			}
			Type customComponentInterface = GetCustomComponentInterface(type);
			if (!(customComponentInterface == null))
			{
				try
				{
					Type type2 = customComponentInterface.GetGenericArguments()[0];
					object obj = JsonUtility.FromJson(customComponentsDatum.json, type2);
					customComponentInterface.GetMethod("OnLoad").Invoke(monoBehaviour, new object[1] { obj });
				}
				catch (Exception ex)
				{
					Debug.LogError("[WgoCustomComponentSerializer] Deserialize failed for " + type.Name + ": " + ex.Message, wgo);
				}
			}
		}
	}

	public static void ResetComponents(Wgo wgo)
	{
		if (wgo == null)
		{
			return;
		}
		MonoBehaviour[] componentsInChildren = wgo.GetComponentsInChildren<MonoBehaviour>(includeInactive: true);
		foreach (MonoBehaviour monoBehaviour in componentsInChildren)
		{
			if (monoBehaviour == null || monoBehaviour is Wgo)
			{
				continue;
			}
			Type type = monoBehaviour.GetType();
			Type customComponentInterface = GetCustomComponentInterface(type);
			if (!(customComponentInterface == null))
			{
				try
				{
					customComponentInterface.GetMethod("OnUnload").Invoke(monoBehaviour, null);
				}
				catch (Exception ex)
				{
					Debug.LogError("[WgoCustomComponentSerializer] Unload failed for " + type.Name + ": " + ex.Message, wgo);
				}
			}
		}
	}

	private static Type GetCustomComponentInterface(Type compType)
	{
		Type[] interfaces = compType.GetInterfaces();
		foreach (Type type in interfaces)
		{
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IWgoCustomComponent<>))
			{
				return type;
			}
		}
		return null;
	}
}
