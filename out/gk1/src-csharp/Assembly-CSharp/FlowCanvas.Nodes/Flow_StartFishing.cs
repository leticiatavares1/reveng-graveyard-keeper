using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Start Fishing", 0)]
[Category("Game Actions")]
public class Flow_StartFishing : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> in_fishing_spot_wgo = AddValueInput<WorldGameObject>("Fishing Spot WGO");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			if (WGOParamOrSelf(in_fishing_spot_wgo) == null)
			{
				Debug.LogError("Can not start fishing: Fishing spot is null!");
			}
			else
			{
				GUIElements.me.fishing.Open(in_fishing_spot_wgo.value);
				flow_out.Call(f);
			}
		});
	}
}
