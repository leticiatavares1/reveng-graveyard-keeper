using UnityEngine;

namespace ParadoxNotion;

public static class RectUtils
{
	public static Rect GetBoundRect(params Rect[] rects)
	{
		float num = float.PositiveInfinity;
		float num2 = float.NegativeInfinity;
		float num3 = float.PositiveInfinity;
		float num4 = float.NegativeInfinity;
		for (int i = 0; i < rects.Length; i++)
		{
			num = Mathf.Min(num, rects[i].xMin);
			num2 = Mathf.Max(num2, rects[i].xMax);
			num3 = Mathf.Min(num3, rects[i].yMin);
			num4 = Mathf.Max(num4, rects[i].yMax);
		}
		return Rect.MinMaxRect(num, num3, num2, num4);
	}

	public static Rect GetBoundRect(params Vector2[] positions)
	{
		float num = float.PositiveInfinity;
		float num2 = float.NegativeInfinity;
		float num3 = float.PositiveInfinity;
		float num4 = float.NegativeInfinity;
		for (int i = 0; i < positions.Length; i++)
		{
			num = Mathf.Min(num, positions[i].x);
			num2 = Mathf.Max(num2, positions[i].x);
			num3 = Mathf.Min(num3, positions[i].y);
			num4 = Mathf.Max(num4, positions[i].y);
		}
		return Rect.MinMaxRect(num, num3, num2, num4);
	}

	public static bool Encapsulates(this Rect a, Rect b)
	{
		if (a.x < b.x && a.xMax > b.xMax && a.y < b.y)
		{
			return a.yMax > b.yMax;
		}
		return false;
	}

	public static Rect ExpandBy(this Rect rect, float margin)
	{
		return Rect.MinMaxRect(rect.xMin - margin, rect.yMin - margin, rect.xMax + margin, rect.yMax + margin);
	}

	public static Rect TransformSpace(this Rect rect, Rect oldContainer, Rect newContainer)
	{
		Rect result = default(Rect);
		result.xMin = Mathf.Lerp(newContainer.xMin, newContainer.xMax, Mathf.InverseLerp(oldContainer.xMin, oldContainer.xMax, rect.xMin));
		result.xMax = Mathf.Lerp(newContainer.xMin, newContainer.xMax, Mathf.InverseLerp(oldContainer.xMin, oldContainer.xMax, rect.xMax));
		result.yMin = Mathf.Lerp(newContainer.yMin, newContainer.yMax, Mathf.InverseLerp(oldContainer.yMin, oldContainer.yMax, rect.yMin));
		result.yMax = Mathf.Lerp(newContainer.yMin, newContainer.yMax, Mathf.InverseLerp(oldContainer.yMin, oldContainer.yMax, rect.yMax));
		return result;
	}
}
