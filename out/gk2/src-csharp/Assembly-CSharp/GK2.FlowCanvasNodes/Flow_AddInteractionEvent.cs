using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Add Interaction Event", 0)]
[Category("Game/Wgo")]
[Color("f47dff")]
[Icon("Dialogue", false, "")]
public class Flow_AddInteractionEvent : GKCustomFlowNodeWithWgoData
{
	[GatherPortsCallback]
	public bool remove;

	[GatherPortsCallback]
	public bool isFake;

	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<string> eventId;

	public override string name => (remove ? "Remove" : "Add") + " Interaction Event";

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), AddEvent);
		@out = AddFlowOutput("out".CapitalizeFirst());
		base.RegisterPorts();
		eventId = AddValueInput<string>("eventId");
	}

	private void AddEvent(Flow flow)
	{
		if (!remove)
		{
			GetWgoData()?.AddInteractionEvent(eventId.value, isFake);
		}
		else
		{
			GetWgoData()?.RemoveInteractionEvent(eventId.value);
		}
		@out.Call(flow);
	}
}
