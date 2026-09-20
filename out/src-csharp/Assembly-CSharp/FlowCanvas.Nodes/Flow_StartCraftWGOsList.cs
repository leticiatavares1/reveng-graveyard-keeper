using System.Collections.Generic;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Start Craft WGOs List", 0)]
[Category("Game Actions")]
[Description("Start Craft WGOs List")]
public class Flow_StartCraftWGOsList : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<List<WorldGameObject>> in_wgo = AddValueInput<List<WorldGameObject>>("WGOs List");
		ValueInput<Item> in_item = AddValueInput<Item>("Item");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			List<WorldGameObject> value = in_wgo.value;
			if (value == null)
			{
				Debug.LogError("WGOs List is null!");
			}
			else
			{
				foreach (WorldGameObject item in value)
				{
					if (!(item == null))
					{
						item.components.craft.Craft(GameBalance.me.GetData<CraftDefinition>("set_" + in_item.value.id));
					}
				}
				flow_out.Call(f);
			}
		});
	}
}
