using System;
using UnityEngine;

namespace LazyBearTechnology;

public class GamepadDynamicSelector : MonoBehaviour
{
	[SerializeField]
	private RectTransform selectorRectTransform;

	[SerializeField]
	private float selectTime;

	[SerializeField]
	private float minWidth;

	[SerializeField]
	private float minHeight;

	[SerializeField]
	private bool useUnscaledTime;

	[Space]
	private GamepadNavigationItem currentItem;

	private GamepadNavigationItem previousItem;

	private Func<bool> onUpdateCheckActivity;

	private bool lerp;

	private float elapsedTime;

	private Vector2 startSizeDelta;

	private Vector3 startPosition;

	private Vector2 endSizeDelta;

	public void Init()
	{
		GamepadNavigationItem.OnUnfocusStatic += OnItemUnfocused;
		GamepadNavigationItem.OnFocusStatic += OnItemFocused;
	}

	public void SetOnUpdateCheckActivity(Func<bool> onUpdateCheckActivity)
	{
		this.onUpdateCheckActivity = onUpdateCheckActivity;
	}

	private void OnItemFocused(GamepadNavigationItem gamepadNavigationItem)
	{
		if (gamepadNavigationItem.Controller.IsEnabled && gamepadNavigationItem.Active)
		{
			currentItem = gamepadNavigationItem;
			CalculateEndSizeDelta();
			startSizeDelta = selectorRectTransform.sizeDelta;
			startPosition = selectorRectTransform.position;
			if (previousItem == null || previousItem.Controller != gamepadNavigationItem.Controller)
			{
				selectorRectTransform.position = gamepadNavigationItem.FocusRectTransform.position;
				selectorRectTransform.sizeDelta = endSizeDelta;
				selectorRectTransform.pivot = currentItem.FocusRectTransform.pivot;
			}
			else
			{
				elapsedTime = selectTime;
				lerp = true;
			}
		}
	}

	private void OnItemUnfocused(GamepadNavigationItem gamepadNavigationItem)
	{
		previousItem = gamepadNavigationItem;
	}

	private void Update()
	{
		if (LazyInput.IsGamepadActive && currentItem != null && currentItem.Controller.IsEnabled && currentItem.Active && currentItem.gameObject.activeInHierarchy && onUpdateCheckActivity != null && onUpdateCheckActivity())
		{
			if (!selectorRectTransform.gameObject.activeSelf || !lerp)
			{
				CalculateEndSizeDelta();
				selectorRectTransform.position = currentItem.FocusRectTransform.position;
				selectorRectTransform.sizeDelta = endSizeDelta;
				selectorRectTransform.pivot = currentItem.FocusRectTransform.pivot;
				selectorRectTransform.gameObject.SetActive(value: true);
			}
			else if (lerp)
			{
				elapsedTime -= (useUnscaledTime ? LazyTime.GetUnscaledDeltaTime : Time.deltaTime);
				elapsedTime = Mathf.Clamp(elapsedTime, 0f, 1f);
				if (elapsedTime > 0f)
				{
					CalculateEndSizeDelta();
					float t = 1f - elapsedTime / selectTime;
					selectorRectTransform.position = Vector3.Lerp(startPosition, currentItem.FocusRectTransform.position, t);
					selectorRectTransform.sizeDelta = Vector2.Lerp(startSizeDelta, endSizeDelta, t);
					lerp = true;
				}
				else
				{
					lerp = false;
				}
			}
		}
		else
		{
			selectorRectTransform.gameObject.SetActive(value: false);
			currentItem = null;
			previousItem = null;
			lerp = false;
		}
	}

	private void CalculateEndSizeDelta()
	{
		Rect rect = currentItem.FocusRectTransform.rect;
		endSizeDelta.x = ((rect.width >= minWidth) ? rect.width : minWidth);
		endSizeDelta.y = ((rect.height >= minHeight) ? rect.height : minHeight);
	}
}
