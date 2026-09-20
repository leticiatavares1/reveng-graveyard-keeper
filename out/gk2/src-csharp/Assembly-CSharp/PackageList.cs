using System.Collections;
using System.Collections.Generic;

public class PackageList<T> : IEnumerable<T>, IEnumerable where T : CommandPackage
{
	private readonly FixedSizeQueue<T> items;

	private Dictionary<ulong, T> idsToItems;

	private ulong lastSentPackage;

	private ulong oldestSentPackage;

	public int Count => items.Count;

	public PackageList(int packageQueueMaxSize)
	{
		items = new FixedSizeQueue<T>(packageQueueMaxSize);
		idsToItems = new Dictionary<ulong, T>();
	}

	public bool TryAdd(T item)
	{
		if (items.Count == items.MaxSize)
		{
			if (oldestSentPackage == 0L)
			{
				return false;
			}
			T val = items.Dequeue();
			idsToItems.Remove(val.packageId);
			if (lastSentPackage == val.packageId)
			{
				lastSentPackage = 0uL;
			}
			oldestSentPackage = ((lastSentPackage == 0L) ? 0 : items.Peek().packageId);
		}
		items.Enqueue(item);
		idsToItems.Add(item.packageId, item);
		return true;
	}

	public bool TryGetForSending(out T item)
	{
		item = null;
		if (items.Count == 0)
		{
			return false;
		}
		if (lastSentPackage == items.Last().packageId)
		{
			return false;
		}
		item = idsToItems[(lastSentPackage == 0L) ? items.Last().packageId : (lastSentPackage + 1)];
		if (lastSentPackage == 0L)
		{
			lastSentPackage = items.Last().packageId;
		}
		else
		{
			lastSentPackage++;
		}
		if (oldestSentPackage == 0L)
		{
			oldestSentPackage = lastSentPackage;
		}
		return true;
	}

	public bool TryGetById(ulong packageId, out T package)
	{
		package = null;
		return idsToItems.TryGetValue(packageId, out package);
	}

	public IEnumerator<T> GetEnumerator()
	{
		return items.GetEnumerator();
	}

	public override string ToString()
	{
		string text = "Package Queue:";
		int num = 0;
		foreach (T item in items)
		{
			text += $"\n[{num}]: Package #{item}; Sent = {item.packageId <= lastSentPackage}";
		}
		return text;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
