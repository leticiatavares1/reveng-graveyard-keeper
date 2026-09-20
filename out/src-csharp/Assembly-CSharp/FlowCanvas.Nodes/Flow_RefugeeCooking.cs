using DLCRefugees;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Refugee Cooking", 0)]
[Category("Game Actions")]
public class Flow_RefugeeCooking : MyFlowNode
{
	protected override void RegisterPorts()
	{
		FlowOutput flow_out = AddFlowOutput("Out");
		bool has_craft = false;
		float craft_time = 0f;
		ValueInput<WorldGameObject> wgo = AddValueInput<WorldGameObject>("WGO");
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject worldGameObject = WGOParamOrSelf(wgo);
			if (worldGameObject != null)
			{
				CraftComponent craft = worldGameObject.components.craft;
				has_craft = craft.is_crafting;
				if (!has_craft)
				{
					craft_time = RefugeesCampEngine.instance.StartCooking(worldGameObject);
				}
			}
			else
			{
				Debug.LogError("Null WGO for Flow_RefugeeCooking");
			}
			flow_out.Call(f);
		});
		AddValueOutput("Has craft", () => has_craft);
		AddValueOutput("Cooking Time", () => craft_time);
	}
}
