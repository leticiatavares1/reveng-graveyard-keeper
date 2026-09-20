using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

[Name("UnAttach Zombie To Conveyor Workbench", 0)]
[Category("Game/Environment")]
[Color("313c8f")]
public class Flow_UnAttachZombieFromConveyorWorkbench : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<WgoData> conveyorWorkbenchData;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), SetZombieToConveyorWorkbench);
		@out = AddFlowOutput("out".CapitalizeFirst());
		conveyorWorkbenchData = AddValueInput<WgoData>("conveyorWorkbenchData".CapitalizeFirst());
	}

	private void SetZombieToConveyorWorkbench(Flow flow)
	{
		if (conveyorWorkbenchData != null && conveyorWorkbenchData.value.Worker != null && conveyorWorkbenchData.value.Worker is ZombieWgoData zombieWgoData)
		{
			zombieWgoData.UnAttachFromWgoData();
			MainGame.Instance.GameSave.zombieSystemData.RemoveZombie(zombieWgoData.UniqueId);
		}
		@out.Call(flow);
	}
}
