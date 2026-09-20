using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Price For Milestone Unlock", 0)]
[Category("Game/UI")]
public class Flow_PriceForMilestoneUnlock : GKCustomFlowNode
{
	private ValueOutput<SmartRes> output;

	protected override void RegisterPorts()
	{
		output = AddValueOutput("output".CapitalizeFirst(), delegate
		{
			SmartRes smartRes = new SmartRes();
			foreach (Item item in base.SelfWgoData.Inventory.Data.Inventory)
			{
				smartRes.items.Add(new ItemCount(item));
			}
			return smartRes;
		});
	}
}
