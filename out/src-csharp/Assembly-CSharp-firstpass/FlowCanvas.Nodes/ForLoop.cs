using System;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[ContextDefinedOutputs(new Type[] { typeof(int) })]
[Description("Perform a for loop")]
[Category("Flow Controllers/Iterators")]
[ContextDefinedInputs(new Type[] { typeof(int) })]
public class ForLoop : FlowControlNode
{
	private int current;

	private bool broken;

	protected override void RegisterPorts()
	{
		ValueInput<int> i = AddValueInput<int>("Loops");
		AddValueOutput("Index", () => current);
		FlowOutput fCurrent = AddFlowOutput("Do");
		FlowOutput fFinish = AddFlowOutput("Done");
		AddFlowInput("In", delegate(Flow f)
		{
			current = 0;
			broken = false;
			f.Break = delegate
			{
				broken = true;
			};
			for (int j = 0; j < i.value; j++)
			{
				if (broken)
				{
					break;
				}
				current = j;
				fCurrent.Call(f);
			}
			f.Break = null;
			fFinish.Call(f);
		});
		AddFlowInput("Break", delegate
		{
			broken = true;
		});
	}
}
