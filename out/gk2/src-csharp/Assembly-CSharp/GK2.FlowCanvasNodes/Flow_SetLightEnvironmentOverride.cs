using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Set Light Environment Override", 0)]
[Category("Game/Environment")]
[Color("313c8f")]
public class Flow_SetLightEnvironmentOverride : GKCustomFlowNode
{
	[GatherPortsCallback]
	public bool reset;

	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<string> presetName;

	public override string name => (reset ? "Reset" : "Set") + " Light Environment Override";

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), ChangePreset);
		@out = AddFlowOutput("out".CapitalizeFirst());
		if (!reset)
		{
			presetName = AddValueInput<string>("presetName");
		}
	}

	private void ChangePreset(Flow flow)
	{
		if (!reset)
		{
			if (!string.IsNullOrEmpty(presetName.value))
			{
				EnvironmentEngine.Instance.ApplyOverridePreset(presetName.value);
			}
		}
		else
		{
			EnvironmentEngine.Instance.ApplyOverridePreset((LightEnvironmentPreset)null, 0f);
		}
		@out.Call(flow);
	}
}
