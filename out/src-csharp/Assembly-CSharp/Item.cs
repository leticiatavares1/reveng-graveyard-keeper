using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using LinqTools;
using UnityEngine;

[Serializable]
public class Item : ISerializationCallbackReceiver
{
	public enum DurabilityState
	{
		Broken,
		PreBroken,
		Used,
		Full
	}

	public enum ItemFindLogics
	{
		FirstFound,
		WithLowestDurability,
		LastFound
	}

	private const bool ALLOW_DEFINITION_CACHE = true;

	public const string TAKEN_FROM_PLAYER_INV = "taken_from_player_inventory";

	public const string INVENTORY_SIZE = "inventory_size";

	public const string RAT_SPEED = "rat_speed";

	public const string RAT_OBEDIENCE = "rat_obedience";

	public const int TOOLBELT_INVENTORY_SIZE = 7;

	public const string WHITE_SKULL_MOD_RES_NAME = "bp_plus_mod";

	public const string RED_SKULL_MOD_RES_NAME = "bp_minus_mod";

	public string id;

	public int value;

	public SmartExpression min_value;

	public SmartExpression max_value;

	public int linked_id = -1;

	public SmartExpression self_chance = new SmartExpression();

	public SmartExpression common_chance = new SmartExpression();

	public int chance_group = -1;

	public bool is_unique;

	public List<string> multiquality_items = new List<string>();

	public ItemDefinition.EquipmentType equipped_as;

	public string drop_zone_id = string.Empty;

	public long worker_unique_id = -1L;

	[SerializeField]
	private GameRes _params = new GameRes();

	[NonSerialized]
	[SmartSerialize]
	public List<Item> inventory = new List<Item>();

	[NonSerialized]
	[SmartSerialize]
	public List<Item> secondary_inventory = new List<Item>();

	[SerializeField]
	[HideInInspector]
	private int _serialize_depth;

	[SmartDontSerialize]
	[HideInInspector]
	[SerializeField]
	private string _serialized_data = "";

	private string _cached_drop_zone_id = string.Empty;

	private WorldZoneDefinition _cached_world_zone;

	private float _cached_zone_dur_k = 1f;

	public string sub_name = "";

	[NonSerialized]
	private BalanceBaseObject _definition;

	[NonSerialized]
	private bool _definition_set;

	[NonSerialized]
	private string _definition_set_id = string.Empty;

	public Worker worker
	{
		get
		{
			if (worker_unique_id > 0)
			{
				return MainGame.me.save.workers.GetWorker(worker_unique_id);
			}
			return null;
		}
	}

	public bool is_worker => worker_unique_id > 0;

	public bool is_bag
	{
		get
		{
			if (definition != null)
			{
				return definition.is_bag;
			}
			return false;
		}
	}

	public int inventory_size
	{
		get
		{
			return _params.GetInt("inventory_size");
		}
		set
		{
			_params.Set("inventory_size", value);
		}
	}

	public float durability
	{
		get
		{
			return _params.durability;
		}
		set
		{
			_params.durability = Mathf.Clamp01(value);
		}
	}

	public DurabilityState durability_state
	{
		get
		{
			float num = durability;
			if (num >= 0.99999f)
			{
				return DurabilityState.Full;
			}
			if (num <= 0.0001f)
			{
				return DurabilityState.Broken;
			}
			if (num < 0.2f)
			{
				return DurabilityState.PreBroken;
			}
			return DurabilityState.Used;
		}
	}

	public static Item empty => new Item();

	public bool is_equipped => equipped_as != ItemDefinition.EquipmentType.None;

	public bool is_equipped_to_toolbar => toolbar_index != -1;

	public int toolbar_index => MainGame.me.save.GetEquippedIndex(id);

	public bool is_multiquality => multiquality_items.Count > 0;

	public ItemDefinition definition
	{
		get
		{
			if (!_definition_set || _definition_set_id != id)
			{
				_definition = ((string.IsNullOrEmpty(id) || id == "empty" || id == "_empty_") ? null : GameBalance.me.GetDataOrNull<ItemDefinition>(id));
				_definition_set = true;
				_definition_set_id = id;
			}
			return (ItemDefinition)_definition;
		}
	}

	public float hp
	{
		get
		{
			return _params.hp;
		}
		set
		{
			_params.hp = value;
		}
	}

	public float progress
	{
		get
		{
			return _params.progress;
		}
		set
		{
			_params.progress = value;
		}
	}

	public float money
	{
		get
		{
			return _params.money;
		}
		set
		{
			_params.money = value;
		}
	}

	public bool is_tech_point => TechDefinition.TECH_POINTS.Contains(id);

	public Item()
		: this("empty", -1)
	{
	}

	public Item(string item_id)
		: this(item_id, 1)
	{
	}

	public Item(string item_id, int item_value)
	{
		id = item_id;
		value = item_value;
		is_unique = false;
		durability = 1f;
		_definition_set = false;
		CheckAndInitBagItem();
	}

	public Item(Item item)
	{
		_definition_set = false;
		if (item == null)
		{
			id = "empty";
			value = -1;
			return;
		}
		id = item.id;
		value = item.value;
		is_unique = item.is_unique;
		linked_id = item.linked_id;
		max_value = item.max_value;
		min_value = item.min_value;
		_params += item._params;
		sub_name = item.sub_name;
		durability = item.durability;
		equipped_as = item.equipped_as;
		CheckAndInitBagItem();
		worker_unique_id = item.worker_unique_id;
		foreach (Item item2 in item.inventory)
		{
			inventory.Add(new Item(item2));
		}
	}

	public void SetItemID(string new_id)
	{
		id = new_id;
		_definition_set = false;
		CheckAndInitBagItem();
	}

	public void Sort()
	{
		inventory.Sort(CompareItems);
	}

	private bool CheckAndInitBagItem()
	{
		if (string.IsNullOrEmpty(id) || id == "empty" || id == "_empty_")
		{
			return false;
		}
		if (definition == null)
		{
			return false;
		}
		if (definition.type != ItemDefinition.ItemType.Bag)
		{
			return false;
		}
		inventory_size = definition.bag_size_x * definition.bag_size_y;
		return true;
	}

	public int CompareItems(Item left, Item right)
	{
		if (left == null && right == null)
		{
			return 0;
		}
		if (left == null)
		{
			return 1;
		}
		if (right == null)
		{
			return -1;
		}
		if (left.definition == null && right.definition == null)
		{
			return 0;
		}
		if (left.definition == null)
		{
			return 1;
		}
		if (right.definition == null)
		{
			return -1;
		}
		if (left.definition.type > right.definition.type)
		{
			return 1;
		}
		if (left.definition.type < right.definition.type)
		{
			return -1;
		}
		if (left.definition.product_weight > right.definition.product_weight)
		{
			return -1;
		}
		if (left.definition.product_weight < right.definition.product_weight)
		{
			return 1;
		}
		if (left.definition.product_tier < right.definition.product_tier)
		{
			return 1;
		}
		if (left.definition.product_tier > right.definition.product_tier)
		{
			return -1;
		}
		if (left.definition.base_price > right.definition.base_price)
		{
			return -1;
		}
		if (left.definition.base_price < right.definition.base_price)
		{
			return 1;
		}
		int num = string.Compare(left.definition.id, right.definition.id, StringComparison.Ordinal);
		if (num != 0)
		{
			return num;
		}
		if (left.value < right.value)
		{
			return 1;
		}
		if (left.value > right.value)
		{
			return -1;
		}
		if (left.durability < right.durability)
		{
			return 1;
		}
		if (left.durability > right.durability)
		{
			return -1;
		}
		return 0;
	}

