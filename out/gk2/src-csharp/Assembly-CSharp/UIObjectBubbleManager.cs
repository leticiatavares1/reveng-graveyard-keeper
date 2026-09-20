using System;
using System.Collections.Generic;
using Cinemachine;
using LazyBearTechnology;
using UnityEngine;

[DefaultExecutionOrder(-100)]
[RequireComponent(typeof(Canvas))]
public class UIObjectBubbleManager : MonoBehaviour, ILazyGUIElement
{
	private static UIObjectBubbleManager instance;

	[SerializeField]
	private List<UIObjectBubble> displayedObjectBubbles = new List<UIObjectBubble>();

	private Dictionary<SGuid, UIObjectBubble> displayedBubblesCache = new Dictionary<SGuid, UIObjectBubble>();

	private Dictionary<long, List<UIObjectBubble>> displayedBubblesCacheByDepth = new Dictionary<long, List<UIObjectBubble>>();

	private readonly Dictionary<SGuid, IBubbleDrawable> pendingDisplayTargets = new Dictionary<SGuid, IBubbleDrawable>();

	private readonly List<long> sortedDepthKeysBuffer = new List<long>();

	private readonly List<IBubbleDrawable> pendingDisplayTargetsBuffer = new List<IBubbleDrawable>();

	[SerializeField]
	private UIObjectBubble prefab;

	[SerializeField]
	private Transform poolParent;

	private Pool pool;

	private Canvas canvas;

	public SGuid PutOnTopTargetId { get; set; } = SGuid.Empty;


	public static UIObjectBubbleManager Instance => instance;

	public void Init()
	{
		if (instance != null)
		{
			Debug.LogError("Found duplication of UIObjectBubbleManager");
			return;
		}
		instance = this;
		pool = new Pool(prefab, poolParent, 0);
		canvas = GetComponent<Canvas>();
		canvas.overrideSorting = true;
		canvas.sortingOrder = 40;
		CinemachineCore.CameraUpdatedEvent.AddListener(UpdatePos);
	}

	public void Clear()
	{
		foreach (UIObjectBubble displayedObjectBubble in displayedObjectBubbles)
		{
			displayedObjectBubble.ForceCancelDelayedHides();
			displayedObjectBubble.Hide();
			pool.ReleaseObject(displayedObjectBubble);
		}
		displayedObjectBubbles.Clear();
		displayedBubblesCache.Clear();
		displayedBubblesCacheByDepth.Clear();
		pendingDisplayTargets.Clear();
		pendingDisplayTargetsBuffer.Clear();
	}

	public void RequestDisplay(IBubbleDrawable target)
	{
		if (target != null && !IsTargetDestroyed(target))
		{
			pendingDisplayTargets[target.BubbleDrawableUniqueId] = target;
		}
	}

	public void Display(IBubbleDrawable target)
	{
		List<LazyWidgetDataBase> bubbleDrawableWidgets = target.BubbleDrawableWidgets;
		if (bubbleDrawableWidgets.Count == 0)
		{
			Hide(target);
			return;
		}
		if (displayedBubblesCache.TryGetValue(target.BubbleDrawableUniqueId, out var value))
		{
			value.TryCompareDataAndRedrawExistingOrDisplay(target, bubbleDrawableWidgets);
			return;
		}
		UIObjectBubble orCreateObject = pool.GetOrCreateObject<UIObjectBubble>();
		orCreateObject.transform.SetParent(base.transform);
		displayedObjectBubbles.Add(orCreateObject);
		displayedBubblesCache.Add(target.BubbleDrawableUniqueId, orCreateObject);
		orCreateObject.Display(target, bubbleDrawableWidgets);
		long depthKeyFromTarget = GetDepthKeyFromTarget(target);
		orCreateObject.SetDepthKey(depthKeyFromTarget);
		if (!displayedBubblesCacheByDepth.TryGetValue(depthKeyFromTarget, out var value2))
		{
			value2 = new List<UIObjectBubble>();
			displayedBubblesCacheByDepth.Add(depthKeyFromTarget, value2);
		}
		value2.Add(orCreateObject);
		value2.Sort(CompareBubblesByTargetId);
	}

	public void HideWidget<T>(IBubbleDrawable target) where T : LazyWidgetDataBase
	{
		if (displayedBubblesCache.TryGetValue(target.BubbleDrawableUniqueId, out var value) && !value.TryDelayHideWidget<T>(delegate
		{
			HideWidget<T>(target);
		}))
		{
			value.HideParticularWidget<T>();
			if (value.DisplayedWidgetsCount == 0)
			{
				Hide(target);
			}
		}
	}

