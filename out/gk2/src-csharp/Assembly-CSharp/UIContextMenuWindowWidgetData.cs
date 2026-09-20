using System;
using LazyBearTechnology;

public class UIContextMenuWindowWidgetData : LazyWidgetDataBase
{
	public string name;

	public Action callback;

	public bool enabled;

	public UIContextMenuWindowWidgetData(string name, Action callback, bool enabled = true)
	{
		this.name = name;
		this.callback = callback;
		this.enabled = enabled;
	}
}
