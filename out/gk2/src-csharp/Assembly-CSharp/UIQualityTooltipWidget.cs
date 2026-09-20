using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIQualityTooltipWidget : LazyWidget<UIQualityTooltipWidgetData>
{
	[SerializeField]
	private TextMeshProUGUI iconLabel;

	[SerializeField]
	private TextMeshProUGUI label;

	public override void Draw(UIQualityTooltipWidgetData data)
	{
		base.Draw(data);
		iconLabel.text = data.IconName.FontIcon();
		label.text = (data.HasValueOverride ? data.ValueText : data.WgoData.Quality.ToInvariantCultureString());
	}

	public override void Hide()
	{
	}

	protected override void TestDraw()
	{
	}
}
