using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class UIWorkbenchAdditionWorldIconWidget : LazyWidget<UIWorkbenchAdditionWorldIconWidgetData>, IUIObjectBubbleWidgetWithoutRebuildingLayout
{
	private const string FallbackIconId = "i_b_blueprint_placeholder";

	private const string BackgroundChildName = "Background";

	[SerializeField]
	private Image icon;

	[SerializeField]
	private GameObject strikethrough;

	private GameObject background;

	private Image strikethroughImage;

	private Sprite defaultStrikethroughSprite;

	private Vector2 defaultStrikethroughSize;

	private bool strikethroughDefaultsCached;

	public override void Redraw()
	{
		if (icon != null)
		{
			icon.sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(data.IconId, "i_b_blueprint_placeholder");
			icon.enabled = icon.sprite != null;
		}
		RedrawBackground();
		RedrawStrikethrough();
	}

	private void RedrawBackground()
	{
		if (background == null)
		{
			background = base.transform.Find("Background")?.gameObject;
		}
		if (background != null)
		{
			background.SetActive(data.ShowBackground);
		}
	}

	private void RedrawStrikethrough()
	{
		if (!(strikethrough == null))
		{
			CacheStrikethroughDefaults();
			if (strikethroughImage != null)
			{
				bool flag = !string.IsNullOrEmpty(data.CrossIconId);
				strikethroughImage.sprite = (flag ? (LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(data.CrossIconId) ?? defaultStrikethroughSprite) : defaultStrikethroughSprite);
				strikethroughImage.rectTransform.sizeDelta = ((flag && icon != null) ? icon.rectTransform.sizeDelta : defaultStrikethroughSize);
			}
			strikethrough.SetActive(!data.IsInRange);
		}
	}

	private void CacheStrikethroughDefaults()
	{
		if (!strikethroughDefaultsCached)
		{
			strikethroughDefaultsCached = true;
			strikethroughImage = strikethrough.GetComponent<Image>();
			if (strikethroughImage != null)
			{
				defaultStrikethroughSprite = strikethroughImage.sprite;
				defaultStrikethroughSize = strikethroughImage.rectTransform.sizeDelta;
			}
		}
	}

	protected override void TestDraw()
	{
	}
}
