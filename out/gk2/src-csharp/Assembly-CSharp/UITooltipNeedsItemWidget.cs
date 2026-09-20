using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class UITooltipNeedsItemWidget : LazyWidget<UITooltipNeedsItemWidgetData>
{
	[SerializeField]
	private Transform cellsParent;

	private List<UITooltipCraftItemCell> items = new List<UITooltipCraftItemCell>();

	public override void Redraw()
	{
		base.Redraw();
		HideCells();
		foreach (UICraftItemCellData craftItemCellsDatum in data.CraftItemCellsData)
		{
			UITooltipCraftItemCell elementFromPool = UIPrefabsPooler.Instance.GetElementFromPool<UITooltipCraftItemCell>(cellsParent);
			elementFromPool.Draw(craftItemCellsDatum, null, isTooltipView: true, data.DrawAsNeedItem);
			elementFromPool.GamepadNavigationItem.group = 1;
			items.Add(elementFromPool);
		}
	}

	public override void Hide()
	{
		HideCells();
		base.Hide();
	}

	private void HideCells()
	{
		foreach (UITooltipCraftItemCell item in items)
		{
			UIPrefabsPooler.Instance.ReleaseElementToPool(item);
		}
		items.Clear();
	}

	protected override void TestDraw()
	{
	}
}
