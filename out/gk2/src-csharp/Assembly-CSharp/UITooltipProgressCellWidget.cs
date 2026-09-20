using LazyBearTechnology;
using TMPro;
using UnityEngine;

public class UITooltipProgressCellWidget : LazyWidget<UITooltipProgressCellWidgetData>
{
	[SerializeField]
	private UIItemCell uiItemCell;

	[SerializeField]
	private TextMeshProUGUI textLabel;

	public override void Redraw()
	{
		base.Redraw();
		if (data.ItemDefBonus != null)
		{
			uiItemCell.Draw(new Item(data.ItemDefBonus.id));
			textLabel.text = LLBase.L(data.ItemDefBonus.id);
			uiItemCell.gameObject.SetActive(value: true);
			textLabel.gameObject.SetActive(value: true);
		}
		else
		{
			uiItemCell.gameObject.SetActive(value: false);
			textLabel.gameObject.SetActive(value: true);
			textLabel.text = LLBase.L(data.PerkDefBonus.id);
		}
	}

	protected override void TestDraw()
	{
	}
}
