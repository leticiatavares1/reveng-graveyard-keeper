using System.Collections.Generic;
using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;
using Pathfinding;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Send Clay Worker To Work", 0)]
[Category("Game/Zombie")]
public class Flow_SendClayWorkerToWork : GKCustomFlowNode
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
		WgoData wgoData = MainGame.WorldData.GetWgoData("builder_clay_sand");
		if (selfWgoData.Worker is ZombieWgoData zombieWgoData)
		{
			GDPointData gDPointData = null;
			List<GDPointData> list = new List<GDPointData>();
			for (int i = 0; i < int.MaxValue; i++)
			{
				gDPointData = MainGame.Instance.GameSave.worldData.gdPointsData.GetGDPointDataById($"clay_zombie_{i + 1}");
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
				Debug.LogError("Can't find gd point for zombie on clay");
			}
			else
			{
				NNInfo nearest = AstarPath.active.graphs[16].GetNearest(gDPointData.Position, NearestNodeConstraint.Walkable);
				NNInfo nearest2 = AstarPath.active.graphs[11].GetNearest(gDPointData.Position, NearestNodeConstraint.Walkable);
				float num = Vector3.Distance(nearest.position, gDPointData.Position);
				float num2 = Vector3.Distance(nearest2.position, gDPointData.Position);
				GraphMask graphMask = NavigationGraphMaskUtils.ToGraphMask((num < num2) ? LazyConsts.Navigation.Graph.SandClay : LazyConsts.Navigation.Graph.RuinedTemple);
				zombieWgoData.GameResStr.Set("clay_point", gDPointData.Id);
				zombieWgoData.AttachedWgoData.SetGameRes("stuff_disabled", 1);
				zombieWgoData.AttachedWgoData.TryRemoveWorkerCutUnit(zombieWgoData);
				zombieWgoData.MovementComponent.StartPath(gDPointData.Position, graphMask, zombieWgoData.WorldId, 1.5f, "clay_craft_start");
			}
		}
		else
		{
			Debug.LogError("No zombie linked to clay object");
		}
		@out.Call(flow);
	}
}
