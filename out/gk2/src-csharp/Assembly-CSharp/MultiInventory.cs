using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MultiInventory
{
	public List<Inventory> inventoryList = new List<Inventory>();

	public MultiInventory()
	{
	}

	public MultiInventory(Inventory inventory)
	{
		inventoryList.Add(inventory);
	}

	public MultiInventory(IEnumerable<Inventory> inventoryList)
	{
		this.inventoryList.AddRange(inventoryList);
		Sort();
	}

	public MultiInventory(params Inventory[] inventoryList)
	{
		this.inventoryList.AddRange(inventoryList);
		Sort();
	}

	public MultiInventory(WorldZoneData worldZoneData, WgoData excludeWgoData = null, bool includePlayerInventory = false)
	{
		if (includePlayerInventory)
		{
			foreach (PlayerData playerData in worldZoneData.playerDataList)
			{
				inventoryList.Add(playerData.inventory);
			}
		}
		foreach (SGuid wgoData2 in worldZoneData.wgoDataList)
		{
			WgoData wgoData = MainGame.Instance.GameSave.worldData.GetWgoData(wgoData2);
			if (wgoData != null && wgoData.Definition.inventorySize != 0 && (excludeWgoData == null || !(wgoData.UniqueId == excludeWgoData.UniqueId)) && wgoData.Definition.OpenInMultiInventory)
			{
				inventoryList.Add(wgoData.Inventory);
			}
		}
		Sort();
	}

	public MultiInventory(PlayerData playerData, bool addCurrentPlayerWorldZone = true)
	{
		if (addCurrentPlayerWorldZone && playerData.CurrentWorldZoneData != null)
		{
			inventoryList.AddRange(new MultiInventory(playerData.CurrentWorldZoneData).inventoryList);
		}
		inventoryList.Insert(0, playerData.inventory);
		Sort();
	}

	public void Add(MultiInventory multiInventory)
	{
		inventoryList.AddRange(multiInventory.inventoryList);
		Sort();
	}

	public void Add(Inventory inventory)
	{
		inventoryList.Add(inventory);
		Sort();
	}

	public int GetTotalCount(string itemId)
	{
		int num = 0;
		foreach (Inventory inventory in inventoryList)
		{
			num += inventory.Data.GetTotalCountInInventory(itemId);
		}
		return num;
	}

	public bool HasItemWithEnoughDurability(string itemId, float needDurability)
	{
		foreach (Inventory inventory in inventoryList)
		{
			if (inventory.Data.HasItemWithEnoughDurability(itemId, needDurability))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasItemsById(List<Item> items)
	{
		Dictionary<string, int> dictionary = new Dictionary<string, int>(items.Count);
		foreach (Item item in items)
		{
			if (dictionary.TryGetValue(item.id, out var value))
			{
				dictionary[item.id] = value + item.Count;
			}
			else
			{
				dictionary.Add(item.id, item.Count);
			}
		}
		foreach (KeyValuePair<string, int> item2 in dictionary)
		{
			if (!HasItemQuantity(item2.Key, item2.Value))
			{
				return false;
			}
		}
		return true;
	}

	public bool HasItemsById(List<NeedItemData> needItems, WgoData wgoData)
	{
		return HasItemsById(needItems, 1, wgoData);
	}

	public bool HasItemsById(List<NeedItemData> needItems, int multiplicator = 1, WgoData wgoData = null)
	{
		Dictionary<string, int> dictionary = new Dictionary<string, int>(needItems.Count);
		foreach (NeedItemData needItem in needItems)
		{
			if (!dictionary.TryGetValue(needItem.Id, out var value))
			{
				dictionary.Add(needItem.id, needItem.GetCount(wgoData) * multiplicator);
			}
			else
			{
				dictionary[needItem.Id] = value + needItem.GetCount(wgoData);
			}
		}
		foreach (KeyValuePair<string, int> item in dictionary)
		{
			if (!HasItemQuantity(item.Key, item.Value))
			{
				return false;
			}
		}
		return true;
	}

	public bool HasItemQuantity(string itemId, int count)
	{
		return GetTotalCount(itemId) >= count;
	}

	public bool CanAddItems(List<Item> items)
	{
		return CanAddItemsToInventories(items, inventoryList);
	}

	public bool CanAddItems(List<ItemCount> itemCounts)
	{
		List<ItemCount> cantAddItemsCount = new List<ItemCount>(itemCounts);
		foreach (Inventory inventory in inventoryList)
		{
			inventory.CanAddItemsToInventory(cantAddItemsCount, out cantAddItemsCount);
			if (cantAddItemsCount.Count == 0)
			{
				return true;
			}
		}
		return false;
	}

	public bool CanAddItem(Item item, int count = -1)
	{
		if (count == -1)
		{
			count = item.Count;
		}
		int num = 0;
		foreach (Inventory inventory in inventoryList)
		{
			num += inventory.Data.CanAddItemCountToInventory(item);
			if (num >= count)
			{
				return true;
			}
		}
		return false;
	}

	public bool TryAddItems(List<Item> items)
	{
		if (!CanAddItems(items))
		{
			return false;
		}
		foreach (Item item in items)
		{
			AddItem(item);
		}
		return true;
	}

	public int TryAddItem(Item item)
	{
		int count = item.Count;
		int num = item.Count;
		foreach (Inventory inventory in inventoryList)
		{
			int num2 = inventory.Data.CanAddItemCountToInventory(item);
			if (num2 > num)
			{
				num2 = num;
			}
			if (num2 > 0)
			{
				Item item2 = Item.Copy(item);
				item2.Count = num2;
				inventory.AddItemToInventory(item2);
				num -= num2;
				if (num <= 0)
				{
					break;
				}
			}
		}
		return count - num;
	}

	public void TryAddItemsPerOne(List<Item> items)
	{
		foreach (Item item in items)
		{
			TryAddItem(item);
		}
	}

	public void AddItems(List<Item> items)
	{
		foreach (Inventory inventory in inventoryList)
		{
			for (int i = 0; i < items.Count; i++)
			{
				Item item = items[i];
				if (inventory.Data.CanAddItemCountToInventory(item) != 0)
				{
					inventory.AddItemToInventory(item);
					if (item.Count == 0)
					{
						items.RemoveAt(i);
						i--;
					}
				}
			}
		}
	}

	public int AddItem(Item item)
	{
		if (item == null || item.Count <= 0)
		{
			return 0;
		}
		int num = item.Count;
		int num2 = 0;
		foreach (Inventory inventory in inventoryList)
		{
			int num3 = inventory.Data.CanAddItemCountToInventory(item);
			if (num3 > num)
			{
				num3 = num;
			}
			if (num3 <= 0)
			{
				continue;
			}
			Item item2 = Item.Copy(item);
			item2.Count = num3;
			if (inventory.AddItemToInventory(item2))
			{
				num2 += num3;
				num -= num3;
				if (num <= 0)
				{
					break;
				}
			}
		}
		return num2;
	}

	public List<Item> RemoveItemByCount(string itemId, int count)
	{
		List<Item> list = new List<Item>();
		int num = count;
		foreach (Inventory inventory in inventoryList)
		{
			if (num <= 0)
			{
				break;
			}
			List<Item> list2 = inventory.RemoveItemById(itemId, num);
			list.AddRange(list2);
			foreach (Item item in list2)
			{
				num -= item.Count;
			}
		}
		return list;
	}

	public List<Item> RemoveItems(List<NeedItemData> items, WgoData wgoData)
	{
		return RemoveItems(items, 1, wgoData);
	}

	public List<Item> RemoveItems(List<NeedItemData> items, int multiplicator = 1, WgoData wgoData = null)
	{
		List<Item> list = new List<Item>();
		foreach (NeedItemData item in items)
		{
			int num = item.GetCount(wgoData) * multiplicator;
			bool flag = false;
			foreach (Inventory inventory in inventoryList)
			{
				List<Item> list2 = inventory.RemoveItemById(item.Id, num);
				list.AddRange(list2);
				foreach (Item item2 in list2)
				{
					num -= item2.Count;
					if (num <= 0)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					break;
				}
			}
		}
		return list;
	}

	public void RemoveItemFromInventoryByUID(Item item, int count = -1)
	{
		using List<Inventory>.Enumerator enumerator = inventoryList.GetEnumerator();
		while (enumerator.MoveNext() && !enumerator.Current.RemoveItemFromInventoryByUID(item, count))
		{
		}
	}

	public void RemoveItem(Item item)
	{
		using List<Inventory>.Enumerator enumerator = inventoryList.GetEnumerator();
		while (enumerator.MoveNext() && enumerator.Current.RemoveItemById(item.id, item.Count).Count <= 0)
		{
		}
	}

	public bool IsEmpty()
	{
		foreach (Inventory inventory in inventoryList)
		{
			if (!inventory.Data.IsInventoryEmpty())
			{
				return false;
			}
		}
		return true;
	}

	private static bool CanAddItemsToInventories(List<Item> items, List<Inventory> inventories)
	{
		if (items.Count == 0)
		{
			return true;
		}
		int num = 0;
		foreach (Inventory inventory in inventories)
		{
			num += inventory.Data.InventorySize - inventory.Data.InventoryCount;
		}
		for (int i = 0; i < items.Count; i++)
		{
			Item item = items[i];
			int num2 = 0;
			foreach (Inventory inventory2 in inventories)
			{
				num2 += inventory2.Data.CanAddItemCountToInventory(item, considerEmptySlots: false);
			}
			if (num2 < item.Count)
			{
				int num3 = Mathf.CeilToInt((float)(item.Count - num2) / (float)item.Definition.stackCount);
				if (num < num3)
				{
					return false;
				}
				num -= num3;
			}
		}
		return true;
	}

	private void Sort()
	{
		inventoryList.Sort(delegate(Inventory x, Inventory y)
		{
			if (!x.Data.HasProperty<FuelContainerSerializedItemProperty>())
			{
				if (y.Data.HasProperty<FuelContainerSerializedItemProperty>())
				{
					return -1;
				}
			}
			else
			{
				if (!y.Data.HasProperty<FuelContainerSerializedItemProperty>())
				{
					return 1;
				}
				if (x.Data.InventorySize < y.Data.InventorySize)
				{
					return -1;
				}
			}
			return 0;
		});
	}
}
