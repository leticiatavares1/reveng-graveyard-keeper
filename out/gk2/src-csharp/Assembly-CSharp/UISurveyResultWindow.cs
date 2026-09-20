using System;
using System.Collections.Generic;
using LazyBearTechnology;
using TMPro;
using UnityEngine;

public class UISurveyResultWindow : LazyWindow<UISurveyResultWindowData>
{
	[SerializeField]
	private UIItemCell itemCell;

	[SerializeField]
	private TextMeshProUGUI itemNameLabel;

	[SerializeField]
	private TextMeshProUGUI runesLabel;

	[SerializeField]
	private TextStyle runesStyle;

	[SerializeField]
	private UIDialogWindowButton windowButton;

	public override void Redraw()
	{
		base.Redraw();
		itemCell.Draw(new Item(data.ItemDef.id));
		itemNameLabel.text = LLBase.L(data.ItemDef.id);
		string text = data.ItemDef.GetRunesAsString();
		if (string.IsNullOrEmpty(text))
		{
			text = "-";
		}
		runesLabel.text = LLBase.L("ui_survey_complete_runes") + ": " + runesStyle.ApplyStyleToString(text);
		windowButton.Draw(new UIDialogWindowData.ButtonData(OnBtnPressed, LLBase.L("btn_ok"), null, replaceForGamepad: true, GameKey.Select));
	}

	private void OnBtnPressed()
	{
		Close();
	}

	protected override void PrintTips()
	{
		lazyButtonTips.Clear();
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
		UISurveyResultWindowData uISurveyResultWindowData = new UISurveyResultWindowData(GameBalance.Me.GetData<ItemDef>("flesh"));
		Open(uISurveyResultWindowData);
	}
}
