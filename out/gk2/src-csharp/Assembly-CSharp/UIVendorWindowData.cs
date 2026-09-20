using System;
using LazyBearTechnology;

public class UIVendorWindowData : LazyWidgetDataBase
{
	public Vendor Vendor { get; set; }

	public MultiInventoryWidgetData PlayerMultiInventoryWidgetData { get; set; }

	public MultiInventoryWidgetData VendorMultiInventoryWidgetData { get; set; }

	public MoneyWidgetData PlayerMoneyWidgetData { get; set; }

	public MoneyWidgetData VendorMoneyWidgetData { get; set; }

	public MoneyWidgetData DealMoneyWidgetData { get; set; }

	public InventoryWidgetData DealBuyInventoryWidgetData { get; set; }

	public InventoryWidgetData DealSellInventoryWidgetData { get; set; }

	public Action OnApplyDealBtnClicked { get; set; }

	public Action OnCancelBtnClicked { get; set; }

	public Func<bool> ApplyButtonInteractableCondition { get; set; }

	public Func<bool> CancelButtonInteractableCondition { get; set; }

	public Func<bool> EnoughMoneyCondition { get; set; }

	public Func<bool> EnoughHappinessCondition { get; set; }

	public Func<bool> PlayerInventoryCanAcceptBuyItemsCondition { get; set; }

	public Func<float> GetTotalHappinessDealDelegate { get; set; }

	public Func<string, int> GetPendingHappinessSoldCount { get; set; }

	public Action<int> OnHappinessRewardGranted { get; set; }

	public InventoryWidgetBase<InventoryWidgetData>.ItemPriceDelegate PlayerInvPriceDelegate { get; set; }

	public InventoryWidgetBase<InventoryWidgetData>.ItemPriceDelegate VendorInvPriceDelegate { get; set; }

	public InventoryWidgetBase<InventoryWidgetData>.ItemPriceDelegate SellInvPriceDelegate { get; set; }

	public InventoryWidgetBase<InventoryWidgetData>.ItemPriceDelegate BuyInvPriceDelegate { get; set; }

	public Action OnRedraw { get; set; }

	public Action OnWindowClosed { get; set; }
}
