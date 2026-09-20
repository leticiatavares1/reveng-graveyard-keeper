using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Description("Removes one time craft if it exists")]
[Name("Remove One Time Craft", 0)]
[Category("Game Actions")]
public class Flow_RemoveOneTimeCraft : MyFlowNode
{
	protected override void RegisterPorts()
	{
		FlowOutput flow_out = AddFlowOutput("Out");
		ValueInput<string> craft = AddValueInput<string>("Craft id");
		AddFlowInput("In", delegate(Flow f)
		{
			MainGame.me.save.completed_one_time_crafts.Remove(craft.value);
			flow_out.Call(f);
		});
	}
}
