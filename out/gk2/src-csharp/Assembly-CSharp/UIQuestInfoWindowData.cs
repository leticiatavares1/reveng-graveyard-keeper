using LazyBearTechnology;

public class UIQuestInfoWindowData : LazyWidgetDataBase
{
	public QuestData QuestData { get; private set; }

	public UIQuestInfoWindowData(QuestData questData)
	{
		QuestData = questData;
	}
}
