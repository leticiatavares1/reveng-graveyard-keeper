using System.Collections.Generic;
using DLCRefugees;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Get Spawned Refugee WGO List", 0)]
[Category("Game Actions")]
public class Flow_GetSpawnedRefugeeWGOList : MyFlowNode
{
	private List<WorldGameObject> _refugee_list;

	protected override void RegisterPorts()
	{
		AddValueOutput("WGO List", () => _refugee_list);
		FlowOutput @out = AddFlowOutput("Out");
		AddValueOutput("Refugees Count", () => RefugeesCampEngine.instance.GetSpawnedRefugeeList().Count);
		AddFlowInput("In", delegate(Flow f)
		{
			_refugee_list = RefugeesCampEngine.instance.GetSpawnedRefugeeList();
			@out.Call(f);
		});
	}
}
