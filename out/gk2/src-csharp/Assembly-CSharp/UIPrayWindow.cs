using System;
using System.Collections.Generic;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPrayWindow : LazyWindow<UIPrayWindowData>
{
	[SerializeField]
	private UIFixedTypeItemCell prayInsertionCell;

	[SerializeField]
	private TextMeshProUGUI churchQualityLabel;

	[SerializeField]
	private TextMeshProUGUI smilesCountLabel;

	[SerializeField]
	private TextMeshProUGUI resultVisitorsLabel;

	[SerializeField]
	private TextMeshProUGUI successChanceLabel;

	[SerializeField]
	private TextMeshProUGUI notEnoughParishionersLabel;

	[SerializeField]
	private TextMeshProUGUI topLabel;

	[SerializeField]
	private GameObject[] activateWhenNoPray;

	[SerializeField]
	private GameObject[] activateWhenYesPray;

	[SerializeField]
	private TextMeshProUGUI gameKeyTipLabel;

	[SerializeField]
	private TextStyle topLabelStyleActive;

	[SerializeField]
	private TextStyle topLabelStyleInactive;

	[SerializeField]
	private TextStyle notEnoughParishionersStyle;

	[Space]
	[SerializeField]
	private LazyButton startButton;

	private int chance;

	public override void Init()
	{
		base.Init();
		startButton.onDown.AddListener(OnStartButtonPress);
		GameObject go = churchQualityLabel.gameObject;
		Vector2 appearOffset = new Vector2(0f, -2f);
		UIMouseTooltip.Attach(go, "tt_pray_1", null, addRaycastTarget: true, disableChildRaycasts: false, default(UIMouseTooltipEdges), appearOffset);
		GameObject go2 = smilesCountLabel.gameObject;
		appearOffset = new Vector2(0f, -2f);
		UIMouseTooltip.Attach(go2, "tt_town_happiness", null, addRaycastTarget: true, disableChildRaycasts: false, default(UIMouseTooltipEdges), appearOffset);
		GameObject go3 = resultVisitorsLabel.gameObject;
		appearOffset = new Vector2(0f, -2f);
		UIMouseTooltip.Attach(go3, "tt_pray_3", null, addRaycastTarget: true, disableChildRaycasts: false, default(UIMouseTooltipEdges), appearOffset);
	}

	public override void Redraw()
	{
		churchQualityLabel.text = "cross".FontIcon() + data.ChurchQuality;
		smilesCountLabel.text = string.Format("{0}{1}", "happiness".FontIcon(), data.Happiness);
		resultVisitorsLabel.text = string.Format("{0}{1}", "happiness_cross".FontIcon(), data.ResultVisitors);
		if (data.SermonDef == null)
		{
			notEnoughParishionersLabel.gameObject.SetActive(value: false);
			topLabel.text = LLBase.L("ui_choose_pray");
			topLabelStyleInactive.ApplyStyle(topLabel);
			prayInsertionCell.DrawEmptyInteractable();
			GameObject[] array = activateWhenNoPray;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: true);
			}
			array = activateWhenYesPray;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: false);
			}
		}
		else
		{
			chance = Math.Clamp((int)((float)data.ResultVisitors * 100f / (float)data.SermonDef.sermonDifficulty), 0, 100);
			successChanceLabel.text = string.Format("{0}: {1}%", LLBase.L("ui_success_chance"), chance);
			prayInsertionCell.Draw(new Item(data.SermonDef.id));
			ItemDef itemDef = GameBalance.Me.GetData<ItemDef>(data.SermonDef.id);
			if (itemDef != null)
			{
				topLabel.text = itemDef.GetHeader();
			}
			else
			{
				topLabel.text = LLBase.L(data.SermonDef.id) ?? "";
			}
			topLabelStyleActive.ApplyStyle(topLabel);
			GameObject[] array = activateWhenNoPray;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: false);
			}
			array = activateWhenYesPray;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: true);
			}
			bool flag = data.EnoughParishioners();
			notEnoughParishionersLabel.gameObject.SetActive(!flag);
			notEnoughParishionersLabel.text = string.Format("{0}: {1}{2}", LLBase.L("ui_not_enough_parishioners"), "happiness_cross".FontIcon(), data.SermonDef.minParishioners);
			if (notEnoughParishionersStyle != null)
			{
				notEnoughParishionersStyle.ApplyStyle(notEnoughParishionersLabel);
			}
			successChanceLabel.gameObject.SetActive(flag);
		}
		prayInsertionCell.UIItemCell.OnItemCellPress = HandlePraySlotPress;
		CheckCraftCanStart();
	}

	public override void Hide()
	{
		if (data != null)
		{
			data.EraseNonStartedCraft();
		}
		base.Hide();
	}

	private void OnStartButtonPress()
	{
		data.StartCraft(data.ResultVisitors, chance);
		Close();
	}

	private void HandlePraySlotPress(UIItemCell cell)
	{
		data.OnPraySlotPress?.Invoke();
	}

	private void CheckCraftCanStart()
	{
		startButton.interactable = data.CanStartCraft();
	}

	private bool OnPrayStartPress()
	{
		if (startButton.interactable)
		{
			OnStartButtonPress();
			return true;
		}
		return false;
	}

	protected override Dictionary<GameKey, Func<bool>> GetGameKeyDelegates()
	{
		Dictionary<GameKey, Func<bool>> gameKeyDelegates = base.GetGameKeyDelegates();
		gameKeyDelegates.Add(GameKey.PrayStart, OnPrayStartPress);
		return gameKeyDelegates;
	}

	protected override void PrintTips()
	{
		lazyButtonTips.Print(LazyGameKeyTip.Select(), LazyGameKeyTip.Back());
		gameKeyTipLabel.text = new LazyGameKeyTip(GameKey.PrayStart, LLBase.L("ui_pray_start"), startButton.interactable).ToString();
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		SermonConfigDef sermonConfigDef = GameBalance.Me.GetData<SermonConfigDef>("church_tribune");
		UIPrayWindowData uIPrayWindowData = new UIPrayWindowData(MainGame.PlayerData, MainGame.WorldData.GetWgoData("church_tribune"), 10, 10, 5, sermonConfigDef.rewardBoxId);
		LazyUI.GetWindow<UIPrayWindow>().Open(uIPrayWindowData);
	}
}
