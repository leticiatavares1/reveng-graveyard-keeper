using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Description("If WGO is null, then self")]
[Name("Add Money", 0)]
[Category("Game Actions")]
public class Flow_AddMoney : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<float> in_value = AddValueInput<float>("Value");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			MainGame.me.player.AddToParams("money", in_value.value);
			DropCollectGUI.OnMoneyCollected(in_value.value);
			flow_out.Call(f);
		});
	}
}
