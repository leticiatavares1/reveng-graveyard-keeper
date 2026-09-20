using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Get Time Of Day", 0)]
[Color("f47dff")]
public class Flow_GetTimeOfDay : GKCustomFlowNode
{
	private ValueOutput<float> timeOfDay;

	protected override void RegisterPorts()
	{
		timeOfDay = AddValueOutput("timeOfDay".CapitalizeFirst(), () => EnvironmentEngine.Instance.timeOfDay);
	}
}
