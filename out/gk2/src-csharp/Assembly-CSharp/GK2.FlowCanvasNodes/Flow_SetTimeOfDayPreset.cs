using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Set Time Of Day Preset", 0)]
[Category("Game/Environment")]
[Color("313c8f")]
public class Flow_SetTimeOfDayPreset : GKCustomFlowNode
{
	protected FlowInput @in;

	protected FlowOutput @out;

	private ValueInput<string> presetName;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), SetTimeOfDayPreset);
		@out = AddFlowOutput("out".CapitalizeFirst());
		presetName = AddValueInput<string>("presetName".CapitalizeFirst());
	}

	protected void SetTimeOfDayPreset(Flow flow)
	{
		EnvironmentEngine.Instance.SetTimeOfDayPreset(presetName.value);
		@out.Call(flow);
	}
}
