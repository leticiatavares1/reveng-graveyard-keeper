using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class UIMapWindow : LazyWindow<MapPageWidgetData>
{
	[SerializeField]
	private MapPageWidget mapPageWidget;

	[SerializeField]
	private LazyButtonTipsStr big;

	[SerializeField]
	private LazyButtonTipsStr small;

	public MapPageWidget MapPageWidget => mapPageWidget;

	public override void Redraw()
	{
		base.Redraw();
		mapPageWidget.Draw(data);
		if (GUIElements.Instance.UIWindowSizeType == UIWindowSizeType.Big)
		{
			big.gameObject.SetActive(value: true);
			small.gameObject.SetActive(value: false);
		}
		else
		{
			big.gameObject.SetActive(value: false);
			small.gameObject.SetActive(value: true);
		}
		((RectTransform)base.transform).RefreshContentFitter();
	}

	public void OnEnterMapMilestone(UIMapMilestone mapMilestone)
	{
		mapPageWidget.OnEnterMapMilestone(mapMilestone);
		PrintTips();
	}

	public void OnExitMapMilestone(UIMapMilestone mapMilestone)
	{
		mapPageWidget.OnExitMapMilestone(mapMilestone);
		PrintTips();
	}

	protected override void UpdateGamepadDependentStuff()
	{
		base.UpdateGamepadDependentStuff();
		mapPageWidget.UpdateGamepadDependentStuff();
	}

	protected override Dictionary<GameKey, Func<bool>> GetGameKeyDelegates()
	{
		Dictionary<GameKey, Func<bool>> gameKeyDelegates = base.GetGameKeyDelegates();
		gameKeyDelegates.Add(GameKey.Select, OnSelectPress);
		return gameKeyDelegates;
	}

	private bool OnSelectPress()
	{
		if (mapPageWidget.CurrentSelected != null)
		{
			mapPageWidget.OnPressMapMilestone(mapPageWidget.CurrentSelected);
			return true;
		}
		return false;
	}

	protected override void PrintTips()
	{
		LazyButtonTipsStr lazyButtonTipsStr;
		if (GUIElements.Instance.UIWindowSizeType == UIWindowSizeType.Big)
		{
			big.gameObject.SetActive(value: true);
			small.gameObject.SetActive(value: false);
			lazyButtonTipsStr = big;
		}
		else
		{
			big.gameObject.SetActive(value: false);
			small.gameObject.SetActive(value: true);
			lazyButtonTipsStr = small;
		}
		if (LazyInput.IsGamepadActive)
		{
			if (mapPageWidget.CurrentSelected != null)
			{
				lazyButtonTipsStr.Print(LazyGameKeyTip.Select(), LazyGameKeyTip.Back());
			}
			else
			{
				lazyButtonTipsStr.Print(LazyGameKeyTip.Back());
			}
		}
		else
		{
			lazyButtonTipsStr.Clear();
		}
	}

	protected override void TestDraw()
	{
	}
}
