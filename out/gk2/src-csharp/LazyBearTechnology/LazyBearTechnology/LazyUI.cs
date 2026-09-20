using System;
using System.Collections.Generic;
using LinqTools;
using UnityEngine;
using UnityEngine.UI;

namespace LazyBearTechnology;

public static class LazyUI
{
	private static CanvasScaler canvasScaler;

	private static Dictionary<Type, LazyWidgetBase> windowsCache = new Dictionary<Type, LazyWidgetBase>();

	private static HashSet<LazyWidgetBase> windowsPendingRemoval = new HashSet<LazyWidgetBase>();

	private static Rect safeZones = Screen.safeArea;

	private static Dictionary<Type, ILazyGUIElement> guiElementsDictionary;

	private static Func<string, LazyWidgetBase> loadWindowActionIfNotFound;

	public static float ScaleFactor { get; private set; }

	public static bool IsInitialized => canvasScaler != null;

	public static void Init(CanvasScaler canvasScaler = null)
	{
		if (IsInitialized)
		{
			Debug.LogWarning("LazyUI.Init() called for the second time.");
		}
		if (!canvasScaler)
		{
			canvasScaler = UnityEngine.Object.FindObjectOfType<CanvasScaler>(includeInactive: true);
			if (!canvasScaler)
			{
				Debug.LogError("Error in LazyUI.Init(): CanvasScaler is not found on the scene.");
			}
		}
		LazyUI.canvasScaler = canvasScaler;
		LazyWindowsStackController.OnWindowClosed -= OnWindowClosed;
		LazyWindowsStackController.OnWindowClosed += OnWindowClosed;
		List<ILazyGUIElement> list = new List<ILazyGUIElement>(UnityEngine.Object.FindObjectsOfType<MonoBehaviour>(includeInactive: true).OfType<ILazyGUIElement>());
		guiElementsDictionary = new Dictionary<Type, ILazyGUIElement>();
		list.ForEach(delegate(ILazyGUIElement uiElement)
		{
			uiElement.Init();
			TryAddGUIElement(uiElement);
		});
		foreach (LazyWidgetBase item in FindWidgetsOfGenericType(typeof(LazyWidget<>)))
		{
			Type type = item.GetType();
			if (!(type != typeof(LazyWindow<>)))
			{
				windowsCache.Add(type, item);
			}
		}
	}

	public static List<LazyWidgetBase> FindWidgetsOfGenericType(Type genericType)
	{
		List<LazyWidgetBase> list = new List<LazyWidgetBase>();
		LazyWidgetBase[] array = UnityEngine.Object.FindObjectsOfType<LazyWidgetBase>(includeInactive: true);
		foreach (LazyWidgetBase lazyWidgetBase in array)
		{
			Type type = lazyWidgetBase.GetType();
			while (type != null && type != typeof(object))
			{
				if (type.IsGenericType && type.GetGenericTypeDefinition() == genericType)
				{
					list.Add(lazyWidgetBase);
					break;
				}
				type = type.BaseType;
			}
		}
		return list;
	}

	private static bool AssertInitialized()
	{
		if (!IsInitialized)
		{
			Debug.LogError("You should call LazyUI.Init() before calling this method.");
			return true;
		}
		return false;
	}

	public static void SetCanvasScaleFactor(float scaleFactor)
	{
		if (!AssertInitialized())
		{
			float scaleFactor2 = (canvasScaler.scaleFactor = scaleFactor);
			ScaleFactor = scaleFactor2;
		}
	}

	public static Bounds GetScreenBounds()
	{
		return new Bounds((Vector3)safeZones.center, (Vector3)safeZones.size);
	}

	public static void SetSafeZones(Rect rect)
	{
		safeZones = rect;
	}

	public static void SetWindowLoadAction(Func<string, LazyWidgetBase> action)
	{
		loadWindowActionIfNotFound = action;
	}

	public static void ClearWindowsFromCache(List<string> windows)
	{
		foreach (Type item in windowsCache.Keys.Where((Type t) => windows.Contains(t.Name)).ToList())
		{
			LazyWidgetBase lazyWidgetBase = windowsCache[item];
			if (LazyWindowsStackController.IsWindowOpened(lazyWidgetBase))
			{
				windowsPendingRemoval.Add(lazyWidgetBase);
				continue;
			}
			UnityEngine.Object.Destroy(lazyWidgetBase.gameObject);
			windowsCache.Remove(item);
		}
	}

