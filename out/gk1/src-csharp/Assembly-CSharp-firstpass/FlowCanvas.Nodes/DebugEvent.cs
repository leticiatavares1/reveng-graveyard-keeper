using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Events/Other")]
[Description("Use to debug send a Flow Signal in PlayMode Only")]
public class DebugEvent : EventNode, IUpdatable
{
	protected override void RegisterPorts()
	{
		AddFlowOutput("Out");
	}

	public void Update()
	{
	}
}
