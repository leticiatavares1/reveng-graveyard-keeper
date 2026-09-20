using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Description("Use for organization.")]
[Name("Identity", 100)]
public class Dummy : FlowControlNode
{
	public override string name => null;

	protected override void RegisterPorts()
	{
		FlowOutput fOut = AddFlowOutput(" ", "Out");
		AddFlowInput(" ", "In", delegate(Flow f)
		{
			fOut.Call(f);
		});
	}
}
