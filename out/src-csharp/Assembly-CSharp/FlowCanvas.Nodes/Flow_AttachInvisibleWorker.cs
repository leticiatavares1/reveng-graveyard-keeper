using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Attach Invisible Worker", 0)]
public class Flow_AttachInvisibleWorker : MyFlowNode
{
	private WorldGameObject out_o;

	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> par_workbench_wgo = AddValueInput<WorldGameObject>("Workbench");
		AddValueOutput("WGO", () => out_o);
		FlowOutput flow_success = AddFlowOutput("Succeed");
		FlowOutput flow_fail = AddFlowOutput("Failed");
		AddFlowInput("In", delegate(Flow f)
		{
			if (WorldMap.AttachInvisibleWorker(WGOParamOrSelf(par_workbench_wgo), out out_o))
			{
				flow_success.Call(f);
			}
			else
			{
				flow_fail.Call(f);
			}
		});
	}
}
