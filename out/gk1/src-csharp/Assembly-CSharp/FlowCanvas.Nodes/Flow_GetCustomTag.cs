using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Get Custom Tag", 0)]
[Category("Game Actions")]
[Description("If WGO is null, then self")]
[Icon("Cube", false, "")]
public class Flow_GetCustomTag : MyFlowNode
{
	private string custom_tag = "";

	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> par_wgo = AddValueInput<WorldGameObject>("WGO");
		AddValueOutput("Custom tag", () => custom_tag);
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject worldGameObject = WGOParamOrSelf(par_wgo);
			if (worldGameObject == null)
			{
				Debug.LogError("FollowWGO error: WGO is null");
			}
			else
			{
				custom_tag = worldGameObject.custom_tag;
				flow_out.Call(f);
			}
		});
	}
}
