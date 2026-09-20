using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

public class UISaveOverlay : MonoBehaviour, ILazyGUIElement
{
	public CanvasGroup canvasGroup;

	public float fadeDuration = 0.25f;

	public float showTime = 1.5f;

	private float openedTime;

	private bool isTimerActive;

	private float timerStartTime;

	private float timer;

	private Action onTimerComplete;

	private bool isShown;

	public void Init()
	{
		SaveSystem.OnSaveWriteStarted += EnableOverlay;
		SaveSystem.OnSaveWriteStartedInstant += EnableOverlayInstant;
		SaveSystem.OnSaveWriteEnded += DisableOverlay;
	}

	protected void Update()
	{
		if (isTimerActive && Time.unscaledTime - timerStartTime >= timer)
		{
			isTimerActive = false;
			onTimerComplete?.Invoke();
		}
	}

	private void EnableOverlay()
	{
		if (!isShown)
		{
			base.gameObject.SetActive(value: true);
			isShown = true;
			canvasGroup.alpha = 0f;
			canvasGroup.DOFade(1f, fadeDuration).SetUpdate(isIndependentUpdate: true);
			openedTime = Time.unscaledTime;
		}
	}

	private void EnableOverlayInstant()
	{
		if (!isShown)
		{
			base.gameObject.SetActive(value: true);
			isShown = true;
			canvasGroup.DOKill();
			canvasGroup.alpha = 1f;
			openedTime = Time.unscaledTime;
		}
	}

	private void DisableOverlay()
	{
		if (!isShown)
		{
			return;
		}
		float num = Time.unscaledTime - openedTime;
		timer = Mathf.Clamp(showTime, 0f, showTime - num);
		isTimerActive = true;
		timerStartTime = Time.unscaledTime;
		onTimerComplete = delegate
		{
			canvasGroup.alpha = 1f;
			TweenerCore<float, float, FloatOptions> tweenerCore = canvasGroup.DOFade(0f, fadeDuration);
			tweenerCore.onComplete = delegate
			{
				base.gameObject.SetActive(value: false);
				isShown = false;
			};
			((Tween)tweenerCore).SetUpdate(isIndependentUpdate: true);
		};
	}
}
