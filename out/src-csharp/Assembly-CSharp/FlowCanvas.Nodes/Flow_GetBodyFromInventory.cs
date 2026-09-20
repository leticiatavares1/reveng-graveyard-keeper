using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Get body from inventory", 0)]
[Category("Game Actions")]
[Description("True if wgo has item with this ID")]
public class Flow_GetBodyFromInventory : MyFlowNode
{
	public Item body_item;

	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> in_wgo = AddValueInput<WorldGameObject>("WGO");
		AddValueOutput("body_item", () => body_item);
		FlowOutput flow_yes = AddFlowOutput("Yes");
		FlowOutput flow_no = AddFlowOutput("No");
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject worldGameObject = WGOParamOrSelf(in_wgo);
			if (worldGameObject == null)
			{
				Debug.LogError("WGO is null!");
				flow_no.Call(f);
			}
			else if (worldGameObject.data?.inventory == null)
			{
				Debug.LogError("WGO inventory is null!");
				flow_no.Call(f);
			}
			else if (worldGameObject.data.inventory.Count == 0)
			{
				flow_no.Call(f);
			}
			else
			{
				foreach (Item item in worldGameObject.data.inventory)
				{
					if (!item.IsEmpty() && item.definition.type == ItemDefinition.ItemType.Body)
					{
						body_item = item;
						flow_yes.Call(f);
						return;
					}
				}
				flow_no.Call(f);
			}
		});
	}
}
