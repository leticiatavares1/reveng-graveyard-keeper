using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("On Disable", 7)]
[Description("Called when the Graph is Disabled")]
[Category("Events/Graph")]
public class DisableEvent : EventNode
{
	private FlowOutput disable;

	public override void OnGraphStoped()
	{
		disable.Call(default(Flow));
	}

	protected override void RegisterPorts()
	{
		disable = AddFlowOutput("Out");
	}
}
