using UnityEngine;

public static class D
{
	public static void Log(string log)
	{
		Debug.Log("#inner# " + log);
	}

	public static void LogWarning(string log)
	{
		Debug.LogWarning(log);
	}

	public static void LogError(string log)
	{
		Debug.LogError(log);
	}

	public static void LogColor(string log, string color)
	{
		Debug.Log(log);
	}

	public static void LogColor(string log, string color, Object context)
	{
		Debug.Log(log, context);
	}
}
