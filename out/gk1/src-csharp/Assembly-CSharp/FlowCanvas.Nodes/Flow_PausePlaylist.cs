using DarkTonic.MasterAudio;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Description("Enable(Disable) playlist by its name. If clip name set then it will be triggered in specified playlist")]
[Category("Game Actions")]
[Name("Pause Playlist", 0)]
public class Flow_PausePlaylist : MyFlowNode
{
	public override string name
	{
		get
		{
			if (!GetInputValuePort<bool>("Pause?").value)
			{
				return "Unpause Playlist";
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
		FlowOutput flow_out = AddFlowOutput("Out");
		ValueInput<bool> in_pause = AddValueInput<bool>("Pause?");
		ValueInput<string> in_playlist_name = AddValueInput<string>("Playlist name");
		AddFlowInput("In", delegate(Flow f)
		{
			string value = in_playlist_name.value;
			bool value2 = in_pause.value;
			if (!string.IsNullOrEmpty(value))
			{
				if (value2)
				{
					MasterAudio.PausePlaylist(value);
				}
				else
				{
					MasterAudio.UnpausePlaylist(value);
				}
			}
			flow_out.Call(f);
		});
	}
}
