using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Inventory
{
	private const string INVENTORY_ITEM_NAME = "inventory";

	private const string CRAFT_INVENTORY_ITEM_NAME = "craftInventory";

	private const int DEFAULT_INVENTORY_SIZE = 40;

	[SerializeField]
	private Item inventoryItem = new Item();

	[SerializeField]
	private string customViewId;

	[NonSerialized]
	private Inventory parentInventory;

	public Item Data => inventoryItem;

	public string ViewId
	{
		get
		{
			if (!string.IsNullOrEmpty(customViewId))
			{
				return customViewId;
			}
			return Data.id;
		}
	}

	public Inventory ParentInventory
	{
		get
		{
			return parentInventory;
		}
		set
		{
			parentInventory = value;
		}
	}

	public event Action<List<Item>> OnItemsAdd;

	public event Action<List<Item>> OnItemsRemove;

	public event Action<Item> OnBagRemoved;

	public event Action<Item> OnBagAdded;

	public event Action OnInventoryFull;

	public Inventory()
	{
	}

	public Inventory(string inventoryId, int inventorySize)
	{
		inventoryItem = new Item(inventoryId);
		inventoryItem.InventorySize = inventorySize;
	}

	public Inventory(string inventoryId, int inventorySize, string customViewId)
	{
		inventoryItem = new Item(inventoryId);
		inventoryItem.InventorySize = inventorySize;
		this.customViewId = customViewId;
	}

	public Inventory(string inventoryId, int inventorySize, bool autoExpand)
		: this(inventoryId, inventorySize)
	{
		if (autoExpand)
		{
			inventoryItem.AddProperty(new AutoExpandSerializedItemProperty());
		}
	}

	public static Inventory GetInventoryFromBag(Item item, Inventory parentInventory = null)
	{
		if (item == null || !item.IsBag)
		{
			Debug.LogError("Can't create inventory not from bag item!");
			return null;
		}
		Inventory inventory = new Inventory();
		inventory.inventoryItem = item;
		inventory.inventoryItem.InventorySize = item.Definition.bagSize;
		inventory.ParentInventory = parentInventory;
		return inventory;
	}

	public Inventory(Item item)
	{
		inventoryItem = item;
	}

	public static Inventory GetEmpty()
	{
		return new Inventory("inventory", 0);
	}

	public static Inventory GetDefault()
	{
		return new Inventory("inventory", 40);
	}

	public static Inventory Create(int inventorySize, bool isFuelInventory = false, WhiteListItemFilter whiteList = null, BlackListItemFilter blackList = null, string customViewId = "")
	{
		Inventory inventory = new Inventory("inventory", inventorySize, customViewId);
		if (isFuelInventory)
		{
			inventory.Data.AddProperty(new FuelContainerSerializedItemProperty());
		}
		if (whiteList != null && !whiteList.IsEmpty)
		{
			inventory.Data.AddProperty(new WhiteListFilterSerializedItemProperty(whiteList));
		}
		if (blackList != null && !blackList.IsEmpty)
		{
			inventory.Data.AddProperty(new BlackListFilterSerializedItemProperty(blackList));
		}
		return inventory;
	}

	public static Inventory GetCraftInventory(int size)
	{
		return new Inventory("craftInventory", size);
	}

	public float GetTotalQuality()
	{
		float num = 0f;
		foreach (Item item in inventoryItem.Inventory)
		{
			num += (float)item.Definition.quality;
		}
		return num;
	}

	public float GetTotalQualityGrave()
	{
		float num = 0f;
		int num2 = 0;
		int num3 = 0;
		foreach (Item item in inventoryItem.Inventory)
		{
			num += (float)item.Definition.quality;
			if (!item.Definition.itemGroupIds.Contains("body"))
			{
				continue;
			}
			foreach (Item item2 in item.Inventory)
			{
				num3 += item2.Definition.redSkulls * item2.Count;
				num2 += item2.Definition.whiteSkulls * item2.Count;
			}
		}
		num3 = Mathf.Clamp(num3, 0, 999);
		num2 = Mathf.Clamp(num2, 0, 999);
		num -= (float)num3;
		return Mathf.Clamp(num, -999f, num2);
	}

	public bool AddItemToInventory(Item item, Item ignoredBag = null, bool ignoreAllBags = false)
	{
		List<Item> addedItems;
		return TryAddItemToInventory(item, out addedItems, ignoredBag, ignoreAllBags);
	}

	public Item TryFindSourceBagForItem(Item item)
	{
		return inventoryItem.TryFindSourceBagForItem(item);
	}

	public bool AddItemToInventory(Item item, out List<Item> addedItems, Item ignoredBag = null, bool ignoreAllBags = false)
	{
		return TryAddItemToInventory(item, out addedItems, ignoredBag, ignoreAllBags);
	}

	public bool CanAddItemToInventory(Item item)
	{
		return inventoryItem.CanAddItemToInventory(item);
	}

	public bool CanAddItemToInventory(string itemId, int count)
	{
		return inventoryItem.CanAddItemToInventory(itemId, count);
	}

	public bool AddItemsToInventory(Inventory other)
	{
		if (inventoryItem.AddItemsToInventory(other.Data, out var addedItems))
		{
			foreach (Item item in addedItems)
			{
				if (item.IsBag)
				{
					this.OnBagAdded?.Invoke(item);
				}
			}
			NotifyItemsAdded(addedItems);
			return true;
		}
		this.OnInventoryFull?.Invoke();
		return false;
	}

	public bool AddItemsToInventory(List<Item> items)
	{
		if (inventoryItem.AddItemsToInventory(items, out var addedItems))
		{
			foreach (Item item in addedItems)
			{
				if (item.IsBag)
				{
					this.OnBagAdded?.Invoke(item);
				}
			}
			NotifyItemsAdded(addedItems);
			return true;
		}
		this.OnInventoryFull?.Invoke();
		return false;
	}

	public bool AddItemsToNestedItemById(string itemId, List<Item> itemsToAdd)
	{
		Item itemById = GetItemById(itemId);
		if (itemById == null)
		{
			Debug.LogError("Add to nested item failed, Can't find nested item [" + itemId + "]");
			return false;
		}
		if (itemById.AddItemsToInventory(itemsToAdd))
		{
			return true;
		}
		return false;
	}

	public bool AddItemsToNestedItemByGroupId(string groupId, List<Item> itemsToAdd)
	{
		Item itemByGroupId = GetItemByGroupId(groupId);
		if (itemByGroupId == null)
		{
			Debug.LogError("Add to nested item failed, Can't find nested item with group [" + groupId + "]");
			return false;
		}
		if (itemByGroupId.AddItemsToInventory(itemsToAdd))
		{
			if (groupId == "body")
			{
				ZombieWgoData zombie = MainGame.ZombieSystemData.GetZombie(itemByGroupId.UniqueId);
				if (zombie != null)
				{
					foreach (Item item in itemsToAdd)
					{
						zombie.SetZombieItem(itemByGroupId);
						zombie.OnAddOrgan(item);
					}
				}
			}
			return true;
		}
		return false;
	}

	public List<Item> RemoveItemsFromNestedItemById(string itemId, List<NeedItemData> items, WgoData wgoData = null)
	{
		List<Item> list = new List<Item>();
		Item itemById = GetItemById(itemId);
		if (itemById == null)
		{
			Debug.LogError("Remove from nested item failed, Can't find nested item [" + itemId + "]");
			return list;
		}
		foreach (NeedItemData item in items)
		{
			switch (item.groupType)
			{
			case ItemGroup.None:
				list.AddRange(itemById.RemoveItemFromInventoryById(item.Id, item.GetCount(wgoData)));
				break;
			case ItemGroup.Common:
				list.AddRange(itemById.RemoveItemFromInventoryByGroup(item.Id, item.GetCount(wgoData)));
				break;
			case ItemGroup.Star:
				list.AddRange(itemById.RemoveItemFromInventoryByStarGroup(item.Id, item.GetCount(wgoData)));
				break;
			}
		}
		return list;
	}

	public List<Item> RemoveItemsFromNestedItemByGroupId(string groupId, List<NeedItemData> items, WgoData wgoData = null)
	{
		List<Item> list = new List<Item>();
		Item itemByGroupId = GetItemByGroupId(groupId);
		if (itemByGroupId == null)
		{
			Debug.LogError("Remove from nested item failed, Can't find nested item with group [" + groupId + "]");
			return list;
		}
		foreach (NeedItemData item in items)
		{
			switch (item.groupType)
			{
			case ItemGroup.None:
				list.AddRange(itemByGroupId.RemoveItemFromInventoryById(item.Id, item.GetCount(wgoData)));
				break;
			case ItemGroup.Common:
				list.AddRange(itemByGroupId.RemoveItemFromInventoryByGroup(item.Id, item.GetCount(wgoData)));
				break;
			case ItemGroup.Star:
				list.AddRange(itemByGroupId.RemoveItemFromInventoryByStarGroup(item.Id, item.GetCount(wgoData)));
				break;
			}
		}
		if (groupId == "body")
		{
			ZombieWgoData zombie = MainGame.ZombieSystemData.GetZombie(itemByGroupId.UniqueId);
			if (zombie != null)
			{
				foreach (Item item2 in list)
				{
					zombie.SetZombieItem(itemByGroupId);
					zombie.OnRemoveOrgan(item2);
				}
			}
		}
		return list;
	}

	public bool CanAddItemsToInventory(Inventory other)
	{
		return inventoryItem.CanAddItemsToInventory(other.Data);
	}

	public bool CanAddItemsToInventory(List<Item> items)
	{
		return inventoryItem.CanAddItemsToInventory(items);
	}

	public bool CanAddItemsToInventory(List<ItemCount> itemCounts, out List<ItemCount> cantAddItemsCount)
	{
		cantAddItemsCount = new List<ItemCount>();
		return inventoryItem.CanAddItemsToInventory(itemCounts, out cantAddItemsCount);
	}

	public bool CanAddItemsToInventory(List<ItemCount> itemCounts)
	{
		List<ItemCount> cantAddItemsCount;
		return CanAddItemsToInventory(itemCounts, out cantAddItemsCount);
	}

	public bool CanAddItemsToInventoryConsideringDestination(CraftElementBase craftEl, List<ItemCount> itemCounts)
	{
		if (craftEl is CraftElement { Definition: var definition })
		{
			switch (definition.transferDestinationEnd)
			{
			case TransferDestination.Wgo:
				return CanAddItemsToInventory(itemCounts);
			case TransferDestination.ItemInside:
			{
				string destinationItemEnd2 = definition.destinationItemEnd;
				return GetItemById(destinationItemEnd2)?.CanAddItemsToInventory(itemCounts) ?? false;
			}
			case TransferDestination.GroupItemInside:
			{
				string destinationItemEnd = definition.destinationItemEnd;
				return GetItemByGroupId(destinationItemEnd)?.CanAddItemsToInventory(itemCounts) ?? false;
			}
			}
		}
		return false;
	}

	public bool RemoveItemFromInventoryByUID(Item item, int count = -1)
	{
		Item item2 = inventoryItem.RemoveItemFromInventoryByUID(item.UniqueId.Guid, count);
		if (!item2.IsEmpty)
		{
			if (item2.IsBag)
			{
				this.OnBagRemoved?.Invoke(item);
			}
			NotifyItemsRemoved(new List<Item> { item });
			return true;
		}
		return false;
	}

	public Item GetItemByType(ItemType itemType)
	{
		return inventoryItem.GetItemByType(itemType);
	}

	public Item GetItemByTypes(ItemType[] itemTypes)
	{
		foreach (ItemType itemType in itemTypes)
		{
			Item itemByType = GetItemByType(itemType);
			if (!itemByType.IsEmpty)
			{
				return itemByType;
			}
		}
		return Item.Empty;
	}

	public void Clear()
	{
		List<Item> list = Data.RemoveAllItems();
		if (list.Count <= 0)
		{
			return;
		}
		foreach (Item item in list)
		{
			if (item.IsBag)
			{
				this.OnBagRemoved?.Invoke(item);
			}
		}
		NotifyItemsRemoved(list);
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
			switch (item.groupType)
			{
			case ItemGroup.None:
				list.AddRange(RemoveItemById(item.Id, item.GetCount(wgoData) * multiplicator));
				break;
			case ItemGroup.Common:
				list.AddRange(RemoveItemByGroup(item.Id, item.GetCount(wgoData) * multiplicator));
				break;
			case ItemGroup.Star:
				list.AddRange(RemoveItemByStarGroup(item.Id, item.GetCount(wgoData) * multiplicator));
				break;
			}
		}
		return list;
	}

	public List<Item> RemoveItemByGroup(string groupId, int count)
	{
		List<Item> list = inventoryItem.RemoveItemFromInventoryByGroup(groupId, count);
		if (list.Count > 0)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].IsBag)
				{
					this.OnBagRemoved?.Invoke(list[i]);
				}
			}
			NotifyItemsRemoved(list);
		}
		return list;
	}

	public List<Item> RemoveItemByStarGroup(string starGroupId, int count)
	{
		List<Item> list = inventoryItem.RemoveItemFromInventoryByStarGroup(starGroupId, count);
		if (list.Count > 0)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].IsBag)
				{
					this.OnBagRemoved?.Invoke(list[i]);
				}
			}
			NotifyItemsRemoved(list);
		}
		return list;
	}

	public List<Item> RemoveItemById(string itemId, int count = -1, Item ignoredBag = null, Item sourceBag = null, bool ignoreAllBags = false)
	{
		List<Item> list = inventoryItem.RemoveItemFromInventoryById(itemId, count, ignoredBag, sourceBag, ignoreAllBags);
		if (list.Count > 0)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].IsBag)
				{
					this.OnBagRemoved?.Invoke(list[i]);
				}
			}
			NotifyItemsRemoved(list);
		}
		return list;
	}

	public bool CanTakeAnyItemsExistingInMeFromOtherInventory(Inventory otherInventory, bool ignoreMyBags = true, bool ignoreOtherBags = true)
	{
		if (otherInventory == null)
		{
			return false;
		}
		return inventoryItem.CanTakeAnyItemsExistingInMeFromOtherInventory(otherInventory.inventoryItem, ignoreMyBags, ignoreOtherBags);
	}

	public void TakeAllItemsExistingInMeFromOtherInventory(Inventory otherInventory, bool ignoreMyBags = true, bool ignoreOtherBags = true)
	{
		List<Item> list = inventoryItem.TakeAllItemsExistingInMeFromOtherInventory(otherInventory.inventoryItem, ignoreMyBags, ignoreOtherBags);
		otherInventory.NotifyItemsRemoved(list);
		NotifyItemsAdded(list);
	}

	public Item GetItemById(string itemId)
	{
		inventoryItem.TryGetItemInInventory(itemId, out var itemResult);
		return itemResult;
	}

	public Item GetItemByUniqueId(string uniqueId)
	{
		inventoryItem.TryGetItemInInventoryByGUID(uniqueId, out var itemResult);
		return itemResult;
	}

	public void Sort(Comparison<Item> comparison)
	{
		Data.Sort(comparison);
	}

	public Item GetItemByGroupId(string groupId)
	{
		inventoryItem.TryGetItemInInventoryByGroupId(groupId, out var itemResult);
		return itemResult;
	}

	public List<Item> GetItemsByGroupId(string groupId)
	{
		List<Item> list = new List<Item>();
		foreach (Item item in inventoryItem.Inventory)
		{
			if (item.Definition.itemGroupIds.Contains(groupId))
			{
				list.Add(item);
			}
		}
		return list;
	}

	public List<Item> GetItemsByType(ItemType itemType)
	{
		List<Item> list = new List<Item>();
		foreach (Item item in inventoryItem.Inventory)
		{
			if (item.Definition.type == itemType)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public bool TryAddItemToInventory(Item item, out List<Item> addedItems, Item ignoredBag = null, bool ignoreAllBags = false)
	{
		if (inventoryItem.AddItemToInventory(item, out addedItems, ignoredBag, ignoreAllBags))
		{
			foreach (Item addedItem in addedItems)
			{
				if (addedItem.IsBag)
				{
					this.OnBagAdded?.Invoke(addedItem);
				}
			}
			NotifyItemsAdded(addedItems);
			return true;
		}
		this.OnInventoryFull?.Invoke();
		return false;
	}

	public void ForceTriggerOnItemsAddEventWithoutItems()
	{
		this.OnItemsAdd?.Invoke(new List<Item>());
	}

	public void NotifyItemsAdded(List<Item> addedItems)
	{
		this.OnItemsAdd?.Invoke(addedItems);
		ParentInventory?.NotifyItemsAdded(addedItems);
	}

	public void NotifyItemsRemoved(List<Item> removedItems)
	{
		this.OnItemsRemove?.Invoke(removedItems);
		ParentInventory?.NotifyItemsRemoved(removedItems);
	}
}
