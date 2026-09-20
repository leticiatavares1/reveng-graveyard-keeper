using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Get Item's Param", 0)]
[Category("Game Actions")]
public class Flow_GetItemParam : MyFlowNode
{
	private float out_param_value;

	protected override void RegisterPorts()
	{
		ValueInput<Item> in_item = AddValueInput<Item>("Item");
		ValueInput<string> in_param = AddValueInput<string>("Param name");
		AddValueOutput("Param Value", () => out_param_value);
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			if (in_item.value == null || in_item.value.IsEmpty())
			{
				Debug.LogError("Item null or empty");
				flow_out.Call(f);
			}
			else
			{
				out_param_value = in_item.value.GetParam(in_param.value);
				flow_out.Call(f);
			}
		});
	}
}