	public bool IsEmpty()
	{
		if (value > 0 && !string.IsNullOrEmpty(id))
		{
			return id == "empty";
		}
		return true;
	}

	public bool IsNotEmpty()
	{
		return !IsEmpty();
	}

	public bool ParsIsEmpty()
	{
		return _params.IsEmpty();
	}

	public string GetParamAsString(string param_name)
	{
		return ((int)_params.Get(param_name)).ToString();
	}

	public float GetParam(string param_name, float default_value = 0f)
	{
		return _params.Get(param_name, default_value);
	}

	public int GetParamInt(string param_name)
	{
		return _params.GetInt(param_name);
	}

	public bool HasParam(string param_name)
	{
		return _params.Has(param_name);
	}

	public float GetCalculatedParam(string param_name)
	{
		float num = 0f;
		ItemDefinition itemDefinition = definition;
		if (itemDefinition != null)
		{
			num += itemDefinition.parameters.Get(param_name);
		}
		return num + _params.Get(param_name);
	}

	public void SetParam(string param_name, float value)
	{
		_params.Set(param_name, value);
	}

	public void SetParam(GameResAtom game_res)
	{
		_params.Set(game_res.type, game_res.value);
	}

	public void SetParam(GameRes game_res)
	{
		foreach (GameResAtom item in game_res.ToAtomList())
		{
			SetParam(item);
		}
	}

	public void AddToParams(string param_name, float value)
	{
		_params.Add(param_name, value);
	}

	public void AddToParams(GameResAtom game_res)
	{
		_params.Add(game_res.type, game_res.value);
	}

	public void SubFromParams(GameResAtom game_res)
	{
		_params.Sub(game_res.type, game_res.value);
	}

	public void AddToParams(GameRes game_res)
	{
		foreach (GameResAtom item in game_res.ToAtomList())
		{
			AddToParams(item);
		}
	}

	public void SubFromParams(GameRes game_res)
	{
		foreach (GameResAtom item in game_res.ToAtomList())
		{
			SubFromParams(item);
		}
	}

	public void SubFromParams(string param_name, float param_value)
	{
		_params.Sub(param_name, param_value);
	}

	public void RemoveZeroParams()
	{
		_params.RemoveZeroValues();
	}

	public bool SetInventorySize(int size)
	{
		_params.Set("inventory_size", size);
		return true;
	}

	public List<Item> GetInventoryCopy()
	{
		return inventory.Select((Item item) => new Item(item)).ToList();
	}

	public bool AddNotFoldedItem(Item item)
	{
		return AddItem(item.id, item.value);
	}

	public bool AddItem(string item_id, int item_value)
	{
		return AddItem(new Item(item_id, item_value));
	}

	public bool AddItemByIndex(Item item, int index)
	{
		if (index < 0 || index >= inventory_size)
		{
			Debug.LogError($"Can't add item [{item.id}:{item.value}], index [{index}] out of range");
			return false;
		}
		if (inventory.Count < index + 1)
		{
			InitializeInventoryTillIndex(index);
		}
		Item item2 = inventory[index];
		if (item2 != null && !item2.IsEmpty())
		{
			if (item2.id != item.id)
			{
				Debug.LogError($"Can't add item [{item.id}:{item.value}], at index [{index}], there's another item inserted [{item2.id}:{item2.value}]");
				return false;
			}
			if (item2.value + item.value <= item2.definition.stack_count)
			{
				item2.value += item.value;
				return false;
			}
			Debug.LogError($"Can't add item [{item.id}:{item.value}], at index [{index}], not enough space");
			return false;
		}
		if (item.value <= item.definition.stack_count)
		{
			inventory[index] = item;
			return true;
		}
		Debug.LogError($"Can't add item [{item.id}:{item.value}], at index [{index}], not enough space");
		return false;
	}

	public bool RemoveItemByIndex(int index)
	{
		if (index < 0 || index >= inventory_size)
		{
			Debug.LogError($"Can't remove item, index [{index}] out of range");
			return false;
		}
		if (inventory.Count < index + 1)
		{
			InitializeInventoryTillIndex(index);
		}
		inventory[index] = new Item();
		return true;
	}

	public Item GetItemByIndex(int index)
	{
		if (index < 0 || index >= inventory_size)
		{
			Debug.LogError($"Can't return item, index [{index}] out of range");
			return null;
		}
		if (inventory.Count < index + 1)
		{
			InitializeInventoryTillIndex(index);
		}
		return inventory[index];
	}

	public bool AddItems(List<Item> items, bool return_false_if_cannot_add_all)
	{
		if (return_false_if_cannot_add_all && !CanAddItems(items))
		{
			return false;
		}
		foreach (Item item in items)
		{
			AddItem(item);
		}
		return true;
	}

	public void AddNotFoldedItemsWithoutCheck(List<Item> items)
	{
		foreach (Item item in items)
		{
			AddItem(item.id, item.value);
		}
	}

	public void RemoveNotFoldedItem(Item i)
	{
		RemoveItem(i.id, i.value);
	}

	public bool RemoveItem(string item_id, int item_value, Item try_from_bag = null)
	{
		return RemoveItem(new Item(item_id, item_value), 0, try_from_bag);
	}

	public bool RemoveItems(List<Item> items, int multiplier = 1)
	{
		bool flag = true;
		foreach (Item item in items)
		{
			flag &= RemoveItem(item, item.value * multiplier);
		}
		return flag;
	}

