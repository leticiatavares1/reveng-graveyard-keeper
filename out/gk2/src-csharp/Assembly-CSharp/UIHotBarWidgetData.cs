using LazyBearTechnology;

public class UIHotBarWidgetData : LazyWidgetDataBase
{
	public PlayerData PlayerData { get; private set; }

	public bool IsUsable { get; private set; }

	public Item PinnableItem { get; private set; }

	public UIHotBarWidgetData(GameSave gameSave, bool isUsable = false, Item pinnableItem = null)
	{
		PlayerData = gameSave.playerData;
		IsUsable = isUsable;
		PinnableItem = pinnableItem;
	}
}
