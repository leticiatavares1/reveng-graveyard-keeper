using System.Collections;
using System.Collections.Generic;

namespace LazyBearTechnology;

public class Dynamic2DArray<T> : IEnumerable<(int x, int y, T value)>, IEnumerable
{
	private readonly Dictionary<int, Dictionary<int, T>> data = new Dictionary<int, Dictionary<int, T>>();

	private int minX;

	private int minY;

	private int maxX;

	private int maxY;

	public int MinX => minX;

	public int MaxX => maxX;

	public int MinY => minY;

	public int MaxY => maxY;

	public T this[int x, int y]
	{
		get
		{
			if (x < minX || y < minY || x > maxX || y > maxY)
			{
				return default(T);
			}
			if (!data.TryGetValue(x, out var value))
			{
				return default(T);
			}
			if (!value.TryGetValue(y, out var value2))
			{
				return default(T);
			}
			return value2;
		}
		set
		{
			if (!data.TryGetValue(x, out var value2))
			{
				value2 = new Dictionary<int, T>();
				data.Add(x, value2);
			}
			if (!value2.TryAdd(y, value))
			{
				value2[y] = value;
			}
			if (x < minX)
			{
				minX = x;
			}
			if (x > maxX)
			{
				maxX = x;
			}
			if (y < minY)
			{
				minY = y;
			}
			if (y > maxY)
			{
				maxY = y;
			}
		}
	}

	public IEnumerator<(int x, int y, T value)> GetEnumerator()
	{
		for (int x = MinX; x <= MaxX; x++)
		{
			if (!data.TryGetValue(x, out var xdata) || xdata == null)
			{
				continue;
			}
			for (int y = MinY; y <= MaxY; y++)
			{
				if (xdata.TryGetValue(y, out var value) && value != null)
				{
					yield return (x: x, y: y, value: value);
				}
			}
			xdata = null;
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Delete(int x, int y)
	{
		if (this[x, y] != null)
		{
			data[x].Remove(y);
		}
	}
}
