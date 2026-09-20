using System.Collections.Generic;
using UnityEngine;

public class VendorDealInventoryWidget : InventoryWidget
{
	[SerializeField]
	private List<UIItemCell> cells;

	public override List<UIItemCell> Cells => cells;

	public override void Redraw()
	{
		int count = data.Inventory.Data.Inventory.Count;
		for (int i = 0; i < data.Inventory.Data.Inventory.Count; i++)
		{
			Item item = data.Inventory.Data.Inventory[i];
			bool flag = data.CustomItemsAvailableCondition == null || data.CustomItemsAvailableCondition(item);
			if (item.Count >= 1)
			{
				cells[i].Draw(item, isNeedItem: false, -1, isCraftResult: false, 1, !flag, 0, drawCounter: true, forceNonEmpty: true, forceDrawCounter: false, data.ItemRelatedWidgetState);
			}
			else
			{
				cells[i].DrawEmpty(!flag);
			}
			cells[i].OnItemCellOver = data.OnItemCellOver;
			cells[i].OnItemCellOut = data.OnItemCellOut;
			cells[i].OnItemCellPress = data.OnItemCellPress;
			cells[i].OnItemCellPress2 = data.OnItemCellPress2;
			cells[i].OnItemCellDown = data.OnItemCellDown;
			cells[i].name = "UIItemCell (" + item.id + ")";
			cells[i].gameObject.SetActive(value: true);
		}
		for (int j = count; j < cells.Count; j++)
		{
			cells[j].DrawEmpty(drawAsNonInteractable: true);
			cells[j].OnItemCellOver = null;
			cells[j].OnItemCellOut = null;
			cells[j].OnItemCellPress = null;
			cells[j].OnItemCellPress2 = null;
			cells[j].OnItemCellDown = null;
			cells[j].name = "UIItemCell (empty)";
			cells[j].gameObject.SetActive(value: true);
		}
		OnRedraw();
	}

	public override void UpdatePrices(ItemPriceDelegate priceDelegate, int countModificator)
	{
		if (priceDelegate == null)
		{
			return;
		}
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		foreach (UIItemCell cell in cells)
		{
			Item displayingItem = cell.DisplayingItem;
			if (displayingItem != null && !displayingItem.IsEmpty)
			{
				if (!dictionary.ContainsKey(displayingItem.id))
				{
					dictionary[displayingItem.id] = 0;
				}
				dictionary[displayingItem.id]++;
			}
		}
		Dictionary<string, int> dictionary2 = new Dictionary<string, int>();
		foreach (UIItemCell cell2 in cells)
		{
			Item displayingItem2 = cell2.DisplayingItem;
			if (displayingItem2 == null || displayingItem2.IsEmpty)
			{
				cell2.ClearPriceLabel();
				continue;
			}
			if (!dictionary2.ContainsKey(displayingItem2.id))
			{
				dictionary2[displayingItem2.id] = 0;
			}
			int indexAmongSameId = dictionary2[displayingItem2.id];
			dictionary2[displayingItem2.id]++;
			int dealCellPriceCountModificator = GetDealCellPriceCountModificator(countModificator, indexAmongSameId, dictionary[displayingItem2.id]);
			cell2.UpdatePriceLabel(priceDelegate(displayingItem2, dealCellPriceCountModificator));
		}
	}

	public static int GetDealCellPriceCountModificator(int baseCountModificator, int indexAmongSameId, int sameIdCount)
	{
		if (sameIdCount < 1)
		{
			sameIdCount = 1;
		}
		if (baseCountModificator > 0)
		{
			return baseCountModificator + sameIdCount - 1 - indexAmongSameId;
		}
		return baseCountModificator - indexAmongSameId;
	}
}
