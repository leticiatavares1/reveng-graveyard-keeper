using System.Collections.Generic;
using FlowCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

[Name("Finish Zombie Clay Craft", 0)]
[Category("Game/Zombie")]
public class Flow_FinishZombieClayCraft : GKCustomFlowNode
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
				MultiInventory multiInventory = new MultiInventory(wgoData.WorldZoneData, wgoData);
				List<Item> items = OutputItems.MakeOutput(wgoData.CraftComponent.CurrentCraftElement.Def.outputItems.MakePreOutput(wgoData));
				multiInventory.TryAddItemsPerOne(items);
				wgoData.CraftComponent.Clear();
				if (startAnother)
				{
					CraftParamsData craftParamsData = new CraftParamsData(wgoData.CraftComponent.AvailableCrafts[0].id, wgoData);
					craftParamsData.customRes.Set("wait_for_zombie_at_clay", 1f);
					craftParamsData.customRes.Set("ignore_handle_output", 1f);
					wgoData.CraftComponent.AddToQueue(new CraftElement(wgoData.CraftComponent.AvailableCrafts[0].id, 1, craftParamsData));
				}
			}
			else
			{
				Debug.LogError("Null CurrentCraftElement in Flow_FinishZombieClayCraft");
			}
		}
		else
		{
			Debug.LogError("No zombie linked to clay object");
		}
	}
}
