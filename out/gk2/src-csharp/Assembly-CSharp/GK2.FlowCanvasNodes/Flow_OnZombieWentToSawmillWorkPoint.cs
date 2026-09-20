using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("On Zombie Went To Sawmill Work Point", 0)]
[Category("Game/Zombie")]
public class Flow_OnZombieWentToSawmillWorkPoint : GKCustomFlowNode
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
			zombieWgoData.AttachedWgoData.CraftComponent.CurrentCraftElement.ParamsData.customRes.Set("wait_for_zombie_at_sawmill", 0f);
			zombieWgoData.SetCustomAnimationState(AnimationState.ToolAxe);
			zombieWgoData.AttachedWgoData.TryAddWorkerCutUnit(zombieWgoData);
		}
		else
		{
			Debug.LogError("Object is not zombie!!!");
		}
		@out.Call(flow);
	}
}
