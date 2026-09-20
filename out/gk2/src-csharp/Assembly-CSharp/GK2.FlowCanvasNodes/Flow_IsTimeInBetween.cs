using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Is Time In Between", 0)]
[Category("Game/Environment")]
public class Flow_IsTimeInBetween : GKCustomFlowNode
{
	private FlowInput @in;

	private ValueInput<float> timeFrom;

	private ValueInput<float> timeTo;

	private FlowOutput trueOut;

	private FlowOutput falseOut;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), IsTimeInBetween);
		timeFrom = AddValueInput<float>("timeFrom");
		timeTo = AddValueInput<float>("timeTo");
		trueOut = AddFlowOutput("<color=green>✔</color>");
		falseOut = AddFlowOutput("<color=red>✘</color>");
	}

	private void IsTimeInBetween(Flow flow)
	{
		float timeOfDay = EnvironmentEngine.Instance.timeOfDay;
		bool num;
		if (!(timeFrom.value > timeTo.value))
		{
			if (!(timeOfDay > timeFrom.value))
			{
				goto IL_0072;
			}
			num = timeOfDay < timeTo.value;
		}
		else
		{
			if (timeOfDay > timeFrom.value)
			{
				goto IL_0065;
			}
			num = timeOfDay < timeTo.value;
		}
		if (num)
		{
			goto IL_0065;
		}
		goto IL_0072;
		IL_0072:
		falseOut.Call(flow);
		return;
		IL_0065:
		trueOut.Call(flow);
	}
}
