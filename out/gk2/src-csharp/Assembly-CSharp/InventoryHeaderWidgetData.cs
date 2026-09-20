using System;
using LazyBearTechnology;

public class InventoryHeaderWidgetData : LazyWidgetDataBase
{
	public bool IsActiveViewState { get; set; }

	public bool DrawHeader { get; set; } = true;


	public Inventory Inventory { get; set; }

	public string HeaderIconId { get; set; }

	public string CustomHeaderId { get; set; }

	public Action OnMoveAllSimilarItemFromPlayerToChest { get; set; }

	public InventoryHeaderWidgetData()
	{
	}

	public InventoryHeaderWidgetData(Inventory inventory, string headerIconId = "comm-header_2-type_icon-main_inventory", string customHeaderId = "", bool isActiveViewState = true, Action onMoveAllSimilarItemFromPlayerToChest = null)
	{
		IsActiveViewState = isActiveViewState;
		Inventory = inventory;
		HeaderIconId = headerIconId;
		CustomHeaderId = customHeaderId;
		OnMoveAllSimilarItemFromPlayerToChest = onMoveAllSimilarItemFromPlayerToChest;
	}
}
