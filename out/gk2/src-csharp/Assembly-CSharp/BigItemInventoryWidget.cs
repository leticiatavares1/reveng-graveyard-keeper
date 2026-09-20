using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

public class BigItemInventoryWidget : InventoryWidget
{
	[SerializeField]
	private UIItemCell bigItemCell;

	public static Pool bigCellPool;

	public override void Init()
	{
		base.Init();
		if (bigCellPool == null)
		{
			bigCellPool = LazyPooler.CreatePoolById("big_item_cell_pool", bigItemCell, 1);
			bigItemCell.gameObject.SetActive(value: false);
		}
	}

	public override void Redraw()
	{
		base.Redraw();
		List<Item> list = new List<Item>();
		if (!data.Inventory.Data.TryGetProperty<WhiteListFilterSerializedItemProperty>(out var property))
		{
			Debug.LogError("Trying to draw big item inventory widget for inventory without storable items!!!");
		}
		for (int j = 0; j < property.WhiteList.itemsIds.Count; j++)
		{
			list.Add(new Item(property.WhiteList.itemsIds[j], 0));
		}
		for (int k = 0; k < data.Inventory.Data.Inventory.Count; k++)
		{
			Item item = data.Inventory.Data.Inventory[k];
			Item item2 = list.Find((Item i) => i.id == item.id);
			if (item2 == null)
			{
				list.Add(new Item(item.id, item.Count));
			}
			else
			{
				item2.Count += item.Count;
			}
		}
		int num = list.Count;
		int count = uiItemCells.Count;
		while (num - count > 0)
		{
			UIItemCell newCell = GetNewCell();
			uiItemCells.Add(newCell);
			newCell.OnItemCellOver = onItemCellOver;
			newCell.OnItemCellOut = onItemCellOut;
			newCell.OnItemCellPress = onItemCellPress;
			newCell.OnItemCellPress2 = onItemCellPress2;
			newCell.OnItemCellDown = onItemCellDown;
			newCell.gameObject.SetActive(value: false);
			num--;
		}
		for (int l = 0; l < uiItemCells.Count; l++)
		{
			uiItemCells[l].gameObject.SetActive(value: false);
		}
		for (int m = 0; m < num; m++)
		{
			Item item3 = list[m];
			bool flag = data.CustomItemsAvailableCondition == null || data.CustomItemsAvailableCondition(item3);
			if (item3.Count >= 1)
			{
				uiItemCells[m].Draw(item3, isNeedItem: false, -1, isCraftResult: false, 1, !flag, 0, drawCounter: true, forceNonEmpty: true, forceDrawCounter: false, data.ItemRelatedWidgetState);
			}
			else
			{
				uiItemCells[m].DrawEmptyWithState(data.ItemRelatedWidgetState, drawAsNonInteractable: true);
				uiItemCells[m].SetWidgetState(ItemRelatedWidgetState.Disabled);
			}
			uiItemCells[m].OnItemCellOver = onItemCellOver;
			uiItemCells[m].OnItemCellOut = onItemCellOut;
			uiItemCells[m].OnItemCellPress = onItemCellPress;
			uiItemCells[m].OnItemCellPress2 = onItemCellPress2;
			uiItemCells[m].OnItemCellDown = onItemCellDown;
			uiItemCells[m].name = "UIItemCell (" + item3.id + ")";
			uiItemCells[m].gameObject.SetActive(value: true);
		}
		for (int n = 0; n < uiItemCells.Count; n++)
		{
			if (uiItemCells[n].gameObject.activeSelf && uiItemCells[n].DisplayingItem != null && !uiItemCells[n].DisplayingItem.IsEmpty && data.CustomItemsNotShowCondition != null && data.CustomItemsNotShowCondition(uiItemCells[n].DisplayingItem))
			{
				uiItemCells[n].gameObject.SetActive(value: false);
			}
		}
		inventoryHeaderWidget.Draw(base.Data.InventoryHeaderWidgetData);
		OnRedraw();
	}

	protected override UIItemCell GetNewCell()
	{
		UIItemCell orCreateObject = bigCellPool.GetOrCreateObject<UIItemCell>();
		orCreateObject.transform.SetParent(grid.transform);
		return orCreateObject;
	}

	protected override void ReleaseCell(UIItemCell cell)
	{
		bigCellPool.ReleaseObject(cell);
	}
}
