using System;

[Serializable]
public class WhiteListItemFilter : ItemFilter
{
	public override bool Contains(ItemDef itemDef)
	{
		if (itemsIds.Count > 0 && !itemsIds.Contains(itemDef.id))
		{
			return false;
		}
		if (groupsIds.Count > 0)
		{
			bool result = false;
			foreach (string itemGroupId in itemDef.itemGroupIds)
			{
				if (groupsIds.Contains(itemGroupId))
				{
					result = true;
					break;
				}
			}
			return result;
		}
		return true;
	}
}
