using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Get copy of WGO's GameRes", 0)]
public class Flow_GetCopyOfWGOParams : MyFlowNode
{
	private GameRes copy;

	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> in_wgo = AddValueInput<WorldGameObject>("WGO");
		AddValueOutput("GameRes", () => copy);
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			copy = in_wgo.value.data.GetParams().Clone();
			flow_out.Call(f);
		});
	}
}
