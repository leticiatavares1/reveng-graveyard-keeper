using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class UIPreorderWindow : LazyWindow<LazyWidgetDataBase>
{
	[SerializeField]
	private UIDialogWindowButton lazyButton;

	[SerializeField]
	private LazyButton nextButton;

	[SerializeField]
	private LazyButton prevButton;

	[SerializeField]
	private GameObject page1;

	[SerializeField]
	private GameObject page2;

	[SerializeField]
	private Image skinImage;

	[SerializeField]
	private Sprite skinPC;

	[SerializeField]
	private Sprite skinPlaystation;

	[SerializeField]
	private Sprite skinXbox;

	[SerializeField]
	private Sprite skinSwitch;

	private UIDialogWindowData.ButtonData btnData;

	private bool isFirstPage;

	public override void Init()
	{
		base.Init();
		nextButton.onClick.AddListener(ShowNextPage);
		prevButton.onClick.AddListener(delegate
		{
			ShowPrevPage();
		});
		nextButton.SetCallbacksIntoGamepadNavigationItem();
		prevButton.SetCallbacksIntoGamepadNavigationItem();
	}

	public override void Open(LazyWidgetDataBase data)
	{
		nextButton.gameObject.SetActive(value: false);
		prevButton.gameObject.SetActive(value: false);
		isFirstPage = true;
		btnData = new UIDialogWindowData.ButtonData(Close, LLBase.L("btn_ok"), () => true, replaceForGamepad: true, GameKey.Select);
		skinImage.sprite = skinPC;
		base.Open(data);
		UpdatePage();
		((RectTransform)base.transform).RefreshContentFitter();
	}

	private void ShowNextPage()
	{
		if (isFirstPage)
		{
			isFirstPage = false;
			UpdatePage();
		}
	}

	private bool ShowPrevPage()
	{
		if (isFirstPage)
		{
			return false;
		}
		isFirstPage = true;
		UpdatePage();
		return true;
	}

	private void UpdatePage()
	{
		page1.SetActive(isFirstPage);
		page2.SetActive(!isFirstPage);
		UpdateButtons();
		((RectTransform)base.transform).RefreshContentFitter();
	}

	private void UpdateButtons()
	{
		nextButton.gameObject.SetActive(value: true);
		prevButton.gameObject.SetActive(value: true);
		nextButton.interactable = isFirstPage;
		prevButton.interactable = !isFirstPage;
		lazyButton.Draw(btnData);
		if (LazyInput.IsGamepadActive)
		{
			base.GamepadNavigationController.ReinitItems(focusOnFirstActive: true);
			PrintTips();
		}
	}

	protected override bool OnPressedBack()
	{
		if (!prevButton.gameObject.activeSelf || !prevButton.interactable)
		{
			return false;
		}
		if (!ShowPrevPage())
		{
			base.OnPressedBack();
		}
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
			if (prevButton.interactable)
			{
				list.Add(LazyGameKeyTip.Back());
			}
		}
		lazyButtonTips.Print(list);
	}

	protected override void TestDraw()
	{
	}
}
