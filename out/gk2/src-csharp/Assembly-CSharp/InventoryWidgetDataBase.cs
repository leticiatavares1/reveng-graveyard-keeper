using System;
using LazyBearTechnology;

public class InventoryWidgetDataBase : LazyWidgetDataBase
{
	private ItemRelatedWidgetState currentWidgetState;

	public Inventory Inventory { get; protected set; }

	public Action<UIItemCell> OnItemCellOver { get; protected set; }

	public Action<UIItemCell> OnItemCellOut { get; protected set; }

	public Action<UIItemCell> OnItemCellPress { get; protected set; }

	public Action<UIItemCell> OnItemCellPress2 { get; protected set; }

	public Action<UIItemCell> OnItemCellDown { get; protected set; }

	public Func<Item, bool> CustomItemsAvailableCondition { get; protected set; }

	public Func<Item, bool> CustomItemsNotShowCondition { get; protected set; }

	public Func<Item, string> ExtraRedTooltipLocIdProvider { get; set; }

	public Func<Item, bool> CustomItemSelectedCondition { get; set; }

	public bool DrawEmptyCellsAsDisabledWhenUnavailable { get; set; }

	public ItemRelatedWidgetState ItemRelatedWidgetState
	{
		get
		{
			return currentWidgetState;
		}
		set
		{
			currentWidgetState = value;
		}
	}

	public InventoryWidgetDataBase(Inventory inventory, Action<UIItemCell> onItemCellOver, Action<UIItemCell> onItemCellOut, Action<UIItemCell> onItemCellPress, Action<UIItemCell> onItemCellPress2, Action<UIItemCell> onItemCellDown, Func<Item, bool> itemsAvailableCondition = null, Func<Item, bool> customItemsNotShowCondition = null, ItemRelatedWidgetState widgetState = ItemRelatedWidgetState.Default)
	{
		Inventory = inventory;
		OnItemCellOver = onItemCellOver;
		OnItemCellOut = onItemCellOut;
		OnItemCellPress = onItemCellPress;
		OnItemCellPress2 = onItemCellPress2;
		OnItemCellDown = onItemCellDown;
		CustomItemsAvailableCondition = itemsAvailableCondition;
		CustomItemsNotShowCondition = customItemsNotShowCondition;
		ItemRelatedWidgetState = widgetState;
	}

	public void SetCustomOnCellPressedAction(Action<UIItemCell> action)
	{
		OnItemCellPress = action;
	}

	public void SetCustomOnCellPressed2Action(Action<UIItemCell> action)
	{
		OnItemCellPress2 = action;
	}
}
