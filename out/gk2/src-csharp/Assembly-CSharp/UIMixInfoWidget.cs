using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class UIMixInfoWidget : LazyWidget<UIMixInfoWidgetData>
{
	[SerializeField]
	private Transform cellsParent;

	private List<UITooltipMixItemCell> ingredients;

	private UITooltipMixItemCell boostItemCell;

	public override void Redraw()
	{
		base.Redraw();
		if (boostItemCell == null)
		{
			boostItemCell = UIPrefabsPooler.Instance.GetElementFromPool<UITooltipMixItemCell>(cellsParent);
		}
		if (ingredients == null)
		{
			ingredients = new List<UITooltipMixItemCell>();
			for (int i = 0; i < 3; i++)
			{
				UITooltipMixItemCell elementFromPool = UIPrefabsPooler.Instance.GetElementFromPool<UITooltipMixItemCell>(cellsParent);
				ingredients.Add(elementFromPool);
			}
		}
		for (int j = 0; j < ingredients.Count; j++)
		{
			ingredients[j].gameObject.SetActive(value: false);
		}
		boostItemCell.gameObject.SetActive(value: false);
		for (int k = 0; k < data.MixDef.ingredients.Length; k++)
		{
			ingredients[k].gameObject.SetActive(value: true);
			ItemDef itemDef = GameBalance.Me.GetData<ItemDef>(data.MixDef.ingredients[k]);
			ingredients[k].uiItemCell.Draw(new Item(itemDef.id));
			ingredients[k].runesLabel.text = itemDef.GetRunesAsString();
			ingredients[k].runesLabel.gameObject.SetActive(value: false);
		}
		if (data.MixDef.BoostCraft != null)
		{
			boostItemCell.gameObject.SetActive(value: true);
			boostItemCell.runesLabel.text = data.MixDef.BoostCraft.GetBoostRunesAsString();
			boostItemCell.uiItemCell.DrawCustom(data.MixDef.BoostCraft.GetCraftResultIcon(), 1);
			boostItemCell.uiItemCell.CustomTooltipShowAction = delegate(UIItemCell cell)
			{
				UITooltip.ShowAlchemyBoostInfo(cell.transform as RectTransform, data.MixDef.BoostCraft);
			};
			boostItemCell.runesLabel.gameObject.SetActive(value: false);
		}
	}

	protected override void TestDraw()
	{
	}
}
