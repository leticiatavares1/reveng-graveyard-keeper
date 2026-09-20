public class BubbleWidgetSeparatorData : BubbleWidgetData
{
	public NGUIText.Alignment alignment = NGUIText.Alignment.Center;

	public override void TrySetAlign(NGUIText.Alignment alignment)
	{
		this.alignment = alignment;
	}
}
