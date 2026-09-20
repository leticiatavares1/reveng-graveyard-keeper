using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Color("4155be")]
[Name("Is Player Enable", 0)]
[Category("Game Actions")]
public class Flow_IsPlayerEnable : MyFlowNode
{
	private bool control_out_value;

	protected override void RegisterPorts()
	{
		FlowOutput flow_yes = AddFlowOutput("Yes");
		FlowOutput flow_no = AddFlowOutput("No");
		AddValueOutput("is enable?", () => control_out_value);
		AddFlowInput("In", delegate(Flow f)
		{
			control_out_value = GS.IsPlayerEnable();
			if (control_out_value)
			{
				flow_yes.Call(f);
			}
			else
			{
				flow_no.Call(f);
			}
		});
	}
}
