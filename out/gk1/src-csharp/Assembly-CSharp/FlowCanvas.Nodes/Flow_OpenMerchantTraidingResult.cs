using ParadoxNotion.Design;

namespace FlowCanvas.Nodes;

[Category("Game Actions")]
[Name("Open Merchant Traiding Result", 0)]
public class Flow_OpenMerchantTraidingResult : MyFlowNode
{
	protected override void RegisterPorts()
	{
		ValueInput<WorldGameObject> in_cashbox_wgo = AddValueInput<WorldGameObject>("cashbox WGO");
		FlowOutput flow_out = AddFlowOutput("Out");
		AddFlowInput("In", delegate(Flow f)
		{
			WorldGameObject cashbox_wgo = WGOParamOrSelf(in_cashbox_wgo);
			GUIElements.me.pray_report.OpenMerchantTraidingResult(cashbox_wgo);
			flow_out.Call(f);
		});
	}
}
