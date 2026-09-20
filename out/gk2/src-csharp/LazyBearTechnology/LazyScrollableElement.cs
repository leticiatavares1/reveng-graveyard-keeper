using System;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class LazyScrollableElement : MonoBehaviour
{
	private Action<LazyScrollableElement> onVisibleAction;

	private Action<LazyScrollableElement> onInvisibleAction;

	private Func<bool> customVisibilityCondition;

	[SerializeField]
	private RectTransform viewport;

	[SerializeField]
	private RectTransform rectTransform;

	[SerializeField]
	private GamepadNavigationItem gamepadNavigationItem;

	private LazyWidgetDataBase data;

	[SerializeField]
	private LazyWidgetBase widget;

	public RectTransform RectTransform
	{
		get
		{
			if (rectTransform == null)
			{
				rectTransform = GetComponent<RectTransform>();
			}
			return rectTransform;
		}
	}

	public GamepadNavigationItem GamepadNavigationItem
	{
		get
		{
			if (gamepadNavigationItem == null)
			{
				gamepadNavigationItem = GetComponent<GamepadNavigationItem>();
			}
			return gamepadNavigationItem;
		}
	}

	public LazyWidgetDataBase Data { get; set; }

	public LazyWidgetBase Widget => widget;

	public void Init(RectTransform viewport, Action<LazyScrollableElement> onVisibleAction, Action<LazyScrollableElement> onInvisibleAction, Func<bool> customVisibilityCondition = null)
	{
		this.viewport = viewport;
		this.onVisibleAction = onVisibleAction;
		this.onInvisibleAction = onInvisibleAction;
		this.customVisibilityCondition = customVisibilityCondition;
	}

	public void SetElement(LazyWidgetBase element)
	{
		element.transform.SetParent(RectTransform);
		RectTransform component = element.GetComponent<RectTransform>();
		component.anchoredPosition = Vector2.zero;
		component.localScale = Vector3.one;
		widget = element;
	}

	public void UnsetElement()
	{
		widget = null;
	}

	public void UpdateSize()
	{
		if (Widget != null)
		{
			RectTransform.sizeDelta = Widget.GetComponent<RectTransform>().sizeDelta;
		}
	}

	public void CheckVisibility()
	{
		if (customVisibilityCondition != null && customVisibilityCondition())
		{
			onVisibleAction?.Invoke(this);
		}
		else if (viewport.IsRectTransformOverlapsOtherRectTransform(RectTransform))
		{
			onVisibleAction?.Invoke(this);
		}
		else
		{
			onInvisibleAction?.Invoke(this);
		}
	}

	public LazyWidgetBase ForceVisibilityAndGetWidget()
	{
		if (widget == null)
		{
			onVisibleAction?.Invoke(this);
		}
		return widget;
	}
}
