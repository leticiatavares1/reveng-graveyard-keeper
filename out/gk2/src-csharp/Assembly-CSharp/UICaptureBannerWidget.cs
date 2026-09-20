using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class UICaptureBannerWidget : LazyWidget<UICaptureBannerWidgetData>
{
	public List<Sprite> pointerSprites = new List<Sprite>();

	public Image progressBarImage;

	public Image glowImage;

	public Image pointerImage;

	private static readonly float[] SpriteAngles = new float[6] { 0f, 19f, 33f, 45f, 57f, 71f };

	public override void Draw(UICaptureBannerWidgetData data)
	{
		base.gameObject.SetActive(data.CaptureProgress.More(0f, 0.0001f) && data.CaptureProgress.Less(1f, 0.0001f) && data.IsOutOfScreen);
		float fillAmount = 1f - data.CaptureProgress;
		progressBarImage.fillAmount = fillAmount;
		glowImage.fillAmount = fillAmount;
		UpdatePointer(data.DirectionToCapturePoint);
		base.transform.position = data.ScreenPosition;
	}

	private void UpdatePointer(Vector2 direction)
	{
		if (!(direction == Vector2.zero) && pointerSprites.Count != 0)
		{
			float num = Mathf.Atan2(direction.y, direction.x) * 57.29578f;
			if (num < 0f)
			{
				num += 360f;
			}
			float num2 = (450f - num) % 360f;
			int num3 = Mathf.FloorToInt(num2 / 90f) % 4;
			float num4 = num2 - (float)num3 * 90f;
			int num5 = FindClosestSpriteIndex(num4);
			float num6 = Mathf.Abs(num4 - SpriteAngles[num5]);
			if (90f - num4 < num6)
			{
				num3 = (num3 + 1) % 4;
				num5 = 0;
			}
			float z = (float)(-num3) * 90f;
			pointerImage.transform.localRotation = Quaternion.Euler(0f, 0f, z);
			pointerImage.sprite = pointerSprites[num5];
		}
	}

	private int FindClosestSpriteIndex(float localAngle)
	{
		int result = 0;
		float num = Mathf.Abs(localAngle - SpriteAngles[0]);
		int num2 = Mathf.Min(SpriteAngles.Length, pointerSprites.Count);
		for (int i = 1; i < num2; i++)
		{
			float num3 = Mathf.Abs(localAngle - SpriteAngles[i]);
			if (num3 < num)
			{
				num = num3;
				result = i;
			}
		}
		return result;
	}

	protected override void TestDraw()
	{
		throw new NotImplementedException();
	}
}
