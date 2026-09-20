using System;
using System.Collections.Generic;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIVendorWindow : LazyWindow<UIVendorWindowData>
{
	[SerializeField]
	private TextMeshProUGUI headerlLeft;

	[SerializeField]
	private TextMeshProUGUI headerlRight;

	[SerializeField]
	private MultiInventoryWidget playerInventoryWidget;

	[SerializeField]
	private MultiInventoryWidget vendorInventoryWidget;

	[SerializeField]
	private MoneyWidget playerMoneyWidget;

	[SerializeField]
	private MoneyWidget vendorMoneyWidget;

	[SerializeField]
	private MoneyWidget dealMoneyWidget;

	[SerializeField]
	private TextStyle enoughMoneyStyle;

	[SerializeField]
	private TextStyle notEnoughMoneyStyle;

	[SerializeField]
	private VendorDealInventoryWidget sellDealInventoryWidget;

	[SerializeField]
	private VendorDealInventoryWidget buyDealInventoryWidget;

	[SerializeField]
	private LazyButton cancelDealBtn;

	[SerializeField]
	private LazyButton applyDealBtn;

	[SerializeField]
	private Image vendorIcon;

	[SerializeField]
	private Color toReplace = new Color(1f, 1f, 1f, 0f);

	[SerializeField]
	private UIWorkerIcon playerIcon;

	[SerializeField]
	private TextMeshProUGUI happinessResultLabel;

	[SerializeField]
	private TextMeshProUGUI happinessVendorLabel;

	[SerializeField]
	private Slider happinessProgressBar;

	[SerializeField]
	private RectTransform green;

	[SerializeField]
	private RectTransform red;

	[SerializeField]
	private GameObject dealDecor;

	[SerializeField]
	private GameObject inventoryFullGamepadHint;

	private int moneyBefore;

	private readonly List<FlyingTechPoint> pendingHappinessFlights = new List<FlyingTechPoint>();

	public override void Init()
	{
		base.Init();
		applyDealBtn.onClick.AddListener(OnApplyDealBtnClicked);
		cancelDealBtn.onClick.AddListener(OnCancelDealBtnClicked);
		applyDealBtn.SetCallbacksIntoGamepadNavigationItem();
		cancelDealBtn.SetCallbacksIntoGamepadNavigationItem();
		UIMouseTooltip.Attach(happinessProgressBar.gameObject, "tt_trade_hap_bar", null, addRaycastTarget: true, disableChildRaycasts: true);
		UIMouseTooltip.Attach(happinessVendorLabel.transform.parent.gameObject, "tt_trade_hap_likes", null, addRaycastTarget: true, disableChildRaycasts: true);
	}

	public override void Open(UIVendorWindowData data)
	{
		base.Open(data);
		data.OnHappinessRewardGranted = PlayHappinessReward;
		moneyBefore = MainGame.PlayerData.GetResInt("money");
		UINotificator.isSilent = true;
		data.OnRedraw = RedrawLite;
		playerInventoryWidget.Draw(data.PlayerMultiInventoryWidgetData);
		vendorInventoryWidget.Draw(data.VendorMultiInventoryWidgetData);
		playerMoneyWidget.Draw(data.PlayerMoneyWidgetData);
		vendorMoneyWidget.Draw(data.VendorMoneyWidgetData);
		dealMoneyWidget.Draw(data.DealMoneyWidgetData);
		enoughMoneyStyle.ApplyStyle(dealMoneyWidget.MoneyLabel);
		sellDealInventoryWidget.Draw(data.DealSellInventoryWidgetData);
		buyDealInventoryWidget.Draw(data.DealBuyInventoryWidgetData);
		playerIcon.ShowWithoutTalent(MainGame.PlayerController, MainGame.PlayerController.View.PlayerAnimation.SkinPreset);
		headerlLeft.text = LLBase.L("ui_player");
		headerlRight.text = LLBase.L(data.Vendor.Definition.id);
		vendorIcon.sprite = data.Vendor.Definition.Icon;
		vendorIcon.enabled = vendorIcon.sprite != null;
		vendorIcon.BlueColorReplace(toReplace);
		UpdatePrices();
		UpdateButtons();
		UpdateHappiness();
		TryUpdateHappinessStatusIcons();
		UpdateDecor();
		UpdateInventoryFullGamepadHint();
		if (LazyInput.IsGamepadActive)
		{
			base.GamepadNavigationController.ReinitItems(focusOnFirstActive: true);
			UpdateGamepadDependentStuff();
		}
		((RectTransform)base.transform).RefreshContentFitter();
	}

	protected override void HideWindow()
	{
		if (data != null)
		{
			CompletePendingHappinessFlights();
			data.OnHappinessRewardGranted = null;
			data.OnWindowClosed?.Invoke();
			playerInventoryWidget.Hide();
			vendorInventoryWidget.Hide();
			buyDealInventoryWidget.Hide();
			sellDealInventoryWidget.Hide();
			UINotificator.isSilent = false;
			int resInt = MainGame.PlayerData.GetResInt("money");
			if (moneyBefore != resInt)
			{
				LazySingleton<UINotificator>.Instance.ShowMoneyNotification(resInt - moneyBefore);
			}
			base.HideWindow();
		}
	}

	public void RedrawLite()
	{
		playerInventoryWidget.Redraw();
		UpdateMoneyWidget();
		UpdatePrices();
		UpdateButtons();
		UpdateHappiness();
		TryUpdateHappinessStatusIcons();
		UpdateDecor();
		UpdateInventoryFullGamepadHint();
		if (LazyInput.IsGamepadActive)
		{
			UpdateGamepadDependentStuff();
		}
	}

	private void UpdatePrices()
	{
		playerInventoryWidget.UpdatePrices(data.PlayerInvPriceDelegate, 1);
		sellDealInventoryWidget.UpdatePrices(data.SellInvPriceDelegate, 0);
		vendorInventoryWidget.UpdatePrices(data.VendorInvPriceDelegate, 0);
		buyDealInventoryWidget.UpdatePrices(data.BuyInvPriceDelegate, 1);
	}

	private void TryUpdateHappinessStatusIcons()
	{
		if (data.Vendor.Definition.townVendor)
		{
			float extraUsedHappiness = data.GetTotalHappinessDealDelegate?.Invoke() ?? 0f;
			playerInventoryWidget.UpdateHappinessStatusIcons(data.Vendor, data.GetPendingHappinessSoldCount, extraUsedHappiness);
			vendorInventoryWidget.UpdateHappinessStatusIcons(data.Vendor, data.GetPendingHappinessSoldCount, extraUsedHappiness);
			sellDealInventoryWidget.UpdateHappinessStatusIcons(data.Vendor, data.GetPendingHappinessSoldCount, extraUsedHappiness);
			buyDealInventoryWidget.UpdateHappinessStatusIcons(data.Vendor, data.GetPendingHappinessSoldCount, extraUsedHappiness);
		}
	}

	private void UpdateMoneyWidget()
	{
		playerMoneyWidget.Redraw();
		vendorMoneyWidget.Redraw();
		dealMoneyWidget.Redraw();
		if (data.EnoughMoneyCondition())
		{
			enoughMoneyStyle.ApplyStyle(dealMoneyWidget.MoneyLabel);
		}
		else
		{
			notEnoughMoneyStyle.ApplyStyle(dealMoneyWidget.MoneyLabel);
		}
	}

	private void UpdateHappiness()
	{
		if (data.Vendor.Definition.townVendor)
		{
			happinessResultLabel.gameObject.SetActive(value: true);
			happinessVendorLabel.gameObject.SetActive(value: true);
			happinessProgressBar.transform.parent.gameObject.SetActive(value: true);
			float num = Mathf.Max(0f, data.GetTotalHappinessDealDelegate());
			float num2 = Mathf.Floor(num * 100f) / 100f;
			happinessResultLabel.text = string.Format("+{0}{1:0.##}", "happiness".FontIcon(), num2);
			happinessVendorLabel.text = string.Format("{0}{1}", "happiness".FontIcon(), data.Vendor.CurrentTierData.happinessCap.EvaluateInt() - (int)data.Vendor.UsedHappinessThisWeek);
			float num3 = data.Vendor.UsedHappinessThisWeek - Mathf.Floor(data.Vendor.UsedHappinessThisWeek);
			float num4 = Mathf.Clamp01(num3 + num);
			happinessProgressBar.value = num4;
			Canvas.ForceUpdateCanvases();
			float width = happinessProgressBar.fillRect.rect.width;
			red.sizeDelta = new Vector2(0f, red.sizeDelta.y);
			float num5 = num4 - num3;
			float x = ((num4 > 0f) ? (Mathf.Clamp01(num5 / num4) * width) : 0f);
			green.sizeDelta = new Vector2(x, green.sizeDelta.y);
			if (data.EnoughHappinessCondition())
			{
				enoughMoneyStyle.ApplyStyle(happinessResultLabel);
			}
			else
			{
				notEnoughMoneyStyle.ApplyStyle(happinessResultLabel);
			}
		}
		else
		{
			happinessResultLabel.gameObject.SetActive(value: false);
			happinessVendorLabel.gameObject.SetActive(value: false);
			happinessProgressBar.transform.parent.gameObject.SetActive(value: false);
		}
	}

	private void UpdateDecor()
	{
		dealDecor.SetActive(string.IsNullOrEmpty(dealMoneyWidget.MoneyLabel.text) && !happinessVendorLabel.gameObject.activeSelf);
	}

	private void UpdateInventoryFullGamepadHint()
	{
		if (!(inventoryFullGamepadHint == null))
		{
			bool active = LazyInput.IsGamepadActive && data != null && data.PlayerInventoryCanAcceptBuyItemsCondition != null && !data.PlayerInventoryCanAcceptBuyItemsCondition();
			inventoryFullGamepadHint.SetActive(active);
		}
	}

	private void UpdateButtons()
	{
		cancelDealBtn.interactable = data.CancelButtonInteractableCondition();
		applyDealBtn.interactable = data.ApplyButtonInteractableCondition();
	}

	private void OnApplyDealBtnClicked()
	{
		data.OnApplyDealBtnClicked?.Invoke();
	}

	private void PlayHappinessReward(int amount)
	{
		if (amount <= 0)
		{
			return;
		}
		if (GUIElements.Instance.UIWindowSizeType != 0)
		{
			TechPointsSpawner.CreateSpawner(MainGame.PlayerController.MovablePosition, 0, 0, 0, amount);
			return;
		}
		Vector3 happinessIconWorldPosition = GetHappinessIconWorldPosition();
		int overrodeSorting = canvas.sortingOrder + 1;
		for (int i = 0; i < amount; i++)
		{
			FlyingTechPoint drop = null;
			drop = FlyingTechPoint.DropFromUI(happinessIconWorldPosition, TechDef.FlyingReses[3], delegate
			{
				pendingHappinessFlights.Remove(drop);
			}, overrodeSorting);
			pendingHappinessFlights.Add(drop);
			LazyAudio.Play("tech_point_collect");
		}
	}

	private Vector3 GetHappinessIconWorldPosition()
	{
		if (happinessResultLabel == null)
		{
			return base.transform.position;
		}
		happinessResultLabel.ForceMeshUpdate();
		TMP_TextInfo textInfo = happinessResultLabel.textInfo;
		if (textInfo != null)
		{
			for (int i = 0; i < textInfo.characterCount; i++)
			{
				TMP_CharacterInfo tMP_CharacterInfo = textInfo.characterInfo[i];
				if (tMP_CharacterInfo.isVisible && tMP_CharacterInfo.elementType == TMP_TextElementType.Sprite)
				{
					Vector3 position = (tMP_CharacterInfo.bottomLeft + tMP_CharacterInfo.topRight) * 0.5f;
					return happinessResultLabel.transform.TransformPoint(position);
				}
			}
		}
		return happinessResultLabel.transform.position;
	}

	private void CompletePendingHappinessFlights()
	{
		if (pendingHappinessFlights.Count == 0)
		{
			return;
		}
		FlyingTechPoint[] array = pendingHappinessFlights.ToArray();
		pendingHappinessFlights.Clear();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != null)
			{
				array[i].CompleteImmediately();
			}
		}
	}

	private void OnCancelDealBtnClicked()
	{
		data.OnCancelBtnClicked?.Invoke();
	}

	private bool OnItemMovePressed()
	{
		if (LazyInput.IsGamepadActive)
		{
			GamepadNavigationItem focusedItem = base.GamepadNavigationController.FocusedItem;
			if (focusedItem != null && focusedItem.TryGetComponent<UIItemCell>(out var component) && component.DisplayingItem != null && !component.DisplayingItem.IsEmpty)
			{
				component.OnGamepadPress2();
			}
		}
		return true;
	}

	protected override bool OnPressedBack()
	{
		if (cancelDealBtn.interactable)
		{
			OnCancelDealBtnClicked();
			return true;
		}
		return base.OnPressedBack();
	}

	protected override void UpdateGamepadDependentStuff()
	{
		LazyPlatformDependentElement[] componentsInChildren = GetComponentsInChildren<LazyPlatformDependentElement>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Init();
		}
		LazyGamepadDependentElement[] componentsInChildren2 = GetComponentsInChildren<LazyGamepadDependentElement>(includeInactive: true);
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			componentsInChildren2[i].UpdateState();
		}
		UpdateInventoryFullGamepadHint();
		if (LazyInput.IsGamepadActive)
		{
			if ((bool)closeButton)
			{
				closeButton.gameObject.SetActive(value: false);
			}
			for (int j = 0; j < playerInventoryWidget.DrawnInventories.Count; j++)
			{
				InventoryWidget inventoryWidget = playerInventoryWidget.DrawnInventories[j];
				for (int k = 0; k < inventoryWidget.Cells.Count; k++)
				{
					inventoryWidget.Cells[k].GamepadNavigationItem.group = 0;
				}
			}
			for (int l = 0; l < vendorInventoryWidget.DrawnInventories.Count; l++)
			{
				InventoryWidget inventoryWidget2 = vendorInventoryWidget.DrawnInventories[l];
				for (int m = 0; m < inventoryWidget2.Cells.Count; m++)
				{
					inventoryWidget2.Cells[m].GamepadNavigationItem.group = 5;
				}
			}
			for (int n = 0; n < sellDealInventoryWidget.Cells.Count; n++)
			{
				sellDealInventoryWidget.Cells[n].GamepadNavigationItem.group = 1;
			}
			for (int num = 0; num < buyDealInventoryWidget.Cells.Count; num++)
			{
				buyDealInventoryWidget.Cells[num].GamepadNavigationItem.group = 2;
			}
			PrintTips(base.GamepadNavigationController.FocusedItem);
			ChangeTipsState(active: true);
		}
		else
		{
			if ((bool)closeButton)
			{
				closeButton.gameObject.SetActive(value: true);
			}
			ChangeTipsState(active: false);
		}
	}

	protected override Dictionary<GameKey, Func<bool>> GetGameKeyDelegates()
	{
		Dictionary<GameKey, Func<bool>> gameKeyDelegates = base.GetGameKeyDelegates();
		gameKeyDelegates.Add(GameKey.ItemMove, OnItemMovePressed);
		gameKeyDelegates.Add(GameKey.AcceptVendorDeal, delegate
		{
			OnApplyDealBtnClicked();
			return true;
		});
		return gameKeyDelegates;
	}

	protected override void PrintTips(GamepadNavigationItem gamepadNavigationItem)
	{
		List<LazyGameKeyTip> list = new List<LazyGameKeyTip>();
		if (gamepadNavigationItem != null && gamepadNavigationItem.TryGetComponent<UIItemCell>(out var component))
		{
			if (component.DisplayingItem != null && !component.DisplayingItem.IsEmpty && component.OnItemCellPress != null)
			{
				list.Add(LazyGameKeyTip.Select());
			}
			if (component.DisplayingItem != null && !component.DisplayingItem.IsEmpty && component.IsInteractable && component.OnItemCellPress2 != null)
			{
				list.Add(new LazyGameKeyTip(GameKey.ItemMove, "tip_item_action"));
			}
		}
		else
		{
			list.Add(LazyGameKeyTip.Select());
		}
		if (applyDealBtn.interactable)
		{
			list.Add(new LazyGameKeyTip(GameKey.AcceptVendorDeal, "tip_accept"));
		}
		if ((bool)closeButton)
		{
			if (cancelDealBtn.interactable)
			{
				list.Add(new LazyGameKeyTip(GameKey.Back, "tip_cancel"));
			}
			else
			{
				list.Add(LazyGameKeyTip.Back());
			}
		}
		lazyButtonTips.Print(list);
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		Trading trading = new Trading();
		UIVendorWindowData vendorWindowData = new UIVendorWindowData();
		trading.FillVendorWindowData(vendorWindowData, "npc_herm", null);
		LazyUI.GetWindow<UIVendorWindow>().Open(vendorWindowData);
	}

	[LazyUITest]
	protected void TestTownVendorDraw()
	{
		Trading trading = new Trading();
		UIVendorWindowData vendorWindowData = new UIVendorWindowData();
		trading.FillVendorWindowData(vendorWindowData, "test_town_vendor", null);
		LazyUI.GetWindow<UIVendorWindow>().Open(vendorWindowData);
	}
}
