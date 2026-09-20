using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class HpBarPlayerWidget : LazyWidget<HpBarPlayerWidgetData>, IUIObjectBubbleWidgetWithoutRebuildingLayout
{
	[SerializeField]
	private LayoutElement layoutElement;

	[SerializeField]
	private Slider slider;

	[SerializeField]
	private RectTransform barImageParentRectTransform;

	public override void Redraw()
	{
		base.Redraw();
		Vector2 vector = new Vector2((data.CustomWidth > 0f) ? data.CustomWidth : layoutElement.preferredWidth, (data.CustomHeight > 0f) ? data.CustomHeight : layoutElement.preferredHeight);
		layoutElement.preferredWidth = vector.x;
		layoutElement.preferredHeight = vector.y;
		slider.maxValue = data.hpComponent.MaxHpValue;
		int num = BarWigdetUtils.ClampSliderValueToViewableState(Mathf.CeilToInt(barImageParentRectTransform.rect.width), data.hpComponent.Hp, data.hpComponent.MaxHpValue);
		slider.value = num;
	}

	public override void CustomUpdate()
	{
		int num = BarWigdetUtils.ClampSliderValueToViewableState(Mathf.CeilToInt(barImageParentRectTransform.rect.width), data.hpComponent.Hp, data.hpComponent.MaxHpValue);
		slider.value = num;
	}

	protected override void TestDraw()
	{
	}
}
