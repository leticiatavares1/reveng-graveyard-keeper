using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Description("Set durability for items in grave WGO inventory (only GraveStone and GraveFence)")]
[Name("Set durability for items in Grave", 0)]
public class Flow_SetDurabilityFoItems : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> in_wgo = AddValueInput<WorldGameObject>("Grave WGO");
		ValueInput<float> in_dur = AddValueInput<float>("durability");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject worldGameObject = WGOParamOrSelf(in_wgo);
			if (worldGameObject == null)
			{
				Debug.LogError("Can not set items durability: WGO is null!");
				flow_out.Call(f);
			}
			else if (worldGameObject.obj_def == null)
			{
				Debug.LogError("Can not set items durability: WGO definition is null!");
				flow_out.Call(f);
			}
			else
			{
				foreach (Item item in worldGameObject.data.inventory)
				{
					if (item != null && (item.definition.type == ItemDefinition.ItemType.GraveStone || item.definition.type == ItemDefinition.ItemType.GraveFence))
					{
						item.durability = in_dur.value;
					}
				}
				worldGameObject.Redraw();
				flow_out.Call(f);
			}
		});
	}
}
