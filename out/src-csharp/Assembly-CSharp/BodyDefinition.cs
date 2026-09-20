using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BodyDefinition : BalanceBaseObject
{
	public List<string> parts_ids = new List<string>();

	public int tier;

	public string linked_item_id;

	public Item GenerateBodyItem()
	{
		Item item = new Item(linked_item_id, 1);
		item.inventory_size = 99;
		if (string.IsNullOrEmpty(linked_item_id))
		{
			Debug.LogError("linked_item_id is empty for body definition: " + id);
		}
		foreach (string parts_id in parts_ids)
		{
			item.inventory.Add(new Item(parts_id, 1));
		}
		Debug.Log("Created body " + id);
		return item;
	}
}
