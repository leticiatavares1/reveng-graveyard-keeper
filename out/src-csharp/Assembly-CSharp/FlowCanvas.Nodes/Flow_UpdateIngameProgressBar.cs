using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Update Ingame Progress Bar", 0)]
public class Flow_UpdateIngameProgressBar : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> in_wgo = AddValueInput<WorldGameObject>("WGO");
		FlowOutput flow_out = AddFlowOutput("out");
		AddFlowInput("In", delegate(Flow f)
		{
			if (in_wgo.value == null)
			{
				Debug.LogError("Flow_UpdateIngameProgressBar error: WGO is null!");
				flow_out.Call(f);
			}
			else
			{
				IngameProgressBar componentInChildren = in_wgo.value.GetComponentInChildren<IngameProgressBar>(includeInactive: true);
				if (componentInChildren == null)
				{
					Debug.LogError("Flow_UpdateIngameProgressBar error: bar not found on WGO \"" + in_wgo.value.obj_id + "\"!");
					flow_out.Call(f);
				}
				else
				{
					componentInChildren.UpdateBar();
					flow_out.Call(f);
				}
			}
		});
	}
}
