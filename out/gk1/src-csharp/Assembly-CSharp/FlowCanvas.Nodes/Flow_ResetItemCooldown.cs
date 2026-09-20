using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Reset item cooldown", 0)]
public class Flow_ResetItemCooldown : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<string> in_item_id = AddValueInput<string>("item id");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			MainGame.me.player.SetParam("_cooldown_" + in_item_id.value, 0f);
			flow_out.Call(f);
		});
	}
}
