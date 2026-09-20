using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Char Wait", 0)]
public class Flow_CharWait : MyFlowNode
{
	protected override void RegisterPorts()
	{
		FlowOutput flow_out = AddFlowOutput("Out");
		FlowOutput on_started_waiting = AddFlowOutput("On Started Waiting");
		FlowOutput on_ended_waiting = AddFlowOutput("On Ended Waiting");
		AddFlowInput("In", delegate(Flow f)
		{
			GUIElements.me.waiting_gui.Open(delegate
			{
				on_started_waiting.Call(f);
			}, delegate
			{
				on_ended_waiting.Call(f);
			});
			flow_out.Call(f);
		});
	}
}
