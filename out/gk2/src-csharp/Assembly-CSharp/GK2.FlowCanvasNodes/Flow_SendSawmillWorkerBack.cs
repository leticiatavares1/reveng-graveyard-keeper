using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Send Sawmill Worker Back", 0)]
[Category("Game/Zombie")]
public class Flow_SendSawmillWorkerBack : GKCustomFlowNode
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
		if (selfWgoData.Worker is ZombieWgoData zombieWgoData)
		{
			MainGame.WorldData.GetWgoData("builder_sawmill").SetGameRes(zombieWgoData.GameResStr.Get("sawmill_point"), 0);
			zombieWgoData.GameResStr.Remove("sawmill_point");
			zombieWgoData.CaretakerPortableItem = new Item(selfWgoData.CraftComponent.CurrentCraftElement.Def.outputItems.chanceOutputItems[0].id);
			zombieWgoData.AttachedWgoData.CraftComponent.CurrentCraftElement.ParamsData.customRes.Set("wait_for_zombie_at_sawmill", 0f);
			zombieWgoData.FireEvent("sawmill_craft_end");
			GDPointData gDPointData = MainGame.WorldData.GetWgoData("sawmill_wood_container").GetGDPointData("zombie_sawmill_wood_container_gd_point");
			zombieWgoData.AttachedWgoData.TryRemoveWorkerCutUnit(zombieWgoData);
			zombieWgoData.MovementComponent.StartPath(gDPointData.Position, NavigationGraphMaskUtils.ToGraphMask(zombieWgoData.WorldZoneData?.MovementGraphs, zombieWgoData.WorldZoneData?.navigationGraph ?? LazyConsts.Navigation.Graph.None), zombieWgoData.WorldId, 1.5f, "sawmill_on_went_back");
		}
		else
		{
			Debug.LogError("No zombie linked to sawmill object");
		}
		@out.Call(flow);
	}
}
