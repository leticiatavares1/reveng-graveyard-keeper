using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Set Pseudo Time", 0)]
[Description("Set Pseudo Time")]
public class Flow_SetPseudoTime : MyFlowNode
{
	public static float remembered_time = -1f;

	public override string name
	{
		get
		{
			if (GetInputValuePort<float>("Time").value < -1f)
			{
				return "Restore Pseudo Time";
			}
			return base.name;
		}
		set
		{
			base.name = value;
		}
	}

	protected override void RegisterPorts()
	{
		ValueInput<float> in_time = AddValueInput<float>("Time");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			if (in_time.value < -1f)
			{
				TimeOfDay.me.time_of_day = remembered_time;
				remembered_time = -1f;
			}
			else
			{
				if (remembered_time == -1f)
				{
					remembered_time = TimeOfDay.me.time_of_day;
				}
				TimeOfDay.me.time_of_day = in_time.value;
			}
			TimeOfDay.me.Update();
			flow_out.Call(f);
		});
	}
}
