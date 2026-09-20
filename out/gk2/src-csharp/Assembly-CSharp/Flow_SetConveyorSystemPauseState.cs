using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

[Name("Set Conveyor System Pause State", 0)]
[Category("Game/Environment")]
[Color("313c8f")]
public class Flow_SetConveyorSystemPauseState : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<bool> isPaused;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), SetZombieToConveyorWorkbench);
		@out = AddFlowOutput("out".CapitalizeFirst());
		isPaused = AddValueInput<bool>("isPaused".CapitalizeFirst());
	}

	private void SetZombieToConveyorWorkbench(Flow flow)
	{
		MainGame.Instance.conveyorSystem.IsPaused = isPaused.value;
		@out.Call(flow);
	}
}
