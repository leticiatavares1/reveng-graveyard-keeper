using System.Collections.Generic;
using UnityEngine;

namespace LazyBearTechnology;

public class LazyUIElementManager : MonoBehaviour
{
	private static LazyUIElementManager instance;

	public List<ILazyUIElementWithId> elements = new List<ILazyUIElementWithId>();

	private static LazyUIElementManager Instance
	{
		get
		{
			if (instance == null)
			{
				instance = Object.FindObjectOfType<LazyUIElementManager>();
				if (instance == null)
				{
					Debug.LogError("LazyUIElementManager.Instance error: Couldn't find a LazyUIElementManager object.");
				}
			}
			return instance;
		}
	}

	private void Awake()
	{
		instance = this;
	}

	public static bool TryRegisterElement(ILazyUIElementWithId elementWithId)
	{
		if (string.IsNullOrEmpty(elementWithId.LazyUIElementId))
		{
			Debug.LogWarning("#lazy_ui# Trying to register elementWithId with empty id!");
		}
		foreach (ILazyUIElementWithId element in Instance.elements)
		{
			if (element != elementWithId && element.LazyUIElementId == elementWithId.LazyUIElementId)
			{
				Debug.LogWarning("#lazy_ui# Duplicate elementWithId id:[" + elementWithId.LazyUIElementId + "]!");
			}
		}
		if (!Instance.elements.Contains(elementWithId))
		{
			Instance.elements.Add(elementWithId);
			return true;
		}
		return false;
	}

	public static bool TryUnregisterElement(ILazyUIElementWithId elementWithId)
	{
		if (Instance == null)
		{
			return false;
		}
		if (Instance.elements.Contains(elementWithId))
		{
			Instance.elements.Remove(elementWithId);
			return true;
		}
		return false;
	}

	public static bool TryGetById(string id, out ILazyUIElementWithId elementWithId)
	{
		elementWithId = Instance.elements.Find((ILazyUIElementWithId b) => b.LazyUIElementId == id);
		return elementWithId != null;
	}

	public static bool TryGetById<T>(string id, out T element) where T : MonoBehaviour
	{
		element = (TryGetById(id, out var elementWithId) ? elementWithId.MonoBehaviour.GetComponent<T>() : null);
		return element != null;
	}
}
