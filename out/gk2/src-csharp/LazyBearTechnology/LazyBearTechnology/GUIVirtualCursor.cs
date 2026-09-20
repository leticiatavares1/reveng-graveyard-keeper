using UnityEngine;

namespace LazyBearTechnology;

public class GUIVirtualCursor : BaseVirtualCursor
{
	public override void CalculateBounds()
	{
		minCoords = Vector2.zero;
		maxCoords.x = Screen.width;
		maxCoords.y = Screen.height;
	}

	protected override float GetSpeed()
	{
		return currentSpeed * LazyUI.ScaleFactor * Time.deltaTime;
	}
}
