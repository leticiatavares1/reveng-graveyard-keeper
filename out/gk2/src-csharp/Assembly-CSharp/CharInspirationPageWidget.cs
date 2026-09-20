using System.Collections.Generic;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class CharInspirationPageWidget : LazyWidget<CharInspirationPageWidgetData>
{
	[Space]
	[SerializeField]
	private Canvas canvas;

	[Space]
	[SerializeField]
	private TalentTabButtonsContainer talentTabButtonsContainer;

	[SerializeField]
	private TalentExpProgressWidget talentExpProgressWidget;

	[SerializeField]
	private UIItemCell faithCell;

	[SerializeField]
	private TextMeshProUGUI nextSubTabGamepadHelper;

	[SerializeField]
	private TextMeshProUGUI prevSubTabGamepadHelper;

	[Space]
	[SerializeField]
	private InspirationWidget inspirationWidgetPrefab;

	[SerializeField]
	private Transform inspirationWidgetsParent;

	[SerializeField]
	private Transform inspirationWidgetsFinishedParent;

	[SerializeField]
	private GameObject finishedSeparator;

	private readonly List<InspirationWidget> displayedInspirations = new List<InspirationWidget>();

	private readonly List<InspirationWidgetFinished> displayedInspirationsFinished = new List<InspirationWidgetFinished>();

	[SerializeField]
	private ScrollRect scrollRect;

	[SerializeField]
	private AutoScroll autoScroll;

	[SerializeField]
	private TalentLevelUpsWidget talentLevelUpsWidget;

	private TalentLevelUpsWidgetData talentLevelUpsWidgetData;

	[SerializeField]
	private FlyingInspirationExpPoint flyingExpPointPrefab;

	[SerializeField]
	private float flyingExpPointDuration = 2f / 3f;

	[SerializeField]
	private GameObject canBuyTalentLevelUpEffect;

	private bool isShown;

	private Pool flyingExpPool;

	private Transform flyingExpOverlay;

	private bool isPlayingExpFlyAnimation;

	private bool isHandlingInspirationPurchase;

	private bool pendingInspirationsRedraw;

	private bool pendingGamepadFocusAfterPurchase;

	private string pendingPurchasedInspirationId;

	private int pendingPurchasedInspirationIndex = -1;

	private string pendingFinishedInspirationId;

	private int pendingFinishedInspirationLevel = -1;

	private int pendingFinishedInspirationIndex = -1;

	private GamepadNavigationItem pendingGamepadFocusNavigationItem;

	private GamepadNavigationItem pendingGamepadFocusSourceNavigationItem;

	private GamepadNavigationController pendingGamepadFocusNavigationController;

	private bool pendingGamepadFocusMovedAfterPurchase;

	private int pendingGamepadFocusAnchorIndex = -1;

	private bool isRestoringGamepadFocusAfterPurchase;

	private int remainingFlyingExpCount;

	private readonly List<FlyingInspirationExpPoint> flyingExpPoints = new List<FlyingInspirationExpPoint>();

	public override void Init()
	{
		base.Init();
		talentTabButtonsContainer.Init(delegate(string talendId)
		{
			LazyUI.GetWindow<CharacterWindow>()?.SetInspirationPageWithSpecificTalent(talendId);
		}, canvas);
		inspirationWidgetPrefab.gameObject.SetActive(value: false);
		EnsureFlyingExpOverlay();
		if (flyingExpPointPrefab != null)
		{
			flyingExpPointPrefab.gameObject.SetActive(value: false);
			flyingExpPool = new Pool(flyingExpPointPrefab, flyingExpOverlay, 1);
		}
	}

	protected override void SetData(CharInspirationPageWidgetData data)
	{
		base.SetData(data);
		data.onTalentExpChanged = null;
		data.onInspirationProgressChanged = OnInspirationProgressChanged;
		data.onTalentLevelPurchased = OnTalentLevelPurchased;
		data.onInspirationPurchased = OnInspirationPurchased;
	}

	public override void Redraw()
	{
		base.Redraw();
		pendingInspirationsRedraw = false;
		CancelFlyingExpAnimation();
		isShown = true;
		RedrawFaith();
		TalentExpProgressWidgetData talentExpProgressWidgetData = new TalentExpProgressWidgetData(data.TalentData);
		talentExpProgressWidget.Draw(talentExpProgressWidgetData);
		DrawInspirations(data.TalentData);
		RedrawTalentLevelUpsWidget();
		UpdateCanBuyTalentLevelUpEffect();
		RedrawTabs();
		UpdateGamepadDependentStuff();
	}

	public override void Hide()
	{
		if (isShown)
		{
			isShown = false;
			pendingInspirationsRedraw = false;
			ClearPendingGamepadFocusAfterPurchase();
			CancelFlyingExpAnimation();
			talentExpProgressWidget.Hide();
			HideInspirations();
			talentLevelUpsWidget.Hide();
			UpdateCanBuyTalentLevelUpEffect();
			if (data != null)
			{
				data.UnsubscribeEvents();
				data.onTalentExpChanged = null;
				data.onInspirationProgressChanged = null;
				data.onTalentLevelPurchased = null;
				data.onInspirationPurchased = null;
			}
			base.Hide();
		}
	}

	private void OnEnable()
	{
		if (isShown)
		{
			RedrawInspirationsIfPending();
		}
	}

	private void OnDisable()
	{
		CancelFlyingExpAnimation();
	}

	public void RedrawFaith()
	{
		faithCell.Draw(new Item("faith", MainGame.PlayerController.PlayerData.Inventory.Data.GetTotalCountInInventory("faith")), isNeedItem: false, -1, isCraftResult: false, 1, drawAsNonInteractable: false, 0, drawCounter: true, forceNonEmpty: true, forceDrawCounter: true);
	}

	public void UpdateGamepadDependentStuff()
	{
		bool isGamepadActive = LazyInput.IsGamepadActive;
		if (nextSubTabGamepadHelper != null)
		{
			nextSubTabGamepadHelper.gameObject.SetActive(isGamepadActive);
			if (isGamepadActive)
			{
				nextSubTabGamepadHelper.text = ControllerIconLibrary.GetIconId(GameKey.NextSubTab);
			}
			nextSubTabGamepadHelper.transform.SetAsLastSibling();
		}
		if (prevSubTabGamepadHelper != null)
		{
			prevSubTabGamepadHelper.gameObject.SetActive(isGamepadActive);
			if (isGamepadActive)
			{
				prevSubTabGamepadHelper.text = ControllerIconLibrary.GetIconId(GameKey.PrevSubTab);
			}
			prevSubTabGamepadHelper.transform.SetAsLastSibling();
		}
	}

	public bool OnPressedPrevTechTab(GamepadNavigationController gamepadNavigationController)
	{
		return talentTabButtonsContainer.OnPressedPrevTechTab(gamepadNavigationController, autoScroll);
	}

	public bool OnPressedNextTechTab(GamepadNavigationController gamepadNavigationController)
	{
		return talentTabButtonsContainer.OnPressedNextTechTab(gamepadNavigationController, autoScroll);
	}

	private void DrawInspirations(TalentData talentData)
	{
		HideInspirations();
		for (int i = 0; i < talentData.activeInspirations.Count; i++)
		{
			if (!talentData.activeInspirations[i].isAllLevelsBought && !talentData.activeInspirations[i].IsHidden)
			{
				InspirationWidget elementFromPool = UIPrefabsPooler.Instance.GetElementFromPool<InspirationWidget>(inspirationWidgetsParent);
				elementFromPool.Init();
				InspirationWidgetData inspirationWidgetData = new InspirationWidgetData(talentData.activeInspirations[i], talentExpProgressWidget.PointIconId);
				elementFromPool.Draw(inspirationWidgetData);
				displayedInspirations.Add(elementFromPool);
			}
		}
		displayedInspirations.Sort(delegate(InspirationWidget x, InspirationWidget y)
		{
			bool isCompleted = x.InspirationData.IsCompleted;
			bool isCompleted2 = y.InspirationData.IsCompleted;
			if (isCompleted != isCompleted2)
			{
				if (!isCompleted)
				{
					return 1;
				}
				return -1;
			}
			int completionPrice = InspirationDef.GetDataForLevel(x.InspirationData.id, x.InspirationData.curLevel).completionPrice;
			int completionPrice2 = InspirationDef.GetDataForLevel(y.InspirationData.id, y.InspirationData.curLevel).completionPrice;
			return completionPrice.CompareTo(completionPrice2);
		});
		for (int j = 0; j < talentData.activeInspirations.Count; j++)
		{
			for (int k = 0; k < talentData.activeInspirations[j].PurchasedInspirations.Count; k++)
			{
				InspirationWidgetFinished elementFromPool2 = UIPrefabsPooler.Instance.GetElementFromPool<InspirationWidgetFinished>(inspirationWidgetsFinishedParent);
				elementFromPool2.Init();
				InspirationWidgetData inspirationWidgetData2 = new InspirationWidgetData(talentData.activeInspirations[j].PurchasedInspirations[k]);
				elementFromPool2.Draw(inspirationWidgetData2);
				displayedInspirationsFinished.Add(elementFromPool2);
			}
		}
		for (int l = 0; l < displayedInspirations.Count; l++)
		{
			displayedInspirations[l].transform.SetSiblingIndex(l);
		}
		for (int m = 0; m < displayedInspirationsFinished.Count; m++)
		{
			displayedInspirationsFinished[m].transform.SetSiblingIndex(m);
		}
		if (displayedInspirationsFinished.Count > 0 && displayedInspirations.Count > 0)
		{
			finishedSeparator.SetActive(value: true);
		}
		else
		{
			finishedSeparator.SetActive(value: false);
		}
	}

	private void HideInspirations()
	{
		foreach (InspirationWidget displayedInspiration in displayedInspirations)
		{
			UIPrefabsPooler.Instance.ReleaseElementToPool(displayedInspiration);
		}
		displayedInspirations.Clear();
		foreach (InspirationWidgetFinished item in displayedInspirationsFinished)
		{
			UIPrefabsPooler.Instance.ReleaseElementToPool(item);
		}
		displayedInspirationsFinished.Clear();
	}

	private void OnInspirationProgressChanged(string inspirationId)
	{
		InspirationData inspirationData = data.TalentData.activeInspirations.Find((InspirationData x) => x.id == inspirationId);
		if (inspirationData == null)
		{
			return;
		}
		foreach (InspirationWidget displayedInspiration in displayedInspirations)
		{
			if (inspirationId == displayedInspiration.IdWithoutLevel && displayedInspiration.Level == inspirationData.curLevel)
			{
				displayedInspiration.Redraw();
			}
		}
	}

	private void OnTalentLevelPurchased(string talentId, string levelId)
	{
		if (!(data.TalentData.id != talentId))
		{
			talentLevelUpsWidgetData.onTalentLevelPurchased?.Invoke(talentId, levelId);
			RedrawTalentExpProgressWidget();
			UpdateCanBuyTalentLevelUpEffect();
			RedrawTabs();
			((RectTransform)base.transform).RefreshContentFitter();
		}
	}

	private void RedrawTalentExpProgressWidget()
	{
		if (!isPlayingExpFlyAnimation)
		{
			TalentExpProgressWidgetData talentExpProgressWidgetData = new TalentExpProgressWidgetData(data.TalentData);
			talentExpProgressWidget.Draw(talentExpProgressWidgetData);
		}
	}

	private void RedrawTabs(bool blockCurrentTalentLevelUpActionIndicator = true)
	{
		talentTabButtonsContainer.Draw(data.TalentData.id, blockCurrentTalentLevelUpActionIndicator);
		LazyUI.GetWindow<CharacterWindow>().RefreshInspirationPageActionIndicatorStatus();
	}

	private void RedrawTalentLevelUpsWidget()
	{
		talentLevelUpsWidgetData = new TalentLevelUpsWidgetData(data.TalentData);
		talentLevelUpsWidget.Draw(talentLevelUpsWidgetData);
	}

	private void UpdateCanBuyTalentLevelUpEffect()
	{
		if (!(canBuyTalentLevelUpEffect == null))
		{
			bool active = isShown && data != null && data.TalentData != null && data.TalentData.HasAvailableTalentLevelUpToPurchase();
			canBuyTalentLevelUpEffect.SetActive(active);
		}
	}

	private void OnInspirationPurchased(string id)
	{
		if (!isShown || isHandlingInspirationPurchase)
		{
			return;
		}
		isHandlingInspirationPurchase = true;
		try
		{
			InspirationWidget inspirationWidget = displayedInspirations.Find((InspirationWidget widget) => widget.IdWithoutLevel == id);
			RegisterGamepadFocusAfterPurchase(id, inspirationWidget);
			int num = ((inspirationWidget != null) ? inspirationWidget.DisplayedCompletionExp : 0);
			Vector3 startPos = ((inspirationWidget != null && inspirationWidget.BuyExpRect != null) ? ((Vector3)inspirationWidget.BuyExpRect.GetWorldRect().center) : Vector3.zero);
			string text = ((inspirationWidget != null) ? inspirationWidget.PointIconId : null);
			TextMeshProUGUI textMeshProUGUI = ((inspirationWidget != null) ? inspirationWidget.BuyExpLabel : null);
			inspirationWidget?.HideFlyingReward();
			RedrawTalentLevelUpsWidget();
			UpdateCanBuyTalentLevelUpEffect();
			RedrawTabs(blockCurrentTalentLevelUpActionIndicator: false);
			RedrawFaith();
			Vector3 worldPosition = default(Vector3);
			if (num <= 0 || string.IsNullOrEmpty(text) || !(textMeshProUGUI != null) || !talentExpProgressWidget.TryGetFlyTarget(out worldPosition))
			{
				DrawInspirations(data.TalentData);
				((RectTransform)base.transform).RefreshContentFitter();
				RestoreGamepadFocusAfterPurchaseIfPending();
				RedrawTalentExpProgressWidget();
				TryShowInspirationTalentsTutorial();
			}
			else
			{
				pendingInspirationsRedraw = true;
				PlayFlyingExpAnimation(startPos, worldPosition, num, text.FontIcon(), textMeshProUGUI);
			}
		}
		finally
		{
			isHandlingInspirationPurchase = false;
		}
	}

	private void RegisterGamepadFocusAfterPurchase(string id, InspirationWidget sourceWidget)
	{
		if (LazyInput.IsGamepadActive)
		{
			pendingGamepadFocusAfterPurchase = true;
			pendingPurchasedInspirationId = id;
			pendingPurchasedInspirationIndex = ((sourceWidget != null) ? displayedInspirations.IndexOf(sourceWidget) : (-1));
			pendingGamepadFocusAnchorIndex = pendingPurchasedInspirationIndex;
			pendingFinishedInspirationId = null;
			pendingFinishedInspirationLevel = -1;
			pendingFinishedInspirationIndex = -1;
			pendingGamepadFocusSourceNavigationItem = ((sourceWidget != null) ? sourceWidget.GetComponent<GamepadNavigationItem>() : null);
			pendingGamepadFocusNavigationItem = pendingGamepadFocusSourceNavigationItem;
			pendingGamepadFocusMovedAfterPurchase = false;
			SubscribePendingGamepadFocusChanges();
		}
	}

	private void UpdatePendingGamepadFocusFromCurrentFocus()
	{
		if (pendingGamepadFocusAfterPurchase && LazyInput.IsGamepadActive)
		{
			CharacterWindow window = LazyUI.GetWindow<CharacterWindow>();
			GamepadNavigationController gamepadNavigationController = ((window != null) ? window.NavigationController : null);
			UpdatePendingGamepadFocusFromItem((gamepadNavigationController != null) ? gamepadNavigationController.FocusedItem : null);
		}
	}

	private void OnPendingGamepadFocusedItemChanged(GamepadNavigationItem focusedItem)
	{
		if (!isRestoringGamepadFocusAfterPurchase && pendingGamepadFocusAfterPurchase)
		{
			UpdatePendingGamepadFocusFromItem(focusedItem);
		}
	}

	private void UpdatePendingGamepadFocusFromItem(GamepadNavigationItem focusedItem)
	{
		if (!(focusedItem == null) && focusedItem.transform.IsChildOf(base.transform) && !(focusedItem == pendingGamepadFocusSourceNavigationItem))
		{
			pendingGamepadFocusMovedAfterPurchase = true;
			InspirationWidgetFinished component2;
			if (focusedItem.TryGetComponent<InspirationWidget>(out var component) && displayedInspirations.Contains(component))
			{
				pendingPurchasedInspirationId = component.IdWithoutLevel;
				pendingPurchasedInspirationIndex = displayedInspirations.IndexOf(component);
				pendingFinishedInspirationId = null;
				pendingFinishedInspirationLevel = -1;
				pendingFinishedInspirationIndex = -1;
			}
			else if (focusedItem.TryGetComponent<InspirationWidgetFinished>(out component2) && displayedInspirationsFinished.Contains(component2))
			{
				pendingPurchasedInspirationId = null;
				pendingPurchasedInspirationIndex = -1;
				pendingFinishedInspirationId = component2.IdWithoutLevel;
				pendingFinishedInspirationLevel = component2.Level;
				pendingFinishedInspirationIndex = displayedInspirationsFinished.IndexOf(component2);
			}
			else
			{
				pendingPurchasedInspirationId = null;
				pendingPurchasedInspirationIndex = -1;
				pendingFinishedInspirationId = null;
				pendingFinishedInspirationLevel = -1;
				pendingFinishedInspirationIndex = -1;
			}
			pendingGamepadFocusNavigationItem = focusedItem;
		}
	}

	private void SubscribePendingGamepadFocusChanges()
	{
		UnsubscribePendingGamepadFocusChanges();
		CharacterWindow window = LazyUI.GetWindow<CharacterWindow>();
		pendingGamepadFocusNavigationController = ((window != null) ? window.NavigationController : null);
		if (pendingGamepadFocusNavigationController != null)
		{
			pendingGamepadFocusNavigationController.OnFocusedItemChanged += OnPendingGamepadFocusedItemChanged;
		}
	}

	private void UnsubscribePendingGamepadFocusChanges()
	{
		if (!(pendingGamepadFocusNavigationController == null))
		{
			pendingGamepadFocusNavigationController.OnFocusedItemChanged -= OnPendingGamepadFocusedItemChanged;
			pendingGamepadFocusNavigationController = null;
		}
	}

	private void RestoreGamepadFocusAfterPurchaseIfPending()
	{
		if (!pendingGamepadFocusAfterPurchase)
		{
			return;
		}
		CharacterWindow window = LazyUI.GetWindow<CharacterWindow>();
		GamepadNavigationController gamepadNavigationController = ((window != null) ? window.NavigationController : null);
		if (!LazyInput.IsGamepadActive || gamepadNavigationController == null)
		{
			ClearPendingGamepadFocusAfterPurchase();
			return;
		}
		GamepadNavigationItem gamepadNavigationItem = (pendingGamepadFocusMovedAfterPurchase ? FindMovedGamepadFocusAfterPurchase() : FindAutomaticGamepadFocusAfterPurchase());
		if (gamepadNavigationItem != null)
		{
			isRestoringGamepadFocusAfterPurchase = true;
			try
			{
				if (autoScroll != null)
				{
					autoScroll.SkipNextAutoscroll = true;
				}
				gamepadNavigationController.ReinitItems(focusOnFirstActive: false);
				gamepadNavigationController.SetFocusedItem(gamepadNavigationItem);
				if (autoScroll != null)
				{
					autoScroll.ScrollToItem(gamepadNavigationItem);
					autoScroll.SkipNextAutoscroll = false;
				}
			}
			finally
			{
				isRestoringGamepadFocusAfterPurchase = false;
			}
		}
		else
		{
			gamepadNavigationController.ReinitItems(focusOnFirstActive: true);
		}
		ClearPendingGamepadFocusAfterPurchase();
	}

	private GamepadNavigationItem FindMovedGamepadFocusAfterPurchase()
	{
		InspirationWidget inspirationWidget = (string.IsNullOrEmpty(pendingPurchasedInspirationId) ? null : displayedInspirations.Find((InspirationWidget widget) => widget.IdWithoutLevel == pendingPurchasedInspirationId));
		GamepadNavigationItem component = null;
		if (inspirationWidget != null)
		{
			inspirationWidget.TryGetComponent<GamepadNavigationItem>(out component);
		}
		if (component == null)
		{
			InspirationWidgetFinished inspirationWidgetFinished = FindPendingFinishedInspirationWidget();
			if (inspirationWidgetFinished != null)
			{
				inspirationWidgetFinished.TryGetComponent<GamepadNavigationItem>(out component);
			}
		}
		if (component == null && IsValidGamepadFocusNavigationItem(pendingGamepadFocusNavigationItem))
		{
			component = pendingGamepadFocusNavigationItem;
		}
		if (component == null)
		{
			component = FindAnyGamepadFocusAfterPurchase();
		}
		return component;
	}

	private GamepadNavigationItem FindAutomaticGamepadFocusAfterPurchase()
	{
		GamepadNavigationItem displayedInspirationNavigationItem = GetDisplayedInspirationNavigationItem(pendingGamepadFocusAnchorIndex, requireBuyable: false);
		if (displayedInspirationNavigationItem != null)
		{
			return displayedInspirationNavigationItem;
		}
		displayedInspirationNavigationItem = FindNearestDisplayedInspirationNavigationItem(pendingGamepadFocusAnchorIndex, 1, requireBuyable: true);
		if (displayedInspirationNavigationItem != null)
		{
			return displayedInspirationNavigationItem;
		}
		displayedInspirationNavigationItem = FindNearestDisplayedInspirationNavigationItem(pendingGamepadFocusAnchorIndex, -1, requireBuyable: true);
		if (displayedInspirationNavigationItem != null)
		{
			return displayedInspirationNavigationItem;
		}
		displayedInspirationNavigationItem = FindNearestDisplayedInspirationNavigationItem(pendingGamepadFocusAnchorIndex, 1, requireBuyable: false);
		if (displayedInspirationNavigationItem != null)
		{
			return displayedInspirationNavigationItem;
		}
		displayedInspirationNavigationItem = FindNearestDisplayedInspirationNavigationItem(pendingGamepadFocusAnchorIndex, -1, requireBuyable: false);
		if (!(displayedInspirationNavigationItem != null))
		{
			return FindAnyGamepadFocusAfterPurchase();
		}
		return displayedInspirationNavigationItem;
	}

	private InspirationWidgetFinished FindPendingFinishedInspirationWidget()
	{
		if (string.IsNullOrEmpty(pendingFinishedInspirationId))
		{
			return null;
		}
		InspirationWidgetFinished inspirationWidgetFinished = displayedInspirationsFinished.Find((InspirationWidgetFinished widget) => widget.IdWithoutLevel == pendingFinishedInspirationId && widget.Level == pendingFinishedInspirationLevel);
		if (inspirationWidgetFinished != null)
		{
			return inspirationWidgetFinished;
		}
		if (pendingFinishedInspirationIndex >= 0 && pendingFinishedInspirationIndex < displayedInspirationsFinished.Count)
		{
			return displayedInspirationsFinished[pendingFinishedInspirationIndex];
		}
		return null;
	}

	private GamepadNavigationItem FindAnyGamepadFocusAfterPurchase()
	{
		foreach (InspirationWidget displayedInspiration in displayedInspirations)
		{
			if (displayedInspiration != null && displayedInspiration.TryGetComponent<GamepadNavigationItem>(out var component) && IsValidGamepadFocusNavigationItem(component))
			{
				return component;
			}
		}
		foreach (InspirationWidgetFinished item in displayedInspirationsFinished)
		{
			if (item != null && item.TryGetComponent<GamepadNavigationItem>(out var component2) && IsValidGamepadFocusNavigationItem(component2))
			{
				return component2;
			}
		}
		return null;
	}

	private GamepadNavigationItem FindNearestDisplayedInspirationNavigationItem(int startIndex, int step, bool requireBuyable)
	{
		if (displayedInspirations.Count == 0)
		{
			return null;
		}
		int i = startIndex + step;
		if (startIndex < 0)
		{
			i = ((step <= 0) ? (displayedInspirations.Count - 1) : 0);
		}
		for (; i >= 0 && i < displayedInspirations.Count; i += step)
		{
			GamepadNavigationItem displayedInspirationNavigationItem = GetDisplayedInspirationNavigationItem(i, requireBuyable);
			if (displayedInspirationNavigationItem != null)
			{
				return displayedInspirationNavigationItem;
			}
		}
		return null;
	}

	private GamepadNavigationItem GetDisplayedInspirationNavigationItem(int index, bool requireBuyable)
	{
		if (index < 0 || index >= displayedInspirations.Count)
		{
			return null;
		}
		InspirationWidget inspirationWidget = displayedInspirations[index];
		if (inspirationWidget == null)
		{
			return null;
		}
		if (requireBuyable && (inspirationWidget.IsBuyLocked || inspirationWidget.InspirationData == null || !inspirationWidget.InspirationData.IsAvailableToBuy))
		{
			return null;
		}
		if (!inspirationWidget.TryGetComponent<GamepadNavigationItem>(out var component) || !IsValidGamepadFocusNavigationItem(component))
		{
			return null;
		}
		return component;
	}

	private bool IsValidGamepadFocusNavigationItem(GamepadNavigationItem navigationItem)
	{
		if (navigationItem != null && navigationItem.isActiveAndEnabled && navigationItem.Active && navigationItem.gameObject.activeInHierarchy)
		{
			return navigationItem.transform.IsChildOf(base.transform);
		}
		return false;
	}

	private void ClearPendingGamepadFocusAfterPurchase()
	{
		pendingGamepadFocusAfterPurchase = false;
		pendingPurchasedInspirationId = null;
		pendingPurchasedInspirationIndex = -1;
		pendingFinishedInspirationId = null;
		pendingFinishedInspirationLevel = -1;
		pendingFinishedInspirationIndex = -1;
		pendingGamepadFocusNavigationItem = null;
		pendingGamepadFocusSourceNavigationItem = null;
		isRestoringGamepadFocusAfterPurchase = false;
		pendingGamepadFocusMovedAfterPurchase = false;
		pendingGamepadFocusAnchorIndex = -1;
		UnsubscribePendingGamepadFocusChanges();
	}

	private void TryShowInspirationTalentsTutorial()
	{
		if (isShown && !MainGame.PlayerData.sawInspirationTalentsTutorialOnce && data != null && data.TalentData != null && data.TalentData.talentExpPoints > 0)
		{
			MainGame.PlayerData.sawInspirationTalentsTutorialOnce = true;
			LazyUI.GetWindow<UITutorialWindow>().Open(new UITutorialWindowData("tut_insp_talents_hdr_new"));
		}
	}

	public InspirationWidget FindDisplayedInspiration(string id)
	{
		if (string.IsNullOrEmpty(id))
		{
			return null;
		}
		return displayedInspirations.Find((InspirationWidget widget) => widget != null && widget.IdWithoutLevel == id);
	}

	public bool CompleteFlyingExpAnimationImmediately()
	{
		if (!isPlayingExpFlyAnimation)
		{
			return false;
		}
		for (int i = 0; i < flyingExpPoints.Count; i++)
		{
			if (flyingExpPoints[i] != null)
			{
				flyingExpPoints[i].StopAndRelease();
			}
		}
		flyingExpPoints.Clear();
		remainingFlyingExpCount = 0;
		FinishFlyingExpAnimation();
		return true;
	}

	private void PlayFlyingExpAnimation(Vector3 startPos, Vector3 targetPos, int completionExp, string icon, TextMeshProUGUI sourceLabel)
	{
		EnsureFlyingExpPool(sourceLabel);
		if (flyingExpPool == null)
		{
			RedrawTalentExpProgressWidget();
			TryShowInspirationTalentsTutorial();
			return;
		}
		int num = remainingFlyingExpCount;
		if (!isPlayingExpFlyAnimation)
		{
			isPlayingExpFlyAnimation = true;
			talentExpProgressWidget.PrepareFillAnimation();
		}
		talentExpProgressWidget.SetFillAnimationTarget(new TalentExpProgressWidgetData(data.TalentData));
		remainingFlyingExpCount += completionExp;
		for (int i = 0; i < completionExp; i++)
		{
			Vector3 worldPosition = targetPos;
			talentExpProgressWidget.TryGetFlyTarget(num + i, out worldPosition);
			FlyingInspirationExpPoint orCreateObject = flyingExpPool.GetOrCreateObject<FlyingInspirationExpPoint>();
			orCreateObject.gameObject.SetActive(value: false);
			orCreateObject.transform.SetParent(flyingExpOverlay, worldPositionStays: false);
			orCreateObject.transform.SetAsLastSibling();
			flyingExpPoints.Add(orCreateObject);
			FlyingInspirationExpPoint capturedPoint = orCreateObject;
			capturedPoint.Fly(startPos, worldPosition, icon, flyingExpPool, flyingExpPointDuration, delegate
			{
				OnFlyingExpPointReached(capturedPoint);
			});
		}
	}

	private void OnFlyingExpPointReached(FlyingInspirationExpPoint flyingPoint)
	{
		flyingExpPoints.Remove(flyingPoint);
		if (isShown && isPlayingExpFlyAnimation)
		{
			remainingFlyingExpCount--;
			talentExpProgressWidget.ApplyArrivedExpPoint(TryFinishFlyingExpAnimation);
		}
	}

	private void TryFinishFlyingExpAnimation()
	{
		if (isShown && isPlayingExpFlyAnimation && remainingFlyingExpCount <= 0 && !talentExpProgressWidget.HasPendingBarReset)
		{
			FinishFlyingExpAnimation();
		}
	}

	private void FinishFlyingExpAnimation()
	{
		isPlayingExpFlyAnimation = false;
		remainingFlyingExpCount = 0;
		flyingExpPoints.Clear();
		talentExpProgressWidget.ResetFillAnimationState();
		if (isShown)
		{
			RedrawInspirationsIfPending();
			RedrawTalentExpProgressWidget();
			TryShowInspirationTalentsTutorial();
		}
	}

	private void CancelFlyingExpAnimation()
	{
		isPlayingExpFlyAnimation = false;
		remainingFlyingExpCount = 0;
		for (int i = 0; i < flyingExpPoints.Count; i++)
		{
			if (flyingExpPoints[i] != null)
			{
				flyingExpPoints[i].StopAndRelease();
			}
		}
		flyingExpPoints.Clear();
		talentExpProgressWidget.ResetFillAnimationState();
		if (!isShown)
		{
			pendingInspirationsRedraw = false;
		}
		else if (base.isActiveAndEnabled)
		{
			RedrawInspirationsIfPending();
		}
	}

	private void RedrawInspirationsIfPending()
	{
		if (pendingInspirationsRedraw)
		{
			pendingInspirationsRedraw = false;
			UpdatePendingGamepadFocusFromCurrentFocus();
			DrawInspirations(data.TalentData);
			((RectTransform)base.transform).RefreshContentFitter();
			RestoreGamepadFocusAfterPurchaseIfPending();
		}
	}

	private void EnsureFlyingExpOverlay()
	{
		if (!(flyingExpOverlay != null))
		{
			RectTransform component = new GameObject("FlyingInspirationExpOverlay", typeof(RectTransform)).GetComponent<RectTransform>();
			component.SetParent(base.transform, worldPositionStays: false);
			component.anchorMin = Vector2.zero;
			component.anchorMax = Vector2.one;
			component.offsetMin = Vector2.zero;
			component.offsetMax = Vector2.zero;
			component.SetAsLastSibling();
			flyingExpOverlay = component;
		}
	}

	private void EnsureFlyingExpPool(TextMeshProUGUI sourceLabel)
	{
		if (flyingExpPool != null)
		{
			return;
		}
		EnsureFlyingExpOverlay();
		if (!(sourceLabel == null))
		{
			GameObject gameObject = Object.Instantiate(sourceLabel.gameObject, flyingExpOverlay);
			gameObject.name = "FlyingInspirationExpPoint";
			gameObject.SetActive(value: false);
			EventTrigger component = gameObject.GetComponent<EventTrigger>();
			if (component != null)
			{
				component.enabled = false;
			}
			TextMeshProUGUI component2 = gameObject.GetComponent<TextMeshProUGUI>();
			if (component2 != null)
			{
				component2.raycastTarget = false;
				component2.overflowMode = TextOverflowModes.Overflow;
				component2.textWrappingMode = TextWrappingModes.NoWrap;
				component2.alignment = TextAlignmentOptions.Center;
			}
			FlyingInspirationExpPoint flyingInspirationExpPoint = gameObject.GetComponent<FlyingInspirationExpPoint>();
			if (flyingInspirationExpPoint == null)
			{
				flyingInspirationExpPoint = gameObject.AddComponent<FlyingInspirationExpPoint>();
			}
			flyingExpPool = new Pool(flyingInspirationExpPoint, flyingExpOverlay, 0);
		}
	}

	public override List<LazyGameKeyTip> GetTips(GamepadNavigationItem gamepadNavigationItem)
	{
		List<LazyGameKeyTip> list = new List<LazyGameKeyTip>();
		if (gamepadNavigationItem == null)
		{
			return list;
		}
		if (gamepadNavigationItem.TryGetComponent<InspirationWidget>(out var component) && component != null && component.InspirationData != null)
		{
			list.Add(LazyGameKeyTip.Select(!component.IsBuyLocked && component.InspirationData.IsAvailableToBuy && component.InspirationData.IsCompleted));
		}
		if (gamepadNavigationItem.TryGetComponent<TalentLevelUpWidget>(out var component2) && component2 != null && component2.Data?.Def != null && MainGame.Instance?.GameSave?.talentSystemData != null)
		{
			list.Add(LazyGameKeyTip.Select(MainGame.Instance.GameSave.talentSystemData.CanPurchaseLevel(component2.Data.Def.id, out var _)));
		}
		return list;
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		Draw(new CharInspirationPageWidgetData(MainGame.Instance.GameSave.talentSystemData));
	}
}
