using LazyBearTechnology;
using UnityEngine;

public class OutputPreview
{
	public string craftId;

	public string itemId;

	public bool isStarOutput;

	public int count;

	public int quality;

	public string customIconId;

	private ItemDef itemDef;

	public string IconId
	{
		get
		{
			if (!string.IsNullOrEmpty(customIconId))
			{
				return customIconId;
			}
			ItemDef itemDef = ResolveItemDef();
			if (itemDef != null && !string.IsNullOrEmpty(itemDef.iconId))
			{
				return itemDef.iconId;
			}
			return craftId;
		}
	}

	public bool IsBigItemOutput
	{
		get
		{
			if (!string.IsNullOrEmpty(customIconId))
			{
				Sprite sprite = LazySingletonSO<EasySpritesCollection>.Instance.GetSprite(customIconId);
				if (sprite != null)
				{
					return sprite.rect.width / sprite.rect.height > 1.5f;
				}
			}
			ItemDef itemDef = ResolveItemDef();
			if (itemDef != null)
			{
				return itemDef.itemSize == ItemSize.Big;
			}
			return false;
		}
	}

	private ItemDef ResolveItemDef()
	{
		if (itemDef != null)
		{
			return itemDef;
		}
		if (string.IsNullOrEmpty(itemId) || GameBalance.Me == null)
		{
			return null;
		}
		if (isStarOutput && GameBalance.Me.starGroupItemsCache.TryGetValue(itemId, out var value) && value != null && value.Count > 0)
		{
			itemDef = value[0];
			return itemDef;
		}
		itemDef = GameBalance.Me.GetDataOrNull<ItemDef>(itemId);
		return itemDef;
	}

	public OutputPreview(string craftId, string itemId, bool isStarOutput, int count, int quality = -1, string customIconId = "")
	{
		this.craftId = craftId;
		this.itemId = itemId;
		this.isStarOutput = isStarOutput;
		this.count = count;
		this.quality = quality;
		this.customIconId = customIconId;
	}
}
