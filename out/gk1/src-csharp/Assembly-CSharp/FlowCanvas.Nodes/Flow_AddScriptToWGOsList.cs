using System.Collections.Generic;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Description("Add Script To WGOs List")]
[Category("Game Actions")]
[Name("Add Script To WGOs List", 0)]
public class Flow_AddScriptToWGOsList : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<List<WorldGameObject>> in_wgos = AddValueInput<List<WorldGameObject>>("WGOs List");
		ValueInput<string> in_script = AddValueInput<string>("Script");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			if (in_wgos.value == null)
			{
				Debug.LogError("WGO is null");
			}
			else
			{
				foreach (WorldGameObject item in in_wgos.value)
				{
					if (!(item == null))
					{
						item.AttachFlowScript(in_script.value);
					}
				}
				flow_out.Call(f);
			}
		});
	}
}
