using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

[Name("Attach Zombie To Conveyor Workbench", 0)]
[Category("Game/Environment")]
[Color("313c8f")]
public class Flow_AttachZombieToConveyorWorkbench : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<ZombieWgoData> zombieData;

	private ValueInput<WgoData> conveyorWorkbenchData;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), SetZombieToConveyorWorkbench);
		@out = AddFlowOutput("out".CapitalizeFirst());
		zombieData = AddValueInput<ZombieWgoData>("zombieData".CapitalizeFirst());
		conveyorWorkbenchData = AddValueInput<WgoData>("conveyorWorkbenchData".CapitalizeFirst());
	}

	private void SetZombieToConveyorWorkbench(Flow flow)
	{
		zombieData.value.AttachToConveyorCraftWgoData(conveyorWorkbenchData.value.UniqueId, zombieData.value.ZombieItem);
		@out.Call(flow);
	}
}
