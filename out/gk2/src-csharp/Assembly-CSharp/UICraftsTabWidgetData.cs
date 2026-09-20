using System;
using System.Collections.Generic;
using LazyBearTechnology;

public class UICraftsTabWidgetData : LazyWidgetDataBase
{
	public string TabId { get; private set; }

	public List<CraftDef> Crafts { get; private set; }

	public WgoData WgoData { get; private set; }

	public Action<CraftDef, List<NeedItemData>, CraftParamsData, int> OnCraftToQueueAdded { get; private set; }

	public Action<CraftDef, List<NeedItemData>, CraftParamsData, int> OnCraftStarted { get; private set; }

	public bool IsGravePartRemove { get; private set; }

	public UICraftsTabWidgetData(WgoData wgoData, List<CraftDef> crafts, string tabId, Action<CraftDef, List<NeedItemData>, CraftParamsData, int> onCraftToQueueAdded, Action<CraftDef, List<NeedItemData>, CraftParamsData, int> onCraftStarted, bool isGravePartRemove)
	{
		WgoData = wgoData;
		OnCraftToQueueAdded = onCraftToQueueAdded;
		OnCraftStarted = onCraftStarted;
		TabId = tabId;
		Crafts = crafts;
		IsGravePartRemove = isGravePartRemove;
	}
}
