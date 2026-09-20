using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Unlock Craft", 0)]
public class Flow_UnlockCraft : MyFlowNode
{
	protected override void RegisterPorts()
	{
		FlowOutput flow_out = AddFlowOutput("Out");
		ValueInput<string> craft = AddValueInput<string>("Craft id");
		AddFlowInput("In", delegate(Flow f)
		{
			MainGame.me.save.UnlockCraft(craft.value);
			flow_out.Call(f);
		});
	}
}
