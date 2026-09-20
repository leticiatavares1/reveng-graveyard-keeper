using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Player throw body", 0)]
public class Flow_PlayerThrowBody : MyFlowNode
{
	public bool thrown_is_not_worker = true;

	protected override void RegisterPorts()
	{
		AddValueOutput("thrown NOT worker", () => thrown_is_not_worker);
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			MainGame.me.player_component.ThrowBodyInRiver(out var thrown_worker);
			thrown_is_not_worker = !thrown_worker;
			flow_out.Call(f);
		});
	}
}
