using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Description("Get Time Of Day")]
[Category("Game Actions")]
[Name("Get Time Of Day", 0)]
public class Flow_GetTimeOfDay : MyFlowNode
{
	protected override void RegisterPorts()
	{
		float time_of_day = 0f;
		int day = 0;
		AddValueOutput("Time Of Day", () => time_of_day);
		AddValueOutput("Day", () => day);
		FlowOutput flow_night = AddFlowOutput("Night");
		FlowOutput flow_morning = AddFlowOutput("Morning");
		FlowOutput flow_daytime = AddFlowOutput("Daytime");
		FlowOutput flow_evening = AddFlowOutput("Evening");
		AddFlowInput("In", delegate(Flow f)
		{
			time_of_day = TimeOfDay.me.GetTimeK();
			day = MainGame.me.save.day;
			if (time_of_day < 0.15f)
			{
				flow_night.Call(f);
			}
			else if (time_of_day < 0.35f)
			{
				flow_morning.Call(f);
			}
			else if (time_of_day < 0.7f)
			{
				flow_daytime.Call(f);
			}
			else
			{
				flow_evening.Call(f);
			}
		});
	}
}
