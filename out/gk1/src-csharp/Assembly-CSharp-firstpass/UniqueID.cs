using UnityEngine;

public static class UniqueID
{
	private static long _unique_id;

	public static long GetUniqueID()
	{
		long num = ++_unique_id;
		if (num == -1 || num == 0L)
		{
			num = GetUniqueID();
		}
		return num;
	}

	public static void SetIterator(long n)
	{
		_unique_id = n;
		Debug.Log("Setting UniqueID = " + n);
	}
}
