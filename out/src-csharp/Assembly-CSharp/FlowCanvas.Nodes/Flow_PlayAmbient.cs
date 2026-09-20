using DarkTonic.MasterAudio;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Description("Play Ambient by name")]
[Name("Play Ambient", 0)]
[Category("Game Actions")]
public class Flow_PlayAmbient : MyFlowNode
{
	public override string name
	{
		get
		{
			if (!GetInputValuePort("sound").isConnected && GetInputValuePort("sound").isDefaultValue)
			{
				return "Stop Ambient";
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
		ValueInput<string> in_sound = AddValueInput<string>("sound");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			if (string.IsNullOrEmpty(in_sound.value))
			{
				MasterAudio.StopPlaylist("ambient");
			}
			else
			{
				MasterAudio.TriggerPlaylistClip("ambient", in_sound.value);
			}
			flow_out.Call(f);
		});
	}
}
