using System;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CinematicsTextWidget : LazyWidget<CinematicsTextWidgetData>
{
	[SerializeField]
	private TextMeshProUGUI label;

	[SerializeField]
	private RectTransform rectTransform;

	[SerializeField]
	private Image backgroundImage;

	[SerializeField]
	private ContentSizeFitter contentSizeFitter;

	public override void Init()
	{
		base.Init();
		if (rectTransform == null)
		{
			rectTransform = base.transform as RectTransform;
		}
		if (label == null)
		{
			label = GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);
		}
	}

	protected override void SetData(CinematicsTextWidgetData data)
	{
		UnsubscribeFromData();
		base.SetData(data);
		SubscribeToData();
	}

	public override void Hide()
	{
		UnsubscribeFromData();
		base.Hide();
	}

	public override void Redraw()
	{
		if (data != null)
		{
			ApplyWorldLayout();
			ApplyBackgroundVisibility();
			ApplyColor(data.Color);
		}
	}

	private void LateUpdate()
	{
		if (data != null && base.isActiveAndEnabled)
		{
			ApplyWorldLayout();
			ApplyBackgroundVisibility();
			ApplyColor(data.Color);
		}
	}

	private void OnDestroy()
	{
		UnsubscribeFromData();
	}

	private void ApplyWorldLayout()
	{
		if (!(rectTransform == null) && !(CameraSystem.Instance == null))
		{
			rectTransform.pivot = data.Pivot;
			rectTransform.position = CameraSystem.WorldToScreenPoint(new Vector3(data.Position.x, data.Position.y, 0f));
			bool showBackground = data.ShowBackground;
			if (contentSizeFitter != null)
			{
				contentSizeFitter.enabled = showBackground;
			}
			if (!showBackground)
			{
				rectTransform.sizeDelta = GetMappedWorldSize();
			}
		}
	}

	private Vector2 GetMappedWorldSize()
	{
		Vector2 pivot = data.Pivot;
		Vector3 worldPoint = new Vector3(data.Position.x - data.Size.x * pivot.x, data.Position.y - data.Size.y * pivot.y, 0f);
		Vector3 worldPoint2 = new Vector3(data.Position.x + data.Size.x * (1f - pivot.x), data.Position.y + data.Size.y * (1f - pivot.y), 0f);
		Vector3 vector = CameraSystem.WorldToScreenPoint(worldPoint);
		Vector3 vector2 = CameraSystem.WorldToScreenPoint(worldPoint2);
		float num = LazyUI.ScaleFactor;
		if (num <= 0f)
		{
			num = Mathf.Max(1, ResolutionConfig.PixelSize);
		}
		return new Vector2(Mathf.Abs(vector2.x - vector.x) / num, Mathf.Abs(vector2.y - vector.y) / num);
	}

	private void SubscribeToData()
	{
		if (data != null)
		{
			CinematicsTextWidgetData cinematicsTextWidgetData = data;
			cinematicsTextWidgetData.OnTextChanged = (Action<string>)Delegate.Combine(cinematicsTextWidgetData.OnTextChanged, new Action<string>(OnTextChanged));
			CinematicsTextWidgetData cinematicsTextWidgetData2 = data;
			cinematicsTextWidgetData2.OnVisibleChanged = (Action<bool>)Delegate.Combine(cinematicsTextWidgetData2.OnVisibleChanged, new Action<bool>(OnVisibleChanged));
			CinematicsTextWidgetData cinematicsTextWidgetData3 = data;
			cinematicsTextWidgetData3.OnColorChanged = (Action<Color>)Delegate.Combine(cinematicsTextWidgetData3.OnColorChanged, new Action<Color>(OnColorChanged));
		}
	}

	private void UnsubscribeFromData()
	{
		if (data != null)
		{
			CinematicsTextWidgetData cinematicsTextWidgetData = data;
			cinematicsTextWidgetData.OnTextChanged = (Action<string>)Delegate.Remove(cinematicsTextWidgetData.OnTextChanged, new Action<string>(OnTextChanged));
			CinematicsTextWidgetData cinematicsTextWidgetData2 = data;
			cinematicsTextWidgetData2.OnVisibleChanged = (Action<bool>)Delegate.Remove(cinematicsTextWidgetData2.OnVisibleChanged, new Action<bool>(OnVisibleChanged));
			CinematicsTextWidgetData cinematicsTextWidgetData3 = data;
			cinematicsTextWidgetData3.OnColorChanged = (Action<Color>)Delegate.Remove(cinematicsTextWidgetData3.OnColorChanged, new Action<Color>(OnColorChanged));
		}
	}

	private void OnTextChanged(string text)
	{
		if (label != null)
		{
			label.text = text;
		}
	}

	private void OnVisibleChanged(bool visible)
	{
		base.gameObject.SetActive(visible);
	}

	private void OnColorChanged(Color color)
	{
		ApplyColor(color);
	}

	private void ApplyBackgroundVisibility()
	{
		if (!(backgroundImage == null))
		{
			backgroundImage.gameObject.SetActive(data != null && data.ShowBackground);
		}
	}

	private void ApplyColor(Color color)
	{
		if (label != null)
		{
			label.color = color;
		}
		if (!(backgroundImage == null) && backgroundImage.gameObject.activeSelf)
		{
			Color color2 = backgroundImage.color;
			color2.a = color.a;
			backgroundImage.color = color2;
		}
	}

	protected override void TestDraw()
	{
	}
}
