using System;
using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

[Serializable]
public class Item : ObjectLinkedToDefinition<ItemDef>
{
	private const int AUTO_EXPAND_SIZE_LIMIT = 1000;

	[SerializeField]
	private int count;

	[SerializeField]
	private SGuid uniqueId;

	[SerializeField]
	private List<Item> inventory = new List<Item>();

	[SerializeField]
	private int inventorySize;

	[SerializeField]
	private int inventoryFillSize = -1;

	[SerializeReference]
	private Dictionary<Type, SerializedItemProperty> properties = new Dictionary<Type, SerializedItemProperty>();

	public int TotalItemsCount
	{
		get
		{
			int totalItemsCount = 0;
			inventory.ForEach(delegate(Item item)
			{
				totalItemsCount += item.count;
			});
			return totalItemsCount;
		}
	}

	public int Count
	{
		get
		{
			return count;
		}
		set
		{
			count = value;
		}
	}

	public SGuid UniqueId => uniqueId;

	public static Item Empty => new Item("empty", 0);

	public bool IsEmpty
	{
		get
		{
			if (!(id == "empty") && !string.IsNullOrEmpty(id))
			{
				return count <= 0;
			}
			return true;
		}
	}

	public bool IsBag
	{
		get
		{
			if (!string.IsNullOrEmpty(id) && id != "empty")
			{
				return base.Definition.isBag;
			}
			return false;
		}
	}

	public bool IsSeed
	{
		get
		{
			if (!string.IsNullOrEmpty(id) && id != "empty")
			{
				return base.Definition.isSeed;
			}
			return false;
		}
	}

	public bool IsFertilizer
	{
		get
		{
			if (!string.IsNullOrEmpty(id) && id != "empty")
			{
				return base.Definition.isFertilizer;
			}
			return false;
		}
	}

	public int InventoryFillSize
	{
		get
		{
			if (inventoryFillSize != -1)
			{
				return inventoryFillSize;
			}
			CalculateInventoryFillSize();
			return inventoryFillSize;
		}
	}

	public int InventorySize
	{
		get
		{
			return inventorySize;
		}
		set
		{
			inventorySize = value;
		}
	}

	public int InventoryCount => inventory.Count;

	public List<Item> Inventory => inventory;

	public Item()
	{
		id = "empty";
		count = -1;
	}

	public Item(string id, int value = 1)
		: base(id)
	{
		base.id = id;
		count = value;
		uniqueId = new SGuid();
		if (base.Definition != null)
		{
			if (base.Definition.inventorySize > 0)
			{
				inventorySize = base.Definition.inventorySize;
			}
			if (base.Definition.isBag)
			{
				inventorySize = base.Definition.bagSize;
			}
			if (base.Definition.hasDurability)
			{
				AddProperty(new DurabilitySerializedItemProperty(1f));
			}
		}
	}

	public static Item Copy(Item other)
	{
		Item item = new Item();
		item.id = other.id;
		item.count = other.count;
		item.uniqueId = new SGuid();
		item.uniqueId.Id = other.uniqueId.Id;
		item.inventorySize = other.inventorySize;
		item.inventoryFillSize = other.inventoryFillSize;
		item.inventory = new List<Item>(other.inventory);
		item.properties = new Dictionary<Type, SerializedItemProperty>();
		foreach (KeyValuePair<Type, SerializedItemProperty> property in other.properties)
		{
			item.properties.Add(property.Key, property.Value.Clone());
		}
		return item;
	}

	public override string ToString()
	{
		return $"{id}={Count}";
	}

	public static float GetQualityAverage(List<Item> items)
	{
		float num = 0f;
		int num2 = 0;
		foreach (Item item in items)
		{
			num += (float)item.Definition.quality;
			num2++;
		}
		if (num2 > 0)
		{
			num /= (float)num2;
		}
		return num;
	}

	public static int GetCraftStartTicksBonusValue(List<Item> items)
	{
		int num = 0;
		foreach (Item item in items)
		{
			num += item.Definition.quality - 1;
		}
		return num;
	}

	public void AddProperty<T>(T property) where T : SerializedItemProperty
	{
		CheckPropertiesCreated();
		Type typeFromHandle = typeof(T);
		if (properties.ContainsKey(typeFromHandle))
		{
			Debug.LogWarning("Item already has property of type " + typeFromHandle.Name);
		}
		else
		{
			properties[typeFromHandle] = property;
		}
	}

	public bool RemoveProperty<T>() where T : SerializedItemProperty
	{
		CheckPropertiesCreated();
		return properties.Remove(typeof(T));
	}

	public bool HasProperty<T>() where T : SerializedItemProperty
	{
		CheckPropertiesCreated();
		return properties.ContainsKey(typeof(T));
	}

