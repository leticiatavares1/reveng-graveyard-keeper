using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Lazy Expression", 0)]
[Category("Game/Environment")]
public class Flow_LazyExpression : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<string> expression;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), GoToSleep);
		@out = AddFlowOutput("out".CapitalizeFirst());
		expression = AddValueInput<string>("expression".CapitalizeFirst());
	}

	private void GoToSleep(Flow flow)
	{
		new LazyExpression(expression.value).Evaluate();
		@out.Call(flow);
	}
}
