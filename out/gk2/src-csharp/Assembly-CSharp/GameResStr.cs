using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[Serializable]
public class GameResStr
{
	[SerializeField]
	private List<string> keys = new List<string>();

	[SerializeField]
	private List<string> values = new List<string>();

	public List<string> Keys => keys;

	public List<string> Values => values;

	public GameResStr()
	{
	}

	public GameResStr(GameResStr other)
	{
		for (int i = 0; i < other.keys.Count; i++)
		{
			Set(other.keys[i], other.values[i]);
		}
	}

	public GameResStr(string key, string value)
	{
		Set(key, value);
	}

	public string Get(string key, string defaultValue = "")
	{
		int num = keys.IndexOf(key);
		if (num != -1)
		{
			return values[num];
		}
		return defaultValue;
	}

	public string GetKeyByValue(string value, string emptyKey = "")
	{
		int num = values.IndexOf(value);
		if (num != -1)
		{
			return keys[num];
		}
		return emptyKey;
	}

	public bool Has(string key)
	{
		return keys.Contains(key);
	}

	public void Clear()
	{
		keys.Clear();
		values.Clear();
	}

	public void Set(string key, string value)
	{
		int num = keys.IndexOf(key);
		if (num != -1)
		{
			values[num] = value;
			return;
		}
		keys.Add(key);
		values.Add(value);
	}

	public void Set(GameResStr gameRes)
	{
		int count = gameRes.Keys.Count;
		for (int i = 0; i < count; i++)
		{
			Set(gameRes.Keys[i], gameRes.values[i]);
		}
	}

	public void Remove(string key)
	{
		int num = keys.IndexOf(key);
		if (num != -1)
		{
			keys.RemoveAt(num);
			values.RemoveAt(num);
		}
	}

	public bool IsEmpty()
	{
		return keys.Count == 0;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder("[GameResStr: ");
		for (int i = 0; i < keys.Count; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append(", ");
			}
			stringBuilder.Append(keys[i]);
			stringBuilder.Append("=");
			stringBuilder.Append(values[i]);
		}
		stringBuilder.Append("]");
		return stringBuilder.ToString();
	}

	public override bool Equals(object obj)
	{
		if (!(obj is GameResStr gameResStr))
		{
			return false;
		}
		if (keys.Count != gameResStr.keys.Count)
		{
			return false;
		}
		for (int i = 0; i < keys.Count; i++)
		{
			if (keys[i] != gameResStr.keys[i] || values[i] != gameResStr.values[i])
			{
				return false;
			}
		}
		return true;
	}

	public override int GetHashCode()
	{
		return (-1419608871 * -1521134295 + EqualityComparer<List<string>>.Default.GetHashCode(keys)) * -1521134295 + EqualityComparer<List<string>>.Default.GetHashCode(values);
	}
}
