using System;
using DG.Tweening;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class TalentExpProgressWidgetBar : MonoBehaviour
{
	public Image image;

	private Image fillOverlay;

	private Tween fillTween;

	public void PlayFillFromCenter(Color filledColor, float duration, Action onComplete = null, Action onInterrupted = null)
	{
		KillFillTween();
		base.transform.localScale = Vector3.one;
		Image image = EnsureFillOverlay();
		SyncOverlayWithImage(image);
		image.color = filledColor;
		image.rectTransform.localScale = new Vector3(0f, 1f, 1f);
		image.gameObject.SetActive(value: true);
		if (duration <= 0f)
		{
			FinishFill(filledColor);
			onComplete?.Invoke();
			return;
		}
		LazyAudio.PlayAndForget("blimp_grow");
		bool finished = false;
		fillTween = image.rectTransform.DOScaleX(1f, duration).SetEase(Ease.OutCubic).SetLink(base.gameObject, LinkBehaviour.KillOnDisable)
			.OnComplete(delegate
			{
				Finish(completed: true);
			})
			.OnKill(delegate
			{
				Finish(completed: false);
			});
		void Finish(bool completed)
		{
			if (!finished)
			{
				finished = true;
				fillTween = null;
				if (completed)
				{
					FinishFill(filledColor);
					onComplete?.Invoke();
				}
				else
				{
					onInterrupted?.Invoke();
				}
			}
		}
	}

	public void ResetVisual(Color color)
	{
		StopFillAnimation();
		if (image != null)
		{
			image.color = color;
		}
	}

	public void StopFillAnimation()
	{
		KillFillTween();
		base.transform.localScale = Vector3.one;
		HideFillOverlay();
	}

	private void FinishFill(Color filledColor)
	{
		if (image != null)
		{
			image.color = filledColor;
		}
		HideFillOverlay();
	}

	private Image EnsureFillOverlay()
	{
		if (fillOverlay != null)
		{
			return fillOverlay;
		}
		GameObject gameObject = new GameObject("FillOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
		RectTransform component = gameObject.GetComponent<RectTransform>();
		component.SetParent(base.transform, worldPositionStays: false);
		component.anchorMin = Vector2.zero;
		component.anchorMax = Vector2.one;
		component.offsetMin = Vector2.zero;
		component.offsetMax = Vector2.zero;
		component.pivot = new Vector2(0.5f, 0.5f);
		component.localScale = Vector3.one;
		fillOverlay = gameObject.GetComponent<Image>();
		fillOverlay.raycastTarget = false;
		gameObject.SetActive(value: false);
		return fillOverlay;
	}

	private void SyncOverlayWithImage(Image overlay)
	{
		if (!(image == null))
		{
			overlay.sprite = image.sprite;
			overlay.type = image.type;
			overlay.preserveAspect = image.preserveAspect;
			overlay.pixelsPerUnitMultiplier = image.pixelsPerUnitMultiplier;
			overlay.material = image.material;
		}
	}

	private void HideFillOverlay()
	{
		if (!(fillOverlay == null))
		{
			fillOverlay.rectTransform.localScale = Vector3.one;
			fillOverlay.gameObject.SetActive(value: false);
		}
	}

	private void OnDisable()
	{
		KillFillTween();
		HideFillOverlay();
		base.transform.localScale = Vector3.one;
	}

	private void KillFillTween()
	{
		Tween tween = fillTween;
		fillTween = null;
		if (tween != null && tween.IsActive())
		{
			tween.Kill();
		}
	}
}