	public bool AddItem(Item item, bool bags_allowed = true)
	{
		if (!CanAddItem(item))
		{
			Debug.Log("Can not add item [" + item.id + ":" + item.value + "]");
			return false;
		}
		string text = item.id;
		int num = item.value;
		int stack_count = item.definition.stack_count;
		int num2 = inventory_size - inventory.Count;
		if (num2 < 0)
		{
			num2 = 0;
		}
		List<Item> list = new List<Item>();
		List<Item> list2 = new List<Item>();
		bool flag = false;
		if (bags_allowed)
		{
			for (int i = 0; i < inventory.Count; i++)
			{
				Item item2 = inventory[i];
				if (item2 == null || item2.IsEmpty())
				{
					inventory.RemoveAt(i);
					i--;
					continue;
				}
				if (item2.id == text)
				{
					flag = true;
				}
				if (!item2.is_bag || !item.definition.can_be_inserted_in_bag.Contains(item2.definition.bag_type))
				{
					continue;
				}
				bool flag2 = false;
				foreach (Item item12 in item2.inventory)
				{
					if (item12.id == text)
					{
						flag2 = true;
						break;
					}
				}
				if (flag2)
				{
					list.Add(item2);
				}
				else
				{
					list2.Add(item2);
				}
			}
		}
		else
		{
			for (int j = 0; j < inventory.Count; j++)
			{
				Item item3 = inventory[j];
				if (item3 == null || item3.IsEmpty())
				{
					inventory.RemoveAt(j);
					j--;
				}
				else if (item3.id == text)
				{
					flag = true;
				}
			}
		}
		if (stack_count == 1)
		{
			bool flag3 = false;
			while (num > 0)
			{
				num--;
				if (flag && num2 > 0)
				{
					num2--;
					Item item4 = new Item(item)
					{
						value = 1
					};
					inventory.Add(item4);
					continue;
				}
				if (bags_allowed && list.Count > 0)
				{
					int num3;
					for (num3 = 0; num3 < list.Count; num3++)
					{
						Item item5 = list[num3];
						if (item5.inventory_size - item5.inventory.Count > 0)
						{
							Item item6 = new Item(item)
							{
								value = 1
							};
							item5.inventory.Add(item6);
							flag3 = true;
							break;
						}
						list.RemoveAt(num3);
						num3--;
						if (num3 >= list.Count)
						{
							break;
						}
					}
					if (flag3)
					{
						continue;
					}
				}
				if (num2 > 0)
				{
					num2--;
					Item item7 = new Item(item)
					{
						value = 1
					};
					inventory.Add(item7);
					continue;
				}
				if (bags_allowed && list2.Count > 0)
				{
					int num4;
					for (num4 = 0; num4 < list2.Count; num4++)
					{
						Item item8 = list2[num4];
						if (item8.inventory_size - item8.inventory.Count > 0)
						{
							Item item9 = new Item(item)
							{
								value = 1
							};
							item8.inventory.Add(item9);
							break;
						}
						list2.RemoveAt(num4);
						num4--;
						if (num4 >= list2.Count)
						{
							break;
						}
					}
					continue;
				}
				return false;
			}
			return true;
		}
		if (flag)
		{
			if (num > 0)
			{
				foreach (Item item13 in inventory)
				{
					if (!(item13.id != text) && item13.value != stack_count)
					{
						int num5 = stack_count - item13.value;
						if (num5 >= num)
						{
							item13.value += num;
							num = 0;
						}
						else
						{
							num -= num5;
							item13.value = stack_count;
						}
						if (num <= 0)
						{
							return true;
						}
					}
				}
			}
			while (num > 0 && inventory.Count < inventory_size)
			{
				if (num > stack_count)
				{
					inventory.Add(new Item(item)
					{
						value = stack_count
					});
					num -= stack_count;
				}
				else
				{
					inventory.Add(new Item(item)
					{
						value = num
					});
					num = 0;
				}
			}
		}
		if (bags_allowed && list.Count > 0)
		{
			for (int k = 0; k < list.Count; k++)
			{
				Item item10 = list[k];
				foreach (Item item14 in item10.inventory)
				{
					if (!(item14.id != text) && item14.value != stack_count)
					{
						int num6 = stack_count - item14.value;
						if (num6 >= num)
						{
							item14.value += num;
							num = 0;
						}
						else
						{
							num -= num6;
							item14.value = stack_count;
						}
						if (num <= 0)
						{
							return true;
						}
					}
				}
				while (num > 0 && item10.inventory.Count < item10.inventory_size)
				{
					if (num > stack_count)
					{
						item10.inventory.Add(new Item(item)
						{
							value = stack_count
						});
						num -= stack_count;
					}
					else
					{
						item10.inventory.Add(new Item(item)
						{
							value = num
						});
						num = 0;
					}
				}
			}
		}
		if (!flag)
		{
			while (num > 0 && (inventory.Count < inventory_size || list2.Count <= 0))
			{
				if (num > stack_count)
				{
					inventory.Add(new Item(item)
					{
						value = stack_count
					});
					num -= stack_count;
				}
				else
				{
					inventory.Add(new Item(item)
					{
						value = num
					});
					num = 0;
				}
			}
		}
		if (bags_allowed && list2.Count > 0)
		{
			for (int l = 0; l < list2.Count; l++)
			{
				Item item11 = list2[l];
				foreach (Item item15 in item11.inventory)
				{
					if (!(item15.id != text) && item15.value != stack_count)
					{
						int num7 = stack_count - item15.value;
						if (num7 >= num)
						{
							item15.value += num;
							num = 0;
						}
						else
						{
							num -= num7;
							item15.value = stack_count;
						}
						if (num <= 0)
						{
							return true;
						}
					}
				}
				while (num > 0 && item11.inventory.Count < item11.inventory_size)
				{
					if (num > stack_count)
					{
						item11.inventory.Add(new Item(item)
						{
							value = stack_count
						});
						num -= stack_count;
					}
					else
					{
						item11.inventory.Add(new Item(item)
						{
							value = num
						});
						num = 0;
					}
				}
			}
		}
		if (num <= 0)
		{
			return true;
		}
		return false;
	}

	public bool RemoveItem(Item item, int count = 0, Item try_from_bag = null)
	{
		if (item == null || item.IsEmpty())
		{
			return false;
		}
		if (count == 0)
		{
			count = item.value;
		}
		if (GetTotalCount(item.id) < count)
		{
			return false;
		}
		RemoveItemNoCheck(item, count, "", null, try_from_bag);
		return true;
	}

	public int RemoveItemNoCheck(Item item, int count = 0, string multiquality_id = "", List<Item> out_really_removed_items = null, Item try_from_bag = null)
	{
		if (count == 0)
		{
			count = item.value;
		}
		if (try_from_bag != null && (try_from_bag.IsEmpty() || !try_from_bag.is_bag))
		{
			try_from_bag = null;
		}
		if (string.IsNullOrEmpty(multiquality_id) && item.multiquality_items.Count > 0 && item.definition == null)
		{
			foreach (string multiquality_item in item.multiquality_items)
			{
				count -= RemoveItemNoCheck(new Item(multiquality_item, count), 0, "", out_really_removed_items, try_from_bag);
				if (count < 0)
				{
					Debug.LogError($"Strange happened, count = {count}, mq_item = {multiquality_item}, item = {item}");
					count = 0;
				}
				if (count == 0)
				{
					break;
				}
			}
			return count;
		}
		int num = 0;
		string item_id = (string.IsNullOrEmpty(multiquality_id) ? item.id : multiquality_id);
		if (GameBalance.me.GetData<ItemDefinition>(item_id).stack_count == 1)
		{
			while (count > 0)
			{
				if (try_from_bag != null)
				{
					int num2 = try_from_bag.inventory.LastIndexOf(item);
					if (num2 < 0)
					{
						num2 = try_from_bag.GetLastItemIndex(item_id);
					}
					if (num2 >= 0)
					{
						try_from_bag.inventory.RemoveAt(num2);
						count--;
						num++;
						out_really_removed_items?.Add(new Item(item_id, 1));
						continue;
					}
				}
				int num3 = inventory.LastIndexOf(item);
				if (num3 < 0)
				{
					num3 = GetLastItemIndex(item_id);
				}
				if (num3 >= 0)
				{
					inventory.RemoveAt(num3);
					count--;
					num++;
					out_really_removed_items?.Add(new Item(item_id, 1));
					continue;
				}
				bool flag = false;
				for (int i = 0; i < inventory.Count; i++)
				{
					if (inventory[i] != null && !inventory[i].IsEmpty() && inventory[i].is_bag)
					{
						Item item2 = inventory[i];
						int num4 = item2.inventory.LastIndexOf(item);
						if (num4 < 0)
						{
							num4 = item2.GetLastItemIndex(item_id);
						}
						if (num4 >= 0)
						{
							item2.inventory.RemoveAt(num4);
							count--;
							num++;
							out_really_removed_items?.Add(new Item(item_id, 1));
							flag = true;
							break;
						}
					}
				}
				if (flag)
				{
					continue;
				}
				return num;
			}
			return num;
		}
		while (count > 0)
		{
			if (try_from_bag != null)
			{
				Item lastItem = try_from_bag.GetLastItem(item_id);
				if (lastItem != null)
				{
					if (lastItem.value > count)
					{
						num += count;
						lastItem.value -= count;
						out_really_removed_items?.Add(new Item(lastItem.id, count));
						count = 0;
					}
					else
					{
						num += lastItem.value;
						count -= lastItem.value;
						out_really_removed_items?.Add(new Item(lastItem.id, value));
						try_from_bag.inventory.Remove(lastItem);
					}
					continue;
				}
			}
			Item lastItem2 = GetLastItem(item_id);
			if (lastItem2 != null)
			{
				if (lastItem2.value > count)
				{
					num += count;
					lastItem2.value -= count;
					out_really_removed_items?.Add(new Item(lastItem2.id, count));
					count = 0;
				}
				else
				{
					num += lastItem2.value;
					count -= lastItem2.value;
					out_really_removed_items?.Add(new Item(lastItem2.id, lastItem2.value));
					inventory.Remove(lastItem2);
				}
				continue;
			}
			bool flag2 = false;
			for (int j = 0; j < inventory.Count; j++)
			{
				if (inventory[j] == null || inventory[j].IsEmpty() || !inventory[j].is_bag)
				{
					continue;
				}
				Item item3 = inventory[j];
				Item lastItem3 = item3.GetLastItem(item_id);
				if (lastItem3 != null)
				{
					if (lastItem3.value > count)
					{
						num += count;
						lastItem3.value -= count;
						out_really_removed_items?.Add(new Item(lastItem3.id, count));
						count = 0;
					}
					else
					{
						num += lastItem3.value;
						count -= lastItem3.value;
						out_really_removed_items?.Add(new Item(lastItem3.id, value));
						item3.inventory.Remove(lastItem3);
					}
					flag2 = true;
					break;
				}
			}
			if (flag2)
			{
				continue;
			}
			return num;
		}
		return num;
	}

