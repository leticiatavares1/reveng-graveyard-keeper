using System.Collections.Generic;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Description("Put Item To Zone WGOs")]
[Category("Game Actions")]
[Name("Put Item To Zone WGOs", 0)]
public class Flow_PutItemToZoneWGOs : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> in_wgo = AddValueInput<WorldGameObject>("WGO");
		ValueInput<Item> in_item = AddValueInput<Item>("item");
		ValueInput<bool> in_drop_tail = AddValueInput<bool>("drop tail");
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
				List<Item> list = new List<Item> { in_item.value };
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
					flow_out.Call(f);
				}
			}
		});
	}
}
