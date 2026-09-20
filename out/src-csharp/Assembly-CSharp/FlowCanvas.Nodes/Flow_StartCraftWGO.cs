using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Description("Start Craft WGO")]
[Name("Start Craft WGO", 0)]
public class Flow_StartCraftWGO : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> in_wgo = AddValueInput<WorldGameObject>("WGO");
		ValueInput<Item> in_item = AddValueInput<Item>("Item");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			if (in_wgo.value == null)
			{
				Debug.LogError("WGO is null!");
			}
			else
			{
				in_wgo.value.components.craft.Craft(GameBalance.me.GetData<CraftDefinition>("set_" + in_item.value.id));
				flow_out.Call(f);
			}
		});
	}
}
