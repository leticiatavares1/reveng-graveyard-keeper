using UnityEngine;
using UnityEngine.UI;

public class BagInventoryWidget : InventoryWidget
{
	[SerializeField]
	private Image noiseLeft;

	[SerializeField]
	private Image noiseRight;

	[SerializeField]
	private Color noiseColorActive;

	[SerializeField]
	private Color noiseColorInactive;

	private BagInventoryWidgetData CastedData => data as BagInventoryWidgetData;

	protected override void SubscribeToInventoryEvents()
	{
		if (!subscribedInventoryEvents)
		{
			CastedData.ParentInventory.OnItemsAdd += base.OnItemsAdded;
			CastedData.ParentInventory.OnItemsRemove += base.OnItemsRemoved;
			data.Inventory.OnItemsRemove += base.OnItemsRemoved;
			data.Inventory.OnItemsAdd += base.OnItemsAdded;
			subscribedInventoryEvents = true;
		}
	}

	protected override void UnsubscribeFromInventoryEvents()
	{
		if (CastedData != null && CastedData.ParentInventory != null && subscribedInventoryEvents)
		{
			CastedData.ParentInventory.OnItemsAdd -= base.OnItemsAdded;
			CastedData.ParentInventory.OnItemsRemove -= base.OnItemsRemoved;
			data.Inventory.OnItemsAdd -= base.OnItemsAdded;
			data.Inventory.OnItemsRemove -= base.OnItemsRemoved;
			subscribedInventoryEvents = false;
		}
	}

	public override void Redraw()
	{
		if (subscribedInventoryEvents)
		{
			UnsubscribeFromInventoryEvents();
		}
		grid.constraintCount = data.Inventory.Data.Definition.bagSizeX;
		base.Redraw();
		if (!subscribedInventoryEvents)
		{
			SubscribeToInventoryEvents();
		}
		noiseLeft.transform.SetAsFirstSibling();
		noiseRight.transform.SetAsFirstSibling();
	}

	public override void UpdateItemRelatedWidgetStateForWidget()
	{
		base.UpdateItemRelatedWidgetStateForWidget();
		if (data.ItemRelatedWidgetState == ItemRelatedWidgetState.Default || data.ItemRelatedWidgetState == ItemRelatedWidgetState.Selected)
		{
			noiseLeft.color = noiseColorActive;
			noiseRight.color = noiseColorActive;
		}
		else
		{
			noiseLeft.color = noiseColorInactive;
			noiseRight.color = noiseColorInactive;
		}
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		Inventory @default = Inventory.GetDefault();
		Item item = new Item("bag_universal");
		item.AddItemToInventory(new Item("axe_0"));
		item.AddItemToInventory(new Item("shovel_0"));
		item.AddItemToInventory(new Item("leaf", 12));
		item.AddItemToInventory(new Item("carrot", 5));
		Item item2 = new Item("bag_tools");
		item2.AddItemToInventory(new Item("axe_1"));
		item2.AddItemToInventory(new Item("shovel_1"));
		@default.AddItemToInventory(item);
		@default.AddItemToInventory(item2);
		Draw(new InventoryWidgetDataBase(@default, null, null, null, null, null));
	}
}
