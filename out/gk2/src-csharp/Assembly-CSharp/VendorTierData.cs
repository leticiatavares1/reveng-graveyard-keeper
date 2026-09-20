using System;
using System.Collections.Generic;

[Serializable]
public class VendorTierData
{
	public int dailyMoneyIncome;

	public int levelupCosts;

	public LazyExpression happinessCap;

	public List<string> notBuying = new List<string>();

	public List<string> notSelling = new List<string>();

	public List<VendorProductData> vendorProducts = new List<VendorProductData>();

	public List<TownVendorProductInfo> townVendorProductInfos = new List<TownVendorProductInfo>();

	public List<string> newProducts = new List<string>();

	public List<string> orders = new List<string>();

	public VendorProductData GetProduct(string itemId)
	{
		return vendorProducts.Find((VendorProductData p) => p.itemId == itemId);
	}

	public bool HasProduct(string itemId)
	{
		return vendorProducts.Find((VendorProductData p) => p.itemId == itemId) != null;
	}

	public TownVendorProductInfo GetTownVendorProductInfo(string itemId)
	{
		return townVendorProductInfos.Find((TownVendorProductInfo p) => p.itemId == itemId);
	}

	public bool HaTownVendorProductInfo(string itemId)
	{
		return townVendorProductInfos.Find((TownVendorProductInfo p) => p.itemId == itemId) != null;
	}

	public bool IsSellingProduct(string itemId)
	{
		return !notSelling.Contains(itemId);
	}

	public bool IsBuyingProduct(string itemId)
	{
		return !notBuying.Contains(itemId);
	}

	public override string ToString()
	{
		string text = string.Empty;
		for (int i = 0; i < newProducts.Count; i++)
		{
			text = text + "New product: " + newProducts[i] + "\n";
		}
		return text;
	}
}
