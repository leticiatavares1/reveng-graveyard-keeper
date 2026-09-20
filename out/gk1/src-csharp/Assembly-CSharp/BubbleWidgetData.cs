public abstract class BubbleWidgetData
{
	public enum WidgetID
	{
		None,
		HPProgress,
		CraftingItem,
		CraftingProgress,
		Fishing,
		Interaction,
		Work,
		Progress,
		Quality
	}

	public WidgetID widget_id;

	public virtual bool IsEmpty()
	{
		return false;
	}

	public virtual void TrySetAlign(NGUIText.Alignment alignment)
	{
	}
}
