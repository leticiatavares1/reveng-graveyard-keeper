using UnityEngine;

namespace LazyBearTechnology;

public static class LazyTime
{
	private static float overrodeUnscaledDeltaTime;

	private static bool isUnscaledDeltaTimeOverrode;

	public static float GetUnscaledDeltaTime
	{
		get
		{
			if (!isUnscaledDeltaTimeOverrode)
			{
				return Time.unscaledDeltaTime;
			}
			return overrodeUnscaledDeltaTime;
		}
	}

	public static void OverrideUnscaledDeltaTime(float value)
	{
		isUnscaledDeltaTimeOverrode = true;
		overrodeUnscaledDeltaTime = value;
	}

	public static void CancelOverrideUnscaledDeltaTime()
	{
		isUnscaledDeltaTimeOverrode = false;
	}
}
