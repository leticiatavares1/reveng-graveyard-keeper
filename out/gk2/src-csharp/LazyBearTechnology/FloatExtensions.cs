using System;
using UnityEngine;

public static class FloatExtensions
{
	public static int Round05(this float value)
	{
		if (value % 0.5f == 0f)
		{
			return Mathf.CeilToInt(value);
		}
		return Mathf.RoundToInt(value);
	}

	public static bool IsInteger(this float value)
	{
		return Math.Abs(value - Mathf.Round(value)) < 0.0001f;
	}
}
