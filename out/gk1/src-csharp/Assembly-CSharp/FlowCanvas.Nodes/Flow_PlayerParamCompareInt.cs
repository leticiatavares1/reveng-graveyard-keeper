using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Player Param compare Int", 0)]
[Category("Game Actions")]
public class Flow_PlayerParamCompareInt : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<string> par_param = AddValueInput<string>("param");
		ValueInput<int> par_value = AddValueInput<int>("value");
		FlowOutput flow_eq = AddFlowOutput("==");
		FlowOutput flow_neq = AddFlowOutput("!=");
		FlowOutput flow_more = AddFlowOutput(">");
		FlowOutput flow_less = AddFlowOutput("<");
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject player = MainGame.me.player;
			string text = par_param.value;
			if (text == "_rel")
			{
				text = "_rel_" + base.wgo.obj_id;
			}
			int paramInt = player.GetParamInt(text);
			if (paramInt == par_value.value)
			{
				flow_eq.Call(f);
			}
			else
			{
				flow_neq.Call(f);
			}
			if (paramInt > par_value.value)
			{
				flow_more.Call(f);
			}
			if (paramInt < par_value.value)
			{
				flow_less.Call(f);
			}
		});
	}
}
