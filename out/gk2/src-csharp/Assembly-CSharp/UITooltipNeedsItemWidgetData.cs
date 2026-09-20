using System.Collections.Generic;
using LazyBearTechnology;

public class UITooltipNeedsItemWidgetData : LazyWidgetDataBase
{
	private List<NeedItemData> customItems;

	private WgoData wgoData;

	public List<UICraftItemCellData> CraftItemCellsData { get; private set; }

	public CraftDef CraftDefinition { get; private set; }

	public CraftComponent CraftComponent { get; private set; }

	public bool DrawAsNeedItem { get; private set; } = true;


	public UITooltipNeedsItemWidgetData(CraftDef craftDef, CraftComponent craftComponent, List<NeedItemData> customItems = null)
	{
		CraftDefinition = craftDef;
		CraftComponent = craftComponent;
		this.customItems = customItems;
		wgoData = craftComponent.CraftableObject as WgoData;
		FillCraftItemCellsData(craftComponent.CraftableObject.GetCraftableMultiInventory());
	}

	public UITooltipNeedsItemWidgetData(CraftDef craftDef, MultiInventory multiInventory, List<NeedItemData> customItems = null)
	{
		CraftDefinition = craftDef;
		this.customItems = customItems;
		FillCraftItemCellsData(multiInventory);
	}

	public UITooltipNeedsItemWidgetData(List<NeedItemData> needItems, MultiInventory multiInventory, WgoData wgoData = null)
	{
		DrawAsNeedItem = false;
		customItems = needItems;
		this.wgoData = wgoData;
		FillCraftItemCellsData(multiInventory);
	}

	private void FillCraftItemCellsData(MultiInventory multiInventory, bool drawRunes = false)
	{
		if (CraftItemCellsData != null)
		{
			CraftItemCellsData.Clear();
		}
		else
		{
			CraftItemCellsData = new List<UICraftItemCellData>();
		}
		if (customItems == null)
		{
			foreach (NeedItemData needItem in CraftDefinition.needItems)
			{
				CraftItemCellsData.Add(new UICraftItemCellData(needItem, multiInventory, null, 0f, drawRunes, wgoData));
			}
			CraftDef craftDefinition = CraftDefinition;
			if (craftDefinition != null && craftDefinition.hasDurabilityUseItem)
			{
				CraftItemCellsData.Add(new UICraftItemCellData(craftDefinition.durabilityUseItem, multiInventory, null, craftDefinition.needItemsDurabilityUse, drawRunes: false, wgoData));
			}
			return;
		}
		foreach (NeedItemData customItem in customItems)
		{
			CraftItemCellsData.Add(new UICraftItemCellData(customItem, multiInventory, null, 0f, drawRunes, wgoData));
		}
	}
}
