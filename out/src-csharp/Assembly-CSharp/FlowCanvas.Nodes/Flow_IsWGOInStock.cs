using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Is WGO In Stock", 0)]
[Category("Game Actions")]
[Description("If WGO is null, then self")]
public class Flow_IsWGOInStock : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> par_wgo = AddValueInput<WorldGameObject>("WGO");
		ValueInput<string> in_gd_point_tag = AddValueInput<string>("GD Point Tag");
		FlowOutput flow_yes = AddFlowOutput("Yes");
		FlowOutput flow_no = AddFlowOutput("No");
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject worldGameObject = WGOParamOrSelf(par_wgo);
			Vector2 vector = WorldMap.GetGDPointByGDTag((!string.IsNullOrEmpty(in_gd_point_tag.value)) ? in_gd_point_tag.value : "default_destroy_point").transform.position;
			Vector2 vector2 = worldGameObject.transform.position;
			if (vector == vector2)
			{
				flow_yes.Call(f);
			}
			else
			{
				flow_no.Call(f);
			}
		});
	}
}
