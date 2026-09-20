using UnityEngine;

namespace NGTools;

public static class UnityExtension
{
	public static T GetIComponent<T>(this GameObject go)
	{
		return go.GetComponent<T>();
	}

	public static T[] GetIComponents<T>(this GameObject go)
	{
		return go.GetComponents<T>();
	}
}
