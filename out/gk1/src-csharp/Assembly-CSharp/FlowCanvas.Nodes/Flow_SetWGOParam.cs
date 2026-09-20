using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Set WGO Param", 0)]
[Category("Game Actions")]
[Description("Set WGO Param")]
public class Flow_SetWGOParam : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> in_wgo = AddValueInput<WorldGameObject>("WGO");
		ValueInput<string> in_param_name = AddValueInput<string>("Param name");
		ValueInput<float> in_value = AddValueInput<float>("Value");
		WorldGameObject _wgo = null;
		FlowOutput flow_out = AddFlowOutput("Out");
		AddValueOutput("WGO", () => _wgo);
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject worldGameObject = (_wgo = WGOParamOrSelf(in_wgo));
			if (worldGameObject != null && !string.IsNullOrEmpty(in_param_name.value))
			{
				worldGameObject.SetParam(in_param_name.value, in_value.value);
			}
			flow_out.Call(f);
		});
	}
}
