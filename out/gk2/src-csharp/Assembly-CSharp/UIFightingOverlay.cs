using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class UIFightingOverlay : LazyWidget<UIFightingOverlayData>
{
	public UICaptureBannerWidget captureBannerWidgetPrefab;

	public float clampWidgetOffsetFromScreenBorder = 20f;

	private Dictionary<int, UICaptureBannerWidget> drawingCapturePoints = new Dictionary<int, UICaptureBannerWidget>();

	private Pool captureBannerWidgetPool;

	protected override void SetData(UIFightingOverlayData data)
	{
		if (base.data != null)
		{
			base.data.OnDataUpdated -= HandleDataUpdated;
			base.data.OnCapturePointTracked -= HandleCapturePointTracked;
			base.data.OnCapturePointUntracked -= HandleCapturePointUntracked;
		}
		base.SetData(data);
		data.OnDataUpdated += HandleDataUpdated;
		data.OnCapturePointTracked += HandleCapturePointTracked;
		data.OnCapturePointUntracked += HandleCapturePointUntracked;
	}

	private void HandleDataUpdated()
	{
		Redraw();
	}

	private void HandleCapturePointTracked(UICaptureBannerWidgetData capturePoint)
	{
		UICaptureBannerWidget orCreateObject = captureBannerWidgetPool.GetOrCreateObject<UICaptureBannerWidget>();
		orCreateObject.Draw(capturePoint);
		orCreateObject.transform.SetParent(base.transform);
		orCreateObject.transform.SetAsLastSibling();
		orCreateObject.gameObject.SetActive(value: true);
		drawingCapturePoints.Add(capturePoint.InstanceID, orCreateObject);
	}

	private void HandleCapturePointUntracked(UICaptureBannerWidgetData capturePoint)
	{
		captureBannerWidgetPool.ReleaseObject(drawingCapturePoints[capturePoint.InstanceID]);
		drawingCapturePoints.Remove(capturePoint.InstanceID);
	}

	public override void Hide()
	{
		base.Hide();
		data.OnDataUpdated -= HandleDataUpdated;
		data.OnCapturePointTracked -= HandleCapturePointTracked;
		data.OnCapturePointUntracked -= HandleCapturePointUntracked;
	}

	public override void Redraw()
	{
		Bounds screenBounds = LazyUI.GetScreenBounds();
		Vector2 vector = new Vector2(screenBounds.center.x, screenBounds.center.y);
		Rect rect = new Rect(screenBounds.min.x + clampWidgetOffsetFromScreenBorder, screenBounds.min.y + clampWidgetOffsetFromScreenBorder, screenBounds.size.x - clampWidgetOffsetFromScreenBorder * 2f, screenBounds.size.y - clampWidgetOffsetFromScreenBorder * 2f);
		foreach (KeyValuePair<int, UICaptureBannerWidget> drawingCapturePoint in drawingCapturePoints)
		{
			UICaptureBannerWidgetData uICaptureBannerWidgetData = data.DrawingCapturePoints[drawingCapturePoint.Key];
			Vector2 screenPosition = uICaptureBannerWidgetData.ScreenPosition;
			uICaptureBannerWidgetData.DirectionToCapturePoint = (screenPosition - vector).normalized;
			bool flag = !rect.Contains(screenPosition);
			if (flag)
			{
				uICaptureBannerWidgetData.ScreenPosition = GetRectEdgeIntersection(vector, screenPosition, rect);
			}
			uICaptureBannerWidgetData.IsOutOfScreen = flag;
			drawingCapturePoint.Value.Draw(uICaptureBannerWidgetData);
		}
	}

	private Vector2 GetRectEdgeIntersection(Vector2 from, Vector2 to, Rect rect)
	{
		Vector2 vector = to - from;
		float a = 0f;
		float num = 1f;
		if (Mathf.Abs(vector.x) > 0.0001f)
		{
			float num2 = (rect.xMin - from.x) / vector.x;
			float num3 = (rect.xMax - from.x) / vector.x;
			if (vector.x < 0f)
			{
				float num4 = num3;
				float num5 = num2;
				num2 = num4;
				num3 = num5;
			}
			a = Mathf.Max(a, num2);
			num = Mathf.Min(num, num3);
		}
		if (Mathf.Abs(vector.y) > 0.0001f)
		{
			float num6 = (rect.yMin - from.y) / vector.y;
			float num7 = (rect.yMax - from.y) / vector.y;
			if (vector.y < 0f)
			{
				float num8 = num7;
				float num5 = num6;
				num6 = num8;
				num7 = num5;
			}
			a = Mathf.Max(a, num6);
			num = Mathf.Min(num, num7);
		}
		float num9 = Mathf.Clamp(num, 0f, 1f);
		return from + vector * num9;
	}

	protected override void TestDraw()
	{
	}

	private void Awake()
	{
		captureBannerWidgetPool = new Pool(captureBannerWidgetPrefab, base.transform, 10);
		captureBannerWidgetPrefab.gameObject.SetActive(value: false);
	}
}
