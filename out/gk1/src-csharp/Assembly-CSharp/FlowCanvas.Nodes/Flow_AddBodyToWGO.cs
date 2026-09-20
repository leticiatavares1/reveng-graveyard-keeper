using System;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Description("Add Item To WGO")]
[Category("Game Actions")]
[Name("Add Body To WGO", 0)]
public class Flow_AddBodyToWGO : MyFlowNode
{
	protected override void RegisterPorts()
	{
		AddValueInput<WorldGameObject>("WGO");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			Debug.LogException(new Exception("Deprecated method. Don't use it!"));
			flow_out.Call(f);
		});
	}
}
