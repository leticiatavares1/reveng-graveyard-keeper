using TMPro;
using UnityEngine;

public class UICraftWidget : UIBaseCraftWidget<UICraftWidgetData>
{
	[SerializeField]
	private TextMeshProUGUI description;

	[SerializeField]
	private UIMixItemCell boostItemCell;

	public override void Init()
	{
		base.Init();
		base.AddToQueueButton.onClick.AddListener(base.OnPress);
	}

	public override void Redraw()
	{
		base.Redraw();
		boostItemCell.gameObject.SetActive(value: false);
		description.text = data.CraftDefinition.Description;
		base.AddToQueueButton.gameObject.SetActive(!data.CraftDefinition.isAuto && !data.CraftDefinition.isMulticraftDisabled);
		CraftDef craftDefinition = data.CraftDefinition;
		AlchemyMixDef mixDef = craftDefinition as AlchemyMixDef;
		if (mixDef != null && mixDef.BoostCraft != null)
		{
			boostItemCell.gameObject.SetActive(value: true);
			boostItemCell.runesLabel.text = mixDef.BoostCraft.GetBoostRunesAsString();
			boostItemCell.uiItemCell.DrawCustom(mixDef.BoostCraft.GetCraftResultIcon(data.WgoData), 1, interactable: true);
			boostItemCell.uiItemCell.CustomTooltipShowAction = delegate(UIItemCell cell)
			{
				UITooltip.ShowAlchemyBoostInfo(cell.transform as RectTransform, mixDef.BoostCraft);
			};
		}
	}

	public override void DeInit()
	{
		base.DeInit();
		base.AddToQueueButton.onClick.RemoveAllListeners();
	}

	protected override void TestDraw()
	{
	}
}
