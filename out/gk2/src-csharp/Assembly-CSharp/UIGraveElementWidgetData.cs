using System;
using System.Collections.Generic;
using LazyBearTechnology;

public class UIGraveElementWidgetData : LazyWidgetDataBase
{
	public Action onElementClicked;

	private Wgo grave;

	private ItemType requiredTool;

	public Item GraveElementItem { get; private set; }

	public bool IsEmpty { get; private set; }

	public GraveElementType GraveElementType { get; private set; }

	public int Quality { get; private set; }

	public string EmptyDescriptionLocale { get; private set; }

	public bool HasRequiredTool
	{
		get
		{
			requiredTool = ItemType.None;
			if (grave == null)
			{
				return false;
			}
			return MainGame.PlayerController.HasToolForWork(grave.Data, out requiredTool);
		}
	}

	public ItemType RequiredTool => requiredTool;

	public UIGraveElementWidgetData()
	{
		IsEmpty = false;
	}

	public UIGraveElementWidgetData(Wgo grave, GraveElementType graveElementType)
	{
		IsEmpty = true;
		this.grave = grave;
		GraveElementType = graveElementType;
		TryGetGravePartItem();
		switch (GraveElementType)
		{
		case GraveElementType.Top:
			EmptyDescriptionLocale = "ui_tombstone_empty";
			break;
		case GraveElementType.Bot:
			EmptyDescriptionLocale = "ui_fence_empty";
			break;
		}
		Quality = ((!IsEmpty) ? GraveElementItem.Definition.quality : 0);
		onElementClicked = OnGraveElementClicked;
	}

	private void TryGetGravePartItem()
	{
		foreach (Item item in grave.Data.Inventory.Data.Inventory)
		{
			switch (GraveElementType)
			{
			case GraveElementType.Top:
				if (item.Definition.itemGroupIds.Contains("gravetop"))
				{
					GraveElementItem = item;
					IsEmpty = false;
					return;
				}
				break;
			case GraveElementType.Bot:
				if (item.Definition.itemGroupIds.Contains("gravebot"))
				{
					GraveElementItem = item;
					IsEmpty = false;
					return;
				}
				break;
			}
		}
	}

	private void OnGraveElementClicked()
	{
		if (IsEmpty)
		{
			UIMultiInventoryWindow window = LazyUI.GetWindow<UIMultiInventoryWindow>();
			UIMultiInventoryWindowData data = new UIMultiInventoryWindowData(MainGame.PlayerData, OnGraveItemSelected, IsGraveItem);
			window.Open(data);
		}
		else
		{
			OpenGravePartRemoveWindow();
		}
	}

	private void OnGraveItemSelected(UIItemCell itemCell)
	{
		UIMultiInventoryWindow window = LazyUI.GetWindow<UIMultiInventoryWindow>();
		UIGraveWindow window2 = LazyUI.GetWindow<UIGraveWindow>();
		bool flag = false;
		if (grave.Data.CraftComponent.HasCraftsByBalance)
		{
			foreach (CraftDefBase availableCraft in grave.Data.CraftComponent.AvailableCrafts)
			{
				foreach (NeedItemData needItem in availableCraft.needItems)
				{
					if (needItem.id == itemCell.DisplayingItem.id)
					{
						grave.Data.CraftComponent.TryStartCraft(new CraftElement(availableCraft, new CraftParamsData(availableCraft.id, grave.Data)));
						flag = true;
						break;
					}
				}
				if (flag)
				{
					break;
				}
			}
		}
		window2.Close();
		window.Close();
	}

	private bool IsGraveItem(Item item)
	{
		if (item == null)
		{
			return false;
		}
		return GraveElementType switch
		{
			GraveElementType.Top => item.Definition.itemGroupIds.Contains("gravetop"), 
			GraveElementType.Bot => item.Definition.itemGroupIds.Contains("gravebot"), 
			_ => false, 
		};
	}

	private void OpenGravePartRemoveWindow()
	{
		CraftDef craftDef = UIBaseCraftWindowData.FindGravePartRemoveCraft(grave.Data.CraftComponent, GraveElementItem);
		if (craftDef != null)
		{
			UICraftSelectionWindowData uICraftSelectionWindowData = new UICraftSelectionWindowData(grave.Data, craftDef, OnGravePartRemoveConfirmed, OnGravePartRemoveConfirmed);
			uICraftSelectionWindowData.IsGravePartRemove = true;
			LazyUI.GetWindow<UICraftSelectionWindow>().Open(uICraftSelectionWindowData);
		}
	}

	private void OnGravePartRemoveConfirmed(CraftDef craftDefinition, List<NeedItemData> selectedNeedItems, CraftParamsData craftParams, int craftsCount)
	{
		OnCraftPressed(new CraftElement(craftDefinition.id, craftsCount, selectedNeedItems, craftParams));
	}

	private void OnCraftPressed(CraftElement craftElement)
	{
		if (!grave.Data.CraftComponent.IsStarted && !grave.Data.CraftComponent.IsQueueDelayed && grave.Data.CraftComponent.TryStartCraft(craftElement))
		{
			LazyUI.GetWindow<UIGraveWindow>().Close();
		}
	}
}