	public bool TryGetProperty<T>(out T property) where T : SerializedItemProperty
	{
		CheckPropertiesCreated();
		if (properties.TryGetValue(typeof(T), out var value))
		{
			property = (T)value;
			return true;
		}
		property = null;
		return false;
	}

	private void CheckPropertiesCreated()
	{
		if (properties == null)
		{
			properties = new Dictionary<Type, SerializedItemProperty>();
		}
	}

	public bool TryAddItem(Item sourceItem)
	{
		if (CanAddItemCount(sourceItem) != sourceItem.Count || sourceItem.IsEmpty || Count == 0)
		{
			return false;
		}
		Count += sourceItem.Count;
		sourceItem.Count = 0;
		return true;
	}

	public bool TryAddItemNoChangeSource(in Item sourceItem)
	{
		if (CanAddItemCount(sourceItem) != sourceItem.Count || sourceItem.IsEmpty || Count == 0)
		{
			return false;
		}
		Count += sourceItem.Count;
		return true;
	}

	public bool TryAddItemPartial(Item sourceItem)
	{
		int num = CanAddItemCount(sourceItem);
		if (num == 0 || sourceItem.IsEmpty)
		{
			return false;
		}
		Count += num;
		sourceItem.Count -= num;
		return true;
	}

	public bool CanAddItem(Item sourceItem)
	{
		return CanAddItemCount(sourceItem) > 0;
	}

	public int CanAddItemCount(Item sourceItem, int countToAdd)
	{
		if (sourceItem == null || sourceItem.IsEmpty)
		{
			return 0;
		}
		if (id != sourceItem.id)
		{
			return 0;
		}
		int num = base.Definition.stackCount - Count;
		if (countToAdd < num)
		{
			return countToAdd;
		}
		return num;
	}

	public int CanAddItemCount(Item sourceItem)
	{
		return CanAddItemCount(sourceItem, sourceItem.Count);
	}

	public Item Split(int newPartCount, bool transferInternalData = false)
	{
		if (newPartCount <= 0)
		{
			Debug.LogError($"Item.Split: incorrect splitting value: [{newPartCount}] for item [{id}]");
			return Empty;
		}
		if (newPartCount > Count)
		{
			Debug.LogWarning($"Item.Split: incorrect splitting value: [{newPartCount}] for item [{id}]");
			newPartCount = Mathf.Min(newPartCount, Count);
		}
		Item item = new Item(id, newPartCount);
		Count -= newPartCount;
		if (transferInternalData)
		{
			item.inventory = inventory;
			inventory = new List<Item>();
			foreach (KeyValuePair<Type, SerializedItemProperty> property in properties)
			{
				item.properties.Add(property.Key, property.Value.Clone());
			}
		}
		return item;
	}

	public Item GetItemByType(ItemType toolType)
	{
		foreach (Item item in inventory)
		{
			if (item.Definition.type == toolType)
			{
				return item;
			}
		}
		return Empty;
	}

	public void Sort(Comparison<Item> comparison)
	{
		inventory.Sort(comparison);
	}

	public bool AddItemToInventory(Item sourceItem, bool ignoreAllBags = false)
	{
		List<Item> addedItems;
		return AddItemToInventory(sourceItem, out addedItems, null, ignoreAllBags);
	}

