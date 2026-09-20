using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Get item count from inventory", 0)]
[Category("Game Actions")]
[Description("Returns count of need item in inventory")]
public class Flow_HasItemCountInInventory : MyFlowNode
{
	private int item_count;

	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> in_wgo = AddValueInput<WorldGameObject>("WGO");
		ValueInput<Item> in_item = AddValueInput<Item>("Item");
		ValueInput<string> in_item_id = AddValueInput<string>("Item ID");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddValueOutput("Item count", () => item_count);
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject worldGameObject = WGOParamOrSelf(in_wgo);
			string text = null;
			if (worldGameObject == null)
			{
				Debug.LogError("WGO is null!");
				flow_out.Call(f);
			}
			else if ((in_item.value == null || in_item.value.IsEmpty()) && in_item_id.isDefaultValue)
			{
				Debug.LogError("Item is null!");
				flow_out.Call(f);
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
				flow_out.Call(f);
			}
			else
			{
				item_count = worldGameObject.data.GetItemsCount(text);
				flow_out.Call(f);
			}
		});
	}
}
