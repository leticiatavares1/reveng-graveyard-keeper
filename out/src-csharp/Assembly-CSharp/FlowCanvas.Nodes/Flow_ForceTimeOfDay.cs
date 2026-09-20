using System;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Force Time Of Day", 0)]
public class Flow_ForceTimeOfDay : MyFlowNode
{
	public enum TimeOfDayValue
	{
		Night,
		Day
	}

	protected override void RegisterPorts()
	{
		ValueInput<TimeOfDayValue> in_time = AddValueInput<TimeOfDayValue>("Time");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			switch (in_time.value)
			{
			case TimeOfDayValue.Night:
				TimeOfDay.me.time_of_day = 0f;
				break;
			case TimeOfDayValue.Day:
				TimeOfDay.me.time_of_day = 1f;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			MainGame.me.gui_elements.hud.Update();
			flow_out.Call(f);
		});
	}
}
