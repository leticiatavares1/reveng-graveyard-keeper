using LazyBearTechnology;
using TMPro;
using UnityEngine;

public class UITooltipTextWidget : LazyWidget<UITooltipTextWidgetData>
{
	[SerializeField]
	private TextMeshProUGUI label;

	[SerializeField]
	private TextStyleComponent textStyleComponent;

	public override void Redraw()
	{
		base.Redraw();
		label.text = data.Text;
		label.alignment = data.TextAlignmentOptions;
		if (data.TextStyle != null)
		{
			textStyleComponent.SetTextStyle(data.TextStyle);
			textStyleComponent.ApplyStyle();
		}
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		Draw(new UITooltipTextWidgetData("some text in widget", TextAlignmentOptions.Center, null));
	}
}
