using System;
using System.Collections.Generic;
using DG.Tweening;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TalentExpProgressWidget : LazyWidget<TalentExpProgressWidgetData>
{
	[Serializable]
	private class TalentViewData
	{
		public Sprite barSprite;

		public string talentId;

		public string pointIconId;
	}

	[SerializeField]
	private Image barElementPrefab;

	[SerializeField]
	private Color activeColor = new Color(1f, 1f, 1f, 1f);

	[SerializeField]
	private Color inactiveColor = new Color(1f, 1f, 1f, 0.5f);

	[SerializeField]
	private TextMeshProUGUI talentExpPointsLabel;

	[SerializeField]
	private TextStyleComponent expPointStyleComponent;

	[SerializeField]
	private TextStyle hasPointsStyle;

	[SerializeField]
	private TextStyle noPointsStyle;

	[SerializeField]
	private List<TalentViewData> viewDatas = new List<TalentViewData>();

	[SerializeField]
	private float pointsLabelDelayAfterBarReset = 0.5f;

	[SerializeField]
	private float barFillFromCenterDuration = 0.16f;

	private TalentViewData currentViewData;

	private List<TalentExpProgressWidgetBar> shownBarElements = new List<TalentExpProgressWidgetBar>();

	private bool isAnimatingFill;

	private bool isResettingFullBar;

	private int displayedFilledCount;

	private int displayedTotal;

	private int displayedPoints;

	private int queuedFillsDuringReset;

	private int pendingFillAnimations;

	private TalentExpProgressWidgetData animationTargetData;

	private Tween barResetTween;

	public string PointIconId => currentViewData?.pointIconId;

	public bool HasPendingBarReset
	{
		get
		{
			if (!isResettingFullBar)
			{
				return pendingFillAnimations > 0;
			}
			return true;
		}
	}

	public override void Init()
	{
		base.Init();
		barElementPrefab.gameObject.SetActive(value: false);
	}

	public override void Hide()
	{
		ResetFillAnimationState();
		base.Hide();
	}

	public override void Redraw()
	{
		base.Redraw();
		ResetFillAnimationState();
		currentViewData = viewDatas.Find((TalentViewData d) => d.talentId == data.TalentId);
		RebuildBarVisuals(data.CurExp, data.TotalExp);
		SetPointsLabel(data.TalentExpPoints);
	}

	public bool TryGetFlyTarget(out Vector3 worldPosition)
	{
		return TryGetFlyTarget(0, out worldPosition);
	}

	public bool TryGetFlyTarget(int upcomingOffset, out Vector3 worldPosition)
	{
		RebuildBarsLayout();
		int num = (isAnimatingFill ? displayedFilledCount : ((data != null) ? data.CurExp : 0));
		int num2 = (isAnimatingFill ? displayedTotal : ((data != null) ? data.TotalExp : shownBarElements.Count));
		int num3 = num + upcomingOffset;
		if (num2 > 0 && num3 >= num2)
		{
			return TryGetRectCenter(GetBarsParent(), out worldPosition);
		}
		if (num3 >= 0 && num3 < shownBarElements.Count)
		{
			return TryGetRectCenter(shownBarElements[num3].transform as RectTransform, out worldPosition);
		}
		return TryGetRectCenter(GetBarsParent(), out worldPosition);
	}

	private static bool TryGetRectCenter(RectTransform rect, out Vector3 worldPosition)
	{
		worldPosition = default(Vector3);
		if (rect == null)
		{
			return false;
		}
		worldPosition = rect.TransformPoint(rect.rect.center);
		return true;
	}

	private RectTransform GetBarsParent()
	{
		if (!(barElementPrefab != null))
		{
			return null;
		}
		return barElementPrefab.rectTransform.parent as RectTransform;
	}

	private void RebuildBarsLayout()
	{
		RectTransform barsParent = GetBarsParent();
		if (barsParent != null)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(barsParent);
		}
	}

	public void PrepareFillAnimation()
	{
		isAnimatingFill = true;
		displayedFilledCount = data.CurExp;
		displayedTotal = data.TotalExp;
		displayedPoints = data.TalentExpPoints;
	}

	public void SetFillAnimationTarget(TalentExpProgressWidgetData targetData)
	{
		animationTargetData = targetData;
	}

	public void ApplyArrivedExpPoint(Action onApplied = null)
	{
		if (!isAnimatingFill || displayedTotal <= 0)
		{
			onApplied?.Invoke();
			return;
		}
		if (isResettingFullBar || (displayedFilledCount >= displayedTotal && pendingFillAnimations > 0))
		{
			queuedFillsDuringReset++;
			onApplied?.Invoke();
			return;
		}
		if (displayedFilledCount >= displayedTotal)
		{
			if (animationTargetData == null)
			{
				onApplied?.Invoke();
				return;
			}
			displayedFilledCount = 0;
			displayedTotal = animationTargetData.TotalExp;
			RebuildBarVisuals(displayedFilledCount, displayedTotal);
		}
		int num = displayedFilledCount;
		displayedFilledCount++;
		if (num >= 0 && num < shownBarElements.Count)
		{
			pendingFillAnimations++;
			shownBarElements[num].PlayFillFromCenter(activeColor, barFillFromCenterDuration, delegate
			{
				NotifyFillAnimationFinished();
				if (isAnimatingFill)
				{
					AfterFillAppeared();
				}
			}, NotifyFillAnimationFinished);
		}
		else
		{
			AfterFillAppeared();
		}
		void AfterFillAppeared()
		{
			if (pendingFillAnimations > 0)
			{
				onApplied?.Invoke();
			}
			else if (displayedFilledCount < displayedTotal || animationTargetData == null)
			{
				onApplied?.Invoke();
			}
			else if (displayedPoints < animationTargetData.TalentExpPoints)
			{
				displayedPoints++;
				PlayFullBarResetThenUpdatePoints(onApplied);
			}
			else
			{
				ApplyQueuedFills(onApplied);
			}
		}
	}

	private void NotifyFillAnimationFinished()
	{
		pendingFillAnimations = Mathf.Max(0, pendingFillAnimations - 1);
	}

	public void ResetFillAnimationState()
	{
		KillBarResetTween();
		isAnimatingFill = false;
		isResettingFullBar = false;
		queuedFillsDuringReset = 0;
		pendingFillAnimations = 0;
		animationTargetData = null;
		for (int i = 0; i < shownBarElements.Count; i++)
		{
			shownBarElements[i]?.StopFillAnimation();
		}
	}

	private void PlayFullBarResetThenUpdatePoints(Action onComplete)
	{
		isResettingFullBar = true;
		int pointsToShow = displayedPoints;
		KillBarResetTween();
		barResetTween = DOVirtual.DelayedCall(pointsLabelDelayAfterBarReset, delegate
		{
			barResetTween = null;
			if (isAnimatingFill)
			{
				displayedFilledCount = 0;
				displayedTotal = ((animationTargetData != null) ? animationTargetData.TotalExp : displayedTotal);
				RebuildBarVisuals(displayedFilledCount, displayedTotal);
				SetPointsLabel(pointsToShow);
				isResettingFullBar = false;
				ApplyQueuedFills(onComplete);
			}
		}).SetLink(base.gameObject, LinkBehaviour.KillOnDisable);
	}

	private void ApplyQueuedFills(Action onComplete)
	{
		int num = queuedFillsDuringReset;
		queuedFillsDuringReset = 0;
		if (num == 0)
		{
			onComplete?.Invoke();
			return;
		}
		for (int i = 0; i < num; i++)
		{
			ApplyArrivedExpPoint(onComplete);
		}
	}

	private void KillBarResetTween()
	{
		Tween tween = barResetTween;
		barResetTween = null;
		if (tween != null && tween.IsActive())
		{
			tween.Kill();
		}
	}

	private void RebuildBarVisuals(int filledCount, int totalCount)
	{
		foreach (TalentExpProgressWidgetBar shownBarElement in shownBarElements)
		{
			shownBarElement?.StopFillAnimation();
			UIPrefabsPooler.Instance.ReleaseElementToPool(shownBarElement);
		}
		shownBarElements.Clear();
		for (int i = 0; i < filledCount; i++)
		{
			shownBarElements.Add(CreateBarElement(activeColor));
		}
		for (int j = 0; j < totalCount - filledCount; j++)
		{
			shownBarElements.Add(CreateBarElement(inactiveColor));
		}
		for (int k = 0; k < shownBarElements.Count; k++)
		{
			shownBarElements[k].transform.SetSiblingIndex(k);
		}
		RebuildBarsLayout();
	}

	private TalentExpProgressWidgetBar CreateBarElement(Color color)
	{
		TalentExpProgressWidgetBar elementFromPool = UIPrefabsPooler.Instance.GetElementFromPool<TalentExpProgressWidgetBar>(barElementPrefab.transform.parent);
		elementFromPool.ResetVisual(color);
		elementFromPool.image.sprite = currentViewData.barSprite;
		elementFromPool.gameObject.SetActive(value: true);
		return elementFromPool;
	}

	private void SetPointsLabel(int points)
	{
		talentExpPointsLabel.text = $"{currentViewData.pointIconId.FontIcon()}{points}";
		if (points > 0)
		{
			expPointStyleComponent.SetTextStyle(hasPointsStyle);
		}
		else
		{
			expPointStyleComponent.SetTextStyle(noPointsStyle);
		}
	}

	protected override void TestDraw()
	{
	}
}
