using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Send Mine Worker Back", 0)]
[Category("Game/Quests")]
public class Flow_SendMineWorkerBack : GKCustomFlowNode
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
		if (base.SelfWgoData is ZombieWgoData zombieWgoData)
		{
			WgoData attachedWgoData = zombieWgoData.AttachedWgoData;
			MainGame.WorldData.GetWgoData("builder_mine").SetGameRes(zombieWgoData.GameResStr.Get("mine_point"), 0);
			zombieWgoData.GameResStr.Remove("mine_point");
			zombieWgoData.AttachedWgoData.CraftComponent.CurrentCraftElement.ParamsData.customRes.Set("wait_for_zombie_at_mine", 0f);
			zombieWgoData.FireEvent("mine_craft_end");
			GDPointData gDPointData = attachedWgoData.GetGDPointData("zombie_mine_crafter_gd_point");
			zombieWgoData.MovementComponent.StartPath(gDPointData.Position, zombieWgoData.WorldZoneData.navigationGraph, zombieWgoData.WorldId, 1.5f, "mine_on_went_back");
		}
		else
		{
			Debug.LogError("No zombie linked to mine object");
		}
		@out.Call(flow);
	}
}
