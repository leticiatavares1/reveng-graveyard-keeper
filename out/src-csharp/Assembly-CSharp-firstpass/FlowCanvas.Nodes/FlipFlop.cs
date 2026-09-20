using System;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[ContextDefinedOutputs(new Type[] { typeof(bool) })]
[Description("Flip Flops between the 2 outputs each time In is called")]
[Category("Flow Controllers/Togglers")]
public class FlipFlop : FlowControlNode
{
	public bool isFlip = true;

	private bool original;

	public override string name => base.name + " " + (isFlip ? "[FLIP]" : "[FLOP]");

	public override void OnGraphStarted()
	{
		original = isFlip;
	}

	public override void OnGraphStoped()
	{
		isFlip = original;
	}

	protected override void RegisterPorts()
	{
		FlowOutput flipF = AddFlowOutput("Flip");
		FlowOutput flopF = AddFlowOutput("Flop");
		AddFlowInput("In", delegate(Flow f)
		{
			Call(isFlip ? flipF : flopF, f);
			isFlip = !isFlip;
		});
		AddFlowInput("Reset", delegate
		{
			isFlip = false;
		});
		AddValueOutput("Is Flip", () => isFlip);
	}
}
