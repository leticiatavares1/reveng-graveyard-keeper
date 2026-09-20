using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CraftsInventory
{
	[SerializeField]
	private List<string> _craft_ids = new List<string>();

	public List<ObjectCraftDefinition> additional_crafts;

	public bool is_building = true;

	public CraftDefinition.CraftType craft_type;

	public void AddCraft(string id)
	{
		_craft_ids.Add(id);
	}

	public void RemoveCraft(string id)
	{
		_craft_ids.Remove(id);
	}

	public List<CraftDefinition> GetCraftsList()
	{
		List<CraftDefinition> list = new List<CraftDefinition>();
		foreach (string craft_id in _craft_ids)
		{
			list.Add(GameBalance.me.GetData<CraftDefinition>(craft_id));
		}
		return list;
	}

	public List<ObjectCraftDefinition> GetObjectCraftsList()
	{
		List<ObjectCraftDefinition> list = new List<ObjectCraftDefinition>();
		foreach (string craft_id in _craft_ids)
		{
			list.Add(GameBalance.me.GetData<ObjectCraftDefinition>(craft_id));
		}
		if (additional_crafts != null)
		{
			list.AddRange(additional_crafts);
		}
		return list;
	}
}
