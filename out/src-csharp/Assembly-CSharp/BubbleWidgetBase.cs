using System;
using UnityEngine;

public abstract class BubbleWidgetBase : MonoBehaviour
{
	public abstract void Init();

	public abstract Vector2 GetSize();

	public abstract Type GetWidgetType();

	public abstract void BaseDraw(BubbleWidgetData data);

	public virtual void UpdateWidget()
	{
	}
}
