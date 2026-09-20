using System;
using UnityEngine;

namespace LazyBearTechnology;

[Serializable]
public struct IntVector3
{
	public int x;

	public int y;

	public int z;

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

	public int v3
	{
		get
		{
			return z;
		}
		set
		{
			z = value;
		}
	}

	public float magnitude => Mathf.Sqrt(x * x + y * y + z * z);

	public IntVector3(int x, int y, int z)
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}

	public IntVector3(float x, float y, float z)
	{
		this.x = Mathf.RoundToInt(x);
		this.y = Mathf.RoundToInt(y);
		this.z = Mathf.RoundToInt(z);
	}

	public static IntVector3 operator +(IntVector3 a, IntVector3 b)
	{
		return new IntVector3(a.x + b.x, a.y + b.y, a.z + b.z);
	}

	public static IntVector3 operator -(IntVector3 a, IntVector3 b)
	{
		return new IntVector3(a.x - b.x, a.y - b.y, a.z - b.z);
	}

	public static IntVector3 operator *(IntVector3 a, IntVector3 b)
	{
		return new IntVector3(a.x * b.x, a.y * b.y, a.z * b.z);
	}

	public static IntVector3 operator /(IntVector3 a, IntVector3 b)
	{
		return new IntVector3(a.x / b.x, a.y / b.y, a.z / b.z);
	}

	public static IntVector3 operator +(IntVector3 a, int b)
	{
		return new IntVector3(a.x + b, a.y + b, a.z + b);
	}

	public static IntVector3 operator -(IntVector3 a, int b)
	{
		return new IntVector3(a.x - b, a.y - b, a.z - b);
	}

	public static IntVector3 operator *(IntVector3 a, int b)
	{
		return new IntVector3(a.x * b, a.y * b, a.z * b);
	}

	public static IntVector3 operator /(IntVector3 a, int b)
	{
		return new IntVector3(a.x / b, a.y / b, a.z / b);
	}

	public static bool operator ==(IntVector3 a, IntVector3 b)
	{
		if (a.x == b.x && a.y == b.y)
		{
			return a.z == b.z;
		}
		return false;
	}

	public static bool operator !=(IntVector3 a, IntVector3 b)
	{
		if (a.x == b.x && a.y == b.y)
		{
			return a.z != b.z;
		}
		return true;
	}

	public bool Equals(IntVector3 o)
	{
		if (x == o.x && y == o.y)
		{
			return z == o.z;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is IntVector3 o)
		{
			return Equals(o);
		}
		return false;
	}

	public override string ToString()
	{
		return $"({x},{y},{z})";
	}

	public override int GetHashCode()
	{
		return (x * 397) ^ y ^ (z * 384723);
	}

	public static implicit operator Vector3(IntVector3 v)
	{
		return new Vector3(v.x, v.y, v.z);
	}

	public static explicit operator IntVector3(Vector3 v)
	{
		return new IntVector3(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.z));
	}
}
