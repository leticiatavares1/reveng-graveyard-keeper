using UnityEngine;

public static class ComparisonExtensions
{
	public static bool EqualsTo(this float a, float b, float epsilon = 1E-05f)
	{
		return Mathf.Abs(a - b) < epsilon;
	}

	public static bool EqualsOrMore(this float a, float b, float epsilon = 1E-05f)
	{
		if (Mathf.Abs(a - b) < epsilon)
		{
			return true;
		}
		return a > b;
	}

	public static bool EqualsOrLess(this float a, float b, float epsilon = 1E-05f)
	{
		if (Mathf.Abs(a - b) < epsilon)
		{
			return true;
		}
		return a < b;
	}

	public static bool More(this float a, float b, float epsilon = 1E-05f)
	{
		if (Mathf.Abs(a - b) < epsilon)
		{
			return false;
		}
		return a > b;
	}

	public static bool Less(this float a, float b, float epsilon = 1E-05f)
	{
		if (Mathf.Abs(a - b) < epsilon)
		{
			return false;
		}
		return a < b;
	}

	public static bool EqualsTo(this Vector2 a, Vector2 b, float epsilon = 1E-05f)
	{
		if (a.x.EqualsTo(b.x, epsilon))
		{
			return a.y.EqualsTo(b.y, epsilon);
		}
		return false;
	}
}
