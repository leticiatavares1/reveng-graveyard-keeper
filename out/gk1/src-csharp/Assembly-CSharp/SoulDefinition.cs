using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SoulDefinition : BalanceBaseObject
{
	public List<string> parts_ids = new List<string>();

	public int tier;

	public string linked_item_id;

	public Item GenerateSoulItem()
	{
		Item item = new Item(linked_item_id, 1)
		{
			inventory_size = 99
		};
		foreach (string parts_id in parts_ids)
		{
			item.inventory.Add(new Item(parts_id, 1));
		}
		item.AddToParams("sins_count", parts_ids.Count);
		Debug.Log("Created soul: " + id);
		return item;
	}
}
