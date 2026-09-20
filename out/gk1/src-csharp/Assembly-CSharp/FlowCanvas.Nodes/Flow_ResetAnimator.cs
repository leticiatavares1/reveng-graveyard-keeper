using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Reset Animator", 0)]
[Description("If WGO is null, then self")]
[Category("Game Actions")]
public class Flow_ResetAnimator : MyFlowNode
{
	protected override void RegisterPorts()
	{
		WorldGameObject out_wgo = null;
		ValueInput<WorldGameObject> par_wgo = AddValueInput<WorldGameObject>("WGO");
		FlowOutput flow_out = AddFlowOutput("Out", "Out");
		AddValueOutput("WGO", () => out_wgo);
		AddFlowInput("In", delegate(Flow f)
		{
			out_wgo = WGOParamOrSelf(par_wgo);
			out_wgo.ResetAnimator();
			flow_out.Call(f);
		});
	}
}
