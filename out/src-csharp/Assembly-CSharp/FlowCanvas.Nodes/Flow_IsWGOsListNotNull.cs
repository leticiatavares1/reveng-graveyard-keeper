using System.Collections.Generic;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Is WGOs List not null", 0)]
[Category("Game Actions")]
[Description("WGOs List is not null")]
public class Flow_IsWGOsListNotNull : MyFlowNode
{
	private List<WorldGameObject> o;

	protected override void RegisterPorts()
	{
		ValueInput<List<WorldGameObject>> par_wgo = AddValueInput<List<WorldGameObject>>("WGO List");
		AddValueOutput("WGO List", () => o);
		FlowOutput flow_yes = AddFlowOutput("WGOs List Is Not NULL");
		FlowOutput flow_no = AddFlowOutput("WGOs List Is NULL");
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
