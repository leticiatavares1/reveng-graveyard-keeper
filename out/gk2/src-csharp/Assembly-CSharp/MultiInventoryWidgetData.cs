using System;
using System.Collections.Generic;
using LazyBearTechnology;

public class MultiInventoryWidgetData : LazyWidgetDataBase
{
	public List<InventoryWidgetDataBase> inventoriesData = new List<InventoryWidgetDataBase>();

	public Action onWidgetHide;

	private InventoryWidgetDataBase selectedWidgetData;

	public Action<int> OnInventoryRemoved { get; set; }

	public Action<InventoryWidgetDataBase> OnInventoryAdded { get; set; }

	public Action OnMoveAllSimilarPressed { get; set; }

	public Func<Inventory> GetMoveAllSimilarTargetInventory { get; set; }

	public Action OnMoveAllSimilarTargetChanged { get; set; }

	public Action OnSelectedWidgetChanged { get; set; }

	public string HeaderLocaleId { get; set; } = "ui_multiinventory";


	public MultiInventoryWidgetMode WidgetSelectionMode { get; set; }

	public InventoryWidgetDataBase SelectedWidgetData
	{
		get
		{
			if (selectedWidgetData != null)
			{
				return selectedWidgetData;
			}
			if (inventoriesData.Count <= 0)
			{
				return null;
			}
			return inventoriesData[0];
		}
		set
		{
			if (selectedWidgetData != value)
			{
				selectedWidgetData = value;
				OnSelectedWidgetChanged?.Invoke();
			}
		}
	}

	public MultiInventoryWidgetData(MultiInventoryWidgetMode widgetSelectionMode = MultiInventoryWidgetMode.Default)
	{
		WidgetSelectionMode = widgetSelectionMode;
	}

	public void Add(InventoryWidgetDataBase widgetData)
	{
		inventoriesData.Add(widgetData);
	}

	public void AddRange(List<InventoryWidgetDataBase> widgetsData)
	{
		inventoriesData.AddRange(widgetsData);
	}

	public Inventory FindBagInventory(Item item)
	{
		for (int i = 0; i < inventoriesData.Count; i++)
		{
			if (inventoriesData[i] is BagInventoryWidgetData bagInventoryWidgetData && bagInventoryWidgetData.Inventory.Data == item)
			{
				return bagInventoryWidgetData.Inventory;
			}
		}
		return null;
	}

	public void OnBagRemoved(Item bag)
	{
		int num = inventoriesData.FindIndex((InventoryWidgetDataBase b) => b.Inventory.Data.UniqueId == bag.UniqueId);
		if (num != -1)
		{
			OnInventoryRemoved?.Invoke(num);
			inventoriesData.RemoveAt(num);
		}
	}

	public void OnBagAdded(BagInventoryWidgetData bagInventoryWidgetData)
	{
		inventoriesData.Add(bagInventoryWidgetData);
		OnInventoryAdded?.Invoke(bagInventoryWidgetData);
	}
}
