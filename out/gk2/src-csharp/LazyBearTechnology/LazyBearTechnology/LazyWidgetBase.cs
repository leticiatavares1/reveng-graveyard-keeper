using System;
using System.Collections.Generic;
using UnityEngine;

namespace LazyBearTechnology;

public abstract class LazyWidgetBase : MonoBehaviour, ILazyGUIElement
{
	public virtual void Init()
	{
	}

	public virtual void DeInit()
	{
	}

	public virtual void Draw()
	{
		base.gameObject.SetActive(value: true);
	}

	public abstract void Draw(LazyWidgetDataBase data);

	public abstract void Redraw();

	public abstract Type GetDataType();

	public virtual void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	public virtual void CustomUpdate()
	{
	}

	public virtual List<LazyGameKeyTip> GetTips(GamepadNavigationItem gamepadNavigationItem)
	{
		return new List<LazyGameKeyTip>();
	}

	[LazyUITest]
	protected abstract void TestDraw();
}
