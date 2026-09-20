using System.Linq;
using FlowCanvas;
using LazyBearTechnology;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Current Track Equals", 0)]
[Category("Game/Sound")]
[Color("f5da42")]
public class Flow_CurrentTrackEquals : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput yes;

	private FlowOutput no;

	private ValueInput<string> playlistId;

	private ValueInput<string> trackId;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), delegate(Flow flow)
		{
			Track track = LazyAudio.GetPlaylistControllers().FirstOrDefault((PlaylistController c) => c.Id == playlistId.value)?.LastTrack;
			if (track == null || string.IsNullOrEmpty(track.id) || track.id != trackId.value)
			{
				no.Call(flow);
			}
			else
			{
				yes.Call(flow);
			}
		});
		yes = AddFlowOutput("yes".CapitalizeFirst());
		no = AddFlowOutput("no".CapitalizeFirst());
		playlistId = AddValueInput<string>("playlistId".CapitalizeFirst());
		trackId = AddValueInput<string>("trackId".CapitalizeFirst());
	}
}
