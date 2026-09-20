using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LazyBearTechnology;

[DisallowMultipleComponent]
[RequireComponent(typeof(ScrollRect))]
[DefaultExecutionOrder(-50)]
public class SmoothMouseWheelScroll : MonoBehaviour, IScrollHandler, IEventSystemHandler, IBeginDragHandler
{
	private const float WheelDeltaTimeCap = 0.05f;

	private const float WheelFinishEpsilon = 0.05f;

	private const float WheelMinFinishSpeed = 220f;

	private const float ExternalMoveSqrThreshold = 0.25f;

	[SerializeField]
	[Min(0.04f)]
	private float wheelSmoothTime = 0.1f;

	private ScrollRect scrollRect;

	private float cachedScrollSensitivity = 1f;

	private Vector2 remainingWheelOffset;

	private Vector2 lastAppliedContentPosition;

	private bool receivedWheelThisFrame;

	private bool hasCachedSensitivity;

	public static SmoothMouseWheelScroll Ensure(ScrollRect target)
	{
		if (target == null)
		{
			return null;
		}
		if (!target.TryGetComponent<SmoothMouseWheelScroll>(out var component))
		{
			return target.gameObject.AddComponent<SmoothMouseWheelScroll>();
		}
		return component;
	}

	private void Awake()
	{
		scrollRect = GetComponent<ScrollRect>();
	}

	private void OnEnable()
	{
		if (scrollRect == null)
		{
			scrollRect = GetComponent<ScrollRect>();
		}
		CacheAndDisableNativeSensitivity();
		ClearWheelSmoothing();
		CacheContentPosition();
	}

	private void OnDisable()
	{
		ClearWheelSmoothing();
		RestoreNativeSensitivity();
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			ClearWheelSmoothing();
		}
	}

	public void OnScroll(PointerEventData eventData)
	{
		if (!base.isActiveAndEnabled || scrollRect == null || !scrollRect.IsActive())
		{
			return;
		}
		Vector2 scrollDelta = eventData.scrollDelta;
		scrollDelta.y *= -1f;
		if (scrollRect.vertical && !scrollRect.horizontal)
		{
			if (Mathf.Abs(scrollDelta.x) > Mathf.Abs(scrollDelta.y))
			{
				scrollDelta.y = scrollDelta.x;
			}
			scrollDelta.x = 0f;
		}
		if (scrollRect.horizontal && !scrollRect.vertical)
		{
			if (Mathf.Abs(scrollDelta.y) > Mathf.Abs(scrollDelta.x))
			{
				scrollDelta.x = scrollDelta.y;
			}
			scrollDelta.y = 0f;
		}
		remainingWheelOffset += scrollDelta * cachedScrollSensitivity;
		receivedWheelThisFrame = true;
		scrollRect.DOKill();
	}

	private void LateUpdate()
	{
		if (scrollRect == null || scrollRect.content == null)
		{
			remainingWheelOffset = Vector2.zero;
			receivedWheelThisFrame = false;
			return;
		}
		RectTransform content = scrollRect.content;
		if (!receivedWheelThisFrame && (content.anchoredPosition - lastAppliedContentPosition).sqrMagnitude > 0.25f)
		{
			remainingWheelOffset = Vector2.zero;
		}
		if (remainingWheelOffset.sqrMagnitude > 0.0025000002f)
		{
			float num = Mathf.Min(Time.unscaledDeltaTime, 0.05f);
			if (num > 0f)
			{
				Vector2 vector = CalculateWheelStep(remainingWheelOffset, num);
				Vector2 anchoredPosition = content.anchoredPosition;
				content.anchoredPosition = anchoredPosition + vector;
				ClampToBounds();
				Vector2 vector2 = content.anchoredPosition - anchoredPosition;
				remainingWheelOffset -= vector;
				if (Mathf.Abs(vector.x) > 0.05f && Mathf.Abs(vector2.x) <= 0.05f)
				{
					remainingWheelOffset.x = 0f;
				}
				if (Mathf.Abs(vector.y) > 0.05f && Mathf.Abs(vector2.y) <= 0.05f)
				{
					remainingWheelOffset.y = 0f;
				}
			}
		}
		else
		{
			remainingWheelOffset = Vector2.zero;
		}
		CacheContentPosition();
		receivedWheelThisFrame = false;
	}

	private Vector2 CalculateWheelStep(Vector2 remaining, float deltaTime)
	{
		float magnitude = remaining.magnitude;
		float num = 220f * deltaTime;
		if (magnitude <= num)
		{
			return remaining;
		}
		float num2 = Mathf.Max(0.04f, wheelSmoothTime);
		float num3 = 1f - Mathf.Exp((0f - deltaTime) / num2);
		float num4 = Mathf.Max(magnitude * num3, num);
		return remaining * (num4 / magnitude);
	}

	private void ClampToBounds()
	{
		if (scrollRect.movementType == ScrollRect.MovementType.Clamped)
		{
			if (scrollRect.horizontal)
			{
				scrollRect.horizontalNormalizedPosition = Mathf.Clamp01(scrollRect.horizontalNormalizedPosition);
			}
			if (scrollRect.vertical)
			{
				scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition);
			}
		}
	}

	private void CacheAndDisableNativeSensitivity()
	{
		if (!(scrollRect == null))
		{
			if (!hasCachedSensitivity)
			{
				cachedScrollSensitivity = scrollRect.scrollSensitivity;
				hasCachedSensitivity = true;
			}
			scrollRect.scrollSensitivity = 0f;
		}
	}

	private void RestoreNativeSensitivity()
	{
		if (scrollRect != null && hasCachedSensitivity)
		{
			scrollRect.scrollSensitivity = cachedScrollSensitivity;
		}
	}

	private void ClearWheelSmoothing()
	{
		remainingWheelOffset = Vector2.zero;
		receivedWheelThisFrame = false;
	}

	private void CacheContentPosition()
	{
		if (scrollRect != null && scrollRect.content != null)
		{
			lastAppliedContentPosition = scrollRect.content.anchoredPosition;
		}
	}
}
