using System.Collections.Generic;
using UnityEngine;

public class MultiInventory
{
	public enum DestinationType
	{
		OnlyFirst,
		AllFromFirst,
		AllFromLast
	}

	public enum PlayerMultiInventory
	{
		DontChange,
		IncludePlayer,
		ExcludePlayer
	}

	private List<Inventory> _inventories = new List<Inventory>();

	public List<Inventory> all => _inventories;

	public float money
	{
		get
		{
			float num = 0f;
			foreach (Inventory inventory in _inventories)
			{
				num += inventory.data.money;
			}
			return num;
		}
	}

	public MultiInventory(Inventory inventory1 = null, Inventory inventory2 = null, Inventory inventory3 = null)
	{
		if (inventory1 != null)
		{
			_inventories.Add(inventory1);
		}
		if (inventory2 != null)
		{
			_inventories.Add(inventory2);
		}
		if (inventory3 != null)
		{
			_inventories.Add(inventory3);
		}
	}

	public MultiInventory(List<Inventory> inventories)
	{
		_inventories = inventories;
	}

	public void AddInventory(Inventory inventory, int specific_position_num = -1)
	{
		if (inventory != null)
		{
			if (specific_position_num > -1 && specific_position_num < _inventories.Count - 1)
			{
				_inventories.Insert(specific_position_num, inventory);
			}
			else
			{
				_inventories.Add(inventory);
			}
		}
	}

	public void SetInventories(List<Inventory> inventories)
	{
		_inventories = inventories;
	}

	public bool AddItem(string id, int count)
	{
		if (IsEmpty())
		{
			return false;
		}
		Item item = new Item(id, count);
		if (CanAddItem(item))
		{
			AddItem(item);
			return true;
		}
		return false;
	}

	public bool AddItems(List<Item> items)
	{
		if (IsEmpty())
		{
			return false;
		}
		if (CanAddItems(items))
		{
			foreach (Item item in items)
			{
				AddItemNoCheck(item);
			}
			return true;
		}
		return false;
	}

	public bool AddItems(List<Item> items, bool allow_bags)
	{
		if (IsEmpty())
		{
			return false;
		}
		if (CanAddItems(items, allow_bags))
		{
			foreach (Item item in items)
			{
				AddItemNoCheck(item, allow_bags);
			}
			return true;
		}
		return false;
	}

	public bool AddItem(Item item)
	{
		if (IsEmpty())
		{
			return false;
		}
		if (CanAddItem(item))
		{
			_inventories[0].data.AddItem(item);
			return true;
		}
		return false;
	}

	public void AddItemNoCheck(Item item, bool allow_bag = true)
	{
		_inventories[0].data.AddItem(item, allow_bag);
	}

	public bool RemoveItem(string item_id, int item_value, DestinationType destination = DestinationType.AllFromLast)
	{
		return RemoveItem(new Item(item_id, item_value), item_value, destination);
	}

	public bool RemoveItems(List<Item> items, DestinationType destination = DestinationType.AllFromLast, List<string> multiquality_ids = null, List<Item> out_really_removed_items = null)
	{
		if (IsEmpty() || !IsEnoughItems(items, destination, multiquality_ids))
		{
			return false;
		}
		if (multiquality_ids == null)
		{
			foreach (Item item2 in items)
			{
				RemoveItemNoCheck(item2, item2.value, destination, "", out_really_removed_items);
			}
		}
		else
		{
			for (int i = 0; i < items.Count; i++)
			{
				Item item = items[i];
				string text = ((i < multiquality_ids.Count) ? multiquality_ids[i] : "");
				if (string.IsNullOrEmpty(text) && item.multiquality_items.Count > 1 && item.definition == null)
				{
					int num = item.value;
					foreach (string multiquality_item in item.multiquality_items)
					{
						int num2 = GetTotalCount(multiquality_item, destination);
						if (num2 > num)
						{
							num2 = num;
						}
						RemoveItem(multiquality_item, num2, destination);
						num -= num2;
						out_really_removed_items?.Add(new Item(multiquality_item, num2));
						if (num <= 0)
						{
							break;
						}
					}
				}
				else
				{
					RemoveItemNoCheck(item, item.value, destination, text, out_really_removed_items);
				}
			}
		}
		return true;
	}

	public bool RemoveItem(Item item, int count = 0, DestinationType destination = DestinationType.AllFromLast)
	{
		if (IsEmpty() || !IsEnoughItem(item, destination))
		{
			return false;
		}
		if (count == 0)
		{
			count = item.value;
		}
		RemoveItemNoCheck(item, count, destination);
		return true;
	}

