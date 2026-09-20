using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Lowpass SFX to low", 0)]
public class Flow_LowpassSFX : MyFlowNode
{
	public override string name
	{
		get
		{
			if (GetInputValuePort<bool>("To Normal Frequency?").value)
			{
				return "Lowpass SFX to normal";
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
		ValueInput<bool> normal_frequency = AddValueInput<bool>("To Normal Frequency?");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			SmartAudioEngine.me.mixer.SetFloat("sfx_cutoff", normal_frequency.value ? 22000 : 5000);
			flow_out.Call(f);
		});
	}
}
