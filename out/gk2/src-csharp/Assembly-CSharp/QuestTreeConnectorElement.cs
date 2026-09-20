using UnityEngine;
using UnityEngine.UI;

public class QuestTreeConnectorElement : MonoBehaviour
{
	public Image image;

	public Vector2 extraSize;

	public Sprite backgroundTypeSprite;

	public Sprite foregroundTypeSprite;

	public Color colorBackground;

	public Color foregroundColorActive;

	public Color foregroundColorInactive;

	public RectTransform RectTransform => image.rectTransform;

	public void SetupAsActive(bool background)
	{
		if (background)
		{
			image.sprite = backgroundTypeSprite;
			image.color = colorBackground;
		}
		else
		{
			image.sprite = foregroundTypeSprite;
			image.color = foregroundColorActive;
		}
	}

	public void SetupAsInactive(bool background)
	{
		if (background)
		{
			image.sprite = backgroundTypeSprite;
			image.color = colorBackground;
		}
		else
		{
			image.sprite = foregroundTypeSprite;
			image.color = foregroundColorInactive;
		}
	}
}
