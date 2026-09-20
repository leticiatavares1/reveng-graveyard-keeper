using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIConveyorNoPowerIconWidget : LazyWidget<UIConveyorNoPowerIconWidgetData>, IUIObjectBubbleWidgetWithoutRebuildingLayout
{
	private const string IconId = "gear";

	private static readonly Color TemporaryTint = Color.red;

	[SerializeField]
	private TextMeshProUGUI iconLabel;

	public override void Redraw()
	{
		if (!(iconLabel == null))
		{
			iconLabel.color = TemporaryTint;
			iconLabel.text = "gear".FontIcon();
		}
	}

	protected override void TestDraw()
	{
	}
}
