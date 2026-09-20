using System;
using System.Collections.Generic;
using System.Text;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class Trading
{
	private UIVendorWindowData cachedWindowData;

	private Inventory buyInventory;

	private Inventory sellInventory;

	private int PlayerMoney
	{
		get
		{
			return MainGame.PlayerData.GetResInt("money");
		}
		set
		{
			MainGame.PlayerData.SetRes("money", value);
		}
	}

	private float PlayerHappiness
	{
		get
		{
			return MainGame.PlayerData.GetRes("happiness");
		}
		set
		{
			MainGame.PlayerData.SetRes("happiness", value);
		}
	}

	public void FillVendorWindowData(UIVendorWindowData vendorWindowData, string vendorId, Action onClosed)
	{
		Vendor vendor = MainGame.Instance.GameSave.vendorSystem.GetVendor(vendorId);
		if (vendor == null)
		{
			Debug.LogError("Can't open vendor window with id:[" + vendorId + "] no such vendor.");
			onClosed?.Invoke();
			return;
		}
		cachedWindowData = vendorWindowData;
		buyInventory = new Inventory("inventory", 6);
		sellInventory = new Inventory("inventory", 6);
		cachedWindowData.Vendor = vendor;
		cachedWindowData.PlayerMoneyWidgetData = new MoneyWidgetData();
		cachedWindowData.PlayerMoneyWidgetData.Money = () => PlayerMoney;
		cachedWindowData.VendorMoneyWidgetData = new MoneyWidgetData();
		cachedWindowData.VendorMoneyWidgetData.Money = () => vendor.CurMoney;
		cachedWindowData.DealMoneyWidgetData = new MoneyWidgetData();
		cachedWindowData.DealMoneyWidgetData.Money = GetTotalDealPrice;
		cachedWindowData.GetTotalHappinessDealDelegate = GetTotalDealHappiness;
		cachedWindowData.GetPendingHappinessSoldCount = GetPendingHappinessSoldCount;
		cachedWindowData.PlayerMultiInventoryWidgetData = new MultiInventoryWidgetData();
		List<InventoryWidgetDataBase> widgetsDataForInventory = InventoryWidgetDataHelper.GetWidgetsDataForInventory(MainGame.PlayerData.inventory, delegate
		{
			LazyAudio.PlayAndForget("gui_hover_light");
		}, null, OnPlayerItemPress1, OnPlayerItemPress2, null, PlayerItemsAvailableCondition);
		for (int i = 0; i < widgetsDataForInventory.Count; i++)
		{
			widgetsDataForInventory[i].DrawEmptyCellsAsDisabledWhenUnavailable = true;
		}
		cachedWindowData.PlayerMultiInventoryWidgetData.AddRange(widgetsDataForInventory);
		SortVendorInventory(null);
		vendor.Inventory.OnItemsAdd += SortVendorInventory;
		cachedWindowData.OnWindowClosed = delegate
		{
			vendor.Inventory.OnItemsAdd -= SortVendorInventory;
			ResetDeal(triggerOnRedraw: false);
			onClosed?.Invoke();
			onClosed = null;
		};
		InventoryWidgetData widgetData = new InventoryWidgetData(vendor.Inventory, new InventoryHeaderWidgetData(vendor.Inventory, $"comm-header_2-type_icon-star_tier_{vendor.CurTier}", $"ui_vendor_tier_{vendor.CurTier}"), delegate
		{
			LazyAudio.PlayAndForget("gui_hover_light");
		}, null, OnVendorItemPress1, OnVendorItemPress2, null, VendorItemsAvailableCondition, VendorItemsNotShowCondition);
		cachedWindowData.VendorMultiInventoryWidgetData = new MultiInventoryWidgetData();
		cachedWindowData.VendorMultiInventoryWidgetData.Add(widgetData);
		if (vendor.Definition.tierDataList.Count > vendor.CurTier)
		{
			TryFormFakeInventoryForTier(vendor.NextTierData, vendor.CurTier + 1);
		}
		if (vendor.Definition.tierDataList.Count > vendor.CurTier + 1)
		{
			TryFormFakeInventoryForTier(vendor.Definition.tierDataList[vendor.CurTier + 1], vendor.CurTier + 2);
		}
		cachedWindowData.DealBuyInventoryWidgetData = new InventoryWidgetData(buyInventory, new InventoryHeaderWidgetData(buyInventory), delegate
		{
			LazyAudio.PlayAndForget("gui_hover_light");
		}, null, OnBuyInventoryItemPress1, OnBuyInventoryItemPress2, null);
		cachedWindowData.DealSellInventoryWidgetData = new InventoryWidgetData(sellInventory, new InventoryHeaderWidgetData(sellInventory), delegate
		{
			LazyAudio.PlayAndForget("gui_hover_light");
		}, null, OnSellInventoryItemPress1, OnSellInventoryItemPress2, null);
		cachedWindowData.OnApplyDealBtnClicked = DoAcceptDeal;
		cachedWindowData.OnCancelBtnClicked = delegate
		{
			ResetDeal(triggerOnRedraw: true);
		};
		cachedWindowData.ApplyButtonInteractableCondition = () => CanAcceptDeal() && !IsDealEmpty();
		cachedWindowData.CancelButtonInteractableCondition = () => !IsDealEmpty();
		cachedWindowData.EnoughMoneyCondition = () => EnoughMoney();
		cachedWindowData.EnoughHappinessCondition = () => EnoughHappiness();
		cachedWindowData.PlayerInventoryCanAcceptBuyItemsCondition = CanPlayerInventoryAcceptBuyItems;
		cachedWindowData.PlayerInvPriceDelegate = GetSingleItemCostInPlayerInventory;
		cachedWindowData.VendorInvPriceDelegate = GetSingleItemCostInTraderInventory;
		cachedWindowData.SellInvPriceDelegate = GetSingleItemCostInPlayerInventory;
		cachedWindowData.BuyInvPriceDelegate = GetSingleItemCostInTraderInventory;
		void TryFormFakeInventoryForTier(VendorTierData tierData, int tier)
		{
			if (vendor.Definition.tierDataList.Count > vendor.CurTier)
			{
				List<string> list = new List<string>();
				for (int j = 0; j < tierData.vendorProducts.Count; j++)
				{
					VendorProductData vendorProductData = tierData.vendorProducts[j];
					if (!vendor.Inventory.Data.HasItemQuantityInInventory(vendorProductData.itemId, 1) && tierData.newProducts.Contains(vendorProductData.itemId))
					{
						list.Add(vendorProductData.itemId);
					}
				}
				if (list.Count > 0)
				{
					int num = 5 - list.Count % 5;
					for (int k = 0; k < num; k++)
					{
						list.Add("empty");
					}
					Inventory inventory = new Inventory("inventory", list.Count);
					for (int l = 0; l < list.Count; l++)
					{
						inventory.AddItemToInventory(new Item(list[l], (!(list[l] == "empty")) ? 1 : (-1)));
					}
					InventoryWidgetData widgetData2 = new InventoryWidgetData(inventory, new InventoryHeaderWidgetData(vendor.Inventory, $"comm-header_2-type_icon-star_tier_{tier}", $"ui_vendor_tier_{tier}"), delegate
					{
						LazyAudio.PlayAndForget("gui_hover_light");
					}, null, null, null, null, (Item _) => false, null, ItemRelatedWidgetState.Disabled);
					cachedWindowData.VendorMultiInventoryWidgetData.Add(widgetData2);
				}
			}
		}
	}

	private int GetSingleItemCostInTraderInventory(Item item, int countModificator = 0)
	{
		int itemsCount = cachedWindowData.Vendor.Inventory.Data.GetTotalCountInInventory(item.id) + countModificator;
		return Mathf.RoundToInt(Mathf.Round((float)cachedWindowData.Vendor.CurPrice(item.id, buy: true, itemsCount) * 100f) / 100f);
	}

	private int GetSingleItemCostInTraderInventory(string itemID, int countModificator = 0)
	{
		return GetSingleItemCostInTraderInventory(new Item(itemID, cachedWindowData.Vendor.Inventory.Data.GetTotalCountInInventory(itemID)), countModificator);
	}

	private int GetSingleItemCostInPlayerInventory(Item item, int countModificator = 0)
	{
		int itemsCount = cachedWindowData.Vendor.Inventory.Data.GetTotalCountInInventory(item.id) + sellInventory.Data.GetTotalCountInInventory(item.id) + countModificator;
		float num = cachedWindowData.Vendor.CurPrice(item.id, buy: false, itemsCount);
		if (num > (float)item.Definition.basePrice)
		{
			num = item.Definition.basePrice;
		}
		return Mathf.RoundToInt(Mathf.Round(num * 100f) / 100f);
	}

	private int GetSingleItemCostInPlayerInventory(string itemID, int countModificator = 0)
	{
		return GetSingleItemCostInPlayerInventory(new Item(itemID, MainGame.PlayerData.inventory.Data.GetTotalCountInInventory(itemID)), countModificator);
	}

	private bool IsDealEmpty()
	{
		if (buyInventory.Data.InventoryCount <= 0)
		{
			return sellInventory.Data.InventoryCount <= 0;
		}
		return false;
	}

	private bool CanPlayerInventoryAcceptBuyItems()
	{
		return MainGame.PlayerData.inventory.CanAddItemsToInventory(buyInventory);
	}

	private bool CanAcceptDeal()
	{
		if (!CanPlayerInventoryAcceptBuyItems())
		{
			return false;
		}
		if (!cachedWindowData.Vendor.Inventory.CanAddItemsToInventory(sellInventory))
		{
			return false;
		}
		float num = GetTotalDealPrice();
		if ((float)PlayerMoney + num < 0f)
		{
			return false;
		}
		if ((float)cachedWindowData.Vendor.CurMoney - num < 0f)
		{
			return false;
		}
		if (!EnoughHappiness())
		{
			return false;
		}
		return true;
	}

	private bool EnoughMoney()
	{
		float num = GetTotalDealPrice();
		if ((float)PlayerMoney + num < 0f)
		{
			return false;
		}
		if ((float)cachedWindowData.Vendor.CurMoney - num < 0f)
		{
			return false;
		}
		return true;
	}

	private bool EnoughHappiness()
	{
		float totalDealHappiness = GetTotalDealHappiness();
		if (totalDealHappiness >= 0f)
		{
			return true;
		}
		float num = (float)(int)cachedWindowData.Vendor.UsedHappinessThisWeek - cachedWindowData.Vendor.UsedHappinessThisWeek;
		return !(totalDealHappiness < num);
	}

	private void DoAcceptDeal()
	{
		if (!CanAcceptDeal())
		{
			return;
		}
		int totalDealPrice = GetTotalDealPrice();
		float totalDealHappiness = GetTotalDealHappiness();
		PlayerMoney += totalDealPrice;
		float num = totalDealHappiness;
		foreach (Item item in sellInventory.Data.Inventory)
		{
			foreach (LazyExpression item2 in item.Definition.expressionsOnSell)
			{
				item2.Evaluate(item);
			}
			TownVendorProductInfo townVendorProductInfo = cachedWindowData.Vendor.CurrentTierData.GetTownVendorProductInfo(item.id);
			if (townVendorProductInfo != null && totalDealHappiness > 0f && num > 0f)
			{
				int num2 = item.Count;
				while (num > 0f && num2 > 0)
				{
					num -= townVendorProductInfo.perOne;
					num2--;
					cachedWindowData.Vendor.SoldItemsWithHappinessThisWeek.Add(item.id, 1f);
				}
			}
		}
		foreach (Item item3 in buyInventory.Data.Inventory)
		{
			foreach (LazyExpression item4 in item3.Definition.expressionsOnBuy)
			{
				item4.Evaluate(item3);
			}
		}
		if (totalDealHappiness > 0f)
		{
			int num3 = (int)cachedWindowData.Vendor.UsedHappinessThisWeek;
			cachedWindowData.Vendor.UsedHappinessThisWeek += totalDealHappiness;
			int num4 = (int)cachedWindowData.Vendor.UsedHappinessThisWeek;
			if (num4 > num3)
			{
				int num5 = num4 - num3;
				if (cachedWindowData.OnHappinessRewardGranted != null)
				{
					cachedWindowData.OnHappinessRewardGranted(num5);
				}
				else
				{
					TechPointsSpawner.CreateSpawner(MainGame.PlayerController.MovablePosition, 0, 0, 0, num5);
				}
			}
		}
		cachedWindowData.Vendor.CurMoney -= totalDealPrice;
		if (!cachedWindowData.Vendor.Inventory.AddItemsToInventory(sellInventory))
		{
			Debug.LogError("Can not add player's deal to vendor's inventory");
			return;
		}
		foreach (Item item5 in buyInventory.Data.Inventory)
		{
			if (!MainGame.PlayerData.inventory.AddItemToInventory(item5))
			{
				Debug.LogError("Can not add vendor's deaf's item \"" + item5.id + "\" to players's inventory");
			}
		}
		sellInventory.Clear();
		buyInventory.Clear();
		Debug.Log("Accepted deal!");
		cachedWindowData.OnRedraw?.Invoke();
	}

	private void ResetDeal(bool triggerOnRedraw)
	{
		if (sellInventory.Data.InventoryCount > 0)
		{
			MainGame.PlayerData.inventory.AddItemsToInventory(sellInventory);
			sellInventory.Clear();
		}
		if (buyInventory.Data.InventoryCount > 0)
		{
			cachedWindowData.Vendor.Inventory.AddItemsToInventory(buyInventory);
			buyInventory.Clear();
		}
		if (triggerOnRedraw)
		{
			cachedWindowData.OnRedraw?.Invoke();
		}
	}

	private int GetTotalDealPrice()
	{
		int num = 0;
		List<string> list = new List<string>();
		foreach (Item item in sellInventory.Data.Inventory)
		{
			if (!list.Contains(item.id))
			{
				int totalCountInInventory = sellInventory.Data.GetTotalCountInInventory(item.id);
				for (int i = 0; i < totalCountInInventory; i++)
				{
					num += GetSingleItemCostInPlayerInventory(item, -i);
				}
				list.Add(item.id);
			}
		}
		int num2 = 0;
		list = new List<string>();
		foreach (Item item2 in buyInventory.Data.Inventory)
		{
			if (!list.Contains(item2.id))
			{
				int totalCountInInventory2 = buyInventory.Data.GetTotalCountInInventory(item2.id);
				for (int j = 0; j < totalCountInInventory2; j++)
				{
					num2 += GetSingleItemCostInTraderInventory(item2, j + 1);
				}
				list.Add(item2.id);
			}
		}
		return num - num2;
	}

	private int GetPendingHappinessSoldCount(string itemId)
	{
		if (string.IsNullOrEmpty(itemId))
		{
			return 0;
		}
		return sellInventory.Data.GetTotalCountInInventory(itemId) - buyInventory.Data.GetTotalCountInInventory(itemId);
	}

	private float GetTotalDealHappiness()
	{
		if (!cachedWindowData.Vendor.Definition.townVendor)
		{
			return 0f;
		}
		float num = 0f;
		GameRes gameRes = new GameRes();
		foreach (Item item in sellInventory.Data.Inventory)
		{
			TownVendorProductInfo townVendorProductInfo = cachedWindowData.Vendor.CurrentTierData.GetTownVendorProductInfo(item.id);
			int @int = cachedWindowData.Vendor.SoldItemsWithHappinessThisWeek.GetInt(item.id);
			if (townVendorProductInfo != null && @int < townVendorProductInfo.itemCount)
			{
				gameRes.Add(item.id, item.Count);
			}
		}
		foreach (Item item2 in buyInventory.Data.Inventory)
		{
			if (cachedWindowData.Vendor.CurrentTierData.GetTownVendorProductInfo(item2.id) != null)
			{
				gameRes.Sub(item2.id, item2.Count);
			}
		}
		for (int i = 0; i < gameRes.List.Count; i++)
		{
			GameResAtom gameResAtom = gameRes.List[i];
			int int2 = cachedWindowData.Vendor.SoldItemsWithHappinessThisWeek.GetInt(gameResAtom.type);
			TownVendorProductInfo townVendorProductInfo2 = cachedWindowData.Vendor.CurrentTierData.GetTownVendorProductInfo(gameResAtom.type);
			if (!(gameResAtom.value <= 0f))
			{
				int num2 = Math.Clamp((int)gameResAtom.value, 0, townVendorProductInfo2.itemCount - int2);
				num += (float)num2 * townVendorProductInfo2.perOne;
			}
		}
		return Math.Clamp(num, 0f, cachedWindowData.Vendor.CurrentTierData.happinessCap.EvaluateFloat() - cachedWindowData.Vendor.UsedHappinessThisWeek);
	}

	private bool PlayerItemsAvailableCondition(Item item)
	{
		if (item == null || item.IsEmpty)
		{
			return false;
		}
		return cachedWindowData.Vendor.CanBuyItemFromPlayer(item.Definition);
	}

	private void OnPlayerItemPress1(UIItemCell itemCell)
	{
		if (itemCell.DisplayingItem.Count <= 1)
		{
			OnPlayerItemPress2(itemCell);
			return;
		}
		OpenItemCountWindow(itemCell, MainGame.PlayerData.inventory, sellInventory, delegate(int amount)
		{
			int num = 0;
			for (int i = 0; i < amount; i++)
			{
				num += GetSingleItemCostInPlayerInventory(itemCell.DisplayingItem.id, i + 1);
			}
			return num;
		});
	}

	private void OnPlayerItemPress2(UIItemCell itemCell)
	{
		if (TryMoveItem(itemCell, 1, MainGame.PlayerData.inventory, sellInventory))
		{
			LazyAudio.PlayAndForget("item_put");
		}
	}

	private bool VendorItemsAvailableCondition(Item item)
	{
		if (item == null || item.IsEmpty)
		{
			return false;
		}
		if (!cachedWindowData.Vendor.CanSellItemToPlayer(item.Definition))
		{
			return false;
		}
		if (item.IsSeed && cachedWindowData.Vendor.Inventory.Data.GetTotalCountInInventory(item.id) < 4)
		{
			return false;
		}
		return true;
	}

	private bool VendorItemsNotShowCondition(Item item)
	{
		if (item == null || item.IsEmpty)
		{
			return true;
		}
		return !cachedWindowData.Vendor.CurrentTierData.HasProduct(item.id);
	}

	private void OnVendorItemPress1(UIItemCell itemCell)
	{
		int purchaseMoveCount = GetPurchaseMoveCount(itemCell.DisplayingItem, cachedWindowData.Vendor.Inventory);
		if (purchaseMoveCount <= 0)
		{
			return;
		}
		if (itemCell.DisplayingItem.Count <= purchaseMoveCount)
		{
			OnVendorItemPress2(itemCell);
			return;
		}
		OpenItemCountWindow(itemCell, cachedWindowData.Vendor.Inventory, buyInventory, delegate(int amount)
		{
			int num = 0;
			for (int i = 0; i < amount; i++)
			{
				num += GetSingleItemCostInTraderInventory(itemCell.DisplayingItem.id, -i);
			}
			return num;
		});
	}

	private void OnVendorItemPress2(UIItemCell itemCell)
	{
		int purchaseMoveCount = GetPurchaseMoveCount(itemCell.DisplayingItem, cachedWindowData.Vendor.Inventory);
		if (purchaseMoveCount > 0 && TryMoveItem(itemCell, purchaseMoveCount, cachedWindowData.Vendor.Inventory, buyInventory))
		{
			LazyAudio.PlayAndForget("item_put");
		}
	}

	private void OnBuyInventoryItemPress1(UIItemCell itemCell)
	{
		int purchaseMoveCount = GetPurchaseMoveCount(itemCell.DisplayingItem, buyInventory);
		if (purchaseMoveCount <= 0)
		{
			return;
		}
		if (itemCell.DisplayingItem.Count <= purchaseMoveCount)
		{
			OnBuyInventoryItemPress2(itemCell);
			return;
		}
		OpenItemCountWindow(itemCell, buyInventory, cachedWindowData.Vendor.Inventory, delegate(int amount)
		{
			int num = 0;
			for (int i = 0; i < amount; i++)
			{
				num += GetSingleItemCostInPlayerInventory(itemCell.DisplayingItem.id, -i);
			}
			return num;
		});
	}

	private void OnBuyInventoryItemPress2(UIItemCell itemCell)
	{
		int purchaseMoveCount = GetPurchaseMoveCount(itemCell.DisplayingItem, buyInventory);
		if (purchaseMoveCount > 0)
		{
			TryMoveItem(itemCell, purchaseMoveCount, buyInventory, cachedWindowData.Vendor.Inventory);
		}
	}

	private void OnSellInventoryItemPress1(UIItemCell itemCell)
	{
		if (itemCell.DisplayingItem.Count <= 1)
		{
			OnSellInventoryItemPress2(itemCell);
			return;
		}
		OpenItemCountWindow(itemCell, sellInventory, MainGame.PlayerData.inventory, delegate(int amount)
		{
			int num = 0;
			for (int i = 0; i < amount; i++)
			{
				num += GetSingleItemCostInTraderInventory(itemCell.DisplayingItem.id, i + 1);
			}
			return num;
		});
	}

	private void OnSellInventoryItemPress2(UIItemCell itemCell)
	{
		TryMoveItem(itemCell, 1, sellInventory, MainGame.PlayerData.inventory);
	}

	private void OpenItemCountWindow(UIItemCell itemCell, Inventory from, Inventory to, UIItemCountWindowData.PriceCalculateDelegate priceCalculateDelegate)
	{
		if (to.CanAddItemToInventory(itemCell.DisplayingItem))
		{
			UIItemCountWindowData uIItemCountWindowData = new UIItemCountWindowData();
			uIItemCountWindowData.Item = new Item(itemCell.DisplayingItem.id);
			uIItemCountWindowData.Min = 1;
			int totalCountInInventory = from.Data.GetTotalCountInInventory(itemCell.DisplayingItem.id);
			uIItemCountWindowData.Max = to.Data.CanAddItemCountToInventory(itemCell.DisplayingItem.Definition, totalCountInInventory);
			uIItemCountWindowData.OnConfirm = delegate(int count)
			{
				TryMoveItem(itemCell, count, from, to);
			};
			uIItemCountWindowData.PriceCalculateDel = priceCalculateDelegate;
			uIItemCountWindowData.IsForVendor = true;
			uIItemCountWindowData.SnapStep = GetPurchaseSnapStep(itemCell.DisplayingItem, from, to);
			uIItemCountWindowData.OkBtnData = new UIDialogWindowData.ButtonData(null, LLBase.L("btn_ok"), null, replaceForGamepad: true, GameKey.Select);
			uIItemCountWindowData.BackBtnData = new UIDialogWindowData.ButtonData(null, LLBase.L("btn_cancel"), null, replaceForGamepad: true, GameKey.Back);
			if (uIItemCountWindowData.SnapStep <= 1 || uIItemCountWindowData.Max >= uIItemCountWindowData.SnapStep)
			{
				LazyAudio.PlayAndForget("item_put");
				LazyUI.GetWindow<UIItemCountWindow>().Open(uIItemCountWindowData);
			}
		}
	}

	private bool TryMoveItem(UIItemCell itemCell, int count, Inventory from, Inventory to)
	{
		if (to.AddItemToInventory(new Item(itemCell.DisplayingItem.id, count)))
		{
			from.RemoveItemById(itemCell.DisplayingItem.id, count, null, from.TryFindSourceBagForItem(itemCell.DisplayingItem));
			cachedWindowData.OnRedraw?.Invoke();
			return true;
		}
		return false;
	}

	private int GetPurchaseSnapStep(Item item, Inventory from, Inventory to)
	{
		if (item == null || !item.IsSeed)
		{
			return 1;
		}
		bool num = from == cachedWindowData.Vendor.Inventory && to == buyInventory;
		bool flag = from == buyInventory && to == cachedWindowData.Vendor.Inventory;
		if (!(num || flag))
		{
			return 1;
		}
		return 4;
	}

	private int GetPurchaseMoveCount(Item item, Inventory source)
	{
		if (item == null || item.IsEmpty)
		{
			return 0;
		}
		if (!item.IsSeed)
		{
			return 1;
		}
		int totalCountInInventory = source.Data.GetTotalCountInInventory(item.id);
		int num = 4;
		if (totalCountInInventory >= num)
		{
			return num;
		}
		if (source != buyInventory)
		{
			return 0;
		}
		return totalCountInInventory;
	}

	private void SortVendorInventory(List<Item> itemsChanged)
	{
		cachedWindowData.Vendor.Inventory.Sort(delegate(Item x, Item y)
		{
			VendorProductData product = cachedWindowData.Vendor.CurrentTierData.GetProduct(x.id);
			VendorProductData product2 = cachedWindowData.Vendor.CurrentTierData.GetProduct(y.id);
			if (product == null)
			{
				if (product2 == null)
				{
					return 0;
				}
				return -1;
			}
			if (product2 == null)
			{
				return 1;
			}
			int num = cachedWindowData.Vendor.CurrentTierData.vendorProducts.IndexOf(product);
			int value = cachedWindowData.Vendor.CurrentTierData.vendorProducts.IndexOf(product2);
			return num.CompareTo(value);
		});
	}

	public static string FormatMoney(int value, bool printZero = false, string delimiter = " ", GameResIconType iconType = null)
	{
		if ((object)iconType == null)
		{
			iconType = GameResIconType.Common;
		}
		StringBuilder stringBuilder = new StringBuilder();
		bool num = value < 0;
		value = Math.Abs(value);
		int num2 = value / 10000;
		int num3 = (value - num2 * 10000) / 100;
		int num4 = value - num2 * 10000 - num3 * 100;
		GameResIconConfig configForRes = GameResDisplayConfig.GetConfigForRes("gld", iconType);
		GameResIconConfig configForRes2 = GameResDisplayConfig.GetConfigForRes("slv", iconType);
		GameResIconConfig configForRes3 = GameResDisplayConfig.GetConfigForRes("brz", iconType);
		if (num)
		{
			stringBuilder.Append("-");
		}
		stringBuilder.Append((num2 > 0) ? (configForRes.iconName.FontIcon() + num2) : "");
		stringBuilder.Append((num2 > 0 && (num3 > 0 || num4 > 0)) ? delimiter : "");
		stringBuilder.Append((num3 > 0) ? (configForRes2.iconName.FontIcon() + num3) : "");
		stringBuilder.Append((num3 > 0 && num4 > 0) ? delimiter : "");
		stringBuilder.Append((num4 > 0) ? (configForRes3.iconName.FontIcon() + num4) : "");
		if (stringBuilder.Length == 0 && printZero)
		{
			stringBuilder.Append(configForRes3.iconName.FontIcon() + "0");
		}
		return stringBuilder.ToString();
	}
}
