using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Vendor
{
	public const float TRADING_WITH_BANK_COEFF = 0.05f;

	public const float TRADING_WITH_BANK_MAX_BASE_COUNT_PART_SALE = 0.25f;

	public const float TRADING_WITH_BANK_MAX_BASE_COUNT_PART_BUY = 0.15f;

	public const float ADDITIONAL_SUM_COEFF = 0.1f;

	public const string VENDOR_INITED = "vendor_inited";

	public const string MONEY = "money";

	public const string VENDOR_TIER = "vendor_tier";

	public const string LEVELUP_BAR = "levelup_bar_";

	public string id;

	private VendorDefinition _definition;

	[NonSerialized]
	public MultiInventory inventory;

	[NonSerialized]
	public MultiInventory drawing_inventory;

	[NonSerialized]
	public Item vendor_data;

	[NonSerialized]
	public Item cur_offer;

	public int max_tier;

	public VendorDefinition definition
	{
		get
		{
			if (_definition == null)
			{
				_definition = ((string.IsNullOrEmpty(id) || id == "empty") ? null : GameBalance.me.GetDataOrNull<VendorDefinition>(id));
			}
			return _definition;
		}
	}

	public float cur_money
	{
		get
		{
			if (vendor_data == null)
			{
				return 0f;
			}
			return vendor_data.GetParam("money");
		}
		set
		{
			if (vendor_data != null)
			{
				vendor_data.SetParam("money", value);
			}
		}
	}

	public int cur_tier
	{
		get
		{
			if (vendor_data == null)
			{
				return 0;
			}
			return vendor_data.GetParamInt("vendor_tier");
		}
		set
		{
			if (vendor_data != null)
			{
				vendor_data.SetParam("vendor_tier", value);
			}
		}
	}

	public Vendor(MultiInventory vendor_inventories, VendorDefinition vendor_definition, Item vendor_data)
	{
		id = vendor_definition.id;
		_definition = vendor_definition;
		this.vendor_data = vendor_data;
		cur_offer = new Item();
		for (int i = 1; i < 3; i++)
		{
			vendor_data.SetParam("levelup_bar_" + i, 0f);
		}
		inventory = vendor_inventories;
		if (Mathf.Abs(vendor_data.GetParam("vendor_inited") - 1f) > 0.01f)
		{
			cur_tier = vendor_definition.start_tire;
			cur_money = vendor_definition.start_money;
			FillVendorInventory();
			vendor_data.SetParam("vendor_inited", 1f);
		}
	}

	public void FillDrawingMultiInventory()
	{
		List<Item>[] array = new List<Item>[3];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new List<Item>();
		}
		foreach (Item item2 in inventory.all[0].data.inventory)
		{
			if (item2 != null && item2.definition != null)
			{
				int product_tier = item2.definition.product_tier;
				if (product_tier >= 1 && product_tier <= 3)
				{
					array[product_tier - 1].Add(item2);
				}
			}
		}
		max_tier = 3;
		int num = 2;
		while (num >= 0 && array[num].Count == 0)
		{
			max_tier = num;
			num--;
		}
		Inventory[] array2 = new Inventory[3];
		if (max_tier == 0)
		{
			max_tier = 1;
		}
		for (int j = 0; j < max_tier; j++)
		{
			int num2 = j + 1;
			Item item = new Item();
			item.SetParam("inventory_size", array[j].Count + 100);
			array2[j] = new Inventory(item, GJL.L("items_tier") + " " + GJCommons.GetRomeNumber(num2))
			{
				is_locked = (num2 > cur_tier),
				vendor_tier_info = new Inventory.VendorTierInfo
				{
					progressbar_visible = false,
					tier_1 = cur_tier,
					tier_2 = num2
				}
			};
			if (num2 == cur_tier + 1)
			{
				array2[j].vendor_tier_info.progressbar_visible = true;
				array2[j].vendor_tier_info.progress = GetVendorLevelupProgress();
			}
			for (int k = 0; k < array[j].Count; k++)
			{
				array2[j].data.AddItem(array[j][k]);
			}
		}
		if (drawing_inventory == null)
		{
			drawing_inventory = new MultiInventory(array2[0]);
			for (int l = 1; l < max_tier; l++)
			{
				drawing_inventory.AddInventory(array2[l]);
			}
			return;
		}
		List<Inventory> list = new List<Inventory>();
		for (int m = 0; m < 3 && array2[m] != null; m++)
		{
			list.Add(array2[m]);
		}
		drawing_inventory.SetInventories(list);
	}

	public float GetSingleItemPrice(Item item, int items_count = 0)
	{
		if (item == null || item.definition == null)
		{
			return 0f;
		}
		return GetSingleItemPrice(item.definition, items_count);
	}

	public float GetSingleItemPrice(ItemDefinition item_def, int items_count = 0)
	{
		if (!CanTradeItem(item_def))
		{
			return 0f;
		}
		if (items_count == 0)
		{
			items_count = GetItemsCount(item_def.id);
		}
		int modifiedBaseCount = GetModifiedBaseCount(item_def);
		return (float)Math.Round(item_def.GetPrice(items_count, modifiedBaseCount), 2);
	}

	public bool CanSellItem(Item item, bool check_tier = false)
	{
		if (item == null)
		{
			return false;
		}
		return CanSellItem(item.definition, check_tier);
	}

	public bool CanSellItem(ItemDefinition item_def, bool check_tier = false)
	{
		if (item_def == null || item_def.product_types == null || item_def.product_types.Count == 0)
		{
			return false;
		}
		if (check_tier && item_def.product_tier > cur_tier)
		{
			return false;
		}
		if (!CanTradeItemType(item_def.product_types))
		{
			return false;
		}
		foreach (VendorDefinition.ItemModificator item in definition.not_selling)
		{
			if (item.item_name == item_def.id)
			{
				if (item.tier < 1)
				{
					return false;
				}
				if (item.tier == cur_tier)
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool CanBuyItem(Item item, bool check_tier = false)
	{
		if (item == null)
		{
			return false;
		}
		return CanBuyItem(item.definition, check_tier);
	}

	public bool CanBuyItem(ItemDefinition item_def, bool check_tier = false)
	{
		if (item_def == null || item_def.product_types == null || item_def.product_types.Count == 0)
		{
			return false;
		}
		if (check_tier && item_def.product_tier > cur_tier)
		{
			return false;
		}
		if (!CanTradeItemType(item_def.product_types))
		{
			return false;
		}
		foreach (VendorDefinition.ItemModificator item in definition.not_buying)
		{
			if (item.item_name == item_def.id)
			{
				if (item.tier < 1)
				{
					return false;
				}
				if (item.tier == cur_tier)
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool CanBuyOrSellItem(ItemDefinition item_def, bool check_tier = false)
	{
		if (item_def == null || item_def.product_types == null || item_def.product_types.Count == 0)
		{
			return false;
		}
		if (check_tier && item_def.product_tier > cur_tier)
		{
			return false;
		}
		if (!CanTradeItemType(item_def.product_types))
		{
			return false;
		}
		bool flag = true;
		foreach (VendorDefinition.ItemModificator item in definition.not_buying)
		{
			if (item.item_name == item_def.id && (item.tier < 1 || item.tier == cur_tier))
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			return true;
		}
		foreach (VendorDefinition.ItemModificator item2 in definition.not_selling)
		{
			if (item2.item_name == item_def.id && (item2.tier < 1 || item2.tier == cur_tier))
			{
				return false;
			}
		}
		return true;
	}

	public bool CanTradeItem(ItemDefinition item_def)
	{
		if (item_def == null || item_def.product_types == null || item_def.product_types.Count == 0)
		{
			return false;
		}
		return CanTradeItemType(item_def.product_types);
	}

	private bool CanTradeItemType(List<string> item_types)
	{
		foreach (string item_type in item_types)
		{
			if (definition.GetProductTypes().Contains(item_type))
			{
				return true;
			}
		}
		return false;
	}

	public int GetModifiedBaseCount(ItemDefinition item_def)
	{
		int base_count = item_def.base_count;
		foreach (VendorDefinition.CountModificator count_modificator in definition.count_modificators)
		{
			if (count_modificator.item_name == item_def.id && count_modificator.tier <= cur_tier)
			{
				base_count = count_modificator.base_count;
			}
		}
		return base_count;
	}

	public int GetItemsCount(string item_id)
	{
		if (inventory != null)
		{
			return inventory.GetTotalCount(item_id);
		}
		return 0;
	}

	public void FillVendorInventory()
	{
		foreach (ItemDefinition items_datum in GameBalance.me.items_data)
		{
			if (!CanBuyOrSellItem(items_datum))
			{
				continue;
			}
			int base_count = items_datum.base_count;
			foreach (VendorDefinition.CountModificator count_modificator in definition.count_modificators)
			{
				if (count_modificator.item_name == items_datum.id)
				{
					base_count = count_modificator.base_count;
				}
			}
			if (base_count != 0)
			{
				inventory.AddItem(items_datum.id, (items_datum.product_tier > cur_tier) ? 1 : items_datum.base_count);
			}
		}
	}

	private int CalcPurchaseAtTheEndOfDay(ItemDefinition item_def)
	{
		int num = 0;
		int modifiedBaseCount = GetModifiedBaseCount(item_def);
		float base_price = item_def.base_price;
		int itemsCount = GetItemsCount(item_def.id);
		float singleItemPrice = GetSingleItemPrice(item_def, itemsCount);
		if (modifiedBaseCount > 0 && Mathf.Abs(((float)itemsCount - (float)modifiedBaseCount) / (float)modifiedBaseCount) < 0.05f)
		{
			return 0;
		}
		if (item_def.is_static_cost)
		{
			num = modifiedBaseCount - itemsCount;
		}
		else
		{
			num = ((!(Mathf.Abs(singleItemPrice - base_price) < 0.01f)) ? ((singleItemPrice < base_price) ? Mathf.FloorToInt((float)modifiedBaseCount * ((float)itemsCount / (float)modifiedBaseCount) * 0.05f * -1f) : Mathf.CeilToInt((float)modifiedBaseCount * ((float)modifiedBaseCount / ((float)itemsCount + 1f)) * 0.05f)) : Math.Sign(modifiedBaseCount - itemsCount));
			if ((float)num > (float)modifiedBaseCount * 0.15f)
			{
				num = Mathf.CeilToInt((float)(Math.Sign(num) * modifiedBaseCount) * 0.15f);
			}
			else if ((float)num < (float)(-1 * modifiedBaseCount) * 0.25f)
			{
				num = Mathf.FloorToInt((float)(Math.Sign(num) * modifiedBaseCount) * 0.25f);
			}
		}
		if (itemsCount > modifiedBaseCount && num > 0)
		{
			Debug.LogError("Something wrong with item {" + item_def.id + "} while vendor [" + definition.id + "] trading with bank #1. Call Bulat {" + itemsCount + "/" + modifiedBaseCount + "; " + singleItemPrice + "/" + base_price + "}");
		}
		else if (itemsCount < modifiedBaseCount && num < 0)
		{
			Debug.LogError("Something wrong with item {" + item_def.id + "} while vendor [" + definition.id + "] trading with bank #2. Call Bulat {" + itemsCount + "/" + modifiedBaseCount + "; " + singleItemPrice + "/" + base_price + "}");
		}
		return num;
	}

	private void TradeWithBank()
	{
		cur_money += definition.daily_money_income;
		List<ItemDefinition> list = new List<ItemDefinition>();
		List<int> list2 = new List<int>();
		foreach (ItemDefinition items_datum in GameBalance.me.items_data)
		{
			if (CanBuyOrSellItem(items_datum) && items_datum.product_tier <= cur_tier)
			{
				int num = CalcPurchaseAtTheEndOfDay(items_datum);
				if (num > 0)
				{
					list.Add(items_datum);
					list2.Add(num);
				}
				else if (num < 0)
				{
					inventory.RemoveItem(items_datum.id, Math.Abs(num));
					cur_money -= (float)num * items_datum.base_price * items_datum.custom_sell_price_koeff;
				}
			}
		}
		if (list.Count != list2.Count)
		{
			Debug.LogError("FATAL ERROR! Count of items not equal to count of counts!");
			return;
		}
		bool flag;
		do
		{
			flag = false;
			for (int i = 0; i < list2.Count; i++)
			{
				if (list2[i] > 0 && !(cur_money < list[i].base_price))
				{
					list2[i]--;
					cur_money -= list[i].base_price;
					inventory.AddItem(list[i].id, 1);
					flag = true;
				}
			}
		}
		while (flag);
	}

	private bool TryLevelUpVendor()
	{
		if (GetVendorLevelupProgress() > 1f)
		{
			cur_tier++;
			Stats.DesignEvent("VendorLevel:" + id + ":" + cur_tier);
			return true;
		}
		return false;
	}

	private float GetMoneyNeededForVendorLevelUp()
	{
		if (cur_tier >= 3)
		{
			return float.PositiveInfinity;
		}
		float num = 0f;
		bool flag = Mathf.Abs(definition.levelup_costs[cur_tier - 1]) < 0.01f;
		float num2 = 0f;
		cur_tier++;
		foreach (ItemDefinition items_datum in GameBalance.me.items_data)
		{
			if (items_datum.product_tier <= cur_tier && CanBuyOrSellItem(items_datum))
			{
				int modifiedBaseCount = GetModifiedBaseCount(items_datum);
				int totalCount = inventory.GetTotalCount(items_datum.id);
				int num3 = modifiedBaseCount - totalCount;
				float num4 = (float)num3 * items_datum.base_price;
				if (num3 < 0)
				{
					num4 *= items_datum.custom_sell_price_koeff;
				}
				num += num4;
				if (flag)
				{
					num2 += (float)modifiedBaseCount * items_datum.base_price * 0.1f;
				}
			}
		}
		cur_tier--;
		if (num < 0f)
		{
			return 0.01f;
		}
		if (flag)
		{
			return num + num2;
		}
		return num + definition.levelup_costs[cur_tier - 1];
	}

	public void OnEndOfDay()
	{
		if (string.IsNullOrEmpty(definition?.id))
		{
			if (string.IsNullOrEmpty(id))
			{
				Debug.LogWarning("Found vendor with null id");
			}
			else if (id == "empty")
			{
				Debug.LogWarning("Found vendor with id=\"empty\"");
			}
			else if (definition == null)
			{
				Debug.LogWarning("Not found definition for vendor with id=\"" + id + "\"");
			}
			else
			{
				Debug.LogWarning("Found vendor with null definiton.id");
			}
			return;
		}
		TradeWithBank();
		if (cur_tier < 3)
		{
			if (TryLevelUpVendor())
			{
				Debug.Log("Vendor \"" + definition.id + "\" level up. Cur tier = " + cur_tier);
			}
		}
		else
		{
			Debug.Log("Vendor \"" + definition.id + "\" already max tier.");
		}
	}

	private float GetVendorLevelupProgress()
	{
		float levelUpBar = GetLevelUpBar();
		float totalGoods = GetTotalGoods();
		float totalGoodsOnNextTier = GetTotalGoodsOnNextTier();
		Debug.Log("Vendor \"" + id + "\" real level up progress: " + totalGoods + "/" + totalGoodsOnNextTier);
		float num = totalGoodsOnNextTier - levelUpBar;
		float num2 = (totalGoods - num) / (totalGoodsOnNextTier - num);
		if (num2 < 0f)
		{
			num2 = 0f;
		}
		return num2;
	}

	private float GetTotalGoods()
	{
		float num = cur_money;
		for (int i = 0; i < GameBalance.me.items_data.Count; i++)
		{
			ItemDefinition itemDefinition = GameBalance.me.items_data[i];
			if (itemDefinition.product_tier <= cur_tier && CanBuyOrSellItem(itemDefinition))
			{
				float num2 = (float)inventory.GetTotalCount(itemDefinition.id) * itemDefinition.base_price;
				if (itemDefinition.custom_sell_price_koeff > 1f)
				{
					num2 *= itemDefinition.custom_sell_price_koeff;
				}
				num += num2;
			}
		}
		return num;
	}

	private float GetTotalGoodsOnNextTier()
	{
		if (cur_tier >= 3)
		{
			return 0f;
		}
		float num = 0f;
		cur_tier++;
		for (int i = 0; i < GameBalance.me.items_data.Count; i++)
		{
			ItemDefinition itemDefinition = GameBalance.me.items_data[i];
			if (itemDefinition.product_tier <= cur_tier && CanBuyOrSellItem(itemDefinition))
			{
				float num2 = (float)GetModifiedBaseCount(itemDefinition) * itemDefinition.base_price;
				num += num2;
			}
		}
		cur_tier--;
		if (Mathf.Abs(definition.levelup_costs[cur_tier - 1]) < 0.01f)
		{
			return num * 1.1f;
		}
		return num + definition.levelup_costs[cur_tier - 1];
	}

	private float GetLevelUpBar()
	{
		if (vendor_data == null)
		{
			return 0f;
		}
		if (cur_tier >= 3)
		{
			Debug.LogError("Wrong tier!");
			return 0f;
		}
		if (vendor_data.GetParam("levelup_bar_" + cur_tier) < 0.1f)
		{
			CalculateLevelUpBar();
		}
		return vendor_data.GetParam("levelup_bar_" + cur_tier);
	}

	private void CalculateLevelUpBar()
	{
		if (cur_tier >= 3)
		{
			Debug.LogError("Wrong tier #2!");
			return;
		}
		float num = GetTotalGoods() - cur_money;
		float totalGoodsOnNextTier = GetTotalGoodsOnNextTier();
		float num2 = totalGoodsOnNextTier - num;
		if (num2 < 0.1f)
		{
			Debug.LogError("Wrong bar for vendor \"" + id + "\": " + num2 + " = " + totalGoodsOnNextTier + " - " + num);
			num2 = totalGoodsOnNextTier;
		}
		vendor_data.SetParam("levelup_bar_" + cur_tier, num2);
	}
}
