using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Add Item To Item's inventory", 0)]
public class Flow_AddItemToItemsInventory : MyFlowNode
{
	private bool item_was_added;

	protected override void RegisterPorts()
	{
		ValueInput<Item> in_item_where = AddValueInput<Item>("Item (where to add)");
		ValueInput<Item> in_item = AddValueInput<Item>("Item");
		AddValueInput<bool>("One Hand Slot");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddValueOutput("Item was added?", () => item_was_added);
		AddFlowInput("In", delegate(Flow f)
		{
			if (in_item_where.value == null || in_item_where.value.IsEmpty())
			{
				Debug.LogError("Item (where to add) null or empty");
				flow_out.Call(f);
			}
			else
			{
				item_was_added = in_item_where.value.AddItem(in_item.value);
				flow_out.Call(f);
			}
		});
	}
}
