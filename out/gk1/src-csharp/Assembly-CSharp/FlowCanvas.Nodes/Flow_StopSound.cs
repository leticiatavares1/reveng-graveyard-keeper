using DarkTonic.MasterAudio;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Stop Sound", 0)]
[Description("Stop sound by name")]
[Category("Game Actions")]
public class Flow_StopSound : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<string> in_sound = AddValueInput<string>("sound");
		ValueInput<bool> in_stop_with_fade = AddValueInput<bool>("stop with fade?");
		ValueInput<float> in_fade_value = AddValueInput<float>("fade out time");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			bool value = in_stop_with_fade.value;
			float value2 = in_fade_value.value;
			if (!value)
			{
				MasterAudio.StopAllOfSound(in_sound.value);
			}
			else
			{
				MasterAudio.FadeOutAllOfSound(in_sound.value, value2);
			}
			flow_out.Call(f);
		});
	}
}
