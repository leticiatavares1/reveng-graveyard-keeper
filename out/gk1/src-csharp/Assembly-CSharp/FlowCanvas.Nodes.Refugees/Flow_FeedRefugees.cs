using DLCRefugees;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes.Refugees;

[Category("Game Actions/Refugees")]
[Name("Feed Refugee", 0)]
public class Flow_FeedRefugees : MyFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	protected override void RegisterPorts()
	{
		ValueInput<float> time_in_days = AddValueInput<float>("Time in days");
		@in = AddFlowInput("In", delegate(Flow flow)
		{
			if (time_in_days.value <= 0f || time_in_days.value > 1f)
			{
				Debug.LogError("Wrong CampEngine refugees feeding time: " + time_in_days.value);
			}
			RefugeesCampEngine.instance.FeedRefugeeForCycle(time_in_days.value);
			@out.Call(flow);
		});
		@out = AddFlowOutput("Out");
	}
}
