using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Add WGO Param", 0)]
[Category("Game Actions")]
[Description("Add WGO Param")]
public class Flow_AddWGOParam : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> in_wgo = AddValueInput<WorldGameObject>("WGO");
		ValueInput<string> in_param_name = AddValueInput<string>("Param name");
		ValueInput<float> in_value = AddValueInput<float>("Value");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject worldGameObject = WGOParamOrSelf(in_wgo);
			if (worldGameObject != null && !string.IsNullOrEmpty(in_param_name.value))
			{
				worldGameObject.AddToParams(in_param_name.value, in_value.value);
			}
			flow_out.Call(f);
		});
	}
}
