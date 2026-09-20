using System;
using System.Collections.Generic;

public class UICraftItemCellData
{
	public NeedItemData currentItem;

	public List<NeedItemData> itemVariants = new List<NeedItemData>();

	public float needDurability;

	public int currentItemIndex;

	public string runesStr;

	public NeedItemData initialItem;

	private Action OnItemChanged;

	public MultiInventory MultiInventory { get; private set; }

	public WgoData WgoData { get; private set; }

	public UICraftItemCellData(NeedItemData item, MultiInventory multiInventory, Action onItemChanged, float needDurability = 0f, string runesStr = "", WgoData wgoData = null)
	{
		this.runesStr = runesStr;
		this.needDurability = needDurability;
		MultiInventory = multiInventory;
		WgoData = wgoData;
		FillItemVariants(item);
		if (itemVariants.Count == 0)
		{
			itemVariants.Add(item);
		}
		currentItem = itemVariants[0];
		OnItemChanged = onItemChanged;
		initialItem = item;
	}

	public UICraftItemCellData(NeedItemData item, MultiInventory multiInventory, Action onItemChanged, float needDurability, bool drawRunes, WgoData wgoData = null)
		: this(item, multiInventory, onItemChanged, needDurability, "", wgoData)
	{
		if (drawRunes)
		{
			runesStr = GetRunesFromCurrentItem();
		}
	}

	public void NextItem()
	{
		if (currentItemIndex == itemVariants.Count - 1)
		{
			currentItemIndex = 0;
		}
		else
		{
			currentItemIndex++;
		}
		currentItem = itemVariants[currentItemIndex];
		OnItemChanged?.Invoke();
	}

	public void PrevItem()
	{
		if (currentItemIndex == 0)
		{
			currentItemIndex = itemVariants.Count - 1;
		}
		else
		{
			currentItemIndex--;
		}
		currentItem = itemVariants[currentItemIndex];
		OnItemChanged?.Invoke();
	}

	private string GetRunesFromCurrentItem()
	{
		if (currentItem == null || currentItem.IsGroup || currentItem.ItemDef == null)
		{
			return string.Empty;
		}
		return currentItem.ItemDef.GetRunesAsString();
	}

	private void FillItemVariants(NeedItemData item)
	{
		if (item.IsGroup)
		{
			if (item.TryGetGroupItemDefs(out var groupItemDefs) && groupItemDefs != null)
			{
				FormCraftItemCellDataForGroupItem(item, groupItemDefs);
			}
		}
		else
		{
			itemVariants.Add(item);
		}
	}

	private void FormCraftItemCellDataForGroupItem(NeedItemData groupItem, List<ItemDef> groupItemDefs)
	{
		List<NeedItemData> list = new List<NeedItemData>();
		List<NeedItemData> list2 = new List<NeedItemData>();
		foreach (ItemDef groupItemDef in groupItemDefs)
		{
			NeedItemData needItemData = new NeedItemData(groupItemDef.id, groupItem.count);
			if (HasVariantInInventory(needItemData))
			{
				list.Add(needItemData);
			}
			else
			{
				list2.Add(needItemData);
			}
		}
		itemVariants.AddRange(list);
		itemVariants.AddRange(list2);
	}

	private bool HasVariantInInventory(NeedItemData variant)
	{
		if (MultiInventory == null)
		{
			return false;
		}
		return MultiInventory.GetTotalCount(variant.Id) > 0;
	}
}
