using System.Collections.Generic;
using LazyBearTechnology;

public class NeedItemsWidgetData : LazyWidgetDataBase
{
	public MultiInventory MultiInventory { get; private set; }

	public List<NeedItemData> NeedItems { get; private set; }

	public bool IsActive { get; private set; }

	public WgoData WgoData { get; private set; }

	public NeedItemsWidgetData(List<NeedItemData> needItems, MultiInventory multiInventory, bool isActive, WgoData wgoData = null)
	{
		NeedItems = needItems;
		MultiInventory = multiInventory;
		IsActive = isActive;
		WgoData = wgoData;
	}
}
