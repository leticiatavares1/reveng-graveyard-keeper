using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Compare Relation", 0)]
public class Flow_CompareRelation : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<string> in_param_name = AddValueInput<string>("NPC id (\"\" if self)", "npc_id");
		ValueInput<float> in_value = AddValueInput<float>("Value");
		FlowOutput flow_equal = AddFlowOutput("rel == value");
		FlowOutput flow_more = AddFlowOutput("rel > value");
		FlowOutput flow_less = AddFlowOutput("rel < value");
		AddFlowInput("In", delegate(Flow f)
		{
			string text = in_param_name.value;
			if (string.IsNullOrEmpty(text))
			{
				text = base.wgo.obj_id;
			}
			float num = MainGame.me.player.GetParam("_rel_" + text) - in_value.value;
			if (Mathf.Abs(num) < 0.01f)
			{
				flow_equal.Call(f);
			}
			else if (num > 0f)
			{
				flow_more.Call(f);
			}
			else
			{
				flow_less.Call(f);
			}
		});
	}
}
