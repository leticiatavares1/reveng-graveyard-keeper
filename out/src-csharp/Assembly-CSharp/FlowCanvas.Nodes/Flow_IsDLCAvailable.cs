using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Color("20c1ae")]
[Description("Returns bool")]
[Category("Game Actions")]
[Name("Is DLC Available", 0)]
public class Flow_IsDLCAvailable : MyFlowNode
{
	protected override void RegisterPorts()
	{
		bool is_available = false;
		AddValueOutput("Is Available", () => is_available);
		ValueInput<DLCEngine.DLCVersion> dlc_version = AddValueInput<DLCEngine.DLCVersion>("DLC Version");
		FlowOutput flow_out = AddFlowOutput("Out");
		FlowOutput flow_yes = AddFlowOutput("True");
		FlowOutput flow_no = AddFlowOutput("False");
		AddFlowInput("In", delegate(Flow f)
		{
			is_available = DLCEngine.IsDLCAvailable(dlc_version.value);
			if (is_available)
			{
				flow_yes.Call(f);
			}
			else
			{
				flow_no.Call(f);
			}
			flow_out.Call(f);
		});
	}
}
