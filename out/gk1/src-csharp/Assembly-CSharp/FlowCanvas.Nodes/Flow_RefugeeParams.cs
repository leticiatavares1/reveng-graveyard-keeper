using System.Collections.Generic;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Set Refugee Params", 0)]
[Category("Game Actions")]
[Description("set all necessary refugee params")]
public class Flow_RefugeeParams : MyFlowNode
{
	public List<string> custom_param_list = new List<string>();

	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> in_wgo = AddValueInput<WorldGameObject>("Refugee WGO");
		WorldGameObject _wgo = null;
		FlowOutput flow_out = AddFlowOutput("Out");
		AddValueOutput("Refugee WGO", () => _wgo);
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject worldGameObject = (_wgo = WGOParamOrSelf(in_wgo));
			if (worldGameObject != null)
			{
				foreach (string item in custom_param_list)
				{
					worldGameObject.SetParam(item, 2f);
				}
				worldGameObject.SetParam("on_the_way_now", 1f);
				worldGameObject.SetParam("refugee_unrollable", 0f);
				worldGameObject.SetParam("first_roll", 0f);
				worldGameObject.SetParam("roll_percent", 0.7f);
			}
			flow_out.Call(f);
		});
	}
}
