using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Color("4155be")]
[Category("Game Actions")]
[Name("Affect Cinematic", 0)]
public class Flow_AffectCinematic : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<bool> par_cinematic = AddValueInput<bool>("Enable cinematic?");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			GS.AffectCinematic(par_cinematic.value);
			flow_out.Call(f);
		});
	}
}
