using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("On Zombie Went To Clay Work Point", 0)]
[Category("Game/Zombie")]
public class Flow_OnZombieWentToClayWorkPoint : GKCustomFlowNode
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
			Debug.Log("Zombie went to clay work point");
			zombieWgoData.AttachedWgoData.CraftComponent.CurrentCraftElement.ParamsData.customRes.Set("wait_for_zombie_at_clay", 0f);
			zombieWgoData.SetCustomAnimationState(AnimationState.ToolShovel);
			zombieWgoData.direction.Value = Direction.Down.ConvertToVector2XZ();
			zombieWgoData.AttachedWgoData.TryAddWorkerCutUnit(zombieWgoData);
		}
		else
		{
			Debug.LogError("Object is not zombie!!!");
		}
		@out.Call(flow);
	}
}
