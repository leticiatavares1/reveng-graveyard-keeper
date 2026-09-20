using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Description("Set WGO State")]
[Name("Set WGO State", 0)]
[Category("Game Actions")]
[Icon("CubeArrowStraight", false, "")]
public class Flow_SetWGOState : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> par_wgo = AddValueInput<WorldGameObject>("WGO");
		ValueInput<bool> in_state = AddValueInput<bool>("State");
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
				worldGameObject.gameObject.SetActive(in_state.value);
				worldGameObject.Redraw();
				flow_out.Call(f);
			}
		});
	}
}
