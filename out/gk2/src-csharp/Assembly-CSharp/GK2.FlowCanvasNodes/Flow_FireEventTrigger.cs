using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Fire Event Trigger", 0)]
[Category("Game/Script")]
[Color("ff5c5c")]
[Icon("FS", false, "")]
public class Flow_FireEventTrigger : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<GlobalEventsSystem.Event.Type> eventType;

	private ValueInput<string> eventId;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), FireEvent);
		@out = AddFlowOutput("out".CapitalizeFirst());
		eventType = AddValueInput<GlobalEventsSystem.Event.Type>("eventType");
		eventId = AddValueInput<string>("eventId");
	}

	private void FireEvent(Flow flow)
	{
		GlobalEventsSystem.FireTrigger(eventType.value, eventId.value ?? string.Empty);
		@out.Call(flow);
	}
}
