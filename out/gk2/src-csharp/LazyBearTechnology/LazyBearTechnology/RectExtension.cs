using UnityEngine;

namespace LazyBearTechnology;

public static class RectExtension
{
	public static bool LCS_Overlaps(this Rect rect, Rect other)
	{
		if (other.xMax > rect.xMin && other.xMin < rect.xMax && other.yMin - other.height < rect.yMin)
		{
			return other.yMin > rect.yMin - rect.height;
		}
		return false;
	}

	public static bool LCS_IsOverlapsWithAnyOthers(this Rect rect, Rect[] others)
	{
		foreach (Rect other in others)
		{
			if (rect.LCS_Overlaps(other))
			{
				return true;
			}
		}
		return false;
	}
}
