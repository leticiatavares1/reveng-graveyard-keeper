using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Follow WGO", 0)]
[Description("If WGO is null, then self")]
[Icon("CubeArrowStraight", false, "")]
[Category("Game Actions")]
public class Flow_FollowWGO : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> par_wgo = AddValueInput<WorldGameObject>("WGO");
		ValueInput<WorldGameObject> par_wgo2 = AddValueInput<WorldGameObject>("WGO to follow");
		FlowOutput flow_out = AddFlowOutput("Out");
		FlowOutput flow_came = AddFlowOutput("Came to dest");
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject worldGameObject = WGOParamOrSelf(par_wgo);
			if (worldGameObject == null)
			{
				Debug.LogError("FollowWGO error: WGO #1 is null");
			}
			else if (par_wgo2.value == null)
			{
				Debug.LogError("FollowWGO error: WGO #2 is null");
			}
			else
			{
				worldGameObject.components.character.FollowTarget(par_wgo2.value, 1.2f, delegate
				{
					flow_came.Call(f);
				});
				flow_out.Call(f);
			}
		});
	}
}
