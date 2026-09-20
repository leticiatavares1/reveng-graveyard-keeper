using System.Collections.Generic;
using System.Linq;
using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Try To Add Corpse To Pallet", 0)]
[Category("Game/Script")]
[Color("70f1ff")]
[Icon("FS", false, "")]
public class Flow_TryToAddCorpseToPallet : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput yes;

	private FlowOutput no;

	private ValueInput<Item> item;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), TryToAddCorpseToPallet);
		item = AddValueInput<Item>("item");
		item.skipSelfInstanceAssignment = false;
		yes = AddFlowOutput("yes".CapitalizeFirst());
		no = AddFlowOutput("no".CapitalizeFirst());
	}

	private void TryToAddCorpseToPallet(Flow flow)
	{
		List<WgoData> wgoDataListByGroup = MainGame.Instance.GameSave.worldData.GetWgoDataListByGroup("morgue_pallets");
		if (wgoDataListByGroup != null && wgoDataListByGroup.Count > 0 && wgoDataListByGroup.Any((WgoData data) => data.Inventory.AddItemToInventory(item.value)))
		{
			yes.Call(flow);
		}
		else
		{
			no.Call(flow);
		}
	}
}
