using UnityEngine;
using UnityEngine.UI;

public class TechTreeConnectorElement : MonoBehaviour
{
	public Image image;

	public Vector2 extraSize;

	public Sprite backgroundTypeSprite;

	public Sprite foregroundTypeSprite;

	public Sprite backgroundTypeSpriteRepWidgetSource;

	public Sprite foregroundTypeSpriteRepWidgetSource;

	public Color colorBackground;

	public Color foregroundColorActive;

	public Color foregroundColorInactive;

	public RectTransform RectTransform => image.rectTransform;

	public void SetupAsActive(bool background, bool useRepWidgetSource = false)
	{
		if (background)
		{
			image.sprite = GetBackgroundSprite(useRepWidgetSource);
			image.color = colorBackground;
		}
		else
		{
			image.sprite = GetForegroundSprite(useRepWidgetSource);
			image.color = foregroundColorActive;
		}
	}

	public void SetupAsInactive(bool background, bool useRepWidgetSource = false)
	{
		if (background)
		{
			image.sprite = GetBackgroundSprite(useRepWidgetSource);
			image.color = colorBackground;
		}
		else
		{
			image.sprite = GetForegroundSprite(useRepWidgetSource);
			image.color = foregroundColorInactive;
		}
	}

	private Sprite GetBackgroundSprite(bool useRepWidgetSource)
	{
		if (!useRepWidgetSource || !(backgroundTypeSpriteRepWidgetSource != null))
		{
			return backgroundTypeSprite;
		}
		return backgroundTypeSpriteRepWidgetSource;
	}

	private Sprite GetForegroundSprite(bool useRepWidgetSource)
	{
		if (!useRepWidgetSource || !(foregroundTypeSpriteRepWidgetSource != null))
		{
			return foregroundTypeSprite;
		}
		return foregroundTypeSpriteRepWidgetSource;
	}
}
