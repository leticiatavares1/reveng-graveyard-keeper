using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Condition: Float value", 0)]
public class Flow_ConditionFloatValue : MyFlowNode
{
	protected override void RegisterPorts()
	{
		float _v = 0f;
		ValueInput<bool> par_condition = AddValueInput<bool>("condition");
		ValueInput<float> par_value_true = AddValueInput<float>("if true");
		ValueInput<float> par_value_false = AddValueInput<float>("if false");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddValueOutput("result", () => _v);
		AddFlowInput("In", delegate(Flow f)
		{
			_v = (par_condition.value ? par_value_true.value : par_value_false.value);
			flow_out.Call(f);
		});
	}
}
