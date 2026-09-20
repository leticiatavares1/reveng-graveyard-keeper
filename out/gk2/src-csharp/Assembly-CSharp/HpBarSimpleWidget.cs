using System;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class HpBarSimpleWidget : LazyWidget<HpBarSimpleWidgetData>, IUIObjectBubbleWidgetWithoutRebuildingLayout
{
	[SerializeField]
	private LayoutElement layoutElement;

	[SerializeField]
	private Slider slider;

	[SerializeField]
	private Image barImage;

	[SerializeField]
	private RectTransform barImageParentRectTransform;

	[Space]
	[SerializeField]
	private Sprite allySprite;

	[SerializeField]
	private Sprite enemySprite;

	public override void Redraw()
	{
		base.Redraw();
		Vector2 vector = new Vector2((data.CustomWidth > 0f) ? data.CustomWidth : layoutElement.preferredWidth, (data.CustomHeight > 0f) ? data.CustomHeight : layoutElement.preferredHeight);
		layoutElement.preferredWidth = vector.x;
		layoutElement.preferredHeight = vector.y;
		slider.maxValue = data.hpComponent.MaxHpValue;
		int num = BarWigdetUtils.ClampSliderValueToViewableState(Mathf.CeilToInt(barImageParentRectTransform.rect.width), data.hpComponent.Hp, data.hpComponent.MaxHpValue);
		slider.value = num;
		switch (data.Sprite)
		{
		case HpBarSimpleWidgetData.SpriteType.Ally:
			barImage.sprite = allySprite;
			break;
		case HpBarSimpleWidgetData.SpriteType.Enemy:
			barImage.sprite = enemySprite;
			break;
		}
	}

	public override void CustomUpdate()
	{
		int num = BarWigdetUtils.ClampSliderValueToViewableState(Mathf.CeilToInt(barImageParentRectTransform.rect.width), data.hpComponent.Hp, data.hpComponent.MaxHpValue);
		slider.value = num;
	}

	protected override void TestDraw()
	{
		throw new NotImplementedException();
	}
}
