using System;
using System.Collections.Generic;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InspirationWidget : LazyWidget<InspirationWidgetData>
{
	[Serializable]
	private class TalentViewData
	{
		public string talentId;

		public string pointIconId;
	}

	[SerializeField]
	private TextMeshProUGUI idLabel;

	[SerializeField]
	private TextStyleComponent idStyleComponent;

	[SerializeField]
	private TextStyle idStyleCompleted;

	[SerializeField]
	private TextStyle idStyleNotCompleted;

	[SerializeField]
	private TextMeshProUGUI descriptionLabel;

	[SerializeField]
	private TextMeshProUGUI buyExp;

	[SerializeField]
	private Image icon;

	[SerializeField]
	private Image iconFrame;

	[SerializeField]
	private Color iconOutlineColor;

	[SerializeField]
	private TextMeshProUGUI priceLabel;

	[SerializeField]
	private TextStyleComponent priceLabelStyle;

	[SerializeField]
	private TextStyle priceLabelEnoughStyle;

	[SerializeField]
	private TextStyle priceLabelNotEnoughStyle;

	[SerializeField]
	private Slider progressBar;

	[SerializeField]
	private TextMeshProUGUI progressLabel;

	[SerializeField]
	private Sprite[] framesSprites;

	[SerializeField]
	private Image background;

	[SerializeField]
	private Sprite backCompletedSprite;

	[SerializeField]
	private Sprite backNotCompletedSprite;

	[SerializeField]
	private RectTransform expPointTooltipRect;

	[SerializeField]
	private LazyButton button;

	[SerializeField]
	private LocalizedLabel buyLocalizedLabel;

	[SerializeField]
	private TMP_Text getCenteredLabel;

	[SerializeField]
	private GameObject visualSeparator;

	private Action onPress;

	[SerializeField]
	private List<TalentViewData> viewDatas = new List<TalentViewData>();

	private TalentViewData currentViewData;

	private int displayedCompletionExp;

	private bool isBuyLocked;

	public string IdWithoutLevel => data.IdWithoutLevel;

	public int Level => data.CurrentLevel;

	public InspirationData InspirationData => data?.InspirationData;

	public RectTransform BuyExpRect
	{
		get
		{
			if (!(buyExp != null))
			{
				return null;
			}
			return buyExp.rectTransform;
		}
	}

	public TextMeshProUGUI BuyExpLabel => buyExp;

	public string PointIconId => currentViewData?.pointIconId;

	public int DisplayedCompletionExp => displayedCompletionExp;

	public bool IsBuyLocked => isBuyLocked;

	public void HideFlyingReward()
	{
		LockBuyButton();
		if (buyExp != null)
		{
			buyExp.alpha = 0f;
		}
	}

	public override void Init()
	{
		base.Init();
		button.onClick.RemoveAllListeners();
		button.onClick.AddListener(OnPress);
	}

	public override void Redraw()
	{
		currentViewData = viewDatas.Find((TalentViewData d) => d.talentId == data.InspirationDef.talentId);
		InspirationData inspirationData = data.InspirationData;
		string text = inspirationData.id + $"_{data.CurrentLevel}";
		idLabel.text = LLBase.L(text);
		descriptionLabel.text = LLBase.L(text + "_d");
		icon.sprite = data.InspirationDef.Icon;
		icon.BlueColorReplace(iconOutlineColor);
		iconFrame.sprite = framesSprites[data.CurrentLevelFrame - 1];
		UpdateBuyButton(data.Price);
		progressBar.value = inspirationData.Progress01;
		progressLabel.text = ((inspirationData.curProgressValue > inspirationData.completionGoalValue) ? $"{inspirationData.completionGoalValue}/{inspirationData.completionGoalValue}" : $"{inspirationData.curProgressValue}/{inspirationData.completionGoalValue}");
		displayedCompletionExp = GameBalance.Me.GetData<InspirationDef>($"{data.InspirationData.id}_{data.CurrentLevel}").completionExp;
		buyExp.text = $"+{currentViewData.pointIconId.FontIcon()}{displayedCompletionExp}";
		buyExp.alpha = 1f;
		isBuyLocked = false;
		SetBuyButtonInteractable(inspirationData.IsAvailableToBuy);
		UpdateDownPart();
		onPress = data.OnPress;
		if (data.InspirationData.IsCompleted)
		{
			background.sprite = backCompletedSprite;
			idStyleComponent.SetTextStyle(idStyleCompleted);
		}
		else
		{
			background.sprite = backNotCompletedSprite;
			idStyleComponent.SetTextStyle(idStyleNotCompleted);
		}
	}

	private void UpdateDownPart()
	{
		if (data.InspirationData.IsCompleted && !data.InspirationData.isAllLevelsBought)
		{
			button.gameObject.SetActive(value: true);
			progressBar.transform.parent.gameObject.SetActive(value: false);
		}
		else
		{
			button.gameObject.SetActive(value: false);
			progressBar.transform.parent.gameObject.SetActive(value: true);
		}
	}

	private void ResolveBuyLabel()
	{
		if (buyLocalizedLabel == null && button != null)
		{
			buyLocalizedLabel = button.GetComponentInChildren<LocalizedLabel>(includeInactive: true);
		}
	}

	private void UpdateBuyButton(int price)
	{
		ResolveBuyLabel();
		bool flag = price <= 0;
		if (priceLabel != null)
		{
			priceLabel.gameObject.SetActive(!flag);
		}
		if (visualSeparator != null)
		{
			visualSeparator.SetActive(!flag);
		}
		if (buyLocalizedLabel != null)
		{
			buyLocalizedLabel.gameObject.SetActive(!flag);
			if (!flag)
			{
				buyLocalizedLabel.langToken = "ui_insp_btn_buy";
				buyLocalizedLabel.Localize();
			}
		}
		if (getCenteredLabel != null)
		{
			getCenteredLabel.gameObject.SetActive(flag);
			if (flag)
			{
				getCenteredLabel.text = LLBase.L("action_get");
			}
		}
		if (!flag)
		{
			priceLabel.text = string.Format("{0}{1}", "faith".FontIcon(), price);
			priceLabelStyle.SetTextStyle(MainGame.PlayerData.inventory.Data.HasItemQuantityInInventory("faith", price) ? priceLabelEnoughStyle : priceLabelNotEnoughStyle);
		}
	}

	private void OnPress()
	{
		if (data.InspirationData == null || isBuyLocked)
		{
			return;
		}
		CharInspirationPageWidget componentInParent = GetComponentInParent<CharInspirationPageWidget>();
		Action confirmPurchase = onPress;
		string id = data.InspirationData.id;
		InspirationWidget widgetToLock;
		UIDialogWindow infoWindow;
		if (data.InspirationData.IsCompleted && !data.InspirationData.isAllLevelsBought)
		{
			int completionExp = GameBalance.Me.GetData<InspirationDef>($"{data.InspirationData.id}_{data.CurrentLevel}").completionExp;
			int price = data.Price;
			string text = idLabel.text;
			string value = ((currentViewData != null) ? $"{currentViewData.pointIconId.FontIcon()}{completionExp}" : completionExp.ToString());
			widgetToLock = FinishFlyAndLock(componentInParent, id);
			infoWindow = LazyUI.GetWindow<UIDialogWindow>();
			string text2 = LLBase.L("complete_insp_for", string.Format("{0}{1}", "faith".FontIcon(), price));
			if (completionExp != 0)
			{
				text2 = text2 + "\n" + LLBase.L("complete_insp_you_get", value);
			}
			UIDialogWindowData uIDialogWindowData = new UIDialogWindowData(text, text2, new UIDialogWindowData.ButtonData(OnConfirm, LLBase.L("btn_yes"), null, replaceForGamepad: true, GameKey.Select));
			uIDialogWindowData.CloseButtonAction = OnCancel;
			infoWindow.Open(uIDialogWindowData);
		}
		void OnCancel()
		{
			widgetToLock?.UnlockBuyButton();
			infoWindow.Close();
		}
		void OnConfirm()
		{
			confirmPurchase?.Invoke();
			LazyUI.GetWindow<UIDialogWindow>().Close();
		}
	}

	private InspirationWidget FinishFlyAndLock(CharInspirationPageWidget page, string inspirationId)
	{
		page?.CompleteFlyingExpAnimationImmediately();
		InspirationWidget obj = ((page != null) ? page.FindDisplayedInspiration(inspirationId) : this);
		if ((object)obj != null)
		{
			obj.LockBuyButton();
			return obj;
		}
		return obj;
	}

	public void LockBuyButton()
	{
		isBuyLocked = true;
		SetBuyButtonInteractable(interactable: false);
	}

	public void UnlockBuyButton()
	{
		isBuyLocked = false;
		SetBuyButtonInteractable(data?.InspirationData != null && data.InspirationData.IsAvailableToBuy);
	}

	private void SetBuyButtonInteractable(bool interactable)
	{
		if (button != null)
		{
			button.interactable = interactable;
		}
	}

	public void ShowExpTooltip()
	{
		if (data != null && data.InspirationData != null && GameBalance.Me.GetData<InspirationDef>($"{data.InspirationData.id}_{data.CurrentLevel}").completionExp != 0 && !string.IsNullOrEmpty(data.TalentExpPointIconId))
		{
			UITooltip.ShowSimpleInfo(expPointTooltipRect.transform, LLBase.L("hint_insp_reward", data.TalentExpPointIconId.FontIcon()));
		}
	}

	public void HideExpTooltip()
	{
		if (UITooltip.IsTooltipShowingAtTarget(expPointTooltipRect))
		{
			UITooltip.Hide();
		}
	}

	private void OnDisable()
	{
		if (UITooltip.IsTooltipShowingAtTarget(expPointTooltipRect))
		{
			UITooltip.HideImmediately();
		}
	}

	protected override void TestDraw()
	{
	}
}
