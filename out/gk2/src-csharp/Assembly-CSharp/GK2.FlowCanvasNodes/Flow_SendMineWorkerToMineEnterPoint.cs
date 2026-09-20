using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Send Mine Worker To Enter Point", 0)]
[Category("Game/Zombie")]
public class Flow_SendMineWorkerToMineEnterPoint : GKCustomFlowNode
{
	public bool directionToMine;

	private FlowInput @in;

	private FlowOutput @out;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), DoAction);
		@out = AddFlowOutput("out".CapitalizeFirst());
	}

	private void DoAction(Flow flow)
	{
		if (base.SelfWgoData.Worker is ZombieWgoData zombieWgoData)
		{
			GDPointData gDPointDataById = MainGame.Instance.GameSave.worldData.gdPointsData.GetGDPointDataById("mine_zombie_enter");
			if (directionToMine)
			{
				zombieWgoData.AttachedWgoData.TryRemoveWorkerCutUnit(zombieWgoData);
				zombieWgoData.MovementComponent.StartPath(gDPointDataById.Position, zombieWgoData.WorldZoneData.navigationGraph, zombieWgoData.WorldId, 1.5f, "mine_on_went_to_enter_dir_to_mine");
				zombieWgoData.AttachedWgoData.SetGameRes("stuff_disabled", 1);
			}
			else
			{
				zombieWgoData.AttachedWgoData.TryRemoveWorkerCutUnit(zombieWgoData);
				zombieWgoData.MovementComponent.StartPath(gDPointDataById.Position, gDPointDataById.GameSceneDataId, gDPointDataById.GameSceneDataId, MovementType.GDGraph, 1.5f, "mine_on_went_to_enter_dir_to_home");
			}
		}
		else
		{
			Debug.LogError("No zombie linked to mine object");
		}
		@out.Call(flow);
	}
}
