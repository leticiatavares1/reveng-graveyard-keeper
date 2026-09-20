using System;
using System.Collections;
using System.Collections.Generic;

public class FixedSizeQueue<T> : IEnumerable<T>, IEnumerable
{
	private readonly LinkedList<T> items = new LinkedList<T>();

	private readonly int maxSize;

	public int Count => items.Count;

	public int MaxSize => maxSize;

	public FixedSizeQueue(int maxSize)
	{
		if (maxSize <= 0)
		{
			throw new ArgumentOutOfRangeException("maxSize", "Max size must be greater than zero.");
		}
		this.maxSize = maxSize;
	}

	public void Enqueue(T item)
	{
		if (items.Count == maxSize)
		{
			items.RemoveFirst();
		}
		items.AddLast(item);
	}

	public T Dequeue()
	{
		if (items.Count == 0)
		{
			throw new InvalidOperationException("The queue is empty.");
		}
		T value = items.First.Value;
		items.RemoveFirst();
		return value;
	}

	public T Peek()
	{
		if (items.Count == 0)
		{
			throw new InvalidOperationException("The queue is empty.");
		}
		return items.First.Value;
	}

	public T Last()
	{
		if (items.Count == 0)
		{
			throw new InvalidOperationException("The queue is empty.");
		}
		return items.Last.Value;
	}

	public IEnumerator<T> GetEnumerator()
	{
		return items.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