	public int RemoveItemOrReturnLeftCount(Item item, int count = 0, string multiquality_id = "", List<Item> out_really_removed_items = null)
	{
		if (item == null || item.IsEmpty())
		{
			return 0;
		}
		if (count == 0)
		{
			count = item.value;
		}
		int totalCount = GetTotalCount(string.IsNullOrEmpty(multiquality_id) ? item.id : multiquality_id);
		if (totalCount >= count)
		{
			RemoveItemNoCheck(item, count, multiquality_id, out_really_removed_items);
			return 0;
		}
		int result = count - totalCount;
		RemoveItemNoCheck(item, totalCount, multiquality_id, out_really_removed_items);
		return result;
	}

	private int GetLastItemIndex(string item_id)
	{
		for (int num = inventory.Count - 1; num >= 0; num--)
		{
			if (!(inventory[num].id != item_id))
			{
				return num;
			}
		}
		return -1;
	}

	public Item GetLastItem(string item_id)
	{
		int lastItemIndex = GetLastItemIndex(item_id);
		if (lastItemIndex >= 0)
		{
			return inventory[lastItemIndex];
		}
		return null;
	}

	public bool CanAddItem(Item item, bool count_empty = true, bool count_bags = true)
	{
		if (item == null || item.IsEmpty())
		{
			return false;
		}
		return CanAddCount(item, count_empty, count_bags) >= item.value;
	}

	public bool CanCollectItemAsDrop(Item item)
	{
		ItemDefinition itemDefinition = item.definition;
		if (itemDefinition == null || itemDefinition.item_size <= 1)
		{
			return CanAddItem(item);
		}
		return false;
	}

	public int CanAddCount(Item item, bool count_empty, bool count_bags = true)
	{
		if (item == null || item.IsEmpty())
		{
			return 0;
		}
		return CanAddCount(item.id, count_empty, count_bags);
	}

	public int CanAddCount(string item_id, bool count_empty, bool count_bags = true)
	{
		if (string.IsNullOrEmpty(item_id))
		{
			return 0;
		}
		ItemDefinition data = GameBalance.me.GetData<ItemDefinition>(item_id);
		if (data == null)
		{
			return 0;
		}
		int stack_count = data.stack_count;
		if (stack_count <= 1 && !count_empty)
		{
			return 0;
		}
		int num = 0;
		bool flag = false;
		if (stack_count > 1)
		{
			foreach (Item item in inventory)
			{
				if (item.id == item_id)
				{
					num += stack_count - item.value;
				}
				else
				{
					if (!count_bags || !item.is_bag || !data.can_be_inserted_in_bag.Contains(item.definition.bag_type))
					{
						continue;
					}
					foreach (Item item2 in item.inventory)
					{
						if (item2.id == item_id)
						{
							num += stack_count - item2.value;
						}
					}
					if (count_empty)
					{
						ItemDefinition itemDefinition = item.definition;
						int num2 = itemDefinition.bag_size_x * itemDefinition.bag_size_y - item.inventory.Count;
						if (num2 < 0)
						{
							num2 = 0;
						}
						num += stack_count * num2;
						flag = true;
					}
				}
			}
		}
		if (count_empty)
		{
			int num3 = inventory_size - inventory.Count;
			if (num3 < 0)
			{
				num3 = 0;
			}
			num += stack_count * num3;
			if (count_bags && !flag)
			{
				foreach (Item item3 in inventory)
				{
					if (item3.is_bag && data.can_be_inserted_in_bag.Contains(item3.definition.bag_type))
					{
						int num4 = item3.inventory_size - item3.inventory.Count;
						if (num4 < 0)
						{
							num4 = 0;
						}
						num += num4 * stack_count;
					}
				}
			}
		}
		return num;
	}

	public bool CanAddItems(List<Item> items_to_add, bool include_bags = false)
	{
		if (items_to_add.Count == 0)
		{
			return true;
		}
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		int num = 0;
		if (!include_bags)
		{
			foreach (Item item3 in items_to_add)
			{
				if (item3.definition.stack_count <= 1)
				{
					num++;
					continue;
				}
				string text = item3.id;
				if (!dictionary.ContainsKey(text))
				{
					dictionary.Add(text, CanAddCount(text, count_empty: false, include_bags));
				}
				if (dictionary[text] > 0)
				{
					dictionary[text] -= item3.value;
					if (dictionary[text] >= 0)
					{
						continue;
					}
				}
				num++;
			}
			return inventory_size - inventory.Count >= num;
		}
		Item item = MakeInventoryCopy();
		for (int i = 0; i < items_to_add.Count; i++)
		{
			if (!items_to_add[i].IsItemInsertableInBag())
			{
				Item item2 = items_to_add[i];
				items_to_add.RemoveAt(i);
				items_to_add.Insert(0, item2);
			}
		}
		foreach (Item item4 in items_to_add)
		{
			if (!item.AddItem(item4))
			{
				return false;
			}
		}
		return true;
	}

