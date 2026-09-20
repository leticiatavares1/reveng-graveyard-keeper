using LazyBearTechnology;
using UnityEngine;

public class UITooltipSeparatorWidget : LazyWidget<UITooltipSeparatorWidgetData>, IUIObjectBubbleWidgetWithoutRebuildingLayout
{
	[SerializeField]
	private RectTransform up;

	[SerializeField]
	private RectTransform down;

	public override void Draw(UITooltipSeparatorWidgetData data)
	{
		up.sizeDelta = new Vector2(up.sizeDelta.x, data.spaceUp);
		down.sizeDelta = new Vector2(down.sizeDelta.x, data.spaceDown);
	}

	public override void Hide()
	{
	}

	protected override void TestDraw()
	{
	}
}
