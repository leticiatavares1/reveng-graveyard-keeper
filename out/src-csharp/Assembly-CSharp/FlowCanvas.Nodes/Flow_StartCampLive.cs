using DLCRefugees;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Start Camp Live", 0)]
[Category("Game Actions")]
public class Flow_StartCampLive : MyFlowNode
{
	public override string name
	{
		get
		{
			if (GetInputValuePort<bool>("state").value)
			{
				return "<color=#FFFF50>Start Camp Live</color>";
			}
			return "<color=#30FF30>Stop Camp Live</color>";
		}
		set
		{
			base.name = value;
		}
	}

	protected override void RegisterPorts()
	{
		ValueInput<bool> start_or_stop = AddValueInput<bool>("state");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			if (start_or_stop.value)
			{
				RefugeesCampEngine.instance.StartCampLive();
			}
			else
			{
				RefugeesCampEngine.instance.StopCampLive();
			}
			flow_out.Call(f);
		});
	}
}
