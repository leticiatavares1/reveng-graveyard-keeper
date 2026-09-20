using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Description("Disable weather. Use only when time is disabled. Don't forget to enable weather back.")]
[Name("Enable Weather", 0)]
[Color("000000")]
[Category("Game Actions")]
public class Flow_DisableWeather : MyFlowNode
{
	public override string name
	{
		get
		{
			if (!GetInputValuePort<bool>("Enable weather").value)
			{
				return "Disable Weather";
			}
			return base.name;
		}
		set
		{
			base.name = value;
		}
	}

	protected override void RegisterPorts()
	{
		ValueInput<bool> par_enable = AddValueInput<bool>("Enable weather");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			EnvironmentEngine.me.weather_is_forced = !par_enable.value;
			flow_out.Call(f);
		});
	}
}
