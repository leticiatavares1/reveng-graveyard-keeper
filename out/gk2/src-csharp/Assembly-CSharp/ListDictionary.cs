using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ListDictionary<T> : ISerializationCallbackReceiver where T : IEquatable<T>
{
	[SerializeField]
	private List<T> list = new List<T>();

	private Dictionary<T, int> dictionary = new Dictionary<T, int>();

	public int Count => list.Count;

	public T this[int index]
	{
		get
		{
			return list[index];
		}
		set
		{
			list[index] = value;
			dictionary[value] = index;
		}
	}

	public bool Contains(T item)
	{
		return dictionary.ContainsKey(item);
	}

	public int IndexOf(T item)
	{
		if (!dictionary.ContainsKey(item))
		{
			return -1;
		}
		return dictionary[item];
	}

	public bool TryAdd(T item)
	{
		if (dictionary.ContainsKey(item))
		{
			return false;
		}
		int count = list.Count;
		list.Add(item);
		dictionary[item] = count;
		return true;
	}

	public bool Remove(T item)
	{
		if (!dictionary.ContainsKey(item))
		{
			return false;
		}
		int index = dictionary[item];
		dictionary.Remove(item);
		list.RemoveAt(index);
		return true;
	}

	public void RemoveAt(int index)
	{
		T key = list[index];
		list.RemoveAt(index);
		dictionary.Remove(key);
	}

	public void Clear()
	{
		list.Clear();
		dictionary.Clear();
	}

	public IEnumerable<T> IterateByList()
	{
		return list;
	}

	public IEnumerable<KeyValuePair<T, int>> IterateByDictionary()
	{
		return dictionary;
	}

	public override string ToString()
	{
		return string.Format("ListDictionary<{0}>({1}): {2}", typeof(T), list.Count, string.Join(",", list));
	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
		dictionary = new Dictionary<T, int>();
		for (int i = 0; i < list.Count; i++)
		{
			dictionary[list[i]] = i;
		}
	}

	public T[] ToArray()
	{
		return list.ToArray();
	}
}
