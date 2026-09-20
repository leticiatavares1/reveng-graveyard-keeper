using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Time Enable", 0)]
[Icon("Clock", false, "")]
public class Flow_TimeEnable : MyFlowNode
{
	public override string name
	{
		get
		{
			if (!GetInputValuePort<bool>("Time Enable").value)
			{
				return "Time Disable";
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
		ValueInput<bool> time_enable = AddValueInput<bool>("Time Enable");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			EnvironmentEngine.me.EnableTime(time_enable.value);
			flow_out.Call(f);
		});
	}
}
