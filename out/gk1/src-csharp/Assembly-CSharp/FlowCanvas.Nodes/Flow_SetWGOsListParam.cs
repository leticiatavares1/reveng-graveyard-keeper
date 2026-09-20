using System.Collections.Generic;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Description("Set WGOs List Param")]
[Category("Game Actions")]
[Name("Set WGOs List Param", 0)]
public class Flow_SetWGOsListParam : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<List<WorldGameObject>> in_wgos = AddValueInput<List<WorldGameObject>>("WGOs List");
		ValueInput<string> in_param_name = AddValueInput<string>("Param name");
		ValueInput<float> in_value = AddValueInput<float>("Value");
		ValueInput<bool> in_need_redraw = AddValueInput<bool>("Need redraw");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			List<WorldGameObject> value = in_wgos.value;
			if (value == null)
			{
				Debug.LogError("WGOs list is null!");
			}
			else
			{
				foreach (WorldGameObject item in value)
				{
					if (item != null && !string.IsNullOrEmpty(in_param_name.value))
					{
						item.SetParam(in_param_name.value, in_value.value);
						if (in_need_redraw.value)
						{
							item.Redraw();
						}
					}
				}
				flow_out.Call(f);
			}
		});
	}
}
