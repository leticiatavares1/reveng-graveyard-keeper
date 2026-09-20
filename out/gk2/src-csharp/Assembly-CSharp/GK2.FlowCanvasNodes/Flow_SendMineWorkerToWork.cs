using System.Collections.Generic;
using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Send Mine Worker To Work", 0)]
[Category("Game/Zombie")]
public class Flow_SendMineWorkerToWork : GKCustomFlowNode
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
		WgoData wgoData = MainGame.WorldData.GetWgoData("builder_mine");
		if (selfWgoData is ZombieWgoData zombieWgoData)
		{
			GDPointData gDPointData = null;
			List<GDPointData> list = new List<GDPointData>();
			for (int i = 0; i < int.MaxValue; i++)
			{
				gDPointData = MainGame.Instance.GameSave.worldData.gdPointsData.GetGDPointDataById($"mine_zombie_{i + 1}");
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
				Debug.LogError("Can't find gd point for zombie on mine");
			}
			else
			{
				zombieWgoData.GameResStr.Set("mine_point", gDPointData.Id);
				zombieWgoData.MovementComponent.StartPath(gDPointData.Position, gDPointData.GameSceneDataId, gDPointData.GameSceneDataId, MovementType.GDGraph, 1.5f, "mine_craft_start");
			}
		}
		else
		{
			Debug.LogError("No zombie linked to mine object");
		}
		@out.Call(flow);
	}
}
