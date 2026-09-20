public class BubbleWidgetSeparator : BubbleWidget<BubbleWidgetSeparatorData>
{
	public override void Draw(BubbleWidgetSeparatorData data)
	{
		switch (data.alignment)
		{
		case NGUIText.Alignment.Left:
			GetComponent<UIWidget>().pivot = UIWidget.Pivot.Left;
			break;
		case NGUIText.Alignment.Center:
			GetComponent<UIWidget>().pivot = UIWidget.Pivot.Center;
			break;
		case NGUIText.Alignment.Right:
			GetComponent<UIWidget>().pivot = UIWidget.Pivot.Right;
			break;
		}
	}
}