	public bool TryRemoveSpecificItemNoCheck(Item item, DestinationType destination = DestinationType.AllFromLast)
	{
		foreach (Inventory inventory in _inventories)
		{
			for (int i = 0; i < inventory.data.inventory.Count; i++)
			{
				if (item == inventory.data.inventory[i])
				{
					inventory.data.inventory.RemoveAt(i);
					return true;
				}
				if (!inventory.data.inventory[i].is_bag || !item.CanBeInsertedInBag(inventory.data.inventory[i]))
				{
					continue;
				}
				for (int j = 0; j < inventory.data.inventory[i].inventory.Count; j++)
				{
					Item item2 = inventory.data.inventory[i].inventory[j];
					if (item == item2)
					{
						inventory.data.inventory[i].inventory.RemoveAt(j);
						return true;
					}
				}
			}
			if (destination == DestinationType.OnlyFirst)
			{
				break;
			}
		}
		Debug.LogError("Couldn't find a specific item: " + item.id);
		return false;
	}

	private void RemoveItemNoCheck(Item item, int count = 0, DestinationType destination = DestinationType.AllFromLast, string multiquality_id = "", List<Item> out_really_removed_items = null)
	{
		if (destination == DestinationType.OnlyFirst || _inventories.Count == 1)
		{
			_inventories[0].data.RemoveItemNoCheck(item, count, multiquality_id, out_really_removed_items);
			return;
		}
		GetInteractionIndexes(destination, out var first, out var last, out var step);
		for (int i = first; (step > 0) ? (i < last) : (i >= last); i += step)
		{
			count = _inventories[i].data.RemoveItemOrReturnLeftCount(item, count, multiquality_id, out_really_removed_items);
			if (count == 0)
			{
				break;
			}
		}
	}

	public Item GetItem(string item_id, Item.ItemFindLogics item_find_logics = Item.ItemFindLogics.FirstFound, bool allow_bags = true)
	{
		List<Item> list = null;
		if (item_find_logics != 0)
		{
			list = new List<Item>();
		}
		foreach (Inventory inventory in _inventories)
		{
			Item itemWithID = inventory.data.GetItemWithID(item_id, item_find_logics, allow_bags);
			if (itemWithID != null)
			{
				if (item_find_logics == Item.ItemFindLogics.FirstFound)
				{
					return itemWithID;
				}
				list.Add(itemWithID);
			}
		}
		if (item_find_logics != 0 && list.Count != 0)
		{
			return Item.SortItemsListByFindLogics(list, item_find_logics)[0];
		}
		return null;
	}

	public bool IsEnoughItem(Item item, DestinationType destination = DestinationType.AllFromFirst, string multiquality_id = "", int used_items = 0, int multiplier = 1)
	{
		if (item == null || item.IsEmpty())
		{
			return true;
		}
		if (IsEmpty())
		{
			return false;
		}
		if (destination == DestinationType.OnlyFirst || _inventories.Count == 1)
		{
			return _inventories[0].data.IsEnoughItems(item, multiquality_id, used_items, multiplier);
		}
		bool num = string.IsNullOrEmpty(multiquality_id) && item.multiquality_items.Count > 1;
		int num2 = 0;
		foreach (string item2 in num ? item.multiquality_items : new List<string> { string.IsNullOrEmpty(multiquality_id) ? item.id : multiquality_id })
		{
			num2 += GetTotalCount(item2, destination);
		}
		return num2 - used_items >= item.value * multiplier;
	}

	public bool IsEnoughItems(List<Item> items, DestinationType destination = DestinationType.AllFromFirst, List<string> multiquality_ids = null, int multiplier = 1)
	{
		if (IsEmpty())
		{
			return false;
		}
		if (multiquality_ids == null)
		{
			foreach (Item item2 in items)
			{
				if (!IsEnoughItem(item2, destination, "", 0, multiplier))
				{
					return false;
				}
			}
		}
		else
		{
			Dictionary<string, int> dictionary = new Dictionary<string, int>();
			for (int i = 0; i < items.Count; i++)
			{
				Item item = items[i];
				string text = ((i < multiquality_ids.Count) ? multiquality_ids[i] : "");
				int value = 0;
				if (!string.IsNullOrEmpty(text))
				{
					dictionary.TryGetValue(text, out value);
				}
				if (!IsEnoughItem(item, destination, text, value, multiplier))
				{
					return false;
				}
				if (!string.IsNullOrEmpty(text))
				{
					if (value == 0)
					{
						dictionary.Add(text, item.value);
					}
					else
					{
						dictionary[text] += item.value * multiplier;
					}
				}
			}
		}
		return true;
	}

