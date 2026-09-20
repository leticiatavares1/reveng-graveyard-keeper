using System;
using DG.Tweening;
using LazyBearTechnology;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class UICinematic : MonoBehaviour, ILazyGUIElement
{
	[SerializeField]
	private RectTransform cinematicUp;

	[SerializeField]
	private RectTransform cinematicDown;

	private Canvas canvas;

	[Space]
	[SerializeField]
	[Range(0f, 10f)]
	public float duration = 1f;

	[SerializeField]
	private Ease enableEaseType = Ease.Linear;

	[SerializeField]
	private Ease disableEaseType = Ease.Linear;

	[SerializeField]
	[Range(0f, 0.5f)]
	private float heightRatio = 0.1f;

	private const float LowResolutionHeightMultiplier = 0.7f;

	private bool isAnimatingEnabling;

	private bool isAnimatingDisabling;

	private Action onCompleted;

	public void Init()
	{
		base.gameObject.SetActive(value: false);
		canvas = GetComponent<Canvas>();
		canvas.overrideSorting = true;
		canvas.sortingOrder = 300;
		GameSettings.OnResolutionChanged += OnResolutionChanged;
	}

	public void EnableCinematic(Action onCompleted = null, bool instant = false)
	{
		if (!base.gameObject.activeSelf || isAnimatingDisabling)
		{
			CalculateHeight();
			if (!instant)
			{
				if (!isAnimatingEnabling)
				{
					if (isAnimatingDisabling)
					{
						this.onCompleted?.Invoke();
						this.onCompleted = null;
						isAnimatingDisabling = false;
						cinematicUp.DOKill();
						cinematicDown.DOKill();
					}
					this.onCompleted = onCompleted;
					LazyUI.Get<HUD>().SetDisableState(HudStateType.Cinematic, isEnabled: false);
					base.gameObject.SetActive(value: true);
					isAnimatingEnabling = true;
					float y = cinematicUp.sizeDelta.y;
					cinematicUp.DOAnchorPosY(0f - y, duration).SetEase(enableEaseType);
					cinematicDown.DOAnchorPosY(y, duration).SetEase(enableEaseType).OnComplete(delegate
					{
						isAnimatingEnabling = false;
						onCompleted?.Invoke();
					});
				}
				else
				{
					onCompleted?.Invoke();
				}
			}
			else
			{
				if (isAnimatingDisabling)
				{
					this.onCompleted?.Invoke();
					this.onCompleted = null;
					isAnimatingDisabling = false;
					cinematicUp.DOKill();
					cinematicDown.DOKill();
				}
				LazyUI.Get<HUD>().SetDisableState(HudStateType.Cinematic, isEnabled: false);
				base.gameObject.SetActive(value: true);
				float y2 = cinematicUp.sizeDelta.y;
				cinematicUp.anchoredPosition = new Vector2(0f, 0f - y2);
				cinematicDown.anchoredPosition = new Vector2(0f, y2);
				onCompleted?.Invoke();
			}
		}
		else
		{
			Debug.LogWarning("Trying to enable already active cinematics");
			onCompleted?.Invoke();
		}
	}

	public void DisableCinematic(Action onCompleted = null, bool instant = false)
	{
		if (base.gameObject.activeSelf)
		{
			if (!instant)
			{
				if (!isAnimatingDisabling)
				{
					if (isAnimatingEnabling)
					{
						this.onCompleted?.Invoke();
						this.onCompleted = null;
						isAnimatingEnabling = false;
						cinematicUp.DOKill();
						cinematicDown.DOKill();
					}
					this.onCompleted = onCompleted;
					isAnimatingDisabling = true;
					cinematicUp.DOAnchorPosY(0f, duration).SetEase(disableEaseType);
					cinematicDown.DOAnchorPosY(0f, duration).SetEase(disableEaseType).OnComplete(delegate
					{
						isAnimatingDisabling = false;
						onCompleted?.Invoke();
						LazyUI.Get<HUD>().SetDisableState(HudStateType.Cinematic, isEnabled: true);
						base.gameObject.SetActive(value: false);
					});
				}
				else
				{
					onCompleted?.Invoke();
				}
			}
			else
			{
				if (isAnimatingEnabling)
				{
					this.onCompleted?.Invoke();
					this.onCompleted = null;
					isAnimatingEnabling = false;
					cinematicUp.DOKill();
					cinematicDown.DOKill();
				}
				isAnimatingDisabling = false;
				cinematicUp.anchoredPosition = Vector2.zero;
				cinematicDown.anchoredPosition = Vector2.zero;
				LazyUI.Get<HUD>().SetDisableState(HudStateType.Cinematic, isEnabled: true);
				base.gameObject.SetActive(value: false);
				onCompleted?.Invoke();
			}
		}
		else
		{
			Debug.LogWarning("Trying to disable already non-active cinematics");
			onCompleted?.Invoke();
		}
	}

	private void OnResolutionChanged(IntVector2 _)
	{
		CalculateHeight();
	}

	private void CalculateHeight()
	{
		float num = GUIElements.Instance.Root.sizeDelta.y * heightRatio;
		if (GameSettings.Instance.GetResolutionIntVector2().y <= 720)
		{
			num *= 0.7f;
		}
		cinematicUp.sizeDelta = new Vector2(0f, num);
		cinematicDown.sizeDelta = new Vector2(0f, num);
	}

	private void OnDestroy()
	{
		GameSettings.OnResolutionChanged -= OnResolutionChanged;
	}
}
