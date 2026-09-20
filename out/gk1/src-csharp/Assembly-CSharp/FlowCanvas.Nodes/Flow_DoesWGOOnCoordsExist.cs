using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Description("If WGO not found, return false")]
[Category("Game Actions")]
[Name("Does WGO On Coords Exist", 0)]
public class Flow_DoesWGOOnCoordsExist : MyFlowNode
{
	protected override void RegisterPorts()
	{
		bool is_found = false;
		AddValueOutput("Is Found", () => is_found);
		ValueInput<string> par_obj_id = AddValueInput<string>("WGO obj_id");
		ValueInput<float> x_coord = AddValueInput<float>("'X' coordinate");
		ValueInput<float> y_coord = AddValueInput<float>("'Y' coordinate");
		FlowOutput flow_out = AddFlowOutput("Out");
		FlowOutput flow_yes = AddFlowOutput("True");
		FlowOutput flow_no = AddFlowOutput("False");
		AddFlowInput("In", delegate(Flow f)
		{
			if (par_obj_id != null)
			{
				is_found = WorldMap.DoesWGOOnCoordsExist(par_obj_id.value, new Vector2(x_coord.value, y_coord.value));
				if (is_found)
				{
					flow_yes.Call(f);
				}
				else
				{
					flow_no.Call(f);
				}
				flow_out.Call(f);
			}
		});
	}
}