	public bool IsEnoughParams(GameRes pars)
	{
		if (pars == null || pars.IsEmpty())
		{
			return true;
		}
		return _params.IsEnough(pars);
	}

	public bool IsEnoughParam(GameResAtom par)
	{
		if (par == null || par.IsEmpty())
		{
			return true;
		}
		return _params.IsEnough(par);
	}

	public bool IsEnoughItems(Item item, string multiquality_id = "", int used_items = 0, int multiplier = 1)
	{
		if (item == null || item.IsEmpty())
		{
			return true;
		}
		bool num = string.IsNullOrEmpty(multiquality_id) && item.multiquality_items.Count > 1;
		int num2 = 0;
		foreach (string item2 in num ? item.multiquality_items : new List<string> { string.IsNullOrEmpty(multiquality_id) ? item.id : multiquality_id })
		{
			num2 += GetTotalCount(item2);
		}
		return num2 - used_items >= item.value * multiplier;
	}

	public bool IsEnoughItems(List<Item> items, int multiplier = 1)
	{
		foreach (Item item in items)
		{
			if (!IsEnoughItems(item, "", 0, multiplier))
			{
				return false;
			}
		}
		return true;
	}

	public int GetTotalCount(string item_id, bool count_in_bags = true)
	{
		if (string.IsNullOrEmpty(item_id))
		{
			return 0;
		}
		int num = 0;
		if (id == item_id)
		{
			num += value;
		}
		foreach (Item item in inventory)
		{
			if (item.id == item_id)
			{
				num += item.value;
			}
			if (!count_in_bags || !item.is_bag)
			{
				continue;
			}
			foreach (Item item2 in item.inventory)
			{
				if (item2.id == item_id)
				{
					num += item2.value;
				}
			}
		}
		return num;
	}

	public bool MoveItemTo(MultiInventory another_inventory, Item item, int count = 0)
	{
		if (count == 0 || count > item.value)
		{
			count = item.value;
		}
		item = new Item(item)
		{
			value = count
		};
		if (!IsEnoughItems(item) || !another_inventory.CanAddItem(item))
		{
			return false;
		}
		RemoveItemNoCheck(item);
		another_inventory.AddItemNoCheck(item);
		return true;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("Item(id = ");
		stringBuilder.Append(id);
		stringBuilder.Append(", value = ");
		stringBuilder.Append(value.ToString());
		stringBuilder.Append(")");
		return stringBuilder.ToString();
	}

	public void OnBeforeSerialize()
	{
		if (_serialize_depth > 3 || inventory == null || inventory.Count == 0)
		{
			_serialized_data = string.Empty;
			return;
		}
		JSONObject jSONObject = new JSONObject(JSONObject.Type.ARRAY);
		for (int i = 0; i < inventory.Count; i++)
		{
			jSONObject.Add(inventory[i].ToJSON(_serialize_depth + 1));
		}
		_serialized_data = jSONObject.Print();
	}

	public void OnAfterDeserialize()
	{
		inventory = new List<Item>();
		if (string.IsNullOrEmpty(_serialized_data))
		{
			return;
		}
		JSONObject jSONObject = new JSONObject(_serialized_data);
		if (jSONObject.list != null)
		{
			for (int i = 0; i < jSONObject.list.Count; i++)
			{
				inventory.Add(JsonUtility.FromJson<Item>(jSONObject.list[i].str));
			}
		}
	}

	public string ToJSON(int ser_depth = 0)
	{
		_serialize_depth = ser_depth;
		return JsonUtility.ToJson(this);
	}

	public float GetItemQuality()
	{
		if (IsEmpty())
		{
			return 0f;
		}
		if (definition == null)
		{
			return 0f;
		}
		if (id == "body" || definition.type == ItemDefinition.ItemType.Body)
		{
			float param = GetParam("min_quality");
			float param2 = GetParam("max_quality");
			if (param2 < param)
			{
				Debug.LogError("Something wrong with body qualities: [" + id + "] = {" + param + "; " + param2 + "}");
				return 0f;
			}
			return (param2 - param) * durability + param;
		}
		if (definition.type == ItemDefinition.ItemType.GraveCover || definition.type == ItemDefinition.ItemType.GraveFence || definition.type == ItemDefinition.ItemType.GraveStone)
		{
			return Mathf.Round(definition.quality * 10f * durability) / 10f;
		}
		return definition.quality * (float)value;
	}

	public string GetItemQualityString()
	{
		return "(skull)" + GetItemQuality().ToString("0.0");
	}

	public string GetItemConditionString()
	{
		return "(hp)" + FloatNumberToPercentString(durability);
	}

	public static string FloatNumberToPercentString(float v)
	{
		int num = Mathf.RoundToInt(v * 100f);
		if (num > 100)
		{
			num = 100;
		}
		if (num < 0)
		{
			num = 0;
		}
		return num + "%";
	}

	public float GetItemQualityMultiplyer()
	{
		if (IsEmpty())
		{
			return 1f;
		}
		if (definition == null)
		{
			return 1f;
		}
		if (id == "body" || definition.type == ItemDefinition.ItemType.Body)
		{
			float param = GetParam("min_quality");
			float param2 = GetParam("max_quality");
			if (param2 <= param)
			{
				Debug.LogError("Something wrong with body qualities: [" + id + "] = {" + param + "; " + param2 + "}");
				return 1f;
			}
			return (param2 - param) * durability + param;
		}
		return definition.quality_multiplyer;
	}

	public float GetInventoryQuality(string ignore_item_id = null)
	{
		if (inventory == null || inventory.Count == 0)
		{
			return 0f;
		}
		float num = 0f;
		foreach (Item item in inventory)
		{
			if (!(item.id == ignore_item_id) && item.definition != null)
			{
				num = ((item.definition.type != ItemDefinition.ItemType.Body) ? (num + item.GetItemQuality()) : (num + item.GetInventoryQuality(ignore_item_id)));
			}
		}
		return num;
	}

	public float GetInventoryQualityMultiplier()
	{
		if (inventory == null || inventory.Count == 0)
		{
			return 0f;
		}
		float num = 1f;
		foreach (Item item in inventory)
		{
			if (item != null && item.definition != null && !item.definition.quality_multiplyer.EqualsTo(0f, 0.001f))
			{
				num *= item.definition.quality_multiplyer;
			}
		}
		return num;
	}

	public void GetBodySkulls(out int negative, out int positive, out int positive_avaialble, bool dont_count_self = false)
	{
		if (dont_count_self)
		{
			negative = 0;
			positive = 0;
		}
		else
		{
			negative = GetRedSkullsValue();
			positive = GetWhiteSkullsValue();
		}
		foreach (Item item in inventory)
		{
			if (item != null && item.definition != null)
			{
				negative += item.GetRedSkullsValue();
				positive += item.GetWhiteSkullsValue();
			}
		}
		if (negative < 0)
		{
			negative = 0;
		}
		if (positive < 0)
		{
			positive = 0;
		}
		int num = Mathf.CeilToInt(durability * 100f);
		if (num > 90)
		{
			num = 100;
		}
		positive_avaialble = Mathf.FloorToInt((float)(positive * num) / 100f);
	}

