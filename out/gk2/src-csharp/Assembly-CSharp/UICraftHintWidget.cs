using System;
using System.Collections.Generic;
using DG.Tweening;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class UICraftHintWidget : LazyWidget<UICraftHintWidgetData>, IBubbleLayoutAlwaysActive, IDelayedUIHide
{
	[SerializeField]
	private UIItemCell craftResultItem;

	[SerializeField]
	private RectTransform progessCellContainer;

	[SerializeField]
	private ProgressBarWiget_Simplified progressBarWidget;

	[SerializeField]
	private Slider zombieProgressBar;

	[SerializeField]
	private Vector2 defaultLayoutSize;

	[SerializeField]
	private Vector2 bigLayoutSize;

	[SerializeField]
	private LayoutElement layoutElement;

	[SerializeField]
	private RectTransform rectTransform;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private RectTransform completionProgressCellsParent;

	[SerializeField]
	private float completionCellFadeInDuration = 0.1f;

	[SerializeField]
	private float completionWidgetFadeOutDuration = 0.15f;

	[SerializeField]
	private float completionCellFlyDuration = 0.45f;

	[SerializeField]
	private float completionCellFlyDurationRandomFactor = 0.15f;

	[SerializeField]
	private float completionCellFlyStartRandomDelay = 0.05f;

	[SerializeField]
	private float completionCellTargetRandomPixels = 5f;

	[SerializeField]
	[Range(0f, 1f)]
	private float completionCellFadeOutStart = 0.7f;

	private readonly List<UICraftHintCompletionProgressCell> completionProgressCells = new List<UICraftHintCompletionProgressCell>();

	private readonly List<Action> hideAfterDelayCallbacks = new List<Action>();

	private bool subscribedToDataChanges;

	private CraftComponent subscribedCraftComponent;

	private CraftElementBase showingElement;

	private float durationTime;

	private bool isItemBig;

	private Sequence completionSequence;

	private bool isCompletionAnimationPlaying;

	private bool isCompletionHoldAdded;

	private bool isCompletionHideRequested;

	private bool isCompletionAnimationPlayedForPreFinish;

	private CraftElementBase completionAnimationPlayedElement;

	public bool ShouldDelayHide => false;

	public override void Draw(UICraftHintWidgetData data)
	{
		if (!isCompletionAnimationPlaying)
		{
			ResetCompletionState();
			if (subscribedCraftComponent != null && subscribedCraftComponent != data.CraftComponent)
			{
				UnsubscribeFromDataChanges();
				zombieProgressBar.DOKill();
			}
			base.Draw(data);
			RedrawCraftHint();
			SubscribeToDataChanges();
		}
	}

	public override void CustomUpdate()
	{
		if (data != null)
		{
			UpdateCellsFillStatus();
			TryPlayCompletionAnimation();
			UpdateQuality();
			UpdateAutoCraftTickProgress();
		}
	}

	public override void Hide()
	{
		if (isCompletionAnimationPlaying)
		{
			isCompletionHideRequested = true;
			return;
		}
		if (HasPendingCompletionAnimation() && TryPlayCompletionAnimation(refreshCellsBeforeStart: true))
		{
			isCompletionHideRequested = true;
			return;
		}
		UnsubscribeFromDataChanges();
		zombieProgressBar.DOKill();
		KillCompletionAnimation();
		HideCells();
		base.Hide();
	}

	private void OnDisable()
	{
		if (isCompletionAnimationPlaying)
		{
			ForceCancelDelayedHide();
		}
		UnsubscribeFromDataChanges();
		zombieProgressBar.DOKill();
		KillCompletionAnimation();
	}

	public void AddHideAfterDelayCallback(Action callback)
	{
		if (!isCompletionAnimationPlaying)
		{
			if (HasPendingCompletionAnimation() && TryPlayCompletionAnimation(refreshCellsBeforeStart: true))
			{
				isCompletionHideRequested = true;
				hideAfterDelayCallbacks.Add(callback);
			}
			else
			{
				callback?.Invoke();
			}
		}
		else
		{
			isCompletionHideRequested = true;
			hideAfterDelayCallbacks.Add(callback);
		}
	}

	public void ForceCancelDelayedHide()
	{
		completionSequence?.Kill();
		completionSequence = null;
		CleanupCompletionCells();
		ReleaseCompletionHold();
		isCompletionAnimationPlaying = false;
		isCompletionHideRequested = false;
		isCompletionAnimationPlayedForPreFinish = false;
		hideAfterDelayCallbacks.Clear();
		if (canvasGroup != null)
		{
			canvasGroup.alpha = 1f;
		}
	}

	private void SubscribeToDataChanges()
	{
		CraftComponent craftComponent = data.CraftComponent;
		if (!subscribedToDataChanges || subscribedCraftComponent != craftComponent)
		{
			UnsubscribeFromDataChanges();
			craftComponent.OnStatusChanged += RedrawCraftHint;
			craftComponent.OnCurCraftIndexUpdate += RedrawCraftHint;
			craftComponent.OnCraftAddedToQueue += RedrawCraftHint;
			craftComponent.OnCraftRemovedFromQueue += RedrawCraftHint;
			craftComponent.OnZombieSubTicksChanged += UpdateZombieProgressWithAnimation;
			subscribedCraftComponent = craftComponent;
			subscribedToDataChanges = true;
		}
	}

	private void UnsubscribeFromDataChanges()
	{
		if (subscribedToDataChanges && subscribedCraftComponent != null)
		{
			subscribedCraftComponent.OnStatusChanged -= RedrawCraftHint;
			subscribedCraftComponent.OnCurCraftIndexUpdate -= RedrawCraftHint;
			subscribedCraftComponent.OnCraftAddedToQueue -= RedrawCraftHint;
			subscribedCraftComponent.OnCraftRemovedFromQueue -= RedrawCraftHint;
			subscribedCraftComponent.OnZombieSubTicksChanged -= UpdateZombieProgressWithAnimation;
			subscribedCraftComponent = null;
			subscribedToDataChanges = false;
		}
	}

	private void UpdateItemSize(bool big)
	{
		if (big)
		{
			layoutElement.minWidth = bigLayoutSize.x;
			layoutElement.minHeight = bigLayoutSize.y;
			rectTransform.sizeDelta = bigLayoutSize;
		}
		else
		{
			layoutElement.minWidth = defaultLayoutSize.x;
			layoutElement.minHeight = defaultLayoutSize.y;
			rectTransform.sizeDelta = defaultLayoutSize;
		}
	}

	private void UpdateStatusIcon(CraftStatus craftStartStatus)
	{
		if (data.Worker is ZombieWgoData zombieWgoData)
		{
			if (zombieWgoData.CrafterCurrentOrder != null && (data.CraftComponent.Status == CraftComponentStatus.ReadyToFinishAutoCraft || data.CraftComponent.Status == CraftComponentStatus.WaitingForWorkerPickUp))
			{
				craftResultItem.UpdateStatusIcon(CraftStatus.NotEnoughSpaceInMultiInventory);
				return;
			}
			if (craftStartStatus == CraftStatus.DoesntHaveRequiredTool)
			{
				craftResultItem.UpdateStatusIcon(craftStartStatus, data.CraftElement.ParamsData.RequiredToolType);
				return;
			}
			if (TryUpdateZombieNotEnoughMasteryStatusIcon(zombieWgoData))
			{
				return;
			}
			if (zombieWgoData.CrafterCurrentOrder != null)
			{
				craftResultItem.UpdateStatusIcon(zombieWgoData.CrafterCurrentOrder);
				return;
			}
		}
		if (craftStartStatus == CraftStatus.DoesntHaveRequiredTool)
		{
			craftResultItem.UpdateStatusIcon(craftStartStatus, data.CraftElement.ParamsData.RequiredToolType);
		}
		else
		{
			craftResultItem.UpdateStatusIcon(craftStartStatus);
		}
	}

	private bool TryUpdateZombieNotEnoughMasteryStatusIcon(ZombieWgoData zombieWgoData)
	{
		CraftComponentStatus status = data.CraftComponent.Status;
		if (status == CraftComponentStatus.ReadyToFinishAutoCraft || status == CraftComponentStatus.WaitingForWorkerPickUp || status == CraftComponentStatus.WaitingForOutputDrop)
		{
			return false;
		}
		if (!(data.CraftComponent.CraftableObject is WgoData wgoData) || data.CraftComponent.CurrentCraftElement == null)
		{
			return false;
		}
		if (zombieWgoData.CrafterIsEnoughMastery(wgoData))
		{
			return false;
		}
		Sprite craftStatusIcon = CraftStatusIconHelper.GetCraftStatusIcon(CraftStatus.NotEnoughMastery, ItemType.None, wgoData.Definition.talent);
		if (craftStatusIcon == null)
		{
			return false;
		}
		craftResultItem.UpdateStatusIcon(craftStatusIcon);
		return true;
	}

	private void TryUpdatePlantingDigStatusIcon()
	{
		if (data.IsPlantingCraft && craftResultItem.gameObject.activeSelf && (!(craftResultItem.StatusIcon != null) || !craftResultItem.StatusIcon.gameObject.activeSelf))
		{
			Sprite plantingDigStatusIcon = CraftStatusIconHelper.GetPlantingDigStatusIcon();
			if (!(plantingDigStatusIcon == null))
			{
				craftResultItem.UpdateStatusIcon(plantingDigStatusIcon, new Vector2(0f, -1f));
			}
		}
	}

	private void HideCells()
	{
		progressBarWidget.Hide();
	}

	private bool TryPlayCompletionAnimation(bool refreshCellsBeforeStart = false)
	{
		if (isCompletionAnimationPlaying || data == null || !base.gameObject.activeInHierarchy)
		{
			return false;
		}
		CraftComponent craftComponent = data.CraftComponent;
		CraftElementBase currentCraftElement = craftComponent.CurrentCraftElement;
		if (!craftComponent.HasPreFinishUpdate)
		{
			isCompletionAnimationPlayedForPreFinish = false;
			completionAnimationPlayedElement = null;
			return false;
		}
		if (!CanPlayCompletionAnimation(currentCraftElement))
		{
			return false;
		}
		if (isCompletionAnimationPlayedForPreFinish || completionAnimationPlayedElement == currentCraftElement || !AreCellsVisible())
		{
			return false;
		}
		if (refreshCellsBeforeStart)
		{
			UpdateCellsFillStatus();
		}
		List<RectTransform> visibleSuccessCells = GetVisibleSuccessCells();
		if (visibleSuccessCells.Count == 0)
		{
			return false;
		}
		EnsureCanvasGroup();
		craftComponent.AddPreFinishHold();
		isCompletionHoldAdded = true;
		isCompletionAnimationPlaying = true;
		isCompletionAnimationPlayedForPreFinish = true;
		completionAnimationPlayedElement = currentCraftElement;
		PlayCompletionAnimation(visibleSuccessCells);
		return true;
	}

	private bool CanPlayCompletionAnimation(CraftElementBase craftElement)
	{
		return false;
	}

	private bool HasPendingCompletionAnimation()
	{
		if (data == null || !base.gameObject.activeInHierarchy)
		{
			return false;
		}
		CraftElementBase currentCraftElement = data.CraftComponent.CurrentCraftElement;
		if (data.CraftComponent.HasPreFinishUpdate && CanPlayCompletionAnimation(currentCraftElement) && !isCompletionAnimationPlayedForPreFinish && completionAnimationPlayedElement != currentCraftElement)
		{
			return AreCellsVisible();
		}
		return false;
	}

	private List<RectTransform> GetVisibleSuccessCells()
	{
		List<RectTransform> list = new List<RectTransform>();
		RectTransform greenFillRect = progressBarWidget.GreenFillRect;
		if (greenFillRect != null && greenFillRect.gameObject.activeInHierarchy)
		{
			list.Add(greenFillRect);
		}
		return list;
	}

	private void PlayCompletionAnimation(List<RectTransform> successCells)
	{
		completionSequence?.Kill();
		completionSequence = DOTween.Sequence();
		RectTransform rectTransform = ((completionProgressCellsParent != null) ? completionProgressCellsParent : (base.transform.parent as RectTransform));
		if (rectTransform == null)
		{
			rectTransform = base.transform as RectTransform;
		}
		RectTransform rect = craftResultItem.Icon.transform as RectTransform;
		canvasGroup.alpha = 1f;
		foreach (RectTransform successCell in successCells)
		{
			UICraftHintCompletionProgressCell elementFromPool = UIPrefabsPooler.Instance.GetElementFromPool<UICraftHintCompletionProgressCell>(rectTransform);
			if (!(elementFromPool == null))
			{
				RectTransform rectTransform2 = elementFromPool.RectTransform;
				rectTransform2.SetAsLastSibling();
				elementFromPool.ShowOver(successCell, rectTransform);
				completionProgressCells.Add(elementFromPool);
				Vector2 endValue = GetLocalCenter(rect, rectTransform) + UnityEngine.Random.insideUnitCircle * completionCellTargetRandomPixels;
				float num = completionCellFlyDuration * UnityEngine.Random.Range(1f - completionCellFlyDurationRandomFactor, 1f + completionCellFlyDurationRandomFactor);
				float num2 = completionCellFadeInDuration + UnityEngine.Random.Range(0f, completionCellFlyStartRandomDelay);
				completionSequence.Insert(0f, elementFromPool.CanvasGroup.DOFade(1f, completionCellFadeInDuration));
				completionSequence.Insert(num2, rectTransform2.DOAnchorPos(endValue, num).SetEase(Ease.InCubic));
				completionSequence.Insert(num2 + num * completionCellFadeOutStart, elementFromPool.CanvasGroup.DOFade(0f, num * (1f - completionCellFadeOutStart)));
			}
		}
		completionSequence.Insert(completionCellFadeInDuration, canvasGroup.DOFade(0f, completionWidgetFadeOutDuration));
		completionSequence.OnComplete(CompleteCompletionAnimation);
	}

	private Vector2 GetLocalCenter(RectTransform rect, RectTransform parent)
	{
		Vector3 position = rect.TransformPoint(rect.rect.center);
		return parent.InverseTransformPoint(position);
	}

	private void CompleteCompletionAnimation()
	{
		ReleaseCompletionHold();
		CleanupCompletionCells();
		isCompletionAnimationPlaying = false;
		if (isCompletionHideRequested)
		{
			isCompletionHideRequested = false;
			UnsubscribeFromDataChanges();
			zombieProgressBar.DOKill();
			HideCells();
			InvokeHideAfterDelayCallbacks();
			base.Hide();
		}
		else
		{
			hideAfterDelayCallbacks.Clear();
			RedrawCraftHint();
		}
	}

	private void ResetCompletionState()
	{
		if (!isCompletionAnimationPlaying)
		{
			KillCompletionAnimation();
			EnsureCanvasGroup();
			canvasGroup.alpha = 1f;
		}
	}

	private void KillCompletionAnimation()
	{
		completionSequence?.Kill();
		completionSequence = null;
		CleanupCompletionCells();
		ReleaseCompletionHold();
		isCompletionAnimationPlaying = false;
		isCompletionHideRequested = false;
		isCompletionAnimationPlayedForPreFinish = false;
		hideAfterDelayCallbacks.Clear();
	}

	private void CleanupCompletionCells()
	{
		foreach (UICraftHintCompletionProgressCell completionProgressCell in completionProgressCells)
		{
			if (completionProgressCell != null)
			{
				UIPrefabsPooler.Instance.ReleaseElementToPool(completionProgressCell);
			}
		}
		completionProgressCells.Clear();
	}

	private void ReleaseCompletionHold()
	{
		if (isCompletionHoldAdded)
		{
			isCompletionHoldAdded = false;
			data?.CraftComponent.ReleasePreFinishHold();
		}
	}

	private void InvokeHideAfterDelayCallbacks()
	{
		List<Action> list = new List<Action>(hideAfterDelayCallbacks);
		hideAfterDelayCallbacks.Clear();
		foreach (Action item in list)
		{
			item?.Invoke();
		}
	}

	private void EnsureCanvasGroup()
	{
		if (!(canvasGroup != null) && !TryGetComponent<CanvasGroup>(out canvasGroup))
		{
			canvasGroup = base.gameObject.AddComponent<CanvasGroup>();
		}
	}

	private void ReDrawCells()
	{
		if (!AreCellsVisible())
		{
			HideCells();
			return;
		}
		CraftElementBase currentCraftElement = data.CraftComponent.CurrentCraftElement;
		if (currentCraftElement != null)
		{
			progressBarWidget.Apply(currentCraftElement.TotalProgressTicks, currentCraftElement.SucceededProgressTicks, currentCraftElement.FailedProgressTicks);
			progessCellContainer.RefreshContentFitter();
			LazySingleton<UICraftHintWidgetForceUpdateCanvasesScheduler>.Instance.RequestUpdate();
		}
	}

	private void UpdateCellsFillStatus()
	{
		if (!AreCellsVisible())
		{
			HideCells();
			return;
		}
		CraftElementBase currentCraftElement = data.CraftComponent.CurrentCraftElement;
		if (currentCraftElement != null)
		{
			progressBarWidget.Apply(currentCraftElement.TotalProgressTicks, currentCraftElement.SucceededProgressTicks, currentCraftElement.FailedProgressTicks);
		}
	}

	private void UpdateQuality()
	{
		craftResultItem.UpdateQualityIcon(showingElement.GetCurrentQuality());
	}

	private void RedrawCraftHint(CraftComponentStatus craftStartStatus)
	{
		RedrawCraftHint();
	}

	private void RedrawCraftHint(CraftElementBase craftElementBase)
	{
		RedrawCraftHint();
	}

	private void RedrawCraftHint()
	{
		if ((data.CraftComponent.IsStarted && data.CraftComponent.CurrentCraftElement == null) || data.CraftComponent.CraftElementsQueue.Count == 0 || data.CraftElement.ParamsData.craftParamsType == CraftParamsData.CraftParamsType.GardenGrowing)
		{
			Hide();
			return;
		}
		isItemBig = false;
		CraftElementBase currentCraftElement = data.CraftComponent.CurrentCraftElement;
		if (currentCraftElement == null)
		{
			Hide();
			return;
		}
		OutputPreview outputPreview = null;
		bool flag = false;
		if (currentCraftElement is CraftElementSurvey craftElementSurvey)
		{
			outputPreview = craftElementSurvey.GetSelectedItemOutputPreview();
		}
		else
		{
			outputPreview = currentCraftElement.Def.GetOutputPreview(data.CraftComponent.CraftableObject as WgoData);
			flag = currentCraftElement.Def is CraftDef craftDef && craftDef.isObjDestroyCraft;
		}
		if (outputPreview == null)
		{
			Hide();
			return;
		}
		int count = outputPreview.count;
		outputPreview.count = currentCraftElement.Count * count;
		if (!flag && data.CraftComponent.CraftsIn.Count == 1 && data.CraftComponent.CraftElementsQueue.Count == 2)
		{
			outputPreview.count += data.CraftComponent.CraftElementsQueue[1].Count * count;
		}
		if (currentCraftElement.Def.id.StartsWith("pocket_extract_item") && currentCraftElement.CustomItems != null && currentCraftElement.CustomItems.Count > 0)
		{
			outputPreview.itemId = currentCraftElement.CustomItems[0].id;
			outputPreview.count = currentCraftElement.CustomItems[0].Count;
			outputPreview.customIconId = string.Empty;
		}
		showingElement = data.CraftComponent.CurrentCraftElement;
		craftResultItem.gameObject.SetActive(value: true);
		int customQuality = (data.IsPlantingCraft ? (-1) : currentCraftElement.GetCurrentQuality());
		craftResultItem.DrawCraftOutput(outputPreview, customQuality, currentCraftElement.CraftStatus, currentCraftElement.ParamsData.RequiredToolType);
		isItemBig = outputPreview.IsBigItemOutput;
		UpdateItemSize(isItemBig);
		if (ShouldShowZombieProgressBar())
		{
			zombieProgressBar.transform.parent.gameObject.SetActive(value: true);
			if (data.CraftComponent.IsAutoCraftable)
			{
				zombieProgressBar.DOKill();
				zombieProgressBar.value = data.CraftComponent.AutoCraftTickProgressNormalized;
			}
			else
			{
				UpdateZombieProgress(data.CraftComponent.ZombieSubTicks, instant: true);
			}
		}
		else
		{
			zombieProgressBar.DOKill();
			zombieProgressBar.transform.parent.gameObject.SetActive(value: false);
		}
		if ((currentCraftElement.Def.id == "sawmill_wood_zombie_craft" || currentCraftElement.Def.id == "mine_zombie_craft" || currentCraftElement.Def.id == "sand_zombie_craft" || currentCraftElement.Def.id == "clay_zombie_craft" || currentCraftElement.Def.id == "carrier_stone_zombie_craft" || currentCraftElement.Def.id == "carrier_marble_zombie_craft") && data.Worker is ZombieWgoData zombieWgoData)
		{
			if (!zombieWgoData.HasToolForWork(zombieWgoData.AttachedWgoData, currentCraftElement.Def))
			{
				craftResultItem.gameObject.SetActive(value: true);
				craftResultItem.UpdateStatusIcon(CraftStatus.DoesntHaveRequiredTool, data.CraftElement.ParamsData.RequiredToolType);
			}
			else if (TryUpdateZombieNotEnoughMasteryStatusIcon(zombieWgoData))
			{
				craftResultItem.gameObject.SetActive(value: true);
			}
			else if (data.CraftComponent.Status == CraftComponentStatus.WaitingForWorkerPickUp)
			{
				craftResultItem.gameObject.SetActive(value: true);
				craftResultItem.UpdateStatusIcon(LazySingletonSO<EasySpritesCollection>.Instance.GetSprite("craft_status_wait"));
			}
		}
		else
		{
			UpdateStatusIcon(currentCraftElement.CraftStatus);
		}
		TryUpdatePlantingDigStatusIcon();
		UpdateQuality();
		ReDrawCells();
		craftResultItem.SetNativeSizeForIcon();
		UpdateItemSize(isItemBig);
	}

	private bool AreCellsVisible()
	{
		CraftElementBase currentCraftElement = data.CraftComponent.CurrentCraftElement;
		if (currentCraftElement != null && currentCraftElement.IsStarted && data.CraftComponent.Status != CraftComponentStatus.ReadyToFinishAutoCraft && data.CraftComponent.Status != CraftComponentStatus.WaitingForWorkerPickUp)
		{
			return data.CraftComponent.Status != CraftComponentStatus.WaitingForOutputDrop;
		}
		return false;
	}

	private bool ShouldShowZombieProgressBar()
	{
		if (data.CraftElement == null || !data.CraftElement.IsStarted)
		{
			return false;
		}
		CraftComponentStatus status = data.CraftComponent.Status;
		if (status == CraftComponentStatus.ReadyToFinishAutoCraft || status == CraftComponentStatus.WaitingForWorkerPickUp || status == CraftComponentStatus.WaitingForOutputDrop)
		{
			return false;
		}
		if (!data.CraftComponent.IsAutoCraftable && !(data.Worker is ZombieWgoData))
		{
			return data.CraftComponent.ZombieSubTicks > 0;
		}
		return true;
	}

	private void UpdateAutoCraftTickProgress()
	{
		if (data.CraftComponent.IsAutoCraftable && zombieProgressBar.transform.parent.gameObject.activeSelf)
		{
			zombieProgressBar.value = data.CraftComponent.AutoCraftTickProgressNormalized;
		}
	}

	private void UpdateZombieProgressWithAnimation(int progress)
	{
		if (!data.CraftComponent.IsAutoCraftable)
		{
			float num = (float)progress / (float)ConstDef.Get("zombie_craft_sub_ticks_count").IntValue;
			if (num > zombieProgressBar.value)
			{
				zombieProgressBar.DOKill();
				zombieProgressBar.DOValue(num, 0.25f);
			}
			else
			{
				zombieProgressBar.DOKill();
				zombieProgressBar.value = num;
			}
		}
	}

	private void UpdateZombieProgress(int progress, bool instant)
	{
		if (instant)
		{
			zombieProgressBar.DOKill();
			float value = (float)progress / (float)ConstDef.Get("zombie_craft_sub_ticks_count").IntValue;
			zombieProgressBar.value = value;
		}
		else
		{
			UpdateZombieProgressWithAnimation(progress);
		}
	}

	protected override void TestDraw()
	{
	}
}
