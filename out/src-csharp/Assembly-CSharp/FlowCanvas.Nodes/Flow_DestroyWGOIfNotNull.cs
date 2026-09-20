using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Destroy WGO if not null", 0)]
[Category("Game Actions")]
[Icon("Cross", false, "")]
[Description("If WGO is null, then do nothing")]
public class Flow_DestroyWGOIfNotNull : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> par_wgo = AddValueInput<WorldGameObject>("WGO");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			if (par_wgo.value != null)
			{
				par_wgo.value.DestroyMe();
			}
			flow_out.Call(f);
		});
	}
}
