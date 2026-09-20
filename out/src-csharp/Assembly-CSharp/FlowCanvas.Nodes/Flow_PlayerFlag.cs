using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Player flag", 0)]
[Icon("Flag", false, "")]
[Category("Game Actions")]
public class Flow_PlayerFlag : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<string> par_param = AddValueInput<string>("param");
		FlowOutput flow_eq0 = AddFlowOutput("<color=red>✘</color>", "== 0");
		FlowOutput flow_moreeq1 = AddFlowOutput("<color=green>✔</color>", ">= 1");
		AddFlowInput("In", delegate(Flow f)
		{
			int paramInt = MainGame.me.player.GetParamInt(par_param.value);
			if (paramInt == 0)
			{
				flow_eq0.Call(f);
			}
			if (paramInt >= 1)
			{
				flow_moreeq1.Call(f);
			}
		});
	}
}
