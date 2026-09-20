using System;
using UnityEngine;

[RequireComponent(typeof(PixelPerfectGUI))]
public abstract class BubbleWidget<T> : BubbleWidgetBase where T : BubbleWidgetData
{
	protected T data;

	[HideInInspector]
	[SerializeField]
	protected UIWidget ui_widget;

	[HideInInspector]
	[SerializeField]
	protected bool initialized;

	public abstract void Draw(T data);

	public override void Init()
	{
		ui_widget = GetComponent<UIWidget>();
		initialized = true;
	}

	public override Type GetWidgetType()
	{
		return typeof(T);
	}

	public override void BaseDraw(BubbleWidgetData data)
	{
		Draw(data as T);
	}

	public override Vector2 GetSize()
	{
		if (!initialized || ui_widget == null)
		{
			Init();
		}
		return ui_widget.localSize;
	}
}
