using UnityEngine;
using UnityEngine.UI;

public class GenericWindowLayout : MonoBehaviour
{
	[SerializeField]
	private GameObject header;

	[SerializeField]
	private RectTransform maskRectTransform;

	[SerializeField]
	private Vector2 maskOffsetMinWithHeader = new Vector2(13f, 13f);

	[SerializeField]
	private Vector2 maskOffsetMaxWithHeader = new Vector2(13f, 36f);

	[SerializeField]
	private Vector2 maskOffsetMinWithoutHeader = new Vector2(13f, 13f);

	[SerializeField]
	private Vector2 maskOffsetMaxWithoutHeader = new Vector2(13f, 13f);

	[SerializeField]
	private VerticalLayoutGroup windowLayoutGroup;

	[SerializeField]
	private int layoutGroupTopPaddingWithHeader = 47;

	[SerializeField]
	private int layoutGroupTopPaddingWithoutHeader = 24;

	[SerializeField]
	private Image[] backgroundImages;

	[SerializeField]
	private Sprite backgroundSpriteBig;

	[SerializeField]
	private Sprite backgroundSpriteSmall;

	public void UpdateSize(bool isSmall)
	{
		if (isSmall)
		{
			header.gameObject.SetActive(value: false);
			maskRectTransform.offsetMin = maskOffsetMinWithoutHeader;
			maskRectTransform.offsetMax = maskOffsetMaxWithoutHeader;
			windowLayoutGroup.padding.top = layoutGroupTopPaddingWithoutHeader;
			Image[] array = backgroundImages;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].sprite = backgroundSpriteSmall;
			}
		}
		else
		{
			header.gameObject.SetActive(value: true);
			maskRectTransform.offsetMin = maskOffsetMinWithHeader;
			maskRectTransform.offsetMax = maskOffsetMaxWithHeader;
			windowLayoutGroup.padding.top = layoutGroupTopPaddingWithHeader;
			Image[] array = backgroundImages;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].sprite = backgroundSpriteBig;
			}
		}
	}
}
