using System.Collections.Generic;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Spawn tavern event visitors", 0)]
[Category("Game Actions")]
[Description("")]
[Icon("CubePlus", false, "")]
public class Flow_TavernEventSpawnVisitors : MyFlowNode
{
	private List<WorldGameObject> visitors;

	private List<WorldGameObject> viewers;

	protected override void RegisterPorts()
	{
		ValueInput<string> in_event_name = AddValueInput<string>("event name");
		AddValueOutput("visitors", () => visitors);
		AddValueOutput("viewers", () => viewers);
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			if (string.IsNullOrEmpty(in_event_name.value))
			{
				Debug.LogError("Flow_TavernEventSpawnVisitors error: event name is empty!");
				flow_out.Call(f);
			}
			else
			{
				TavernEventDefinition data = GameBalance.me.GetData<TavernEventDefinition>(in_event_name.value);
				if (data == null)
				{
					Debug.LogError("Flow_TavernEventSpawnVisitors error: event definition is null!");
					flow_out.Call(f);
				}
				else
				{
					int num = Mathf.FloorToInt(data.visitors_count.EvaluateFloat(null, MainGame.me.player));
					int num2 = Mathf.FloorToInt(data.viewers_count.EvaluateFloat(null, MainGame.me.player));
					List<string> availableIdlePoints = MainGame.me.save.players_tavern_engine.GetAvailableIdlePoints(data);
					List<GDPoint> list = new List<GDPoint>();
					List<GDPoint> list2 = new List<GDPoint>();
					foreach (string item in availableIdlePoints)
					{
						GDPoint gDPointByGDTag = WorldMap.GetGDPointByGDTag(item);
						if (gDPointByGDTag != null)
						{
							list.Add(gDPointByGDTag);
						}
					}
					if (num >= availableIdlePoints.Count)
					{
						list2.AddRange(list);
					}
					else
					{
						do
						{
							int index = 0;
							if (!data.idle_points_whitelist.Contains(list[0].gd_tag))
							{
								index = UnityEngine.Random.Range(0, list.Count);
							}
							list2.Add(list[index]);
							list.RemoveAt(index);
						}
						while (num-- > 0);
					}
					visitors = new List<WorldGameObject>();
					int maxExclusive = MainGame.me.save.players_tavern_engine.TAVERN_VISITORS.Length;
					foreach (GDPoint item2 in list2)
					{
						WorldGameObject worldGameObject = GS.Spawn(MainGame.me.save.players_tavern_engine.TAVERN_VISITORS[UnityEngine.Random.Range(0, maxExclusive)], item2.transform, "npc_event_visitor");
						worldGameObject.OnCameToGDPoint(item2);
						worldGameObject.SetParam("is_in_event", 1f);
						visitors.Add(worldGameObject);
					}
					viewers = new List<WorldGameObject>();
					List<string> list3 = new List<string>();
					list3.AddRange(data.viewers_points);
					List<string> list4 = new List<string>();
					if (num2 > 0 && list3.Count > 0)
					{
						if (num2 >= list3.Count)
						{
							list4.AddRange(list3);
						}
						else
						{
							do
							{
								int index2 = UnityEngine.Random.Range(0, list3.Count);
								list4.Add(list3[index2]);
								list3.RemoveAt(index2);
							}
							while (num2-- > 0);
						}
						List<GDPoint> list5 = new List<GDPoint>();
						foreach (string item3 in list4)
						{
							GDPoint gDPointByGDTag2 = WorldMap.GetGDPointByGDTag(item3);
							if (gDPointByGDTag2 != null)
							{
								list5.Add(gDPointByGDTag2);
							}
						}
						foreach (GDPoint item4 in list5)
						{
							WorldGameObject worldGameObject2 = GS.Spawn(MainGame.me.save.players_tavern_engine.TAVERN_VISITORS[UnityEngine.Random.Range(0, maxExclusive)], item4.transform, "npc_event_visitor");
							worldGameObject2.OnCameToGDPoint(item4);
							worldGameObject2.SetParam("is_in_event", 1f);
							worldGameObject2.SetParam("is_custom_anim", 1f);
							viewers.Add(worldGameObject2);
						}
					}
					Debug.Log($"Tavern Engine: Spawned visitors: {visitors.Count}, viewers: {viewers.Count}");
					flow_out.Call(f);
				}
			}
		});
	}
}
