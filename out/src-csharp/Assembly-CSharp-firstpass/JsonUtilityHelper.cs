using System.Collections.Generic;
using UnityEngine;

public static class JsonUtilityHelper
{
	public class JsonList<T>
	{
		public List<T> list;
	}

	public static string ToJsonList<T>(List<T> list)
	{
		return JsonUtility.ToJson(new JsonList<T>
		{
			list = list
		});
	}

	public static List<T> FromJsonList<T>(string json)
	{
		return JsonUtility.FromJson<JsonList<T>>(json).list;
	}
}
