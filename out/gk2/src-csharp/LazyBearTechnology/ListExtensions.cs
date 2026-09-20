using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class ListExtensions
{
	public static T GetRandom<T>(this List<T> list)
	{
		int index = UnityEngine.Random.Range(0, list.Count);
		return list[index];
	}

	public static T PopFirst<T>(this List<T> list)
	{
		T result = list[0];
		list.RemoveAt(0);
		return result;
	}

	public static T PopLast<T>(this List<T> list)
	{
		T result = list[list.Count - 1];
		list.RemoveAt(list.Count - 1);
		return result;
	}

	public static T PopRandom<T>(this List<T> list)
	{
		int index = UnityEngine.Random.Range(0, list.Count);
		T val = list[index];
		list.Remove(val);
		return val;
	}

	public static void RemoveUnityNulls<T>(this List<T> list) where T : UnityEngine.Object
	{
		for (int i = 0; i < list.Count; i++)
		{
			if (!(list[i] != null))
			{
				list.RemoveAt(i);
				i--;
			}
		}
	}

	public static void Move<T>(this List<T> list, T item, int newIndex)
	{
		int num = list.IndexOf(item);
		if (num == -1)
		{
			throw new ArgumentException("Item not found in list", "item");
		}
		if (newIndex < 0 || newIndex >= list.Count)
		{
			throw new ArgumentOutOfRangeException("newIndex");
		}
		if (num != newIndex)
		{
			list.RemoveAt(num);
			list.Insert(newIndex, item);
		}
	}

	public static void RemoveNulls<T>(this List<T> list)
	{
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] == null)
			{
				list.RemoveAt(i);
				i--;
			}
		}
	}

	public static string FormatString<T>(this List<T> list, string separator = "")
	{
		if (list.Count == 0)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (T item in list)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(separator);
			}
			stringBuilder.Append(item);
		}
		return stringBuilder.ToString();
	}

	public static void AddIfNotContains<T>(this List<T> list, T item)
	{
		if (!list.Contains(item))
		{
			list.Add(item);
		}
	}

	public static void Shuffle<T>(this IList<T> list)
	{
		int num = list.Count;
		while (num > 1)
		{
			num--;
			int index = UnityEngine.Random.Range(0, num);
			T value = list[index];
			list[index] = list[num];
			list[num] = value;
		}
	}
}
