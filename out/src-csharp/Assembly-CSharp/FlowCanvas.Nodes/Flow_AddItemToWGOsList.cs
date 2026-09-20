using System.Collections.Generic;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Add Item To WGOs List", 0)]
[Category("Game Actions")]
[Description("Add Item To WGO")]
public class Flow_AddItemToWGOsList : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<List<WorldGameObject>> in_wgo = AddValueInput<List<WorldGameObject>>("WGOs List");
		ValueInput<Item> in_item = AddValueInput<Item>("Item");
		AddValueInput<bool>("One Hand Slot");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			if (in_wgo.value == null)
			{
				Debug.LogError("WGO is null");
			}
			else
			{
				foreach (WorldGameObject item in in_wgo.value)
				{
					if (!(item == null))
					{
						item.AddToInventory(in_item.value);
						item.Redraw();
					}
				}
				flow_out.Call(f);
			}
		});
	}
}
