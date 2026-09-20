using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("On Start", 9)]
[Category("Events/Graph")]
[Description("Called only once and the first time the Graph is enabled.\nThis is called immediate.")]
public class StartEvent : EventNode
{
	private FlowOutput start;

	private bool called;

	public override void OnGraphStarted()
	{
		if (!called)
		{
			called = true;
			start.Call(default(Flow));
		}
	}

	protected override void RegisterPorts()
	{
		start = AddFlowOutput("Once");
	}
}
