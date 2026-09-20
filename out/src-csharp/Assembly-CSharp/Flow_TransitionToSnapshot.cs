using FlowCanvas;
using FlowCanvas.Nodes;
using ParadoxNotion.Design;

[Category("Game Actions")]
[Name("Transition To Snapshot", 0)]
public class Flow_TransitionToSnapshot : MyFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<string> snapshot_name;

	private ValueInput<float> transition_time;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("In", TransitionToSnapshot);
		@out = AddFlowOutput("Out");
		snapshot_name = AddValueInput<string>("Snapshot name");
		transition_time = AddValueInput<float>("Transition time");
	}

	private void TransitionToSnapshot(Flow flow)
	{
		SmartAudioEngine.me.TransitionToSnapshot(snapshot_name.value, transition_time.value);
		@out.Call(flow);
	}
}
