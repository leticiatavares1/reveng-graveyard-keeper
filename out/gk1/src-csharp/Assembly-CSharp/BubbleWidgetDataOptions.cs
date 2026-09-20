using System;
using System.Collections.Generic;

public class BubbleWidgetDataOptions : BubbleWidgetData
{
	public struct OptionData
	{
		public string name;

		public Action callback;

		public bool enabled;

		public OptionData(string name, Action callback, bool enabled = true)
		{
			this.name = name;
			this.callback = callback;
			this.enabled = enabled;
		}
	}

	public List<OptionData> options = new List<OptionData>();

	public Action on_hide;

	public BubbleWidgetDataOptions()
	{
	}

	public BubbleWidgetDataOptions(string name, Action callback, bool enabled = true)
	{
		AddOption(name, callback, enabled);
	}

	public BubbleWidgetDataOptions(string[] names, Action[] callbacks)
	{
		for (int i = 0; i < names.Length; i++)
		{
			AddOption(names[i], callbacks[i]);
		}
	}

	public void AddOption(string name, Action callback, bool enabled = true)
	{
		options.Add(new OptionData(name, callback, enabled));
	}
}
