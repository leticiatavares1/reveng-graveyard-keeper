using LazyBearTechnology;
using UnityEngine;

public class UIMagnifyingGlassWidgetData : LazyWidgetDataBase
{
	public SGuid UniqueId { get; private set; }

	public Vector2 DirectionToTarget { get; set; }

	public Vector2 ScreenPosition { get; set; }

	public bool IsOutOfScreen { get; set; }

	public UIMagnifyingGlassWidgetData(SGuid uniqueId, Vector2 directionToTarget, Vector2 screenPosition)
	{
		UniqueId = uniqueId;
		DirectionToTarget = directionToTarget;
		ScreenPosition = screenPosition;
	}
}
