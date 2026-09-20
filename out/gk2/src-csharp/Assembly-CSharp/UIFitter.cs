using LazyBearTechnology;
using UnityEngine;

public class UIFitter : MonoBehaviour
{
	private RectTransform rectTransform;

	private RectTransform RectTransform
	{
		get
		{
			if (rectTransform == null)
			{
				rectTransform = GetComponent<RectTransform>();
			}
			return rectTransform;
		}
	}

	public void UpdateSafeArea(float scaleFactor)
	{
		LazyUI.SetSafeZones(Screen.safeArea);
		Bounds screenBounds = LazyUI.GetScreenBounds();
		RectTransform.offsetMin = new Vector2(screenBounds.center.x, (float)Screen.height - screenBounds.extents.y - screenBounds.center.y) / scaleFactor;
		RectTransform.offsetMax = new Vector2((float)Screen.width - screenBounds.center.x - screenBounds.extents.x, screenBounds.center.y) / scaleFactor;
		Debug.Log($"SetUISafeArea: res: [{Screen.width}x{Screen.height}], scale: [{scaleFactor}], offsetMin: [{RectTransform.offsetMin}], offsetMax: [{RectTransform.offsetMax}]");
	}
}
