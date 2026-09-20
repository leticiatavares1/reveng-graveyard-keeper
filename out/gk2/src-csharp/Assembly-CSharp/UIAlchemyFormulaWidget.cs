using LazyBearTechnology;
using TMPro;
using UnityEngine;

public class UIAlchemyFormulaWidget : LazyWidget<UIAlchemyFormulaWidgetData>
{
	[SerializeField]
	private UIItemCell itemCell;

	[SerializeField]
	private TextMeshProUGUI nameLabel;

	[SerializeField]
	private TextMeshProUGUI runesLabel;

	public override void Redraw()
	{
		base.Redraw();
		nameLabel.text = LLBase.L(data.AlchemyFormulaDef.id);
		runesLabel.text = data.AlchemyFormulaDef.GetRunesAsString();
		itemCell.Draw(new Item(data.AlchemyFormulaDef.ItemDef.id));
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		UIAlchemyFormulaWidgetData uIAlchemyFormulaWidgetData = new UIAlchemyFormulaWidgetData();
		uIAlchemyFormulaWidgetData.AlchemyFormulaDef = GameBalance.Me.GetData<AlchemyFormulaDef>("heal_potion");
		Draw(uIAlchemyFormulaWidgetData);
	}
}
