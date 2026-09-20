using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[Name("Wait Day", 0)]
public class Flow_WaitDay : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<float> par_speed = AddValueInput<float>("speed");
		FlowOutput flow_out = AddFlowOutput("Immediate");
		FlowOutput flow_done = AddFlowOutput("Finished");
		AddFlowInput("In", delegate(Flow f)
		{
			EnvironmentEngine.me.EnableTime(enable: false);
			float start_time = TimeOfDay.me.time_of_day;
			bool looped = false;
			GJTimer.AddConditionalChecker(() => (looped && TimeOfDay.me.time_of_day >= start_time) ? true : false, delegate
			{
				TimeOfDay.me.time_of_day += Time.deltaTime * par_speed.value;
				if (TimeOfDay.me.time_of_day > 1f)
				{
					TimeOfDay.me.time_of_day -= 2f;
					looped = true;
				}
			}, delegate
			{
				flow_done.Call(f);
			});
			flow_out.Call(f);
		});
	}
}
