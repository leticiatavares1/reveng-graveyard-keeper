using FlowCanvas;
using UnityEngine;

public static class MyFlowCanvasExtensions
{
	public static bool HasValue<T>(this ValueInput<T> v)
	{
		if (v.value as GameObject != null || v.type == typeof(GameObject) || v.type == typeof(WorldGameObject))
		{
			if (v.isConnected)
			{
				return v.value != null;
			}
			return false;
		}
		if (!v.isConnected)
		{
			return !v.isDefaultValue;
		}
		return true;
	}
}
