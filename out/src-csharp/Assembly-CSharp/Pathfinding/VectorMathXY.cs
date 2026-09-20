using UnityEngine;

namespace Pathfinding;

public static class VectorMathXY
{
	public static float SqrDistanceXY(Vector3 a, Vector3 b)
	{
		Vector3 vector = a - b;
		return vector.x * vector.x + vector.y * vector.y;
	}

	public static long SignedTriangleAreaTimes2XY(Int3 a, Int3 b, Int3 c)
	{
		return (long)(b.x - a.x) * (long)(c.y - a.y) - (long)(c.x - a.x) * (long)(b.y - a.y);
	}

	public static float SignedTriangleAreaTimes2XY(Vector3 a, Vector3 b, Vector3 c)
	{
		return (b.x - a.x) * (c.y - a.y) - (c.x - a.x) * (b.y - a.y);
	}

	public static bool RightXY(Vector3 a, Vector3 b, Vector3 p)
	{
		return (b.x - a.x) * (p.y - a.y) - (p.x - a.x) * (b.y - a.y) < -1E-45f;
	}

	public static bool RightXY(Int3 a, Int3 b, Int3 p)
	{
		return (long)(b.x - a.x) * (long)(p.y - a.y) - (long)(p.x - a.x) * (long)(b.y - a.y) < 0;
	}

	public static bool RightOrColinearXY(Vector3 a, Vector3 b, Vector3 p)
	{
		return (b.x - a.x) * (p.y - a.y) - (p.x - a.x) * (b.y - a.y) <= 0f;
	}

	public static bool RightOrColinearXY(Int3 a, Int3 b, Int3 p)
	{
		return (long)(b.x - a.x) * (long)(p.y - a.y) - (long)(p.x - a.x) * (long)(b.y - a.y) <= 0;
	}

	public static bool IsClockwiseMarginXY(Vector3 a, Vector3 b, Vector3 c)
	{
		return (b.x - a.x) * (c.y - a.y) - (c.x - a.x) * (b.y - a.y) <= float.Epsilon;
	}

	public static bool IsClockwiseXY(Vector3 a, Vector3 b, Vector3 c)
	{
		return (b.x - a.x) * (c.y - a.y) - (c.x - a.x) * (b.y - a.y) < 0f;
	}

	public static bool IsClockwiseXY(Int3 a, Int3 b, Int3 c)
	{
		return RightXY(a, b, c);
	}

	public static bool IsClockwiseOrColinearXY(Int3 a, Int3 b, Int3 c)
	{
		return RightOrColinearXY(a, b, c);
	}

	public static bool IsColinearXY(Int3 a, Int3 b, Int3 c)
	{
		return (long)(b.x - a.x) * (long)(c.y - a.y) - (long)(c.x - a.x) * (long)(b.y - a.y) == 0;
	}

	public static bool IsColinearXY(Vector3 a, Vector3 b, Vector3 c)
	{
		float num = (b.x - a.x) * (c.y - a.y) - (c.x - a.x) * (b.y - a.y);
		if (num <= 1E-07f)
		{
			return num >= -1E-07f;
		}
		return false;
	}

	public static bool IsColinearAlmostXY(Int3 a, Int3 b, Int3 c)
	{
		long num = (long)(b.x - a.x) * (long)(c.y - a.y) - (long)(c.x - a.x) * (long)(b.y - a.y);
		if (num > -1)
		{
			return num < 1;
		}
		return false;
	}

	public static bool SegmentsIntersectXY(Int3 start1, Int3 end1, Int3 start2, Int3 end2)
	{
		if (RightOrColinearXY(start1, end1, start2) != RightOrColinearXY(start1, end1, end2))
		{
			return RightOrColinearXY(start2, end2, start1) != RightOrColinearXY(start2, end2, end1);
		}
		return false;
	}

	public static bool SegmentsIntersectXY(Vector3 start1, Vector3 end1, Vector3 start2, Vector3 end2)
	{
		Vector3 vector = end1 - start1;
		Vector3 vector2 = end2 - start2;
		float num = vector2.y * vector.x - vector2.x * vector.y;
		if (num == 0f)
		{
			return false;
		}
		float num2 = vector2.x * (start1.y - start2.y) - vector2.y * (start1.x - start2.x);
		float num3 = vector.x * (start1.y - start2.y) - vector.y * (start1.x - start2.x);
		float num4 = num2 / num;
		float num5 = num3 / num;
		if (num4 < 0f || num4 > 1f || num5 < 0f || num5 > 1f)
		{
			return false;
		}
		return true;
	}

	public static Vector3 LineDirIntersectionPointXY(Vector3 start1, Vector3 dir1, Vector3 start2, Vector3 dir2)
	{
		float num = dir2.y * dir1.x - dir2.x * dir1.y;
		if (num == 0f)
		{
			return start1;
		}
		float num2 = (dir2.x * (start1.y - start2.y) - dir2.y * (start1.x - start2.x)) / num;
		return start1 + dir1 * num2;
	}

	public static Vector3 LineDirIntersectionPointXY(Vector3 start1, Vector3 dir1, Vector3 start2, Vector3 dir2, out bool intersects)
	{
		float num = dir2.y * dir1.x - dir2.x * dir1.y;
		if (num == 0f)
		{
			intersects = false;
			return start1;
		}
		float num2 = (dir2.x * (start1.y - start2.y) - dir2.y * (start1.x - start2.x)) / num;
		intersects = true;
		return start1 + dir1 * num2;
	}

