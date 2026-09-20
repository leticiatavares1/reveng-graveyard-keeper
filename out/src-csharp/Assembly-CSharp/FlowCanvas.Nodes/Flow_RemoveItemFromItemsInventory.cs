using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Remove item from Item's inventory", 0)]
[Description("Remove item from item's inventory")]
[Category("Game Actions")]
public class Flow_RemoveItemFromItemsInventory : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<Item> in_item_where = AddValueInput<Item>("Item (where to remove)");
		ValueInput<Item> in_item = AddValueInput<Item>("Item");
		ValueInput<string> in_item_id = AddValueInput<string>("Item ID");
		ValueInput<int> in_item_count = AddValueInput<int>("Items Count");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			if (in_item_where.value == null || in_item_where.value.IsEmpty())
			{
				Debug.LogError("Item (where to remove) is null!");
			}
			else if (in_item.value == null && in_item_id.isDefaultValue)
			{
				Debug.LogError("Item is null!");
			}
			else if (in_item.value != null && !in_item.value.IsEmpty())
			{
				in_item_where.value.RemoveItem(in_item.value);
				flow_out.Call(f);
			}
			else if (!string.IsNullOrEmpty(in_item_id.value) && in_item_count.value > 0)
			{
				in_item_where.value.RemoveItem(new Item(in_item_id.value, in_item_count.value));
				flow_out.Call(f);
			}
			else
			{
				flow_out.Call(f);
			}
		});
	}
}
