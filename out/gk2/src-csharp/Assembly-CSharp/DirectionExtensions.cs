using UnityEngine;

public static class DirectionExtensions
{
	public static Vector3 ConvertToVector3(this Direction direction)
	{
		switch (direction)
		{
		case Direction.Up:
			return Vector3.forward.normalized;
		case Direction.Right:
			return Vector3.right.normalized;
		case Direction.Down:
			return Vector3.back.normalized;
		case Direction.Left:
			return Vector3.left.normalized;
		default:
			Debug.LogWarning($"DirectionExtensions Error: no direction [{direction}] setup.");
			return Vector3.zero;
		}
	}

	public static Direction ConvertFromVector2(this Vector2 direction)
	{
		return BasicNpcSteppedRotationPreset.Instance.ComputeAngle(direction).ConvertFromSignedAngle();
	}

	public static Direction ConvertFromVector3(this Vector3 direction)
	{
		return direction.XZ2().ConvertFromVector2();
	}

	public static Vector2 ConvertToVector2XZ(this Direction direction)
	{
		Vector3 vector = direction.ConvertToVector3();
		return new Vector2(vector.x, vector.z);
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

	public static Direction OppositeDir(this Direction dir)
	{
		return dir switch
		{
			Direction.Right => Direction.Left, 
			Direction.Up => Direction.Down, 
			Direction.Left => Direction.Right, 
			Direction.Down => Direction.Up, 
			_ => dir, 
		};
	}

	public static float ConvertToSignedAngle(this Direction direction)
	{
		switch (direction)
		{
		case Direction.Up:
			return 90f;
		case Direction.Right:
			return 0f;
		case Direction.Down:
			return -90f;
		case Direction.Left:
			return 180f;
		default:
			Debug.LogWarning($"DirectionExtensions Error: no direction [{direction}] setup.");
			return 0f;
		}
	}

	public static Direction ConvertFromSignedAngle(this float angle)
	{
		return BasicNpcSteppedRotationPreset.Instance.GetDirectionFromAngle(angle);
	}

	public static bool IsValid(this Direction direction)
	{
		if (direction != Direction.Up && direction != Direction.Down && direction != Direction.Left)
		{
			return direction == Direction.Right;
		}
		return true;
	}

	private static Vector2 ConvertVector3DirectionToVector2XZ(this Vector3 direction3D)
	{
		return (Quaternion.AngleAxis(90f, Vector3.right) * new Vector2(direction3D.x, direction3D.y)).normalized;
	}
}
