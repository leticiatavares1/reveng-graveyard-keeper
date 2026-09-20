using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Param flag", 0)]
[Category("Game Actions")]
[Icon("Flag", false, "")]
[Description("If WGO is null, then self")]
public class Flow_ParamFlag : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> par_wgo = AddValueInput<WorldGameObject>("WGO");
		ValueInput<string> par_param = AddValueInput<string>("param");
		FlowOutput flow_eq0 = AddFlowOutput("<color=red>✘</color>", "== 0");
		FlowOutput flow_moreeq1 = AddFlowOutput("<color=green>✔</color>", ">= 1");
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject worldGameObject = WGOParamOrSelf(par_wgo);
			if (!(worldGameObject == null))
			{
				int paramInt = worldGameObject.GetParamInt(par_param.value);
				if (paramInt == 0)
				{
					flow_eq0.Call(f);
				}
				if (paramInt >= 1)
				{
					flow_moreeq1.Call(f);
				}
			}
		});
	}
}
