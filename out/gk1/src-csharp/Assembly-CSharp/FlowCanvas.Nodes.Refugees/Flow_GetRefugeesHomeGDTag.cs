using DLCRefugees;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes.Refugees;

[Category("Game Actions/Refugees")]
[Name("Get Refugee's Home GD Tag", 0)]
public class Flow_GetRefugeesHomeGDTag : MyFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<WorldGameObject> refuge_wgo_in;

	private ValueOutput<string> home_gd_tag_out;

	private string home_gd_tag_out_value;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("In", delegate(Flow flow)
		{
			if (refuge_wgo_in.value != null)
			{
				home_gd_tag_out_value = RefugeesCampEngine.instance.GetRefugeesHomeGDTag(refuge_wgo_in.value);
			}
			else
			{
				Debug.LogError("Refugee WGO is null");
			}
			@out.Call(flow);
		});
		@out = AddFlowOutput("Out");
		refuge_wgo_in = AddValueInput<WorldGameObject>("Refugee WGO");
		home_gd_tag_out = AddValueOutput("Home GD Tag", () => home_gd_tag_out_value);
	}
}
