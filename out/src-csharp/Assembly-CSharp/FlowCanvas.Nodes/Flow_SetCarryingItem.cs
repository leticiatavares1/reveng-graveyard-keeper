using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Set Carrying Item", 0)]
[Icon("Cube", false, "")]
[Category("Game Actions")]
[Description("If Character is null, then Player")]
public class Flow_SetCarryingItem : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> in_wgo = AddValueInput<WorldGameObject>("WGO");
		ValueInput<string> in_item_id = AddValueInput<string>("Item ID");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			BaseCharacterComponent baseCharacterComponent = in_wgo.value?.components?.character;
			if (baseCharacterComponent == null)
			{
				Debug.LogError("Flow_SetCarryingItem error: character is null!");
				flow_out.Call(f);
			}
			else
			{
				baseCharacterComponent.SetCarryingItem(new Item(in_item_id.value));
				flow_out.Call(f);
			}
		});
	}
}
