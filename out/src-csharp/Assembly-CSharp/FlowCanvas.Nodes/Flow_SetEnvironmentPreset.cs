using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Description("Set Environment Preset By Its Name")]
[Category("Game Actions")]
[Name("Set Environment Preset", 0)]
public class Flow_SetEnvironmentPreset : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<string> in_preset = AddValueInput<string>("Preset name");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			string value = in_preset.value;
			MainGame.me.save.SetEnvironmentPreset(value);
			flow_out.Call(f);
		});
	}
}
