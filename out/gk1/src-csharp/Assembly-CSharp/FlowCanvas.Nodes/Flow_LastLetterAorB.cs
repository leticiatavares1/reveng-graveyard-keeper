using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Description("A -> inside, B -> outside")]
[Category("Flow Controllers")]
[Name("Last letter A or B", 0)]
public class Flow_LastLetterAorB : MyFlowNode
{
	private bool last_letter_is_A;

	private string changed_str = "";

	protected override void RegisterPorts()
	{
		ValueInput<string> in_custom_tag = AddValueInput<string>("Custom Tag");
		AddValueOutput("Last Letter Is A Bool", () => last_letter_is_A);
		AddValueOutput("Changed out string", () => changed_str);
		FlowOutput flow_out = AddFlowOutput("Out");
		FlowOutput out_last_letter_is_A = AddFlowOutput("Last Letter Is A");
		FlowOutput out_last_letter_is_B = AddFlowOutput("Last Letter Is B");
		AddFlowInput("In", delegate(Flow f)
		{
			changed_str = in_custom_tag.value.Trim('_');
			last_letter_is_A = changed_str[changed_str.Length - 1] == 'a' || changed_str[changed_str.Length - 1] == 'A';
			if (last_letter_is_A)
			{
				out_last_letter_is_A.Call(f);
			}
			else
			{
				out_last_letter_is_B.Call(f);
			}
			flow_out.Call(f);
		});
	}
}
