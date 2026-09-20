using UnityEngine;
using UnityEngine.UI;

namespace LazyBearTechnology;

public static class RectTransformExtension
{
	public static bool IsRectOutOfScreen(this RectTransform rectTransform, Bounds screenBounds)
	{
		Rect worldRect = rectTransform.GetWorldRect();
		if (worldRect.xMax < screenBounds.min.x)
		{
			return true;
		}
		if (worldRect.xMin > screenBounds.max.x)
		{
			return true;
		}
		if (worldRect.yMin > screenBounds.max.y)
		{
			return true;
		}
		if (worldRect.yMax < screenBounds.min.y)
		{
			return true;
		}
		return false;
	}
}
