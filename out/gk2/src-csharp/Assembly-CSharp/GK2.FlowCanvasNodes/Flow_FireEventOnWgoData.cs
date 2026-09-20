using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Fire Event On WGO Data", 0)]
[Category("Game/Script")]
[Color("ff5c5c")]
[Icon("FS", false, "")]
public class Flow_FireEventOnWgoData : GKCustomFlowNodeWithWgoData
{
	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<string> eventName;

	private ValueInput<float> delayTime;

	protected override void RegisterPorts()
	{
		base.RegisterPorts();
		@in = AddFlowInput("in".CapitalizeFirst(), FireEvent);
		@out = AddFlowOutput("out".CapitalizeFirst());
		eventName = AddValueInput<string>("eventName");
		delayTime = AddValueInput<float>("delayTime");
	}

	private void FireEvent(Flow flow)
	{
		if (delayTime.value.More(0f))
		{
			GetWgoData().AddDelayedEvent(eventName.value, delayTime.value);
		}
		else
		{
			GetWgoData().FireEvent(eventName.value);
		}
		@out.Call(flow);
	}
}
