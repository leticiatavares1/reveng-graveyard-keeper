using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class UICraftWindow : UIBaseCraftWindow, IUIWindowCustomOperable
{
	[SerializeField]
	private UIInfoWidget uiInfoWidget;

	[SerializeField]
	private RectTransform contentRectTransform;

	[SerializeField]
	private float contentHeightBig;

	[SerializeField]
	private float contentHeightSmall;

	[SerializeField]
	[Space]
	private GameObject craftListContent;

	[SerializeField]
	private GameObject queueListContent;

	[SerializeField]
	private GameObject noQueueElementsGo;

	[SerializeField]
	private GameObject contentQueue;

	[SerializeField]
	private GameObject frameQueue;

	[SerializeField]
	private LazyScrollRect craftScrollRect;

	[SerializeField]
	private AutoScroll queueAutoScroll;

	[SerializeField]
	private Transform otstoynik;

	private UICraftQueueElementWidget currentCraftQueueElement;

	private List<UICraftsTabWidget> displayedTabs = new List<UICraftsTabWidget>();

	private List<UICraftsTabSeparatorWidget> displayedTabSeparators = new List<UICraftsTabSeparatorWidget>();

	private List<UICraftQueueElementWidget> craftQueueElements = new List<UICraftQueueElementWidget>();

	private RectTransform craftListContentTransform;

	private List<UICraftPreviewItemCell> craftPreviewItemCells = new List<UICraftPreviewItemCell>();

	private Action<CraftElement> onCraftAddedToQueuePressed;

	private Action<CraftElement> onCraftStartPressed;

	private bool isQueueSelectedOnGamepad;

	private bool isQueueUiEnabled = true;

	private bool isCompactLayoutApplied;

	private Vector2 fullContentSizeDelta;

	private Vector2 fullCraftScrollSizeDelta;

	private Vector2 fullCraftScrollAnchoredPosition;

	private Vector2 fullFadeDownAnchoredPosition;

	private Vector2 fullBackMaskSizeDelta;

	private Vector2 fullBackMaskAnchoredPosition;

	private bool hasStoredBackMaskLayout;

	private RectTransform craftScrollRectTransform;

	[SerializeField]
	private RectTransform backMaskRect;

	[SerializeField]
	private RectTransform fadeDownRect;

	private const float BackMaskBottomPaddingCompact = 13f;

	private const float CompactExtraHeightReduction = 50f;

	private readonly HoldRepeatValueChanger queueCountHold = new HoldRepeatValueChanger();

	private UICraftQueueElementWidget queueCountHoldTarget;

	public UIBaseCraftWindowData Data => data;

	public override void Init()
	{
		base.Init();
		craftListContentTransform = craftListContent.GetComponent<RectTransform>();
		craftScrollRectTransform = ((craftScrollRect != null) ? ((RectTransform)craftScrollRect.transform) : null);
		StoreBackMaskFullLayout();
	}

	public override void Open(UIBaseCraftWindowData data)
	{
		base.Open(data);
		isQueueSelectedOnGamepad = false;
		if (LazyInput.IsGamepadActive)
		{
			base.GamepadNavigationController.ReinitItems(focusOnFirstActive: true);
		}
	}

	public override void Redraw()
	{
		onCraftAddedToQueuePressed = data.OnAddToQueuePressed;
		onCraftStartPressed = data.OnStartCraftPressed;
		data.onCraftAddedToQueue = OnAddToQeueButtonPressed;
		data.onCraftRemovedFromQueue = OnRemovedFromQueue;
		data.SubscribeEvents();
		foreach (KeyValuePair<string, List<CraftDef>> craftsByTab in data.CraftsByTabs)
		{
			if (craftsByTab.Value.Count != 0)
			{
				DisplayTabWidget(craftsByTab.Key, craftsByTab.Value);
			}
		}
		foreach (KeyValuePair<string, List<CraftDef>> extensionCraft in data.ExtensionCrafts)
		{
			DisplayTabWidget(extensionCraft.Key, extensionCraft.Value);
		}
		isQueueUiEnabled = data == null || !data.IsAddToQueueDisabledForAllCrafts;
		if (isQueueUiEnabled)
		{
			data.CraftComponent.UpdateQueueElementsCraftStatus();
			ShowQueueElements();
			UpdateQueueVisualState();
		}
		else
		{
			HideQueueElements();
		}
		uiInfoWidget.Draw(data.InfoWidgetData);
		((RectTransform)base.transform).RefreshContentFitter();
		base.Redraw();
		ApplyQueueLayout();
		if (LazyInput.IsGamepadActive)
		{
			base.GamepadNavigationController.ReinitItems(focusOnFirstActive: true);
		}
	}

	public override void Hide()
	{
		if (data != null)
		{
			data.UnsubscribeEvents();
		}
		foreach (UICraftsTabWidget displayedTab in displayedTabs)
		{
			displayedTab.Hide();
			UIPrefabsPooler.Instance.ReleaseElementToPool(displayedTab);
		}
		displayedTabs.Clear();
		foreach (UICraftsTabSeparatorWidget displayedTabSeparator in displayedTabSeparators)
		{
			displayedTabSeparator.Hide();
			UIPrefabsPooler.Instance.ReleaseElementToPool(displayedTabSeparator);
		}
		displayedTabSeparators.Clear();
		HideQueueElements();
		uiInfoWidget.Hide();
		RestoreLayoutIfCompact();
		RestoreBackMaskFullLayout();
		SetQueueObjectsActive(active: true);
		isQueueUiEnabled = true;
		base.Hide();
	}

	private void RemoveQueueWidget(UICraftQueueElementWidget uiCraftQueueElementWidget)
	{
		craftQueueElements.Remove(uiCraftQueueElementWidget);
		uiCraftQueueElementWidget.DeInit();
		uiCraftQueueElementWidget.Hide();
		UIPrefabsPooler.Instance.ReleaseElementToPool(uiCraftQueueElementWidget);
		UpdateQueueVisualState();
	}

	public UICraftPreviewItemCell GetCraftWidget(Transform parent, UICraftPreviewItemCellData data)
	{
		UICraftPreviewItemCell elementFromPool = UIPrefabsPooler.Instance.GetElementFromPool<UICraftPreviewItemCell>(otstoynik);
		elementFromPool.Draw(data);
		craftPreviewItemCells.Add(elementFromPool);
		if (data != null)
		{
			if (data.IsTab)
			{
				parent.transform.SetAsFirstSibling();
			}
			else if (data.IsUnknown)
			{
				parent.transform.SetAsLastSibling();
			}
			if (data.IsTab)
			{
				elementFromPool.ItemCellGamepadNavigationItem.Active = false;
			}
			else
			{
				elementFromPool.ItemCellGamepadNavigationItem.Active = true;
			}
		}
		else
		{
			elementFromPool.ItemCellGamepadNavigationItem.Active = false;
		}
		return elementFromPool;
	}

	public void ReleaseCraftWidget(UICraftPreviewItemCell cellWidget)
	{
		cellWidget.Hide();
		UIPrefabsPooler.Instance.ReleaseElementToPool(cellWidget);
		craftPreviewItemCells.Remove(cellWidget);
	}

	private void OnCraftStartPressed(Action<CraftElement> action, CraftDef craftDefinition, List<NeedItemData> selectedNeedItems, CraftParamsData craftParams, int craftsCount = 1)
	{
		CraftElement craftElement = (craftDefinition.isConveyorCraft ? new ConveyorCraftElement(craftDefinition.id, craftsCount, selectedNeedItems, craftParams) : new CraftElement(craftDefinition.id, craftsCount, selectedNeedItems, craftParams));
		if (craftDefinition.isFuelCraft)
		{
			craftElement.DoBeforeStartCalculations(data.AssignedWgo.Data);
			ItemDef itemDef = GameBalance.Me.GetData<ItemDef>(craftElement.Definition.addItemsToWgoOnFinish.chanceOutputItems[0].id);
			int num = Mathf.FloorToInt((float)data.AssignedWgo.Data.Inventory.Data.CanAddItemCountToInventory(itemDef, 99999) / (float)craftElement.PreToWgoOnFinishItems[0].count);
			craftElement.Count = ((craftsCount > num) ? num : craftsCount);
		}
		action?.Invoke(craftElement);
		UpdateCraftsLocks();
	}

	private void AddQueueItem(CraftElementBase queueElement)
	{
		UICraftQueueElementWidget elementFromPool = UIPrefabsPooler.Instance.GetElementFromPool<UICraftQueueElementWidget>(queueListContent.transform);
		craftQueueElements.Add(elementFromPool);
		UICraftQueueElementWidgetData uICraftQueueElementWidgetData = new UICraftQueueElementWidgetData(data.AssignedWgo.Data, queueElement, data.CraftComponent.CraftElementsQueue, null, null, null, null, TryUpToQueue, TryDownToQueue, data.OnQueueElementRemovePressed);
		elementFromPool.Init();
		elementFromPool.Draw(uICraftQueueElementWidgetData);
	}

	private bool IsCraftItemCellFocusedByGamepad(out UICraftItemCell craftItemCell)
	{
		craftItemCell = null;
		if (!LazyInput.IsGamepadActive)
		{
			return false;
		}
		GamepadNavigationItem focusedItem = base.GamepadNavigationController.FocusedItem;
		if (focusedItem != null && focusedItem.TryGetComponent<UICraftItemCell>(out craftItemCell))
		{
			return true;
		}
		return false;
	}

	private void HideQueueElements()
	{
		foreach (UICraftQueueElementWidget craftQueueElement in craftQueueElements)
		{
			craftQueueElement.DeInit();
			craftQueueElement.Hide();
			UIPrefabsPooler.Instance.ReleaseElementToPool(craftQueueElement);
		}
		craftQueueElements.Clear();
	}

	private void ShowQueueElements()
	{
		if (!isQueueUiEnabled || !UpdateQueueVisualState())
		{
			return;
		}
		foreach (CraftElementBase item in data.CraftComponent.CraftElementsQueue)
		{
			AddQueueItem(item);
		}
	}

	private void RedrawQueueElements()
	{
		HideQueueElements();
		ShowQueueElements();
	}

	private void TryUpToQueue(CraftElementBase queueElement)
	{
		if (data.CraftComponent.TryElementUpToQueue(queueElement))
		{
			RedrawQueueElements();
		}
	}

	private void TryDownToQueue(CraftElementBase queueElement)
	{
		if (data.CraftComponent.TryElementDownToQueue(queueElement))
		{
			RedrawQueueElements();
		}
	}

	private void OnAddToQeueButtonPressed(CraftElementBase queueElement)
	{
		if (isQueueUiEnabled)
		{
			AddQueueItem(queueElement);
			RedrawQueueElements();
			PrintTips();
			craftListContentTransform.RefreshContentFitter();
		}
	}

	private void OnRemovedFromQueue(CraftElementBase queueElement)
	{
		foreach (UICraftQueueElementWidget craftQueueElement in craftQueueElements)
		{
			if (craftQueueElement.Data.CraftQueueElement != queueElement)
			{
				continue;
			}
			bool num = LazyInput.IsGamepadActive && craftQueueElement.OutputItem.GamepadNavigationItem.IsFocused;
			RemoveQueueWidget(craftQueueElement);
			if (num)
			{
				if (craftQueueElements.Count > 0)
				{
					isQueueSelectedOnGamepad = true;
					base.GamepadNavigationController.FocusOnFirstActive(1);
				}
				else
				{
					isQueueSelectedOnGamepad = false;
					base.GamepadNavigationController.FocusOnFirstActive(0);
				}
			}
			PrintTips();
			break;
		}
	}

	private bool HasQueueElements()
	{
		if (data?.CraftComponent?.CraftElementsQueue != null)
		{
			return data.CraftComponent.CraftElementsQueue.Count > 0;
		}
		return false;
	}

	private bool UpdateQueueVisualState()
	{
		if (!isQueueUiEnabled)
		{
			return false;
		}
		if (!HasQueueElements())
		{
			noQueueElementsGo.SetActive(value: true);
			return false;
		}
		noQueueElementsGo.SetActive(value: false);
		return true;
	}

	private void ApplyQueueLayout()
	{
		bool flag = data != null && data.IsAddToQueueDisabledForAllCrafts;
		isQueueUiEnabled = data == null || !flag;
		RestoreLayoutIfCompact();
		ApplyFullContentHeight();
		SetQueueObjectsActive(isQueueUiEnabled);
		if (!isQueueUiEnabled)
		{
			isQueueSelectedOnGamepad = false;
			ApplyCompactLayout();
			SetBackMaskBottomPadding(13f);
		}
		else
		{
			RestoreBackMaskFullLayout();
		}
	}

	private void ApplyFullContentHeight()
	{
		float y = ((GUIElements.Instance.UIWindowSizeType == UIWindowSizeType.Big) ? contentHeightBig : contentHeightSmall);
		contentRectTransform.sizeDelta = new Vector2(contentRectTransform.sizeDelta.x, y);
	}

	private void CaptureFullLayoutSnapshot()
	{
		fullContentSizeDelta = contentRectTransform.sizeDelta;
		if (craftScrollRectTransform != null)
		{
			fullCraftScrollSizeDelta = craftScrollRectTransform.sizeDelta;
			fullCraftScrollAnchoredPosition = craftScrollRectTransform.anchoredPosition;
		}
		if (fadeDownRect != null)
		{
			fullFadeDownAnchoredPosition = fadeDownRect.anchoredPosition;
		}
	}

	private void RestoreLayoutIfCompact()
	{
		if (isCompactLayoutApplied)
		{
			contentRectTransform.sizeDelta = fullContentSizeDelta;
			if (craftScrollRectTransform != null)
			{
				craftScrollRectTransform.sizeDelta = fullCraftScrollSizeDelta;
				craftScrollRectTransform.anchoredPosition = fullCraftScrollAnchoredPosition;
			}
			if (fadeDownRect != null)
			{
				fadeDownRect.anchoredPosition = fullFadeDownAnchoredPosition;
			}
			isCompactLayoutApplied = false;
		}
	}

	private void SetQueueObjectsActive(bool active)
	{
		if (contentQueue != null)
		{
			contentQueue.SetActive(active);
		}
		if (frameQueue != null)
		{
			frameQueue.SetActive(active);
		}
	}

	private void StoreBackMaskFullLayout()
	{
		if (!hasStoredBackMaskLayout && !(backMaskRect == null))
		{
			fullBackMaskSizeDelta = backMaskRect.sizeDelta;
			fullBackMaskAnchoredPosition = backMaskRect.anchoredPosition;
			hasStoredBackMaskLayout = true;
		}
	}

	private void RestoreBackMaskFullLayout()
	{
		if (!(backMaskRect == null) && hasStoredBackMaskLayout)
		{
			backMaskRect.sizeDelta = fullBackMaskSizeDelta;
			backMaskRect.anchoredPosition = fullBackMaskAnchoredPosition;
		}
	}

	private void SetBackMaskBottomPadding(float padding)
	{
		if (!(backMaskRect == null))
		{
			StoreBackMaskFullLayout();
			Vector2 offsetMin = backMaskRect.offsetMin;
			offsetMin.y = padding;
			backMaskRect.offsetMin = offsetMin;
		}
	}

	private void ApplyCompactLayout()
	{
		if (isCompactLayoutApplied || craftScrollRectTransform == null)
		{
			return;
		}
		float y = craftScrollRectTransform.offsetMin.y;
		if (!(y <= 0.01f))
		{
			CaptureFullLayoutSnapshot();
			contentRectTransform.sizeDelta = new Vector2(contentRectTransform.sizeDelta.x, contentRectTransform.sizeDelta.y - y - 50f);
			Vector2 offsetMin = craftScrollRectTransform.offsetMin;
			offsetMin.y = 0f;
			craftScrollRectTransform.offsetMin = offsetMin;
			if (fadeDownRect != null)
			{
				Canvas.ForceUpdateCanvases();
				RectTransform rectTransform = (RectTransform)fadeDownRect.parent;
				fadeDownRect.anchoredPosition = new Vector2(fadeDownRect.anchoredPosition.x, rectTransform.rect.yMin + fadeDownRect.rect.height * fadeDownRect.pivot.y);
			}
			isCompactLayoutApplied = true;
		}
	}

	protected override Dictionary<GameKey, Func<bool>> GetGameKeyDelegates()
	{
		Dictionary<GameKey, Func<bool>> gameKeyDelegates = base.GetGameKeyDelegates();
		gameKeyDelegates.Add(GameKey.CraftWindowZoneSwitch, OnSwitchZonePressed);
		gameKeyDelegates.Add(GameKey.DpadDown, OnDpadDown);
		gameKeyDelegates.Add(GameKey.DpadUp, OnDpadUp);
		gameKeyDelegates.Add(GameKey.Down, OnDpadDown);
		gameKeyDelegates.Add(GameKey.Up, OnDpadUp);
		gameKeyDelegates.Add(GameKey.CraftWindowQueueRight, OnQueueRight);
		gameKeyDelegates.Add(GameKey.CraftWindowQueueLeft, OnQueueLeft);
		gameKeyDelegates.Add(GameKey.RightClick, OnPressedBack);
		return gameKeyDelegates;
	}

	private bool OnSwitchZonePressed()
	{
		if (!isQueueUiEnabled)
		{
			return false;
		}
		if (LazyInput.IsGamepadActive)
		{
			if (isQueueSelectedOnGamepad)
			{
				isQueueSelectedOnGamepad = false;
				base.GamepadNavigationController.FocusOnFirstActive(0);
			}
			else if (craftQueueElements.Count > 0)
			{
				isQueueSelectedOnGamepad = true;
				base.GamepadNavigationController.FocusOnFirstActive(1);
			}
		}
		return true;
	}

	protected override void UpdateGamepadDependentStuff()
	{
		base.UpdateGamepadDependentStuff();
		isQueueSelectedOnGamepad = false;
		base.GamepadNavigationController.FocusOnFirstActive(0);
	}

	protected override void Update()
	{
		TickQueueCountHold();
		base.Update();
	}

	private void TickQueueCountHold()
	{
		if (!base.IsShownAndTop || !isQueueUiEnabled || !isQueueSelectedOnGamepad || base.GamepadNavigationController.FocusedItem == null)
		{
			ResetQueueCountHold();
			return;
		}
		UICraftQueueElementWidget componentInParent = base.GamepadNavigationController.FocusedItem.GetComponentInParent<UICraftQueueElementWidget>();
		if (componentInParent == null)
		{
			ResetQueueCountHold();
			return;
		}
		if (queueCountHoldTarget != componentInParent)
		{
			queueCountHold.Reset();
			queueCountHoldTarget = componentInParent;
		}
		if (HoldRepeatValueChanger.GetPointerHoldDirection(componentInParent.PlusCraftButton, componentInParent.MinusCraftButton) != 0)
		{
			queueCountHold.Reset();
			return;
		}
		bool flag = (HoldRepeatValueChanger.IsAnyKeyHeld(GameKey.DpadUp, GameKey.Up) || HoldRepeatValueChanger.GetAxisHoldDirection(vertical: true) > 0) && componentInParent.PlusCraftButton != null && componentInParent.PlusCraftButton.interactable;
		bool flag2 = (HoldRepeatValueChanger.IsAnyKeyHeld(GameKey.DpadDown, GameKey.Down) || HoldRepeatValueChanger.GetAxisHoldDirection(vertical: true) < 0) && componentInParent.MinusCraftButton != null && componentInParent.MinusCraftButton.interactable;
		int direction = 0;
		if (flag != flag2)
		{
			direction = (flag ? 1 : (-1));
		}
		queueCountHold.Tick(direction, componentInParent.ChangeCount);
	}

	private void ResetQueueCountHold()
	{
		queueCountHold.Reset();
		queueCountHoldTarget = null;
	}

	private bool OnDpadDown()
	{
		if (!LazyInput.IsGamepadActive || base.GamepadNavigationController.FocusedItem == null)
		{
			return false;
		}
		if (isQueueUiEnabled && isQueueSelectedOnGamepad && base.GamepadNavigationController.FocusedItem != null)
		{
			UICraftQueueElementWidget componentInParent = base.GamepadNavigationController.FocusedItem.GetComponentInParent<UICraftQueueElementWidget>();
			if (componentInParent == null || componentInParent.MinusCraftButton == null || !componentInParent.MinusCraftButton.interactable)
			{
				return false;
			}
			return true;
		}
		return lazyWindowInputController.OnPressedDownDpad();
	}

	private bool OnDpadUp()
	{
		if (!LazyInput.IsGamepadActive || base.GamepadNavigationController.FocusedItem == null)
		{
			return false;
		}
		if (isQueueUiEnabled && isQueueSelectedOnGamepad && base.GamepadNavigationController.FocusedItem != null)
		{
			UICraftQueueElementWidget componentInParent = base.GamepadNavigationController.FocusedItem.GetComponentInParent<UICraftQueueElementWidget>();
			if (componentInParent == null || componentInParent.PlusCraftButton == null || !componentInParent.PlusCraftButton.interactable)
			{
				return false;
			}
			return true;
		}
		return lazyWindowInputController.OnPressedUpDpad();
	}

	private bool OnQueueRight()
	{
		return TryMoveFocusedQueueItem(moveRight: true);
	}

	private bool OnQueueLeft()
	{
		return TryMoveFocusedQueueItem(moveRight: false);
	}

	private bool TryMoveFocusedQueueItem(bool moveRight)
	{
		if (!isQueueUiEnabled || !isQueueSelectedOnGamepad || base.GamepadNavigationController.FocusedItem == null)
		{
			return false;
		}
		UICraftQueueElementWidget componentInParent = base.GamepadNavigationController.FocusedItem.GetComponentInParent<UICraftQueueElementWidget>();
		if (componentInParent == null || componentInParent.Data == null || componentInParent.Data.CraftQueueElement == null)
		{
			return false;
		}
		CraftElementBase craftQueueElement = componentInParent.Data.CraftQueueElement;
		if (moveRight)
		{
			if (componentInParent.QueueDownButton == null || !componentInParent.QueueDownButton.interactable)
			{
				return false;
			}
			componentInParent.QueueDownButton.onClick.Invoke();
		}
		else
		{
			if (componentInParent.QueueUpButton == null || !componentInParent.QueueUpButton.interactable)
			{
				return false;
			}
			componentInParent.QueueUpButton.onClick.Invoke();
		}
		FocusQueueElement(craftQueueElement);
		return true;
	}

	private void FocusQueueElement(CraftElementBase craftElement)
	{
		foreach (UICraftQueueElementWidget craftQueueElement in craftQueueElements)
		{
			if (craftQueueElement == null || craftQueueElement.Data == null || craftQueueElement.Data.CraftQueueElement != craftElement)
			{
				continue;
			}
			GamepadNavigationItem componentInChildren = craftQueueElement.GetComponentInChildren<GamepadNavigationItem>();
			if (!(componentInChildren == null))
			{
				((RectTransform)queueListContent.transform).RefreshContentFitter();
				if (queueAutoScroll != null)
				{
					queueAutoScroll.SkipNextAutoscroll = true;
				}
				base.GamepadNavigationController.SetFocusedItem(componentInChildren);
				if (queueAutoScroll != null)
				{
					queueAutoScroll.ScrollToItem(componentInChildren);
				}
				craftQueueElement.OnOver();
			}
			break;
		}
	}

	protected override void PrintTips()
	{
		PrintTips(base.GamepadNavigationController.FocusedItem);
	}

	protected override void PrintTips(GamepadNavigationItem gamepadNavigationItem)
	{
		List<LazyGameKeyTip> list = new List<LazyGameKeyTip>();
		if (isQueueUiEnabled && HasQueueElements())
		{
			list.Add(new LazyGameKeyTip(GameKey.CraftWindowZoneSwitch, "tip_switch"));
		}
		if (gamepadNavigationItem != null)
		{
			UICraftQueueElementWidget uICraftQueueElementWidget = (isQueueUiEnabled ? gamepadNavigationItem.GetComponentInParent<UICraftQueueElementWidget>() : null);
			if (uICraftQueueElementWidget != null)
			{
				list.Add(new LazyGameKeyTip(GameKey.DpadUp, "+"));
				list.Add(new LazyGameKeyTip(GameKey.DpadDown, "-"));
				if (uICraftQueueElementWidget.QueueUpButton.interactable)
				{
					list.Add(new LazyGameKeyTip(GameKey.CraftWindowQueueLeft, "<"));
				}
				if (uICraftQueueElementWidget.QueueDownButton.interactable)
				{
					list.Add(new LazyGameKeyTip(GameKey.CraftWindowQueueRight, ">"));
				}
			}
			else
			{
				list.Add(LazyGameKeyTip.Select());
			}
		}
		if ((bool)closeButton)
		{
			list.Add(LazyGameKeyTip.Back());
		}
		lazyButtonTips.Print(list);
	}

	private void UpdateCraftsLocks()
	{
		foreach (UICraftPreviewItemCell craftPreviewItemCell in craftPreviewItemCells)
		{
			craftPreviewItemCell.UpdateLocks();
		}
	}

	private void DisplayTabWidget(string crafterId, List<CraftDef> crafts)
	{
		if (displayedTabs.Count != 0)
		{
			UICraftsTabSeparatorWidgetData uICraftsTabSeparatorWidgetData = new UICraftsTabSeparatorWidgetData();
			UICraftsTabSeparatorWidget elementFromPool = UIPrefabsPooler.Instance.GetElementFromPool<UICraftsTabSeparatorWidget>(craftListContent.transform);
			elementFromPool.Draw(uICraftsTabSeparatorWidgetData);
			displayedTabSeparators.Add(elementFromPool);
		}
		UICraftsTabWidgetData uICraftsTabWidgetData = new UICraftsTabWidgetData(data.AssignedWgo.Data, crafts, crafterId, delegate(CraftDef craftDef, List<NeedItemData> needsData, CraftParamsData cpd, int count)
		{
			OnCraftStartPressed(onCraftAddedToQueuePressed, craftDef, needsData, cpd, count);
		}, delegate(CraftDef craftDef, List<NeedItemData> needsData, CraftParamsData cpd, int count)
		{
			OnCraftStartPressed(onCraftStartPressed, craftDef, needsData, cpd, count);
		}, data.IsGravePartRemove);
		UICraftsTabWidget elementFromPool2 = UIPrefabsPooler.Instance.GetElementFromPool<UICraftsTabWidget>(craftListContent.transform);
		elementFromPool2.Draw(uICraftsTabWidgetData);
		displayedTabs.Add(elementFromPool2);
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		Wgo wgo = Wgo.Spawn(new WgoData("woodworking_workbench_1", MainGame.PlayerController.MovablePosition, MainGame.PlayerData.currentGameSceneId), MainGame.PlayerController.CurrentGameScene.transform);
		CraftInteractionHandler craftInteractionHandler = new CraftInteractionHandler();
		craftInteractionHandler.Init(wgo);
		craftInteractionHandler.HasInteraction(MainGame.PlayerController);
		craftInteractionHandler.Interact(MainGame.PlayerController);
	}

	bool IUIWindowCustomOperable.get_IsShown()
	{
		return base.IsShown;
	}
}
