using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIGraveElementWidget : LazyWidget<UIGraveElementWidgetData>
{
	[SerializeField]
	private UIInsertItemCell insertItemCell;

	[SerializeField]
	private TextMeshProUGUI description;

	[SerializeField]
	private TextMeshProUGUI qualityValue;

	[SerializeField]
	private Image qualityType;

	[SerializeField]
	private TextStyle activeTextStyleValue;

	[SerializeField]
	private TextStyle inactiveTextStyleValue;

	[SerializeField]
	private TextStyle activeTextStyleText;

	[SerializeField]
	private TextStyle inactiveTextStyleText;

	public void StartItemCellSelectionBlinking()
	{
		insertItemCell.UIItemCell.StartSelectionBlinking();
	}

	public override void Redraw()
	{
		base.Redraw();
		description.text = "";
		if (!data.IsEmpty)
		{
			insertItemCell.Draw(data.GraveElementItem, data.HasRequiredTool, data.RequiredTool);
			description.text = LLBase.L(data.GraveElementItem.id);
			activeTextStyleText.ApplyStyle(description);
			activeTextStyleValue.ApplyStyle(qualityValue);
			qualityType.gameObject.SetActive(value: true);
			qualityValue.text = data.Quality.ToString();
		}
		else
		{
			insertItemCell.DrawEmpty(data.HasRequiredTool, data.RequiredTool);
			inactiveTextStyleText.ApplyStyle(description);
			inactiveTextStyleValue.ApplyStyle(qualityValue);
			qualityType.gameObject.SetActive(value: false);
			description.text = LLBase.L(data.EmptyDescriptionLocale);
			qualityValue.text = "0";
		}
		insertItemCell.UIItemCell.OnItemCellPress = OnElementClicked;
	}

	private void OnElementClicked(UIItemCell itemCell)
	{
		data.onElementClicked?.Invoke();
	}

	protected override void TestDraw()
	{
	}
}