	public bool AddItemToInventory(Item sourceItem, out List<Item> addedItems, Item ignoredBag = null, bool ignoreAllBags = false)
	{
		addedItems = new List<Item>();
		int num = CanAddItemCountToInventory(sourceItem, considerEmptySlots: true, ignoredBag, ignoreAllBags);
		if (num == 0 || sourceItem.Count == 0)
		{
			return false;
		}
		int stackCount = sourceItem.Definition.stackCount;
		if (stackCount == 1)
		{
			Item item = sourceItem.Split(1, transferInternalData: true);
			item.UniqueId.SetGuid(sourceItem.UniqueId);
			bool flag = false;
			if (!ignoreAllBags)
			{
				for (int i = 0; i < inventory.Count; i++)
				{
					if (inventory[i].IsBag && sourceItem.Definition.CanBeInsertedInBag(inventory[i].Definition) && inventory[i].uniqueId != ignoredBag?.uniqueId && inventory[i].CanAddItemToInventory(item) && inventory[i].GetTotalCountInInventory(item.id) > 0)
					{
						inventory[i].InsertItemToInventory(item);
						flag = true;
						break;
					}
				}
			}
			if (HasProperty<AutoExpandSerializedItemProperty>() && InventorySize < 1000 && InventoryFillSize == InventorySize)
			{
				inventorySize++;
			}
			if (!flag && InventoryFillSize != InventorySize)
			{
				InsertItemToInventory(item);
				flag = true;
			}
			if (!ignoreAllBags && !flag)
			{
				for (int j = 0; j < inventory.Count; j++)
				{
					if (inventory[j].IsBag && sourceItem.Definition.CanBeInsertedInBag(inventory[j].Definition) && inventory[j].uniqueId != ignoredBag?.uniqueId && inventory[j].CanAddItemToInventory(item))
					{
						inventory[j].InsertItemToInventory(item);
						flag = true;
						break;
					}
				}
			}
			if (flag)
			{
				addedItems.Add(item);
				CalculateInventoryFillSize();
				return true;
			}
			return false;
		}
		int num2 = num;
		if (!ignoreAllBags)
		{
			for (int k = 0; k < inventory.Count; k++)
			{
				if (inventory[k].IsBag && sourceItem.Definition.CanBeInsertedInBag(inventory[k].Definition) && inventory[k].uniqueId != ignoredBag?.uniqueId && inventory[k].GetTotalCountInInventory(sourceItem.id) > 0)
				{
					int num3 = inventory[k].CanAddItemCountToInventory(sourceItem.Definition, num2, considerEmptySlots: true, null, ignoreAllBags: true);
					addedItems.Add(new Item(sourceItem.id, num3));
					inventory[k].AddItemToInventory(new Item(sourceItem.id, num3));
					num2 -= num3;
				}
				if (num2 <= 0)
				{
					sourceItem.Count = 0;
					return true;
				}
			}
		}
		foreach (Item item3 in inventory)
		{
			if (!item3.IsEmpty)
			{
				int num4 = item3.CanAddItemCount(sourceItem, num2);
				if (num4 > 0)
				{
					Item sourceItem2 = new Item(sourceItem.id, num4);
					addedItems.Add(sourceItem2);
					item3.TryAddItemNoChangeSource(in sourceItem2);
					num2 -= num4;
				}
				else if (num2 <= 0)
				{
					break;
				}
			}
		}
		if (num2 <= 0)
		{
			sourceItem.Count = 0;
			CalculateInventoryFillSize();
			return true;
		}
		int num5 = (HasProperty<AutoExpandSerializedItemProperty>() ? 1000 : (inventorySize - inventoryFillSize));
		int num6 = Mathf.Min(num2, num5 * stackCount);
		while (num6 > 0)
		{
			int num7 = Mathf.Min(num2, stackCount);
			Item item2 = sourceItem.Split(num7);
			num2 -= num7;
			addedItems.Add(item2);
			if (HasProperty<AutoExpandSerializedItemProperty>())
			{
				CalculateInventoryFillSize();
				if (InventoryFillSize == InventorySize)
				{
					inventorySize++;
				}
			}
			if (InventoryFillSize != InventorySize)
			{
				InsertItemToInventory(item2);
				CalculateInventoryFillSize();
			}
			num6 -= num7;
		}
		if (num2 <= 0)
		{
			sourceItem.Count = 0;
			CalculateInventoryFillSize();
			return true;
		}
		if (!ignoreAllBags)
		{
			for (int l = 0; l < inventory.Count; l++)
			{
				if (inventory[l].IsBag && sourceItem.Definition.CanBeInsertedInBag(inventory[l].Definition) && inventory[l].uniqueId != ignoredBag?.uniqueId)
				{
					int num8 = inventory[l].CanAddItemCountToInventory(sourceItem.Definition, num2, considerEmptySlots: true, null, ignoreAllBags: true);
					addedItems.Add(new Item(sourceItem.id, num8));
					inventory[l].AddItemToInventory(new Item(sourceItem.id, num8));
					num2 -= num8;
				}
				if (num2 <= 0)
				{
					sourceItem.Count = 0;
					return true;
				}
			}
		}
		sourceItem.Count = 0;
		return true;
	}

	public bool CanAddItemToInventory(Item sourceItem, bool considerEmptySlots = true, bool ignoreAllBags = false)
	{
		return CanAddItemCountToInventory(sourceItem, considerEmptySlots, null, ignoreAllBags) == sourceItem.count;
	}

	public bool CanAddItemToInventory(string itemId, int count, bool considerEmptySlots = true, bool ignoreAllBags = false)
	{
		if (string.IsNullOrEmpty(itemId) || count == 0)
		{
			return false;
		}
		ItemDef data = GameBalance.Me.GetData<ItemDef>(itemId);
		if (data == null)
		{
			return false;
		}
		return CanAddItemCountToInventory(data, count, considerEmptySlots, null, ignoreAllBags) == count;
	}

