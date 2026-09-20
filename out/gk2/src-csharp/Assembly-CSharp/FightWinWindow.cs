using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class FightWinWindow : LazyWindow<FightEndWindowData>
{
	[SerializeField]
	private GameObject noRewardsObj;

	[SerializeField]
	private GameObject rewardsObj;

	[SerializeField]
	private UIDialogWindowButton lazyButton;

	[SerializeField]
	private UIItemCell[] rewardCells;

	private UIDialogWindowData.ButtonData btnData;

	public override void Redraw()
	{
		base.Redraw();
		btnData = new UIDialogWindowData.ButtonData(Close, LLBase.L("btn_ok"), null, replaceForGamepad: true, GameKey.Select);
		lazyButton.Draw(btnData);
		UIItemCell[] array = rewardCells;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.SetActive(value: false);
		}
		if (data.FightDefinition.rewards.Count > 0)
		{
			for (int j = 0; j < data.FightDefinition.rewards.Count; j++)
			{
				rewardCells[j].gameObject.SetActive(value: true);
				rewardCells[j].Draw(new Item(data.FightDefinition.rewards[j].id, data.FightDefinition.rewards[j].GetCount()));
			}
			noRewardsObj.SetActive(value: false);
			rewardsObj.SetActive(value: true);
		}
		else
		{
			noRewardsObj.SetActive(value: true);
			rewardsObj.SetActive(value: false);
		}
		if (LazyInput.IsGamepadActive)
		{
			base.GamepadNavigationController.ReinitItems(focusOnFirstActive: true);
		}
		((RectTransform)base.transform).RefreshContentFitter();
	}

	protected override void PrintTips()
	{
		List<LazyGameKeyTip> list = new List<LazyGameKeyTip>();
		list.Add(LazyGameKeyTip.Back());
		lazyButtonTips.Print(list);
	}

	protected override Dictionary<GameKey, Func<bool>> GetGameKeyDelegates()
	{
		Dictionary<GameKey, Func<bool>> gameKeyDelegates = base.GetGameKeyDelegates();
		gameKeyDelegates.Add(GameKey.Select, () => false);
		return gameKeyDelegates;
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		Open(new FightEndWindowData(GameBalance.Me.GetData<FightDef>("fight_A1_1")));
	}
}
