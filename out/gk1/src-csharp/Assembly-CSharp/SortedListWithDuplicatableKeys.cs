using System.Collections.Generic;

public class SortedListWithDuplicatableKeys<T>
{
	public List<int> keys = new List<int>();

	public List<T> values = new List<T>();

	public int Count => keys.Count;

	public void Insert(int key, T value)
	{
		if (keys.Count == 0)
		{
			keys.Add(key);
			values.Add(value);
			return;
		}
		int i;
		for (i = 0; i < keys.Count && keys[i] <= key; i++)
		{
		}
		if (i == keys.Count)
		{
			keys.Add(key);
			values.Add(value);
		}
		else
		{
			keys.Insert(i, key);
			values.Insert(i, value);
		}
	}
}
