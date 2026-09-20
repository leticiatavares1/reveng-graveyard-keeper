using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class UIInteractionHintWidget : LazyWidget<UIInteractionHintWidgetData>
{
	[SerializeField]
	private UIInteractionHintRow row1;

	[SerializeField]
	private UIInteractionHintRow row2;

	public override void Redraw()
	{
		row1.Draw(data.RowData1);
		if (data.RowData2 != null)
		{
			row2.Draw(data.RowData2);
		}
		else
		{
			row2.Hide();
		}
		((RectTransform)base.transform).EnableLayoutGroupsAndRefreshContentFitter();
	}

	public override void Hide()
	{
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		Draw(new UIInteractionHintWidgetData(new UIInteractionHintRowWidgetData(new InteractionInfo(LLBase.L("hint_interaction")))));
	}
}
