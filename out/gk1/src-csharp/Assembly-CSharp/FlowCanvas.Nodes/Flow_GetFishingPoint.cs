using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Get Fishing Point", 0)]
[Category("Game Actions")]
[Description("Get fishing point of fishing spot")]
public class Flow_GetFishingPoint : MyFlowNode
{
	protected override void RegisterPorts()
	{
		GDPoint fishing_point = null;
		string fishing_point_tag = string.Empty;
		AddValueOutput("GDPoint", () => fishing_point);
		AddValueOutput("GDPoint tag", () => fishing_point_tag);
		ValueInput<WorldGameObject> in_wgo = AddValueInput<WorldGameObject>("WGO");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject worldGameObject = WGOParamOrSelf(in_wgo);
			if (worldGameObject == null)
			{
				Debug.LogError("WGO is null!");
			}
			else
			{
				fishing_point_tag = worldGameObject.obj_id + "_point";
				fishing_point = WorldMap.GetGDPointByGDTag(fishing_point_tag);
				flow_out.Call(f);
			}
		});
	}
}
