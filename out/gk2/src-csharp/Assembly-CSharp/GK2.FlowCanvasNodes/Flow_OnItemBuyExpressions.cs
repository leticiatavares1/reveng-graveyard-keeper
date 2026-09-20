using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Call On Item Buy Expressions", 0)]
[Category("Game/Item")]
[Color("FFFFFF")]
public class Flow_OnItemBuyExpressions : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	private ValueInput<Item> item;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), CallOnBuyExpressions);
		@out = AddFlowOutput("out".CapitalizeFirst());
		item = AddValueInput<Item>("item");
	}

	private void CallOnBuyExpressions(Flow flow)
	{
		Item value = item.value;
		if (value != null)
		{
			foreach (LazyExpression item in value.Definition.expressionsOnBuy)
			{
				item.Evaluate(value);
			}
		}
		@out.Call(flow);
	}
}
