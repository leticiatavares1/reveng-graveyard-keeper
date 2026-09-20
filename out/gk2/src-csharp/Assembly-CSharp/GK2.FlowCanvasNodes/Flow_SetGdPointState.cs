using FlowCanvas;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Change GD Point State", 0)]
[Category("Game/Environment")]
[Color("313c8f")]
public class Flow_SetGdPointState : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<GDPointData> gdPoint;

	private ValueInput<bool> enabled;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in", SetGdPointState);
		@out = AddFlowOutput("out");
		gdPoint = AddValueInput<GDPointData>("gdPoint");
		enabled = AddValueInput<bool>("enabled");
	}

	private void SetGdPointState(Flow flow)
	{
		gdPoint.value.Enabled = enabled.value;
		@out.Call(flow);
	}
}
