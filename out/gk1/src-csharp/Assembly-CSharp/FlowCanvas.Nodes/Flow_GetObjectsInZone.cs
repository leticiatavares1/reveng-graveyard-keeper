using System.Collections.Generic;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Get Objects In Zone", 0)]
[Category("Game Actions")]
public class Flow_GetObjectsInZone : MyFlowNode
{
	protected override void RegisterPorts()
	{
		List<WorldGameObject> result = new List<WorldGameObject>();
		ValueInput<string> in_zone_id = AddValueInput<string>("zone_id");
		ValueInput<string> in_obj_id = AddValueInput<string>("obj_id");
		AddValueOutput("WGOs List", () => result);
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			result = new List<WorldGameObject>();
			if (string.IsNullOrEmpty(in_zone_id.value))
			{
				Debug.LogError("Zone ID can not be null!");
				flow_out.Call(f);
			}
			else
			{
				WorldZone zoneByID = WorldZone.GetZoneByID(in_zone_id.value);
				if (zoneByID == null)
				{
					Debug.LogError("Not found zone with ID \"" + in_zone_id.value + "\"");
					flow_out.Call(f);
				}
				else
				{
					if (string.IsNullOrEmpty(in_obj_id.value))
					{
						result = zoneByID.GetZoneWGOs();
					}
					else
					{
						string value = in_obj_id.value;
						foreach (WorldGameObject zoneWGO in zoneByID.GetZoneWGOs())
						{
							if (!(zoneWGO == null) && zoneWGO.obj_id == value)
							{
								result.Add(zoneWGO);
							}
						}
					}
					Debug.Log("Total found WGOs: " + result.Count);
					flow_out.Call(f);
				}
			}
		});
	}
}
