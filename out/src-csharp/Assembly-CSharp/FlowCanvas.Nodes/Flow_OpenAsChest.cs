using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Open As Chest", 0)]
[Category("Game Actions")]
public class Flow_OpenAsChest : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> in_wgo = AddValueInput<WorldGameObject>("WGO");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			if (in_wgo.value != null)
			{
				GUIElements.me.chest.Open(in_wgo.value);
			}
			else
			{
				Debug.LogError("Can not open WGO as chest: WGO is null!");
			}
			flow_out.Call(f);
		});
	}
}
