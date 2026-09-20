using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Try Free Idle Point", 0)]
[Description("Try Free Idle Point")]
[Category("Game Actions")]
public class Flow_TryFreeIdlePoint : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> in_stock_wgo = AddValueInput<WorldGameObject>("Stock WGO");
		ValueInput<string> in_points_prefix = AddValueInput<string>("Points Prefix");
		ValueInput<WorldGameObject> in_npc_wgo = AddValueInput<WorldGameObject>("NPC");
		ValueInput<int> in_free_point_num = AddValueInput<int>("Free Point Num");
		FlowOutput flow_yes = AddFlowOutput("Done");
		FlowOutput flow_no = AddFlowOutput("Error");
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject worldGameObject = null;
			if (!in_stock_wgo.HasValue())
			{
				Debug.LogError("Stock WGO not connected!");
				flow_no.Call(f);
			}
			worldGameObject = in_stock_wgo.value;
			string param_name;
			if (in_npc_wgo.HasValue())
			{
				if (!in_npc_wgo.value.cur_gd_point.StartsWith(in_points_prefix.value))
				{
					if (in_npc_wgo.value.cur_gd_point.StartsWith("personal_"))
					{
						if (in_npc_wgo.value.data.GetParamInt(in_npc_wgo.value.cur_gd_point) == 1)
						{
							in_npc_wgo.value.data.SetParam(in_npc_wgo.value.cur_gd_point, 0f);
						}
					}
					else
					{
						Debug.LogError("Can not free idle point: npc.gd_point is not idle point!");
						flow_no.Call(f);
					}
					return;
				}
				param_name = in_npc_wgo.value.cur_gd_point;
			}
			else
			{
				if (in_free_point_num.value <= 0)
				{
					Debug.LogError("Can not free idle point: point to free not set!");
					flow_no.Call(f);
					return;
				}
				param_name = in_points_prefix.value + in_free_point_num.value;
			}
			if (worldGameObject.data.GetParamInt(param_name) == 1)
			{
				worldGameObject.data.SetParam(param_name, 0f);
				flow_yes.Call(f);
			}
			else
			{
				Debug.LogError("Can not free idle point: point is already free or not set!");
				flow_no.Call(f);
			}
		});
	}
}
