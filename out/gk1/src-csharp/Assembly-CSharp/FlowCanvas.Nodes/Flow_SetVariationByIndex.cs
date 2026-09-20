using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Set Variation by Index", 0)]
public class Flow_SetVariationByIndex : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> in_wgo = AddValueInput<WorldGameObject>("WGO");
		ValueInput<int> variation_index = AddValueInput<int>("Variation Index");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject worldGameObject = WGOParamOrSelf(in_wgo);
			if (worldGameObject == null)
			{
				Debug.LogError("Flow_SetVariationByIndex error: WGO is null!");
				flow_out.Call(f);
			}
			else
			{
				Debug.Log("Set variation for WGO by index:" + variation_index.value);
				worldGameObject.SetVariationByIndex(variation_index.value);
				flow_out.Call(f);
			}
		});
	}
}
