using System.Collections.Generic;
using UnityEngine;

namespace DarkTonic.MasterAudio;

public static class DTMonoHelper
{
	public static Transform GetChildTransform(this Transform transParent, string childName)
	{
		return transParent.Find(childName);
	}

	public static bool IsActive(GameObject go)
	{
		return go.activeInHierarchy;
	}

	public static void SetActive(GameObject go, bool isActive)
	{
		go.SetActive(isActive);
	}

	public static void DestroyAllChildren(this Transform tran)
	{
		List<GameObject> list = new List<GameObject>();
		for (int i = 0; i < tran.childCount; i++)
		{
			list.Add(tran.GetChild(i).gameObject);
		}
		int num = 0;
		while (list.Count > 0 && num < 200)
		{
			GameObject gameObject = list[0];
			Object.Destroy(gameObject);
			if (list[0] == gameObject)
			{
				list.RemoveAt(0);
			}
			num++;
		}
	}
}
