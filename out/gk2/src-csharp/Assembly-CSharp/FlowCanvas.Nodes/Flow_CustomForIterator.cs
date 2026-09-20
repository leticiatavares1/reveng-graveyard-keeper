using System;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Custom For Iterator", 0)]
[Category("Flow Controllers/Iterators")]
[ContextDefinedInputs(new Type[] { typeof(int) })]
[ContextDefinedOutputs(new Type[] { typeof(int) })]
public class Flow_CustomForIterator : FlowControlNode
{
	private int current;

	protected override void RegisterPorts()
	{
		ValueInput<int> i = AddValueInput<int>("Loops");
		AddValueOutput("Index", () => current);
		FlowOutput fCurrent = AddFlowOutput("Do");
		FlowOutput fFinish = AddFlowOutput("Done");
		AddFlowInput("In", delegate(Flow f)
		{
			current = 0;
			fCurrent.Call(f);
		});
		AddFlowInput("Iterate", delegate(Flow f)
		{
			current++;
			if (current < i.value)
			{
				fCurrent.Call(f);
			}
			else
			{
				fFinish.Call(f);
			}
		});
	}
}
