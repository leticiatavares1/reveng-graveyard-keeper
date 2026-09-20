using LazyBearTechnology;

public class UIHotBarSelectionWindowData : LazyWidgetDataBase
{
	public Item Item { get; private set; }

	public UIHotBarWidgetData HotBarWidgetData { get; private set; }

	public UIHotBarSelectionWindowData(GameSave gameSave, Item item)
	{
		Item = item;
		HotBarWidgetData = new UIHotBarWidgetData(gameSave, isUsable: false, item);
	}
}