	public int CanAddItemCountToInventory(Item sourceItem, int countToAdd, bool considerEmptySlots = true, Item ignoredBag = null, bool ignoreAllBags = false)
	{
		if (sourceItem == null || sourceItem.IsEmpty)
		{
			return 0;
		}
		return CanAddItemCountToInventory(sourceItem.Definition, countToAdd, considerEmptySlots, ignoredBag, ignoreAllBags);
	}

	public int CanAddItemCountToInventory(ItemDef itemDef, int countToAdd, bool considerEmptySlots = true, Item ignoredBag = null, bool ignoreAllBags = false)
	{
		if ((TryGetProperty<WhiteListFilterSerializedItemProperty>(out var property) && !property.WhiteList.Contains(itemDef)) || (TryGetProperty<BlackListFilterSerializedItemProperty>(out var property2) && property2.BlackList.Contains(itemDef)))
		{
			return 0;
		}
		int num = 0;
		int stackCount = itemDef.stackCount;
		List<Item> list = new List<Item>(inventory);
		int num2 = 0;
		int num3 = 0;
		for (int i = 0; i < inventory.Count; i++)
		{
			if (!ignoreAllBags && inventory[i].IsBag && itemDef.CanBeInsertedInBag(inventory[i].Definition) && inventory[i].uniqueId != ignoredBag?.uniqueId)
			{
				list.AddRange(inventory[i].inventory);
				if (considerEmptySlots)
				{
					num2 += inventory[i].inventorySize;
					num3 += inventory[i].inventoryFillSize;
				}
			}
		}
		foreach (Item item in list)
		{
			if (item.id == itemDef.id)
			{
				num += stackCount - item.Count;
			}
		}
		if (considerEmptySlots)
		{
			int num4 = (HasProperty<AutoExpandSerializedItemProperty>() ? 1000 : (inventorySize + num2));
			num += (num4 - (inventoryFillSize + num3)) * stackCount;
		}
		if (countToAdd < num)
		{
			return countToAdd;
		}
		return num;
	}

	public int CanAddItemCountToInventory(Item sourceItem, bool considerEmptySlots = true, Item ignoredBag = null, bool ignoreAllBags = false)
	{
		return CanAddItemCountToInventory(sourceItem, sourceItem.Count, considerEmptySlots, ignoredBag, ignoreAllBags);
	}

	public bool TryGetItemInInventory(string itemId, out Item itemResult)
	{
		itemResult = Empty;
		foreach (Item item in inventory)
		{
			if (itemId == item.id)
			{
				itemResult = item;
				return true;
			}
		}
		foreach (Item item2 in inventory)
		{
			if (item2.IsBag && item2.TryGetItemInInventory(itemId, out itemResult))
			{
				return true;
			}
		}
		return false;
	}

	public Item TryFindSourceBagForItem(Item item)
	{
		for (int i = 0; i < inventory.Count; i++)
		{
			Item item2 = inventory[i];
			if (!item2.IsBag)
			{
				continue;
			}
			for (int j = 0; j < item2.inventory.Count; j++)
			{
				if (item2.inventory[j].UniqueId == item.UniqueId)
				{
					return item2;
				}
			}
		}
		return null;
	}

	public bool TryGetItemInInventoryByGUID(string uniqueId, out Item itemResult)
	{
		itemResult = Empty;
		for (int i = 0; i < inventory.Count; i++)
		{
			Item item = inventory[i];
			if (item.uniqueId.Id == uniqueId)
			{
				itemResult = item;
				return true;
			}
		}
		for (int j = 0; j < inventory.Count; j++)
		{
			Item item2 = inventory[j];
			if (item2.IsBag && item2.TryGetItemInInventoryByGUID(uniqueId, out itemResult))
			{
				return true;
			}
		}
		return false;
	}

	public bool TryGetItemInInventoryByGroupId(string groupId, out Item itemResult)
	{
		itemResult = Empty;
		for (int i = 0; i < inventory.Count; i++)
		{
			Item item = inventory[i];
			if (item.Definition.itemGroupIds.Contains(groupId))
			{
				itemResult = item;
				return true;
			}
		}
		for (int j = 0; j < inventory.Count; j++)
		{
			Item item2 = inventory[j];
			if (item2.IsBag && item2.TryGetItemInInventoryByGroupId(groupId, out itemResult))
			{
				return true;
			}
		}
		return false;
	}

	public List<Item> RemoveItemFromInventoryById(string itemId, int count = -1, Item ignoredBag = null, Item sourceBag = null, bool ignoreAllBags = false)
	{
		return RemoveItemFromInventory(itemId, count, IsItemTheSameByItemId, ignoredBag, sourceBag, ignoreAllBags);
	}

