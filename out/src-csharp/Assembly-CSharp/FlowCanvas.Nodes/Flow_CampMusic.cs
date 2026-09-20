using DLCRefugees;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Set Camp Music", 0)]
[Category("Game Actions")]
public class Flow_CampMusic : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<RefugeeCampMusic> music_type = AddValueInput<RefugeeCampMusic>("Music Type");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			RefugeesCampEngine.instance.SetCampMusic(music_type.value);
			flow_out.Call(f);
		});
	}
}
