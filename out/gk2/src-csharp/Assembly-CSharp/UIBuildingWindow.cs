using System;
using System.Collections.Generic;
using DG.Tweening;
using LazyBearTechnology;
using LinqTools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class UIBuildingWindow : LazyWindow<UIBuildingWindowData>
{
	[SerializeField]
	private UIInfoWidget uiInfoWidget;

	[SerializeField]
	private UIBuildingWidget buildingElement;

	[SerializeField]
	[Space]
	private GameObject buildingsListContent;

	[SerializeField]
	private ScrollRect scrollRect;

	[SerializeField]
	private GameObject tabsContainer;

	[SerializeField]
	private RectTransform tabsContentRectTransform;

	[SerializeField]
	private AutoScroll autoScroll;

	[SerializeField]
	private GameObject noBuildingsObj;

	[SerializeField]
	private TextMeshProUGUI noBuildingsText;

	[Space]
	[SerializeField]
	private GameObject fightButtonsContainer;

	[Space]
	[SerializeField]
	private RectTransform fightButtonsRectTransform;

	[SerializeField]
	private UIDialogWindowButton startFightButton;

	[SerializeField]
	private UIDialogWindowButton endPreFightButton;

	private List<UIBuildingWidget> displayedBuildItemGUIs = new List<UIBuildingWidget>();

	private Action<BuildData, List<NeedItemData>> onBuildPressed;

	private Func<BuildData, List<NeedItemData>, bool> canBuild;

	[SerializeField]
	private TextMeshProUGUI nextTabGamepadHelper;

	[SerializeField]
	private TextMeshProUGUI prevTabGamepadHelper;

	private List<BuildingWindowTab> drawnTabs = new List<BuildingWindowTab>();

	private UIBuildingWidget foldedElement;

	private int currentTab;

	private List<string> tabs;

	private MultiInventory multiInventory;

	private bool tabsDrawn;

	private static readonly Dictionary<string, string> lastOpenedTabByWgoId = new Dictionary<string, string>();

	public override void Init()
	{
		base.Init();
		buildingElement.gameObject.SetActive(value: false);
		base.GamepadNavigationController.loopVerticalNavigation = true;
	}

	public override void Redraw()
	{
		onBuildPressed = data.OnBuildPressed;
		canBuild = data.CanBuild;
		multiInventory = new MultiInventory(data.PlayerData);
		if (data.AdditionalInventories != null)
		{
			foreach (Inventory additionalInventory in data.AdditionalInventories)
			{
				multiInventory.Add(additionalInventory);
			}
		}
		tabs = data.TabSortedBuilds.Keys.ToList();
		MoveDefaultTabFirst();
		tabsDrawn = tabs.Count > 1;
		currentTab = GetInitialTabIndex();
		if (tabsDrawn)
		{
			tabsContainer.SetActive(value: true);
			foreach (string tab in tabs)
			{
				BuildingWindowTab elementFromPool = UIPrefabsPooler.Instance.GetElementFromPool<BuildingWindowTab>(tabsContentRectTransform.transform);
				drawnTabs.Add(elementFromPool);
				elementFromPool.Init(tab, OnTabPressed);
				elementFromPool.UpdateState(isActive: false, canvas);
			}
			drawnTabs[currentTab].UpdateState(isActive: true, canvas);
		}
		else
		{
			tabsContainer.SetActive(value: false);
		}
		DrawTab(currentTab);
		UpdateNoBuildingsObject();
		uiInfoWidget.Draw(new UIInfoWidgetData(data.AssignedWgo.Data, (string)null, defineIconBackgroundFromWgo: false));
		bool flag = data.AssignedWgo.Data.Definition.interactionType == WGODef.InteractionType.FightBuilder && LazySingleton<FightingGameController>.Instance.CurrentFightState == FightState.InPreFight;
		fightButtonsContainer.gameObject.SetActive(flag);
		if (flag)
		{
			UIDialogWindowData.ButtonData buttonData = new UIDialogWindowData.ButtonData(StartFight, LLBase.L("start_fight_two_lines"), null, replaceForGamepad: true, GameKey.StartFight, LLBase.L("start_fight"));
			UIDialogWindowData.ButtonData buttonData2 = new UIDialogWindowData.ButtonData(EndPreFight, LLBase.L("end_pre_fight"), null, replaceForGamepad: true, GameKey.EndPrefight);
			startFightButton.Draw(buttonData);
			endPreFightButton.Draw(buttonData2);
		}
		base.Redraw();
		((RectTransform)base.transform).RefreshContentFitter();
		if (!flag)
		{
			return;
		}
		FitAndEqualizeChildrenInHorizontalGroup(fightButtonsRectTransform);
		if (LazyInput.IsGamepadActive)
		{
			for (int i = 0; i < fightButtonsRectTransform.childCount; i++)
			{
				fightButtonsRectTransform.GetChild(i).GetComponent<LayoutElement>().preferredWidth = -1f;
			}
		}
	}

	public void UpdateCurrentTab(string tabId)
	{
		if (tabsDrawn)
		{
			drawnTabs[currentTab].UpdateState(isActive: false, canvas);
			currentTab = drawnTabs.IndexOf(drawnTabs.Find((BuildingWindowTab t) => t.TabId == tabId));
			drawnTabs[currentTab].UpdateState(isActive: true, canvas);
		}
	}

	private void OnTabPressed(BuildingWindowTab tab)
	{
		DrawTab(tabs.IndexOf(tab.TabId));
	}

	private void DrawTab(int tab)
	{
		HideDisplayedBuildItems();
		if (foldedElement != null)
		{
			foldedElement.Fold();
			foldedElement = null;
		}
		if (tab < 0)
		{
			return;
		}
		lastOpenedTabByWgoId[data.AssignedWgo.Data.id] = tabs[tab];
		foreach (BuildData item in data.TabSortedBuilds[tabs[tab]])
		{
			UIBuildingWidget elementFromPool = UIPrefabsPooler.Instance.GetElementFromPool<UIBuildingWidget>(buildingsListContent.transform);
			displayedBuildItemGUIs.Add(elementFromPool);
			UIBuildingWidgetData uIBuildingWidgetData = new UIBuildingWidgetData(item, multiInventory, OnBuildPressed, canBuild, null, null, data.AssignedWgo.Data.WorldZoneData);
			elementFromPool.Init();
			elementFromPool.Draw(uIBuildingWidgetData);
		}
		if (tabsDrawn)
		{
			UpdateCurrentTab(tabs[tab]);
		}
		if (LazyInput.IsGamepadActive)
		{
			base.GamepadNavigationController.ReinitItems(focusOnFirstActive: true);
		}
		scrollRect.DOKill();
		scrollRect.verticalNormalizedPosition = 1f;
	}

	private int GetInitialTabIndex()
	{
		if (tabs.Count == 0)
		{
			return -1;
		}
		if (lastOpenedTabByWgoId.TryGetValue(data.AssignedWgo.Data.id, out var value))
		{
			int num = tabs.IndexOf(value);
			if (num >= 0)
			{
				return num;
			}
		}
		return 0;
	}

	private void MoveDefaultTabFirst()
	{
		int num = tabs.IndexOf("tab_building_default");
		if (num > 0)
		{
			tabs.RemoveAt(num);
			tabs.Insert(0, "tab_building_default");
		}
	}

	private void HideDisplayedBuildItems()
	{
		foreach (UIBuildingWidget displayedBuildItemGUI in displayedBuildItemGUIs)
		{
			displayedBuildItemGUI.DeInit();
			displayedBuildItemGUI.Hide();
			UIPrefabsPooler.Instance.ReleaseElementToPool(displayedBuildItemGUI);
		}
		displayedBuildItemGUIs.Clear();
	}

	private void FitAndEqualizeChildrenInHorizontalGroup(RectTransform rectTransform)
	{
		if (!(rectTransform == null))
		{
			for (int i = 0; i < rectTransform.childCount; i++)
			{
				Transform child = rectTransform.GetChild(i);
				Transform child2 = child.GetChild(0);
				child2.GetComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
				child2.GetComponent<LayoutElement>().preferredWidth = -1f;
				child.GetComponent<LayoutElement>().preferredWidth = -1f;
			}
			rectTransform.RefreshContentFitter();
			float num = 0f;
			for (int j = 0; j < rectTransform.childCount; j++)
			{
				RectTransform rectTransform2 = rectTransform.GetChild(j) as RectTransform;
				num = Mathf.Max(num, rectTransform2.rect.width);
			}
			for (int k = 0; k < rectTransform.childCount; k++)
			{
				Transform child3 = rectTransform.GetChild(k);
				Transform child4 = child3.GetChild(0);
				child4.GetComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
				child4.GetComponent<LayoutElement>().preferredWidth = num;
				child3.GetComponent<LayoutElement>().preferredWidth = num;
			}
			LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
		}
	}

	public override void Hide()
	{
		HideDisplayedBuildItems();
		foreach (BuildingWindowTab drawnTab in drawnTabs)
		{
			UIPrefabsPooler.Instance.ReleaseElementToPool(drawnTab);
		}
		drawnTabs.Clear();
		base.Hide();
	}

	private void OnBuildPressed(BuildData buildData, List<NeedItemData> needItems)
	{
		onBuildPressed?.Invoke(buildData, needItems);
	}

	protected override Dictionary<GameKey, Func<bool>> GetGameKeyDelegates()
	{
		Dictionary<GameKey, Func<bool>> gameKeyDelegates = base.GetGameKeyDelegates();
		gameKeyDelegates.Add(GameKey.Fold, FoldPress);
		gameKeyDelegates.Add(GameKey.NextTab, OnPressedNextTab);
		gameKeyDelegates.Add(GameKey.PrevTab, OnPressedPrevTab);
		gameKeyDelegates.Add(GameKey.RightClick, OnPressedBack);
		return gameKeyDelegates;
	}

	protected override void UpdateGamepadDependentStuff()
	{
		base.UpdateGamepadDependentStuff();
		if (!LazyInput.IsGamepadActive && foldedElement != null)
		{
			foldedElement.Fold();
			foldedElement = null;
		}
		if (LazyInput.IsGamepadActive)
		{
			nextTabGamepadHelper.gameObject.SetActive(value: true);
			prevTabGamepadHelper.gameObject.SetActive(value: true);
			nextTabGamepadHelper.text = ControllerIconLibrary.GetIconId(GameKey.NextTab);
			prevTabGamepadHelper.text = ControllerIconLibrary.GetIconId(GameKey.PrevTab);
		}
		else
		{
			nextTabGamepadHelper.gameObject.SetActive(value: false);
			prevTabGamepadHelper.gameObject.SetActive(value: false);
		}
	}

	public bool OnPressedPrevTab()
	{
		if (!tabsDrawn)
		{
			return false;
		}
		int num = currentTab;
		num--;
		if (num < 0)
		{
			num = tabs.Count - 1;
		}
		DrawTab(num);
		return true;
	}

	public bool OnPressedNextTab()
	{
		if (!tabsDrawn)
		{
			return false;
		}
		int num = currentTab;
		num++;
		if (num > tabs.Count - 1)
		{
			num = 0;
		}
		DrawTab(num);
		return true;
	}

	protected override bool OnPressedBack()
	{
		if (LazyInput.IsGamepadActive && base.GamepadNavigationController.FocusedItem != null && base.GamepadNavigationController.FocusedItem.TryGetComponent<UICraftItemCell>(out var _))
		{
			foldedElement.Fold();
			base.GamepadNavigationController.SetFocusedItem(foldedElement.GetComponentInParent<GamepadNavigationItem>());
			foldedElement = null;
			return true;
		}
		return base.OnPressedBack();
	}

	private bool FoldPress()
	{
		GamepadNavigationController gamepadNavigationController = base.GamepadNavigationController;
		if (gamepadNavigationController == null)
		{
			return false;
		}
		GamepadNavigationItem focusedItem = gamepadNavigationController.FocusedItem;
		if (focusedItem == null)
		{
			return false;
		}
		if (focusedItem.TryGetComponent<UIBuildingWidget>(out var component))
		{
			List<UICraftItemCell> displayedIngredients = component.DisplayedIngredients;
			if (displayedIngredients == null || displayedIngredients.Count == 0)
			{
				return false;
			}
			foreach (UICraftItemCell item in displayedIngredients)
			{
				if (item == null || item.ItemCell == null || item.GamepadNavigationItem == null)
				{
					return false;
				}
			}
			GamepadNavigationItem gamepadNavigationItem = displayedIngredients[0].GamepadNavigationItem;
			if (gamepadNavigationItem == null)
			{
				return false;
			}
			component.Unfold();
			foldedElement = component;
			gamepadNavigationController.SetFocusedItem(gamepadNavigationItem);
			return true;
		}
		if (focusedItem.TryGetComponent<UICraftItemCell>(out var _))
		{
			if (foldedElement == null)
			{
				return false;
			}
			List<UICraftItemCell> displayedIngredients2 = foldedElement.DisplayedIngredients;
			if (displayedIngredients2 == null)
			{
				return false;
			}
			foreach (UICraftItemCell item2 in displayedIngredients2)
			{
				if (item2 == null || item2.ItemCell == null || item2.GamepadNavigationItem == null)
				{
					return false;
				}
			}
			GamepadNavigationItem componentInParent = foldedElement.GetComponentInParent<GamepadNavigationItem>();
			if (componentInParent == null)
			{
				return false;
			}
			foldedElement.Fold();
			gamepadNavigationController.SetFocusedItem(componentInParent);
			foldedElement = null;
			return true;
		}
		return false;
	}

	protected override void PrintTips(GamepadNavigationItem gamepadNavigationItem)
	{
		List<LazyGameKeyTip> list = new List<LazyGameKeyTip>();
		list.Add(LazyGameKeyTip.Select());
		if (gamepadNavigationItem.TryGetComponent<UIBuildingWidget>(out var component) && component.DisplayedIngredients.Count > 0)
		{
			list.Add(new LazyGameKeyTip(GameKey.Fold, "tip_unfold"));
		}
		if (gamepadNavigationItem.TryGetComponent<UICraftItemCell>(out var _))
		{
			list.Add(new LazyGameKeyTip(GameKey.Fold, "tip_fold"));
			list.Add(new LazyGameKeyTip(GameKey.Back, "tip_back"));
		}
		else if ((bool)closeButton)
		{
			list.Add(LazyGameKeyTip.Back());
		}
		lazyButtonTips.Print(list);
	}

	private void StartFight()
	{
		LazySingleton<FightingGameController>.Instance.Play();
		Close();
	}

	private void EndPreFight()
	{
		if (!(LazySingleton<FightingGameController>.Instance.CurrentLevel == null))
		{
			Close();
			LazyUI.Get<UIFade>().Fade(1f, delegate
			{
				LazySingleton<FightingGameController>.Instance.CancelPreFight();
			});
		}
	}

	private void UpdateNoBuildingsObject()
	{
		if (tabs.Count == 0)
		{
			noBuildingsObj.gameObject.SetActive(value: true);
			if (LazySingleton<FightingGameController>.Instance.CurrentLevel != null)
			{
				FightDef fightDef = GameBalance.Me.GetData<FightDef>(LazySingleton<FightingGameController>.Instance.CurrentLevel.id);
				if (fightDef != null && fightDef.isBarricadesUnavailable && fightDef.isTowersUnavailable)
				{
					noBuildingsText.text = LLBase.L("ui_fight_construction_unavailable") ?? "";
				}
				else
				{
					noBuildingsText.text = LLBase.L("ui_builddesk_is_empty") ?? "";
				}
			}
			else
			{
				noBuildingsText.text = LLBase.L("ui_builddesk_is_empty") ?? "";
			}
		}
		else
		{
			noBuildingsObj.gameObject.SetActive(value: false);
		}
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		LazySingleton<BuildManager>.Instance.TryEnable(GameScene.GetWgoViewGlobal(MainGame.WorldData.GetWgoData("builder_graveyard").UniqueId));
	}
}
