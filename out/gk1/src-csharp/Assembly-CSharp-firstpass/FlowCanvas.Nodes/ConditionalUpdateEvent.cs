using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Description("Checks the condition boolean input per frame and calls outputs when the value has changed")]
[Category("Events/Other")]
[Name("Conditional Event", 0)]
public class ConditionalUpdateEvent : EventNode, IUpdatable
{
	private FlowOutput becameTrue;

	private FlowOutput becameFalse;

	private ValueInput<bool> condition;

	private bool lastState;

	protected override void RegisterPorts()
	{
		becameTrue = AddFlowOutput("Became True");
		becameFalse = AddFlowOutput("Became False");
		condition = AddValueInput<bool>("Condition");
	}

	public void Update()
	{
		if (!condition.value)
		{
			if (lastState)
			{
				becameFalse.Call(default(Flow));
				lastState = false;
			}
		}
		else if (!lastState)
		{
			becameTrue.Call(default(Flow));
			lastState = true;
		}
	}
}
