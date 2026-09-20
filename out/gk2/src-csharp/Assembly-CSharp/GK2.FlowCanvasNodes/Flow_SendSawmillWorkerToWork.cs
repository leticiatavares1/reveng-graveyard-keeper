using System.Collections.Generic;
using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Send Sawmill Worker To Work", 0)]
[Category("Game/Zombie")]
public class Flow_SendSawmillWorkerToWork : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), DoAction);
		@out = AddFlowOutput("out".CapitalizeFirst());
	}

	private void DoAction(Flow flow)
	{
		WgoData selfWgoData = base.SelfWgoData;
		WgoData wgoData = MainGame.WorldData.GetWgoData("builder_sawmill");
		if (selfWgoData.Worker is ZombieWgoData zombieWgoData)
		{
			GDPointData gDPointData = null;
			List<GDPointData> list = new List<GDPointData>();
			for (int i = 0; i < int.MaxValue; i++)
			{
				gDPointData = MainGame.Instance.GameSave.worldData.gdPointsData.GetGDPointDataById($"sawmill_zombie_{i + 1}");
				if (gDPointData == null)
				{
					break;
				}
				list.Add(gDPointData);
			}
			for (int j = 0; j < list.Count; j++)
			{
				GDPointData gDPointData2 = list[j];
				if (wgoData.GetGameResInt(gDPointData2.Id) == 0)
				{
					gDPointData = gDPointData2;
					wgoData.SetGameRes(gDPointData2.Id, 1);
					break;
				}
			}
			if (gDPointData == null)
			{
				Debug.LogError("Can't find gd point for zombie on sawmill");
			}
			else
			{
				zombieWgoData.GameResStr.Set("sawmill_point", gDPointData.Id);
				zombieWgoData.AttachedWgoData.SetGameRes("stuff_disabled", 1);
				zombieWgoData.AttachedWgoData.TryRemoveWorkerCutUnit(zombieWgoData);
				zombieWgoData.MovementComponent.StartPath(gDPointData.Position, NavigationGraphMaskUtils.ToGraphMask(zombieWgoData.WorldZoneData?.MovementGraphs, zombieWgoData.WorldZoneData?.navigationGraph ?? LazyConsts.Navigation.Graph.None), zombieWgoData.WorldId, 1.5f, "sawmill_craft_start");
			}
		}
		else
		{
			Debug.LogError("No zombie linked to sawmill object");
		}
		@out.Call(flow);
	}
}
