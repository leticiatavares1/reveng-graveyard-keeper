using System.Collections.Generic;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Icon("CubeArrowCube", false, "")]
[Name("Try Spawn Ghosts at Graveyard", 0)]
[Category("Game Actions")]
[Description("Try Spawn Ghosts at Graveyard")]
public class Flow_TrySpawnGhosts : MyFlowNode
{
	protected override void RegisterPorts()
	{
		FlowOutput flow_out_correct = AddFlowOutput("Correct");
		FlowOutput flow_out_wrong = AddFlowOutput("Wrong");
		AddFlowInput("In", delegate(Flow f)
		{
			WorldZone zoneByID = WorldZone.GetZoneByID("graveyard");
			if (zoneByID == null)
			{
				flow_out_wrong.Call(f);
			}
			else
			{
				List<WorldGameObject> darkGraves = zoneByID.GetDarkGraves();
				if (darkGraves == null)
				{
					flow_out_wrong.Call(f);
				}
				else
				{
					foreach (WorldGameObject item in darkGraves)
					{
						Vector3 position = item.transform.position;
						item.custom_tag = "{" + position.x + ";" + position.y + "} + grave";
						WorldMap.SpawnWGO(item.transform.parent, "ghost", item.transform.position).components.character.SetAnchor(item);
						Debug.Log("Spawned ghost with anchor " + item.name, item);
					}
					flow_out_correct.Call(f);
				}
			}
		});
	}
}
