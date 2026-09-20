using System;
using System.Collections.Generic;
using LazyBearTechnology;
using LinqTools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterWindow : LazyWindow<CharacterWindowData>, IUIWindowCustomOperable
{
	[Space]
	[SerializeField]
	private CharMainPageWidget mainPageWidget;

	[SerializeField]
	private CharInspirationPageWidget inspirationPageWidget;

	[SerializeField]
	private TechTreePageWidget techTreePageWidget;

	[SerializeField]
	private QuestTreePageWidget questTreePageWidget;

	[SerializeField]
	private MapPageWidget mapPageWidget;

	[SerializeField]
	private TextMeshProUGUI nextTabGamepadHelper;

	[SerializeField]
	private TextMeshProUGUI prevTabGamepadHelper;

	private Dictionary<CharacterWindowData.CharPage, LazyWidgetBase> pages = new Dictionary<CharacterWindowData.CharPage, LazyWidgetBase>();

	private Dictionary<CharacterWindowData.CharPage, CharPageTabButton> pagesButtons = new Dictionary<CharacterWindowData.CharPage, CharPageTabButton>();

	[SerializeField]
	private GameObject charPageTabButtonParent;

	[SerializeField]
	private CharPageTabButton charPageTabButtonPrefab;

	[SerializeField]
	private float defaultGamepadNavigationMaxDistanceBetweenElements = 200f;

	[SerializeField]
	private float inspirationGamepadNavigationMaxDistanceBetweenElements = 55f;

	public GamepadNavigationController NavigationController => base.GamepadNavigationController;

	public TechTreePageWidget TechTreePageWidget => techTreePageWidget;

	public CharacterWindowData.CharPage LastOpenedPage
	{
		get
		{
			if (data != null)
			{
				return data.PlayerData.lastOpenedPage;
			}
			return CharacterWindowData.CharPage.Undefined;
		}
		set
		{
			if (data != null)
			{
				data.PlayerData.lastOpenedPage = value;
			}
		}
	}

	public static event Action<CharacterWindowData.CharPage> OnTabChanged;

	public void RefreshCharPageTabButtonParentContentFitter()
	{
		((RectTransform)charPageTabButtonParent.transform).RefreshContentFitter();
	}

	public void RefreshTechTreePageWidgetContentFitter()
	{
		try
		{
			((RectTransform)techTreePageWidget.transform).RefreshContentFitter();
		}
		catch (Exception)
		{
		}
	}

	public override void Init()
	{
		base.Init();
		pages.Add(CharacterWindowData.CharPage.Main, mainPageWidget);
		pages.Add(CharacterWindowData.CharPage.TechTree, techTreePageWidget);
		pages.Add(CharacterWindowData.CharPage.Inspiration, inspirationPageWidget);
		pages.Add(CharacterWindowData.CharPage.QuestTree, questTreePageWidget);
		pages.Add(CharacterWindowData.CharPage.Map, mapPageWidget);
		charPageTabButtonPrefab.gameObject.SetActive(value: false);
		int count = pages.Count;
		int num = 0;
		foreach (KeyValuePair<CharacterWindowData.CharPage, LazyWidgetBase> page in pages)
		{
			bool isLastTab = num == count - 1;
			string text = page.Value.gameObject.name;
			string text2 = text + "_Button";
			CharPageTabButton charPageTabButton = charPageTabButtonPrefab.Copy(charPageTabButtonParent.transform, activate: true, text2);
			charPageTabButton.Init(text, delegate
			{
				SwitchPage(page.Key, checkCurrent: true);
			}, isLastTab);
			pagesButtons.Add(page.Key, charPageTabButton);
			num++;
		}
		LastOpenedPage = CharacterWindowData.CharPage.Undefined;
	}

	protected override void SetData(CharacterWindowData data)
	{
		base.SetData(data);
		data.SubscribeEvents();
		data.onPageStatusChanged = UpdatePageHasActionStatus;
		data.CharMainPageWidgetData.OnBagShow = OnBagShown;
		data.CharMainPageWidgetData.OnBagHide = OnBagHide;
	}

	public override void Redraw()
	{
		SwitchPage(data.Page);
		pagesButtons[CharacterWindowData.CharPage.TechTree].UpdateActionIndicatorStatus(value: false);
		pagesButtons[CharacterWindowData.CharPage.QuestTree].UpdateActionIndicatorStatus(value: false);
		pagesButtons[CharacterWindowData.CharPage.Inspiration].UpdateActionIndicatorStatus(data.HasAvailableActionOnInspirationPage);
		pagesButtons[CharacterWindowData.CharPage.Main].UpdateActionIndicatorStatus(value: false);
		pagesButtons[CharacterWindowData.CharPage.Map].UpdateActionIndicatorStatus(value: false);
		for (int i = 0; i < pagesButtons.Keys.Count; i++)
		{
			CharacterWindowData.CharPage charPage = pagesButtons.Keys.ElementAt(i);
			pagesButtons[charPage].gameObject.SetActive(!MainGame.Instance.GameSave.knowledgeSystem.IsCharTabLocked(charPage));
		}
	}

	public override void Close()
	{
		CloseContextMenuIfOpen();
		base.Close();
	}

	public override void Hide()
	{
		base.Hide();
		if (data != null)
		{
			data.UnsubscribeEvents();
			data.InspirationPageWidgetData.UnsubscribeEvents();
		}
		if (pages.ContainsKey(LastOpenedPage))
		{
			pages[LastOpenedPage].Hide();
		}
	}

	private static void CloseContextMenuIfOpen()
	{
		UIContextMenuWindow window = LazyUI.GetWindow<UIContextMenuWindow>();
		if (window != null && window.IsShown)
		{
			window.Close();
		}
	}

	public void SetInspirationPageWithSpecificTalent(string talentId)
	{
		data.UpdateTalentInInspirationWidgetData(talentId);
		SwitchPage(CharacterWindowData.CharPage.Inspiration);
	}

	public void RefreshInspirationPageActionIndicatorStatus()
	{
		data.UpdateHasAvailableActionOnInspirationPageStatus();
	}

	private void SwitchPage(CharacterWindowData.CharPage page, bool checkCurrent = false)
	{
		if (checkCurrent && page == LastOpenedPage)
		{
			return;
		}
		if (pages.ContainsKey(LastOpenedPage))
		{
			pages[LastOpenedPage].Hide();
		}
		foreach (KeyValuePair<CharacterWindowData.CharPage, CharPageTabButton> pagesButton in pagesButtons)
		{
			CharacterWindowData.CharPage key = pagesButton.Key;
			pagesButton.Value.UpdateState(key == page);
		}
		switch (page)
		{
		case CharacterWindowData.CharPage.Main:
			mainPageWidget.Draw(data.CharMainPageWidgetData);
			try
			{
				((RectTransform)mainPageWidget.transform).RefreshContentFitter();
			}
			catch (Exception)
			{
			}
			break;
		case CharacterWindowData.CharPage.Inspiration:
			inspirationPageWidget.Draw(data.InspirationPageWidgetData);
			data.InspirationPageWidgetData.SubscribeEvents();
			try
			{
				((RectTransform)inspirationPageWidget.transform).RefreshContentFitter();
			}
			catch (Exception)
			{
			}
			break;
		case CharacterWindowData.CharPage.TechTree:
			techTreePageWidget.Draw(null);
			techTreePageWidget.DisplayLastTab();
			try
			{
				((RectTransform)techTreePageWidget.transform).RefreshContentFitter();
			}
			catch (Exception)
			{
			}
			break;
		case CharacterWindowData.CharPage.QuestTree:
			questTreePageWidget.Draw(null);
			questTreePageWidget.Display();
			try
			{
				((RectTransform)questTreePageWidget.transform).RefreshContentFitter();
			}
			catch (Exception)
			{
			}
			break;
		case CharacterWindowData.CharPage.Map:
			mapPageWidget.Draw(data.MapPageWidgetData);
			break;
		default:
			throw new ArgumentOutOfRangeException("page", page, null);
		}
		UpdateGamepadNavigationMaxDistanceBetweenElements(page);
		((RectTransform)charPageTabButtonParent.transform).RefreshContentFitter();
		LastOpenedPage = page;
		if (page == CharacterWindowData.CharPage.Inspiration)
		{
			data.UpdateHasAvailableActionOnInspirationPageStatus();
		}
		if (LazyInput.IsGamepadActive && page != CharacterWindowData.CharPage.TechTree)
		{
			base.GamepadNavigationController.ReinitItems(focusOnFirstActive: true);
		}
		CharacterWindow.OnTabChanged?.Invoke(page);
		if (page == CharacterWindowData.CharPage.Inspiration)
		{
			TryShowInspirationTutorial();
		}
	}

	private void UpdateGamepadNavigationMaxDistanceBetweenElements(CharacterWindowData.CharPage page)
	{
		base.GamepadNavigationController.maxDistanceBetweenElements = ((page == CharacterWindowData.CharPage.Inspiration) ? inspirationGamepadNavigationMaxDistanceBetweenElements : defaultGamepadNavigationMaxDistanceBetweenElements);
	}

	private bool TrySwitchPageToMain()
	{
		if (LastOpenedPage == CharacterWindowData.CharPage.Main)
		{
			return OnPressedBack();
		}
		SwitchPage(CharacterWindowData.CharPage.Main);
		return true;
	}

	private bool TrySwitchPageToTechTree()
	{
		if (LastOpenedPage == CharacterWindowData.CharPage.TechTree)
		{
			return OnPressedBack();
		}
		SwitchPage(CharacterWindowData.CharPage.TechTree);
		return true;
	}

	private bool TrySwitchPageToMap()
	{
		if (LastOpenedPage == CharacterWindowData.CharPage.Map)
		{
			return OnPressedBack();
		}
		SwitchPage(CharacterWindowData.CharPage.Map);
		return true;
	}

	private bool TrySwitchPageToQuestTree()
	{
		if (LastOpenedPage == CharacterWindowData.CharPage.QuestTree)
		{
			return OnPressedBack();
		}
		SwitchPage(CharacterWindowData.CharPage.QuestTree);
		return true;
	}

	private bool TrySwitchPageToInspirations()
	{
		if (LastOpenedPage == CharacterWindowData.CharPage.Inspiration)
		{
			return OnPressedBack();
		}
		SwitchPage(CharacterWindowData.CharPage.Inspiration);
		return true;
	}

	private void TryShowInspirationTutorial()
	{
		if (!MainGame.PlayerData.sawInspirationTutorialOnce && MainGame.Instance.GameSave.talentSystemData.HasTwoZeroFaithInspirationsToBuyInSameBranch())
		{
			MainGame.PlayerData.sawInspirationTutorialOnce = true;
			LazyUI.GetWindow<UITutorialWindow>().Open(new UITutorialWindowData("tut_insp_talents_hdr"));
		}
	}

	public bool OnPressedPrevTab()
	{
		int num = (int)LastOpenedPage;
		CharacterWindowData.CharPage page;
		do
		{
			num--;
			if (num < 1)
			{
				num = pages.Count;
			}
			page = (CharacterWindowData.CharPage)num;
		}
		while (MainGame.Instance.GameSave.knowledgeSystem.IsCharTabLocked(page));
		if (LazyInput.IsGamepadActive)
		{
			LazyAudio.PlayAndForget("tab_click");
		}
		SwitchPage(page);
		return true;
	}

	public bool OnPressedNextTab()
	{
		int num = (int)LastOpenedPage;
		CharacterWindowData.CharPage page;
		do
		{
			num++;
			if (num > pages.Count)
			{
				num = 1;
			}
			page = (CharacterWindowData.CharPage)num;
		}
		while (MainGame.Instance.GameSave.knowledgeSystem.IsCharTabLocked(page));
		if (LazyInput.IsGamepadActive)
		{
			LazyAudio.PlayAndForget("tab_click");
		}
		SwitchPage(page);
		return true;
	}

	public bool OnPressedPrevSubTab()
	{
		bool flag = false;
		switch (LastOpenedPage)
		{
		case CharacterWindowData.CharPage.TechTree:
			flag = techTreePageWidget.OnPressedPrevTechTab();
			break;
		case CharacterWindowData.CharPage.Inspiration:
			flag = inspirationPageWidget.OnPressedPrevTechTab(base.GamepadNavigationController);
			break;
		}
		if (flag)
		{
			LazyAudio.PlayAndForget("tab_click");
		}
		return flag;
	}

	public bool OnPressedNextSubTab()
	{
		bool flag = false;
		switch (LastOpenedPage)
		{
		case CharacterWindowData.CharPage.TechTree:
			flag = techTreePageWidget.OnPressedNextTechTab();
			break;
		case CharacterWindowData.CharPage.Inspiration:
			flag = inspirationPageWidget.OnPressedNextTechTab(base.GamepadNavigationController);
			break;
		}
		if (flag)
		{
			LazyAudio.PlayAndForget("tab_click");
		}
		return flag;
	}

	private bool OnItemPressed2()
	{
		if (!base.IsShownAndTop)
		{
			return false;
		}
		if (LazyInput.IsGamepadActive)
		{
			GamepadNavigationItem focusedItem = base.GamepadNavigationController.FocusedItem;
			if (focusedItem != null && focusedItem.TryGetComponent<UIItemCell>(out var component) && component.DisplayingItem != null && !component.DisplayingItem.IsEmpty)
			{
				component.OnPress2();
				return true;
			}
		}
		return false;
	}

	protected override bool OnPressedBack()
	{
		if (!base.IsShownAndTop)
		{
			return false;
		}
		if (data != null && LastOpenedPage == CharacterWindowData.CharPage.Main && data.CharMainPageWidgetData != null && data.CharMainPageWidgetData.IsBagShown)
		{
			data.CharMainPageWidgetData.OnHideBagPressed?.Invoke();
			return true;
		}
		if ((bool)closeButton)
		{
			Close();
			return true;
		}
		return false;
	}

	private void UpdatePageHasActionStatus(CharacterWindowData.CharPage pageType, bool value)
	{
		pagesButtons[pageType].UpdateActionIndicatorStatus(value);
	}

	private void OnBagShown(Item bag)
	{
		GamepadNavigationItem focusedItem = base.GamepadNavigationController.FocusedItem;
		mainPageWidget.OnBagShown(bag);
		if (LazyInput.IsGamepadActive)
		{
			base.GamepadNavigationController.ReinitItems(focusOnFirstActive: false);
			base.GamepadNavigationController.SetFocusedItem(focusedItem);
		}
	}

	private void OnBagHide(Item bag)
	{
		GamepadNavigationItem focusedItem = base.GamepadNavigationController.FocusedItem;
		mainPageWidget.OnBagHide(bag);
		if (LazyInput.IsGamepadActive)
		{
			if (base.GamepadNavigationController.FocusedItem != null && base.GamepadNavigationController.FocusedItem.gameObject.activeSelf)
			{
				base.GamepadNavigationController.ReinitItems(focusOnFirstActive: false);
				base.GamepadNavigationController.SetFocusedItem(focusedItem);
			}
			else
			{
				base.GamepadNavigationController.ReinitItems(focusOnFirstActive: true);
			}
		}
	}

	protected override Dictionary<GameKey, Func<bool>> GetGameKeyDelegates()
	{
		Dictionary<GameKey, Func<bool>> gameKeyDelegates = base.GetGameKeyDelegates();
		gameKeyDelegates.Add(GameKey.CharacterWindow, OnCharacterButtonPressed);
		gameKeyDelegates.Add(GameKey.ItemMove, OnItemPressed2);
		gameKeyDelegates.Add(GameKey.NextTab, OnPressedNextTab);
		gameKeyDelegates.Add(GameKey.PrevTab, OnPressedPrevTab);
		gameKeyDelegates.Add(GameKey.NextSubTab, OnPressedNextSubTab);
		gameKeyDelegates.Add(GameKey.PrevSubTab, OnPressedPrevSubTab);
		gameKeyDelegates.Add(GameKey.Inventory, TrySwitchPageToMain);
		gameKeyDelegates.Add(GameKey.TechTree, TrySwitchPageToTechTree);
		gameKeyDelegates.Add(GameKey.Map, TrySwitchPageToMap);
		gameKeyDelegates.Add(GameKey.QuestTree, TrySwitchPageToQuestTree);
		gameKeyDelegates.Add(GameKey.Inspirations, TrySwitchPageToInspirations);
		gameKeyDelegates.Add(GameKey.MoveAllItemsFromPlayer, delegate
		{
			if (LastOpenedPage == CharacterWindowData.CharPage.Main)
			{
				if (mainPageWidget.IsBagModeEnabled())
				{
					mainPageWidget.OnAllToChestPressed();
				}
				else if (LazyInput.IsGamepadActive)
				{
					OnPressedBack();
				}
			}
			return true;
		});
		return gameKeyDelegates;
	}

	private bool OnCharacterButtonPressed()
	{
		if (LazyInput.IsGamepadActive)
		{
			return false;
		}
		return OnPressedBack();
	}

	protected override void UpdateGamepadDependentStuff()
	{
		base.UpdateGamepadDependentStuff();
		if (LastOpenedPage == CharacterWindowData.CharPage.TechTree)
		{
			techTreePageWidget?.UpdateGamepadDependentStuff();
		}
		if (LastOpenedPage == CharacterWindowData.CharPage.Map)
		{
			mapPageWidget?.UpdateGamepadDependentStuff();
		}
		if (LastOpenedPage == CharacterWindowData.CharPage.Inspiration)
		{
			inspirationPageWidget?.UpdateGamepadDependentStuff();
		}
		bool isGamepadActive = LazyInput.IsGamepadActive;
		if (nextTabGamepadHelper != null)
		{
			nextTabGamepadHelper.gameObject.SetActive(isGamepadActive);
			if (isGamepadActive)
			{
				nextTabGamepadHelper.text = ControllerIconLibrary.GetIconId(GameKey.NextTab);
			}
			nextTabGamepadHelper.transform.SetAsLastSibling();
		}
		if (prevTabGamepadHelper != null)
		{
			prevTabGamepadHelper.gameObject.SetActive(isGamepadActive);
			if (isGamepadActive)
			{
				prevTabGamepadHelper.text = ControllerIconLibrary.GetIconId(GameKey.PrevTab);
			}
			prevTabGamepadHelper.transform.SetAsLastSibling();
		}
	}

	public void RefreshGamepadTips()
	{
		if (LazyInput.IsGamepadActive)
		{
			PrintTips();
		}
	}

	protected override void PrintTips()
	{
		GamepadNavigationItem gamepadNavigationItem = null;
		if (base.GamepadNavigationController != null)
		{
			gamepadNavigationItem = base.GamepadNavigationController.FocusedItem;
		}
		PrintTips(gamepadNavigationItem);
	}

	protected override void PrintTips(GamepadNavigationItem gamepadNavigationItem)
	{
		List<LazyGameKeyTip> list = new List<LazyGameKeyTip>();
		CharacterWindowData.CharPage key = ((LastOpenedPage == CharacterWindowData.CharPage.Undefined) ? CharacterWindowData.CharPage.Main : LastOpenedPage);
		if (pages.TryGetValue(key, out var value) && value != null)
		{
			List<LazyGameKeyTip> tips = value.GetTips(gamepadNavigationItem);
			if (tips != null)
			{
				list.AddRange(tips);
			}
		}
		if ((bool)closeButton)
		{
			list.Add(LazyGameKeyTip.Back());
		}
		lazyButtonTips?.Print(list);
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		LazyUI.GetWindow<CharacterWindow>().Open(new CharacterWindowData(MainGame.Instance.GameSave));
	}

	[LazyUITest]
	protected void TestDraw_Inspiration()
	{
		LazyUI.GetWindow<CharacterWindow>().Open(new CharacterWindowData(MainGame.Instance.GameSave));
		SwitchPage(CharacterWindowData.CharPage.Inspiration);
	}

	[LazyUITest]
	protected void TestDraw_TechTree()
	{
		CharacterWindow window = LazyUI.GetWindow<CharacterWindow>();
		window.Open(new CharacterWindowData(MainGame.Instance.GameSave));
		window.SwitchPage(CharacterWindowData.CharPage.TechTree);
	}

	[LazyUITest]
	protected void TestDraw_Map()
	{
		CharacterWindow window = LazyUI.GetWindow<CharacterWindow>();
		window.Open(new CharacterWindowData(MainGame.Instance.GameSave));
		window.SwitchPage(CharacterWindowData.CharPage.Map);
	}

	[LazyUITest]
	protected void TestDraw_QuestTree()
	{
		CharacterWindow window = LazyUI.GetWindow<CharacterWindow>();
		MainGame.Instance.GameSave.questSystemData.questCollection.questsCache["17_village_nun_meet"].isHidden = false;
		MainGame.Instance.GameSave.questSystemData.questCollection.questsCache["19_base_ceremony_church"].isHidden = false;
		MainGame.Instance.GameSave.questSystemData.questCollection.questsCache["17_village_nun_meet"].status = QuestStatus.InProgress;
		MainGame.Instance.GameSave.questSystemData.questCollection.questsCache["19_base_ceremony_church"].status = QuestStatus.InProgress;
		MainGame.Instance.GameSave.questSystemData.questCollection.questsCache["16_foreman_letter_wake"].isHidden = false;
		MainGame.Instance.GameSave.questSystemData.questCollection.questsCache["18_base_doctor_encounter"].isHidden = false;
		MainGame.Instance.GameSave.questSystemData.questCollection.questsCache["16_foreman_letter_wake"].status = QuestStatus.InProgress;
		MainGame.Instance.GameSave.questSystemData.questCollection.questsCache["18_base_doctor_encounter"].status = QuestStatus.InProgress;
		window.Open(new CharacterWindowData(MainGame.Instance.GameSave));
		SwitchPage(CharacterWindowData.CharPage.QuestTree);
	}

	bool IUIWindowCustomOperable.get_IsShown()
	{
		return base.IsShown;
	}
}
