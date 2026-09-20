using System;
using UnityEngine;

namespace LazyBearTechnology;

public static class MathUtilities
{
	public static Vector2 RadianToVector2(float radian)
	{
		return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
	}

	public static Vector2 AngleToVector2(float angle)
	{
		return RadianToVector2(angle * (MathF.PI / 180f));
	}

	public static int ClampCycle(int value, int min, int max)
	{
		if (value > max)
		{
			return min;
		}
		if (value < min)
		{
			return max;
		}
		return value;
	}

	public static float ClampCycle(float value, float min, float max)
	{
		if (value > max)
		{
			return min;
		}
		if (value < min)
		{
			return max;
		}
		return value;
	}

	public static float ClampCycleWithinRange(int value, int min, int max)
	{
		if (value > max)
		{
			return min + Mathf.Clamp(value - max, min, max);
		}
		if (value < min)
		{
			return max - Mathf.Clamp(min - value, min, max);
		}
		return value;
	}

	public static float ClampCycleWithinRange(float value, float min, float max)
	{
		if (value > max)
		{
			return min + Mathf.Clamp(value - max, min, max);
		}
		if (value < min)
		{
			return max - Mathf.Clamp(min - value, min, max);
		}
		return value;
	}

	public static Vector2 Clamp(this Vector2 value, Vector2 min, Vector2 max)
	{
		return new Vector2(Mathf.Clamp(value.x, min.x, max.x), Mathf.Clamp(value.y, min.y, max.y));
	}
}
