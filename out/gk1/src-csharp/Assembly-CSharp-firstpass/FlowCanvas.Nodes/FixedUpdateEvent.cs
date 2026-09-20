using ParadoxNotion.Design;
using ParadoxNotion.Services;

namespace FlowCanvas.Nodes;

[Category("Events/Graph")]
[Name("On Fixed Update", 4)]
[Description("Called every fixed framerate frame, which should be used when dealing with Physics")]
public class FixedUpdateEvent : EventNode
{
	private FlowOutput fixedUpdate;

	protected override void RegisterPorts()
	{
		fixedUpdate = AddFlowOutput("Out");
	}

	public override void OnGraphStarted()
	{
		MonoManager.current.onFixedUpdate += FixedUpdate;
	}

	public override void OnGraphStoped()
	{
		MonoManager.current.onFixedUpdate -= FixedUpdate;
	}

	private void FixedUpdate()
	{
		fixedUpdate.Call(default(Flow));
	}
}
