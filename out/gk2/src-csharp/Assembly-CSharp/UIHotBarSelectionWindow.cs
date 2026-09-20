using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class UIHotBarSelectionWindow : LazyWindow<UIHotBarSelectionWindowData>
{
	[SerializeField]
	private UIHotBarWidget hotBarWidget;

	[SerializeField]
	private Transform hotBarKeyboardPos;

	[SerializeField]
	private Transform hotBarGamepadPos;

	[SerializeField]
	private UIItemCell itemKeyboard;

	[SerializeField]
	private UIItemCell itemGamepad;

	protected override void ShowWindow()
	{
		base.ShowWindow();
		LazyInput.OnInputChanged += Close;
	}

	public override void Redraw()
	{
		base.Redraw();
		itemKeyboard.Draw(data.Item);
		itemGamepad.Draw(data.Item);
		itemKeyboard.ClearCallbacks();
		itemGamepad.ClearCallbacks();
		hotBarWidget.Draw(data.HotBarWidgetData);
		UpdateHotBarPos();
		((RectTransform)base.transform).RefreshContentFitter();
	}

	public override void Close()
	{
		base.Close();
		LazyInput.OnInputChanged -= Close;
		hotBarWidget.Hide();
	}

	protected override void Update()
	{
		base.Update();
		if (!LazyInput.IsGamepadActive)
		{
			if (LazyInput.GetKeyDown(GameKey.UseHotBarItem1))
			{
				SetSelectItemToHotBarAtIndex(0);
				Close();
			}
			else if (LazyInput.GetKeyDown(GameKey.UseHotBarItem2))
			{
				SetSelectItemToHotBarAtIndex(1);
				Close();
			}
			else if (LazyInput.GetKeyDown(GameKey.UseHotBarItem3))
			{
				SetSelectItemToHotBarAtIndex(2);
				Close();
			}
			else if (LazyInput.GetKeyDown(GameKey.UseHotBarItem4))
			{
				SetSelectItemToHotBarAtIndex(3);
				Close();
			}
		}
	}

	protected override bool OnPressedBack()
	{
		Close();
		return true;
	}

	private void UpdateHotBarPos()
	{
		if (LazyInput.IsGamepadActive)
		{
			hotBarWidget.transform.SetParent(hotBarGamepadPos);
			hotBarGamepadPos.gameObject.SetActive(value: true);
			hotBarKeyboardPos.gameObject.SetActive(value: false);
		}
		else
		{
			hotBarWidget.transform.SetParent(hotBarKeyboardPos);
			hotBarGamepadPos.gameObject.SetActive(value: false);
			hotBarKeyboardPos.gameObject.SetActive(value: true);
		}
		((RectTransform)hotBarWidget.transform).anchoredPosition = Vector3.zero;
	}

	private bool SetSelectItemToHotBarAtIndex(int index)
	{
		MainGame.PlayerData.SetHotBarItemAtIndex(data.Item.id, index);
		Close();
		return true;
	}

	protected override Dictionary<GameKey, Func<bool>> GetGameKeyDelegates()
	{
		Dictionary<GameKey, Func<bool>> gameKeyDelegates = base.GetGameKeyDelegates();
		gameKeyDelegates.Add(GameKey.UseHotBarItem1, () => SetSelectItemToHotBarAtIndex(0));
		gameKeyDelegates.Add(GameKey.UseHotBarItem2, () => SetSelectItemToHotBarAtIndex(1));
		gameKeyDelegates.Add(GameKey.UseHotBarItem3, () => SetSelectItemToHotBarAtIndex(2));
		gameKeyDelegates.Add(GameKey.UseHotBarItem4, () => SetSelectItemToHotBarAtIndex(3));
		return gameKeyDelegates;
	}

	protected override void PrintTips()
	{
		lazyButtonTips.Clear();
	}

	protected override void TestDraw()
	{
		Open(new UIHotBarSelectionWindowData(MainGame.Instance.GameSave, new Item("beer")));
	}
}
