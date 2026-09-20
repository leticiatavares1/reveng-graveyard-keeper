using System.Collections;
using UnityEngine;

namespace LazyBearTechnology;

public class CoroutineManager : MonoBehaviour
{
	private static CoroutineManager instance;

	private static CoroutineManager Instance
	{
		get
		{
			if (instance == null)
			{
				instance = Object.FindObjectOfType<CoroutineManager>();
				if (instance == null)
				{
					GameObject obj = new GameObject(typeof(CoroutineManager).ToString());
					instance = obj.AddComponent<CoroutineManager>();
					Object.DontDestroyOnLoad(obj);
				}
			}
			return instance;
		}
	}

	public static Coroutine StartEnumerator(IEnumerator enumerator)
	{
		return Instance.StartCoroutine(enumerator);
	}

	public static void StopEnumerator(IEnumerator enumerator)
	{
		Instance.StopCoroutine(enumerator);
	}

	public static void StopAll()
	{
		Instance.StopAllCoroutines();
	}
}
