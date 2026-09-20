using System.Collections.Generic;
using UnityEngine;

public class Trading
{
	public const float SELLING_PRICE_MODIFICATOR = 0.75f;

	public Vendor trader;

	public WorldGameObject player;

	public MultiInventory player_inventory;

	public Item player_offer = new Item();

	public float player_money
	{
		get
		{
			return player.data.money;
		}
		set
		{
			player.data.money = value;
		}
	}

	public Trading(WorldGameObject wgo)
	{
		trader = wgo.vendor;
		trader.FillDrawingMultiInventory();
		player = MainGame.me.player;
		player_inventory = player.GetMultiInventory(null, "", MultiInventory.PlayerMultiInventory.DontChange, include_toolbelt: false, sortWGOS: true, include_bags: true);
	}

	public bool IsUsableItem(Item item)
	{
		if (item == null || item.IsEmpty())
		{
			return true;
		}
		foreach (string product_type in item.definition.product_types)
		{
			if (trader.definition.GetProductTypes().Contains(product_type))
			{
				return true;
			}
		}
		return false;
	}

	public InventoryWidget.ItemFilterResult SellableItemsFilter(Item item, InventoryWidget widget)
	{
		if (item == null || item.IsEmpty())
		{
			return InventoryWidget.ItemFilterResult.Hide;
		}
		if (!trader.CanSellItem(item, check_tier: true))
		{
			return InventoryWidget.ItemFilterResult.Inactive;
		}
		return InventoryWidget.ItemFilterResult.Active;
	}

	public InventoryWidget.ItemFilterResult BuyableItemsFilter(Item item, InventoryWidget widget)
	{
		if (!trader.CanBuyItem(item, check_tier: true))
		{
			return InventoryWidget.ItemFilterResult.Inactive;
		}
		return InventoryWidget.ItemFilterResult.Active;
	}

	public InventoryWidget.ItemFilterResult FilterItem(Item item)
	{
		if (!IsUsableItem(item))
		{
			return InventoryWidget.ItemFilterResult.Inactive;
		}
		return InventoryWidget.ItemFilterResult.Active;
	}

	public float GetTotalBalance()
	{
		float num = 0f;
		List<string> list = new List<string>();
		foreach (Item item in player_offer.inventory)
		{
			if (!list.Contains(item.id))
			{
				int totalCount = player_offer.GetTotalCount(item.id);
				for (int i = 0; i < totalCount; i++)
				{
					num += Mathf.Round(GetSingleItemCostInPlayerInventory(item, -i) * 100f) / 100f;
				}
				list.Add(item.id);
			}
		}
		float num2 = 0f;
		list = new List<string>();
		foreach (Item item2 in trader.cur_offer.inventory)
		{
			if (!list.Contains(item2.id))
			{
				int totalCount2 = trader.cur_offer.GetTotalCount(item2.id);
				for (int j = 0; j < totalCount2; j++)
				{
					num2 += Mathf.Round(GetSingleItemCostInTraderInventory(item2, j + 1) * 100f) / 100f;
				}
				list.Add(item2.id);
			}
		}
		return num - num2;
	}

	public float GetSingleItemCostInTraderInventory(Item item, int count_modificator = 0)
	{
		int items_count = trader.inventory.GetTotalCount(item.id) + count_modificator;
		return Mathf.Round(trader.GetSingleItemPrice(item, items_count) * 100f) / 100f;
	}

	public float GetSingleItemCostInTraderInventory(string item_id, int count_modificator = 0)
	{
		return GetSingleItemCostInTraderInventory(new Item(item_id, trader.inventory.GetTotalCount(item_id)), count_modificator);
	}

	public float GetSingleItemCostInPlayerInventory(Item item, int count_modificator = 0)
	{
		int items_count = trader.inventory.GetTotalCount(item.id) + player_offer.GetTotalCount(item.id) + count_modificator;
		float num = trader.GetSingleItemPrice(item, items_count) * 0.75f;
		if (num > item.definition.base_price)
		{
			num = item.definition.base_price;
		}
		return Mathf.Round(num * 100f) / 100f;
	}

	public float GetSingleItemCostInPlayerInventory(string item_id, int count_modificator = 0)
	{
		return GetSingleItemCostInPlayerInventory(new Item(item_id, player_inventory.GetTotalCount(item_id)), count_modificator);
	}

	public bool CanAcceptOffer()
	{
		if (!player_inventory.CanAddItems(trader.cur_offer.inventory, include_bags: true))
		{
			return false;
		}
		if (!trader.inventory.CanAddItems(player_offer.inventory))
		{
			return false;
		}
		float totalBalance = GetTotalBalance();
		if (player_money + totalBalance < 0f)
		{
			return false;
		}
		if (trader.cur_money - totalBalance < 0f)
		{
			return false;
		}
		return true;
	}

	public void DoAcceptOffer(bool need_check = true)
	{
		if (need_check && !CanAcceptOffer())
		{
			return;
		}
		float totalBalance = GetTotalBalance();
		player_money += totalBalance;
		if (totalBalance > 0f)
		{
			Stats.PlayerAddMoney(totalBalance, trader.id);
		}
		else
		{
			Stats.PlayerDecMoney(0f - totalBalance, trader.id);
		}
		trader.cur_money -= totalBalance;
		if (!trader.inventory.AddItems(player_offer.inventory))
		{
			Debug.LogError("Can not add player's offer to vendor's inventory");
			return;
		}
		foreach (Item item in trader.cur_offer.inventory)
		{
			if (!MainGame.me.player.AddToInventory(item))
			{
				Debug.LogError("Can not add vendor's offer's item \"" + item.id + "\" to players's inventory");
			}
			else if (item.definition.equipment_type != 0)
			{
				MainGame.me.player.TryEquipPickupedDrop(item);
			}
		}
		foreach (Item item2 in player_offer.inventory)
		{
			for (int i = 0; i < item2.value; i++)
			{
				item2.OnTraded();
			}
		}
		foreach (Item item3 in trader.cur_offer.inventory)
		{
			for (int j = 0; j < item3.value; j++)
			{
				item3.OnTraded();
			}
		}
		player_offer.inventory.Clear();
		trader.cur_offer.inventory.Clear();
		Debug.Log("Accepted offer!");
		trader.FillDrawingMultiInventory();
	}

	public static string FormatMoney(float value, bool print_zero = false, bool use_spaces = true)
	{
		bool num = value < 0f;
		value = Mathf.Round(Mathf.Abs(value) * 100f) / 100f;
		int num2 = Mathf.FloorToInt(value / 100f);
		int num3 = Mathf.FloorToInt(value - (float)num2 * 100f);
		int num4 = Mathf.RoundToInt((value - (float)num2 * 100f - (float)num3) * 100f);
		string text = (use_spaces ? " " : "");
		string text2 = (num ? "-" : "");
		text2 += ((num2 > 0) ? ("(gld)" + num2) : "");
		text2 += ((num2 > 0 && (num3 > 0 || num4 > 0)) ? text : "");
		text2 += ((num3 > 0) ? ("(slv)" + num3) : "");
		text2 += ((num3 > 0 && num4 > 0) ? text : "");
		text2 += ((num4 > 0) ? ("(brz)" + num4) : "");
		if (text2.Length == 0 && print_zero)
		{
			return "(brz)0";
		}
		return text2;
	}

	public static void DrawMoneyOnLabel(UILabel label, float price, bool print_zero = false)
	{
		label.text = FormatMoney(price, print_zero);
	}
}
