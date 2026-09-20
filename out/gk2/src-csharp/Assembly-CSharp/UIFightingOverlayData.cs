using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class UIFightingOverlayData : LazyWidgetDataBase
{
	public FightingGameController FightingGameController { get; private set; }

	public Dictionary<int, FightingCapturePoint> CapturePoint { get; private set; } = new Dictionary<int, FightingCapturePoint>();


	public Dictionary<int, UICaptureBannerWidgetData> DrawingCapturePoints { get; private set; } = new Dictionary<int, UICaptureBannerWidgetData>();


	public event Action OnDataUpdated;

	public event Action<UICaptureBannerWidgetData> OnCapturePointTracked;

	public event Action<UICaptureBannerWidgetData> OnCapturePointUntracked;

	public UIFightingOverlayData(FightingGameController fightingGameController)
	{
		FightingGameController = fightingGameController;
	}

	public void UpdateData(Vector3 playerPosition)
	{
		if (FightingGameController == null)
		{
			return;
		}
		foreach (KeyValuePair<int, FightingCapturePoint> item in CapturePoint)
		{
			Vector2 directionToCapturePoint = (item.Value.transform.position - playerPosition).XZ().normalized;
			if (DrawingCapturePoints.TryGetValue(item.Key, out var value))
			{
				value.CaptureProgress = item.Value.CurrentProgress;
				value.DirectionToCapturePoint = directionToCapturePoint;
				value.ScreenPosition = CameraSystem.WorldToScreenPoint(item.Value.transform.position);
			}
		}
		this.OnDataUpdated?.Invoke();
	}

	public void TrackCapturePoint(FightingCapturePoint capturePoint)
	{
		if (!(capturePoint == null))
		{
			int instanceID = capturePoint.GetInstanceID();
			CapturePoint.Add(instanceID, capturePoint);
			DrawingCapturePoints.Add(instanceID, new UICaptureBannerWidgetData(instanceID, 0f, Vector2.zero, Vector2.zero));
			this.OnCapturePointTracked?.Invoke(DrawingCapturePoints[instanceID]);
		}
	}

	public void UntrackCapturePoint(FightingCapturePoint capturePoint)
	{
		if (!(capturePoint == null))
		{
			int instanceID = capturePoint.GetInstanceID();
			if (DrawingCapturePoints.TryGetValue(instanceID, out var value))
			{
				this.OnCapturePointUntracked?.Invoke(value);
				DrawingCapturePoints.Remove(instanceID);
			}
			CapturePoint.Remove(instanceID);
		}
	}
}
