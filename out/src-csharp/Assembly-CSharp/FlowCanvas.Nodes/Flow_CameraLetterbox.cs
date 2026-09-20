using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Camera Letterbox", 0)]
[Category("Game Actions")]
public class Flow_CameraLetterbox : MyFlowNode
{
	public override string name
	{
		get
		{
			return base.name + (GetInputValuePort<bool>("Show").value ? " (on)" : " (off)");
		}
		set
		{
			base.name = value;
		}
	}

	protected override void RegisterPorts()
	{
		ValueInput<bool> par_show = AddValueInput<bool>("Show");
		FlowOutput flow_out = AddFlowOutput("Out");
		FlowOutput flow_finished = AddFlowOutput("On Finished");
		AddFlowInput("In", delegate(Flow f)
		{
			CameraTools.TweenLetterbox(par_show.value);
			GJTimer.AddTimer(1f, delegate
			{
				flow_finished.Call(f);
			});
			flow_out.Call(f);
		});
	}
}
