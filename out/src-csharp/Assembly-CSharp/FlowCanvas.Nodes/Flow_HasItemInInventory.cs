using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Has item in inventory", 0)]
[Category("Game Actions")]
[Description("True if wgo has item with this ID")]
public class Flow_HasItemInInventory : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> in_wgo = AddValueInput<WorldGameObject>("WGO");
		ValueInput<Item> in_item = AddValueInput<Item>("Item");
		ValueInput<string> in_item_id = AddValueInput<string>("Item ID");
		FlowOutput flow_yes = AddFlowOutput("Yes");
		FlowOutput flow_no = AddFlowOutput("No");
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject worldGameObject = WGOParamOrSelf(in_wgo);
			string text = null;
			if (worldGameObject == null)
			{
				Debug.LogError("WGO is null!");
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
			else if (worldGameObject.data.HasItemInInventory(text))
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
