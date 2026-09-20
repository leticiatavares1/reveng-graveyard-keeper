using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GraveRequirement : BalanceBaseObject
{
	[SerializeField]
	private List<Item> _grave_req = new List<Item>();

	public List<Item> GetRequiredItems()
	{
		List<Item> list = new List<Item>();
		foreach (Item item in _grave_req)
		{
			list.Add(new Item(item));
		}
		return list;
	}
}
