using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Description("Try to set anchor to character")]
[Name("Try to set anchor to character", 0)]
[Category("Game Actions")]
[Icon("CubePlus", false, "")]
public class Flow_TrySetAnchorToChar : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> in_wgo = AddValueInput<WorldGameObject>("WGO");
		ValueInput<GameObject> in_anchor = AddValueInput<GameObject>("Anchor");
		FlowOutput flow_correct = AddFlowOutput("Correctly Added");
		FlowOutput flow_wrong = AddFlowOutput("Mistake Happen");
		AddFlowInput("In", delegate(Flow f)
		{
			if (in_anchor.value == null)
			{
				Debug.Log("Anchor is null");
				flow_wrong.Call(f);
			}
			else if (in_wgo.value != null)
			{
				WorldGameObject value = in_wgo.value;
				if (value != null && value.components != null && value.components.character.enabled)
				{
					value.components.character.SetAnchor(in_anchor.value);
					flow_correct.Call(f);
				}
				else
				{
					Debug.Log("WGO Character is null");
					flow_wrong.Call(f);
				}
			}
			else
			{
				Debug.Log("WGO is null");
				flow_wrong.Call(f);
			}
		});
	}
}
