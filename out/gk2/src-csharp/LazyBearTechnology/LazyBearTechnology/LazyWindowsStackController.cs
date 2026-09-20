using System;
using System.Collections.Generic;

namespace LazyBearTechnology;

public static class LazyWindowsStackController
{
	public static int WINDOWS_INITIAL_SORTING_ORDER_VALUE = 400;

	private static List<LazyWidgetBase> openedWindowsStack = new List<LazyWidgetBase>();

	private static Dictionary<LazyWidgetBase, bool> windowsWithModalStatus = new Dictionary<LazyWidgetBase, bool>();

	private static LazyWidgetBase activeWindow;

	private static int activeWindowSorting;

	public static bool HasAnyModalWindowOpened { get; private set; }

	public static LazyWidgetBase ActiveWindow => activeWindow;

	public static event Action<LazyWidgetBase> OnWindowOpened;

	public static event Action<LazyWidgetBase> OnWindowClosed;

	public static event Action OnAllWindowsClosed;

	public static event Action<LazyWidgetBase> OnWindowBecameVisibleInStack;

	public static event Action<LazyWidgetBase> OnWindowBecameHiddenInStack;

	public static void AddToStack<T>(LazyWindow<T> window) where T : LazyWidgetDataBase
	{
		if (openedWindowsStack.Count > 0)
		{
			Action<LazyWidgetBase> onWindowBecameHiddenInStack = LazyWindowsStackController.OnWindowBecameHiddenInStack;
			if (onWindowBecameHiddenInStack != null)
			{
				List<LazyWidgetBase> list = openedWindowsStack;
				onWindowBecameHiddenInStack(list[list.Count - 1]);
			}
		}
		openedWindowsStack.Add(window);
		windowsWithModalStatus.TryAdd(window, window.IsModalWindow);
		UpdateModality();
		activeWindow = window;
		LazyWindowsStackController.OnWindowOpened?.Invoke(window);
		LazyWindowsStackController.OnWindowBecameVisibleInStack?.Invoke(window);
		if (openedWindowsStack.Count > 1)
		{
			activeWindowSorting += 15;
		}
		else
		{
			activeWindowSorting = WINDOWS_INITIAL_SORTING_ORDER_VALUE;
		}
		window.Canvas.sortingOrder = activeWindowSorting;
	}

	public static void RemoveFromStack<T>(LazyWindow<T> window) where T : LazyWidgetDataBase
	{
		if (!openedWindowsStack.Contains(window))
		{
			return;
		}
		openedWindowsStack.Remove(window);
		windowsWithModalStatus.Remove(window);
		UpdateModality();
		LazyWindowsStackController.OnWindowClosed?.Invoke(window);
		if (openedWindowsStack.Count > 0)
		{
			List<LazyWidgetBase> list = openedWindowsStack;
			activeWindow = list[list.Count - 1];
			Action<LazyWidgetBase> onWindowBecameVisibleInStack = LazyWindowsStackController.OnWindowBecameVisibleInStack;
			if (onWindowBecameVisibleInStack != null)
			{
				List<LazyWidgetBase> list2 = openedWindowsStack;
				onWindowBecameVisibleInStack(list2[list2.Count - 1]);
			}
		}
		else
		{
			activeWindow = null;
			LazyWindowsStackController.OnAllWindowsClosed?.Invoke();
		}
	}

	public static bool IsWindowOnTop<T>(LazyWindow<T> window) where T : LazyWidgetDataBase
	{
		return activeWindow == window;
	}

	public static bool IsWindowOpened(LazyWidgetBase window)
	{
		return openedWindowsStack.Contains(window);
	}

	private static void UpdateModality()
	{
		foreach (bool value in windowsWithModalStatus.Values)
		{
			if (value)
			{
				HasAnyModalWindowOpened = true;
				return;
			}
		}
		HasAnyModalWindowOpened = false;
	}
}
