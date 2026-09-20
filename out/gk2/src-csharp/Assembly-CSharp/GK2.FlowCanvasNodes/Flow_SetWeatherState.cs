using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Set Weather State", 0)]
[Category("Game/Weather")]
[Color("313c8f")]
public class Flow_SetWeatherState : GKCustomFlowNode
{
	[GatherPortsCallback]
	public bool reset;

	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<string> stateName;

	public override string name => (reset ? "Reset" : "Set") + " Weather State";

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), ChangeGameRes);
		@out = AddFlowOutput("out".CapitalizeFirst());
		if (!reset)
		{
			stateName = AddValueInput<string>("stateName");
		}
	}

	private void ChangeGameRes(Flow flow)
	{
		if (!reset)
		{
			if (!string.IsNullOrEmpty(stateName.value))
			{
				WeatherSystem.Instance.SetWeatherState(stateName.value, force: true);
			}
		}
		else
		{
			WeatherSystem.Instance.ResetWeatherState();
		}
		@out.Call(flow);
	}
}
