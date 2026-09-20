using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Description("If WGO is null, then self")]
[Category("Game Actions")]
[Name("Stop Following WGO", 0)]
[Icon("CubeArrowStraight", false, "")]
public class Flow_StopFollowWGO : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> par_wgo = AddValueInput<WorldGameObject>("WGO");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject worldGameObject = WGOParamOrSelf(par_wgo);
			if (worldGameObject == null)
			{
				Debug.LogError("FollowWGO error: WGO #1 is null");
			}
			else
			{
				worldGameObject.components.character.StopTargetFollowing();
				flow_out.Call(f);
			}
		});
	}
}
