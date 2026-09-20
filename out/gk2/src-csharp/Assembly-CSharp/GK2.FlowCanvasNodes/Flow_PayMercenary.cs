using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Pay Mercenary", 0)]
[Category("Game/Fighting")]
[Color("313c8f")]
public class Flow_PayMercenary : GKCustomFlowNodeWithWgoData
{
	private FlowInput @in;

	private FlowOutput @out;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), PayMercenary);
		@out = AddFlowOutput("out".CapitalizeFirst());
	}

	private void PayMercenary(Flow flow)
	{
		MainGame.Instance.GameSave.militaryBaseData.IsMercenaryPayed = true;
		MercenariesDef data = GameBalance.Me.GetData<MercenariesDef>(MainGame.Instance.GameSave.militaryBaseData.MercenariesPaymentId);
		if (data != null)
		{
			foreach (LazyExpression item in data.afterPayExpr)
			{
				item.Evaluate();
			}
		}
		@out.Call(flow);
	}
}
