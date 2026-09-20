using UnityEngine;

namespace LazyBearTechnology;

public class WorldVirtualCursor : BaseVirtualCursor
{
	protected Vector2 screenVector => new Vector2(Screen.width, Screen.height);

	public override void CalculateBounds()
	{
		minCoords = ScreenToWorld(Vector2.zero);
		maxCoords = ScreenToWorld(screenVector);
	}

	protected virtual Vector2 ScreenToWorld(Vector2 point)
	{
		return Camera.main.ScreenToWorldPoint(point);
	}
}
