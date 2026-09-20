using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Description("If WGO is null, then self")]
[Name("Does WGO Stay On GDPoint", 0)]
public class Flow_DoesWGOStayOnGDPoint : MyFlowNode
{
	private bool is_wgo_stay;

	protected override void RegisterPorts()
	{
		FlowOutput out_flow = AddFlowOutput("Out");
		ValueInput<WorldGameObject> par_wgo = AddValueInput<WorldGameObject>("WGO");
		ValueInput<string> gd_tag = AddValueInput<string>("GD tag");
		AddValueOutput("stay?", () => is_wgo_stay);
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject worldGameObject = WGOParamOrSelf(par_wgo);
			GDPoint gDPointByGDTag = WorldMap.GetGDPointByGDTag(gd_tag.value);
			if (worldGameObject != null && gDPointByGDTag != null)
			{
				is_wgo_stay = (Vector2)worldGameObject.transform.position == (Vector2)gDPointByGDTag.transform.position;
			}
			out_flow.Call(f);
		});
	}
}
