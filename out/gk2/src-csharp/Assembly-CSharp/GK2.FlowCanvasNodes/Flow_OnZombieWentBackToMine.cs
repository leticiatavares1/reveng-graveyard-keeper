using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("On Zombie Went Back To Mine", 0)]
[Category("Game/Zombie")]
public class Flow_OnZombieWentBackToMine : GKCustomFlowNode
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
			zombieWgoData.CaretakerPortableItem = Item.Empty;
			zombieWgoData.AttachedWgoData.SetGameRes("stuff_disabled", 0);
			zombieWgoData.AttachedWgoData.FireEvent("craft_finish");
			zombieWgoData.MovableDirection = Direction.Down.ConvertToVector2XZ();
			zombieWgoData.AttachedWgoData.TryAddWorkerCutUnit(zombieWgoData);
		}
		else
		{
			Debug.LogError("Object is not zombie!!!");
		}
		@out.Call(flow);
	}
}
