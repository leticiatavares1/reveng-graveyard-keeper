using System;

public class QuestTreeElementWidgetData : TreeElementBaseWidgetData
{
	public Action<QuestTreeElementWidgetData> onQuestClicked;

	public QuestViewStatus displayViewStatus;

	public bool hideQuestionMark;
}
