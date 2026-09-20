using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Add Relation", 0)]
[Color("f386ca")]
[Category("Game Actions")]
public class Flow_AddRelation : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<string> in_param_name = AddValueInput<string>("NPC id (\"\" if self)", "npc_id");
		ValueInput<float> in_value = AddValueInput<float>("Value");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			string text = in_param_name.value;
			if (string.IsNullOrEmpty(text))
			{
				text = base.wgo.obj_id;
			}
			MainGame.me.player.AddToParams("_rel_" + text, in_value.value);
			flow_out.Call(f);
		});
	}
}
