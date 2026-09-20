using UnityEngine;

public static class VectorExtensions
{
	public static Vector3 XZ(this Vector3 vector)
	{
		return new Vector3(vector.x, 0f, vector.z);
	}

	public static Vector2 XZ2(this Vector3 vector)
	{
		return new Vector2(vector.x, vector.z);
	}

	public static Vector3 XZ(this Vector2 vector)
	{
		return new Vector3(vector.x, 0f, vector.y);
	}
}
