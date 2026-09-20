using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Remove item from inventory", 0)]
[Description("Remove item from inventory")]
public class Flow_RemoveItemFromInventory : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> in_wgo = AddValueInput<WorldGameObject>("WGO");
		ValueInput<Item> in_item = AddValueInput<Item>("Item");
		ValueInput<string> in_item_id = AddValueInput<string>("Item ID");
		ValueInput<int> in_item_count = AddValueInput<int>("Items Count");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject worldGameObject = WGOParamOrSelf(in_wgo);
			if (worldGameObject == null)
			{
				Debug.LogError("WGO is null!");
			}
			else if (in_item.value == null && in_item_id.isDefaultValue)
			{
				Debug.LogError("Item is null!");
			}
			else if (in_item.value != null && !in_item.value.IsEmpty())
			{
				worldGameObject.data.RemoveItem(in_item.value);
				flow_out.Call(f);
			}
			else if (!string.IsNullOrEmpty(in_item_id.value) && in_item_count.value > 0)
			{
				worldGameObject.data.RemoveItem(new Item(in_item_id.value, in_item_count.value));
				flow_out.Call(f);
			}
			else
			{
				flow_out.Call(f);
			}
		});
	}
}
