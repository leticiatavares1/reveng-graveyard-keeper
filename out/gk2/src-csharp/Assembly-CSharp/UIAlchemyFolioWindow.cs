using System;
using System.Collections.Generic;
using System.Linq;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class UIAlchemyFolioWindow : LazyWindow<UIAlchemyFolioWindowData>
{
	[Space]
	[SerializeField]
	private LazyScrollRect scrollRect;

	[Space]
	[SerializeField]
	private List<UIFolioWindowTab> tabs;

	[Space]
	[SerializeField]
	private GameObject emptyObject;

	[Space]
	[SerializeField]
	private GameObject tabsParent;

	private AlchemyFormulaTab currentTab;

	private static AlchemyFormulaTab? lastOpenedTab;

	public override void Init()
	{
		base.Init();
		foreach (UIFolioWindowTab tab in tabs)
		{
			tab.Init(OnTabButtonClicked);
			tab.gameObject.SetActive(value: true);
		}
		scrollRect.Init(GetFormulaWidget, ReleaseWidgetForParent, base.GamepadNavigationController);
	}

	public override void Open(UIAlchemyFolioWindowData data)
	{
		base.Open(data);
		bool flag = data.Data.Count > 0;
		emptyObject.SetActive(!flag);
		tabsParent.SetActive(flag);
		HideCurrentElements();
		foreach (UIFolioWindowTab tab in tabs)
		{
			tab.gameObject.SetActive(IsTabUnlocked(tab.Tab));
		}
		if (flag)
		{
			DisplayTab(GetInitialTab());
		}
	}

	public void DisplayTab(AlchemyFormulaTab tab)
	{
		currentTab = tab;
		lastOpenedTab = tab;
		foreach (UIFolioWindowTab tab2 in tabs)
		{
			tab2.SetState(tab2.Tab == currentTab);
		}
		HideCurrentElements();
		for (int i = 0; i < data.Data[tab].Count; i++)
		{
			AlchemyFormulaDef alchemyFormulaDef = data.Data[tab][i];
			UIAlchemyFormulaWidgetData uIAlchemyFormulaWidgetData = new UIAlchemyFormulaWidgetData();
			uIAlchemyFormulaWidgetData.AlchemyFormulaDef = alchemyFormulaDef;
			scrollRect.AddScrollableElement(uIAlchemyFormulaWidgetData).transform.SetSiblingIndex(i);
		}
		((RectTransform)base.transform).RefreshContentFitter();
		scrollRect.CheckVisibility();
		if (LazyInput.IsGamepadActive)
		{
			base.GamepadNavigationController.ReinitItems(focusOnFirstActive: true);
		}
	}

	private void HideCurrentElements()
	{
		scrollRect.ClearDisplayingScrollableElements();
	}

	protected override void PrintTips()
	{
		lazyButtonTips.Print(LazyGameKeyTip.Back());
	}

	private void OnTabButtonClicked(UIFolioWindowTab tabButton)
	{
		if (currentTab != tabButton.Tab)
		{
			DisplayTab(tabButton.Tab);
		}
	}

	private UIAlchemyFormulaWidget GetFormulaWidget(LazyScrollableElement parent)
	{
		UIAlchemyFormulaWidget elementFromPool = UIPrefabsPooler.Instance.GetElementFromPool<UIAlchemyFormulaWidget>(parent.RectTransform);
		elementFromPool.Init();
		elementFromPool.Draw(parent.Data);
		return elementFromPool;
	}

	private void ReleaseWidgetForParent(LazyScrollableElement parent)
	{
		ReleaseCommonWidget(parent.Widget as UIAlchemyFormulaWidget);
	}

	private void ReleaseCommonWidget(UIAlchemyFormulaWidget formulaWidget)
	{
		formulaWidget.DeInit();
		formulaWidget.Hide();
		UIPrefabsPooler.Instance.ReleaseElementToPool(formulaWidget);
	}

	private bool OnPressedPrevTab()
	{
		if (data.Data.Count <= 0)
		{
			return false;
		}
		int num = (int)currentTab;
		do
		{
			num--;
			if (num < 0)
			{
				num = tabs.Count - 1;
			}
		}
		while (!IsTabUnlocked((AlchemyFormulaTab)num));
		DisplayTab((AlchemyFormulaTab)num);
		return true;
	}

	private bool OnPressedNextTab()
	{
		if (data.Data.Count <= 0)
		{
			return false;
		}
		int num = (int)currentTab;
		do
		{
			num++;
			if (num > tabs.Count - 1)
			{
				num = 0;
			}
		}
		while (!IsTabUnlocked((AlchemyFormulaTab)num));
		DisplayTab((AlchemyFormulaTab)num);
		return true;
	}

	private AlchemyFormulaTab GetInitialTab()
	{
		if (lastOpenedTab.HasValue && IsTabUnlocked(lastOpenedTab.Value))
		{
			return lastOpenedTab.Value;
		}
		return data.Data.Keys.First();
	}

	private bool IsTabUnlocked(AlchemyFormulaTab tab)
	{
		return data.Data.ContainsKey(tab);
	}

	protected override Dictionary<GameKey, Func<bool>> GetGameKeyDelegates()
	{
		Dictionary<GameKey, Func<bool>> gameKeyDelegates = base.GetGameKeyDelegates();
		gameKeyDelegates.Add(GameKey.NextTab, OnPressedNextTab);
		gameKeyDelegates.Add(GameKey.PrevTab, OnPressedPrevTab);
		return gameKeyDelegates;
	}

	protected override void TestDraw()
	{
	}
}
