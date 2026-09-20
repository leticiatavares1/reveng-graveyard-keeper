using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Try Remove Stacked Church Visitors", 0)]
public class Flow_TryRemoveStackedChurchVisitors : MyFlowNode
{
	protected override void RegisterPorts()
	{
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			WorldMap.TryRemoveStackedChurchVisitors();
			flow_out.Call(f);
		});
	}
}
