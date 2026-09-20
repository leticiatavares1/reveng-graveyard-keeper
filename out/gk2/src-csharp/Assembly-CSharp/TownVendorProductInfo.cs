using System;

[Serializable]
public class TownVendorProductInfo : IAutoParsable
{
	public string itemId;

	public int itemCount;

	public int happinessCount;

	public float perOne;

	public ItemDef Definition => GameBalance.Me.GetData<ItemDef>(itemId);

	public override string ToString()
	{
		if (string.IsNullOrEmpty(itemId))
		{
			return string.Empty;
		}
		string text = itemId;
		if (happinessCount != 0)
		{
			text = $"[{happinessCount}]" + text;
		}
		if (itemCount != 0)
		{
			text += $"={itemCount}";
		}
		return text;
	}
}
