using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Drop Overhead", 0)]
[Category("Game/Player")]
[Color("313c8f")]
public class Flow_PlayerDropOverhead : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	private Item lastDroppedItem;

	private ValueOutput<Item> item;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), TryDropOverhead);
		@out = AddFlowOutput("out".CapitalizeFirst());
		item = AddValueOutput("item", () => lastDroppedItem);
	}

	private void TryDropOverhead(Flow flow)
	{
		lastDroppedItem = null;
		if (base.PlayerData.HasMultipleOverheadItems)
		{
			@out.Call(flow);
			return;
		}
		if (base.PlayerData.HasOverheadItem)
		{
			lastDroppedItem = base.PlayerData.overheadItem;
			base.PlayerData.DropOverheadItem();
		}
		@out.Call(flow);
	}
}
