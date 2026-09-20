using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Description("Does CurGDPoint Is Idle Point")]
[Category("Game Actions")]
[Name("Does CurGDPoint Is Idle Point", 0)]
public class Flow_IsCurGDPointIsIdlePoint : MyFlowNode
{
	public int idle_point_num;

	public GDPoint.IdlePointPrefix idle_point_prefix_enum = GDPoint.IdlePointPrefix.None;

	public string idle_point_prefix_string = string.Empty;

	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> in_npc_wgo = AddValueInput<WorldGameObject>("NPC");
		AddValueOutput("Point Num", () => idle_point_num);
		AddValueOutput("Prefix Enum", () => idle_point_prefix_enum);
		AddValueOutput("Prefix String", () => idle_point_prefix_string);
		FlowOutput flow_yes = AddFlowOutput("Is Idle Point");
		FlowOutput flow_no = AddFlowOutput("Not Idle Point");
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject worldGameObject = WGOParamOrSelf(in_npc_wgo);
			if (worldGameObject == null)
			{
				Debug.LogError("NPC is null!");
			}
			else
			{
				string cur_gd_point = worldGameObject.cur_gd_point;
				if (string.IsNullOrEmpty(cur_gd_point))
				{
					Debug.Log("not Is Idle point");
					flow_no.Call(f);
				}
				else if (GDPoint.TryParseIdlePointTag(cur_gd_point, out idle_point_prefix_enum, out idle_point_num) || cur_gd_point.StartsWith("personal_"))
				{
					if (cur_gd_point.StartsWith("personal_"))
					{
						idle_point_prefix_enum = GDPoint.IdlePointPrefix.Camp;
					}
					idle_point_prefix_string = GDPoint.GetIdlePrefix(idle_point_prefix_enum);
					flow_yes.Call(f);
					Debug.Log("Is Idle point");
				}
				else
				{
					Debug.Log("not Is Idle point");
					flow_no.Call(f);
				}
			}
		});
	}
}
