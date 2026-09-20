using System.Collections.Generic;
using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Remove Items From WgoData", 0)]
[Category("Game/Item")]
[Color("FFFFFF")]
public class Flow_RemoveItemsFromWgoData : GKCustomFlowNodeWithWgoData
{
	[GatherPortsCallback]
	public bool splitItemsByOne;

	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<string> itemId;

	private ValueOutput<List<Item>> removedItems;

	private ValueOutput<int> removedCount;

	private List<Item> removedItemsList;

	private int removedCountValue;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), RemoveItems);
		@out = AddFlowOutput("out".CapitalizeFirst());
		base.RegisterPorts();
		itemId = AddValueInput<string>("itemId");
		removedItems = AddValueOutput("removedItems", () => removedItemsList);
		removedCount = AddValueOutput("removedCount", () => removedCountValue);
	}

	private void RemoveItems(Flow flow)
	{
		removedItemsList = GetWgoData()?.Inventory.RemoveItemById(itemId.value);
		removedCountValue = removedItemsList?.Count ?? 0;
		if (splitItemsByOne && removedCountValue > 0)
		{
			List<Item> list = new List<Item>(removedItemsList);
			removedItemsList.Clear();
			removedCountValue = 0;
			foreach (Item item in list)
			{
				int count = item.Count;
				for (int i = 0; i < count; i++)
				{
					removedItemsList.Add(item.Split(1));
					removedCountValue++;
				}
			}
		}
		Debug.Log($"Flow_RemoveItemsFromWgoData: {itemId.value}, count: {removedItemsList?.Count}");
		@out.Call(flow);
	}
}