	public void UpdateDurability(float delta_time, float parent_modificator = 1f)
	{
		if (definition == null)
		{
			return;
		}
		if (!definition.has_durability)
		{
			if (definition.type != ItemDefinition.ItemType.Rat)
			{
				return;
			}
			{
				foreach (Item item in inventory)
				{
					item.UpdateDurability(delta_time, parent_modificator);
				}
				return;
			}
		}
		if (definition.is_update_children_durability)
		{
			float durability_modificator_for_children = definition.durability_modificator_for_children;
			foreach (Item item2 in inventory)
			{
				item2.UpdateDurability(delta_time, durability_modificator_for_children);
			}
		}
		float childrenDurabilityModificator = GetChildrenDurabilityModificator();
		float num = 1f;
		if (!string.IsNullOrEmpty(drop_zone_id))
		{
			if (_cached_drop_zone_id != drop_zone_id)
			{
				_cached_drop_zone_id = drop_zone_id;
				_cached_world_zone = GameBalance.me.GetDataOrNull<WorldZoneDefinition>(drop_zone_id);
				_cached_zone_dur_k = _cached_world_zone.zone_params.Get("dur_k_" + id, 1f);
			}
			if (_cached_world_zone != null)
			{
				num = _cached_zone_dur_k;
			}
		}
		durability -= definition.durability_decrease * parent_modificator * childrenDurabilityModificator * delta_time * num;
		if (durability < 0.001f && !string.IsNullOrEmpty(definition.dur_0_change))
		{
			SetItemID(definition.dur_0_change);
			durability = 1f;
		}
	}

	private float GetChildrenDurabilityModificator()
	{
		float num = 1f;
		foreach (Item item in inventory)
		{
			if (item != null && item.definition != null)
			{
				num *= item.definition.durability_modificator;
			}
		}
		return num;
	}

	public bool HasItemInInventory(string item_id)
	{
		return GetItemsCount(item_id) > 0;
	}

	public int GetItemsCount(string item_id, bool count_secondary_inventory = false)
	{
		if (inventory == null || inventory.Count == 0)
		{
			return 0;
		}
		if (string.IsNullOrEmpty(item_id))
		{
			return 0;
		}
		int num = 0;
		foreach (Item item in inventory)
		{
			if (item.is_bag)
			{
				foreach (Item item2 in item.inventory)
				{
					if (item2.id == item_id)
					{
						num += item2.value;
					}
				}
			}
			else if (item.id == item_id)
			{
				num += item.value;
			}
		}
		if (count_secondary_inventory && secondary_inventory != null && secondary_inventory.Count > 0)
		{
			foreach (Item item3 in secondary_inventory)
			{
				if (item3.id == item_id)
				{
					num += item3.value;
				}
			}
		}
		return num;
	}

	public Item GetItemWithID(string id, ItemFindLogics item_find_logics = ItemFindLogics.FirstFound, bool allow_bags = true)
	{
		if (item_find_logics == ItemFindLogics.FirstFound || item_find_logics == ItemFindLogics.LastFound)
		{
			Item result = null;
			{
				foreach (Item item in inventory)
				{
					if (item.id == id)
					{
						result = item;
						if (item_find_logics == ItemFindLogics.FirstFound)
						{
							return result;
						}
					}
					if (!item.is_bag)
					{
						continue;
					}
					foreach (Item item2 in item.inventory)
					{
						if (item2.id == id)
						{
							result = item2;
							if (item_find_logics == ItemFindLogics.FirstFound)
							{
								return result;
							}
						}
					}
				}
				return result;
			}
		}
		List<Item> list = new List<Item>();
		foreach (Item item3 in inventory)
		{
			if (item3.id == id)
			{
				list.Add(item3);
			}
			else
			{
				if (!item3.is_bag)
				{
					continue;
				}
				foreach (Item item4 in item3.inventory)
				{
					if (item4.id == id)
					{
						list.Add(item4);
					}
				}
			}
		}
		if (list.Count != 0)
		{
			return SortItemsListByFindLogics(list, item_find_logics)[0];
		}
		return null;
	}

	public static List<Item> SortItemsListByFindLogics(List<Item> items, ItemFindLogics item_find_logics)
	{
		if (item_find_logics == ItemFindLogics.WithLowestDurability)
		{
			items.Sort((Item a, Item b) => a.durability.CompareTo(b.durability));
			return items;
		}
		throw new ArgumentOutOfRangeException("item_find_logics", item_find_logics, null);
	}

	public Item GetItemOfType(ItemDefinition.ItemType type, bool first = true)
	{
		Item result = null;
		foreach (Item item in inventory)
		{
			if (item.definition.type == type)
			{
				result = item;
				if (first)
				{
					return result;
				}
			}
		}
		return result;
	}

	public string GetBodyDescription()
	{
		int num = Mathf.RoundToInt(durability * 100f);
		if (num > 100)
		{
			num = 100;
		}
		else if (num < 0)
		{
			num = 0;
		}
		string v = "?";
		if (MainGame.me.player.CanSeeDarkness())
		{
			v = ((GetParam("dark") > 0f) ? "dark" : "not dark");
		}
		return GJL.L("body_descr", num.ToString(), GetItemQuality().ToString("0.0"), v);
	}

	public List<Item> GetInventoryAsCollapsedStacks()
	{
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		foreach (Item item in inventory)
		{
			if (!dictionary.ContainsKey(item.id))
			{
				dictionary.Add(item.id, 0);
			}
			dictionary[item.id] += item.value;
		}
		List<Item> list = new List<Item>();
		foreach (KeyValuePair<string, int> item2 in dictionary)
		{
			list.Add(new Item(item2.Key, item2.Value));
		}
		return list;
	}

	public GameRes GetParams()
	{
		return _params;
	}

	public string InitMultiqualityItems()
	{
		string text = string.Empty;
		if (id == "money")
		{
			return text;
		}
		if (id == "r" || id == "g" || id == "b")
		{
			return text;
		}
		ItemDefinition dataOrNull = GameBalance.me.GetDataOrNull<ItemDefinition>(id);
		if (dataOrNull != null)
		{
			dataOrNull.not_used = false;
			return text;
		}
		string text2 = id + ":";
		foreach (ItemDefinition items_datum in GameBalance.me.items_data)
		{
			if (items_datum.id.StartsWith(text2))
			{
				multiquality_items.Add(items_datum.id);
				items_datum.not_used = false;
			}
		}
		if (multiquality_items.Count == 0)
		{
			text = text + "Not found multiquality items for group \"" + id + "\"";
		}
		return text;
	}

