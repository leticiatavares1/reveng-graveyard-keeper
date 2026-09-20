using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Get Current GDPoint", 0)]
[Category("Game Actions")]
[Description("If WGO is null, then self")]
[Icon("Cube", false, "")]
public class Flow_GetCurGDPoint : MyFlowNode
{
	private string cur_gd_point = "";

	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> par_wgo = AddValueInput<WorldGameObject>("WGO");
		AddValueOutput("GDPoint", () => cur_gd_point);
		FlowOutput flow_not_empty = AddFlowOutput("Not Empty");
		FlowOutput flow_empty = AddFlowOutput("Empty");
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject worldGameObject = WGOParamOrSelf(par_wgo);
			if (worldGameObject == null)
			{
				Debug.LogError("GetCurGDPoint error: WGO is null");
			}
			else
			{
				cur_gd_point = worldGameObject.cur_gd_point;
				if (string.IsNullOrEmpty(cur_gd_point))
				{
					flow_empty.Call(f);
				}
				else
				{
					flow_not_empty.Call(f);
				}
			}
		});
	}
}
