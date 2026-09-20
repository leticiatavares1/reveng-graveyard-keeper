using System;
using System.Collections.Generic;
using LazyBearTechnology;

public abstract class InventoryWidgetBase<T> : LazyWidget<InventoryWidgetDataBase> where T : InventoryWidgetDataBase
{
	public delegate int ItemPriceDelegate(Item item, int countModificator);

	protected Action<UIItemCell> onItemCellOver;

	protected Action<UIItemCell> onItemCellOut;

	protected Action<UIItemCell> onItemCellPress;

	protected Action<UIItemCell> onItemCellPress2;

	protected Action<UIItemCell> onItemCellDown;

	protected bool subscribedInventoryEvents;

	public override void Redraw()
	{
		base.Redraw();
		onItemCellOver = data.OnItemCellOver;
		onItemCellOut = data.OnItemCellOut;
		onItemCellPress = data.OnItemCellPress;
		onItemCellPress2 = data.OnItemCellPress2;
		onItemCellDown = data.OnItemCellDown;
	}

	public override void Hide()
	{
		base.Hide();
		ClearCallbacks();
	}

	public void UpdateItemRelatedWidgetStateForEveryThing()
	{
		UpdateItemRelatedWidgetStateForCells();
		UpdateItemRelatedWidgetStateForWidget();
	}

	public abstract void UpdateItemRelatedWidgetStateForCells();

	public virtual void UpdateItemRelatedWidgetStateForWidget()
	{
	}

	protected virtual void ClearCallbacks()
	{
		onItemCellOver = null;
		onItemCellOut = null;
		onItemCellPress = null;
		onItemCellPress2 = null;
		onItemCellDown = null;
	}

	protected virtual void SubscribeToInventoryEvents()
	{
		if (!subscribedInventoryEvents)
		{
			data.Inventory.OnItemsAdd += OnItemsAdded;
			data.Inventory.OnItemsRemove += OnItemsRemoved;
			subscribedInventoryEvents = true;
		}
	}

	protected virtual void UnsubscribeFromInventoryEvents()
	{
		if (data != null && data.Inventory != null && subscribedInventoryEvents)
		{
			data.Inventory.OnItemsAdd -= OnItemsAdded;
			data.Inventory.OnItemsRemove -= OnItemsRemoved;
			subscribedInventoryEvents = false;
		}
	}

	protected void OnItemsAdded(List<Item> items)
	{
		Redraw();
	}

	protected void OnItemsRemoved(List<Item> items)
	{
		Redraw();
	}
}
