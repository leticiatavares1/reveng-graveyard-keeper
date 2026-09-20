using DLCRefugees;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Set Camp Music Alarich", 0)]
[Category("Game Actions")]
public class Flow_SetCampAlarichMusic : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<RefugeeCampMusicAlarich> music_type = AddValueInput<RefugeeCampMusicAlarich>("Music Type");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			RefugeesCampEngine.instance.SetCampMusicAlarich(music_type.value);
			flow_out.Call(f);
		});
	}
}
