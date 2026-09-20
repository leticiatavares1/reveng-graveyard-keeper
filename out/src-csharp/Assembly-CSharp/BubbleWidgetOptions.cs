using System;
using System.Collections.Generic;
using UnityEngine;

public class BubbleWidgetOptions : BubbleWidget<BubbleWidgetDataOptions>
{
	[HideInInspector]
	[SerializeField]
	protected UITable table;

	[SerializeField]
	[HideInInspector]
	protected BubbleWidgetOptionsItem item_prefab;

	private List<BubbleWidgetOptionsItem> options = new List<BubbleWidgetOptionsItem>();

	public override void Init()
	{
		table = GetComponentInChildren<UITable>();
		item_prefab = GetComponentInChildren<BubbleWidgetOptionsItem>();
		item_prefab.Init();
		base.Init();
	}

	public override void Draw(BubbleWidgetDataOptions data)
	{
		if (table == null || item_prefab == null)
		{
			Init();
		}
		base.data = data;
		options.Clear();
		foreach (BubbleWidgetDataOptions.OptionData option in data.options)
		{
			BubbleWidgetOptionsItem bubbleWidgetOptionsItem = item_prefab.Copy();
			bubbleWidgetOptionsItem.Draw(option.name, delegate
			{
				OnOptionSelected(option.callback);
			}, option.enabled);
			options.Add(bubbleWidgetOptionsItem);
		}
		item_prefab.Deactivate();
		table.Reposition();
	}

	private void OnOptionSelected(Action opt_delegate)
	{
		opt_delegate.TryInvoke();
		data.on_hide.TryInvoke();
	}

	public override Vector2 GetSize()
	{
		if (!initialized || ui_widget == null)
		{
			Init();
		}
		Vector2 zero = Vector2.zero;
		if (!Application.isPlaying)
		{
			options.Clear();
			options.AddRange(GetComponentsInChildren<BubbleWidgetOptionsItem>());
		}
		foreach (BubbleWidgetOptionsItem option in options)
		{
			Vector2 size = option.GetSize();
			zero.y += size.y;
			if (size.x > zero.x)
			{
				zero.x = size.x;
			}
		}
		zero.y -= 2 * options.Count;
		ui_widget.width = Mathf.CeilToInt(zero.x);
		ui_widget.height = Mathf.CeilToInt(zero.y);
		return base.GetSize();
	}
}
