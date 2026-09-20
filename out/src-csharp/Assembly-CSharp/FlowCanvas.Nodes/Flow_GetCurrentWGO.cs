using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Get Current WGO", 0)]
[Icon("Cube", false, "")]
public class Flow_GetCurrentWGO : MyFlowNode
{
	private WorldGameObject current_wgo;

	protected override void RegisterPorts()
	{
		AddValueOutput("Current WGO", () => current_wgo);
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			current_wgo = base.wgo;
			if (current_wgo == null)
			{
				Debug.LogError("FollowWGO error: WGO is null");
			}
			else
			{
				flow_out.Call(f);
			}
		});
	}
}
