using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

[Serializable]
public class Vendor : ObjectLinkedToDefinition<VendorDef>
{
	private const float SELL_TO_VENDOR_DISCOUNT = 0.75f;

	[SerializeField]
	private Inventory inventory;

	[SerializeField]
	private int curMoney;

	[SerializeField]
	private int curTier;

	[SerializeField]
	private float usedHappinessThisWeek;

	[SerializeField]
	private GameRes soldItemsWithHappinessThisWeek = new GameRes();

	[SerializeField]
	private List<VendorOrderData> orders = new List<VendorOrderData>();

	public GameRes SoldItemsWithHappinessThisWeek => soldItemsWithHappinessThisWeek;

	public float UsedHappinessThisWeek
	{
		get
		{
			return usedHappinessThisWeek;
		}
		set
		{
			usedHappinessThisWeek = value;
		}
	}

	public int CurTier => curTier;

	public int CurMoney
	{
		get
		{
			return curMoney;
		}
		set
		{
			curMoney = value;
		}
	}

	public Inventory Inventory => inventory;

	public VendorTierData CurrentTierData => base.Definition.tierDataList[curTier - 1];

	public VendorTierData NextTierData => base.Definition.tierDataList[curTier];

	public List<VendorOrderData> Orders => orders;

	public Vendor()
	{
	}

	public Vendor(string id, int globalCountModificator)
	{
		base.id = id;
		inventory = new Inventory("inventory", 0, autoExpand: true);
		curTier = base.Definition.startTier;
		curMoney = base.Definition.startMoney;
		for (int i = 0; i < CurrentTierData.vendorProducts.Count; i++)
		{
			VendorProductData vendorProductData = CurrentTierData.vendorProducts[i];
			inventory.AddItemToInventory(new Item(vendorProductData.itemId, CurBaseCount(vendorProductData, globalCountModificator)));
		}
		for (int j = 0; j < base.Definition.tierDataList.Count; j++)
		{
			using List<string>.Enumerator enumerator = base.Definition.tierDataList[j].orders.GetEnumerator();
			while (enumerator.MoveNext())
			{
				VendorOrderData item = new VendorOrderData(enumerator.Current)
				{
					Tier = j + 1
				};
				orders.Add(item);
			}
		}
	}

	public int CurBasePrice(VendorProductData productData)
	{
		if (productData.Definition.isStaticCost)
		{
			return productData.Definition.basePrice;
		}
		int num = productData.Definition.basePrice + MainGame.PlayerData.GetResInt(productData.itemId + "_base_price_global_mod") + productData.priceMod;
		if (num <= 0)
		{
			return 1;
		}
		return num;
	}

	public int CurBaseCount(VendorProductData productData)
	{
		return CurBaseCount(productData, MainGame.PlayerData.GetResInt(productData.itemId + "_base_count_global_mod"));
	}

	public int CurBaseCount(VendorProductData productData, int globalCountModificator)
	{
		return globalCountModificator + productData.baseCount;
	}

	public int CurPrice(string itemId, bool buy, int itemsCount = 0)
	{
		return CurPrice(CurrentTierData.GetProduct(itemId), buy, itemsCount);
	}

	public float CurPriceFloat(string itemId, bool buy, int itemsCount = 0)
	{
		return CurPriceFloat(CurrentTierData.GetProduct(itemId), buy, itemsCount);
	}

	public int CurPrice(VendorProductData productData, bool buy, int itemsCount = 0)
	{
		if (productData == null)
		{
			return 0;
		}
		if (itemsCount == 0)
		{
			itemsCount = CurCount(productData);
		}
		if (itemsCount == 0)
		{
			itemsCount = 1;
		}
		if (productData.Definition.isStaticCost)
		{
			return productData.Definition.basePrice * itemsCount;
		}
		int num = Mathf.RoundToInt(1f * (float)CurBasePrice(productData) * Mathf.Sqrt((float)CurBaseCount(productData) / (float)itemsCount) * (buy ? 1f : 0.75f));
		if (num <= 0)
		{
			return 1;
		}
		return num;
	}

	public float CurPriceFloat(VendorProductData productData, bool buy, int itemsCount = 0)
	{
		if (productData == null)
		{
			return 0f;
		}
		if (itemsCount == 0)
		{
			itemsCount = CurCount(productData);
		}
		if (itemsCount == 0)
		{
			itemsCount = 1;
		}
		if (productData.Definition.isStaticCost)
		{
			return productData.Definition.basePrice * itemsCount;
		}
		float num = 1f * (float)CurBasePrice(productData) * Mathf.Sqrt((float)CurBaseCount(productData) / (float)itemsCount) * (buy ? 1f : 0.75f);
		if (!(num > 0f))
		{
			return 1f;
		}
		return num;
	}

	public int CurCount(VendorProductData productData)
	{
		return CurCount(productData.itemId);
	}

	public int CurCount(string itemId)
	{
		return inventory.Data.GetTotalCountInInventory(itemId);
	}

	public bool CanSellItemToPlayer(ItemDef itemDef)
	{
		if (!CurrentTierData.HasProduct(itemDef.id))
		{
			return false;
		}
		if (!CurrentTierData.IsSellingProduct(itemDef.id))
		{
			return false;
		}
		return true;
	}

	public bool CanBuyItemFromPlayer(ItemDef itemDef)
	{
		if (!CurrentTierData.HasProduct(itemDef.id))
		{
			return false;
		}
		if (!CurrentTierData.IsBuyingProduct(itemDef.id))
		{
			return false;
		}
		return true;
	}

	public void OnEndOfDay()
	{
		TradeWithBank();
	}

