using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

[Name("Start Craft At Conveyor Workbench", 0)]
[Category("Game/Environment")]
[Color("313c8f")]
public class Flow_StartCraftAtConveyorWorkbench : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<WgoData> coneyorWorkbenchData;

	private ValueInput<string> craftId;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), SetZombieToConveyorWorkbench);
		@out = AddFlowOutput("out".CapitalizeFirst());
		coneyorWorkbenchData = AddValueInput<WgoData>("coneyorWorkbenchData".CapitalizeFirst());
		craftId = AddValueInput<string>("craftId".CapitalizeFirst());
	}

	private void SetZombieToConveyorWorkbench(Flow flow)
	{
		if (coneyorWorkbenchData != null)
		{
			coneyorWorkbenchData.value.CraftComponent.AddToQueue(new CraftElement(GameBalance.GetCraftDef(craftId.value), new CraftParamsData(craftId.value, coneyorWorkbenchData.value)));
			@out.Call(flow);
		}
	}
}