	public static void RemoveItemWithIDFromTheList(List<Item> list, string item_id, bool dont_remove_but_set_zero = false)
	{
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].id == item_id)
			{
				if (dont_remove_but_set_zero)
				{
					list[i] = new Item(list[i].id, 0);
				}
				else
				{
					list.RemoveAt(i);
				}
				break;
			}
		}
	}

	private float RecalculateTotalCooldown()
	{
		if (!definition.cooldown.has_expression)
		{
			Debug.LogError("ERROR: Can't recalculate cooldown for item id = " + id);
			return 0f;
		}
		float result = definition.cooldown.EvaluateFloat(MainGame.me.player);
		SetParam("_cooldown", result);
		return result;
	}

	public int GetGrayedCooldownPercent()
	{
		if (!definition.cooldown.has_expression)
		{
			return 0;
		}
		float param = MainGame.me.player.GetParam("_cooldown_" + id);
		if (param <= 0f)
		{
			return 0;
		}
		float num = GetParam("_cooldown");
		if (num.EqualsTo(0f))
		{
			num = RecalculateTotalCooldown();
		}
		int num2 = Mathf.RoundToInt(param / num * 100f);
		if (num2 > 100)
		{
			num2 = 100;
		}
		if (num2 < 0)
		{
			num2 = 0;
		}
		return num2;
	}

	public GameRes UseItem(WorldGameObject wgo = null, Vector3? effect_bubble_pos = null)
	{
		if (definition.cooldown.has_expression)
		{
			if (GetGrayedCooldownPercent() != 0)
			{
				Debug.LogError("Can't use item '" + id + "' because of cooldown");
				return new GameRes();
			}
			MainGame.me.player.SetParam("_cooldown_" + id, RecalculateTotalCooldown());
		}
		MainGame.me.save.quests.CheckKeyQuests("use_item_" + id);
		Stats.DesignEvent("Item:Use:" + id);
		GUIElements.me.hud.toolbar.Redraw();
		if (!string.IsNullOrEmpty(definition.on_use_snd))
		{
			Sounds.PlaySound(definition.on_use_snd);
		}
		MainGame.me.player.AddToParams(definition.params_on_use);
		if (definition.drop_on_use.Count > 0)
		{
			((wgo == null) ? MainGame.me.player : wgo).DropItems(definition.drop_on_use, Direction.ToPlayer);
		}
		GameRes gameRes = new GameRes(definition.params_on_use);
		if (definition.on_use_expressions != null && definition.on_use_expressions.Count > 0)
		{
			float param = MainGame.me.player.GetParam("energy");
			foreach (SmartExpression on_use_expression in definition.on_use_expressions)
			{
				on_use_expression.Evaluate();
			}
			float num = MainGame.me.player.GetParam("energy") - param;
			gameRes.Add("energy", num);
		}
		string text = definition.on_use_script;
		if (wgo != null)
		{
			wgo.Redraw();
			EffectBubblesManager.ShowImmediately(effect_bubble_pos ?? wgo.bubble_pos_tf.position, gameRes);
			text = wgo.ReplaceStringParams(text);
		}
		if (!string.IsNullOrEmpty(text))
		{
			GS.RunFlowScript(text);
		}
		return gameRes;
	}

	public void OnTraded()
	{
		if (definition.on_trade_expressions == null || definition.on_trade_expressions.Count <= 0)
		{
			return;
		}
		foreach (SmartExpression on_trade_expression in definition.on_trade_expressions)
		{
			on_trade_expression.Evaluate();
		}
	}

	public CraftDefinition GetFixCraftAndPrice(out Item fix_price)
	{
		float num = durability;
		fix_price = new Item();
		fix_price.SetInventorySize(100);
		CraftDefinition fixCraftForItem = GameBalance.me.GetFixCraftForItem(id);
		if (fixCraftForItem == null)
		{
			Debug.LogError("Couldn't find a fix craft for item " + id);
			return null;
		}
		foreach (Item need in fixCraftForItem.needs)
		{
			int item_value = Mathf.CeilToInt((float)need.value * (1f - num));
			fix_price.AddItem(need.id, item_value);
		}
		return fixCraftForItem;
	}

	public string GetDurabilityHint()
	{
		return GJL.L("hint_item_condition", Mathf.RoundToInt(durability * 100f) + "%");
	}

	public static Item GetItemFromList(List<Item> list, string item_id)
	{
		foreach (Item item in list)
		{
			if (item.id == item_id)
			{
				return item;
			}
		}
		return null;
	}

	public string GetIcon()
	{
		if (id == "chapter")
		{
			Debug.Log("chapter");
		}
		if (_definition_set && definition != null)
		{
			return definition.GetIcon();
		}
		string text = id;
		while (text.Contains(":"))
		{
			text = text.Substring(0, text.LastIndexOf(':'));
			if (EasySpritesCollection.GetSprite("i_" + text, not_found_is_valid: true) != null)
			{
				return "i_" + text;
			}
		}
		if (GameBalance.me.GetDataOrNull<ItemDefinition>(id) == null)
		{
			List<string> itemsOfBaseName = GameBalance.me.GetItemsOfBaseName(id);
			if (itemsOfBaseName.Count > 0)
			{
				return GameBalance.me.GetData<ItemDefinition>(itemsOfBaseName[0]).GetIcon();
			}
		}
		if (definition != null)
		{
			return definition.GetIcon();
		}
		return "";
	}

	public string GetOverheadIcon()
	{
		if (definition != null)
		{
			return definition.GetOverheadIcon();
		}
		Debug.LogError("Can not find icon for " + id + "! Call Bulat.");
		return "";
	}

	public string GetItemName()
	{
		switch (id)
		{
		case "money":
			return Trading.FormatMoney((float)value / 100f);
		case "faith":
			return GJL.L("faith") + $" (x{value:0})";
		default:
		{
			string text = definition.GetItemName();
			if (value > 1)
			{
				text += $" (x{value:0})";
			}
			return text;
		}
		}
	}

	public void ReplaceItemIfNeeded()
	{
		if (definition != null && definition.item_replace != null && MainGame.me.player.GetParamInt(definition.item_replace.player_flag) > 0)
		{
			id = definition.item_replace.replace_id;
			_definition_set = false;
		}
	}

	public string GetMultiqualityItemDescription()
	{
		string text = string.Empty;
		if (!is_multiquality)
		{
			return text;
		}
		List<ItemDefinition> list = new List<ItemDefinition>();
		foreach (string multiquality_item in multiquality_items)
		{
			ItemDefinition dataOrNull = GameBalance.me.GetDataOrNull<ItemDefinition>(multiquality_item);
			if (dataOrNull != null)
			{
				list.Add(dataOrNull);
			}
		}
		string text2 = id + "d";
		string text3 = GJL.L(text2);
		if (text2 != text3 && !string.IsNullOrEmpty(text3))
		{
			text += LocalizedLabel.ColorizeTags(text3, LocalizedLabel.TextColor.SpeechBubble);
			text += "\n";
		}
		if (list.Count == 0)
		{
			return text;
		}
		List<GameRes> list2 = new List<GameRes>();
		foreach (ItemDefinition item in list)
		{
			GameRes gameRes = new GameRes();
			if (item.on_use_expressions != null && item.on_use_expressions.Count > 0)
			{
				foreach (SmartExpression on_use_expression in item.on_use_expressions)
				{
					if (on_use_expression == null || on_use_expression.HasNoExpresion())
					{
						continue;
					}
					foreach (GameResAtom item2 in GameRes.ParseSmartExpression(on_use_expression).ToAtomList())
					{
						gameRes.Add(item2);
					}
				}
			}
			foreach (GameResAtom item3 in item.params_on_use.ToAtomList())
			{
				gameRes.Add(item3);
			}
			list2.Add(gameRes);
		}
		string text4 = GameRes.ToFormattedString(list2);
		if (!list[0].can_be_used && list[0].is_tool && !string.IsNullOrEmpty(text4))
		{
			text = text.ConcatWithSeparator(GJL.L("tool_energy_spend", text4));
		}
		if (list[0].can_be_used)
		{
			string text5 = "";
			foreach (SmartExpression on_use_expression2 in list[0].on_use_expressions)
			{
				Regex regex = new Regex("AddBuff\\(\"(.+?)\"\\)");
				string rawExpressionString = on_use_expression2.GetRawExpressionString();
				if (string.IsNullOrEmpty(rawExpressionString) || !rawExpressionString.Contains("AddBuff("))
				{
					continue;
				}
				Match match = regex.Match(rawExpressionString);
				if (!match.Success)
				{
					continue;
				}
				string text6 = match.Groups[1].Captures[0].ToString();
				BuffDefinition data = GameBalance.me.GetData<BuffDefinition>(text6);
				if (data != null)
				{
					text5 = "[c][C16000]" + data.GetLocalizedName() + "[-][/c]";
					string descriptionIfExists = data.GetDescriptionIfExists();
					if (!string.IsNullOrEmpty(descriptionIfExists))
					{
						text5 = text5 + " (" + descriptionIfExists + ")";
					}
				}
			}
			if (!string.IsNullOrEmpty(text5) || !string.IsNullOrEmpty(text4))
			{
				string text7 = text4;
				if (string.IsNullOrEmpty(text4))
				{
					text7 = text5;
				}
				else if (!string.IsNullOrEmpty(text5))
				{
					text7 = text7 + ", " + text5;
				}
				text = text.ConcatWithSeparator(GJL.L("item_effect_on_use")) + " " + text7;
			}
		}
		return text;
	}

	public void CalcRatParams()
	{
		if (definition.type != ItemDefinition.ItemType.Rat)
		{
			Debug.LogWarning("Trying to calc Rat params on non-Rat item: \"" + id + "\"");
			return;
		}
		float num = definition.rat_speed;
		float num2 = 1f;
		float num3 = definition.rat_obedience;
		float num4 = 1f;
		foreach (Item item in inventory)
		{
			if (item?.definition != null && item.definition.type == ItemDefinition.ItemType.RatBuff)
			{
				num += item.definition.rat_speed_add;
				num3 += item.definition.rat_obedience_add;
				num2 *= item.definition.rat_speed_multiply;
				num4 *= item.definition.rat_obedience_multiply;
			}
		}
		num *= num2;
		num3 *= num4;
		_params.Set("rat_speed", num);
		_params.Set("rat_obedience", num3);
	}

	public List<Item> GetAllRatBuffs()
	{
		if (definition.type != ItemDefinition.ItemType.Rat)
		{
			Debug.LogError("FATAL ERROR: trying to get rat buffs from non-rat item! id = " + id);
			return null;
		}
		List<Item> list = new List<Item>();
		foreach (Item item in inventory)
		{
			if (item != null && item.definition?.type == ItemDefinition.ItemType.RatBuff)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public string GetRatDescription(bool include_base = true)
	{
		StringBuilder stringBuilder = new StringBuilder();
		CalcRatParams();
		stringBuilder.Append(GJL.L("Speed"));
		stringBuilder.Append(": ");
		stringBuilder.Append(Mathf.Round(_params.Get("rat_speed") * 10f) / 10f);
		if (include_base)
		{
			stringBuilder.Append("(");
			stringBuilder.Append(definition.rat_speed);
			stringBuilder.Append(")");
		}
		stringBuilder.Append("(speed)");
		stringBuilder.Append("\n");
		stringBuilder.Append(GJL.L("Obedience"));
		stringBuilder.Append(": ");
		stringBuilder.Append(Mathf.Round(_params.Get("rat_obedience") * 10f) / 10f);
		if (include_base)
		{
			stringBuilder.Append("(");
			stringBuilder.Append(definition.rat_obedience);
			stringBuilder.Append(")");
		}
		stringBuilder.Append("(obedience)");
		return stringBuilder.ToString();
	}

	public List<Item> GetCustomInsertionResult(CustomItemInsertion.InsertionType insertion_type)
	{
		List<Item> result = new List<Item>();
		if (insertion_type != CustomItemInsertion.InsertionType.OnUse)
		{
			throw new ArgumentOutOfRangeException("insertion_type", insertion_type, null);
		}
		return result;
	}

	public bool CanBeInsertedInBag(Item bag_item)
	{
		if (IsEmpty())
		{
			return false;
		}
		if (bag_item == null || bag_item.IsEmpty() || !bag_item.is_bag)
		{
			return false;
		}
		if (definition?.can_be_inserted_in_bag == null)
		{
			return false;
		}
		if (!definition.can_be_inserted_in_bag.Contains(bag_item.definition.bag_type))
		{
			return false;
		}
		return true;
	}

	public bool IsItemInsertableInBag()
	{
		if (definition == null)
		{
			return false;
		}
		if (IsEmpty())
		{
			return false;
		}
		if (definition.can_be_inserted_in_bag == null)
		{
			return false;
		}
		if (definition.can_be_inserted_in_bag.Count == 0)
		{
			return false;
		}
		if (definition.can_be_inserted_in_bag.Contains(ItemDefinition.BagType.None))
		{
			return false;
		}
		return true;
	}

	public Item MakeInventoryCopy()
	{
		Item item = new Item("inventory_copy");
		item.inventory = new List<Item>();
		item.inventory_size = inventory_size;
		foreach (Item item2 in inventory)
		{
			item.inventory.Add(new Item(item2));
		}
		return item;
	}

	public int GetWhiteSkullsValue()
	{
		if (definition == null)
		{
			return 0;
		}
		if (IsEmpty())
		{
			return 0;
		}
		return definition.q_plus + GetParamInt("bp_plus_mod");
	}

	public int GetRedSkullsValue()
	{
		if (definition == null)
		{
			return 0;
		}
		if (IsEmpty())
		{
			return 0;
		}
		return definition.q_minus + GetParamInt("bp_minus_mod");
	}

	public string GetItemBodyModificators()
	{
		if (definition == null)
		{
			return string.Empty;
		}
		string text = "";
		if (definition.show_q_hint.has_expression && !definition.show_q_hint.EvaluateBoolean())
		{
			return text;
		}
		int redSkullsValue = GetRedSkullsValue();
		int whiteSkullsValue = GetWhiteSkullsValue();
		if (redSkullsValue != 0)
		{
			text = text.ConcatWithSeparator((redSkullsValue < 0) ? "-" : "+", ", ");
			for (int i = 0; i < Mathf.Abs(redSkullsValue); i++)
			{
				text += "(rskull)";
			}
		}
		if (whiteSkullsValue != 0)
		{
			text = text.ConcatWithSeparator((whiteSkullsValue < 0) ? "-" : "+", ", ");
			for (int j = 0; j < Mathf.Abs(whiteSkullsValue); j++)
			{
				text += "(skull)";
			}
		}
		return text;
	}

	private bool IsIDNeedToBeReplaced(string id_to_check, out string id_replaced)
	{
		id_replaced = string.Empty;
		switch (id_to_check)
		{
		case "flesh":
			id_replaced = "flesh:flesh";
			return true;
		case "fat":
			id_replaced = "fat:fat";
			return true;
		case "blood":
			id_replaced = "blood:blood";
			return true;
		case "skin":
			id_replaced = "skin:skin";
			return true;
		default:
			return false;
		}
	}

	private void InitializeInventoryTillIndex(int index)
	{
		inventory.AddRange(Enumerable.Repeat(new Item(), index - inventory.Count + 1));
	}
}
