using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Add Script To WGO", 0)]
[Description("Add Script To WGO")]
[Category("Game Actions")]
public class Flow_AddScriptToWGO : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> in_wgo = AddValueInput<WorldGameObject>("WGO");
		ValueInput<string> in_script = AddValueInput<string>("Script");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			if (in_wgo.value == null)
			{
				Debug.LogError("WGO is null");
			}
			else
			{
				in_wgo.value.AttachFlowScript(in_script.value);
				flow_out.Call(f);
			}
		});
	}
}
