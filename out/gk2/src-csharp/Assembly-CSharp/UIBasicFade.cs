using System;
using DG.Tweening;
using LazyBearTechnology;
using UnityEngine;

public abstract class UIBasicFade : MonoBehaviour, ILazyGUIElement
{
	protected const float FADE_DURATION = 0.3f;

	[SerializeField]
	protected CanvasGroup blackoutCanvas;

	private Tween currentTweener;

	private bool locked;

	private MultiFlagAND<FadeFlag> fadeFlags = new MultiFlagAND<FadeFlag>();

	public bool IsFadeShowing => base.gameObject.activeInHierarchy;

	public virtual void Init()
	{
		fadeFlags.Init(null, initialFlag: true);
	}

	public void Fade(float fadeTime = 0.3f, Action onInCompleted = null, Action onOutCompleted = null)
	{
		FadeIn(fadeTime, delegate
		{
			onInCompleted?.Invoke();
			FadeOut(fadeTime, onOutCompleted);
		});
	}

	public void FadeIn(Action onComplete, FadeFlag fadeFlag = FadeFlag.Common, bool blockInterceptions = false)
	{
		FadeIn(0.3f, onComplete, fadeFlag, blockInterceptions);
	}

	public void FadeIn(float fadeTime = 0.3f, Action onComplete = null, FadeFlag fadeFlag = FadeFlag.Common, bool blockInterceptions = false)
	{
		if (locked)
		{
			Debug.LogWarning("Fade in is already in progress");
			return;
		}
		if (fadeFlag == FadeFlag.All)
		{
			foreach (FadeFlag value in Enum.GetValues(typeof(FadeFlag)))
			{
				if (value != FadeFlag.All)
				{
					fadeFlags.UpdateFlag(value, newValue: false);
				}
			}
		}
		else
		{
			fadeFlags.UpdateFlag(fadeFlag, newValue: false);
		}
		if (base.gameObject.activeInHierarchy)
		{
			onComplete?.Invoke();
			return;
		}
		SetBlackState();
		EnsureTweenStopped();
		locked = blockInterceptions;
		currentTweener = blackoutCanvas.DOFade(1f, fadeTime).OnComplete(delegate
		{
			onComplete?.Invoke();
			locked = false;
		});
	}

	public void FadeInInstant(FadeFlag fadeFlag = FadeFlag.Common)
	{
		if (fadeFlag == FadeFlag.All)
		{
			foreach (FadeFlag value in Enum.GetValues(typeof(FadeFlag)))
			{
				if (value != FadeFlag.All)
				{
					fadeFlags.UpdateFlag(value, newValue: false);
				}
			}
		}
		else
		{
			fadeFlags.UpdateFlag(fadeFlag, newValue: false);
		}
		SetBlackState();
		blackoutCanvas.alpha = 1f;
	}

	public void FadeOutInstant(FadeFlag fadeFlag = FadeFlag.Common)
	{
		if (fadeFlag == FadeFlag.All)
		{
			foreach (FadeFlag value in Enum.GetValues(typeof(FadeFlag)))
			{
				if (value != FadeFlag.All)
				{
					fadeFlags.UpdateFlag(value, newValue: true);
				}
			}
		}
		else
		{
			fadeFlags.UpdateFlag(fadeFlag, newValue: true);
		}
		if (fadeFlags.ResultFlag)
		{
			Disable();
			blackoutCanvas.alpha = 1f;
		}
	}

	public void FadeOut(Action onComplete, FadeFlag fadeFlag = FadeFlag.Common)
	{
		FadeOut(0.3f, onComplete, fadeFlag);
	}

	public void FadeOut(float fadeTime = 0.3f, Action onComplete = null, FadeFlag fadeFlag = FadeFlag.Common)
	{
		if (locked)
		{
			Debug.LogWarning("Fade out is already in progress");
			return;
		}
		if (fadeFlag == FadeFlag.All)
		{
			foreach (FadeFlag value in Enum.GetValues(typeof(FadeFlag)))
			{
				if (value != FadeFlag.All)
				{
					fadeFlags.UpdateFlag(value, newValue: true);
				}
			}
		}
		else
		{
			fadeFlags.UpdateFlag(fadeFlag, newValue: true);
		}
		if (!fadeFlags.ResultFlag)
		{
			onComplete?.Invoke();
			return;
		}
		EnsureTweenStopped();
		currentTweener = blackoutCanvas.DOFade(0f, fadeTime).OnComplete(delegate
		{
			Disable();
			onComplete?.Invoke();
		});
	}

	private void Disable()
	{
		base.gameObject.SetActive(value: false);
	}

	public void SetBlackState()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			blackoutCanvas.alpha = 0f;
			base.gameObject.SetActive(value: true);
		}
	}

	private void EnsureTweenStopped()
	{
		if (currentTweener != null && currentTweener.IsActive())
		{
			currentTweener.Kill();
		}
	}
}