	public int GetTotalCount(string item_id, DestinationType destination = DestinationType.AllFromFirst, bool count_in_bags = true)
	{
		if (IsEmpty())
		{
			return 0;
		}
		if (destination == DestinationType.OnlyFirst || _inventories.Count == 1)
		{
			return _inventories[0].data.GetTotalCount(item_id, count_in_bags);
		}
		int num = 0;
		GetInteractionIndexes(destination, out var first, out var last, out var step);
		for (int i = first; (step > 0) ? (i < last) : (i >= last); i += step)
		{
			if (_inventories[i].data.is_bag)
			{
				Debug.Log("#BAG# Found bag in multiinventory: " + _inventories[i].data.id + ", first: " + _inventories[0].data.id);
			}
			else
			{
				num += _inventories[i].data.GetTotalCount(item_id, count_in_bags);
			}
		}
		return num;
	}

	public int CanAddCount(string item_id, bool count_bags = false)
	{
		if (IsEmpty())
		{
			return 0;
		}
		int num = 0;
		return _inventories[0].data.CanAddCount(item_id, count_empty: true, count_bags);
	}

	public bool CanAddItem(Item item, bool allow_bags = true)
	{
		if (!IsEmpty())
		{
			return _inventories[0].data.CanAddItem(item, count_empty: true, allow_bags);
		}
		return false;
	}

	public bool CanAddItems(List<Item> items, bool include_bags = false)
	{
		if (!IsEmpty())
		{
			return _inventories[0].data.CanAddItems(items, include_bags);
		}
		return false;
	}

	public bool MoveItemTo(MultiInventory another_inventory, Item item, int count = 0, bool use_only_first_from_inventory = false, bool allow_bag = true)
	{
		count = CheckCountOfMovingItem(item, count, (!use_only_first_from_inventory) ? DestinationType.AllFromFirst : DestinationType.OnlyFirst);
		Item item2 = item;
		item = new Item(item)
		{
			value = count,
			equipped_as = ItemDefinition.EquipmentType.None
		};
		if (!IsEnoughItem(item) || !another_inventory.CanAddItem(item, allow_bag))
		{
			return false;
		}
		if (count == 1 && item2.definition.stack_count == 1)
		{
			if (!TryRemoveSpecificItemNoCheck(item2))
			{
				RemoveItemNoCheck(item);
			}
		}
		else
		{
			RemoveItemNoCheck(item, 0, (!use_only_first_from_inventory) ? DestinationType.AllFromLast : DestinationType.OnlyFirst);
		}
		if (another_inventory.all[0].IsTavernPalette() && item.id.Contains("cup_beer"))
		{
			MainGame.me.save.quests.CheckKeyQuests("put_beer_into_taverns_rack");
		}
		another_inventory.AddItemNoCheck(item, allow_bag);
		return true;
	}

	public bool MoveItemTo(Item another_inventory, Item item, int count = 0)
	{
		count = CheckCountOfMovingItem(item, count);
		Item item2 = item;
		item = new Item(item)
		{
			value = count,
			equipped_as = ItemDefinition.EquipmentType.None
		};
		if (!IsEnoughItem(item) || !another_inventory.CanAddItem(item))
		{
			return false;
		}
		if (count == 1 && item2.definition.stack_count == 1)
		{
			if (!TryRemoveSpecificItemNoCheck(item2))
			{
				RemoveItemNoCheck(item);
			}
		}
		else
		{
			RemoveItemNoCheck(item);
		}
		another_inventory.AddNotFoldedItem(item);
		return true;
	}

	private int CheckCountOfMovingItem(Item item, int count, DestinationType dest = DestinationType.AllFromFirst)
	{
		if (count == 0)
		{
			return item.value;
		}
		int totalCount = GetTotalCount(item.id, dest);
		if (count > totalCount)
		{
			return totalCount;
		}
		return count;
	}

	private bool IsEmpty(bool print_log = true)
	{
		bool num = _inventories.Count == 0;
		if (num && print_log)
		{
			Debug.LogError("empty MultiInventories");
		}
		return num;
	}

	private void GetInteractionIndexes(DestinationType destination, out int first, out int last, out int step)
	{
		int count = _inventories.Count;
		first = ((destination == DestinationType.AllFromLast) ? (count - 1) : 0);
		step = ((destination != DestinationType.AllFromLast) ? 1 : (-1));
		switch (destination)
		{
		case DestinationType.AllFromFirst:
			last = count;
			break;
		case DestinationType.AllFromLast:
			last = 0;
			break;
		default:
			last = ((count > 1) ? 1 : count);
			break;
		}
	}
}
