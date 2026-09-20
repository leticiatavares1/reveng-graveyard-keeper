using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Get Linked Worker WGO", 0)]
public class Flow_GetLinkedWorker : MyFlowNode
{
	private WorldGameObject o_wgo;

	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> par_wgo = AddValueInput<WorldGameObject>("WGO");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddValueOutput("WGO", () => o_wgo);
		AddFlowInput("In", delegate(Flow f)
		{
			o_wgo = par_wgo.value.linked_worker;
			flow_out.Call(f);
		});
	}
}
