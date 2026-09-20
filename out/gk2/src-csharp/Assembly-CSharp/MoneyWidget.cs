using LazyBearTechnology;
using TMPro;
using UnityEngine;

public class MoneyWidget : LazyWidget<MoneyWidgetData>
{
	[SerializeField]
	private TextMeshProUGUI moneyLabel;

	public TextMeshProUGUI MoneyLabel => moneyLabel;

	public override void Redraw()
	{
		base.Redraw();
		moneyLabel.text = Trading.FormatMoney(data.Money(), printZero: true, " ", GameResIconType.MoneyBig);
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		MoneyWidgetData moneyWidgetData = new MoneyWidgetData();
		moneyWidgetData.Money = () => 11111;
		Draw(moneyWidgetData);
	}
}
