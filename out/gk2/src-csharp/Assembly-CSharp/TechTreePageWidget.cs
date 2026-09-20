using System;
using System.Collections.Generic;
using DG.Tweening;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class TechTreePageWidget : LazyWidget<TechTreePageWidgetData>
{
	[SerializeField]
	private GamepadNavigationController gamepadNavigationController;

	[SerializeField]
	private TextMeshProUGUI redSpheresLabel;

	[SerializeField]
	private TextMeshProUGUI greenSpheresLabel;

	[SerializeField]
	private TextMeshProUGUI blueSpheresLabel;

	[SerializeField]
	private GameObject activeConnectorsContent;

	[SerializeField]
	private GameObject inactiveConnectorsContent;

	[SerializeField]
	private GameObject backgroundConnectorsContent;

	[SerializeField]
	private GameObject techLayerBack;

	[SerializeField]
	private GameObject techLayerFront;

	[SerializeField]
	private LinkedEntityWidget unlockWidgetPrefab;

	[SerializeField]
	private TextMeshProUGUI nextSubTabGamepadHelper;

	[SerializeField]
	private TextMeshProUGUI prevSubTabGamepadHelper;

	[SerializeField]
	private TextMeshProUGUI smallVersionTabLabel;

	[SerializeField]
	[Space]
	private TechTreeTabButton tabButtonPrefab;

	private List<TechTreeTabButton> tabButtons = new List<TechTreeTabButton>();

	[Space]
	private TechTreeTab currentTab;

	[Space]
	[SerializeField]
	private LazyScrollRect scrollRect;

	[SerializeField]
	private AutoScroll autoScroll;

	[Space]
	[SerializeField]
	private TechTreeConnectorPortData commonWidgetConnectorPortData;

	[SerializeField]
	private TechTreeConnectorPortData repWidgetConnectorPortData;

	[Space]
	[SerializeField]
	private Vector2 edgeOffset = new Vector2(10f, 10f);

	[SerializeField]
	private float edgeOffsetContentAdditional;

	[SerializeField]
	private Vector2 technologyOffset = new Vector2(210f, 80f);

	[SerializeField]
	private Vector2 repWidgetOffset = new Vector2(54f, 16f);

	[SerializeField]
	private Vector2 techElementSize = new Vector2(160f, 80f);

	[SerializeField]
	private Vector2 techElementSizeRep = new Vector2(46f, 50f);

	private List<TechTreeConnector> techConnectors = new List<TechTreeConnector>();

	private static Pool unlocksPool;

	public static Pool UnlocksPool => unlocksPool;

	public override void Init()
	{
		base.Init();
		TechDef.InitTechs();
		tabButtonPrefab.gameObject.SetActive(value: false);
		string[] names = Enum.GetNames(typeof(TechTreeTab));
		int num = 0;
		foreach (object value in Enum.GetValues(typeof(TechTreeTab)))
		{
			TechTreeTabButton techTreeTabButton = tabButtonPrefab.Copy(tabButtonPrefab.transform.parent);
			string text = "tech_tab_" + names[num];
			techTreeTabButton.Init((TechTreeTab)value, LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(text, "i_tech_tree_tab_placeholder"), text, OnTechTreeTabButtonClicked);
			num++;
			techTreeTabButton.gameObject.SetActive(value: true);
			tabButtons.Add(techTreeTabButton);
		}
		scrollRect.Init(GetTechWidget, ReleaseWidgetForParent, gamepadNavigationController);
		SmoothMouseWheelScroll.Ensure(scrollRect);
		unlocksPool = new Pool(unlockWidgetPrefab, base.transform, 0);
	}

	private void OnDestroy()
	{
		unlocksPool = null;
	}

	public override void Redraw()
	{
		base.Redraw();
		RedrawSpheres();
		for (int i = 0; i < tabButtons.Count; i++)
		{
			tabButtons[i].gameObject.SetActive(MainGame.Instance.GameSave.knowledgeSystem.IsTechTabUnlocked(tabButtons[i].TechTreeTab));
		}
		UpdateGamepadDependentStuff();
	}

	private void RedrawSpheres()
	{
		redSpheresLabel.text = Param.FormIconFromPlayerRes("tech_red");
		greenSpheresLabel.text = Param.FormIconFromPlayerRes("tech_green");
		blueSpheresLabel.text = Param.FormIconFromPlayerRes("tech_blue");
	}

	public override void Hide()
	{
		HideCurrentElements();
		base.Hide();
	}

	public void DisplayLastTab(string focusOnTech = "")
	{
		DisplayTab(currentTab, focusOnTech);
	}

	public void DisplayTab(TechTreeTab tab, string focusOnTech = "")
	{
		currentTab = tab;
		foreach (TechTreeTabButton tabButton in tabButtons)
		{
			tabButton.SetState(tabButton.TechTreeTab == currentTab);
		}
		if (smallVersionTabLabel != null)
		{
			smallVersionTabLabel.text = LLBase.L($"tech_tab_{currentTab}");
		}
		HideCurrentElements();
		HideConnectors();
		List<TechDef> list = new List<TechDef>();
		foreach (TechDef techDef6 in GameBalance.Me.techDefs)
		{
			bool flag = ShouldDisplayTech(techDef6);
			UnlockReputationTechIfAvailable(techDef6, flag);
			if (techDef6.tab == currentTab && flag)
			{
				list.Add(techDef6);
			}
		}
		LazyScrollableElement lazyScrollableElement = null;
		float num = 255f;
		float num2 = -255f;
		for (int i = 0; i < list.Count; i++)
		{
			TechDef techDef = list[i];
			TechDefType techDefType = techDef.techDefType;
			LazyScrollableElement lazyScrollableElement2;
			if (techDefType == TechDefType.CharRep || techDefType == TechDefType.DisRep)
			{
				TechTreeCharReputationWidgetData techTreeCharReputationWidgetData = new TechTreeCharReputationWidgetData();
				techTreeCharReputationWidgetData.onTechClicked = OnTechClicked;
				techTreeCharReputationWidgetData.techDef = techDef;
				lazyScrollableElement2 = scrollRect.AddScrollableElement(techTreeCharReputationWidgetData);
				lazyScrollableElement2.RectTransform.sizeDelta = techElementSizeRep;
			}
			else
			{
				TechTreeElementWidgetData techTreeElementWidgetData = new TechTreeElementWidgetData();
				techTreeElementWidgetData.techDef = techDef;
				techTreeElementWidgetData.onTechClicked = OnTechClicked;
				lazyScrollableElement2 = scrollRect.AddScrollableElement(techTreeElementWidgetData);
				lazyScrollableElement2.RectTransform.sizeDelta = techElementSize;
			}
			lazyScrollableElement2.transform.SetSiblingIndex(i);
			if (techDef.TreePos.x < num)
			{
				lazyScrollableElement = lazyScrollableElement2;
				num = techDef.TreePos.x;
				num2 = techDef.TreePos.y;
			}
			else if (techDef.TreePos.y > num2 && techDef.TreePos.x <= num)
			{
				lazyScrollableElement = lazyScrollableElement2;
				num = techDef.TreePos.x;
				num2 = techDef.TreePos.y;
			}
		}
		UpdateRectContentSize();
		foreach (LazyScrollableElement displayingElement in scrollRect.DisplayingElements)
		{
			TechTreeElementBaseWidgetData obj = displayingElement.Data as TechTreeElementBaseWidgetData;
			TechDef techDef2 = obj.techDef;
			bool flag2 = techDef2.techDefType == TechDefType.Common;
			displayingElement.RectTransform.localPosition = new Vector2(techDef2.TreePos.x * technologyOffset.x + edgeOffset.x + (flag2 ? 0f : repWidgetOffset.x), Mathf.Ceil(techDef2.TreePos.y * technologyOffset.y + edgeOffset.y + (flag2 ? 0f : repWidgetOffset.y)));
			obj.rightConnectorPos = displayingElement.RectTransform.localPosition + (Vector3)(flag2 ? commonWidgetConnectorPortData.right : repWidgetConnectorPortData.right);
			obj.leftConnectorPos = displayingElement.RectTransform.localPosition + (Vector3)(flag2 ? commonWidgetConnectorPortData.left : repWidgetConnectorPortData.left);
			obj.downConnectorPos = displayingElement.RectTransform.localPosition + (Vector3)(flag2 ? commonWidgetConnectorPortData.down : repWidgetConnectorPortData.down);
			obj.upConnectorPos = displayingElement.RectTransform.localPosition + (Vector3)(flag2 ? commonWidgetConnectorPortData.up : repWidgetConnectorPortData.up);
		}
		((RectTransform)base.transform).RefreshContentFitter();
		for (int j = 0; j < scrollRect.DisplayingElements.Count; j++)
		{
			TechDef techDef3 = (scrollRect.DisplayingElements[j].Data as TechTreeElementBaseWidgetData).techDef;
			List<TechDef> childDefinitionList = techDef3.childDefinitionList;
			for (int k = 0; k < childDefinitionList.Count; k++)
			{
				TechDef techDef4 = childDefinitionList[k];
				if (techDef4.tab == currentTab && ShouldDisplayTech(techDef4))
				{
					TechTreeConnector elementFromPool = UIPrefabsPooler.Instance.GetElementFromPool<TechTreeConnector>(scrollRect.content);
					TechTreeConnector elementFromPool2 = UIPrefabsPooler.Instance.GetElementFromPool<TechTreeConnector>(scrollRect.content);
					techConnectors.Add(elementFromPool);
					techConnectors.Add(elementFromPool2);
					LazyScrollableElement lazyScrollableElement3 = FindTechByDefinition(techDef4);
					if (lazyScrollableElement3 == null)
					{
						Debug.LogError("Can't find child element:[" + techDef4.id + "] for:[" + techDef3.id + "]");
					}
					elementFromPool.Draw(scrollRect.DisplayingElements[j], lazyScrollableElement3, isBackground: false);
					elementFromPool2.Draw(scrollRect.DisplayingElements[j], lazyScrollableElement3, isBackground: true);
					elementFromPool.transform.SetParent(elementFromPool.IsConnectorActive ? activeConnectorsContent.transform : inactiveConnectorsContent.transform);
					elementFromPool.transform.SetAsFirstSibling();
					elementFromPool2.transform.SetParent(backgroundConnectorsContent.transform);
					elementFromPool2.transform.SetAsFirstSibling();
				}
			}
		}
		scrollRect.CheckVisibility();
		if (LazyInput.IsGamepadActive)
		{
			autoScroll.SkipNextAutoscroll = true;
			gamepadNavigationController.ReinitItems(focusOnFirstActive: false);
		}
		LazyScrollableElement lazyScrollableElement4 = null;
		TechDef techDef5 = (string.IsNullOrEmpty(focusOnTech) ? null : GameBalance.Me.GetData<TechDef>(focusOnTech));
		if (techDef5 != null && ShouldDisplayTech(techDef5))
		{
			lazyScrollableElement4 = FindTechByDefinition(techDef5);
		}
		if (lazyScrollableElement4 != null)
		{
			scrollRect.DOKill();
			gamepadNavigationController.SetFocusedItem(lazyScrollableElement4.GamepadNavigationItem.GetComponent<GamepadNavigationItem>());
			scrollRect.DOKill();
			scrollRect.ScrollToTargetInstant(lazyScrollableElement4.RectTransform, RectTransform.Axis.Horizontal);
			return;
		}
		if (lazyScrollableElement != null)
		{
			gamepadNavigationController.SetFocusedItem(lazyScrollableElement.GamepadNavigationItem.GetComponent<GamepadNavigationItem>());
		}
		else
		{
			gamepadNavigationController.FocusOnFirstActive();
		}
		scrollRect.DOKill();
		scrollRect.horizontalNormalizedPosition = 0f;
	}

	private static bool ShouldDisplayTech(TechDef techDef)
	{
		return true;
	}

	private static void UnlockReputationTechIfAvailable(TechDef definition, bool shouldDisplayTech)
	{
		if (shouldDisplayTech && IsTechAvailableInCurrentBuild(definition) && (definition.techDefType == TechDefType.CharRep || definition.techDefType == TechDefType.DisRep) && definition.TechState == TechState.Available && definition.EnoughResources)
		{
			definition.Unlock();
		}
	}

	private static bool IsTechAvailableInCurrentBuild(TechDef techDef)
	{
		return techDef.isAvailableInDemo;
	}

	private static void UnlockAvailableReputationTechs()
	{
		foreach (TechDef techDef in GameBalance.Me.techDefs)
		{
			UnlockReputationTechIfAvailable(techDef, ShouldDisplayTech(techDef));
		}
	}

	private void HideConnectors()
	{
		foreach (TechTreeConnector techConnector in techConnectors)
		{
			UIPrefabsPooler.Instance.ReleaseElementToPool(techConnector);
		}
		techConnectors.Clear();
	}

	private LazyScrollableElement FindTechByDefinition(TechDef techDef)
	{
		foreach (LazyScrollableElement displayingElement in scrollRect.DisplayingElements)
		{
			if ((displayingElement.Data as TechTreeElementBaseWidgetData).techDef == techDef)
			{
				return displayingElement;
			}
		}
		Debug.Log("Cannot find element for definition " + techDef.id);
		return null;
	}

	private TechTreeElementBaseWidget GetTechWidget(LazyScrollableElement parent)
	{
		TechDefType techDefType = (parent.Data as TechTreeElementBaseWidgetData).techDef.techDefType;
		if ((uint)(techDefType - 1) <= 1u)
		{
			TechTreeCharReputationWidget elementFromPool = UIPrefabsPooler.Instance.GetElementFromPool<TechTreeCharReputationWidget>(parent.RectTransform);
			elementFromPool.Init();
			elementFromPool.Draw(parent.Data);
			parent.GamepadNavigationItem.SetCallbacks(elementFromPool.button.ForceOnEnter, elementFromPool.button.ForceOnExit, elementFromPool.button.ForceOnClick);
			parent.transform.SetParent(techLayerFront.transform);
			return elementFromPool;
		}
		TechTreeElementWidget elementFromPool2 = UIPrefabsPooler.Instance.GetElementFromPool<TechTreeElementWidget>(parent.RectTransform);
		elementFromPool2.Init();
		elementFromPool2.Draw(parent.Data);
		parent.GamepadNavigationItem.SetCallbacks(elementFromPool2.button.ForceOnEnter, elementFromPool2.button.ForceOnExit, elementFromPool2.button.ForceOnClick);
		parent.transform.SetParent(techLayerFront.transform);
		elementFromPool2.Background.transform.SetParent(techLayerBack.transform);
		elementFromPool2.Background.transform.position = parent.transform.position;
		return elementFromPool2;
	}

	private void ReleaseWidgetForParent(LazyScrollableElement parent)
	{
		if (parent.Widget is TechTreeCharReputationWidget reputationWidget)
		{
			ReleaseReputationWidget(reputationWidget);
		}
		else
		{
			ReleaseCommonWidget(parent.Widget as TechTreeElementWidget);
		}
	}

	private void ReleaseReputationWidget(TechTreeCharReputationWidget reputationWidget)
	{
		reputationWidget.DeInit();
		reputationWidget.Hide();
		UIPrefabsPooler.Instance.ReleaseElementToPool(reputationWidget);
	}

	private void ReleaseCommonWidget(TechTreeElementWidget techTreeElementWidget)
	{
		techTreeElementWidget.DeInit();
		techTreeElementWidget.Hide();
		techTreeElementWidget.Background.transform.SetParent(techTreeElementWidget.transform.GetChild(0).transform);
		UIPrefabsPooler.Instance.ReleaseElementToPool(techTreeElementWidget);
	}

	private void UpdateRectContentSize()
	{
		if (scrollRect.DisplayingElements.Count == 0)
		{
			return;
		}
		float num = 0f;
		float num2 = 0f;
		foreach (LazyScrollableElement displayingElement in scrollRect.DisplayingElements)
		{
			TechDef techDef = (displayingElement.Data as TechTreeElementBaseWidgetData).techDef;
			if (techDef == null)
			{
				Debug.Log($"data is null?:[{displayingElement.Data == null}]");
			}
			if (techDef.TreePos.x > num)
			{
				num = techDef.TreePos.x;
			}
			if (techDef.TreePos.y > num2)
			{
				num2 = techDef.TreePos.y;
			}
		}
		scrollRect.content.sizeDelta = new Vector2(num * technologyOffset.x + edgeOffset.x * 2f + edgeOffsetContentAdditional + techElementSize.x, scrollRect.content.sizeDelta.y);
	}

	private void HideCurrentElements()
	{
		scrollRect.ClearDisplayingScrollableElements();
	}

	private void OnTechTreeTabButtonClicked(TechTreeTabButton techTreeTabButton)
	{
		if (currentTab != techTreeTabButton.TechTreeTab)
		{
			DisplayTab(techTreeTabButton.TechTreeTab);
		}
	}

	public bool OnPressedPrevTechTab()
	{
		int num = (int)currentTab;
		do
		{
			num--;
			if (num < 0)
			{
				num = tabButtons.Count - 1;
			}
		}
		while (MainGame.Instance.GameSave.knowledgeSystem.IsTechTabLocked((TechTreeTab)num));
		DisplayTab((TechTreeTab)num);
		return true;
	}

	public bool OnPressedNextTechTab()
	{
		int num = (int)currentTab;
		do
		{
			num++;
			if (num > tabButtons.Count - 1)
			{
				num = 0;
			}
		}
		while (MainGame.Instance.GameSave.knowledgeSystem.IsTechTabLocked((TechTreeTab)num));
		DisplayTab((TechTreeTab)num);
		return true;
	}

	public void UpdateElements()
	{
		foreach (LazyScrollableElement displayingElement in scrollRect.DisplayingElements)
		{
			if (displayingElement.Widget != null)
			{
				displayingElement.Widget.Redraw();
			}
		}
	}

	private void UpdateConnectors()
	{
		foreach (TechTreeConnector techConnector in techConnectors)
		{
			techConnector.UpdateState();
		}
	}

	private void OnTechClicked(TechTreeElementBaseWidgetData techData)
	{
		TechDef techDef = techData.techDef;
		if (!MainGame.Instance.GameSave.knowledgeSystem.IsTechUnlocked(techDef.id))
		{
			if (techData.techDef.techDefType == TechDefType.Common)
			{
				UITechTreeElementWindow window = LazyUI.GetWindow<UITechTreeElementWindow>();
				UIDialogWindowData.ButtonData item = new UIDialogWindowData.ButtonData(Unlock, LLBase.L("btn_unlock"), () => techDef.TechState == TechState.Available && techDef.EnoughResources, replaceForGamepad: true, GameKey.Select);
				UIDialogWindowData.ButtonData item2 = new UIDialogWindowData.ButtonData(LazyUI.GetWindow<UITechTreeElementWindow>().Close, LLBase.L("btn_cancel"), null, replaceForGamepad: true, GameKey.Back);
				window.Open(new UITechTreeElementWindowData(techDef, new List<UIDialogWindowData.ButtonData> { item, item2 }, techDef.ParentsUnlocked ? string.Empty : LLBase.L("tech_not_all_techs_unlocked")));
			}
		}
		else if (techData.techDef.techDefType == TechDefType.Common)
		{
			UITechTreeElementWindow window2 = LazyUI.GetWindow<UITechTreeElementWindow>();
			UIDialogWindowData.ButtonData item3 = new UIDialogWindowData.ButtonData(LazyUI.GetWindow<UITechTreeElementWindow>().Close, LLBase.L("btn_ok"), null, replaceForGamepad: true, GameKey.Select);
			window2.Open(new UITechTreeElementWindowData(techDef, new List<UIDialogWindowData.ButtonData> { item3 }));
		}
		void Unlock()
		{
			techDef.Unlock();
			UnlockAvailableReputationTechs();
			RedrawSpheres();
			UpdateElements();
			UpdateConnectors();
			LazyUI.GetWindow<UITechTreeElementWindow>().Close();
			LazyAudio.PlayAndForget("unlock");
		}
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
		}
		if (prevSubTabGamepadHelper != null)
		{
			prevSubTabGamepadHelper.gameObject.SetActive(isGamepadActive);
			if (isGamepadActive)
			{
				prevSubTabGamepadHelper.text = ControllerIconLibrary.GetIconId(GameKey.PrevSubTab);
			}
		}
	}

	public override List<LazyGameKeyTip> GetTips(GamepadNavigationItem gamepadNavigationItem)
	{
		List<LazyGameKeyTip> list = new List<LazyGameKeyTip>();
		if (gamepadNavigationItem != null)
		{
			list.Add(LazyGameKeyTip.Select());
		}
		return list;
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		Draw(new TechTreePageWidgetData());
	}
}
