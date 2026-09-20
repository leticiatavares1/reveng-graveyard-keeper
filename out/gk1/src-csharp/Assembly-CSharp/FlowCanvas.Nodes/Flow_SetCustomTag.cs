using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Icon("Cube", false, "")]
[Description("If WGO is null, then self")]
[Category("Game Actions")]
[Name("Set Custom Tag", 0)]
public class Flow_SetCustomTag : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> par_wgo = AddValueInput<WorldGameObject>("WGO");
		ValueInput<string> in_custom_tag = AddValueInput<string>("custom_tag");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject worldGameObject = WGOParamOrSelf(par_wgo);
			if (worldGameObject == null)
			{
				Debug.LogError("Flow_SetCustomTag error: WGO is null");
			}
			else
			{
				worldGameObject.custom_tag = in_custom_tag.value;
				flow_out.Call(f);
			}
		});
	}
}
