using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class UIMagnifyingGlassWidget : LazyWidget<UIMagnifyingGlassWidgetData>
{
	private const int SpriteCount = 12;

	private const float DegreesPerSprite = 30f;

	private const string OnScreenExitIconId = "icon_view_bubble";

	public List<Sprite> pointerSprites = new List<Sprite>();

	public Image pointerImage;

	public Image iconImage;

	private Sprite originalIconSprite;

	private Vector2 originalIconSize;

	private bool originalIconCached;

	public override void Draw(UIMagnifyingGlassWidgetData data)
	{
		base.gameObject.SetActive(value: true);
		CacheOriginalIcon();
		bool isOutOfScreen = data.IsOutOfScreen;
		if (pointerImage != null)
		{
			pointerImage.gameObject.SetActive(isOutOfScreen);
		}
		if (iconImage != null)
		{
			iconImage.gameObject.SetActive(value: true);
			if (isOutOfScreen)
			{
				if (originalIconSprite != null)
				{
					iconImage.sprite = originalIconSprite;
					iconImage.rectTransform.sizeDelta = originalIconSize;
				}
			}
			else
			{
				Sprite sprite = ((LazySingletonSO<EasySpritesCollection>.Instance != null) ? LazySingletonSO<EasySpritesCollection>.Instance.GetSprite("icon_view_bubble") : null);
				if (sprite != null)
				{
					iconImage.sprite = sprite;
					iconImage.SetNativeSize();
				}
			}
		}
		if (isOutOfScreen)
		{
			UpdatePointer(data.DirectionToTarget);
		}
		base.transform.position = data.ScreenPosition;
	}

	private void CacheOriginalIcon()
	{
		if (!originalIconCached && !(iconImage == null))
		{
			originalIconSprite = iconImage.sprite;
			originalIconSize = iconImage.rectTransform.sizeDelta;
			originalIconCached = true;
		}
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
			int num2 = Mathf.RoundToInt((90f - num + 360f) % 360f / 30f) % 12;
			if (num2 >= 0 && num2 < pointerSprites.Count)
			{
				pointerImage.sprite = pointerSprites[num2];
				pointerImage.transform.localRotation = Quaternion.identity;
			}
		}
	}

	protected override void TestDraw()
	{
		throw new NotImplementedException();
	}
}
