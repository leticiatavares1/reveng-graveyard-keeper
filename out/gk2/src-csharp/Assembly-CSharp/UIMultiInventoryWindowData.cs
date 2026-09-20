using System;
using System.Collections.Generic;
using LazyBearTechnology;

public class UIMultiInventoryWindowData : LazyWidgetDataBase
{
	public MultiInventoryWidgetData MultiInventoryWidgetData { get; private set; }

	public MultiInventory MultiInventory { get; private set; }

	public UIMultiInventoryWindowData(PlayerData playerData, Action<UIItemCell> onCellClicked, Func<Item, bool> itemsAvailableCondition = null, bool addCurrentPlayerWorldZone = true, string customHeaderId = null, Func<Item, string> extraRedTooltipLocIdProvider = null)
	{
		MultiInventoryWidgetData = new MultiInventoryWidgetData();
		MultiInventory multiInventory = new MultiInventory(playerData, addCurrentPlayerWorldZone);
		List<InventoryWidgetDataBase> widgetsDataForMultiInventory = InventoryWidgetDataHelper.GetWidgetsDataForMultiInventory(multiInventory, delegate
		{
			LazyAudio.PlayAndForget("gui_hover_light");
		}, null, delegate(UIItemCell item)
		{
			onCellClicked?.Invoke(item);
			LazyAudio.PlayAndForget("gui_click");
		}, null, null, itemsAvailableCondition);
		if (!string.IsNullOrEmpty(customHeaderId))
		{
			MultiInventoryWidgetData.HeaderLocaleId = customHeaderId;
		}
		if (extraRedTooltipLocIdProvider != null)
		{
			for (int i = 0; i < widgetsDataForMultiInventory.Count; i++)
			{
				widgetsDataForMultiInventory[i].ExtraRedTooltipLocIdProvider = extraRedTooltipLocIdProvider;
			}
		}
		MultiInventoryWidgetData.AddRange(widgetsDataForMultiInventory);
		MultiInventory = multiInventory;
	}
}
