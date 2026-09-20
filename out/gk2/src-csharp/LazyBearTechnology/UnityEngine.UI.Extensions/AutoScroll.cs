using System;
using LazyBearTechnology;

namespace UnityEngine.UI.Extensions;

[RequireComponent(typeof(ScrollRect))]
public class AutoScroll : MonoBehaviour
{
	[SerializeField]
	private RectTransform content;

	[SerializeField]
	private AutoScrollTargetType autoScrollTargetType;

	[SerializeField]
	private float borderOffset;

	[Space]
	[SerializeField]
	private float autoScrollTime = 0.3f;

	[SerializeField]
	private GamepadNavigationController gamepadController;

	[Space]
	[SerializeField]
	private RectTransform.Axis direction = RectTransform.Axis.Vertical;

	protected RectTransform scrollRectTransform;

	protected ScrollRect scrollRect;

	private bool isNextAutoscrollInstant;

	private bool skipNextAutoscroll;

	public bool IsNextAutoscrollInstant
	{
		get
		{
			return isNextAutoscrollInstant;
		}
		set
		{
			isNextAutoscrollInstant = value;
		}
	}

	public bool SkipNextAutoscroll
	{
		get
		{
			return skipNextAutoscroll;
		}
		set
		{
			skipNextAutoscroll = value;
		}
	}

	protected void Awake()
	{
		scrollRect = GetComponent<ScrollRect>();
		scrollRectTransform = scrollRect.GetComponent<RectTransform>();
		gamepadController.OnFocusedItemChanged += delegate(GamepadNavigationItem item)
		{
			if (skipNextAutoscroll)
			{
				skipNextAutoscroll = false;
			}
			else
			{
				ScrollToItem(item);
			}
		};
	}

	public void ScrollToItem(GamepadNavigationItem item)
	{
		if (item == null || item.ignoreScroll || !IsItemInsideContent(item))
		{
			return;
		}
		if (scrollRect == null)
		{
			scrollRect = GetComponent<ScrollRect>();
			scrollRectTransform = scrollRect.GetComponent<RectTransform>();
		}
		if (!item.TryGetComponent<RectTransform>(out var component) || !scrollRect.enabled)
		{
			isNextAutoscrollInstant = false;
			return;
		}
		switch (autoScrollTargetType)
		{
		case AutoScrollTargetType.Center:
			if (isNextAutoscrollInstant)
			{
				scrollRect.ScrollToTargetInstant(component, direction);
			}
			else
			{
				scrollRect.ScrollToTarget(component, direction, autoScrollTime, isIndependentUpdate: true);
			}
			break;
		case AutoScrollTargetType.NearestVisiblePosition:
			scrollRect.ScrollToVisiblePosition(component, direction, borderOffset * LazyUI.ScaleFactor, isNextAutoscrollInstant ? 0f : autoScrollTime, isIndependentUpdate: true);
			break;
		default:
			throw new ArgumentOutOfRangeException($"Unhandled value [{autoScrollTargetType}]");
		}
		isNextAutoscrollInstant = false;
	}

	private bool IsItemInsideContent(GamepadNavigationItem item)
	{
		Transform parent = item.transform.parent;
		do
		{
			if (parent == content)
			{
				return true;
			}
			parent = parent.parent;
		}
		while (parent != null);
		return false;
	}
}
