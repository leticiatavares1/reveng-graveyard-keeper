using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemCount
{
	public string itemId;

	public int count;

	[NonSerialized]
	private ItemDef itemDef;

	public bool IsEmpty
	{
		get
		{
			if (!string.IsNullOrEmpty(itemId) && count != 0)
			{
				return itemDef == null;
			}
			return true;
		}
	}

	public ItemDef Def => itemDef ?? (itemDef = GameBalance.Me.GetData<ItemDef>(itemId));

	public ItemCount()
	{
	}

	public ItemCount(string itemId, int count)
	{
		this.itemId = itemId;
		this.count = count;
		itemDef = GameBalance.Me.GetData<ItemDef>(itemId);
	}

	public ItemCount(ItemDef itemDef, int count)
	{
		itemId = itemDef.id;
		this.count = count;
		this.itemDef = itemDef;
	}

	public ItemCount(Item item)
		: this(item.Definition, item.Count)
	{
	}

	public List<Item> CreateItems()
	{
		List<Item> list = new List<Item>();
		int num = count;
		while (num > 0)
		{
			int num2 = Mathf.Min(num, itemDef.stackCount);
			list.Add(new Item(itemId, num2));
			num -= num2;
		}
		return list;
	}

	public static List<Item> CreateItems(List<ItemCount> items)
	{
		List<Item> list = new List<Item>();
		foreach (ItemCount item in items)
		{
			list.AddRange(item.CreateItems());
		}
		return list;
	}
}
