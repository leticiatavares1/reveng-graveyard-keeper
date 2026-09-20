using System;
using UnityEngine;

namespace LazyBearTechnology;

[Serializable]
public struct IntVector2
{
	public int x;

	public int y;

	public int v1
	{
		get
		{
			return x;
		}
		set
		{
			x = value;
		}
	}

	public int v2
	{
		get
		{
			return y;
		}
		set
		{
			y = value;
		}
	}

	public float magnitude => Mathf.Sqrt(x * x + y * y);

	public IntVector2(int x, int y)
	{
		this.x = x;
		this.y = y;
	}

	public IntVector2(float x, float y)
	{
		this.x = Mathf.RoundToInt(x);
		this.y = Mathf.RoundToInt(y);
	}

	public static IntVector2 operator +(IntVector2 a, IntVector2 b)
	{
		return new IntVector2(a.x + b.x, a.y + b.y);
	}

	public static IntVector2 operator -(IntVector2 a, IntVector2 b)
	{
		return new IntVector2(a.x - b.x, a.y - b.y);
	}

	public static IntVector2 operator *(IntVector2 a, IntVector2 b)
	{
		return new IntVector2(a.x * b.x, a.y * b.y);
	}

	public static IntVector2 operator /(IntVector2 a, IntVector2 b)
	{
		return new IntVector2(a.x / b.x, a.y / b.y);
	}

	public static IntVector2 operator +(IntVector2 a, int b)
	{
		return new IntVector2(a.x + b, a.y + b);
	}

	public static IntVector2 operator -(IntVector2 a, int b)
	{
		return new IntVector2(a.x - b, a.y - b);
	}

	public static IntVector2 operator *(IntVector2 a, int b)
	{
		return new IntVector2(a.x * b, a.y * b);
	}

	public static IntVector2 operator /(IntVector2 a, int b)
	{
		return new IntVector2(a.x / b, a.y / b);
	}

	public static bool operator ==(IntVector2 a, IntVector2 b)
	{
		if (a.x == b.x)
		{
			return a.y == b.y;
		}
		return false;
	}

	public static bool operator !=(IntVector2 a, IntVector2 b)
	{
		if (a.x == b.x)
		{
			return a.y != b.y;
		}
		return true;
	}

	public bool Equals(IntVector2 o)
	{
		if (x == o.x)
		{
			return y == o.y;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is IntVector2 o)
		{
			return Equals(o);
		}
		return false;
	}

	public override string ToString()
	{
		return $"({x},{y})";
	}

	public override int GetHashCode()
	{
		return (x * 397) ^ y;
	}

	public static implicit operator Vector2(IntVector2 v)
	{
		return new Vector2(v.x, v.y);
	}

	public static explicit operator IntVector2(Vector2 v)
	{
		return new IntVector2(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y));
	}
}
