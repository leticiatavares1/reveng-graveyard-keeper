using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Get Seconds To The Midnight", 0)]
public class Flow_GetSecondsToTheMidnight : MyFlowNode
{
	private float seconds_value;

	protected override void RegisterPorts()
	{
		FlowOutput flow_out = AddFlowOutput("Out");
		AddValueOutput("Seconds", () => seconds_value);
		AddFlowInput("In", delegate(Flow f)
		{
			seconds_value = TimeOfDay.me.GetSecondsToTheMidnight();
			flow_out.Call(f);
		});
	}
}
