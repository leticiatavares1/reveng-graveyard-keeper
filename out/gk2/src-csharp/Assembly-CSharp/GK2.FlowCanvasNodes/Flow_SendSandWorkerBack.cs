using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Send Sand Worker Back", 0)]
[Category("Game/Zombie")]
public class Flow_SendSandWorkerBack : GKCustomFlowNode
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
		if (base.SelfWgoData.Worker is ZombieWgoData zombieWgoData)
		{
			WgoData attachedWgoData = zombieWgoData.AttachedWgoData;
			MainGame.WorldData.GetWgoData("builder_clay_sand").SetGameRes(zombieWgoData.GameResStr.Get("sand_point"), 0);
			zombieWgoData.GameResStr.Remove("sand_point");
			zombieWgoData.AttachedWgoData.CraftComponent.CurrentCraftElement.ParamsData.customRes.Set("wait_for_zombie_at_sand", 0f);
			zombieWgoData.FireEvent("sand_craft_end");
			GDPointData gDPointData = attachedWgoData.GetGDPointData("zombie_clay_sand_crafter_gd_point");
			zombieWgoData.AttachedWgoData.TryRemoveWorkerCutUnit(zombieWgoData);
			zombieWgoData.MovementComponent.StartPath(gDPointData.Position, zombieWgoData.WorldZoneData.navigationGraph, zombieWgoData.WorldId, 1.5f, "sand_on_went_back");
		}
		else
		{
			Debug.LogError("No zombie linked to sand object");
		}
		@out.Call(flow);
	}
}
