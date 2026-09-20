using DarkTonic.MasterAudio;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Fade Sound To Volume", 0)]
[Category("Game Actions")]
[Description("Fades sound to the volume(0..1)")]
public class Flow_FadeSoundToVolume : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<string> in_sound = AddValueInput<string>("sound");
		ValueInput<float> in_sound_volume = AddValueInput<float>("volume to fade");
		ValueInput<float> in_fade_value = AddValueInput<float>("fade time");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			MasterAudio.FadeSoundGroupToVolume(in_sound.value, in_sound_volume.value, in_fade_value.value);
			flow_out.Call(f);
		});
	}
}
