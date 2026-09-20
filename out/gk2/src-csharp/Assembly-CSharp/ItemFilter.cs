using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemFilter
{
	public enum ItemFilterElementType
	{
		Item,
		Group
	}

	[SerializeField]
	public List<string> itemsIds = new List<string>();

	[SerializeField]
	public List<string> groupsIds = new List<string>();

	public bool IsEmpty
	{
		get
		{
			if (itemsIds.Count == 0)
			{
				return groupsIds.Count == 0;
			}
			return false;
		}
	}

	public void AddElement(string id, ItemFilterElementType elementType)
	{
		switch (elementType)
		{
		case ItemFilterElementType.Item:
			if (!itemsIds.Contains(id))
			{
				itemsIds.Add(id);
			}
			break;
		case ItemFilterElementType.Group:
			if (!groupsIds.Contains(id))
			{
				groupsIds.Add(id);
			}
			break;
		}
	}

	public virtual bool Contains(ItemDef itemDef)
	{
		return false;
	}
}