	public List<Item> RemoveItemFromInventoryByGroup(string groupId, int count = -1)
	{
		return RemoveItemFromInventory(groupId, count, IsItemTheSameByGroupId);
	}

	public List<Item> RemoveItemFromInventoryByStarGroup(string groupId, int count = -1)
	{
		return RemoveItemFromInventory(groupId, count, IsItemTheSameByStarGroupId);
	}

	public Item RemoveItemFromInventoryByUID(Guid uniqueId, int count = -1)
	{
		Item empty = Empty;
		for (int i = 0; i < inventory.Count; i++)
		{
			Item item = inventory[i];
			if (IsItemTheSameByUId(item, uniqueId))
			{
				int num = ((count == -1) ? item.count : count);
				if (item.count <= num)
				{
					empty = item;
					inventory.RemoveAt(i);
				}
				else
				{
					empty = item.Split(num);
				}
				CalculateInventoryFillSize();
				return empty;
			}
			if (!item.IsBag)
			{
				continue;
			}
			for (int j = 0; j < item.inventory.Count; j++)
			{
				Item item2 = item.inventory[j];
				if (IsItemTheSameByUId(item2, uniqueId))
				{
					int num2 = ((count == -1) ? item2.count : count);
					if (item2.count <= num2)
					{
						empty = item2;
						item.inventory.RemoveAt(j);
					}
					else
					{
						empty = item2.Split(num2);
					}
					item.CalculateInventoryFillSize();
					return empty;
				}
			}
		}
		return empty;
	}

