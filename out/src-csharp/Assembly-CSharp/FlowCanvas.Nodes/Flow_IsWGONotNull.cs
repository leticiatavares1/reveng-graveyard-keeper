using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Is WGO not null", 0)]
[Category("Game Actions")]
[Description("WGO is not null")]
public class Flow_IsWGONotNull : MyFlowNode
{
	private WorldGameObject o;

	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> par_wgo = AddValueInput<WorldGameObject>("WGO");
		AddValueOutput("WGO", () => o);
		FlowOutput flow_yes = AddFlowOutput("WGO Is Not NULL");
		FlowOutput flow_no = AddFlowOutput("WGO Is NULL");
		AddFlowInput("In", delegate(Flow f)
		{
			if (par_wgo.value != null)
			{
				o = par_wgo.value;
				flow_yes.Call(f);
			}
			else
			{
				flow_no.Call(f);
			}
		});
	}
}