	public static bool RaySegmentIntersectXY(Int3 start1, Int3 end1, Int3 start2, Int3 end2)
	{
		Int3 @int = end1 - start1;
		Int3 int2 = end2 - start2;
		long num = int2.y * @int.x - int2.x * @int.y;
		if (num == 0L)
		{
			return false;
		}
		long num2 = int2.x * (start1.y - start2.y) - int2.y * (start1.x - start2.x);
		long num3 = @int.x * (start1.y - start2.y) - @int.y * (start1.x - start2.x);
		if (!((num2 < 0) ^ (num < 0)))
		{
			return false;
		}
		if (!((num3 < 0) ^ (num < 0)))
		{
			return false;
		}
		if ((num >= 0 && num3 > num) || (num < 0 && num3 <= num))
		{
			return false;
		}
		return true;
	}

	public static bool LineIntersectionFactorXY(Int3 start1, Int3 end1, Int3 start2, Int3 end2, out float factor1, out float factor2)
	{
		Int3 @int = end1 - start1;
		Int3 int2 = end2 - start2;
		long num = int2.y * @int.x - int2.x * @int.y;
		if (num == 0L)
		{
			factor1 = 0f;
			factor2 = 0f;
			return false;
		}
		long num2 = int2.x * (start1.y - start2.y) - int2.y * (start1.x - start2.x);
		long num3 = @int.x * (start1.y - start2.y) - @int.y * (start1.x - start2.x);
		factor1 = (float)num2 / (float)num;
		factor2 = (float)num3 / (float)num;
		return true;
	}

	public static bool LineIntersectionFactorXY(Vector3 start1, Vector3 end1, Vector3 start2, Vector3 end2, out float factor1, out float factor2)
	{
		Vector3 vector = end1 - start1;
		Vector3 vector2 = end2 - start2;
		float num = vector2.y * vector.x - vector2.x * vector.y;
		if (num <= 1E-05f && num >= -1E-05f)
		{
			factor1 = 0f;
			factor2 = 0f;
			return false;
		}
		float num2 = vector2.x * (start1.y - start2.y) - vector2.y * (start1.x - start2.x);
		float num3 = vector.x * (start1.y - start2.y) - vector.y * (start1.x - start2.x);
		float num4 = num2 / num;
		float num5 = num3 / num;
		factor1 = num4;
		factor2 = num5;
		return true;
	}

	public static float LineRayIntersectionFactorXY(Int3 start1, Int3 end1, Int3 start2, Int3 end2)
	{
		Int3 @int = end1 - start1;
		Int3 int2 = end2 - start2;
		int num = int2.y * @int.x - int2.x * @int.y;
		if (num == 0)
		{
			return float.NaN;
		}
		int num2 = int2.x * (start1.y - start2.y) - int2.y * (start1.x - start2.x);
		if ((float)(@int.x * (start1.y - start2.y) - @int.y * (start1.x - start2.x)) / (float)num < 0f)
		{
			return float.NaN;
		}
		return (float)num2 / (float)num;
	}

	public static float LineIntersectionFactorXY(Vector3 start1, Vector3 end1, Vector3 start2, Vector3 end2)
	{
		Vector3 vector = end1 - start1;
		Vector3 vector2 = end2 - start2;
		float num = vector2.y * vector.x - vector2.x * vector.y;
		if (num == 0f)
		{
			return -1f;
		}
		return (vector2.x * (start1.y - start2.y) - vector2.y * (start1.x - start2.x)) / num;
	}

	public static Vector3 LineIntersectionPointXY(Vector3 start1, Vector3 end1, Vector3 start2, Vector3 end2)
	{
		bool intersects;
		return LineIntersectionPointXY(start1, end1, start2, end2, out intersects);
	}

	public static Vector3 LineIntersectionPointXY(Vector3 start1, Vector3 end1, Vector3 start2, Vector3 end2, out bool intersects)
	{
		Vector3 vector = end1 - start1;
		Vector3 vector2 = end2 - start2;
		float num = vector2.y * vector.x - vector2.x * vector.y;
		if (num == 0f)
		{
			intersects = false;
			return start1;
		}
		float num2 = (vector2.x * (start1.y - start2.y) - vector2.y * (start1.x - start2.x)) / num;
		intersects = true;
		return start1 + vector * num2;
	}

	public static Vector3 SegmentIntersectionPointXY(Vector3 start1, Vector3 end1, Vector3 start2, Vector3 end2, out bool intersects)
	{
		Vector3 vector = end1 - start1;
		Vector3 vector2 = end2 - start2;
		float num = vector2.y * vector.x - vector2.x * vector.y;
		if (num == 0f)
		{
			intersects = false;
			return start1;
		}
		float num2 = vector2.x * (start1.y - start2.y) - vector2.y * (start1.x - start2.x);
		float num3 = vector.x * (start1.y - start2.y) - vector.y * (start1.x - start2.x);
		float num4 = num2 / num;
		float num5 = num3 / num;
		if (num4 < 0f || num4 > 1f || num5 < 0f || num5 > 1f)
		{
			intersects = false;
			return start1;
		}
		intersects = true;
		return start1 + vector * num4;
	}

	public static bool ReversesFaceOrientationsXY(Matrix4x4 matrix)
	{
		Vector3 vector = matrix.MultiplyVector(new Vector3(1f, 0f, 0f));
		Vector3 vector2 = matrix.MultiplyVector(new Vector3(0f, 0f, 1f));
		return vector.x * vector2.y - vector2.x * vector.y < 0f;
	}
}
