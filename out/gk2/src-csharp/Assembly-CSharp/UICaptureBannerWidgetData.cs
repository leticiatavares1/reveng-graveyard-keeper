using LazyBearTechnology;
using UnityEngine;

public class UICaptureBannerWidgetData : LazyWidgetDataBase
{
	public int InstanceID { get; private set; }

	public float CaptureProgress { get; set; }

	public Vector2 DirectionToCapturePoint { get; set; }

	public Vector2 ScreenPosition { get; set; }

	public bool IsOutOfScreen { get; set; }

	public UICaptureBannerWidgetData(int instanceID, float captureProgress, Vector2 directionToCapturePoint, Vector2 screenPosition)
	{
		InstanceID = instanceID;
		CaptureProgress = captureProgress;
		DirectionToCapturePoint = directionToCapturePoint;
		ScreenPosition = screenPosition;
	}
}
