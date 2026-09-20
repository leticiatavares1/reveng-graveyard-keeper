using System;
using System.Collections.Generic;

[Serializable]
public class ToolActions
{
	public List<ItemDefinition.ItemType> action_tools = new List<ItemDefinition.ItemType>();

	public List<float> action_k = new List<float>();

	public bool no_actions => action_tools.Count == 0;

	public bool GetToolK(ItemDefinition.ItemType item_type, out float k)
	{
		k = 0f;
		int num = action_tools.IndexOf(item_type);
		if (num == -1)
		{
			return false;
		}
		k = action_k[num];
		return true;
	}

	public bool HasToolK(ItemDefinition.ItemType item_type)
	{
		return action_tools.IndexOf(item_type) != -1;
	}
}
