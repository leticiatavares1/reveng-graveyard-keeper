using System;
using System.Collections.Generic;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITutorialWindow : LazyWindow<UITutorialWindowData>
{
	[SerializeField]
	private TextMeshProUGUI headerLabel;

	[SerializeField]
	private UIDialogWindowButton lazyButton;

	[SerializeField]
	private LazyButton nextButton;

	[SerializeField]
	private LazyButton prevButton;

	[SerializeField]
	private List<GameObject> pages = new List<GameObject>();

	[SerializeField]
	private GameObject arrowsObj;

	[SerializeField]
	private VerticalLayoutGroup verticalLayoutGroup;

	[SerializeField]
	private RectOffset paddingWithArrows;

	[SerializeField]
	private RectOffset paddingWithoutArrows;

	private List<GameObject> currentTutorialPages = new List<GameObject>();

	private int currentPageIndex;

	private UIDialogWindowData.ButtonData btnData;

	private bool isOkBtnAvailable;

	public override void Init()
	{
		base.Init();
		nextButton.onClick.AddListener(ShowNextPage);
		prevButton.onClick.AddListener(ShowPrevPage);
		nextButton.SetCallbacksIntoGamepadNavigationItem();
		prevButton.SetCallbacksIntoGamepadNavigationItem();
	}

	public override void Open(UITutorialWindowData data)
	{
		MainGame.Instance.GameSave.knowledgeSystem.UnlockTutorial(data.Page);
		MainGame.Instance.GameSave.knowledgeSystem.AddViewedTutorial(data.Page);
		nextButton.gameObject.SetActive(value: false);
		prevButton.gameObject.SetActive(value: false);
		currentPageIndex = 0;
		currentTutorialPages.Clear();
		pages.ForEach(delegate(GameObject x)
		{
			x.gameObject.SetActive(value: false);
		});
		btnData = new UIDialogWindowData.ButtonData(Close, LLBase.L("btn_ok"), () => isOkBtnAvailable, replaceForGamepad: true, GameKey.Select);
		base.Open(data);
		bool flag = false;
		foreach (GameObject page in pages)
		{
			if (page.name == data.Page)
			{
				flag = true;
				currentTutorialPages.Add(page);
			}
			page.SetActive(value: false);
		}
		if (!flag)
		{
			Debug.LogError("UITutorialWindow:Cannot find tutorial page  [" + data.Page + "].");
			Close();
		}
		else
		{
			UpdateHeaderLabel();
			currentTutorialPages[0].SetActive(value: true);
			UpdateButtons();
		}
		((RectTransform)base.transform).RefreshContentFitter();
	}

	private void UpdateHeaderLabel()
	{
		GameObject page = currentTutorialPages[currentPageIndex];
		headerLabel.text = LLBase.L(GetHeaderLocaleId(data.Page, page));
	}

	private static string GetHeaderLocaleId(string defaultHeaderLocaleId, GameObject page)
	{
		if (page.TryGetComponent<UITutorialWindowPageHeaderOverride>(out var component))
		{
			return component.HeaderLocaleId;
		}
		return defaultHeaderLocaleId;
	}

	public override void Close()
	{
		LazyInput.ClearAllKeysDown();
		base.Close();
		data.OnCompleteCallback?.Invoke();
	}

	private void ShowNextPage()
	{
		if (currentTutorialPages.Count > 1 && currentPageIndex != currentTutorialPages.Count - 1)
		{
			currentTutorialPages[currentPageIndex].SetActive(value: false);
			currentPageIndex++;
			currentTutorialPages[currentPageIndex].SetActive(value: true);
			UpdateHeaderLabel();
			UpdateButtons();
			((RectTransform)base.transform).RefreshContentFitter();
		}
	}

	private void ShowPrevPage()
	{
		if (currentTutorialPages.Count > 1 && currentPageIndex != 0)
		{
			currentTutorialPages[currentPageIndex].SetActive(value: false);
			currentPageIndex--;
			currentTutorialPages[currentPageIndex].SetActive(value: true);
			UpdateHeaderLabel();
			UpdateButtons();
			((RectTransform)base.transform).RefreshContentFitter();
		}
	}

	private void UpdateButtons()
	{
		UITutorialWindowData uITutorialWindowData = data;
		bool flag = (uITutorialWindowData != null && uITutorialWindowData.CanCloseFromAnyPage) || currentPageIndex == currentTutorialPages.Count - 1;
		if (currentTutorialPages.Count == 1)
		{
			isOkBtnAvailable = true;
			nextButton.interactable = false;
			prevButton.interactable = false;
			nextButton.gameObject.SetActive(value: false);
			prevButton.gameObject.SetActive(value: false);
			arrowsObj.gameObject.SetActive(value: false);
		}
		else if (currentPageIndex == currentTutorialPages.Count - 1)
		{
			arrowsObj.gameObject.SetActive(value: true);
			isOkBtnAvailable = flag;
			nextButton.gameObject.SetActive(value: true);
			prevButton.gameObject.SetActive(value: true);
			nextButton.interactable = false;
			prevButton.interactable = true;
		}
		else if (currentPageIndex == 0)
		{
			arrowsObj.gameObject.SetActive(value: true);
			isOkBtnAvailable = flag;
			nextButton.gameObject.SetActive(value: true);
			prevButton.gameObject.SetActive(value: true);
			nextButton.interactable = true;
			prevButton.interactable = false;
		}
		else
		{
			arrowsObj.gameObject.SetActive(value: true);
			isOkBtnAvailable = flag;
			nextButton.gameObject.SetActive(value: true);
			prevButton.gameObject.SetActive(value: true);
			nextButton.interactable = true;
			prevButton.interactable = true;
		}
		ApplyVerticalLayoutPadding(arrowsObj.activeSelf);
		lazyButton.Draw(btnData);
		if (LazyInput.IsGamepadActive)
		{
			base.GamepadNavigationController.ReinitItems(focusOnFirstActive: true);
			PrintTips();
		}
	}

	private void ApplyVerticalLayoutPadding(bool hasArrows)
	{
		if (verticalLayoutGroup == null)
		{
			return;
		}
		RectOffset rectOffset = (hasArrows ? paddingWithArrows : paddingWithoutArrows);
		if (rectOffset != null)
		{
			RectOffset rectOffset2 = verticalLayoutGroup.padding;
			if (rectOffset2 == null)
			{
				rectOffset2 = new RectOffset();
				verticalLayoutGroup.padding = rectOffset2;
			}
			rectOffset2.left = rectOffset.left;
			rectOffset2.right = rectOffset.right;
			rectOffset2.top = rectOffset.top;
			rectOffset2.bottom = rectOffset.bottom;
		}
	}

	protected override bool OnPressedBack()
	{
		if (!isOkBtnAvailable)
		{
			return false;
		}
		Close();
		return true;
	}

	protected bool OnPressedLeft()
	{
		ShowPrevPage();
		return true;
	}

	protected bool OnPressedRight()
	{
		ShowNextPage();
		return true;
	}

	protected override Dictionary<GameKey, Func<bool>> GetGameKeyDelegates()
	{
		Dictionary<GameKey, Func<bool>> gameKeyDelegates = base.GetGameKeyDelegates();
		gameKeyDelegates.Add(GameKey.Select, () => false);
		gameKeyDelegates.Add(GameKey.DpadRight, OnPressedRight);
		gameKeyDelegates.Add(GameKey.DpadLeft, OnPressedLeft);
		gameKeyDelegates.Add(GameKey.Right, OnPressedRight);
		gameKeyDelegates.Add(GameKey.Left, OnPressedLeft);
		return gameKeyDelegates;
	}

	protected override void PrintTips()
	{
		List<LazyGameKeyTip> list = new List<LazyGameKeyTip>();
		if (nextButton.gameObject.activeSelf)
		{
			list.Add(new LazyGameKeyTip(GameKey.DpadRight, "tip_next", nextButton.interactable));
		}
		if (prevButton.gameObject.activeSelf)
		{
			list.Add(new LazyGameKeyTip(GameKey.DpadLeft, "tip_prev", prevButton.interactable));
		}
		list.Add(LazyGameKeyTip.Back(isOkBtnAvailable));
		lazyButtonTips.Print(list);
	}

	public void TestOpen(string page)
	{
		UITutorialWindowData uITutorialWindowData = new UITutorialWindowData(page);
		Open(uITutorialWindowData);
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		TestOpen("tut_energy_hdr");
	}

	[LazyUITest]
	protected void TestDrawTwoPages()
	{
		TestOpen("tutorial_hdr_questslog");
	}
}
