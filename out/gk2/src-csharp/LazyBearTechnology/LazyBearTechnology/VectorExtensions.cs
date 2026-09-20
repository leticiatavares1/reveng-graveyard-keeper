using UnityEngine;

namespace LazyBearTechnology;

public static class VectorExtensions
{
	public static Vector2 SetX(this Vector2 v, float newValue)
	{
		return new Vector2(newValue, v.y);
	}

	public static Vector2 SetY(this Vector2 v, float newValue)
	{
		return new Vector2(v.x, newValue);
	}

	public static Vector3 SetX(this Vector3 v, float newValue)
	{
		return new Vector3(newValue, v.y, v.z);
	}

	public static Vector3 SetY(this Vector3 v, float newValue)
	{
		return new Vector3(v.x, newValue, v.z);
	}

	public static Vector3 SetZ(this Vector3 v, float newValue)
	{
		return new Vector3(v.x, v.y, newValue);
	}
}