	public bool HasHappinessForItem(string itemId, int extraSoldCount = 0, float extraUsedHappiness = 0f)
	{
		if (string.IsNullOrEmpty(itemId))
		{
			return false;
		}
		TownVendorProductInfo townVendorProductInfo = CurrentTierData.GetTownVendorProductInfo(itemId);
		if (townVendorProductInfo == null)
		{
			return false;
		}
		if (CurrentTierData.happinessCap.EvaluateFloat() - usedHappinessThisWeek - Math.Max(0f, extraUsedHappiness) <= 0f)
		{
			return false;
		}
		return soldItemsWithHappinessThisWeek.GetInt(itemId) + Math.Max(0, extraSoldCount) < townVendorProductInfo.itemCount;
	}

	public void ForceLevelUp()
	{
		if (curTier < base.Definition.tierDataList.Count)
		{
			int num = NeedCapitalForLevelUp();
			curTier++;
			soldItemsWithHappinessThisWeek.Clear();
			usedHappinessThisWeek = 0f;
			AddMissingCurrentTierProductsToInventory();
			int num2 = CurCapital();
			if (num2 < num)
			{
				curMoney += num - num2;
			}
		}
	}

	private void AddMissingCurrentTierProductsToInventory()
	{
		List<VendorProductData> vendorProducts = CurrentTierData.vendorProducts;
		for (int i = 0; i < vendorProducts.Count; i++)
		{
			VendorProductData vendorProductData = vendorProducts[i];
			if (!string.IsNullOrEmpty(vendorProductData.itemId) && !inventory.Data.HasItemQuantityInInventory(vendorProductData.itemId, 1))
			{
				inventory.AddItemToInventory(new Item(vendorProductData.itemId, CurBaseCount(vendorProductData)));
			}
		}
	}

	private int CalcPurchaseCountAtTheEndOfDay(int curCount, int curBaseCount, bool isStaticCost)
	{
		if (curCount == curBaseCount)
		{
			return 0;
		}
		if (curCount != curBaseCount && isStaticCost)
		{
			return curBaseCount - curCount;
		}
		if (curBaseCount == 0)
		{
			return Mathf.FloorToInt(-0.2f * (float)curCount);
		}
		float num = (float)curCount / (float)curBaseCount;
		if (num <= 0.5f)
		{
			return Mathf.RoundToInt(0.2f * (float)curBaseCount);
		}
		if (num >= 1.5f)
		{
			return Mathf.RoundToInt(-0.2f * (float)curBaseCount);
		}
		if (num > 0.5f && num < 1f)
		{
			return Mathf.CeilToInt(0.8f * (num * num - 2f * num + 1f) * (float)curBaseCount);
		}
		if (num > 1f && num < 1.5f)
		{
			return Mathf.FloorToInt(-0.8f * (num * num - 2f * num + 1f) * (float)curBaseCount);
		}
		return 0;
	}

	private void TradeWithBank()
	{
		curMoney += CurrentTierData.dailyMoneyIncome;
		Debug.Log("#economy# TradeWithBank:[" + id + "]");
		List<ItemDef> list = new List<ItemDef>();
		List<int> list2 = new List<int>();
		List<string> itemsToCheck = new List<string>();
		for (int i = 0; i < CurrentTierData.vendorProducts.Count; i++)
		{
			TryAddItemToCheck(CurrentTierData.vendorProducts[i].itemId);
		}
		for (int j = 0; j < inventory.Data.Inventory.Count; j++)
		{
			TryAddItemToCheck(inventory.Data.Inventory[j].id);
		}
		foreach (string item in itemsToCheck)
		{
			ItemDef data = GameBalance.Me.GetData<ItemDef>(item);
			int num;
			if (CurrentTierData.HasProduct(item))
			{
				VendorProductData product = CurrentTierData.GetProduct(data.id);
				num = CalcPurchaseCountAtTheEndOfDay(CurCount(product), CurBaseCount(product), data.isStaticCost);
			}
			else
			{
				num = CalcPurchaseCountAtTheEndOfDay(CurCount(item), 0, data.isStaticCost);
			}
			if (num > 0)
			{
				list.Add(data);
				list2.Add(num);
			}
			else if (num < 0)
			{
				for (int k = 0; k < -num; k++)
				{
					inventory.RemoveItemById(data.id, 1);
					curMoney += data.basePrice;
				}
			}
		}
		bool flag;
		do
		{
			flag = false;
			for (int l = 0; l < list2.Count; l++)
			{
				if (list2[l] > 0 && curMoney >= list[l].basePrice)
				{
					list2[l]--;
					curMoney -= list[l].basePrice;
					inventory.AddItemToInventory(new Item(list[l].id));
					flag = true;
				}
			}
		}
		while (flag);
		void TryAddItemToCheck(string id)
		{
			if (!itemsToCheck.Contains(id))
			{
				itemsToCheck.Add(id);
			}
		}
	}

	private int CurCapital()
	{
		int num = curMoney;
		foreach (Item item in inventory.Data.Inventory)
		{
			VendorProductData vendorProductData = CurrentTierData.GetProduct(item.id);
			if (vendorProductData == null)
			{
				vendorProductData = new VendorProductData();
				vendorProductData.itemId = item.id;
			}
			num += CurBaseCount(vendorProductData) * CurBasePrice(vendorProductData);
		}
		return num;
	}

	private int NeedCapitalForLevelUp()
	{
		int num = 0;
		for (int i = 0; i < NextTierData.vendorProducts.Count; i++)
		{
			VendorProductData productData = NextTierData.vendorProducts[i];
			int num2 = CurBaseCount(productData);
			int num3 = CurBasePrice(productData);
			num += num2 * num3;
		}
		return num + CurrentTierData.levelupCosts;
	}
}
