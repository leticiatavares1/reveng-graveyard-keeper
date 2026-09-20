using System;
using LazyBearTechnology;

public class UITutorialWindowData : LazyWidgetDataBase
{
	public string Page { get; set; }

	public Action OnCompleteCallback { get; set; }

	public bool CanCloseFromAnyPage { get; set; }

	public UITutorialWindowData(string pageId, Action onComplete = null, bool canCloseFromAnyPage = false)
	{
		Page = pageId;
		OnCompleteCallback = onComplete;
		CanCloseFromAnyPage = canCloseFromAnyPage;
	}
}
