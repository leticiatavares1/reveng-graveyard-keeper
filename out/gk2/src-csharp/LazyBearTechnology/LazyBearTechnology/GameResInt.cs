using System;
using System.Collections.Generic;
using UnityEngine;

namespace LazyBearTechnology;

[Serializable]
public class GameResInt
{
	[SerializeField]
	private List<string> ids = new List<string>();

	[SerializeField]
	private List<int> amounts = new List<int>();

	public int Get(string id)
	{
		int num = ids.IndexOf(id);
		if (num != -1)
		{
			return amounts[num];
		}
		return 0;
	}

	public void Add(string id, int count = 1)
	{
		int num = ids.IndexOf(id);
		if (num == -1)
		{
			amounts.Add(count);
			ids.Add(id);
		}
		else
		{
			amounts[num] += count;
		}
	}

	public bool Remove(string id, int count = 1)
	{
		int num = ids.IndexOf(id);
		if (num == -1)
		{
			return false;
		}
		int num2 = amounts[num];
		if (count > num2)
		{
			RemoveAll(id);
			return false;
		}
		if (count == num2)
		{
			RemoveAll(id);
			return true;
		}
		amounts[num] = num2 - count;
		return true;
	}

	public void RemoveAll(string id)
	{
		int num = ids.IndexOf(id);
		if (num != -1)
		{
			amounts.RemoveAt(num);
			ids.RemoveAt(num);
		}
	}
}
