using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BodyData
{
	public string name;

	[SerializeField]
	private Item _data = new Item();

	public long linked_obj_id;

	public string grave_req_id;

	[SerializeField]
	private int _id;

	[SerializeField]
	private string _definition_id = "";

	public BodyDefinition definition => GameBalance.me.GetData<BodyDefinition>(_definition_id);

	public int id => _id;

	public Item data => _data;

	public BodyData()
	{
	}

	public BodyData(string definition_id)
	{
		_id = ProjectTools.GenerateIntId();
		SetItemByDefinition(definition_id);
	}

	public void SetItemByDefinition(string definition_id)
	{
		_definition_id = definition_id;
		SetItem(definition.GenerateBodyItem());
	}

	private void SetItem(Item _item)
	{
		_data = _item;
	}

	public List<ItemDefinition.ItemType> GetExistingBodyParts()
	{
		List<ItemDefinition.ItemType> list = new List<ItemDefinition.ItemType>();
		foreach (Item item in _data.inventory)
		{
			list.Add(item.definition.type);
		}
		return list;
	}

	public bool HasBodyPart(ItemDefinition.ItemType type)
	{
		foreach (Item item in _data.inventory)
		{
			if (item.definition.type == type)
			{
				return true;
			}
		}
		return false;
	}

	public Item GetPartItem(ItemDefinition.ItemType type)
	{
		foreach (Item item in _data.inventory)
		{
			if (item.definition.type == type)
			{
				return item;
			}
		}
		return null;
	}
}
