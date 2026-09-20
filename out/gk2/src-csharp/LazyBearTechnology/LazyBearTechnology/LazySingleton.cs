using UnityEngine;

namespace LazyBearTechnology;

public abstract class LazySingleton<T> : MonoBehaviour where T : MonoBehaviour
{
	private static T instance;

	public static T Instance
	{
		get
		{
			if (instance == null)
			{
				instance = Object.FindObjectOfType<T>(includeInactive: true);
				if (instance == null)
				{
					new GameObject($"{typeof(T)} Instance").AddComponent<T>();
				}
			}
			return instance;
		}
	}

	public static void SetReference(T reference)
	{
		instance = reference;
	}

	protected virtual void Awake()
	{
		if (!(instance != null))
		{
			SetReference(this as T);
		}
	}
}
