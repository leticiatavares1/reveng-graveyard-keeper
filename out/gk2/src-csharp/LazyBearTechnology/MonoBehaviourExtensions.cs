using UnityEngine;

public static class MonoBehaviourExtensions
{
	public static T Copy<T>(this T source, Transform parent = null, bool activate = true, string name = "") where T : MonoBehaviour
	{
		if (source == null)
		{
			Debug.LogError("Copy Method error, prefab is null");
			return null;
		}
		T val = Object.Instantiate(source, parent ?? source.transform.parent, worldPositionStays: false);
		if (!string.IsNullOrEmpty(name))
		{
			val.name = name;
		}
		val.gameObject.SetActive(activate);
		return val;
	}

	public static T GetComponentInParentExcludeCurrent<T>(this Component component, bool includeInactive) where T : Component
	{
		T result = null;
		if (component.transform.parent == null)
		{
			return result;
		}
		return component.transform.parent.GetComponentInParent<T>(includeInactive);
	}

	public static bool HasComponentInParentExcludeCurrent<T>(this Component component, bool includeInactive) where T : Component
	{
		if (component.transform.parent == null)
		{
			return false;
		}
		return component.transform.parent.GetComponentInParent<T>(includeInactive) != null;
	}
}
