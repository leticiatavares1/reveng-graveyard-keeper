using System;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TalentLevelUpWidget : LazyWidget<TalentLevelUpWidgetData>
{
	[SerializeField]
	private LazyButton button;

	[SerializeField]
	private Image[] icons;

	[SerializeField]
	private TextMeshProUGUI[] dbgImageTexts;

	[SerializeField]
	private GameObject selectionFrame;

	[SerializeField]
	private Vector2 zombiePos;

	[SerializeField]
	private Vector2 playerPos;

	[SerializeField]
	private TextMeshProUGUI[] priceLabels;

	[SerializeField]
	private TextStyle enoughStyle;

	[SerializeField]
	private TextStyle notEnoughStyle;

	[SerializeField]
	private GameObject availableGreenFrame;

	[SerializeField]
	private GameObject unknownState;

	[SerializeField]
	private GameObject visibleState;

	[SerializeField]
	private GameObject availableState;

	[SerializeField]
	private GameObject unlockedState;

	[SerializeField]
	private TextMeshProUGUI redSkullObject;

	[SerializeField]
	private GameObject activeBorder;

	[SerializeField]
	private GameObject inactiveBorder;

	[SerializeField]
	private Image activeImage;

	[SerializeField]
	private Material inactiveMaterial;

	[SerializeField]
	private Material activeMaterial;

	[SerializeField]
	private GameObject canBuyEffect;

	private Action onPress;

	private Action onOver;

	private Action onOut;

	public TalentLevelUpWidgetData Data => data;

	public override void Redraw()
	{
		base.Redraw();
		for (int i = 0; i < icons.Length; i++)
		{
			icons[i].sprite = data.Def.Icon;
			icons[i].gameObject.SetActive(icons[i].sprite != null);
			dbgImageTexts[i].text = data.Def.id;
			dbgImageTexts[i].gameObject.SetActive(icons[i].sprite == null);
		}
		onPress = data.OnPress;
		onOver = data.OnOver;
		onOut = data.OnOut;
		button.interactable = false;
		unknownState.SetActive(value: false);
		visibleState.SetActive(value: false);
		availableState.SetActive(value: false);
		unlockedState.SetActive(value: false);
		redSkullObject.gameObject.SetActive(value: false);
		inactiveBorder.gameObject.SetActive(value: false);
		activeImage.material = activeMaterial;
		TalentData talentData;
		bool flag = (data.Def.isZombiePerk ? data.ZombieWgoData.IsEnoughResourcesToBuyTalentLevelUp(data.Def) : MainGame.Instance.GameSave.talentSystemData.CanPurchaseLevel(data.Def.id, out talentData));
		for (int j = 0; j < priceLabels.Length; j++)
		{
			if (data.Def.isZombiePerk)
			{
				priceLabels[j].text = $"{data.Def.ZombieTechPointsIcon}{data.Def.ZombieTechPoints}";
				priceLabels[j].rectTransform.anchoredPosition = zombiePos;
			}
			else
			{
				priceLabels[j].text = $"{data.PriceIconId.FontIcon()}{data.Def.talentExpPointsPrice}";
				priceLabels[j].rectTransform.anchoredPosition = playerPos;
			}
			if (flag)
			{
				enoughStyle.ApplyStyle(priceLabels[j]);
			}
			else
			{
				notEnoughStyle.ApplyStyle(priceLabels[j]);
			}
		}
		switch (data.State)
		{
		case TalentLevelUpDef.State.Unknown:
			unknownState.SetActive(value: true);
			break;
		case TalentLevelUpDef.State.Visible:
			button.interactable = true;
			visibleState.SetActive(value: true);
			break;
		case TalentLevelUpDef.State.Available:
			button.interactable = true;
			availableState.SetActive(value: true);
			break;
		case TalentLevelUpDef.State.Unlocked:
			button.interactable = true;
			unlockedState.SetActive(value: true);
			activeBorder.SetActive(value: true);
			break;
		}
		base.transform.localPosition = data.localPosition;
		base.gameObject.SetActive(value: true);
		if (canBuyEffect != null)
		{
			canBuyEffect.SetActive(data.State == TalentLevelUpDef.State.Available && !data.Def.isZombiePerk);
		}
		if (data.Def.isZombiePerk)
		{
			if (data.ZombieWgoData.IsTalentLevelUpStudied(data.Def.id))
			{
				redSkullObject.gameObject.SetActive(value: true);
				redSkullObject.text = "rskull-zombie_window-active_prk".FontIcon();
			}
			if (data.ZombieWgoData.disabledTalentLevelUps.Contains(data.Def.id))
			{
				activeBorder.SetActive(value: false);
				inactiveBorder.gameObject.SetActive(value: true);
				activeImage.material = inactiveMaterial;
				redSkullObject.text = "rskull-zombie_window-inactive_perk".FontIcon() + "-1";
			}
		}
		if (availableGreenFrame != null)
		{
			availableGreenFrame.SetActive(data.Def.isZombiePerk);
		}
	}

	public void ClearCallbacks()
	{
		onPress = null;
		onOver = null;
		onOut = null;
	}

	private void Awake()
	{
		button.onClick.AddListener(OnPress);
		button.onEnter.AddListener(OnOver);
		button.onExit.AddListener(OnOut);
		selectionFrame.SetActive(value: false);
	}

	private void OnPress()
	{
		if (data.State != TalentLevelUpDef.State.Available)
		{
			if (data.State == TalentLevelUpDef.State.Visible && !data.Def.isZombiePerk)
			{
				ShowCantPurchaseDialog();
			}
		}
		else
		{
			onPress?.Invoke();
		}
	}

	private void ShowCantPurchaseDialog()
	{
		TalentData talentBranch = MainGame.Instance.GameSave.talentSystemData.GetTalentBranch(data.Def.talentId);
		string text = "";
		if (talentBranch.talentExpPoints < data.Def.talentExpPointsPrice)
		{
			text = LLBase.L("ui_dont_have_points", data.PriceIconId.FontIcon());
		}
		if (!data.Def.ParentsUnlocked)
		{
			if (!string.IsNullOrEmpty(text))
			{
				text += "\n";
			}
			text += LLBase.L("ui_previous_perk_locked");
		}
		UIDialogWindowData uIDialogWindowData = new UIDialogWindowData(GetDialogHeader(), text, new UIDialogWindowData.ButtonData(LazyUI.GetWindow<UIDialogWindow>().Close, LLBase.L("btn_ok"), null, replaceForGamepad: true, GameKey.Select));
		LazyUI.GetWindow<UIDialogWindow>().Open(uIDialogWindowData);
	}

	private string GetDialogHeader()
	{
		if (!string.IsNullOrEmpty(data.Def.linkedPerk))
		{
			PerkDef perkDef = GameBalance.Me.GetData<PerkDef>(data.Def.linkedPerk);
			if (perkDef != null)
			{
				return perkDef.GetHeader();
			}
		}
		return LLBase.L(data.Def.id);
	}

	private void OnOver()
	{
		selectionFrame.SetActive(value: true);
		UITooltip.ShowTalentLevelUpWidget(this);
		onOver?.Invoke();
	}

	private void OnOut()
	{
		selectionFrame.SetActive(value: false);
		UITooltip.Hide();
		onOut?.Invoke();
	}

	private void OnDisable()
	{
		selectionFrame.SetActive(value: false);
		if (UITooltip.IsTooltipShowingAtTarget(base.transform as RectTransform))
		{
			UITooltip.HideImmediately();
		}
	}

	protected override void TestDraw()
	{
	}
}
