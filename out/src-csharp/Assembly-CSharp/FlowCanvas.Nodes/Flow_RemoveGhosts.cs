using System.Collections.Generic;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Description("Remove Ghosts from Graveyard and decay graves")]
[Icon("CubeArrowCube", false, "")]
[Name("Remove Ghosts from Graveyard", 0)]
[Category("Game Actions")]
public class Flow_RemoveGhosts : MyFlowNode
{
	protected override void RegisterPorts()
	{
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			List<WorldGameObject> worldGameObjectsByObjId = WorldMap.GetWorldGameObjectsByObjId("ghost");
			if (worldGameObjectsByObjId == null)
			{
				Debug.LogError("Something wrong!");
			}
			else
			{
				int count = worldGameObjectsByObjId.Count;
				if (count > 0)
				{
					WorldZone zoneByID = WorldZone.GetZoneByID("graveyard");
					if (zoneByID == null)
					{
						Debug.LogError("Zone graveyard not found");
					}
					else if (zoneByID.GetZoneWGOs() == null)
					{
						Debug.LogError("WGOs list of zone graveyard is null!", zoneByID);
					}
					else
					{
						foreach (WorldGameObject zoneWGO in zoneByID.GetZoneWGOs())
						{
							if (!(zoneWGO == null) && !(zoneWGO.obj_id != "grave_ground"))
							{
								zoneWGO.data.AddToParams("decay", count);
							}
						}
						while (worldGameObjectsByObjId.Count > 0)
						{
							worldGameObjectsByObjId[0].DestroyMe();
							worldGameObjectsByObjId.RemoveAt(0);
						}
					}
				}
			}
			flow_out.Call(f);
		});
	}
}
