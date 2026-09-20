using DarkTonic.MasterAudio;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Description("Play sound by name")]
[Category("Game Actions")]
[Name("Play Sound", 0)]
public class Flow_PlaySound : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<string> in_sound = AddValueInput<string>("sound");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			MasterAudio.PlaySound(in_sound.value);
			flow_out.Call(f);
		});
	}
}
