using System;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Flow Controllers/Switchers")]
[Name("Switch Condition", 0)]
[ContextDefinedInputs(new Type[] { typeof(bool) })]
[Description("Branch the Flow based on a conditional boolean value")]
public class SwitchBool : FlowControlNode
{
	protected override void RegisterPorts()
	{
		ValueInput<bool> c = AddValueInput<bool>("Condition");
		FlowOutput tOut = AddFlowOutput("True");
		FlowOutput fOut = AddFlowOutput("False");
		AddFlowInput("In", delegate(Flow f)
		{
			Call(c.value ? tOut : fOut, f);
		});
	}
}
