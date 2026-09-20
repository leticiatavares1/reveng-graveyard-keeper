using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Description("Add smthng to string")]
[Category("Game Actions")]
[Name("Add to string", 0)]
public class Flow_AddToString : MyFlowNode
{
	private string sum = "";

	protected override void RegisterPorts()
	{
		ValueInput<string> in_str_1 = AddValueInput<string>("String 1");
		ValueInput<string> in_str_2 = AddValueInput<string>("String 2");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddValueOutput("Sum", () => sum);
		AddFlowInput("In", delegate(Flow f)
		{
			sum = in_str_1.value + in_str_2.value;
			flow_out.Call(f);
		});
	}
}
