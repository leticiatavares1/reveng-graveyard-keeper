using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Description("If WGO is null, then self")]
[Category("Game Actions")]
[Name("Trigger Animation", 0)]
public class Flow_TriggerAnimation : MyFlowNode
{
	protected override void RegisterPorts()
	{
		WorldGameObject out_wgo = null;
		ValueInput<WorldGameObject> par_wgo = AddValueInput<WorldGameObject>("WGO");
		ValueInput<string> par_anim = AddValueInput<string>("Anim");
		FlowOutput flow_out = AddFlowOutput("Out", "Out");
		AddValueOutput("WGO", () => out_wgo);
		FlowOutput flow_finished = AddFlowOutput("Finished");
		AddFlowInput("In", delegate(Flow f)
		{
			out_wgo = WGOParamOrSelf(par_wgo);
			out_wgo.TriggerSmartAnimation(par_anim.value, delegate
			{
				flow_finished.Call(f);
			});
			flow_out.Call(f);
		});
	}
}
