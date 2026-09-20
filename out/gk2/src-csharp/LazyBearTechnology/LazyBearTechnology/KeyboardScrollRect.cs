using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace LazyBearTechnology;

[DisallowMultipleComponent]
[RequireComponent(typeof(ScrollRect))]
public class KeyboardScrollRect : MonoBehaviour
{
	[SerializeField]
	[Min(0f)]
	[Tooltip("Scroll speed in pixels per second.")]
	private float scrollSpeed = 500f;

	[SerializeField]
	[Tooltip("Use unscaled time so scrolling works while the game is paused.")]
	private bool useUnscaledTime;

	private ScrollRect scrollRect;

	private LazyWidgetBase ownerWindow;

	private bool canProcessInput;

	private void Awake()
	{
		scrollRect = GetComponent<ScrollRect>();
		CacheOwnerWindow();
		LazyWindowsStackController.OnWindowBecameVisibleInStack += OnWindowBecameVisibleInStack;
		LazyWindowsStackController.OnWindowBecameHiddenInStack += OnWindowBecameHiddenInStack;
		SyncInputProcessing();
	}

	private void OnEnable()
	{
		if (scrollRect == null)
		{
			scrollRect = GetComponent<ScrollRect>();
		}
		CacheOwnerWindow();
		SyncInputProcessing();
	}

	private void OnDisable()
	{
		canProcessInput = false;
	}

	private void Update()
	{
		try
		{
			if (!CanScrollThisFrame())
			{
				return;
			}
			Vector2 input = ReadScrollInput();
			if (!LazyInput.IsGamepadActive && !(input.sqrMagnitude < 0.0001f))
			{
				if (input.sqrMagnitude > 1f)
				{
					input.Normalize();
				}
				float num = (useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
				float num2 = scrollSpeed * num;
				if (!(num <= 0f) && float.IsFinite(num2) && !(num2 <= 0f) && !LazyInput.IsGamepadActive)
				{
					scrollRect.StopMovement();
					scrollRect.DOKill();
					ApplyScroll(input, num2);
				}
			}
		}
		catch (Exception)
		{
		}
	}

	private void OnDestroy()
	{
		LazyWindowsStackController.OnWindowBecameVisibleInStack -= OnWindowBecameVisibleInStack;
		LazyWindowsStackController.OnWindowBecameHiddenInStack -= OnWindowBecameHiddenInStack;
	}

	private bool CanScrollThisFrame()
	{
		if (!canProcessInput || !base.isActiveAndEnabled)
		{
			return false;
		}
		if (!LazyInput.IsInitialized || LazyInput.IsGamepadActive)
		{
			return false;
		}
		if (ownerWindow == null || ownerWindow != LazyWindowsStackController.ActiveWindow)
		{
			return false;
		}
		if (scrollRect == null || !scrollRect.IsActive() || scrollRect.content == null)
		{
			return false;
		}
		if (!float.IsFinite(scrollSpeed) || scrollSpeed <= 0f)
		{
			return false;
		}
		return true;
	}

	private Vector2 ReadScrollInput()
	{
		Vector2 zero = Vector2.zero;
		if (scrollRect.horizontal)
		{
			if (IsKeyboardKeyHeld(GameKey.Left))
			{
				zero.x -= 1f;
			}
			if (IsKeyboardKeyHeld(GameKey.Right))
			{
				zero.x += 1f;
			}
		}
		if (scrollRect.vertical)
		{
			if (IsKeyboardKeyHeld(GameKey.Up))
			{
				zero.y += 1f;
			}
			if (IsKeyboardKeyHeld(GameKey.Down))
			{
				zero.y -= 1f;
			}
		}
		return zero;
	}

	private static bool IsKeyboardKeyHeld(GameKey key)
	{
		if ((object)key == null || !LazyInput.IsInitialized || LazyInput.IsGamepadActive)
		{
			return false;
		}
		return LazyInput.GetKey(key);
	}

	private void ApplyScroll(Vector2 input, float pixelDelta)
	{
		RectTransform content = scrollRect.content;
		if (content == null)
		{
			return;
		}
		RectTransform rectTransform = scrollRect.viewport;
		if (rectTransform == null)
		{
			rectTransform = scrollRect.transform as RectTransform;
		}
		if (rectTransform == null)
		{
			return;
		}
		Rect rect = rectTransform.rect;
		Rect rect2 = content.rect;
		if (scrollRect.horizontal && Mathf.Abs(input.x) > 0f)
		{
			float num = rect2.width - rect.width;
			if (num > 0.01f)
			{
				float num2 = scrollRect.horizontalNormalizedPosition + input.x * pixelDelta / num;
				if (float.IsFinite(num2))
				{
					scrollRect.horizontalNormalizedPosition = num2;
				}
			}
		}
		if (scrollRect.vertical && Mathf.Abs(input.y) > 0f)
		{
			float num3 = rect2.height - rect.height;
			if (num3 > 0.01f)
			{
				float num4 = scrollRect.verticalNormalizedPosition + input.y * pixelDelta / num3;
				if (float.IsFinite(num4))
				{
					scrollRect.verticalNormalizedPosition = num4;
				}
			}
		}
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

	private void OnWindowBecameVisibleInStack(LazyWidgetBase window)
	{
		if (ownerWindow != null && window == ownerWindow)
		{
			canProcessInput = true;
		}
	}

	private void OnWindowBecameHiddenInStack(LazyWidgetBase window)
	{
		if (ownerWindow != null && window == ownerWindow)
		{
			canProcessInput = false;
		}
	}

	private void CacheOwnerWindow()
	{
		ownerWindow = null;
		Transform parent = base.transform;
		while (parent != null)
		{
			if (parent.TryGetComponent<LazyWidgetBase>(out var component) && parent.TryGetComponent<Canvas>(out var _))
			{
				ownerWindow = component;
				break;
			}
			parent = parent.parent;
		}
	}

	private void SyncInputProcessing()
	{
		canProcessInput = ownerWindow != null && ownerWindow == LazyWindowsStackController.ActiveWindow;
	}
}
