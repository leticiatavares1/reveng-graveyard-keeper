using System;

namespace LazyBearTechnology;

public abstract class LazyWidget<T> : LazyWidgetBase where T : LazyWidgetDataBase
{
	protected T data;

	public virtual void Draw(T data)
	{
		SetData(data);
		base.Draw();
		Redraw();
	}

	public override void Draw(LazyWidgetDataBase data)
	{
		Draw(data as T);
	}

	public override void Redraw()
	{
	}

	public override Type GetDataType()
	{
		return typeof(T);
	}

	public override void Hide()
	{
		base.Hide();
	}

	protected virtual void SetData(T data)
	{
		this.data = data;
	}
}
