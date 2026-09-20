using System;
using System.Collections.Generic;
using UnityEngine;

namespace LazyBearTechnology;

[Serializable]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
	[SerializeField]
	private List<TKey> serKeys = new List<TKey>();

	[SerializeField]
	private List<TValue> serValues = new List<TValue>();

	public void OnBeforeSerialize()
	{
		serKeys.Clear();
		serValues.Clear();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<TKey, TValue> current = enumerator.Current;
			serKeys.Add(current.Key);
			serValues.Add(current.Value);
		}
	}

	public void OnAfterDeserialize()
	{
		Clear();
		for (int i = 0; i < serKeys.Count; i++)
		{
			Add(serKeys[i], serValues[i]);
		}
	}
}
