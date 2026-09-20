using System.Collections.Generic;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Spawn NPC From Stock", 0)]
public class Flow_SpawnNPCFromStock : MyFlowNode
{
	private WorldGameObject out_o;

	protected override void RegisterPorts()
	{
		ValueInput<string> in_wgo_name = AddValueInput<string>("WGO ID", "obj_id");
		ValueInput<string> in_wgo_tag = AddValueInput<string>("WGO tag", "custom_tag");
		ValueInput<string> in_gd_point_tag = AddValueInput<string>("GD Point Tag", "gd_point");
		AddValueOutput("WGO", () => out_o);
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject worldGameObject = null;
			if (!string.IsNullOrEmpty(in_wgo_tag.value))
			{
				worldGameObject = WorldMap.GetWorldGameObjectByCustomTag(in_wgo_tag.value);
			}
			else
			{
				if (string.IsNullOrEmpty(in_wgo_name.value))
				{
					Debug.LogError("Tag and name are null!");
					return;
				}
				List<WorldGameObject> worldGameObjectsByObjId = WorldMap.GetWorldGameObjectsByObjId(in_wgo_name.value);
				if (worldGameObjectsByObjId != null && worldGameObjectsByObjId.Count > 0)
				{
					worldGameObject = worldGameObjectsByObjId[0];
				}
			}
			if (worldGameObject == null)
			{
				Debug.LogError("WGO is null!");
			}
			else
			{
				GDPoint gDPointByGDTag = WorldMap.GetGDPointByGDTag(in_gd_point_tag.value);
				if (gDPointByGDTag == null)
				{
					Debug.LogError("Can't find GD point: " + in_gd_point_tag.value);
				}
				else
				{
					worldGameObject.transform.position = gDPointByGDTag.transform.position;
					worldGameObject.RefreshPositionCache();
					worldGameObject.gameObject.SetActive(value: true);
					worldGameObject.OnCameToGDPoint(gDPointByGDTag);
					worldGameObject.DrawPuffFX();
					out_o = worldGameObject;
					Debug.Log("Teleporting, output name = " + worldGameObject.name + ", obj_id = " + worldGameObject.obj_id + ", instance_id = " + worldGameObject.gameObject.GetInstanceID());
					flow_out.Call(f);
				}
			}
		});
	}

	protected override void OnNodeInspectorGUI()
	{
		base.OnNodeInspectorGUI();
		MakeStringNullIfEmpty("obj_id");
		MakeStringNullIfEmpty("custom_tag");
		MakeStringNullIfEmpty("gd_point");
	}
}
