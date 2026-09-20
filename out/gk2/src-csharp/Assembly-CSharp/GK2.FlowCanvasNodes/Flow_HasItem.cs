using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace GK2.FlowCanvasNodes;

[Name("Has Item", 0)]
[Category("Game/Script")]
[Color("70f1ff")]
[Icon("FS", false, "")]
public class Flow_HasItem : GKCustomFlowNodeWithWgoData
{
	[GatherPortsCallback]
	public bool isForPlayer;

	[GatherPortsCallback]
	public bool isInventoryEmpty;

	private FlowInput @in;

	private FlowOutput yes;

	private FlowOutput no;

	private ValueInput<string> itemId;

	private ValueInput<int> itemCount;

	public override string name => (isInventoryEmpty ? "Is" : "Has") + " " + (isForPlayer ? "Player" : "WgoData") + " " + (isInventoryEmpty ? "Inventory Empty" : "Item In Inventory");

	protected override void RegisterPorts()
	{
		if (!isForPlayer)
		{
			base.RegisterPorts();
		}
		@in = AddFlowInput("in".CapitalizeFirst(), HasItem);
		yes = AddFlowOutput("yes".CapitalizeFirst());
		no = AddFlowOutput("no".CapitalizeFirst());
		if (!isInventoryEmpty)
		{
			itemId = AddValueInput<string>("itemId".CapitalizeFirst());
			itemCount = AddValueInput<int>("itemCount".CapitalizeFirst());
		}
	}

	private void HasItem(Flow flow)
	{
		if (isInventoryEmpty)
		{
			if (isForPlayer)
			{
				if (MainGame.PlayerData.Inventory.Data.IsInventoryEmpty() || MainGame.PlayerData.toolBeltInventory.Data.IsInventoryEmpty())
				{
					yes.Call(flow);
				}
				else
				{
					no.Call(flow);
				}
				return;
			}
			WgoData wgoData = GetWgoData();
			if (wgoData != null && wgoData.Inventory.Data.IsInventoryEmpty())
			{
				yes.Call(flow);
			}
			else
			{
				no.Call(flow);
			}
		}
		else if (isForPlayer)
		{
			if (MainGame.PlayerData.Inventory.Data.HasItemQuantityInInventory(itemId.value, itemCount.value) || MainGame.PlayerData.toolBeltInventory.Data.HasItemQuantityInInventory(itemId.value, itemCount.value))
			{
				yes.Call(flow);
			}
			else
			{
				no.Call(flow);
			}
		}
		else
		{
			WgoData wgoData2 = GetWgoData();
			if (wgoData2 != null && wgoData2.Inventory.Data.HasItemQuantityInInventory(itemId.value, itemCount.value))
			{
				yes.Call(flow);
			}
			else
			{
				no.Call(flow);
			}
		}
	}
}
