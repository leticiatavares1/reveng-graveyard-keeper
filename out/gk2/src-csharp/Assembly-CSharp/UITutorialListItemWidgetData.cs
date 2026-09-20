using System;
using LazyBearTechnology;

public class UITutorialListItemWidgetData : LazyWidgetDataBase
{
	public string TutorialId { get; }

	public Action<string> OnPressed { get; }

	public UITutorialListItemWidgetData(string tutorialId, Action<string> onPressed)
	{
		TutorialId = tutorialId;
		OnPressed = onPressed;
	}
}
