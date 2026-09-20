using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Clear WGO Inventory", 0)]
[Category("Game Actions")]
[Description("Clear WGO Inventory")]
public class Flow_ClearWGOInventory : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> in_wgo = AddValueInput<WorldGameObject>("WGO");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject worldGameObject = WGOParamOrSelf(in_wgo);
			if (worldGameObject == null)
			{
				Debug.LogError("WGO is null");
			}
			else
			{
				worldGameObject.data.inventory.Clear();
				worldGameObject.Redraw();
				flow_out.Call(f);
			}
		});
	}
}
