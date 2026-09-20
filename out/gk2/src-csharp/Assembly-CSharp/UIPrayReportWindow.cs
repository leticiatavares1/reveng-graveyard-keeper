using System;
using System.Collections.Generic;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPrayReportWindow : LazyWindow<UIPrayReportWindowData>
{
	[SerializeField]
	private LazyButton okButton;

	[SerializeField]
	private UIFixedTypeItemCell prayerCell;

	[SerializeField]
	private TextMeshProUGUI topLabel;

	[SerializeField]
	private TextStyle topLabelSuccessStyle;

	[SerializeField]
	private TextStyle topLabelFailStyle;

	[SerializeField]
	private TextMeshProUGUI visitorsLabel;

	[SerializeField]
	private TextMeshProUGUI faithLabel;

	[SerializeField]
	private TextMeshProUGUI moneyLabel;

	[SerializeField]
	private TextMeshProUGUI faithBonusLabel;

	[SerializeField]
	private TextMeshProUGUI moneyBonusLabel;

	[SerializeField]
	private GameObject rewardBlock;

	[SerializeField]
	private GameObject itemsBlock;

	[SerializeField]
	private PerkWidget givenBuff;

	[SerializeField]
	private List<UIItemCell> rewardCells = new List<UIItemCell>();

	[SerializeField]
	private TextMeshProUGUI gameKeyTipLabel;

	public override void Init()
	{
		base.Init();
		okButton.onClick.AddListener(Close);
	}

	public override void Redraw()
	{
		base.Redraw();
		prayerCell.Draw(new Item(data.SermonResultData.Definition.id));
		int moneyOnlyParishioners = data.SermonResultData.MoneyOnlyParishioners;
		int faithOnlyParishioners = data.SermonResultData.FaithOnlyParishioners;
		visitorsLabel.text = string.Format("{0}{1}", "happiness_cross".FontIcon(), data.SermonResultData.parishionersCount);
		if (faithOnlyParishioners > 0)
		{
			faithLabel.text = string.Format("{0}{1}", "faith".FontIcon(), faithOnlyParishioners);
			faithLabel.transform.parent.parent.gameObject.SetActive(value: true);
		}
		else
		{
			faithLabel.transform.parent.parent.gameObject.SetActive(value: false);
		}
		if (moneyOnlyParishioners > 0)
		{
			moneyLabel.text = Trading.FormatMoney(moneyOnlyParishioners, printZero: false, " ", GameResIconType.MoneyBig);
			moneyLabel.transform.parent.parent.gameObject.SetActive(value: true);
		}
		else
		{
			moneyLabel.transform.parent.parent.gameObject.SetActive(value: false);
		}
		foreach (UIItemCell rewardCell in rewardCells)
		{
			rewardCell.gameObject.SetActive(value: false);
		}
		itemsBlock.gameObject.SetActive(value: false);
		if (data.SermonResultData.success)
		{
			rewardBlock.gameObject.SetActive(value: true);
			topLabel.text = LLBase.L("ui_pray_success_text");
			topLabelSuccessStyle.ApplyStyle(topLabel);
			int faithOnlyBonus = data.SermonResultData.FaithOnlyBonus;
			int moneyOnlyBonus = data.SermonResultData.MoneyOnlyBonus;
			if (faithOnlyBonus > 0)
			{
				faithBonusLabel.text = string.Format("{0}{1}", "faith".FontIcon(), faithOnlyBonus);
				faithBonusLabel.transform.parent.parent.gameObject.SetActive(value: true);
			}
			else
			{
				faithBonusLabel.transform.parent.parent.gameObject.SetActive(value: false);
			}
			if (moneyOnlyBonus > 0)
			{
				moneyBonusLabel.text = Trading.FormatMoney(moneyOnlyBonus, printZero: false, " ", GameResIconType.MoneyBig);
				moneyBonusLabel.transform.parent.parent.gameObject.SetActive(value: true);
			}
			else
			{
				moneyBonusLabel.transform.parent.parent.gameObject.SetActive(value: false);
			}
			bool flag = false;
			bool flag2 = false;
			if (!string.IsNullOrEmpty(data.SermonResultData.Definition.successRewardBuff))
			{
				givenBuff.Draw(new PerkWidgetData(new PerkData(data.SermonResultData.Definition.successRewardBuff), isActive: true, null, null, null));
				givenBuff.gameObject.SetActive(value: true);
				flag = true;
			}
			else
			{
				givenBuff.gameObject.SetActive(value: false);
			}
			WgoData wgoData = MainGame.Instance.GameSave.worldData.GetWgoData(data.SermonResultData.sermonConfigId);
			List<Item> list = OutputItems.MakeOutput(data.SermonResultData.Definition.successRewardItem.MakePreOutput(wgoData));
			for (int i = 0; i < list.Count; i++)
			{
				flag2 = true;
				rewardCells[i].Draw(list[i]);
				rewardCells[i].gameObject.SetActive(value: true);
			}
			itemsBlock.gameObject.SetActive(flag2 || flag);
		}
		else
		{
			rewardBlock.gameObject.SetActive(value: false);
			topLabel.text = LLBase.L("ui_pray_fail_text");
			topLabelFailStyle.ApplyStyle(topLabel);
		}
		((RectTransform)base.transform).RefreshContentFitter();
	}

	protected override void HideWindow()
	{
		data?.HandleClosingWindow();
		base.HideWindow();
	}

	protected override Dictionary<GameKey, Func<bool>> GetGameKeyDelegates()
	{
		Dictionary<GameKey, Func<bool>> gameKeyDelegates = base.GetGameKeyDelegates();
		gameKeyDelegates.Add(GameKey.Select, delegate
		{
			Close();
			return true;
		});
		return gameKeyDelegates;
	}

	protected override void PrintTips()
	{
		lazyButtonTips.Clear();
		gameKeyTipLabel.text = new LazyGameKeyTip(GameKey.Select, LLBase.L("btn_ok")).ToString();
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		UIPrayReportWindowData uIPrayReportWindowData = new UIPrayReportWindowData(new SermonResultData("prayer_base:1", "church_tribune", 5, success: true, 5, 4), null);
		LazyUI.GetWindow<UIPrayReportWindow>().Open(uIPrayReportWindowData);
	}

	[LazyUITest]
	protected void TestDraw2()
	{
		UIPrayReportWindowData uIPrayReportWindowData = new UIPrayReportWindowData(new SermonResultData("prayer_faith:2", "church_tribune", 11, success: true, 11, 55), null);
		LazyUI.GetWindow<UIPrayReportWindow>().Open(uIPrayReportWindowData);
	}

	[LazyUITest]
	protected void TestDraw3()
	{
		UIPrayReportWindowData uIPrayReportWindowData = new UIPrayReportWindowData(new SermonResultData("prayer_harvest:1", "church_tribune", 2, success: true, 2, 55), null);
		LazyUI.GetWindow<UIPrayReportWindow>().Open(uIPrayReportWindowData);
	}

	[LazyUITest]
	protected void TestDrawFail()
	{
		UIPrayReportWindowData uIPrayReportWindowData = new UIPrayReportWindowData(new SermonResultData("prayer_combat:2", "church_tribune", 11, success: false, 11, 55), null);
		LazyUI.GetWindow<UIPrayReportWindow>().Open(uIPrayReportWindowData);
	}
}
