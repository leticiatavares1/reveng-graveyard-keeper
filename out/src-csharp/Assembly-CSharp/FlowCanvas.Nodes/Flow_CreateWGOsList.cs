using System.Collections.Generic;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Create WGOs list", 0)]
[Category("Game Actions")]
public class Flow_CreateWGOsList : MyFlowNode
{
	protected override void RegisterPorts()
	{
		FlowOutput flow_out = AddFlowOutput("Out");
		List<WorldGameObject> out_list_var = new List<WorldGameObject>();
		AddValueOutput("list", () => out_list_var);
		AddFlowInput("In", delegate(Flow f)
		{
			out_list_var = new List<WorldGameObject>();
			flow_out.Call(f);
		});
	}
}
