using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Open Last Pray Report", 0)]
[Category("Game Actions")]
public class Flow_OpenLastPrayReport : MyFlowNode
{
	protected override void RegisterPorts()
	{
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			GUIElements.me.pray_report.Open(PrayLogics.last_pray_result);
			flow_out.Call(f);
		});
	}
}
