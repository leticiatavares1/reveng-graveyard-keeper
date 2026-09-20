using System.Collections.Generic;
using DG.Tweening;

namespace UnityEngine.UI;

public static class UIExtensions
{
	private static Vector3[] cornersCached = new Vector3[4];

	public static void ResetPosition(this ScrollRect scrollRect)
	{
		scrollRect.verticalNormalizedPosition = 1f;
		scrollRect.horizontalNormalizedPosition = 1f;
	}

	public static void RefreshContentFitter(this RectTransform transform)
	{
		if (transform == null || !transform.gameObject.activeSelf)
		{
			return;
		}
		for (int i = 0; i < transform.childCount; i++)
		{
			if (transform.GetChild(i) is RectTransform transform2)
			{
				transform2.RefreshContentFitter();
			}
		}
		LayoutGroup component = transform.GetComponent<LayoutGroup>();
		ContentSizeFitter component2 = transform.GetComponent<ContentSizeFitter>();
		if (component != null)
		{
			component.CalculateLayoutInputHorizontal();
			component.CalculateLayoutInputVertical();
			component.SetLayoutHorizontal();
			component.SetLayoutVertical();
		}
		if (component2 != null)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(transform);
		}
	}

	public static void EnableLayoutGroupsAndRefreshContentFitter(this RectTransform transform)
	{
		if (transform == null || !transform.gameObject.activeSelf)
		{
			return;
		}
		List<LayoutGroup> list = new List<LayoutGroup>();
		List<ContentSizeFitter> list2 = new List<ContentSizeFitter>();
		List<LayoutElement> list3 = new List<LayoutElement>();
		transform.GetComponentsInChildren(includeInactive: true, list);
		transform.GetComponentsInChildren(includeInactive: true, list2);
		transform.GetComponentsInChildren(includeInactive: true, list3);
		foreach (LayoutGroup item in list)
		{
			item.enabled = true;
		}
		foreach (ContentSizeFitter item2 in list2)
		{
			item2.enabled = true;
		}
		foreach (LayoutElement item3 in list3)
		{
			item3.enabled = true;
		}
		for (int i = 0; i < transform.childCount; i++)
		{
			if (transform.GetChild(i) is RectTransform transform2)
			{
				transform2.RefreshContentFitter();
			}
		}
		LayoutGroup component = transform.GetComponent<LayoutGroup>();
		ContentSizeFitter component2 = transform.GetComponent<ContentSizeFitter>();
		if (component != null)
		{
			component.CalculateLayoutInputHorizontal();
			component.CalculateLayoutInputVertical();
			component.SetLayoutHorizontal();
			component.SetLayoutVertical();
		}
		if (component2 != null)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(transform);
		}
	}

	public static void RefreshContentFitterAndDisable(this RectTransform transform)
	{
		if (transform == null || !transform.gameObject.activeSelf)
		{
			return;
		}
		List<LayoutGroup> list = new List<LayoutGroup>();
		List<ContentSizeFitter> list2 = new List<ContentSizeFitter>();
		List<LayoutElement> list3 = new List<LayoutElement>();
		transform.GetComponentsInChildren(includeInactive: true, list);
		transform.GetComponentsInChildren(includeInactive: true, list2);
		transform.GetComponentsInChildren(includeInactive: true, list3);
		foreach (LayoutGroup item in list)
		{
			item.enabled = true;
		}
		foreach (ContentSizeFitter item2 in list2)
		{
			item2.enabled = true;
		}
		foreach (LayoutElement item3 in list3)
		{
			item3.enabled = true;
		}
		transform.RefreshContentFitter();
		foreach (LayoutGroup item4 in list)
		{
			item4.enabled = false;
		}
		foreach (ContentSizeFitter item5 in list2)
		{
			item5.enabled = false;
		}
		foreach (LayoutElement item6 in list3)
		{
			item6.enabled = false;
		}
	}

	public static void ScrollToTarget(this ScrollRect scrollRect, RectTransform target, RectTransform.Axis axis, float autoScrollTime, bool isIndependentUpdate = false)
	{
		RectTransform viewport = scrollRect.viewport;
		RectTransform rectTransform = (viewport ? viewport : scrollRect.GetComponent<RectTransform>());
		Rect rect = rectTransform.rect;
		Bounds bounds = target.TransformBoundsTo(rectTransform);
		if (axis == RectTransform.Axis.Vertical)
		{
			float distance = rect.center.y - bounds.center.y;
			float value = scrollRect.verticalNormalizedPosition - scrollRect.NormalizeScrollDistance(axis, distance);
			scrollRect.DOVerticalNormalizedPos(Mathf.Clamp(value, 0f, 1f), autoScrollTime).SetUpdate(isIndependentUpdate);
		}
		else
		{
			float distance2 = rect.center.x - bounds.center.x;
			float value2 = scrollRect.horizontalNormalizedPosition - scrollRect.NormalizeScrollDistance(axis, distance2);
			scrollRect.DOHorizontalNormalizedPos(Mathf.Clamp(value2, 0f, 1f), autoScrollTime).SetUpdate(isIndependentUpdate);
		}
	}

	public static void ScrollToTarget(this ScrollRect scrollRect, RectTransform target, RectTransform.Axis axis, float autoScrollTime, out Tween tween, bool isIndependentUpdate = false)
	{
		RectTransform viewport = scrollRect.viewport;
		RectTransform rectTransform = (viewport ? viewport : scrollRect.GetComponent<RectTransform>());
		Rect rect = rectTransform.rect;
		Bounds bounds = target.TransformBoundsTo(rectTransform);
		if (axis == RectTransform.Axis.Vertical)
		{
			float distance = rect.center.y - bounds.center.y;
			float value = scrollRect.verticalNormalizedPosition - scrollRect.NormalizeScrollDistance(axis, distance);
			tween = scrollRect.DOVerticalNormalizedPos(Mathf.Clamp(value, 0f, 1f), autoScrollTime).SetUpdate(isIndependentUpdate);
		}
		else
		{
			float distance2 = rect.center.x - bounds.center.x;
			float value2 = scrollRect.horizontalNormalizedPosition - scrollRect.NormalizeScrollDistance(axis, distance2);
			tween = scrollRect.DOHorizontalNormalizedPos(Mathf.Clamp(value2, 0f, 1f), autoScrollTime).SetUpdate(isIndependentUpdate);
		}
	}

	public static void ScrollToVisiblePosition(this ScrollRect scrollRect, RectTransform target, RectTransform.Axis axis, float borderOffset, float autoScrollTime, bool isIndependentUpdate = false)
	{
		RectTransform viewport = scrollRect.viewport;
		RectTransform rectTransform = (viewport ? viewport : scrollRect.GetComponent<RectTransform>());
		Rect rect = rectTransform.rect;
		Bounds bounds = target.TransformBoundsTo(rectTransform);
		if (axis == RectTransform.Axis.Vertical)
		{
			float num = rect.min.y - bounds.min.y + borderOffset;
			float num2 = rect.max.y - bounds.max.y - borderOffset;
			if (!(num < 0f) || !(num2 > 0f))
			{
				float distance = ((Mathf.Abs(num2) < Mathf.Abs(num)) ? num2 : num);
				float value = scrollRect.verticalNormalizedPosition - scrollRect.NormalizeScrollDistance(axis, distance);
				scrollRect.DOVerticalNormalizedPos(Mathf.Clamp(value, 0f, 1f), autoScrollTime).SetUpdate(isIndependentUpdate);
			}
		}
		else
		{
			float num3 = rect.min.x - bounds.min.x + borderOffset;
			float num4 = rect.max.x - bounds.max.x - borderOffset;
			if (!(num3 < 0f) || !(num4 > 0f))
			{
				float distance2 = ((Mathf.Abs(num4) < Mathf.Abs(num3)) ? num4 : num3);
				float value2 = scrollRect.horizontalNormalizedPosition - scrollRect.NormalizeScrollDistance(axis, distance2);
				scrollRect.DOHorizontalNormalizedPos(Mathf.Clamp(value2, 0f, 1f), autoScrollTime).SetUpdate(isIndependentUpdate);
			}
		}
	}

	public static void ScrollToVisiblePosition(this ScrollRect scrollRect, RectTransform target, RectTransform.Axis axis, float borderOffset, float autoScrollTime, out Tween tween, bool isIndependentUpdate = false)
	{
		tween = null;
		RectTransform viewport = scrollRect.viewport;
		RectTransform rectTransform = (viewport ? viewport : scrollRect.GetComponent<RectTransform>());
		Rect rect = rectTransform.rect;
		Bounds bounds = target.TransformBoundsTo(rectTransform);
		if (axis == RectTransform.Axis.Vertical)
		{
			float num = rect.min.y - bounds.min.y + borderOffset;
			float num2 = rect.max.y - bounds.max.y - borderOffset;
			if (!(num < 0f) || !(num2 > 0f))
			{
				float distance = ((Mathf.Abs(num2) < Mathf.Abs(num)) ? num2 : num);
				float value = scrollRect.verticalNormalizedPosition - scrollRect.NormalizeScrollDistance(axis, distance);
				tween = scrollRect.DOVerticalNormalizedPos(Mathf.Clamp(value, 0f, 1f), autoScrollTime).SetUpdate(isIndependentUpdate);
			}
		}
		else
		{
			float num3 = rect.min.x - bounds.min.x + borderOffset;
			float num4 = rect.max.x - bounds.max.x - borderOffset;
			if (!(num3 < 0f) || !(num4 > 0f))
			{
				float distance2 = ((Mathf.Abs(num4) < Mathf.Abs(num3)) ? num4 : num3);
				float value2 = scrollRect.horizontalNormalizedPosition - scrollRect.NormalizeScrollDistance(axis, distance2);
				tween = scrollRect.DOHorizontalNormalizedPos(Mathf.Clamp(value2, 0f, 1f), autoScrollTime).SetUpdate(isIndependentUpdate);
			}
		}
	}

	public static void ScrollToTargetInstant(this ScrollRect scrollRect, RectTransform target, RectTransform.Axis axis)
	{
		RectTransform viewport = scrollRect.viewport;
		RectTransform rectTransform = (viewport ? viewport : scrollRect.GetComponent<RectTransform>());
		Rect rect = rectTransform.rect;
		Bounds bounds = target.TransformBoundsTo(rectTransform);
		if (axis == RectTransform.Axis.Vertical)
		{
			float distance = rect.center.y - bounds.center.y;
			float value = scrollRect.verticalNormalizedPosition - scrollRect.NormalizeScrollDistance(axis, distance);
			scrollRect.verticalNormalizedPosition = Mathf.Clamp(value, 0f, 1f);
		}
		else
		{
			float distance2 = rect.center.x - bounds.center.x;
			float value2 = scrollRect.horizontalNormalizedPosition - scrollRect.NormalizeScrollDistance(axis, distance2);
			scrollRect.horizontalNormalizedPosition = Mathf.Clamp(value2, 0f, 1f);
		}
	}

	public static float NormalizeScrollDistance(this ScrollRect scrollRect, RectTransform.Axis axis, float distance)
	{
		RectTransform viewport = scrollRect.viewport;
		RectTransform rectTransform = ((viewport != null) ? viewport : scrollRect.GetComponent<RectTransform>());
		Rect rect = rectTransform.rect;
		Bounds bounds = new Bounds((Vector3)rect.center, (Vector3)rect.size);
		RectTransform content = scrollRect.content;
		float num = ((content != null) ? content.TransformBoundsTo(rectTransform) : default(Bounds)).size[(int)axis] - bounds.size[(int)axis];
		return distance / num;
	}

	public static Bounds TransformBoundsTo(this RectTransform source, Transform target)
	{
		source.GetWorldCorners(cornersCached);
		Vector3 vector = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		Vector3 vector2 = new Vector3(float.MinValue, float.MinValue, float.MinValue);
		Matrix4x4 worldToLocalMatrix = target.worldToLocalMatrix;
		for (int i = 0; i < 4; i++)
		{
			Vector3 lhs = worldToLocalMatrix.MultiplyPoint3x4(cornersCached[i]);
			vector = Vector3.Min(lhs, vector);
			vector2 = Vector3.Max(lhs, vector2);
		}
		Bounds result = new Bounds(vector, Vector3.zero);
		result.Encapsulate(vector2);
		return result;
	}

	public static Rect GetWorldRect(this RectTransform rectTransform)
	{
		Vector3[] array = new Vector3[4];
		rectTransform.GetWorldCorners(array);
		Vector2 vector = array[0];
		Vector2 size = (Vector2)array[2] - vector;
		return new Rect(vector, size);
	}

	public static bool IsRectTransformFullyInsideOtherRectTransform(this RectTransform rectTransform1, RectTransform rectTransform2)
	{
		Rect worldRect = rectTransform1.GetWorldRect();
		Rect worldRect2 = rectTransform2.GetWorldRect();
		if (worldRect.xMin <= worldRect2.xMin && worldRect.yMin <= worldRect2.yMin && worldRect.xMax >= worldRect2.xMax)
		{
			return worldRect.yMax >= worldRect2.yMax;
		}
		return false;
	}

	public static bool IsRectTransformOverlapsOtherRectTransform(this RectTransform rectTransform1, RectTransform rectTransform2)
	{
		Rect worldRect = rectTransform1.GetWorldRect();
		Rect worldRect2 = rectTransform2.GetWorldRect();
		return worldRect.Overlaps(worldRect2);
	}

	public static string FontIcon(this string iconName)
	{
		return "<sprite name=\"" + iconName + "\">";
	}

	public static string NOBR(this string str)
	{
		return "<nobr>" + str + "</nobr>";
	}
}