	private static void OnWindowClosed(LazyWidgetBase window)
	{
		if (windowsPendingRemoval.Contains(window))
		{
			Type type = window.GetType();
			if (windowsCache.TryGetValue(type, out var value) && value == window)
			{
				windowsCache.Remove(type);
			}
			DestroyPendingWindow(window);
		}
	}

	private static void DestroyPendingWindow(LazyWidgetBase window)
	{
		if (!(window == null))
		{
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(window.gameObject);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(window.gameObject);
			}
		}
	}

	public static bool TryAddGUIElement(ILazyGUIElement newElement)
	{
		if (!guiElementsDictionary.TryAdd(newElement.GetType(), newElement))
		{
			Debug.Log($"GUI with type [{newElement.GetType()}] alreadyExist");
			return false;
		}
		return true;
	}

	public static T GetElement<T>() where T : ILazyGUIElement
	{
		if (!guiElementsDictionary.TryGetValue(typeof(T), out var value))
		{
			throw new Exception($"no such type gui element: {typeof(T)}");
		}
		return (T)value;
	}

	public static T Get<T>() where T : ILazyGUIElement
	{
		return GetElement<T>();
	}

	public static T GetWindow<T>() where T : LazyWidgetBase
	{
		Type typeFromHandle = typeof(T);
		if (windowsCache.TryGetValue(typeFromHandle, out var value))
		{
			return (T)value;
		}
		T val = UnityEngine.Object.FindObjectsOfType<T>(includeInactive: true).FirstOrDefault((T window) => !windowsPendingRemoval.Contains(window));
		if (!val)
		{
			if (loadWindowActionIfNotFound != null)
			{
				val = loadWindowActionIfNotFound?.Invoke(typeFromHandle.Name) as T;
				windowsCache.Add(typeFromHandle, val);
				return val;
			}
			Debug.LogError($"Error in LazyUI.Get<{typeFromHandle}>: Window of this class is not found on the scene.");
			return null;
		}
		windowsCache.Add(typeFromHandle, val);
		return val;
	}

	public static TInterface GetWindowByInterface<TInterface>(string windowTypeName) where TInterface : class
	{
		Type typeFromHandle = typeof(TInterface);
		if (!typeFromHandle.IsInterface)
		{
			Debug.LogError("Type " + typeFromHandle.Name + " is not an interface");
			return null;
		}
		Type type = Type.GetType(windowTypeName);
		if (type != null && windowsCache.TryGetValue(type, out var value) && typeFromHandle.IsAssignableFrom(type))
		{
			return value as TInterface;
		}
		LazyWidgetBase[] array = UnityEngine.Object.FindObjectsOfType<LazyWidgetBase>(includeInactive: true);
		foreach (LazyWidgetBase lazyWidgetBase in array)
		{
			if (!windowsPendingRemoval.Contains(lazyWidgetBase))
			{
				type = lazyWidgetBase.GetType();
				if (type.Name == windowTypeName && typeFromHandle.IsAssignableFrom(type))
				{
					windowsCache[type] = lazyWidgetBase;
					return lazyWidgetBase as TInterface;
				}
			}
		}
		if (loadWindowActionIfNotFound != null)
		{
			TInterface val = loadWindowActionIfNotFound?.Invoke(windowTypeName) as TInterface;
			windowsCache.Add(type, val as LazyWidgetBase);
			return val;
		}
		Debug.LogWarning("No window of type '" + windowTypeName + "' implementing interface '" + typeFromHandle.Name + "' was found.");
		return null;
	}

	public static List<LazyWidgetBase> GetAllWindows()
	{
		List<LazyWidgetBase> list = FindWidgetsOfGenericType(typeof(LazyWindow<>));
		foreach (LazyWidgetBase value in windowsCache.Values)
		{
			if (value != null && !list.Contains(value))
			{
				list.Add(value);
			}
		}
		return list;
	}
}
