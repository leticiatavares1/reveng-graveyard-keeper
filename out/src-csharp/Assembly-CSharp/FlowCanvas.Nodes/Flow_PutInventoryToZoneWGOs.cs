using System.Collections.Generic;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Put Inventory To Zone WGOs", 0)]
[Category("Game Actions")]
[Description("Put Inventory To Zone WGOs")]
public class Flow_PutInventoryToZoneWGOs : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> in_wgo = AddValueInput<WorldGameObject>("WGO");
		ValueInput<bool> in_drop_tail = AddValueInput<bool>("drop tail");
		ValueInput<bool> in_clear_inventory = AddValueInput<bool>("clear inventory");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			if (in_wgo.value == null)
			{
				Debug.LogError("WGO is null");
				flow_out.Call(f);
			}
			else
			{
				List<Item> list = in_wgo.value?.data?.inventory;
				if (list == null)
				{
					Debug.LogError("Flow_PutInventoryToZoneWGOs error: Items list is null!");
					flow_out.Call(f);
				}
				else if (list.Count == 0)
				{
					flow_out.Call(f);
				}
				else
				{
					WorldZone myWorldZone = in_wgo.value.GetMyWorldZone();
					if (myWorldZone != null)
					{
						myWorldZone.PutToAllPossibleInventoriesSmart(list, out var cant_insert);
						if (in_drop_tail.value && cant_insert != null && cant_insert.Count > 0)
						{
							in_wgo.value.DropItems(cant_insert);
						}
					}
					if (in_clear_inventory.value)
					{
						in_wgo.value.data.inventory = new List<Item>();
					}
					flow_out.Call(f);
				}
			}
		});
	}
}
