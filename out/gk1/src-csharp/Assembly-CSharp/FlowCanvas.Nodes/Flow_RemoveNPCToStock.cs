using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Remove NPC To Stock", 0)]
public class Flow_RemoveNPCToStock : MyFlowNode
{
	public bool is_with_puff_fx = true;

	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> in_wgo = AddValueInput<WorldGameObject>("WGO");
		ValueInput<string> in_gd_point_tag = AddValueInput<string>("GD Point Tag");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject worldGameObject = WGOParamOrSelf(in_wgo);
			if (worldGameObject == null)
			{
				Debug.LogError("WGO is null!");
			}
			else
			{
				if (is_with_puff_fx)
				{
					worldGameObject.DrawPuffFX();
				}
				GDPoint gDPoint = null;
				gDPoint = WorldMap.GetGDPointByGDTag((!string.IsNullOrEmpty(in_gd_point_tag.value)) ? in_gd_point_tag.value : "default_destroy_point");
				if (gDPoint == null)
				{
					Debug.LogError("Can't find GD point: " + in_gd_point_tag.value);
				}
				else
				{
					Debug.Log("Teleporting " + worldGameObject.obj_id + " to GD point: " + gDPoint.name, gDPoint.gameObject);
					worldGameObject.transform.position = gDPoint.transform.position;
					worldGameObject.RefreshPositionCache();
					flow_out.Call(f);
					worldGameObject.OnCameToGDPoint(gDPoint);
				}
			}
		});
	}
}
