using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Description("Called when the Graph is enabled")]
[Category("Events/Graph")]
[Name("On Enable", 8)]
public class EnableEvent : EventNode
{
	private FlowOutput enable;

	public override void OnGraphStarted()
	{
		enable.Call(default(Flow));
	}

	protected override void RegisterPorts()
	{
		enable = AddFlowOutput("Out");
	}
}
