using System.Collections.Generic;
using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Finish Zombie Sawmill Craft", 0)]
[Category("Game/Zombie")]
public class Flow_FinishZombieSawmillCraft : GKCustomFlowNode
{
	private FlowInput @in;

	private FlowOutput @out;

	protected override void RegisterPorts()
	{
		@in = AddFlowInput("in".CapitalizeFirst(), DoAction);
		@out = AddFlowOutput("out".CapitalizeFirst());
	}

	private void DoAction(Flow flow)
	{
		FinishCraft(base.SelfWgoData, startAnother: true);
		@out.Call(flow);
	}

	public static void FinishCraft(WgoData wgoData, bool startAnother)
	{
		if (wgoData.Worker is ZombieWgoData)
		{
			if (wgoData.CraftComponent.CurrentCraftElement != null)
			{
				Item item = new Item(wgoData.CraftComponent.CurrentCraftElement.Def.outputItems.chanceOutputItems[0].id);
				for (int i = 0; i < wgoData.WorldZoneData.MultiInventoryWgoDatas.Count; i++)
				{
					WgoData wgoData2 = wgoData.WorldZoneData.MultiInventoryWgoDatas[i];
					if (wgoData2.id == "sawmill_wood_container" && wgoData2.Inventory.Data.InventoryFillSize < wgoData2.Inventory.Data.InventorySize)
					{
						wgoData2.Inventory.AddItemToInventory(item);
						break;
					}
				}
				MultiInventory multiInventory = new MultiInventory(wgoData.WorldZoneData, wgoData);
				List<Item> list = OutputItems.MakeOutput(wgoData.CraftComponent.CurrentCraftElement.Def.outputItems.MakePreOutput(wgoData));
				for (int j = 1; j < list.Count; j++)
				{
					multiInventory.TryAddItem(list[j]);
				}
				wgoData.CraftComponent.Clear();
				if (startAnother)
				{
					CraftParamsData craftParamsData = new CraftParamsData(wgoData.CraftComponent.AvailableCrafts[0].id, wgoData);
					craftParamsData.customRes.Set("wait_for_zombie_at_sawmill", 1f);
					craftParamsData.customRes.Set("ignore_handle_output", 1f);
					wgoData.CraftComponent.AddToQueue(new CraftElement(wgoData.CraftComponent.AvailableCrafts[0].id, 1, craftParamsData));
				}
			}
			else
			{
				Debug.LogError("Null CurrentCraftElement in Flow_FinishZombieSawmillCraft");
			}
		}
		else
		{
			Debug.LogError("No zombie linked to sawmill object");
		}
	}
}
