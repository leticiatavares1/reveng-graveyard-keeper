using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class FightLoseWindow : LazyWindow<FightEndWindowData>
{
	[SerializeField]
	private UIDialogWindowButton lazyButton;

	private UIDialogWindowData.ButtonData btnData;

	public override void Redraw()
	{
		base.Redraw();
		btnData = new UIDialogWindowData.ButtonData(Close, LLBase.L("btn_ok"), null, replaceForGamepad: true, GameKey.Select);
		lazyButton.Draw(btnData);
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