	private List<Item> RemoveItemFromInventory(string itemId, int count = -1, Func<Item, string, bool> removeCondition = null, Item ignoredBag = null, Item sourceBag = null, bool ignoreAllBags = false)
	{
		List<Item> removedItems = new List<Item>();
		int num = 0;
		int leftToRemoveCount = count;
		bool removeFullCount = count == -1;
		ItemDef data = GameBalance.Me.GetData<ItemDef>(itemId);
		if (!ignoreAllBags && sourceBag != null)
		{
			BagIteration(sourceBag);
		}
		List<Item> list = new List<Item>();
		for (int num2 = inventory.Count - 1; num2 >= 0 && leftToRemoveCount != 0; num2--)
		{
			Item item = inventory[num2];
			if (sourceBag != null && item == sourceBag)
			{
				continue;
			}
			if (!ignoreAllBags && item.IsBag && data != null && data.CanBeInsertedInBag(item.Definition) && ignoredBag?.uniqueId != item.uniqueId)
			{
				if (ignoredBag == null)
				{
					list.Add(item);
					continue;
				}
				if (ignoredBag.uniqueId != item.uniqueId)
				{
					list.Add(item);
					continue;
				}
			}
			Func<Item, string, bool> func = removeCondition;
			if (func == null || func(item, itemId))
			{
				int num3 = (removeFullCount ? item.count : leftToRemoveCount);
				if (item.count <= num3)
				{
					removedItems.Add(item);
					num = item.count;
					inventory.RemoveAt(num2);
				}
				else
				{
					removedItems.Add(item.Split(num3));
					num = num3;
				}
				if (!removeFullCount)
				{
					leftToRemoveCount -= num;
				}
			}
		}
		if (!ignoreAllBags && (removeFullCount || leftToRemoveCount > 0))
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (!removeFullCount && leftToRemoveCount <= 0)
				{
					break;
				}
				BagIteration(list[i]);
			}
		}
		CalculateInventoryFillSize();
		return removedItems;
		void BagIteration(Item bag)
		{
			if (removeFullCount || leftToRemoveCount > 0)
			{
				int num4 = (removeFullCount ? (-1) : leftToRemoveCount);
				List<Item> list2 = bag.RemoveItemFromInventory(itemId, num4, removeCondition);
				removedItems.AddRange(list2);
				if (!removeFullCount)
				{
					for (int j = 0; j < list2.Count; j++)
					{
						leftToRemoveCount -= list2[j].count;
					}
				}
			}
		}
	}

	private int GetLastItemIndexInInventory(string itemId)
	{
		for (int num = inventory.Count - 1; num >= 0; num--)
		{
			if (!(inventory[num].id != itemId))
			{
				return num;
			}
		}
		return -1;
	}

	private Item GetLastItemInInventory(string itemId)
	{
		int lastItemIndexInInventory = GetLastItemIndexInInventory(itemId);
		if (lastItemIndexInInventory != -1)
		{
			return inventory[lastItemIndexInInventory];
		}
		return null;
	}

	public int GetTotalCountInInventory(string itemId, Item ignoredBag = null, bool ignoreAllBags = false)
	{
		if (string.IsNullOrEmpty(itemId))
		{
			return 0;
		}
		int num = 0;
		foreach (Item item in inventory)
		{
			if (item == ignoredBag)
			{
				continue;
			}
			if (IsItemTheSameByItemId(item, itemId))
			{
				num += item.count;
			}
			if (!item.IsBag || ignoreAllBags)
			{
				continue;
			}
			foreach (Item item2 in item.inventory)
			{
				if (IsItemTheSameByItemId(item2, itemId))
				{
					num += item2.count;
				}
			}
		}
		return num;
	}

	public int GetTotalCountInInventory(Guid uniqueId)
	{
		int num = 0;
		foreach (Item item in inventory)
		{
			if (IsItemTheSameByUId(item, uniqueId))
			{
				num += item.count;
			}
			if (!item.IsBag)
			{
				continue;
			}
			foreach (Item item2 in item.inventory)
			{
				if (IsItemTheSameByUId(item2, uniqueId))
				{
					num += item2.count;
				}
			}
		}
		return num;
	}

	public bool IsInventoryEmpty()
	{
		foreach (Item item in inventory)
		{
			if (item.count > 0)
			{
				return false;
			}
		}
		return true;
	}

	public bool HasItemQuantityInInventory(string itemId, int count)
	{
		return GetTotalCountInInventory(itemId) >= count;
	}

	public bool HasItemWithEnoughDurability(string itemId, float needDurability)
	{
		for (int i = 0; i < inventory.Count; i++)
		{
			if (inventory[i].id == itemId && inventory[i].TryGetProperty<DurabilitySerializedItemProperty>(out var property) && property.Durability >= needDurability)
			{
				return true;
			}
		}
		return false;
	}

	public Item GetAndRemoveItemWithEnoughDurability(string itemId, float needDurability)
	{
		Item item = null;
		for (int i = 0; i < inventory.Count; i++)
		{
			if (inventory[i].id == itemId && inventory[i].TryGetProperty<DurabilitySerializedItemProperty>(out var property) && property.Durability >= needDurability)
			{
				item = inventory[i];
				inventory.RemoveAt(i);
				break;
			}
			ItemDef data = GameBalance.Me.GetData<ItemDef>(itemId);
			if (inventory[i].IsBag && data != null && data.CanBeInsertedInBag(inventory[i].Definition))
			{
				item = inventory[i].GetAndRemoveItemWithEnoughDurability(itemId, needDurability);
				if (item != null)
				{
					break;
				}
			}
		}
		CalculateInventoryFillSize();
		return item;
	}

	public bool CanTakeAnyItemsExistingInMeFromOtherInventory(Item otherInventory, bool ignoreMyBags, bool ignoreOtherBags)
	{
		if (otherInventory == null)
		{
			return false;
		}
		HashSet<string> hashSet = CollectUniqueItemIds(ignoreMyBags);
		for (int i = 0; i < otherInventory.inventory.Count; i++)
		{
			Item item = otherInventory.inventory[i];
			if (item.Definition.stackCount > 1)
			{
				if (hashSet.Contains(item.id) && CanAddItemCountToInventory(item, considerEmptySlots: true, null, ignoreMyBags) > 0)
				{
					return true;
				}
			}
			else
			{
				if (ignoreOtherBags || !item.IsBag)
				{
					continue;
				}
				for (int j = 0; j < item.inventory.Count; j++)
				{
					Item item2 = item.inventory[j];
					if (item2.Definition.stackCount > 1 && hashSet.Contains(item2.id) && CanAddItemCountToInventory(item2, considerEmptySlots: true, null, ignoreMyBags) > 0)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public List<Item> TakeAllItemsExistingInMeFromOtherInventory(Item otherInventory, bool ignoreMyBags, bool ignoreOtherBags)
	{
		List<Item> list = new List<Item>();
		HashSet<string> hashSet = CollectUniqueItemIds(ignoreMyBags);
		for (int num = otherInventory.inventory.Count - 1; num >= 0; num--)
		{
			Item item = otherInventory.inventory[num];
			if (item.Definition.stackCount > 1)
			{
				if (hashSet.Contains(item.id) && AddItemToInventory(new Item(item.id, item.Count), ignoreMyBags))
				{
					otherInventory.RemoveItemFromInventory(item.id, item.Count, IsItemTheSameByItemId, null, null, ignoreOtherBags);
					if (!list.Contains(otherInventory))
					{
						list.Add(otherInventory);
					}
				}
			}
			else if (!ignoreOtherBags && item.IsBag)
			{
				for (int num2 = item.inventory.Count - 1; num2 >= 0; num2--)
				{
					Item item2 = item.inventory[num2];
					if (item2.Definition.stackCount > 1 && hashSet.Contains(item2.id) && AddItemToInventory(new Item(item2.id, item2.Count), ignoreMyBags))
					{
						item.RemoveItemFromInventory(item2.id, item2.Count, IsItemTheSameByItemId, null, null, ignoreOtherBags);
						if (!list.Contains(item2))
						{
							list.Add(item2);
						}
					}
				}
			}
		}
		return list;
	}

	private HashSet<string> CollectUniqueItemIds(bool ignoreBags)
	{
		HashSet<string> hashSet = new HashSet<string>();
		for (int i = 0; i < inventory.Count; i++)
		{
			Item item = inventory[i];
			hashSet.Add(item.id);
			if (!ignoreBags && item.IsBag)
			{
				for (int j = 0; j < item.inventory.Count; j++)
				{
					hashSet.Add(item.inventory[j].id);
				}
			}
		}
		return hashSet;
	}

	public bool AddItemsToInventory(Item inventoryItem, out List<Item> addedItems)
	{
		return AddItemsToInventory(inventoryItem.inventory, out addedItems);
	}

	public bool AddItemsToInventory(List<Item> itemsToAdd)
	{
		List<Item> addedItems;
		return AddItemsToInventory(itemsToAdd, out addedItems);
	}

	public bool AddItemsToInventory(List<Item> itemsToAdd, out List<Item> addedItems)
	{
		addedItems = new List<Item>();
		if (!CanAddItemsToInventory(itemsToAdd))
		{
			return false;
		}
		foreach (Item item in itemsToAdd)
		{
			AddItemToInventory(item, out var addedItems2);
			addedItems.AddRange(addedItems2);
		}
		return true;
	}

	public bool CanAddItemsToInventory(Item inventoryItem)
	{
		return CanAddItemsToInventory(inventoryItem.inventory);
	}

	public bool CanAddItemsToInventory(List<Item> itemsToAdd)
	{
		List<ItemCount> list = new List<ItemCount>();
		foreach (Item item in itemsToAdd)
		{
			if (!item.IsEmpty)
			{
				list.Add(new ItemCount(item.Definition, item.count));
			}
		}
		return CanAddItemsToInventory(list);
	}

	public bool CanAddItemsToInventory(List<ItemCount> itemCounts, out List<ItemCount> cantAddItemsCount)
	{
		cantAddItemsCount = new List<ItemCount>();
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		Dictionary<string, ItemDef> dictionary2 = new Dictionary<string, ItemDef>();
		foreach (ItemCount itemCount in itemCounts)
		{
			if (!itemCount.IsEmpty && (!TryGetProperty<WhiteListFilterSerializedItemProperty>(out var property) || property.WhiteList.Contains(itemCount.Def)) && (!TryGetProperty<BlackListFilterSerializedItemProperty>(out var property2) || !property2.BlackList.Contains(itemCount.Def)))
			{
				if (!dictionary.TryAdd(itemCount.itemId, itemCount.count))
				{
					dictionary[itemCount.itemId] += itemCount.count;
				}
				dictionary2.TryAdd(itemCount.itemId, itemCount.Def);
			}
		}
		foreach (string item in new List<string>(dictionary.Keys))
		{
			int num = dictionary[item];
			if (num != 0 && dictionary2.TryGetValue(item, out var value))
			{
				int num2 = CanAddItemCountToInventory(value, num, considerEmptySlots: false);
				if (num2 != 0 && num2 <= num)
				{
					dictionary[item] -= num2;
				}
			}
		}
		int num3 = (HasProperty<AutoExpandSerializedItemProperty>() ? 1000 : inventorySize) - InventoryFillSize;
		if (num3 > 0)
		{
			foreach (string item2 in new List<string>(dictionary.Keys))
			{
				if (num3 == 0)
				{
					break;
				}
				if (dictionary2.TryGetValue(item2, out var value2))
				{
					while (dictionary[item2] > 0 && num3 > 0)
					{
						dictionary[item2] -= Mathf.Min(value2.stackCount, dictionary[item2]);
						num3--;
					}
				}
			}
		}
		bool result = true;
		foreach (KeyValuePair<string, int> item3 in dictionary)
		{
			string key = item3.Key;
			int value3 = item3.Value;
			if (value3 > 0)
			{
				result = false;
				if (dictionary2.TryGetValue(key, out var value4))
				{
					cantAddItemsCount.Add(new ItemCount(value4, value3));
				}
			}
		}
		return result;
	}

	public bool CanAddItemsToInventory(List<ItemCount> itemCounts)
	{
		List<ItemCount> cantAddItemsCount;
		return CanAddItemsToInventory(itemCounts, out cantAddItemsCount);
	}

	public bool HasItemsWithIds(List<NeedItemData> items, WgoData wgoData)
	{
		return HasItemsWithIds(items, 1, wgoData);
	}

	public bool HasItemsWithIds(List<NeedItemData> items, int multiplicator = 1, WgoData wgoData = null)
	{
		Dictionary<string, int> dictionary = new Dictionary<string, int>(items.Count);
		for (int i = 0; i < items.Count; i++)
		{
			if (!dictionary.TryGetValue(items[i].Id, out var value))
			{
				dictionary.Add(items[i].Id, items[i].GetCount(wgoData) * multiplicator);
			}
			else
			{
				dictionary[items[i].Id] = value + items[i].GetCount(wgoData) * multiplicator;
			}
		}
		foreach (KeyValuePair<string, int> item in dictionary)
		{
			if (!HasItemQuantityInInventory(item.Key, item.Value))
			{
				return false;
			}
		}
		return true;
	}

	public bool HasItemsByItemType(ItemType type)
	{
		return inventory.Exists((Item x) => x.Definition.type == type);
	}

	public bool HasItemByItemId(string itemId, int count)
	{
		return GetTotalCountInInventory(itemId) >= count;
	}

	public bool HasItemsByItemId(List<Item> items)
	{
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		foreach (Item item in items)
		{
			if (dictionary.ContainsKey(item.id))
			{
				dictionary[item.id] += item.Count;
			}
			else
			{
				dictionary.TryAdd(item.id, item.Count);
			}
		}
		foreach (KeyValuePair<string, int> item2 in dictionary)
		{
			if (item2.Value - GetTotalCountInInventory(item2.Key) > 0)
			{
				return false;
			}
		}
		return true;
	}

	public bool HasItemsByUniqueId(List<Item> items)
	{
		foreach (Item item in items)
		{
			if (item.count > GetTotalCountInInventory(item.UniqueId.Guid))
			{
				return false;
			}
		}
		return true;
	}

	public List<Item> RemoveAllItems()
	{
		List<Item> list = new List<Item>();
		for (int num = inventory.Count - 1; num >= 0; num--)
		{
			list.Add(RemoveItemFromInventoryByUID(inventory[num].UniqueId.Guid));
		}
		inventory.Clear();
		CalculateInventoryFillSize();
		return list;
	}

	public Item RemoveItemAt(int index)
	{
		if (index < 0 || index >= inventory.Count)
		{
			return null;
		}
		Item result = inventory[index];
		inventory.RemoveAt(index);
		CalculateInventoryFillSize();
		return result;
	}

	public bool RemoveItemsFromInventory(List<Item> items)
	{
		if (!HasItemsByItemId(items))
		{
			return false;
		}
		foreach (Item item in items)
		{
			RemoveItemFromInventory(item.id, item.count);
		}
		return false;
	}

	public bool RemoveItemsFromInventoryByUniqueId(List<Item> items)
	{
		if (!HasItemsByUniqueId(items))
		{
			return false;
		}
		foreach (Item item in items)
		{
			RemoveItemFromInventoryByUID(item.UniqueId.Guid);
		}
		return true;
	}

	private static bool IsItemTheSameByItemId(Item item, string itemId)
	{
		return item.id == itemId;
	}

	private static bool IsItemTheSameByUId(Item item, Guid uniqueId)
	{
		return uniqueId == item.UniqueId.Guid;
	}

	private static bool IsItemTheSameByGroupId(Item item, string groupId)
	{
		return item.Definition.itemGroupIds.Contains(groupId);
	}

	private static bool IsItemTheSameByStarGroupId(Item item, string starGroupId)
	{
		if (!GameBalance.Me.starGroupItemsCache.TryGetValue(starGroupId, out var value))
		{
			return false;
		}
		return value.Contains(item.Definition);
	}

	private void CalculateInventoryFillSize()
	{
		inventoryFillSize = 0;
		foreach (Item item in inventory)
		{
			if (!item.IsEmpty)
			{
				inventoryFillSize++;
			}
			if (item.IsBag)
			{
				item.CalculateInventoryFillSize();
			}
		}
	}

	private void InsertItemToInventory(Item item)
	{
		int num = inventory.BinarySearch(item, Comparer<Item>.Create(delegate(Item a, Item b)
		{
			int num2 = a.Definition.sortOrder.CompareTo(b.Definition.sortOrder);
			if (num2 != 0)
			{
				return num2;
			}
			int num3 = string.Compare(a.id, b.id, StringComparison.Ordinal);
			return (num3 != 0) ? num3 : b.count.CompareTo(a.count);
		}));
		if (num < 0)
		{
			num = ~num;
		}
		inventory.Insert(num, item);
	}
}
