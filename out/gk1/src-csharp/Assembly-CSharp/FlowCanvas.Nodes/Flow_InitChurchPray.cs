using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Init Church Pray", 0)]
[Category("Game Actions")]
public class Flow_InitChurchPray : MyFlowNode
{
	protected override void RegisterPorts()
	{
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			PrayLogics.InitPrayPlacesList();
			flow_out.Call(f);
		});
	}
}
