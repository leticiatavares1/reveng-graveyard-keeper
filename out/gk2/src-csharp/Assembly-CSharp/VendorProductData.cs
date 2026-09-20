using System;

[Serializable]
public class VendorProductData : IAutoParsable
{
	public string itemId;

	public int priceMod;

	public int baseCount;

	public ItemDef Definition => GameBalance.Me.GetData<ItemDef>(itemId);

	public override string ToString()
	{
		if (string.IsNullOrEmpty(itemId))
		{
			return string.Empty;
		}
		string text = itemId;
		if (priceMod != 0)
		{
			text = $"[{priceMod}]" + text;
		}
		if (baseCount != 0)
		{
			text += $"={baseCount}";
		}
		return text;
	}
}
