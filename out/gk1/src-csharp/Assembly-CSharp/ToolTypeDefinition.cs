using System;

[Serializable]
public class ToolTypeDefinition : BalanceBaseObject
{
	[Flags]
	public enum Flag
	{
		None = 0,
		Collider = 1,
		Work = 2
	}

	public Flag flag;

	public bool driven_by_anim_event => flag == Flag.None;

	public static ToolTypeDefinition Get(ItemDefinition.ItemType item_type)
	{
		int num = (int)item_type;
		string text = num.ToString();
		foreach (ToolTypeDefinition tools_datum in GameBalance.me.tools_data)
		{
			if (tools_datum.id == text)
			{
				return tools_datum;
			}
		}
		return null;
	}
}
