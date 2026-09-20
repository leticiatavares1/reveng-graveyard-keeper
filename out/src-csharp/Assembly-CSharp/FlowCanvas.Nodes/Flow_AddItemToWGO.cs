using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Description("Add Item To WGO")]
[Category("Game Actions")]
[Name("Add Item To WGO", 0)]
public class Flow_AddItemToWGO : MyFlowNode
{
	private bool item_was_added;

	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> in_wgo = AddValueInput<WorldGameObject>("WGO");
		ValueInput<Item> in_item = AddValueInput<Item>("Item");
		AddValueInput<bool>("One Hand Slot");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddValueOutput("Item was added?", () => item_was_added);
		AddFlowInput("In", delegate(Flow f)
		{
			if (in_wgo.value == null)
			{
				Debug.LogError("WGO is null");
			}
			else
			{
				item_was_added = in_wgo.value.AddToInventory(in_item.value);
				in_wgo.value.Redraw();
				flow_out.Call(f);
			}
		});
	}
}
