using System.Collections.Generic;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Tavern Engine Get GDPoints For Event", 0)]
[Category("Game Actions")]
public class Flow_TavernEngineGetGDPointsForEvent : MyFlowNode
{
	private List<GDPoint> _out_gd_points;

	protected override void RegisterPorts()
	{
		ValueInput<string> in_event_id = AddValueInput<string>("event id");
		ValueInput<List<GDPoint>> in_gd_points = AddValueInput<List<GDPoint>>("available GDPoints");
		AddValueOutput("points for event", () => _out_gd_points);
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			_out_gd_points = new List<GDPoint>();
			if (string.IsNullOrEmpty(in_event_id.value))
			{
				Debug.LogError("Flow_TavernEngineGetGDPointsForEvent error: in_event_id is empty!");
			}
			else
			{
				TavernEventDefinition data = GameBalance.me.GetData<TavernEventDefinition>(in_event_id.value);
				if (data == null)
				{
					Debug.LogError("Flow_TavernEngineGetGDPointsForEvent error: event_definition is NULL!");
				}
				else
				{
					int num = Mathf.FloorToInt(data.visitors_count.EvaluateFloat(null, MainGame.me.player));
					if (in_gd_points.value == null || in_gd_points.value.Count == 0)
					{
						Debug.LogError("Flow_TavernEngineGetGDPointsForEvent error: input gd_points is null or empty!");
						flow_out.Call(f);
					}
					else if (num >= in_gd_points.value.Count)
					{
						_out_gd_points = new List<GDPoint>();
						_out_gd_points.AddRange(in_gd_points.value);
						flow_out.Call(f);
					}
					else
					{
						List<GDPoint> list = new List<GDPoint>();
						list.AddRange(in_gd_points.value);
						do
						{
							int index = 0;
							if (!data.idle_points_whitelist.Contains(list[0].gd_tag))
							{
								index = UnityEngine.Random.Range(0, list.Count);
							}
							_out_gd_points.Add(list[index]);
							list.RemoveAt(index);
						}
						while (num-- > 0);
						flow_out.Call(f);
					}
				}
			}
		});
	}
}
