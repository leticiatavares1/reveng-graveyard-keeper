using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Description("When In is called, calls On or Off depending on the current toggle state. Whenever Toggle input is called the state changes.")]
[Category("Flow Controllers/Togglers")]
public class Toggle : FlowControlNode
{
	public bool open = true;

	private bool original;

	public override string name => base.name + " " + (open ? "[ON]" : "[OFF]");

	public override void OnGraphStarted()
	{
		original = open;
	}

	public override void OnGraphStoped()
	{
		open = original;
	}

	protected override void RegisterPorts()
	{
		FlowOutput tOut = AddFlowOutput("On");
		FlowOutput fOut = AddFlowOutput("Off");
		AddFlowInput("In", delegate(Flow f)
		{
			Call(open ? tOut : fOut, f);
		});
		AddFlowInput("Toggle", delegate
		{
			open = !open;
		});
	}
}
