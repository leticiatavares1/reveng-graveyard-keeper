using System;
using UnityEngine;

namespace DungeonGenerator;

[Serializable]
public class IntVector2
{
	[SerializeField]
	public int x;

	[SerializeField]
	public int y;

	public IntVector2()
	{
		x = (y = 0);
	}

	public IntVector2(int t_x = 0, int t_y = 0)
	{
		x = t_x;
		y = t_y;
	}

	public IntVector2(Vector2 v)
	{
		x = Mathf.RoundToInt(v.x);
		y = Mathf.RoundToInt(v.y);
	}

	public static IntVector2 operator +(IntVector2 left, IntVector2 right)
	{
		return new IntVector2(left.x + right.x, left.y + right.y);
	}

	public IntVector2 Copy()
	{
		return new IntVector2(x, y);
	}

	public override string ToString()
	{
		return "[" + x + ", " + y + "]";
	}

	public Vector2 ToVector2()
	{
		return new Vector2(x, y);
	}

	public bool EqualsTo(IntVector2 v)
	{
		if (v.x == x)
		{
			return v.y == y;
		}
		return false;
	}
}
