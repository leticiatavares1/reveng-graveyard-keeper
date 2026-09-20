using UnityEngine;

public static class VecTools
{
	public static Direction ToDirection(this Vector2 vec)
	{
		if (vec.magnitude.EqualsTo(0f))
		{
			return Direction.None;
		}
		if (Mathf.Abs(vec.x) > Mathf.Abs(vec.y))
		{
			if (!(vec.x < 0f))
			{
				return Direction.Right;
			}
			return Direction.Left;
		}
		if (!(vec.y < 0f))
		{
			return Direction.Up;
		}
		return Direction.Down;
	}

	public static Vector2 ToVec(this Direction dir)
	{
		return dir switch
		{
			Direction.Right => Vector2.right, 
			Direction.Up => Vector2.up, 
			Direction.Left => Vector2.left, 
			Direction.Down => Vector2.down, 
			_ => Vector2.zero, 
		};
	}

	public static Vector3 ToVec3(this Direction dir)
	{
		return dir switch
		{
			Direction.Right => Vector3.right, 
			Direction.Up => Vector3.up, 
			Direction.Left => Vector3.left, 
			Direction.Down => Vector3.down, 
			_ => Vector3.zero, 
		};
	}

	public static Vector3 ToRotaionVector(this Direction dir)
	{
		return dir switch
		{
			Direction.Right => new Vector3(0f, 0f, -90f), 
			Direction.Up => new Vector3(0f, 0f, 0f), 
			Direction.Left => new Vector3(0f, 0f, 90f), 
			Direction.Down => new Vector3(0f, 0f, 180f), 
			_ => Vector3.zero, 
		};
	}

	public static Direction ClockwiseDir(this Direction dir)
	{
		return dir switch
		{
			Direction.Right => Direction.Down, 
			Direction.Up => Direction.Right, 
			Direction.Left => Direction.Up, 
			Direction.Down => Direction.Left, 
			_ => Direction.None, 
		};
	}

	public static Direction Opposite(this Direction dir)
	{
		return dir switch
		{
			Direction.Right => Direction.Left, 
			Direction.Up => Direction.Down, 
			Direction.Left => Direction.Right, 
			Direction.Down => Direction.Up, 
			_ => Direction.None, 
		};
	}

	public static float DistSqrTo(this Vector3 from, Vector3 to)
	{
		from -= to;
		return from.x * from.x + from.y * from.y;
	}

	public static float DistSqrTo(this Vector3 from, Vector3 to, float scale)
	{
		from = (from - to) / scale;
		return from.x * from.x + from.y * from.y;
	}

	public static float DistTo(this Vector3 from, Vector3 to)
	{
		return Mathf.Sqrt(from.DistSqrTo(to));
	}

	public static float GridDistTo(this Vector2 from, Vector2 to, float grid_size = 96f)
	{
		return (from - to).magnitude / grid_size;
	}

	public static Vector2 DirTo(this Transform from_tf, Transform to_tf, float grid_size = 96f)
	{
		if (from_tf == null || to_tf == null)
		{
			return Vector2.zero;
		}
		return (to_tf.position - from_tf.position) / grid_size;
	}

	public static Vector2 DirTo(this Vector2 from, Vector2 to, float grid_size = 96f)
	{
		return (to - from) / grid_size;
	}

	public static Vector2 GetRandomNormalizedVec2()
	{
		return new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
	}

	public static Vector3 GetRandomNormalizedVec3()
	{
		return new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
	}

	public static float Atan2(this Vector2 v)
	{
		return Mathf.Atan2(v.y, v.x);
	}
}
