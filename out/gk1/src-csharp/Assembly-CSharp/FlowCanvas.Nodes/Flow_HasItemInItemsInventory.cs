using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Description("True if item has another item with this ID")]
[Name("Has item in Item's inventory", 0)]
[Category("Game Actions")]
public class Flow_HasItemInItemsInventory : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<Item> in_item_where = AddValueInput<Item>("Item (where to check?)");
		ValueInput<Item> in_item = AddValueInput<Item>("Item");
		ValueInput<string> in_item_id = AddValueInput<string>("Item ID");
		FlowOutput flow_yes = AddFlowOutput("Yes");
		FlowOutput flow_no = AddFlowOutput("No");
		AddFlowInput("In", delegate(Flow f)
		{
			string text = null;
			if (in_item_where.value == null || in_item_where.value.IsEmpty())
			{
				Debug.LogError("Item (where to check) is null");
				flow_no.Call(f);
			}
			else if ((in_item.value == null || in_item.value.IsEmpty()) && in_item_id.isDefaultValue)
			{
				Debug.LogError("Item is null!");
				flow_no.Call(f);
			}
			else if (in_item.value != null && !in_item.value.IsEmpty())
			{
				text = in_item.value.id;
			}
			else if (!string.IsNullOrEmpty(in_item_id.value))
			{
				text = in_item_id.value;
			}
			if (string.IsNullOrEmpty(text))
			{
				Debug.LogError("Item id not set!");
				flow_no.Call(f);
			}
			else if (in_item_where.value.HasItemInInventory(text))
			{
				flow_yes.Call(f);
			}
			else
			{
				flow_no.Call(f);
			}
		});
	}
}
