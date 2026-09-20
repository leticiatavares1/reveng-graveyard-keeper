using LazyBearTechnology;

public class UINotesWindowData : LazyWidgetDataBase
{
	public ItemDef NoteItem { get; private set; }

	public UINotesWindowData(ItemDef itemDef)
	{
		NoteItem = itemDef;
	}
}
