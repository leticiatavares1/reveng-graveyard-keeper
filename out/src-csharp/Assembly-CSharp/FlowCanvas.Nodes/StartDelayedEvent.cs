using System.Collections;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("On Start Delayed", 9)]
[Description("Called only once and the first time the Graph is enabled.\nThis is called 1 frame after all Awake events are called.")]
[Category("Events/Graph")]
public class StartDelayedEvent : EventNode
{
	private FlowOutput start;

	private bool called;

	public override void OnGraphStarted()
	{
		if (!called)
		{
			called = true;
			StartCoroutine(DelayCall());
		}
	}

	private IEnumerator DelayCall()
	{
		yield return null;
		start.Call(default(Flow));
	}

	protected override void RegisterPorts()
	{
		start = AddFlowOutput("Once");
	}
}
