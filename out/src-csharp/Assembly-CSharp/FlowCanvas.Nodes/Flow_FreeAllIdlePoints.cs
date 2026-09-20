using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Free All Idle Points", 0)]
[Category("Game Actions")]
[Description("Free All Idle Points")]
public class Flow_FreeAllIdlePoints : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<GDPoint.IdlePointPrefix> in_idle_points_prefix = AddValueInput<GDPoint.IdlePointPrefix>("prefix");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			if (in_idle_points_prefix.value == GDPoint.IdlePointPrefix.None)
			{
				Debug.LogError("Wrong IdlePointPrefix: " + in_idle_points_prefix.value);
				flow_out.Call(f);
			}
			else
			{
				string idlePrefix = GDPoint.GetIdlePrefix(in_idle_points_prefix.value);
				WorldGameObject worldGameObjectByCustomTag = WorldMap.GetWorldGameObjectByCustomTag(idlePrefix + "stock");
				if (worldGameObjectByCustomTag == null)
				{
					Debug.LogError("Not found stock WGO with custom tag \"" + idlePrefix + "stock\"!");
					flow_out.Call(f);
				}
				else
				{
					int paramInt = worldGameObjectByCustomTag.GetParamInt("max_idle_points");
					if (paramInt < 1)
					{
						Debug.LogError("Max idle points with prefix \"" + idlePrefix + "\", stored in [" + worldGameObjectByCustomTag.name + "::" + worldGameObjectByCustomTag.obj_id + "] is wrong: " + paramInt);
						flow_out.Call(f);
					}
					else
					{
						for (int i = 1; i <= paramInt; i++)
						{
							if (worldGameObjectByCustomTag.data.GetParamInt(idlePrefix + i) == 1)
							{
								worldGameObjectByCustomTag.data.SetParam(idlePrefix + i, 0f);
								Debug.Log("#ipm# [" + worldGameObjectByCustomTag.obj_id + "]: free idle point {" + idlePrefix + i + "}");
							}
						}
						flow_out.Call(f);
					}
				}
			}
		});
	}
}