	public void HideWidget(IBubbleDrawable target, LazyWidgetDataBase widgetData)
	{
		if (displayedBubblesCache.TryGetValue(target.BubbleDrawableUniqueId, out var value) && !value.TryDelayHideWidget(widgetData.GetType(), delegate
		{
			HideWidget(target, widgetData);
		}))
		{
			value.HideParticularWidget(widgetData);
			if (value.DisplayedWidgetsCount == 0)
			{
				Hide(target);
			}
		}
	}

	public bool TryGetDisplayedBubble(SGuid uniqueId, out UIObjectBubble bubble)
	{
		bubble = null;
		if (SGuid.IsNullOrEmpty(uniqueId))
		{
			return false;
		}
		return displayedBubblesCache.TryGetValue(uniqueId, out bubble);
	}

	public void Hide(IBubbleDrawable target)
	{
		pendingDisplayTargets.Remove(target.BubbleDrawableUniqueId);
		if (!displayedBubblesCache.TryGetValue(target.BubbleDrawableUniqueId, out var value) || value.TryDelayHide(delegate
		{
			Hide(target);
		}))
		{
			return;
		}
		displayedObjectBubbles.Remove(value);
		displayedBubblesCache.Remove(target.BubbleDrawableUniqueId);
		value.Hide();
		long depthKey = value.DepthKey;
		if (displayedBubblesCacheByDepth.TryGetValue(depthKey, out var value2))
		{
			value2.Remove(value);
			if (value2.Count == 0)
			{
				displayedBubblesCacheByDepth.Remove(depthKey);
			}
		}
		pool.ReleaseObject(value);
	}

	private static long GetDepthKey(float worldZ)
	{
		return Convert.ToInt64(Math.Round(worldZ * 10f));
	}

	private static long GetDepthKeyFromTarget(IBubbleDrawable target)
	{
		return GetDepthKey(target.BubbleDrawablePosition.z);
	}

	private static int CompareBubblesByTargetId(UIObjectBubble a, UIObjectBubble b)
	{
		return a.Target.BubbleDrawableUniqueId.Guid.CompareTo(b.Target.BubbleDrawableUniqueId.Guid);
	}

	private static bool IsTargetDestroyed(IBubbleDrawable target)
	{
		if (target is UnityEngine.Object @object)
		{
			return @object == null;
		}
		return false;
	}

	private void FlushPendingDisplays()
	{
		if (pendingDisplayTargets.Count == 0)
		{
			return;
		}
		pendingDisplayTargetsBuffer.Clear();
		foreach (IBubbleDrawable value in pendingDisplayTargets.Values)
		{
			pendingDisplayTargetsBuffer.Add(value);
		}
		pendingDisplayTargets.Clear();
		foreach (IBubbleDrawable item in pendingDisplayTargetsBuffer)
		{
			if (!IsTargetDestroyed(item))
			{
				Display(item);
			}
		}
		pendingDisplayTargetsBuffer.Clear();
		UpdatePos(null);
	}

	private void ApplyDepthSorting()
	{
		sortedDepthKeysBuffer.Clear();
		foreach (long key in displayedBubblesCacheByDepth.Keys)
		{
			sortedDepthKeysBuffer.Add(key);
		}
		sortedDepthKeysBuffer.Sort((long a, long b) => b.CompareTo(a));
		foreach (long item in sortedDepthKeysBuffer)
		{
			foreach (UIObjectBubble item2 in displayedBubblesCacheByDepth[item])
			{
				if (!item2.IsOutOfScreen && item2.gameObject.activeSelf)
				{
					item2.transform.SetAsLastSibling();
				}
			}
		}
	}

	private void UpdatePos(CinemachineBrain brain)
	{
		foreach (UIObjectBubble displayedObjectBubble in displayedObjectBubbles)
		{
			displayedObjectBubble.UpdatePos();
		}
		ApplyDepthSorting();
		if (!SGuid.IsNullOrEmpty(PutOnTopTargetId) && displayedBubblesCache.TryGetValue(PutOnTopTargetId, out var value))
		{
			value.transform.SetAsLastSibling();
		}
	}

	private void Update()
	{
		displayedObjectBubbles.ForEach(delegate(UIObjectBubble item)
		{
			item.CustomUpdate();
		});
	}

	private void LateUpdate()
	{
		FlushPendingDisplays();
	}

	private void OnDestroy()
	{
		CinemachineCore.CameraUpdatedEvent.RemoveListener(UpdatePos);
	}
}
