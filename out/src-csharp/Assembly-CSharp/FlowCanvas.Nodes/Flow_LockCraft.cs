using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Lock Craft", 0)]
public class Flow_LockCraft : MyFlowNode
{
	protected override void RegisterPorts()
	{
		FlowOutput flow_out = AddFlowOutput("Out");
		ValueInput<string> craft = AddValueInput<string>("Craft id");
		AddFlowInput("In", delegate(Flow f)
		{
			MainGame.me.save.LockCraft(craft.value);
			flow_out.Call(f);
		});
	}
}
