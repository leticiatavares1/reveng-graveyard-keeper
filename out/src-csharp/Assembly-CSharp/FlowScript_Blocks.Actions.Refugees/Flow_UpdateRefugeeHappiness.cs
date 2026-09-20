using DLCRefugees;
using FlowCanvas;
using FlowCanvas.Nodes;
using ParadoxNotion.Design;

namespace FlowScript_Blocks.Actions.Refugees;

[Category("Game Actions/Refugees")]
[Name("Update Refugee Happiness", 0)]
public class Flow_UpdateRefugeeHappiness : MyFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<RefugeesCampEngine.UpdateHappinessItemsMode> mode;

	private ValueInput<float> happiness_delta;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("In", UpdateRefugeeHappiness);
		@out = AddFlowOutput("Out");
		mode = AddValueInput<RefugeesCampEngine.UpdateHappinessItemsMode>("update mode");
		happiness_delta = AddValueInput<float>("happiness delta");
	}

	private void UpdateRefugeeHappiness(Flow flow)
	{
		RefugeesCampEngine.instance.UpdateRefugeeCampValues(happiness_delta.value, mode.value);
		@out.Call(flow);
	}
}
