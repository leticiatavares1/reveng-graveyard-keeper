using System.Collections.Generic;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Clear WGOs List Inventory", 0)]
[Description("Clear WGOs List Inventory")]
[Category("Game Actions")]
public class Flow_ClearWGOsListInventory : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<List<WorldGameObject>> in_wgo = AddValueInput<List<WorldGameObject>>("WGOs List");
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
						item.data.inventory.Clear();
						item.Redraw();
					}
				}
				flow_out.Call(f);
			}
		});
	}
}
