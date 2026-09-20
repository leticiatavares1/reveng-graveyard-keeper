using LazyBearTechnology;
using TMPro;
using UnityEngine;

public class UIModulesLimitsWidget : LazyWidget<UIModulesLimitsWidgetData>, IUIObjectBubbleWidgetWithoutRebuildingLayout
{
	[SerializeField]
	private TextMeshProUGUI label;

	[SerializeField]
	private TextStyle slashStyle;

	public override void Redraw()
	{
		base.Redraw();
		label.text = string.Format("{0}{1}{2}", data.ModulesCount, slashStyle.ApplyStyleToString("/", staticFont: true), data.ModulesLimit);
	}

	protected override void TestDraw()
	{
	}
}
