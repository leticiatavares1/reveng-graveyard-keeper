using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Name("Open Tavern Cashbox", 0)]
[Category("Game Actions")]
public class Flow_OpenTavernCashbox : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> in_cashbox_wgo = AddValueInput<WorldGameObject>("cashbox WGO");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject tavern_cashbox = WGOParamOrSelf(in_cashbox_wgo);
			GUIElements.me.pray_report.OpenTavernCashbox(tavern_cashbox);
			flow_out.Call(f);
		});
	}
}
